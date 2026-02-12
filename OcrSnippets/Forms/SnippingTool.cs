// Original from: http://stackoverflow.com/a/3124252/122195
// Modified version multiple monitors aware and text scaling aware

using System;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TcT.ActivityRecorderClient.SnippingTool;

namespace TcT.OcrSnippets
{
    public sealed partial class SnippingTool : Form
    {
        public static event EventHandler Cancel;
        public static event EventHandler<SelectedEventArgs> AreaSelected;
        private static SnippingTool[] forms;
        private Rectangle rectSelection;
        private Point pointStart;
        private readonly int actScreen;

        private SnippingTool(Image screenShot, int x, int y, int width, int height, int monitor)
        {
            InitializeComponent();
            BackgroundImage = screenShot;
            BackgroundImageLayout = ImageLayout.Stretch;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            SetBounds(x, y, width, height);
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            Cursor = Cursors.Cross;
            TopMost = true;
            actScreen = monitor;
        }
        private void OnCancel(EventArgs e)
        {
            Cancel?.Invoke(this, e);
        }
        private void OnAreaSelected(SelectedEventArgs e)
        {
            AreaSelected?.Invoke(this, e);
        }
        private void CloseForms()
        {
            foreach (var t in forms)
                t.Close();
        }

        public static void Snip()
        {
            var screens = ScreenHelper.GetMonitorsInfo();
            forms = new SnippingTool[screens.Count];
            for (int i = 0; i < screens.Count; i++)
            {
                int hRes = screens[i].HorizontalResolution;
                int vRes = screens[i].VerticalResolution;
                int top = screens[i].MonitorArea.Top;
                int left = screens[i].MonitorArea.Left;
                var bmp = new Bitmap(hRes, vRes, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(bmp))
                    g.CopyFromScreen(left, top, 0, 0, bmp.Size);
                forms[i] = new SnippingTool(bmp, left, top, hRes, vRes, i);
                forms[i].Show();
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Start the snip on mouse down
            if (e.Button != MouseButtons.Left)
                return;
            pointStart = e.Location;
            rectSelection = new Rectangle(e.Location, new Size(0, 0));
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            int x1 = Math.Min(e.X, pointStart.X);
            int y1 = Math.Min(e.Y, pointStart.Y);
            int x2 = Math.Max(e.X, pointStart.X);
            int y2 = Math.Max(e.Y, pointStart.Y);
            rectSelection = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (rectSelection.Width <= 0 || rectSelection.Height <= 0)
            {
                CloseForms();
                OnCancel(EventArgs.Empty);
                return;
            }
            var sea =
                new SelectedEventArgs
                {
                    Rectangle = rectSelection,
                    Monitor = EnumWindowsHelper.Monitors[actScreen]
                };
            CloseForms();
            OnAreaSelected(sea);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            using (Brush br = new SolidBrush(Color.FromArgb(120, Color.White)))
            {
                int x1 = rectSelection.X;
                int x2 = rectSelection.X + rectSelection.Width;
                int y1 = rectSelection.Y;
                int y2 = rectSelection.Y + rectSelection.Height;
                e.Graphics.FillRectangle(br, new Rectangle(0, 0, x1, Height));
                e.Graphics.FillRectangle(br, new Rectangle(x2, 0, Width - x2, Height));
                e.Graphics.FillRectangle(br, new Rectangle(x1, 0, x2 - x1, y1));
                e.Graphics.FillRectangle(br, new Rectangle(x1, y2, x2 - x1, Height - y2));
            }
            using (Pen pen = new Pen(Color.Red, 2))
                e.Graphics.DrawRectangle(pen, rectSelection);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                CloseForms();
                OnCancel(new EventArgs());
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}