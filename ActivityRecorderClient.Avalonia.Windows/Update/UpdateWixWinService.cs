using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;
using WindowsInstaller;

namespace Tct.ActivityRecorderClient.Update
{
	//todo make sure we won't restart indefinitely?
	//todo save updateinfo and update on intialize
	//todo remove all files from prg dir on uninstall
	//todo this is a mess tidy up code, don't use exceptions for controlling program flow
	public class UpdateWixWinService : IUpdateService
	{
		public const string RestartAsAdminArg = "RestartAsAdmin";

		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Assembly currentAssembly = Assembly.GetExecutingAssembly();

		private const string bootstrapFileName = "bootstrap.exe";
		private const string startTaskName = "JcStart";
		public static readonly string MsiExecFullPath = GetFullPath("msiexec.exe");
		private static readonly TimeSpan updateFreeInterval = TimeSpan.FromHours(3);
		private static readonly string installTempRootFolder = Path.Combine(ConfigManager.JcLocalPath, "InstallTemp");
		private static readonly string installTempFolder = Path.Combine(installTempRootFolder, Guid.NewGuid().ToString());
		private const string installTempPrefixName = "JobCTRL-";
		private const string MsiName = "jobctrl.msi";
		private string prevInstallPackageFolderPath;
		private string updatePackageFolderPath;

		
		private volatile UpdateInfo updateInfo;

		private UpdateWixData updateWixData;

		public static readonly string InitializeExMsgMissingMsiexec = "WiX build without msiexec";
		public static readonly string InitializeExMsgMissingBootstrap = "WiX build without bootstrap";
		public static readonly string InitializeExMsgRestartNeeded = "WiX build restart needed";
		public static readonly string InitializeExMsgUpdatetNeeded = "WiX build update needed";

		private const int msiLogsLimit = 5;

		private static string applicationName;
		private static string ApplicationName
		{
			get
			{
				if (!String.IsNullOrEmpty(applicationName)) return applicationName;

				applicationName = Environment.Version.Major == 4
					? "JobCTRL4"
					: ConfigManager.EnvironmentInfo.IsNet4Available
						? "JobCTRL4"
						: "JobCTRL";
				applicationName += $"#{ConfigManager.ApplicationName}";
				return applicationName;
			}
		}

		public Version CurrentVersion
		{
			get { return currentAssembly.GetName().Version; }
		}

		public UpdateWixWinService()
		{
			updateInfo = new UpdateInfo(CurrentVersion.ToString(), null);
		}

