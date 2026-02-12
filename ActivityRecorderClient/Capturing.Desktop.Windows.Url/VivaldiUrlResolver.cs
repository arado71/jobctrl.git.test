using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public class VivaldiUrlResolver : ChromiumUrlResolverBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private WinApi.WinEventDelegate accessibilityDelegate;
		private IntPtr winEventPtr = IntPtr.Zero;

		public VivaldiUrlResolver() : base(log)
		{
		}

		public override string ProcessName => "vivaldi.exe";

		public override Browser Browser => Browser.Vivaldi;

		protected override void Initialize()
		{
			accessibilityDelegate = winEventHook;
			winEventPtr = WinApi.SetWinEventHook(WinApi.SystemEventContants.EVENT_SYSTEM_ALERT, WinApi.SystemEventContants.EVENT_SYSTEM_ALERT, IntPtr.Zero,
				accessibilityDelegate, 0, 0, WinApi.WinEventFlags.WINEVENT_OUTOFCONTEXT);
			Automation.AddAutomationFocusChangedEventHandler(new AutomationFocusChangedEventHandler((x, y) => { }));
			base.Initialize();
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

		~VivaldiUrlResolver()
		{
			if (winEventPtr != IntPtr.Zero) WinApi.UnhookWinEvent(winEventPtr);
		}
	}
}
