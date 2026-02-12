using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.View
{
	public class Shadow : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly int shadowSize;
		protected readonly Form targetForm;
		private bool isBringingToFront;
		private Point offset;
		private readonly Brush bgBrush3 = new SolidBrush(Color.FromArgb(30, Color.Black));
		private readonly Brush bgBrush6 = new SolidBrush(Color.FromArgb(60, Color.Black));

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= WinApi.WS_EX_LAYERED | WinApi.WS_EX_TRANSPARENT | WinApi.WS_EX_NOACTIVATE;
				return cp;
			}
		}

		public Shadow(Form targetForm, int shadowSize)
		{
			offset = new Point(-shadowSize, -shadowSize);
			this.targetForm = targetForm;
			this.shadowSize = shadowSize;
			targetForm.Activated += HandleTargetFormActivated;
			targetForm.VisibleChanged += HandleTargetFormVisibleChanged;
			targetForm.Move += HandleTargetFormMoved;
			if (targetForm.Owner != null)
				Owner = targetForm.Owner;
			targetForm.Owner = this;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			ShowIcon = false;
			FormBorderStyle = FormBorderStyle.None;
			Bounds = GetShadowBounds();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				PaintShadow();
				if (targetForm.TopMost)
					NotificationWinService.SetInactiveTopMost(targetForm);
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			if (targetForm.TopMost)
				NotificationWinService.SetInactiveTopMost(targetForm);
		}

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			isBringingToFront = true;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PaintShadow();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Visible = true;
			PaintShadow();
		}

		protected override void Dispose(bool disposing)
		{
			targetForm.Owner = Owner;
			Owner = null;
			bgBrush3.Dispose();
			bgBrush6.Dispose();
			base.Dispose(disposing);
		}

		protected void ClearShadow()
		{
			using (var img = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
			{
				using (var g = Graphics.FromImage(img))
				{
					g.Clear(Color.Transparent);
					g.Flush();
				}
				SetBitmap(img, 255);
			}
		}

		protected void PaintShadow()
		{
			try
			{
				using (var getShadow = DrawBlurBorder())
					SetBitmap(getShadow, 255);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to paint shadow", ex);
			}
		}

		private void HandleTargetFormActivated(object sender, EventArgs e)
		{
			if (Visible) Update();
			if (isBringingToFront)
			{
				Visible = true;
				isBringingToFront = false;
			}
		}

		private void HandleTargetFormMoved(object sender, EventArgs e)
		{
			if (!targetForm.Visible || targetForm.WindowState != FormWindowState.Normal)
			{
				Visible = false;
			}
			else
			{
				Bounds = GetShadowBounds();
			}
		}

		private void HandleTargetFormVisibleChanged(object sender, EventArgs e)
		{
			Visible = targetForm.Visible && targetForm.WindowState != FormWindowState.Minimized;
			if (Visible) Bounds = GetShadowBounds();
			Update();
		}

		private Bitmap DrawBlurBorder()
		{
			return DrawOutsetShadow(new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height));
		}

		private Bitmap DrawOutsetShadow(Rectangle shadowCanvasArea)
		{
			if (this.IsDisposed) return null;
			Rectangle rOuter = shadowCanvasArea;
			var rInner = new Rectangle(shadowCanvasArea.X + (-offset.X - 1), shadowCanvasArea.Y + (-offset.Y - 1),
				shadowCanvasArea.Width - (-offset.X * 2 - 1), shadowCanvasArea.Height - (-offset.Y * 2 - 1));

			var img = new Bitmap(rOuter.Width, rOuter.Height, PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(img))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;

				g.FillRectangle(bgBrush3, rOuter);
				g.FillRectangle(bgBrush6, rInner);

				g.Flush();
			}

			return img;
		}

		private Rectangle GetShadowBounds()
		{
			Rectangle r = targetForm.Bounds;
			r.Inflate(shadowSize, shadowSize);
			return r;
		}

		[SecuritySafeCritical]
		private void SetBitmap(Bitmap bitmap, byte opacity)
		{
			if (bitmap == null) return;
			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
				throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

			IntPtr screenDc = WinApi.GetDC(IntPtr.Zero);
			IntPtr memDc = WinApi.CreateCompatibleDC(screenDc);
			IntPtr hBitmap = IntPtr.Zero;
			IntPtr oldBitmap = IntPtr.Zero;

			try
			{
				hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
				oldBitmap = WinApi.SelectObject(memDc, hBitmap);

				var size = new WinApi.SIZE(bitmap.Width, bitmap.Height);
				var pointSource = new WinApi.POINT(0, 0);
				var topPos = new WinApi.POINT(Left, Top);
				var blend = new WinApi.BLENDFUNCTION
				{
					BlendOp = WinApi.AC_SRC_OVER,
					BlendFlags = 0,
					SourceConstantAlpha = opacity,
					AlphaFormat = WinApi.AC_SRC_ALPHA
				};

				WinApi.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, WinApi.ULW_ALPHA);
			}
			finally
			{
				if (oldBitmap != IntPtr.Zero && memDc != IntPtr.Zero) WinApi.SelectObject(memDc, oldBitmap);
				if (hBitmap != IntPtr.Zero) WinApi.DeleteObject(hBitmap);
				if (memDc != IntPtr.Zero) WinApi.DeleteDC(memDc);
				if (screenDc != IntPtr.Zero) WinApi.ReleaseDC(IntPtr.Zero, screenDc);
			}
		}
	}
}