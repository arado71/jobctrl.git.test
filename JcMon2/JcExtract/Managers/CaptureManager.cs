using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using JcMon2.SystemAdapter;
using log4net;

namespace JcExtract.Managers
{
	internal class CaptureManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (CaptureManager));

		internal event EventHandler<CaptureEventArgs> Captured;

		public CaptureManager() : base(log)
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
					Capture = new Capture()
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
