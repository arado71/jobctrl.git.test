using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace JCAutomation.SystemAdapter
{
	public static class WindowHelper
	{
		[ThreadStatic]
		private static StringBuilder localSb;

		public static string GetClassName(IntPtr hWnd)
		{
			if (localSb == null) localSb = new StringBuilder(64);
			while (true)
			{
				GetClassName(hWnd, localSb, localSb.Capacity);
				if (localSb.Length != localSb.Capacity - 1)
				{
					return localSb.ToString();
				}
				localSb.Capacity *= 2;
			}
		}

		public static string GetWindowText(IntPtr hWnd)
		{
			if (localSb == null) localSb = new StringBuilder(64);
			int windowTextLength = GetWindowTextLength(hWnd) + 1;
			if (windowTextLength > localSb.Capacity)
			{
				localSb.Capacity = windowTextLength;
			}

			GetWindowText(hWnd, localSb, windowTextLength);
			return localSb.ToString();
		}

		public static string GetWindowTextMsg(IntPtr hWnd)
		{
			if (localSb == null) localSb = new StringBuilder(64);
			var windowTextLength = SendMessage(hWnd, WM_GETTEXTLENGTH, 0, 0) + 1;
			if (windowTextLength > localSb.Capacity)
			{
				localSb.Capacity = windowTextLength;
			}

			SendMessage(hWnd, WM_GETTEXT, windowTextLength, localSb);
			return localSb.ToString();
		}

		public static int GetWindowProcessId(IntPtr hWnd)
		{
			int procId;
			GetWindowThreadProcessId(hWnd, out procId);
			return procId;
		}

		public static Bitmap GetBitmap(IntPtr hWnd)
		{
			var clientArea = GetClientRectInScreenCoord(hWnd);
			return GetBitmap(hWnd, clientArea);
		}

		public static Rectangle GetClientRectInScreenCoord(IntPtr hWnd)
		{
			RECT rect;
			GetWindowRect(hWnd, out rect);
			var point = new POINT(rect.Left, rect.Top);
			ClientToScreen(hWnd, ref point);
			return new Rectangle(point.X, point.Y, rect.Right - rect.Left, rect.Bottom - rect.Top);
		}

		public static Bitmap GetBitmap(IntPtr hWnd, Rectangle area)
		{
			IntPtr hSrce = IntPtr.Zero, hDest = IntPtr.Zero, hBmp = IntPtr.Zero, hOldBmp = IntPtr.Zero;
			try
			{
				hSrce = GetWindowDC(hWnd);
				if (hSrce == IntPtr.Zero) return null;
				hDest = CreateCompatibleDC(hSrce);
				if (hDest == IntPtr.Zero) return null;
				hBmp = CreateCompatibleBitmap(hSrce, area.Width, area.Height);
				if (hBmp == IntPtr.Zero) return null;
				hOldBmp = SelectObject(hDest, hBmp);
				var res = BitBlt(hDest, 0, 0, area.Width, area.Height, hSrce, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
				if (res == 0) return null;
				//http://msdn.microsoft.com/en-us/library/k061we7x.aspx
				//The FromHbitmap method makes a copy of the GDI bitmap; so you can release the incoming GDI bitmap using the GDI DeleteObject method immediately after creating the new Image.
				return Image.FromHbitmap(hBmp);
			}
			catch (Exception ex)
			{
				return null;
			}
			finally
			{
				if (hDest != IntPtr.Zero && hOldBmp != IntPtr.Zero) SelectObject(hDest, hOldBmp);
				if (hBmp != IntPtr.Zero) DeleteObject(hBmp);
				if (hDest != IntPtr.Zero) DeleteDC(hDest);
				if (hWnd != IntPtr.Zero && hSrce != IntPtr.Zero) ReleaseDC(hWnd, hSrce);
			}
		}

		public static IEnumerable<IntPtr> GetChildren(IntPtr hWnd)
		{
			var lastChild = IntPtr.Zero;
			while ((lastChild = FindWindowEx(hWnd, lastChild, null, null)) != IntPtr.Zero)
			{
				yield return lastChild;
			}
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;        // x position of upper-left corner
			public int Top;         // y position of upper-left corner
			public int Right;       // x position of lower-right corner
			public int Bottom;      // y position of lower-right corner
		}

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

			public static implicit operator System.Drawing.Point(POINT p)
			{
				return new System.Drawing.Point(p.X, p.Y);
			}

			public static implicit operator POINT(System.Drawing.Point p)
			{
				return new POINT(p.X, p.Y);
			}
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr GetWindowDC(IntPtr ptr);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern int BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern IntPtr DeleteObject(IntPtr hDc);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern IntPtr DeleteDC(IntPtr hDc);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		private const uint WM_GETTEXT = 0x000D;
		private const uint WM_GETTEXTLENGTH = 0x000E;

		[DllImport("user32.dll")]
		private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, StringBuilder lParam);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		[DllImport("user32.dll")]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
	}
}
