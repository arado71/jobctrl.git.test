using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Tct.Java.Accessibility;
using Tct.Java.Service;

namespace Tct.Java.Plugin
{
	public class JavaAccessibilityPlugin
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private bool is64Bit => Environment.Is64BitProcess;

		private HashSet<string> initialized = new HashSet<string>();
		private HashSet<string> isCapturingPossible = new HashSet<string>();
		private readonly object initLock = new object();
		private readonly SynchronizationContext synchronizationContext;

		public JavaAccessibilityPlugin(SynchronizationContext syncContext)
		{
			synchronizationContext = syncContext;
		}

		private Func<IntPtr, KeyValuePair<string, string>> getCaptureFuncFromIndexList(string captureName, IEnumerable<int> indexes, JavaPluginValueType pvt, string parameters)
		{
			return x =>
			{
				KeyValuePair<string, string> result = new KeyValuePair<string, string>(null, null);
				if (!JabApiController.Instance.IsJavaWindow(x))
					return result;
				AccessibleWrapper aw = JabApiController.Instance.GetContextFromHwnd(x);
				foreach (var index in indexes)
				{
					aw = JabApiController.Instance.GetChildElementAt(aw, index);
				}
				switch (pvt)
				{
					case JavaPluginValueType.Description:
						result = new KeyValuePair<string, string>(captureName, aw.Description);
						break;
					case JavaPluginValueType.Name:
						result = new KeyValuePair<string, string>(captureName, aw.Name);
						break;
					case JavaPluginValueType.Role:
						result = new KeyValuePair<string, string>(captureName, aw.Role);
						break;
					case JavaPluginValueType.Text:
						result = new KeyValuePair<string, string>(captureName, JabApiController.Instance.GetTextElementFromAccessibleWrapper(aw));
						break;
					case JavaPluginValueType.ComboValue:
						result = new KeyValuePair<string, string>(captureName, JabApiController.Instance.GetComboValueFromAccessibleWrapper(aw));
						break;
					case JavaPluginValueType.Table:
						int colNumber = int.Parse(parameters);
						result = new KeyValuePair<string, string>(captureName, JabApiController.Instance.GetTableValueFromAccessibleWrapper(aw, colNumber));
						break;
				}
				return result;
			};
		}

		public KeyValuePair<string, string>? Capture(JavaCaptureSettings captureSettings)
		{
			if (!initialized.Contains(captureSettings.ProcessName)) initialize(captureSettings.ProcessName);
			if (!isCapturingPossible.Contains(captureSettings.ProcessName)) return null;

			return getCaptureFuncFromIndexList(captureSettings.CaptureName,
				captureSettings.PathToElement,
				captureSettings.JavaPluginValueType,
				captureSettings.Parameters)
					.Invoke(captureSettings.Hwnd);
			//log.Verbose($"Java accessibility capture took {sw.Elapsed.TotalMilliseconds:0.000} ms. Result: {{{string.Join(Environment.NewLine, result.Select(x => x.Key + " - " + x.Value))}}}");

		}

		private void initialize(string processName)
		{
			lock (initLock)
			{
				try
				{
					if (initialized.Contains(processName)) return;

					try
					{
						using (var regKey = Registry.CurrentUser.OpenSubKey(
							@"Software\Microsoft\Windows NT\CurrentVersion\Accessibility", RegistryKeyPermissionCheck.ReadSubTree))
						{
							if (regKey != null)
							{
								var val = regKey.GetValue("Configuration");

								if (val != null && !((string)val).Contains("oracle_javaaccessbridge"))
									throw new AccessibilityNotEnabledException();
							}
						}

						synchronizationContext.Send(_ =>
						{
							try
							{
								var test = JabApiController.Instance;
								isCapturingPossible.Add(processName);
								log.Debug("Initialization success!");
							}
							catch (Exception ex)
							{
								log.Info("Couldn't initialize java access bridge, maybe because JAB is not enabled. Trying to enable...", ex);
							}
						}, null);
					}
					catch (Exception exc)
					{
						log.Info("Couldn't check java access bridge state, maybe because JAB is not enabled. Trying to enable...", exc);
					}

					if (isCapturingPossible.Contains(processName)) return;
					string javaBinPath = null;
					try
					{
						Process javaProcess = Process.GetProcessesByName(processName.Replace(".exe", "")).FirstOrDefault();
						var javaProcessPath = javaProcess?.MainModule?.FileName;
						var javaDirPath = string.IsNullOrEmpty(javaProcessPath) ? null : Path.GetDirectoryName(javaProcessPath);
						if (javaDirPath != null && File.Exists(Path.Combine(javaDirPath, "jabswitch.exe")))
						{
							javaBinPath = javaDirPath;
						}
					}
					catch (Win32Exception ex) { log.Debug($"Couldn't access process path: {processName}", ex); }

					try
					{
						if (javaBinPath == null)
						{
							string programFilesFolderPath = is64Bit
								? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
								: Environment.GetFolderPath(Environment.Is64BitOperatingSystem
									? Environment.SpecialFolder.ProgramFilesX86
									: Environment.SpecialFolder.ProgramFiles);

							string javaFolderPath = Path.Combine(programFilesFolderPath, "Java");
							javaFolderPath = javaFolderPath.TrimEnd('\\');
							javaBinPath = Directory.EnumerateDirectories(javaFolderPath)
								.FirstOrDefault(s => Regex.IsMatch(s, "^" + javaFolderPath.Replace(@"\", @"\\").Replace("(", "\\(").Replace(")", "\\)") + @"\\jre1.8.0_\d\d\d?", RegexOptions.IgnoreCase));
							if (javaBinPath != null) javaBinPath = Path.Combine(javaBinPath, "bin");
						}

						if (javaBinPath == null && File.Exists(@"c:\Program Files (x86)\Java\JRE8\bin\jabswitch.exe"))
						{
							javaBinPath = @"c:\Program Files (x86)\Java\JRE8\bin";
						}
						if (javaBinPath == null && File.Exists(@"C:\JAVA\jre\bin\jabswitch.exe"))
						{
							javaBinPath = @"C:\JAVA\jre\bin";
						}
						if (javaBinPath == null)
						{
							log.Debug("jabswitch.exe is not found.");
							return;
						}
						string jabSwitchPath = Path.Combine(javaBinPath, "jabswitch.exe");
						ProcessStartInfo processStartInfo = new ProcessStartInfo()
						{
							Arguments = "-enable",
							CreateNoWindow = true,
							FileName = jabSwitchPath,
							UseShellExecute = false,
							RedirectStandardOutput = true
						};
						Process p = new Process
						{
							StartInfo = processStartInfo
						};
						log.Info("Trying to start jabswitch...");
						p.Start();
						log.Debug(p.StandardOutput.ReadToEnd());
						synchronizationContext.Send(_ =>
						{
							try
							{
								var test = JabApiController.Instance;
								isCapturingPossible.Add(processName);
								log.Debug("Initialization success!");
							}
							catch (Exception ex)
							{
								log.Info("Java Access Bridge can't run.", ex);
							}
						}, null);
					}
					catch (Exception ex)
					{
						log.Warn("Couldn't initialize Java Access Bridge", ex);
					}
				}
				finally
				{
					initialized.Add(processName);
				}
			}
		}

		class AccessibilityNotEnabledException : Exception
		{

		}
	}
}
