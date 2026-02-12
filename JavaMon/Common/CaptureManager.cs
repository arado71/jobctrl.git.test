using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient;

namespace Tct.JcMon.Common
{
	class CaptureManager: PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public delegate void CaptureOccuredDelegate(IEnumerable<CaptureResult> captures);

		public event CaptureOccuredDelegate CaptureOccured;

		public CaptureManager()
		:base(log, false)
		{

		}

		protected override void ManagerCallbackImpl()
		{
			List<CaptureResult> res = new List<CaptureResult>();
			IntPtr hwnd = Configuration.Hwnd;
			lock (Configuration.CaptureFuncs)
			foreach (var capture in Configuration.CaptureFuncs)
			{
				try
				{
					res.Add(capture(hwnd));
				}
				catch (Exception ex)
				{
					log.Error("Capture failed.", ex);
				}
			}
			CaptureOccured?.Invoke(res);
		}

		protected override int ManagerCallbackInterval => Configuration.CaptureInterval;

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();
	}
}
