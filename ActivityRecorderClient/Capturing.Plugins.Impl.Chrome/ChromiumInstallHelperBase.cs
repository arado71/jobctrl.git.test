#if !DEBUG
#define NO_DEBUG
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using Microsoft.Win32;
using NativeMessagingHost;
using Newtonsoft.Json.Linq;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Update;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome
{
	public abstract class ChromiumInstallHelperBase
	{
		private readonly ILog log;

#if DEBUG
		internal static readonly string subDirName = "ChromeInterop";
#else
		internal static readonly string subDirName = Path.Combine("v" + ConfigManager.Version, "ChromeInterop");
#endif
		private const string extensionFileName = "chromeinterop@jobctrl.com.crx";
		private const string extensionResourceName = "Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome." + extensionFileName;
		private const string nativeHostManifestFileName = "com.tct.jobctrl.json";
		private const string nativeHostManifestResourceName = "Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome." + nativeHostManifestFileName;
		private const string nativeHostFileNameWoExt = "JC.Chrome";
		private const string nativeHostFileName = nativeHostFileNameWoExt + ".exe";
		private const string nativeHostResourceName = "Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome." + nativeHostFileName;
		private const string nativeHostConfigFileName = nativeHostFileName + ".config";
		private const string nativeHostConfigResourceName =
			"Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome." + nativeHostConfigFileName;
		private const string nativeHostLogFileExtension = ".log";
		private const string pathToFileNode = @"//*[local-name()='configuration']/*[local-name()='log4net']/*[local-name()='appender']/*[local-name()='file']";
		private const string currentExtensionId = "obmlbfkihnobgbokahopnbaehffncfoe";
		private const string currentExtensionIdNum = "7500";
		private static readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		internal static readonly string NativeHostDir =
			Path.Combine(appDir, subDirName);
		private static Timer cleanupTimer;
		private static int cleanupTimerStarted;
		private static readonly object cleanupLockObj = new object();
		private static readonly KeyValuePair<string, string>[] oldExtensionIds = { new KeyValuePair<string, string>("cdiboboglhphpeooagpmpkfbokmffeme", "7498"), new KeyValuePair<string, string>("mpjhadgoffhidbcmjjcofofkfgbpnfbd", "7499") };

		private int isInstalled;
		protected abstract string ExtensionRegKey { get; }
		protected abstract string ExtensionForceRegKey { get; }
		protected abstract string NativeHostRegKey { get; }
		protected abstract string PreferencesPath { get; }
		protected abstract string ImageName { get; }
		protected abstract Func<ChromiumCaptureClientWrapperBase> ClientWrapperFactory { get; }

		protected void InstallExtensionOneTimeIfApplicableInternal()
		{
			if (Interlocked.CompareExchange(ref isInstalled, 1, 0) != 0) return;
			ThreadPool.QueueUserWorkItem(_ => InstallExtensionNoThrow());
		}

		protected ChromiumInstallHelperBase(ILog log)
		{
			this.log = log;
		}

		[Conditional("NO_DEBUG")]
		private void InstallExtensionNoThrow()
		{
			try
			{
				var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var dir = Path.Combine(currDir, subDirName);

				var nativeHostPath = Path.Combine(dir, nativeHostFileName);
				UnregisterNativeHost(); //Unregister so chrome won't be able to restart native messaging host.
				StopNativeHost(nativeHostPath);
				var nativeHostConfigFilePath = Path.Combine(dir, nativeHostConfigFileName);

				try
				{
					var rootAppender = ((Hierarchy)LogManager.GetRepository())
						.Root.Appenders.OfType<FileAppender>()
						.FirstOrDefault();
					string clientLog4netFileName = rootAppender != null ? rootAppender.File : string.Empty;
					bool shouldNormalizeLogPath = !(clientLog4netFileName.Contains('$') | clientLog4netFileName.Contains(':'));
					log.Info("Setting log4net path to " + clientLog4netFileName);
					XmlDocument doc = new XmlDocument();
					doc.Load(nativeHostConfigFilePath);
					var el = doc.SelectSingleNode(pathToFileNode)?.Attributes?["value"];
					if (el == null)
					{
						log.Warn("Couldn't find the proper node in the config.");
						throw new Exception("Missing node");
					}
					else
					{
						string clientLog4netDirectoryPath = Path.GetDirectoryName(clientLog4netFileName);
						if (!shouldNormalizeLogPath)
						{
							el.Value = Path.Combine(clientLog4netDirectoryPath ?? "", nativeHostFileNameWoExt + nativeHostLogFileExtension).Replace("\\", "/");
							log.Debug("log4net path set.");
						}
						else
						{
							Uri fileUri = new Uri(clientLog4netFileName);
							Uri rootUri = new Uri(appDir);
							Uri relativeUri = fileUri.MakeRelativeUri(rootUri);
							string relativeLogPath = Uri.UnescapeDataString(relativeUri.ToString());
							string relativeLogPathDirectory = Path.GetDirectoryName(relativeLogPath);
							if(relativeLogPathDirectory == null) throw new NullReferenceException("Relative log path directory is null");
							relativeLogPath = Path.Combine(relativeLogPathDirectory, nativeHostFileNameWoExt + nativeHostLogFileExtension);
							relativeLogPath = relativeLogPath.Replace("\\", "/");
							log.Debug($"Setting JC {ImageName} config file's log4net path to [{relativeLogPath}]");
							el.Value = relativeLogPath;
						}
						doc.Save(nativeHostConfigFilePath);
					}
				}
				catch (Exception ex)
				{
					log.Warn("Couldn't set nativemessaging's log path.", ex);
				}

				RegisterNativeHost(Path.Combine(dir, nativeHostManifestFileName));


				//Copying log4net next to the native messaging host
				string logmanagerLocation = Assembly.GetAssembly(typeof(LogManager)).Location;
				if (!File.Exists(Path.Combine(dir, Path.GetFileName(logmanagerLocation))))
					File.Copy(logmanagerLocation, Path.Combine(dir, Path.GetFileName(logmanagerLocation)));

				//User can drag&drop the crx file to chrome://extensions page to use it for a single chrome session for testing purposes. Or it can be used as an off-store extension in enterprise context with Group Policy Objects. 
				RegisterExtension(currentExtensionId); //Registration to auto-install (or update) extension from Chrome Web Store
				foreach (var id in oldExtensionIds)
				{
					UnregisterExtension(id.Key, id.Value);
				}
				CleanupPreferences();
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error during extension install", ex);
			}
		}

		private void StopNativeHost(string path)
		{
			Process[] processes = null;
			try
			{
				processes = Process.GetProcessesByName(nativeHostFileNameWoExt);
				var nativeHostProcess = processes.FirstOrDefault(p =>
				{
					try
					{
						var parentProcess = ParentProcessUtilities.GetParentProcess(p);
						while (parentProcess?.ProcessName == "cmd")
							parentProcess = ParentProcessUtilities.GetParentProcess(parentProcess);
						return string.Equals(p.Modules[0].FileName, path, StringComparison.InvariantCultureIgnoreCase)
							&& parentProcess?.ProcessName == ImageName;
					}
					catch (Win32Exception)
					{
						return false; // process not accessible
					}
				});
				if (nativeHostProcess == null) return;

				using (var client = ClientWrapperFactory())
				{
					client.Client.StopService();
				}

				if (!nativeHostProcess.WaitForExit(5000))
				{
					log.Info("Chrome native messaging host faild to stop. Try to kill it.");
					nativeHostProcess.Kill();
				}
			}
			catch (Exception e)
			{
				log.Error("Error while stoping native messaging host.", e);
			}
			finally
			{
				if (processes != null) foreach (var process in processes) process.Dispose();
			}
		}

		//https://developer.chrome.com/extensions/external_extensions#registry
		private void RegisterExtension(string id)
		{
			if (ProcessElevationHelper.IsElevated())
			{
				SetRegistryValue(Registry.LocalMachine, ExtensionForceRegKey, currentExtensionIdNum, id + ";https://clients2.google.com/service/update2/crx");  //This key is redirected under HKEY_LOCAL_MACHINE but we don't care
			}
			SetRegistryValue(Registry.CurrentUser, ExtensionRegKey + id, "update_url", "https://clients2.google.com/service/update2/crx");
		}

		private void UnregisterExtension(string id, string id2)
		{
			if (ProcessElevationHelper.IsElevated())
			{
				DeleteRegistryValue(Registry.LocalMachine, ExtensionForceRegKey, id2);
			}
			DeleteRegistryKey(Registry.CurrentUser, ExtensionRegKey + id);
		}

		//https://developer.chrome.com/extensions/nativeMessaging
		private void RegisterNativeHost(string path)
		{
			SetRegistryValue(Registry.CurrentUser, NativeHostRegKey, "", path);
		}

		private void UnregisterNativeHost()
		{
			DeleteRegistryKey(Registry.CurrentUser, NativeHostRegKey);
		}

		private static void SetRegistryValue(RegistryKey rootKey, string key, string name, string value)
		{
			using (var regkey = rootKey.CreateSubKey(key))
			{
				var regVal = regkey.GetValue(name);
				if (regVal != null)
				{
					if (regVal.ToString() == value) return;
					regkey.DeleteValue(name, false); //delete old key if exists
				}
				regkey.SetValue(name, value);
			}
		}

		private static void DeleteRegistryValue(RegistryKey rootKey, string key, string name)
		{
			using (var regkey = rootKey.OpenSubKey(key, true))
			{
				if (regkey != null) regkey.DeleteValue(name, false);
			}
		}

		private static void DeleteRegistryKey(RegistryKey rootKey, string key)
		{
			using (var regkey = rootKey.OpenSubKey(key, false))
			{
				if (regkey == null) return;
			}
			rootKey.DeleteSubKeyTree(key);
		}

		private void ExtractResource(string resourceName, string path)
		{
			var exists = File.Exists(path);
			try
			{
				log.Info("Extracting resource to " + (exists ? "(exists) " : "") + path);
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
				using (var file = File.Create(path))
				{
					CopyStream(stream, file);
				}
			}
			catch (Exception ex)
			{
				if (exists) //probably it's the same file
				{
					log.Debug("Unable to extract resource", ex);
				}
				else
				{
					throw;
				}
			}
		}

		private void CleanupPreferences()
		{
			var currentSession = Process.GetCurrentProcess().SessionId;
			var chromes = Process.GetProcessesByName(ImageName).Where(p => p.SessionId == currentSession).ToArray();
			if (chromes.Length > 0)
				foreach (var proc in chromes)
				{
					proc.EnableRaisingEvents = true;
					proc.Exited += ChromeOnExited;
				}
			else DoCleanup(null);
		}

		private void ChromeOnExited(object sender, EventArgs eventArgs)
		{
			var proc = sender as Process;
			Debug.Assert(proc != null);
			proc.Exited -= ChromeOnExited;
			lock (cleanupLockObj)
			{
				if (cleanupTimer == null)
				{
					cleanupTimerStarted = Environment.TickCount;
					cleanupTimer = new Timer(DoCleanup, null, 1000, Timeout.Infinite);
				}
				else
				{
					var dueTime = Environment.TickCount - cleanupTimerStarted + 1000;
					cleanupTimer.Change(dueTime, Timeout.Infinite);
				}
			}
		}

		private void DoCleanup(object a)
		{
			try
			{
				// TODO: scan all profiles instead of default
				log.Debug("DoCleanup initiated");
				var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PreferencesPath);
				var prefs = File.ReadAllText(path);
				var prefsJson = JObject.Parse(prefs);
				// Remove extensionid from external_uninstalls and updateclientdata.apps as it prevents installing the extension
				var extUninst = prefsJson.SelectToken("extensions.external_uninstalls") as JArray;
				if (extUninst != null)
				{
					var element = extUninst.FirstOrDefault(x => x.Value<string>() == currentExtensionId);
					if (element != null) extUninst.Remove(element);
				}
				var updateClientData = prefsJson.SelectToken("updateclientdata.apps");
				if (updateClientData != null)
				{
					var element = updateClientData.Children<JProperty>().FirstOrDefault(x => x.Name == currentExtensionId);
					if (element != null) element.Remove();
				}
				var extJson = prefsJson.SelectToken("extensions.settings." + currentExtensionId);
				if (extJson != null)
				{
					var state = extJson.Value<string>("state");
					var disableReason = extJson.Value<string>("disable_reasons");
					if ((state != null && state != "1") || disableReason != null)
					{
						extJson.Parent.Remove();
					}
				}
				File.WriteAllText(path, prefsJson.ToString());
				log.Debug("Extension cleaned");
			}
			catch (DirectoryNotFoundException)
			{
				// do nothing, chrome may not be installed
			}
			catch (IOException ex)
			{
				log.Warn("chromes prefs update failed", ex);
			}
			catch (UnauthorizedAccessException ex)
			{
				log.Warn("chromes prefs update failed", ex);
			}
			catch (Exception ex)
			{
				log.Warn("Unexpected exception", ex);
			}
			finally
			{
				lock (cleanupLockObj)
				{
					if (cleanupTimer != null)
					{
						cleanupTimer.Dispose();
						cleanupTimer = null;
					}
				}
			}
		}

		private static void CopyStream(Stream input, Stream output)
		{
			if (input == null) throw new ArgumentNullException("input");
			if (output == null) throw new ArgumentNullException("output");

			var buffer = new byte[8192];
			int bytesRead;
			while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, bytesRead);
			}
		}
	}
}
