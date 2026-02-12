using JobCTRL.Plugins;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Tct.Java.Accessibility;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	class PluginBurOffice : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public string Id => "JobCTRL.BurOffice";
		private const string KeyHeader = "BurOffice.Header";
		private readonly int[] headerPath = { 0, 1, 0, 0, 0, 1, 0, 0, 0 };

		private readonly Dictionary<string, int[]> keyPathDictionary;
		private bool initialized = false;
		private bool isCapturingPossible = false;
		private readonly object initLock = new object();

		public PluginBurOffice()
		{
			keyPathDictionary = new Dictionary<string, int[]>()
			{
				{ KeyHeader, headerPath }
			};
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyHeader;
		}

		public IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public void SetParameter(string name, string value)
		{
		}


		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			var sw = Stopwatch.StartNew();
			if (!string.Equals(processName, "jp2launcher.exe", StringComparison.OrdinalIgnoreCase) && !string.Equals(processName, "java.exe", StringComparison.OrdinalIgnoreCase) && !string.Equals(processName, "javaw.exe", StringComparison.OrdinalIgnoreCase)) return null;
			if (!initialized) initialize();
			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId, "Java");
			if (!isCapturingPossible) return null;
			try
			{
				if (!JabApiController.Instance.IsJavaWindow(hWnd)) return null;
				List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
				int vmID = 0;
				foreach (var keyPath in keyPathDictionary)
				{
					var res = JabApiController.Instance.GetNameValueFromHWndIEnumerable(hWnd, keyPath.Value);
					//var res = JabHelpers.GetTextValueFromHWndIEnumerable(hWnd, keyPath.Value, out IntPtr pointerToElement);
					result.Add(new KeyValuePair<string, string>(keyPath.Key,
						res));
				}
				log.Verbose($"BurOffice capture took {sw.Elapsed.TotalMilliseconds:N3} ms.");
				return result;
			}
			catch (Exception e)
			{
				log.Debug("Error in BurOffice capturing.", e);
				return null;
			}
		}

		private void initialize()
		{
			lock (initLock)
			{
				if (initialized) return;
				try
				{
					log.Debug("Initializing Java Access Bridge...");
					initialized = true;
					try
					{
						string javaInstPath = null;
						if (File.Exists(@"c:\Program Files (x86)\Java\JRE8\bin\jabswitch.exe"))
						{
							javaInstPath = @"c:\Program Files (x86)\Java\JRE8";
						}

						if (javaInstPath == null)
						{
							try
							{
								javaInstPath = Directory.EnumerateDirectories(@"c:\Program Files\Java\")
									.FirstOrDefault(s => Regex.IsMatch(s, @"^c:\\Program Files\\Java\\jre1.8.0_\d\d\d?"));
							}
							catch (DirectoryNotFoundException)
							{
								try
								{
									log.Debug("Couldn't find java in Program Files. Trying to find in ProgramFiles.");
									javaInstPath = Directory.EnumerateDirectories(@"c:\ProgramFiles\Java\")
										.FirstOrDefault(s => Regex.IsMatch(s, @"^c:\\ProgramFiles\\Java\\jre1.8.0_\d\d\d?"));
								}
								catch (DirectoryNotFoundException)
								{
								}
							}
						}


						if (javaInstPath != null)
						{
							string jabSwitchPath = Path.Combine(Path.Combine(javaInstPath, "bin"), "jabswitch.exe");
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
						}
					}
					catch (Exception ex)
					{
						log.Debug("Error in running JABSwitch.", ex);
					}
					Platform.Factory.GetGuiSynchronizationContext()?.Post(_ =>
					{
						try
						{
							var test = JabApiController.Instance;
							isCapturingPossible = true;
							log.Debug("Initialization success!");
							return;
						}
						catch (Exception ex)
						{
							log.Info("Java Access Bridge can't run.", ex);
						}

						ThreadPool.QueueUserWorkItem(__ =>
						{
							ProcessStartInfo processStartInfo = new ProcessStartInfo()
							{
								Arguments = "-enable",
								CreateNoWindow = true,
								FileName = "jabswitch.exe",
								UseShellExecute = false,
								RedirectStandardOutput = true
							};
							Process p = new Process
							{
								StartInfo = processStartInfo
							};
							log.Info("Trying to start jabswitch...");
							try
							{
								p.Start();
								log.Debug(p.StandardOutput.ReadToEnd());
							}
							catch (Exception ex)
							{
								log.Warn("Couldn't start jabswitch.", ex);
								log.Warn("Can't find Java installation path in the registry. Capturing disabled.");
								isCapturingPossible = false;
								return;
							}

							Platform.Factory.GetGuiSynchronizationContext()?.Post(___ =>
							{
								try
								{
									var test = JabApiController.Instance;
									isCapturingPossible = true;
								}
								catch (Exception ex)
								{
									log.Warn("Error in initialization. BurOffice capturing disabled.", ex);
								}
							}, null);
						});
					}, null);
				}
				catch (Exception ex)
				{
					log.Warn("Error in initialization. Capturing disabled.", ex);
					isCapturingPossible = false;
				}
			}
		}
	}
}
