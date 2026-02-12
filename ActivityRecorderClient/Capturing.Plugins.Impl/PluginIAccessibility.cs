using JC.IAccessibilityLib;
using JobCTRL.Plugins;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	class PluginIAccessibility : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly IEnumerable<KeyValuePair<string, string>> EmptyResult = Enumerable.Empty<KeyValuePair<string, string>>();
		private static readonly int WarnInterval = ConfigManager.RuleMatchingInterval / 2;

		private const string PluginId = "JobCTRL.IAccessibility";
		private const string ParamProcess = "ProcessName";
		private const string ParamCapture = "Capture";

		private HashSet<string> processNames = new HashSet<string>();
		private List<IAccessibilityCapture> captures = new List<IAccessibilityCapture>();
		private int lastScreenReaderTest;
		private static bool initialized = false;
		private static object initlock = new object();

		private static WinApi.WinEventDelegate accessibilityDelegate;
		private static IntPtr winEventPtr = IntPtr.Zero;

		public PluginIAccessibility()
		{
			lock (initlock)
			{
				if (!initialized) Platform.Factory.GetGuiSynchronizationContext().Send(_ => initialize(), null);
			}
		}

		private static void initialize()
		{
			try
			{
				accessibilityDelegate = winEventHook;
				winEventPtr = WinApi.SetWinEventHook(WinApi.SystemEventContants.EVENT_SYSTEM_ALERT, WinApi.SystemEventContants.EVENT_SYSTEM_ALERT, IntPtr.Zero,
					accessibilityDelegate, 0, 0, WinApi.WinEventFlags.WINEVENT_OUTOFCONTEXT);
				initialized = true;
			}
			catch (Exception ex)
			{
				log.Error("Couldn't initialize winEventHook.", ex);
				initialized = true;
			}
		}

		private static void winEventHook(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, uint idObject, uint idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (idObject == 1)
			{
				Accessibility.IAccessible accessible;
				object child;
				var res = WinApi.AccessibleObjectFromEvent(hwnd, idObject, idChild, out accessible, out child);
				log.Debug($"Message sent to Chrome window {hwnd}. Result: {res}");
			}
		}

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
					captures = IAccessibilityScriptHelper.Compile(value);
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
				log.VerboseFormat("IAccessibilityCapture {0} took {1} ms to capture {2}", capture.Name, capture.LastRunTime, captureResult);
				if (capture.LastRunTime > WarnInterval)
				{
					log.DebugFormat("IAccessibilityCapture {0} took {1} ms to capture {2}", capture.Name, capture.LastRunTime, captureResult);
				}

				if (captureResult != null) result.Add(capture.Name, captureResult);
			}

			return result;
		}
	}
}
