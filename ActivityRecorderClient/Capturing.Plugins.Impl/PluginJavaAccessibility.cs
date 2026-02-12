using JobCTRL.Plugins;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Rules;
using Tct.Java.Accessibility;
using Tct.Java.Plugin;
using Tct.Java.Service;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	class PluginJavaAccessibility : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "JobCTRL.JavaCapture";
		public string Id => PluginId;

		private HashSet<string> processNames;
		private CachedRegex windowTitle;
		private readonly List<string> capturableKeys = new List<string>();
		private readonly JavaAccessibilityPlugin pluginInstance;
		private List<JavaCaptureSettings> captureSettingsList = new List<JavaCaptureSettings>();
		private static readonly CachedDictionary<int, bool> is64BitProcessCachedDictionary = new CachedDictionary<int, bool>(TimeSpan.FromMinutes(5), true);
		private static readonly object cachedDictLock = new object();

		private const string ParamProcess = "ProcessName";
		private const string ParamCapture = "Capture";
		private const string ParamWindowTitle = "WindowTitle";

		private bool isCapturingPossible = true;
		private bool isJavaProcessRunning = false;
		private readonly object initLock = new object();

		public PluginJavaAccessibility()
		{
			var context = Platform.Factory.GetGuiSynchronizationContext();
			Debug.Assert(context != null, "Synccontext is null.");
			pluginInstance = new JavaAccessibilityPlugin(context);
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamProcess;
			yield return ParamCapture;
			yield return ParamWindowTitle;
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			return capturableKeys;
		}

		public void SetParameter(string name, string value)
		{
			if (string.Equals(name, ParamWindowTitle, StringComparison.OrdinalIgnoreCase))
			{
				windowTitle = new CachedRegex
				{
					Regex = new RegexOrContains(new Regex(value))
				};
			}
			if (string.Equals(name, ParamProcess, StringComparison.OrdinalIgnoreCase))
			{
				processNames = new HashSet<string>(value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
			}
			// TODO:
			if (string.Equals(name, ParamCapture, StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					compile(value);
				}
				catch (Exception ex)
				{
					log.Warn("Failed to compile script", ex);
					captureSettingsList.Clear();
				}
			}
		}

		private void compile(string param)
		{
			string[] splitted = param.Split(';');
			captureSettingsList.Clear();
			foreach (var query in splitted)
			{
				JavaCaptureSettings settings = new JavaCaptureSettings();
				string[] splittedQuery = query.Split(new[] { "//" }, StringSplitOptions.None);
				string name = splittedQuery[0].Substring(0, splittedQuery[0].IndexOf(':'));
				capturableKeys.Add(name);
				string[] elementQueries = splittedQuery[1].Split('/');
				List<int> indexes = new List<int>();
				foreach (var elementQuery in elementQueries)
				{
					var index = int.Parse(elementQuery.Substring(7, elementQuery.Length - 8));
					indexes.Add(index);
				}
				string capturableValueTypeString = splittedQuery[2].Substring(1, splittedQuery[2].Length - 2);
				JavaPluginValueType pvt;
				string pluginValueTypeString;
				string parameters = null;
				if (capturableValueTypeString.Contains("("))
				{
					int indexOfParenthesis = capturableValueTypeString.IndexOf("(", StringComparison.Ordinal);
					pluginValueTypeString = capturableValueTypeString.Substring(0, indexOfParenthesis);
					parameters = capturableValueTypeString.Substring(indexOfParenthesis + 1, capturableValueTypeString.IndexOf(")", StringComparison.Ordinal) - (indexOfParenthesis + 1));
				}
				else
				{
					pluginValueTypeString = capturableValueTypeString;
				}
				pvt = (JavaPluginValueType)Enum.Parse(typeof(JavaPluginValueType),
					pluginValueTypeString);
				settings.CaptureName = name;
				settings.JavaPluginValueType = pvt;
				settings.PathToElement = indexes.ToArray();
				settings.Parameters = parameters;
				captureSettingsList.Add(settings);
			}
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!isCapturingPossible) return null;
			var sw = Stopwatch.StartNew();
			if (processNames != null && !processNames.Contains(processName)) return null;
			AppVersionLogger.LogAssemblyVersionFromProcId((uint)processId, "Java");
			if (windowTitle != null && 
			    !windowTitle.IsMatch(WindowTextHelper.GetWindowText(hWnd))) return null;
			var result = new List<KeyValuePair<string, string>>();
			try
			{
				foreach (JavaCaptureSettings settings in captureSettingsList)
				{
					settings.Hwnd = hWnd;
					settings.ProcessName = processName;


					if (!Environment.Is64BitOperatingSystem)
					{
						pluginInstance.Capture(settings);
					}

					bool isWow64Process;

					lock (cachedDictLock)
					{
						if (!is64BitProcessCachedDictionary.TryGetValue(processId, out isWow64Process))
						{
							Process p = Process.GetProcessById(processId);
							if (!WinApi.IsWow64Process(p.Handle, out isWow64Process))
								throw new Exception("WinApi IsWow64Process call error.");
							is64BitProcessCachedDictionary.Add(processId, isWow64Process);
						}
					}

					if (!isWow64Process)
					{
						EnsureProcessRunning();
						using (JavaCaptureClientWrapper wrapper = new JavaCaptureClientWrapper())
						{
							try
							{
								result.Add(wrapper.Client.Capture(settings));
							}
							catch (FaultException<FailReason> ex)
							{
								if (ex.Detail.Type == FailReasonType.CaptureImpossible)
								{
									isCapturingPossible = false;
								}
								else throw;
							}
						}
					}
					else
					{
						var res = pluginInstance.Capture(settings);
						if (res.HasValue)
							result.Add(res.Value);
					}
				}
			}
			catch (Exception ex)
			{
				log.Warn("Java accessibiltiy capture failure.", ex);
				if (ex.InnerException is PipeException
				    && ((PipeException)ex.InnerException).ErrorCode == -2146232800)
				{
					ProcessCoordinator.JavaCaptureProcessCoordinator.RestartIfNeeded();
				}
			}
			log.Verbose($"Java accessibility capture took {sw.Elapsed.TotalMilliseconds:0.000} ms. Result: {{{string.Join(Environment.NewLine, result.Select(x => x.Key + " - " + x.Value))}}}");
			return result;
		}

		private void EnsureProcessRunning()
		{
			if (!isJavaProcessRunning)
			{
				ProcessCoordinator.JavaCaptureProcessCoordinator.Start();
				isJavaProcessRunning = true;
			}
		}
	}
}
