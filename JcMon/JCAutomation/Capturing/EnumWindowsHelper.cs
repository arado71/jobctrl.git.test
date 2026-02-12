namespace JCAutomation.Capturing
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class EnumWindowsHelper
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, int lParam);
        private static string GetClassName(IntPtr hWnd)
        {
            int capacity = 0x40;
            while (true)
            {
                StringBuilder lpClassName = new StringBuilder(capacity);
                GetClassName(hWnd, lpClassName, lpClassName.Capacity);
                if (lpClassName.Length != (capacity - 1))
                {
                    return lpClassName.ToString();
                }
                capacity *= 2;
            }
        }

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        private static Rectangle GetClientRectInScreenCoord(IntPtr hWnd)
        {
            RECT rect;
            GetClientRect(hWnd, out rect);
            POINT lpPoint = new POINT(rect.Left, rect.Top);
            ClientToScreen(hWnd, ref lpPoint);
            return new Rectangle(lpPoint.X, lpPoint.Y, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        private static Rectangle GetWindowRect(IntPtr hWnd)
        {
            RECT rect;
            GetWindowRect(hWnd, out rect);
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        public static List<DesktopWindow> GetWindowsInfo(bool isLocked = false)
        {
            WindowInfoBuilder builder = new WindowInfoBuilder();
            EnumWindows(new EnumWindowsProc(builder.ProcessWindow), 0);
            builder.EnsureActiveWindow(isLocked);
            return builder.WindowsInfo;
        }

        private static string GetWindowText(IntPtr hWnd)
        {
            int capacity = GetWindowTextLength(hWnd) + 1;
            StringBuilder text = new StringBuilder(capacity);
            GetWindowText(hWnd, text, capacity);
            return text.ToString();
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);
        private static int GetWindowThreadProcessIdWrapper(IntPtr hWnd)
        {
            int num;
            GetWindowThreadProcessId(hWnd, out num);
            return num;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        private static extern bool IsIconic(IntPtr hWnd);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        private static extern bool IsZoomed(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator Point(EnumWindowsHelper.POINT p)
	        {
		        return new Point(p.X, p.Y);
	        }

	        public static implicit operator EnumWindowsHelper.POINT(Point p)
	        {
		        return new EnumWindowsHelper.POINT(p.X, p.Y);
	        }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private class WindowInfoBuilder
        {
            private bool activeFound;
            private IntPtr activeHWnd;
            private bool hasActiveWindow;
            public readonly List<DesktopWindow> WindowsInfo = new List<DesktopWindow>();

            public void EnsureActiveWindow(bool isLocked)
            {
                if (!this.activeFound)
                {
                    DesktopWindow item = new DesktopWindow {
                        Handle = IntPtr.Zero,
                        IsActive = true,
                        WindowRect = new Rectangle(0, 0, 0, 0),
                        ClientRect = new Rectangle(0, 0, 0, 0),
                        Title = "",
                        ProcessId = isLocked ? -1 : 0,
                        CreateDate = DateTime.UtcNow
                    };
                    this.WindowsInfo.Insert(0, item);
                    this.activeFound = true;
                }
            }

            public bool ProcessWindow(IntPtr hWnd, int lParam)
            {
                try
                {
                    if (!this.hasActiveWindow)
                    {
                        this.activeHWnd = EnumWindowsHelper.GetForegroundWindow();
                        this.hasActiveWindow = true;
                    }
                    if (!EnumWindowsHelper.IsWindowVisible(hWnd))
                    {
                        return true;
                    }
                    DesktopWindow item = new DesktopWindow {
                        Handle = hWnd,
                        IsActive = (this.activeHWnd != IntPtr.Zero) && (this.activeHWnd == hWnd),
                        Minimized = EnumWindowsHelper.IsIconic(hWnd),
                        IsMaximized = EnumWindowsHelper.IsZoomed(hWnd),
                        WindowRect = EnumWindowsHelper.GetWindowRect(hWnd),
                        ClientRect = EnumWindowsHelper.GetClientRectInScreenCoord(hWnd),
                        Title = EnumWindowsHelper.GetWindowText(hWnd),
                        ProcessId = EnumWindowsHelper.GetWindowThreadProcessIdWrapper(hWnd),
                        ClassName = EnumWindowsHelper.GetClassName(hWnd),
                        CreateDate = DateTime.UtcNow
                    };
                    item.ProcessName = ProcessNameResolver.Instance.GetProcessName(item.ProcessId);
                    this.activeFound |= item.IsActive;
                    this.WindowsInfo.Add(item);
                }
                catch (Exception)
                {
                }
                return true;
            }
        }
    }
}