		public bool Initialize()
		{
			//ClickOnceMigrator.TryFinishMigration();

			// maybe cause problems if internal storing of workitems changed
			try
			{
				IsolatedStorageSerializationHelper.GetFileNames("WorkItems\\*");
			}
			catch (DirectoryNotFoundException)
			{
				IsFirstRun = true;
			}
			log.Verbose("FirstStart=" + IsFirstRun);
			updateWixData = UpdateWixData.Load();
			if (updateWixData != null)
			{
				if (updateWixData.VersionBeforeUpdate == CurrentVersion.ToString())
				{
					if (updateWixData.LastUnsuccessfulUpdate == null)
					{
						updateWixData.LastUnsuccessfulUpdate = DateTime.UtcNow;
						UpdateWixData.Save(updateWixData);
						log.Warn("Update was unsuccessful, version remains " + CurrentVersion);
						deleteUnsuccessfulUpdateFolder();
						LastUpdateFailureReason = UpdateFailureReason.InstallationNotCompleted;
					}
					else
						log.Debug("Still running on version " + CurrentVersion);
				}
				else
				{
					UpdateWixData.Delete();
					log.Debug("Update was successful to version " + CurrentVersion);
					deletePreviousMsiFiles();
					deleteUnnecessaryFoldersAndLogFiles();
				}
			}

			// clean up previous installation files except the last msi
		   var dir = Directory.CreateDirectory(installTempRootFolder);
			foreach (var file in dir.EnumerateFileSystemInfos("*.*"))
			{
				var type = "n/a";
				try
				{
					if (file is DirectoryInfo)
					{
						if (file.Name.StartsWith(installTempPrefixName)) continue;
						(file as DirectoryInfo).Delete(true);
						type = "directory";
					}
					else
					{
						if (file.Name == MsiName) continue;
						file.Delete();
						type = "file";
					}
					if (file.Exists)
						log.WarnFormat("Temporary {0}: {1} can't be deleted", type, file.FullName);
					else
						log.DebugFormat("Temporary {0}: {1} deleted", type, file.FullName);
				}
				catch (Exception ex)
				{
					log.Error(string.Format("Temporary {0}: {1} can't be deleted", type, file.FullName), ex);
				}
			}

			if (!File.Exists(MsiExecFullPath))
			{
				log.ErrorAndFail("Future application updates can't be performed because 'msiexec' not available");
				throw new Exception(InitializeExMsgMissingMsiexec);
			}
			if (!File.Exists(Path.Combine(Path.GetDirectoryName(currentAssembly.Location), bootstrapFileName)))
			{
				log.ErrorAndFail("Cannot find bootstrap file");
				throw new Exception(InitializeExMsgMissingBootstrap);
			}
			//todo update (one time) if possible InitializeExMsgUpdatetNeeded
			if (ElevatedPrivilegesHelper.IsLocalAdmin
				&& ElevatedPrivilegesHelper.RunAsAdmin)
			{
				try
				{
#if !DEBUG
					using (var ts = TaskSchedulerHelper.CreateServiceClassInstance())
					{
						bool isNeedRecreateJob = false;
						var rootFolderTasks = TaskSchedulerHelper.GetRootFolderTasks(ts);
						var jcTask = rootFolderTasks.SingleOrDefault(t => TaskSchedulerHelper.GetTaskName(t) == startTaskName);
						if (jcTask != null)
						{
							var taskLocation = TaskSchedulerHelper.GetExecActionPath(jcTask);
							var taskArgs = TaskSchedulerHelper.GetExecActionArgs(jcTask);
							var taskRunLevel = TaskSchedulerHelper.GetPrincipalRunLevel(jcTask);
							var currentSettings = TaskSchedulerHelper.GetTaskSettings(jcTask);
							var isDesiredSettings = TaskSchedulerHelper.CheckDesiredTaskSettings(currentSettings);
							isNeedRecreateJob = taskLocation != currentAssembly.Location
												|| taskArgs != RestartAsAdminArg
												|| !isDesiredSettings
												|| taskRunLevel != TaskSchedulerHelper.TaskRunLevel_Highest;
						}
						if (ElevatedPrivilegesHelper.IsElevated)
						{
							// create task if doesn't exist
							if (jcTask != null && isNeedRecreateJob)
							{
								log.Info("removing task " + startTaskName);
								TaskSchedulerHelper.DeleteTask(ts, startTaskName);
								jcTask = null;
							}
							if (jcTask == null)
							{
								log.Info("creating new task " + startTaskName);
								var td = TaskSchedulerHelper.CreateNewTask(ts, currentAssembly.Location,
									TaskSchedulerHelper.TaskRunLevel_Highest, RestartAsAdminArg);
								TaskSchedulerHelper.RegisterTaskDefinition(ts, startTaskName, td);
							}
						}
						else
						{
							try
							{
								if (jcTask == null || isNeedRecreateJob)
								{
									RestartAsAdmin();
								}
								else
								{
									try
									{
										log.Info("Restarting to switch elevated mode (start task)");
										TaskSchedulerHelper.RunTask(jcTask);
									}
									catch (Exception ex)
									{
										log.Error("Start task failed", ex);
										RestartAsAdmin();
									}
								}
								throw new Exception(InitializeExMsgRestartNeeded);
							}
							catch (Win32Exception ex)
							{
								log.Error("Restarting failed", ex); //i.e. UAC prompt declined on RestartAsAdmin
							}
						}
					}
#endif
					int dummy = 0;
					MsiInterop.MsiSetInternalUI(MsiInstallUILevel.None, ref dummy);
					StringBuilder prod = new StringBuilder(100);
					log.Debug("Checking unnecessary installed JCUpdater products");
					uint index = 0;
					while (MsiInterop.MsiEnumRelatedProducts("{d657476f-cebc-4d8e-b408-72742cf6fa88}", 0, index++, prod) == MsiError.NoError)
					{
						var res = MsiInterop.MsiConfigureProduct(prod.ToString(), MsiInstallLevel.Default, MsiInstallState.Absent);
						if (res == MsiError.NoError)
							log.DebugFormat("JCUpdater product {0} successfuly removed", prod);
						else
						{
							log.DebugFormat("Removing of JCUpdater product {0} failed with result {1}", prod, res);
						}
					}
					if (index == 1)
						log.Debug("No unnecessary JCUpdater product");
				}
				catch (Exception ex)
				{
					if (ex.Message == InitializeExMsgRestartNeeded) throw; //that is OK
					log.FatalAndFail("Unexpected error in Initialize", ex); //it is better to start without elevation than not to start at all
					return true; //we don't care to run with 'runas' if not elevated
				}
			}
			return true;
		}

