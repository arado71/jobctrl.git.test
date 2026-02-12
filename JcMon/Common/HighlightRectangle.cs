namespace Tct.JcMon.Common
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    internal class HighlightRectangle : IDisposable
    {
        private Form _bottomForm = new Form();
        private System.Drawing.Color _color;
        private Form _leftForm = new Form();
        private Rectangle _location;
        private Form _rightForm = new Form();
        private bool _show;
        private ToolTip _toolTipBottom;
        private ToolTip _toolTipLeft;
        private ToolTip _toolTipRight;
        private string _tooltipText;
        private ToolTip _toolTipTop;
        private Form _topForm = new Form();
        private int _width = 3;

        public HighlightRectangle()
        {
            Form[] formArray = new Form[] { this._leftForm, this._topForm, this._rightForm, this._bottomForm };
            foreach (Form form in formArray)
            {
                form.FormBorderStyle = FormBorderStyle.None;
                form.ShowInTaskbar = false;
                form.TopMost = true;
                form.Visible = false;
                form.Left = 0;
                form.Top = 0;
                form.Width = 1;
                form.Height = 1;
                form.Show();
                form.Hide();
                form.Opacity = 0.5;
                int windowLong = UnsafeNativeMethods.GetWindowLong(form.Handle, -20);
                UnsafeNativeMethods.SetWindowLong(form.Handle, -20, windowLong | 0x80);
            }
            this._toolTipLeft = new ToolTip();
            this._toolTipTop = new ToolTip();
            this._toolTipRight = new ToolTip();
            this._toolTipBottom = new ToolTip();
            this._toolTipLeft.ShowAlways = true;
            this._toolTipTop.ShowAlways = true;
            this._toolTipRight.ShowAlways = true;
            this._toolTipBottom.ShowAlways = true;
            this.Color = System.Drawing.Color.FromArgb(0xff, 0x66, 0);
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in new object[] { this._bottomForm, this._leftForm, this._rightForm, this._toolTipBottom, this._toolTipLeft, this._toolTipRight, this._toolTipTop, this._topForm }.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }

        private void Layout()
        {
            SafeNativeMethods.SetWindowPos(this._leftForm.Handle, NativeMethods.HWND_TOPMOST, this._location.Left - this._width, this._location.Top, this._width, this._location.Height, 0x10);
            SafeNativeMethods.SetWindowPos(this._topForm.Handle, NativeMethods.HWND_TOPMOST, this._location.Left - this._width, this._location.Top - this._width, this._location.Width + (2 * this._width), this._width, 0x10);
            SafeNativeMethods.SetWindowPos(this._rightForm.Handle, NativeMethods.HWND_TOPMOST, this._location.Left + this._location.Width, this._location.Top, this._width, this._location.Height, 0x10);
            SafeNativeMethods.SetWindowPos(this._bottomForm.Handle, NativeMethods.HWND_TOPMOST, this._location.Left - this._width, this._location.Top + this._location.Height, this._location.Width + (2 * this._width), this._width, 0x10);
        }

        private void Show(bool show)
        {
            if (show)
            {
                SafeNativeMethods.ShowWindow(this._leftForm.Handle, 8);
                SafeNativeMethods.ShowWindow(this._topForm.Handle, 8);
                SafeNativeMethods.ShowWindow(this._rightForm.Handle, 8);
                SafeNativeMethods.ShowWindow(this._bottomForm.Handle, 8);
            }
            else
            {
                this._leftForm.Hide();
                this._topForm.Hide();
                this._rightForm.Hide();
                this._bottomForm.Hide();
            }
        }

        public System.Drawing.Color Color
        {
            set
            {
                this._color = value;
                this._leftForm.BackColor = value;
                this._topForm.BackColor = value;
                this._rightForm.BackColor = value;
                this._bottomForm.BackColor = value;
            }
        }

        public Rectangle Location
        {
            set
            {
                this._location = value;
                this.Layout();
            }
        }

        public string ToolTipText
        {
            set
            {
                this._tooltipText = value;
                this._toolTipLeft.SetToolTip(this._leftForm, this._tooltipText);
                this._toolTipTop.SetToolTip(this._topForm, this._tooltipText);
                this._toolTipRight.SetToolTip(this._rightForm, this._tooltipText);
                this._toolTipBottom.SetToolTip(this._bottomForm, this._tooltipText);
            }
        }

        public bool Visible
        {
	        get
	        {
		        return
			        this._show;
	        }
	        set
            {
                if (this._show != value)
                {
                    this._show = value;
                    if (this._show)
                    {
                        this.Layout();
                        this.Show(true);
                    }
                    else
                    {
                        this.Show(false);
                    }
                }
            }
        }

        public int Width
        {
            set
            {
                if (this._width != value)
                {
                    this._width = value;
                    this.Layout();
                }
            }
        }

        internal static class NativeMethods
        {
            internal const int DLGC_STATIC = 0x100;
            internal const int GWL_EXSTYLE = -20;
            internal static readonly IntPtr HWND_TOPMOST;
            internal const uint MOD_ALT = 1;
            internal const uint MOD_CONTROL = 2;
            internal const uint MOD_SHIFT = 4;
            internal const uint OBJ_BITMAP = 7;
            internal const int SRCCOPY = 0xcc0020;
            internal const int SW_RESTORE = 9;
            internal const int SW_SHOWNA = 8;
            internal const int SWP_NOACTIVATE = 0x10;
            internal const int TOKEN_ELEVATION = 20;
            internal const int TOKEN_ELEVATION_TYPE = 0x12;
            internal const int TOKEN_ELEVATION_TYPE_DEFAULT = 1;
            internal const int TOKEN_ELEVATION_TYPE_FULL = 2;
            internal const int TOKEN_ELEVATION_TYPE_LIMITED = 3;
            internal const int TOKEN_QUERY = 8;
            internal const int VK_F1 = 0x70;
            internal const uint VK_R = 0x52;
            internal const int VK_SHIFT = 0x10;
            internal const int WM_GETDLGCODE = 0x87;
            internal const int WM_HOTKEY = 0x312;
            internal const int WM_KEYDOWN = 0x100;
            internal const int WM_NCLBUTTONDBLCLK = 0xa3;
            internal const int WS_EX_TOOLWINDOW = 0x80;

            [StructLayout(LayoutKind.Sequential)]
            internal struct POINT
            {
                public int x;
                public int y;
            }
        }

        private static class SafeNativeMethods
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern bool BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", ExactSpelling=true)]
            private static extern bool GetPhysicalCursorPos(out HighlightRectangle.NativeMethods.POINT pt);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", ExactSpelling=true)]
            private static extern bool SetPhysicalCursorPos(int x, int y);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", ExactSpelling=true)]
            internal static extern bool SetProcessDPIAware();
            [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hwndAfter, int x, int y, int width, int height, int flags);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        }

        private static class UnsafeNativeMethods
        {
            [DllImport("user32.dll")]
            internal static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);
            [DllImport("gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern IntPtr GetCurrentObject(IntPtr hdc, uint objectType);
            [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern IntPtr GetDC(IntPtr hWnd);
            [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll", CharSet=CharSet.Auto)]
            internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
            [DllImport("ntdll.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern void NtClose(IntPtr hToken);
            [DllImport("ntdll.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern int NtOpenProcessToken(IntPtr hProcess, uint accessMask, out IntPtr hToken);
            [DllImport("ntdll.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern int NtQueryInformationToken(IntPtr hToken, uint tokenElevationType, out IntPtr elevationInfo, uint bufferSize, out uint tokensize);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", CharSet=CharSet.Auto)]
            internal static extern bool RegisterHotKey(IntPtr hWnd, int atom, uint fsModifiers, uint vk);
            [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern bool SetForegroundWindow(IntPtr hWnd);
            [DllImport("user32.dll", CharSet=CharSet.Auto)]
            internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", CharSet=CharSet.Auto)]
            internal static extern bool UnregisterHotKey(IntPtr hWnd, int atom);

            [StructLayout(LayoutKind.Sequential)]
            internal struct TOKEN_ELEVATION_INFO
            {
                [MarshalAs(UnmanagedType.U4)]
                internal uint TokenIsElevated;
            }
        }
    }
}

