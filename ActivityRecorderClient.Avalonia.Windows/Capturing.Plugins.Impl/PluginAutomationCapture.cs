using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginAutoCapture : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly IEnumerable<KeyValuePair<string, string>> EmptyResult = Enumerable.Empty<KeyValuePair<string, string>>();
		private static readonly int WarnInterval = ConfigManager.RuleMatchingInterval / 2;

		private const string PluginId = "JobCTRL.AutoCapture";
		private const string ParamProcess = "ProcessName";
		private const string ParamCapture = "Capture";

		private HashSet<string> processNames = new HashSet<string>();
		private List<AutomationCapture> captures = new List<AutomationCapture>();
		private int lastScreenReaderTest;

		public string Id
		{
			get { return PluginId; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamProcess;
			yield return ParamCapture;
		}

		public void SetParameter(string name, string value)
		{
			if (string.Equals(name, ParamProcess, StringComparison.OrdinalIgnoreCase))
			{
				processNames = new HashSet<string>(value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
			}

			if (string.Equals(name, ParamCapture, StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					captures = AutomationScriptHelper.Compile(value);
				}
				catch (Exception ex)
				{
					log.Warn("Failed to compile script", ex);
					captures = null;
				}
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			return captures.Select(x => x.Name);
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (captures == null) return EmptyResult;
			if (!processNames.Contains(processName)) return EmptyResult;

			if (ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableAutomationCapture)) return EmptyResult;

			if (captures.Count > 0 && Environment.TickCount - lastScreenReaderTest > 60000)
			{
				if (!WinApi.IsScreenReaderRunning())
				{
					WinApi.ScreenReaderOn();
					log.Debug("SPI_SCREENREADER was not set, now set");
				}
				lastScreenReaderTest = Environment.TickCount;
			}

			var result = new Dictionary<string, string>(captures.Count);
			foreach (var capture in captures)
			{
				if (result.ContainsKey(capture.Name)) continue;
				var captureResult = capture.Capture(hWnd);
				log.VerboseFormat("AutomationCapture {0} took {1} ms to capture {2}", capture.Name, capture.LastRunTime, captureResult);
				if (capture.LastRunTime > WarnInterval)
				{
					log.DebugFormat("AutomationCapture {0} took {1} ms to capture {2}", capture.Name, capture.LastRunTime, captureResult);
				}

				if (captureResult != null) result.Add(capture.Name, captureResult);
			}

			return result;
		}
	}
}