		private static string[] deletableEmptyFoldersRegexes =
		{
			@"LotusNotesSync",
			@"OutlookSync",
			@"ChromeInterop",
			@"v2\.[2,3]\.[0-9]+\.[0-9]+\\FirefoxInterop",
			@"v2\.[2,3]\.[0-9]+\.[0-9]+\\OutlookAddin",
			@"v2\.[2,3]\.[0-9]+\.[0-9]+"
		};

		private void deleteUnnecessaryFoldersAndLogFiles()
		{
			try
			{
				var deletableLogFoldersRegexes = new[]
				{
				@"LotusNotesSync\\Logs",
				@"OutlookSync\\Logs",
				@"ChromeInterop\\Logs",
				@"v2\.[2,3]\.[0-9]+\.[0-9]+\\FirefoxInterop\\Logs",
				@"v2\.[2,3]\.[0-9]+\.[0-9]+\\OutlookAddin\\Logs"
			};
				var logFileExtension = @"\.log";

				var rootAppender = ((Hierarchy)LogManager.GetRepository())
					.Root.Appenders.OfType<FileAppender>()
					.FirstOrDefault();
				string clientLogFileName = rootAppender != null ? rootAppender.File : string.Empty;
				string clientLogDirectoryPath = Path.GetDirectoryName(clientLogFileName);
				string clientLogParentDirectoryPath = Path.GetDirectoryName(clientLogDirectoryPath);
				foreach (var dir in SafeEnumerateDirectories(clientLogParentDirectoryPath, "*.*", SearchOption.AllDirectories))
				{
					string relDir = dir.Substring(clientLogParentDirectoryPath.Length);
					foreach (var folderRegex in deletableLogFoldersRegexes)
					{
						if (Regex.IsMatch(relDir, folderRegex))
						{
							try
							{
								bool deletable = true;
								foreach (var file in Directory.EnumerateFiles(dir))
								{
									if (!Regex.IsMatch(file, logFileExtension)) deletable = false;
								}
								if (!deletable) continue;
								log.Info($"Deleting old log directory on path: {dir}");
								Directory.Delete(dir, true);
							}
							catch (Exception ex)
							{
								log.Warn($"Couldn't delete log folder. path: {dir}", ex);
							}
						}
					}
				}
				deleteEmptyFolders(clientLogParentDirectoryPath);
			}
			catch (Exception ex)
			{
				log.Warn($"Unexpected exception in deleting log folders.", ex);
			}
		}


		//https://stackoverflow.com/a/5957525
		private static IEnumerable<string> SafeEnumerateDirectories(string path, string searchPattern, SearchOption searchOpt)
		{
			try
			{
				var dirFiles = Enumerable.Empty<string>();
				if (searchOpt == SearchOption.AllDirectories)
				{
					dirFiles = Directory.EnumerateDirectories(path)
						.SelectMany(x => SafeEnumerateDirectories(x, searchPattern, searchOpt));
				}
				return dirFiles.Concat(Directory.EnumerateDirectories(path, searchPattern));
			}
			catch (UnauthorizedAccessException ex)
			{
				log.Warn($"Couldn't access folder {path}", ex);
				return Enumerable.Empty<string>();
			}
		}

		private static void deleteEmptyFolders(string path, string origPath = null)
		{
			foreach (var dir in Directory.EnumerateDirectories(path))
			{
				deleteEmptyFolders(dir, origPath ?? path);
			}
			if (origPath != null && !Directory.EnumerateFileSystemEntries(path).Any())
			{
				try
				{
					if (deletableEmptyFoldersRegexes.Any(x => Regex.IsMatch(path.Substring(origPath.Length), x)))
						log.Info($"Deleting old log directory: {path}");
					Directory.Delete(path);
				}
				catch (Exception ex)
				{
					log.Warn($"Couldn't delete old log directory at path: {path}", ex);
				}
			}
		}

