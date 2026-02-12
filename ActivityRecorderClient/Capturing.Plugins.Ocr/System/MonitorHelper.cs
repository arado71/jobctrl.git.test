using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	public class Monitors : List<DeviceInfo>
	{
		public Monitors()
		{
			var taskbars = Taskbar.FindDockedTaskBars();
			this.AddRange(MonitorHelper.GetMonitorsInfo());
			this.ForEach(e =>
			{
				e.Screen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == e.DeviceName);
				if (0 < taskbars.Count)
					foreach (var r in taskbars)
						if (e.MonitorArea.Contains(r.Area))
							e.Taskbar = r;
			});
		}
		public DeviceInfo this[Screen screen]
		{
			get
			{
				return this.SingleOrDefault(e => e.Screen.Equals(screen));
			}
		}

		public DeviceInfo Actual(Rectangle rect)
		{
			if (Count == 1)
				return this[0];
			var corners = new List<Rectangle>
			{
				new Rectangle(rect.X, rect.Y, rect.Width, 1),
				new Rectangle(rect.X, rect.Y, 1, rect.Height),
				new Rectangle(rect.X, rect.Y+rect.Height, rect.Width, 1),
				new Rectangle(rect.X+rect.Width, rect.Y, 1, rect.Height)
			};
			foreach (var corner in corners)
			{
				var monitor = this.Find(e => e.MonitorArea.Contains(corner));
				if(monitor != null)				
					return monitor;
			}
			return null;
		}
		private static class MonitorHelper
		{
			private const int DesktopVertRes = 117;
			private const int DesktopHorzRes = 118;
			private const int VERTRES = 10;
			private const int HORZRES = 8;
			private static List<DeviceInfo> _result;

			[DllImport("user32.dll", EntryPoint = "EnumDisplayMonitors")]
			private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum,
				IntPtr dwData);

			[DllImport("gdi32.dll", EntryPoint = "CreateDC")]
			private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

			[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetMonitorInfo")]
			private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

			[DllImport("User32.dll", EntryPoint = "ReleaseDC")]
			private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

			[DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps")]
			private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

			public static List<DeviceInfo> GetMonitorsInfo()
			{
				_result = new List<DeviceInfo>();
				EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumCallback, IntPtr.Zero);
				return _result;
			}

			private static bool MonitorEnumCallback(IntPtr hMonitor, IntPtr hdcMonitor, ref EnumWindowsHelper.RECT lprcMonitor, IntPtr dwData)
			{
				var mi = new MONITORINFOEX { Size = Marshal.SizeOf(typeof(MONITORINFOEX)) };
				var success = GetMonitorInfo(hMonitor, ref mi);
				if (!success) return true;
				var dc = CreateDC(mi.DeviceName, mi.DeviceName, null, IntPtr.Zero);
				var verticalResolution = GetDeviceCaps(dc, DesktopVertRes);
				var horizontalResolution = GetDeviceCaps(dc, DesktopHorzRes);
				var logicalScreenHeight = GetDeviceCaps(dc, VERTRES);
				var logicalScreenWidth = GetDeviceCaps(dc, HORZRES);
				var screen = Screen.FromHandle(hMonitor);
				var di = new DeviceInfo
				{
					Handle = hMonitor,
					DeviceName = mi.DeviceName,
					MonitorArea = new Rectangle(mi.Monitor.Left, mi.Monitor.Top, mi.Monitor.Width, mi.Monitor.Height),
					VerticalResolution = verticalResolution,
					HorizontalResolution = GetDeviceCaps(dc, DesktopHorzRes),
					VScale = verticalResolution / (float)logicalScreenHeight,
					HScale = horizontalResolution / (float)logicalScreenWidth,
					Screen = screen
				};

				ReleaseDC(IntPtr.Zero, dc);
				_result.Add(di);
				return true;
			}

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			internal struct MONITORINFOEX
			{
				public int Size;
				public EnumWindowsHelper.RECT Monitor;
				public EnumWindowsHelper.RECT WorkArea;
				public uint Flags;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
				public string DeviceName;
			}

			private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref EnumWindowsHelper.RECT lprcMonitor, IntPtr dwData);
		}
	}
}