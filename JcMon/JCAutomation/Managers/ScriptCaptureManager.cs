using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JcExtract;
using JCAutomation.Data;
using JCAutomation.SystemAdapter;
using log4net;
using ProcessNameHelper = Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo.ProcessNameHelper;

namespace JCAutomation.Managers
{
	internal class ScriptCaptureManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (ScriptCaptureManager));

		internal event EventHandler<CaptureEventArgs> Captured;

		public ScriptCaptureManager() : base(log)
		{
		}

		protected override void ManagerCallbackImpl()
		{
			var hWnd = WindowHelper.GetForegroundWindow();
			if (hWnd == IntPtr.Zero) return;
			var processId = WindowHelper.GetWindowProcessId(hWnd);
			string processName;
			if (!ProcessNameHelper.TryGetProcessName(processId, out processName)) return;
			var captures = Configuration.ProcessFuncs;
			if (captures == null) return;
			var result = new List<Tuple<string, string, double>>(captures.Count);
			foreach (var capture in captures)
			{
				var sw = Stopwatch.StartNew();
				var captureResult = capture.Capture(hWnd);
				var elapsed = sw.Elapsed.TotalMilliseconds;
				result.Add(Tuple.Create(capture.Name, captureResult, elapsed));
			}

			OnCaptured(hWnd, processName, result);
		}

		protected void OnCaptured(IntPtr windowHandle, string processName, List<Tuple<string, string, double>> capturedElements)
		{
			var evt = Captured;
			if (evt != null)
				evt(this, new CaptureEventArgs()
				{
					ScriptCapture = new ScriptCapture()
					{
						Values =
							capturedElements.Select(x => new CapturedValue() {Key = x.Item1, Time = x.Item3, Value = x.Item2}).ToList(),
						WindowHandle = windowHandle,
						ProcessName = processName,
					}
				});
		}

		protected override int ManagerCallbackInterval
		{
			get { return Configuration.CaptureInterval; }
		}
	}
}