		private void RestartAsAdmin()
		{
			log.Info("Restarting to switch elevated mode (runas)");
			var startInfo = new ProcessStartInfo(currentAssembly.Location) { Verb = "runas", Arguments = RestartAsAdminArg};
			Process.Start(startInfo); // restart as admin
		}

		private DateTime lastSuccessfulCheck = DateTime.UtcNow;
		/// <summary>
		/// Check for update and perform if applicable
		/// </summary>
		/// <param name="force">true: perform if update check is skipped</param>
		/// <returns>false: no new version found, true: new version found, null: any error occurred</returns>
		public bool? UpdateIfApplicable(bool force = false)
		{
			if (updateWixData != null && updateWixData.LastUnsuccessfulUpdate != null && !force)
			{
				if (DateTime.UtcNow < updateWixData.LastUnsuccessfulUpdate + updateFreeInterval)
				{
					log.Debug("Update check is skipped");
					return false;
				}
				UpdateWixData.Delete();
			}
			log.Debug("Check if update is available (" + ApplicationName + ")");
			try
			{
//				return GetUpdateTest(updateInfo);
				var result = GetUpdateWcf(updateInfo);
				lastSuccessfulCheck = DateTime.UtcNow;
				return result;
			}
			catch
			{
				try
				{
					if (DateTime.UtcNow - lastSuccessfulCheck < TimeSpan.FromHours(1)) return null;
					lastSuccessfulCheck = DateTime.UtcNow; // this method repeated once per hour
					return GetUpdateHttp(updateInfo);
				}
				catch
				{
					return null;
				}
			}
		}

		private bool GetUpdateTest(UpdateInfo currInfo)
		{
			var msiFile = Directory.GetFiles(@"c:\temp", "jobctrl_*.msi", SearchOption.TopDirectoryOnly).OrderByDescending(f => f).FirstOrDefault();
			if (msiFile == null) return false;
			var version = msiFile.Substring(msiFile.IndexOf("jobctrl_", StringComparison.InvariantCultureIgnoreCase) + 8);
			version = version.Substring(0, version.Length - 4);
			if (version == currInfo.UpdatedVersion) return false;
			log.Info("Found new version " + version);
			Directory.CreateDirectory(installTempFolder);
			var msiTempFilePath = Path.Combine(installTempFolder, MsiName);
			log.Info(string.Format("Copy [{0}]->[{1}]", msiFile, msiTempFilePath));
			File.Copy(msiFile, msiTempFilePath, true);
			updateInfo = new UpdateInfo(version, msiTempFilePath);
			return true;
		}

		private bool GetUpdateWcf(UpdateInfo currInfo)
		{
			if (currInfo.MsiPath != null && currInfo.UpdatedVersion == null)
			{
				log.Debug("Skipping update check since we have a newer version but don't know its version number");
				return false;
			}
			var msiTempFilePath = Path.Combine(installTempFolder, MsiName);
			try
			{
				var appUpdateInfo = ActivityRecorderClientWrapper.Execute(n => n.GetApplicationUpdate(ConfigManager.UserId, ApplicationName, currInfo.UpdatedVersion));
				if (appUpdateInfo == null) return false;
				Directory.CreateDirectory(installTempFolder);
				log.Info("Found new version " + appUpdateInfo.Version);
				if (!CheckRequirements()) return false;
				log.Debug("Start loading, no of chunks: " + appUpdateInfo.ChunkCount);
				using (FileStream stream = File.Create(msiTempFilePath))
				{
					for (int i = 0; i < appUpdateInfo.ChunkCount; i++)
					{
						byte[] chunk = ActivityRecorderClientWrapper.Execute(n => n.GetUpdateChunk(appUpdateInfo.FileId, i));
						stream.Write(chunk, 0, chunk.Length);
						log.Debug("Chunk {" + i + "} loaded, size: " + chunk.Length);
					}
					log.Info("Update package downloaded, size: " + stream.Length + " ver: " + appUpdateInfo.Version);
					stream.Flush();
					stream.Close();
				}
				if (updateInfo.MsiPath != null) DeleteFileNoThrow(updateInfo.MsiPath); //delete previous version
				Directory.Move(installTempFolder, NewFolderPathForUpdatePackage);
				updateInfo = new UpdateInfo(appUpdateInfo.Version, Path.Combine(NewFolderPathForUpdatePackage, MsiName));
				return true;
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get application update", log, ex);
				DeleteFileNoThrow(msiTempFilePath);
				throw;
			}
		}

