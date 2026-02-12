using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Capturing.Plugins.Ocr;
using TcT.OcrSnippets;

namespace TcT.ActivityRecorderClient.SnippingTool
{
    public static class EnumWindowsHelper
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly bool isVistaOrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;
        private static readonly bool isWindows8OrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 2);
        private static readonly int[] sizes = { 260, 1024, 32767 };
		private static readonly WindowInfoBuilder builder = new WindowInfoBuilder();
	    private delegate bool EnumWindowCallback(IntPtr hwnd, int lparam);
        public static List<DeviceInfo> Monitors { get { return new Monitors(); } }

        public static List<WindowInfo> GetWindowsInfo(Stopwatch sw)
        {
            EnumWindows(new EnumWindowCallback(builder.ProcessEnumWindow), 0);
			log.DebugFormat("OCR - ProcessEnumWindow {0} ms", sw.ElapsedMilliseconds);
            builder.ProcessAdditionalWindows();
			log.DebugFormat("OCR - ProcessAdditionalWindows {0} ms", sw.ElapsedMilliseconds);
            builder.EnsureActiveWindow(false); //legacy hax. It's good to have one active window for every capture (for time calculations and for rules)
			log.DebugFormat("OCR - EnsureActiveWindow {0} ms", sw.ElapsedMilliseconds);
            return builder.WindowsInfo;
        }

	    public static WindowInfo GetWindowInfo(IntPtr hWnd)
	    {
	        WindowInfo wi;
	        if (builder.TryGetWindow(hWnd, out wi))
	            return wi;
	        return null;
	    }

	    private class WindowInfoBuilder
        {
            public readonly List<WindowInfo> WindowsInfo = new List<WindowInfo>();
            private readonly HashSet<IntPtr> win8EnumWindowHandles = new HashSet<IntPtr>(); //only relevant for win8 or later
            private readonly HashSet<IntPtr> win8AllHandles = new HashSet<IntPtr>(); //only relevant for win8 or later
            private int insertAtIdx;
            private bool hasActiveWindow;
            private IntPtr activeHWnd;
            private bool activeFound;

            public void EnsureActiveWindow(bool isLocked)
            {
                if (activeFound) return;
                WindowsInfo.Insert(0, new WindowInfo()
                {
                    Handle = IntPtr.Zero,
                    IsActive = true,
                    WindowRect = new Rectangle(0, 0, 0, 0),
                    ClientRect = new Rectangle(0, 0, 0, 0),
                    Title = "",
                    ProcessId = isLocked ? -1 : 0,
                    Image = null
                });
                activeFound = true;
            }
            public void ProcessAdditionalWindows()
            {
                if (!isWindows8OrLater) return;
                try
                {
                    var i = 0;
                    var hWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, IntPtr.Zero);
                    while (hWnd != IntPtr.Zero && i++ < 3000) //Enumerate only first 3 thousand windows to protect against infinite loop. 
                    {
                        ProcessAdditionalWindow(hWnd);
                        hWnd = FindWindowEx(IntPtr.Zero, hWnd, null, IntPtr.Zero);
                    }
                    if (hWnd != IntPtr.Zero)
                        log.WarnAndFail("Infinite loop detected.");
                }
                catch (Exception ex)
                {
                    log.Error("EnumWindowsByFindWindow failed.", ex);
                }
            }
            private void ProcessAdditionalWindow(IntPtr hWnd) //windows from findwindow are in z-order too, we assume the z-order is not changed since enumwindows
            {
                Debug.Assert(isWindows8OrLater);
                if (win8AllHandles.Contains(hWnd))
                {
                    if (win8EnumWindowHandles.Contains(hWnd)) //Update insert position
                    {
                        while (insertAtIdx < WindowsInfo.Count && WindowsInfo[insertAtIdx].Handle != hWnd) insertAtIdx++;
                        if (insertAtIdx < WindowsInfo.Count) insertAtIdx++;
                    }
                    return;
                }
                //new handle
                WindowInfo window;
                if (TryGetWindow(hWnd, out window))
                {
                    int cloaked;
                    if (DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out cloaked, 4) == 0 && cloaked != 0) return; //Check if potential metro app window is visible or moved to the background.
                    WindowsInfo.Insert(insertAtIdx++, window);
                }
            }
            public bool ProcessEnumWindow(IntPtr hWnd, int lParam)
            {
                WindowInfo window;
                if (TryGetWindow(hWnd, out window))
                {
                    WindowsInfo.Add(window);
                    if (isWindows8OrLater) win8EnumWindowHandles.Add(hWnd);
                }
                return true; //process next window
            }
            public bool TryGetWindow(IntPtr hWnd, out WindowInfo window)
            {
                window = null;
                try
                {
                    if (isWindows8OrLater) win8AllHandles.Add(hWnd);
                    if (!hasActiveWindow)
                    {
                        activeHWnd = GetForegroundWindow(); //returns IntPtr.Zero on locked machine -> no active window (can also happen when minimizing)
                        hasActiveWindow = true;
                    }
                    var visible = IsWindowVisible(hWnd);
                    if (!visible) return false; //proceed to the next window if this is not visible
                    var className = WindowTextHelper.GetClassName(hWnd);
					log.DebugFormat("OCR -- {0}", className);
                    if (className == "TaskListThumbnailWnd") return false; //this is a transparent window on Win7 (shows censored ones / hides other windows
                    if (isWindows8OrLater &&
                        (className == "EdgeUiInputWndClass" || className == "EdgeUiInputTopWndClass" ||
                         className == "Shell_LightDismissOverlay")) return false;   //these are transparent windows on Win8
                    var PID = GetWindowThreadProcessIdWrapper(hWnd);
                    string PNA;
                    TryGetProcessName(PID, out PNA);
                    var clientRect = CorrectCoordinates(hWnd, GetClientRectInScreenCoord(hWnd));
                    if (PNA == "chrome.exe") Debug.WriteLine(hWnd.ToInt64());
                    var image = GetBitmap(hWnd, Monitors.SingleOrDefault(e => e.MonitorArea.Contains(clientRect)));
                    window = new WindowInfo
                    {
                        Handle = hWnd,
                        IsActive = activeHWnd != IntPtr.Zero && activeHWnd == hWnd,
                        IsMaximized = IsZoomed(hWnd),
                        WindowRect = GetWindowRect(hWnd),
                        ClientRect = clientRect,
                        Title = WindowTextHelper.GetWindowText(hWnd),
                        ProcessId = PID,
                        ClassName = GetClassName(hWnd),
                        ProcessName = PNA,
                        Image = image
                    };
                    activeFound |= window.IsActive;
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error("Unable to process window with hande " + hWnd, ex);
                }
                return false; //process next window
            }
        }

        public static Rectangle CorrectCoordinates(IntPtr mHandle, Rectangle rect)
        {
	        bool isZoomed = IsZoomed(mHandle);
            if (!isZoomed) return rect;
            int offset, X = rect.X, Y = rect.Y, W = rect.Width, H = rect.Height;
            if (rect.Y < 0)
            {
                offset = Math.Abs(rect.Y);
                Y = 0;
                H -= offset << 1;
                X += offset;
                W -= offset << 1;
            }
            return new Rectangle(X, Y, W, H);
        }

        private static int GetWindowThreadProcessIdWrapper(IntPtr hWnd)
        {
            int procId;
            GetWindowThreadProcessId(hWnd, out procId);
            return procId;
        }

        private static string GetClassName(IntPtr hWnd)
        {
            var length = 64;
            while (true)
            {
                var sb = new StringBuilder(length);
                GetClassName(hWnd, sb, sb.Capacity);
                if (sb.Length != length - 1)
                    return sb.ToString();
                length *= 2;
            }
        }

        private static Rectangle GetClientRectInScreenCoord(IntPtr hWnd)
        {
            RECT rect;
            GetClientRect(hWnd, out rect);
            var point = new POINT(rect.Left, rect.Top);
            ClientToScreen(hWnd, ref point);
            return new Rectangle(point.X, point.Y, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
        public static Bitmap GetBitmap(IntPtr hwnd, DeviceInfo monitor)
        {
            RECT window;
            GetWindowRect(hwnd, out window);
	        log.DebugFormat("OCR GetBitmap1 {0} {1}", window.Width, window.Height);
            Rectangle r = CorrectCoordinates(hwnd, window);
            bool hScaled = monitor.HScale.CompareTo(1.0) != 0;
            bool vScaled = monitor.VScale.CompareTo(1.0) != 0;
            var rcWindow = new RECT(
                hScaled ? (int)(r.Left * monitor.HScale) : r.Left,
                vScaled ? (int)(r.Top * monitor.VScale) : r.Top,
                hScaled ? (int)(r.Right * monitor.HScale) : r.Right,
                vScaled ? (int)(r.Bottom * monitor.VScale) : r.Bottom);
			log.DebugFormat("OCR GetBitmap2 {0} {1}", rcWindow.Width, rcWindow.Height);
            using (Bitmap bmp = new Bitmap(rcWindow.Width, rcWindow.Height, PixelFormat.Format32bppArgb))
            {
                Graphics gfxBmp = Graphics.FromImage(bmp);
                gfxBmp.InterpolationMode = InterpolationMode.High;
                gfxBmp.CompositingQuality = CompositingQuality.HighQuality;
                gfxBmp.SmoothingMode = SmoothingMode.AntiAlias;

                IntPtr hdcBitmap = gfxBmp.GetHdc();
                PrintWindow(hwnd, hdcBitmap, 0);

                gfxBmp.ReleaseHdc(hdcBitmap);
                gfxBmp.Dispose();
                return bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format32bppPArgb);
            }
        }
        public static Rectangle GetWindowRect(IntPtr hWnd)
        {
            RECT rect;
            GetWindowRect(hWnd, out rect);
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        private static bool TryGetProcessName(int processId, out string processName)
        {
            var cache = new Dictionary<int, KeyValuePair<DateTime, string>>();
            if (cache.ContainsKey(processId))
                if (DateTime.UtcNow - cache[processId].Key > TimeSpan.FromSeconds(30))
                {
                    cache.Remove(processId);
                }
                else
                {
                    processName = cache[processId].Value;
                    return true;
                }

            try
            {
                using (var process = Process.GetProcessById(processId))
                {
                    processName = process.MainModule.ModuleName;
                    cache.Add(processId, new KeyValuePair<DateTime, string>(DateTime.UtcNow, processName));
                    return true;
                }
            }
            catch (Exception ex)
            {
                //casing is different for QueryFullProcessImageName so use it as a backup only atm.
                //299 Only part of a ReadProcessMemory or WriteProcessMemory request was completed
                //5 Access denied
                if (isVistaOrLater)
                    using (
                        var safeHandle = SafeProcessHandle.OpenProcess((int)ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false,
                            processId))
                    {
                        if (!safeHandle.IsInvalid)
                        {
                            if (TryQueryProcessFileName(safeHandle, out processName))
                            {
                                cache.Add(processId, new KeyValuePair<DateTime, string>(DateTime.UtcNow, processName));
                                return true;
                            }

                            return false;
                        }
                    }
            }
            processName = null;
            return false;
        }

        private static bool TryQueryProcessFileName(SafeProcessHandle handle, out string fileName)
        {
            try
            {
                string path;
                if (TryQueryFullProcessImageName(handle, out path))
                {
                    fileName = Path.GetFileName(path);
                    return true;
                }
            }
            catch (Exception ex)
            {
            }
            fileName = null;
            return false;
        }

        private static bool TryQueryFullProcessImageName(SafeProcessHandle handle, out string path)
        {
            foreach (var size in sizes)
            {
                var currSize = size;
                var sb = new StringBuilder(currSize);
                if (!QueryFullProcessImageName(handle, 0, sb, ref currSize))
                {
                    var errCode = Marshal.GetLastWin32Error();
                    if (errCode == 122) continue; //ERROR_INSUFFICIENT_BUFFER
                    break; //other error
                }
                path = sb.ToString();
                return true;
            }
            path = null;
            return false;
        }
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowCallback lpEnumFunc, int lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(SafeProcessHandle hProcess, uint dwFlags, StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out int pvAttribute, int cbAttribute);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        [DllImport("User32.dll")]
		private static extern IntPtr MonitorFromPoint([In]Point pt, [In]uint dwFlags);
		const int MONITOR_DEFAULTTONEAREST = 2;

	    public static IntPtr MonitorFromPoint([In] Point pt)
	    {
		    uint dwFlags = MONITOR_DEFAULTTONEAREST;
		    return MonitorFromPoint(pt, dwFlags);
	    }

        private enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }

        public class WindowInfo
        {
            public bool IsActive;
            public bool IsMaximized;
            public Rectangle WindowRect;
            public Rectangle ClientRect;

            public string Title { set; get; }
            public string ProcessName { get; set; }
            public int ProcessId { get; set; }
            public string ClassName { get; set; }
            public IntPtr Handle { get; set; }
            private Bitmap image;
            public Bitmap Image { get { return image; }
                set { image = value; }
            }
        }

        private enum ProcessAccessFlags
        {
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public readonly int Left; // x position of upper-left corner
            public readonly int Top; // y position of upper-left corner
            public readonly int Right; // x position of lower-right corner
            public readonly int Bottom; // y position of lower-right corner
            public int Width { get { return Right - Left; } }
            public int Height { get { return Bottom - Top; } }
            public int X { get { return Left; } }
            public int Y { get { return Top; } }

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public override string ToString()
            {
                return string.Format("RECT  L:{1:d3} T:{0:D3}-R:{3:d3} B:{2:d3}", Top, Left, Bottom, Right);
            }

            public static implicit operator Rectangle(RECT b)  // explicit byte to digit conversion operator
            {
                Rectangle r = new Rectangle(b.Left, b.Top, b.Right, b.Bottom);
                System.Console.WriteLine("Conversion occurred.");
                return r;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public readonly int X;
            public readonly int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static implicit operator Point(POINT p)
            {
                return new Point(p.X, p.Y);
            }

            public static implicit operator POINT(Point p)
            {
                return new POINT(p.X, p.Y);
            }

            public override string ToString()
            {
                return string.Format("POIN X:{0:D3} Y:{1:d3}", X, Y);
            }
        }

        private enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
    }
}