		private bool GetUpdateHttp(UpdateInfo currInfo)
		{
			if (currInfo.MsiPath != null)
			{
				log.Debug("Skipping http update check because we have a newer version (" + currInfo.UpdatedVersion + ")");
				return false; //newer version already downloaded (although we don't know if it is the latest or not)
			}
			var msiTempFilePath = Path.Combine(installTempFolder, MsiName);
			try
			{
				Directory.CreateDirectory(installTempFolder);
				using (var stream = File.Create(msiTempFilePath))
				{
					GetHttpApplicationUpdate(ConfigManager.UserId, currInfo.UpdatedVersion, stream);
					var fileLength = stream.Length;
					stream.Close();
					if (fileLength == 0) throw new Exception("Downloaded file is empty");
					log.Info("Update package downloaded (http), size: " + fileLength);
				}
				if (!CheckRequirements())
				{
					DeleteFileNoThrow(msiTempFilePath);
					return false;
				}
				Directory.Move(installTempFolder, NewFolderPathForUpdatePackage);
				updateInfo = new UpdateInfo(null, Path.Combine(NewFolderPathForUpdatePackage, MsiName)); //we don't have version info here...
				return true;
			}
			catch (Exception ex)
			{
				log.Debug("Unable to get application update via http", ex); //only debug because this would always fail atm.
				DeleteFileNoThrow(msiTempFilePath);
				throw;
			}
		}

		private bool CheckRequirements()
		{
			var subkey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.QueryValues);
			var value = subkey?.GetValue("PendingFileRenameOperations");
			if (value != null)
			{
				var str = value as string;
				var strarr = value as string[];
				if ((str != null || strarr != null) && (str??string.Join(",", strarr)).ToLower().Contains(Path.GetDirectoryName(currentAssembly.Location).ToLower()))
				{
					log.Debug("There is a restart pending. Restart computer then the update process continues.");
					LastUpdateFailureReason = UpdateFailureReason.RestartRequired;
					updateWixData = new UpdateWixData() { LastUnsuccessfulUpdate = DateTime.UtcNow };
					return false;
				}
			}
			var checkRequirements = ConfigManager.EnvironmentInfo.IsNet45Available;
			if (checkRequirements) return true;
			log.Debug(".Net 4.5 not available, update cannot performed");
			updateWixData = new UpdateWixData() { LastUnsuccessfulUpdate = DateTime.UtcNow };
			return false;
		}

		public bool RestartWithNewVersion()
		{
			var currInfo = updateInfo;
			Debug.Assert(currInfo != null);
			Debug.Assert(currInfo.MsiPath != null);
			string bootstrapTmp = null;
			try
			{
				log.Info("Invoking bootsrapper to perform msi update for " + (currInfo == null ? null : currInfo.MsiPath));
				var bootstrapSrc = Path.Combine(Path.GetDirectoryName(currentAssembly.Location), bootstrapFileName);
				Directory.CreateDirectory(installTempFolder);
				bootstrapTmp = Path.Combine(installTempFolder, "bootstrap.exe"); 
				log.Info(string.Format("Copy [{0}]->[{1}]", bootstrapSrc, bootstrapTmp));
				File.Copy(bootstrapSrc, bootstrapTmp, true);
				var oldFileInfo = new FileInfo(bootstrapSrc);
				var newFileInfo = new FileInfo(bootstrapTmp);
				var msiFileInfo = new FileInfo(currInfo.MsiPath);
				if (!newFileInfo.Exists || newFileInfo.Length != oldFileInfo.Length || msiFileInfo.Length < 10000)
				{
					log.WarnFormat("Copied bootstrap or msi file corrupted, update aborted (bootstrap: {0}, msi: {1})", bootstrapTmp, currInfo.MsiPath);
					return false;
				}
				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.CreateNoWindow = false;
				startInfo.UseShellExecute = false;
				startInfo.FileName = bootstrapTmp;
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
				startInfo.Arguments = "UpdateAndRestart \"" + currInfo.MsiPath + "\" \"" + GetNewMsiLogPathAndDeleteOldMsiLogs(currInfo) + "\" \"" + Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "\"";
				//startInfo.EnvironmentVariables["UpdateProgress"] = Application.ProductVersion; //todo ez wtf ?
				startInfo.WorkingDirectory = installTempFolder;
				startInfo.ErrorDialog = true;
				updateWixData = new UpdateWixData();
				updateWixData.VersionBeforeUpdate = CurrentVersion.ToString();
				UpdateWixData.Save(updateWixData);
				var proc = Process.Start(startInfo);
				log.Info("Stopping application to avoid file locks");
			}
			catch (Exception ex) //it is quite important not to hit this!
			{
				log.Error("Unable start update process", ex);
				updateInfo = new UpdateInfo(CurrentVersion.ToString(), null); //reset update info
				DeleteFileNoThrow(currInfo == null ? null : currInfo.MsiPath); //todo don't delete here try to install at startup?
				DeleteFileNoThrow(bootstrapTmp);
				return false;
			}
			return true;
		}

		private static void DeleteFileNoThrow(string path)
		{
			try
			{
				if (path == null) throw new ArgumentNullException("path");
				if (File.Exists(path))
				{
					File.Delete(path);
					log.Debug("Deleted file " + path);
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to delete file " + path, ex);
			}
		}

		public string GetAppPath()
		{
			return currentAssembly.Location;
		}

		public DateTime? LastUpdateFailed => updateWixData?.LastUnsuccessfulUpdate;
		public UpdateFailureReason LastUpdateFailureReason { get; private set; }

		public bool IsFirstRun { get; private set; }
		public bool IsAppLevelStorageNeeded { get { return false; } }
		public string UpdateMethodName { get { return "WiX"; } }

		private static void GetHttpApplicationUpdate(int userId, string currentVersion, Stream updatePackageStream)
		{
			var req = (HttpWebRequest)WebRequest.Create(ConfigManager.HttpApplicationUpdateUrl);

			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			var postParams = string.Format("UserId={0}&Application={1}&CurrentVersion={2}", userId, ApplicationName, currentVersion);
			var postBuf = Encoding.ASCII.GetBytes(postParams);
			req.ContentLength = postBuf.Length;
			var requestStream = req.GetRequestStream();
			requestStream.Write(postBuf, 0, postBuf.Length);
			requestStream.Close();

			var response = (HttpWebResponse)req.GetResponse();
			var stream = response.GetResponseStream();
			var status = response.StatusCode;
			if (status != HttpStatusCode.OK)
			{
				response.Close();
				throw new Exception("Download failed with status: " + status.ToString());
			}
			var contentType = response.ContentType;
			if (contentType.ToLower().Contains("html"))
			{
				response.Close();
				throw new Exception("Not an object content type: " + contentType);
			}

			if (stream == null) return;
			var buffer = new byte[32768];
			int read;
			while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
			{
				updatePackageStream.Write(buffer, 0, read);
			}
			response.Close();
		}

		private static string GetFullPath(string fileName)
		{
			if (File.Exists(fileName)) return Path.GetFullPath(fileName);

			var fullPath = Path.Combine(Environment.SystemDirectory, fileName);
			if (File.Exists(fullPath)) return fullPath;

			var values = Environment.GetEnvironmentVariable("PATH");
			return values == null ? null : values.Split(';').Select(path => Path.Combine(path, fileName)).FirstOrDefault(File.Exists);
		}

		private string LatestInstallPackageFolderPath
		{
			get
			{
				if (prevInstallPackageFolderPath != null) return prevInstallPackageFolderPath;
				int max = 0;
				foreach (var directory in Directory.GetDirectories(installTempRootFolder))
				{
					string dirname = Path.GetFileName(directory);
					if (dirname.StartsWith(installTempPrefixName))
					{
						if (int.TryParse(dirname.Substring(installTempPrefixName.Length), out int r) && r > max) max = r;
					}
				}

				prevInstallPackageFolderPath = Path.Combine(installTempRootFolder, installTempPrefixName + max);
				return prevInstallPackageFolderPath;
			}
		}

		private void deleteUnsuccessfulUpdateFolder()
		{
			try
			{
				Directory.Delete(LatestInstallPackageFolderPath, true);
				prevInstallPackageFolderPath = null;
			}
			catch (Exception ex)
			{
				log.Warn("Couldn't delete previous update package folder.", ex);
			}
		}

		private void deletePreviousMsiFiles()
		{
			foreach (var directory in Directory.GetDirectories(installTempRootFolder))
			{
				string dirname = Path.GetFileName(directory);
				if (dirname.StartsWith(installTempPrefixName))
				{
					if (LatestInstallPackageFolderPath != directory)
					{
						try
						{
							Directory.Delete(directory, true);
						}
						catch (Exception ex)
						{
							log.Warn($"Couldn't delete install package folder {directory}.", ex);
						}
					}
				}
			}
		}

		private string NewFolderPathForUpdatePackage
		{
			get
			{
				if (updatePackageFolderPath != null) return updatePackageFolderPath;
				int max = 0;
				foreach (var directory in Directory.GetDirectories(installTempRootFolder))
				{
					string dirname = Path.GetFileName(directory);
					if (dirname.StartsWith(installTempPrefixName))
					{
						if (int.TryParse(dirname.Substring(installTempPrefixName.Length), out int r) && r > max) max = r;
					}
				}

				max++;
				updatePackageFolderPath = Path.Combine(installTempRootFolder, installTempPrefixName + max);
				while (Directory.Exists(updatePackageFolderPath))
				{
					max++;
					updatePackageFolderPath = Path.Combine(installTempRootFolder, installTempPrefixName + max);
				}

				log.Info($"NewFolderPath : {updatePackageFolderPath}");
				return updatePackageFolderPath;
			}
		}

		[DataContract]
		internal partial class UpdateInfo
		{
			[DataMember]
			public string UpdatedVersion { get; private set; }
			[DataMember]
			public string MsiPath { get; private set; }

			public UpdateInfo(string updatedVersion, string msiPath)
			{
				UpdatedVersion = updatedVersion;
				MsiPath = msiPath;
			}
		}

		public bool InstallAndStartNewVersion(bool waitForInstall)
		{
			var currInfo = updateInfo;
			Debug.Assert(currInfo != null);
			Debug.Assert(currInfo.MsiPath != null);

			try
			{
				var startInfo = new ProcessStartInfo
				{
					CreateNoWindow = false,
					UseShellExecute = false,
					FileName = MsiExecFullPath,
					WindowStyle = ProcessWindowStyle.Hidden,
					Arguments = "/qn /i \"" + currInfo.MsiPath + "\" /L*v \"" + GetNewMsiLogPathAndDeleteOldMsiLogs(currInfo) + "\" RUNAFTERINSTALL=1"
				};
				log.Info("Starting install process. (" + startInfo.FileName + " " + startInfo.Arguments + ")");
				using (var msiProcess = Process.Start(startInfo))
				{
					if (waitForInstall)
					{
						msiProcess.WaitForExit();
						log.Info("Install process finished.");
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to start install process", ex);
				return false;
			}
			return true;
		}

		private string GetNewMsiLogPathAndDeleteOldMsiLogs(UpdateInfo currUpdateInfo)
		{
			var logPath = Path.Combine(ConfigManager.LogPath, "Logs");
			try
			{
				var dir = new DirectoryInfo(logPath);
				foreach (var file in dir.GetFiles("Update_v*.log").OrderByDescending(x => x.CreationTime).Skip(msiLogsLimit - 1))
				{
					file.Delete();
				}

			}
			catch (Exception e)
			{
				log.Error("Delete old msi logs failed", e);
			}

			return Path.Combine(logPath, "Update_v" + CurrentVersion + "-v" + currUpdateInfo.UpdatedVersion + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log");
		}
	}

	[DataContract]
	public class UpdateWixData
	{
		private static readonly string FilePath = "UpdateWixData-" + ConfigManager.UserId;

		public static UpdateWixData Load()
		{
			if (!IsolatedStorageSerializationHelper.Exists(FilePath)) return null;
			UpdateWixData data;
			IsolatedStorageSerializationHelper.Load(FilePath, out data);
			return data;
		}

		public static void Save(UpdateWixData data)
		{
			IsolatedStorageSerializationHelper.Save(FilePath, data);
		}

		public static void Delete()
		{
			IsolatedStorageSerializationHelper.Delete(FilePath);
		}

		[DataMember]
		public string VersionBeforeUpdate { get; set; }
		[DataMember]
		public DateTime? LastUnsuccessfulUpdate { get; set; }
	}
}
