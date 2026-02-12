using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using MetroFramework;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Telemetry;

namespace Tct.ActivityRecorderClient.View
{
	public partial class NotificationForm : Form, ILocalizableControl
	{
		private const int MinHeight = 2;
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Image backgroundLogo = SetImageOpacity(Resources.logo, 0.35f, 40, 40);

		private readonly FadeAnimation animation = new FadeAnimation();
		private bool isFadingOutStarted;
		private int maxHeight, maxWidth;
		private bool notificationClickedFired;
		private NotificationPosition position;
		private bool isScrollbarVisible;

		public event EventHandler NotificationClicked;

		public TimeSpan FadeInDuration
		{
			get
			{
				return animation.FadeInDuration;
			}

			set
			{
				animation.FadeInDuration = value;
			}
		}

		public TimeSpan FadeOutDuration
		{
			get
			{
				return animation.FadeOutDuration;
			}

			set
			{
				animation.FadeOutDuration = value;
			}
		}


		public TimeSpan ShowDuration { get; set; }

		public Color Color
		{
			get { return pColor.BackColor; }

			set { pColor.BackColor = value; }
		}

		public NotificationPosition Position
		{
			get { return position; }
			set
			{
				if (IsDisposed) return;
				if (position == value) return;
				if (value == NotificationPosition.Hidden)
				{
					Close();
				}
				else
				{
					position = value;
					AnimateWindow(Width, Height);
				}
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams ret = base.CreateParams;
				ret.ExStyle |= WinApi.WS_EX_NOACTIVATE;
				return ret;
			}
		}

		//don't steal focus (this might not needed but i'll leave it here)
		//http://stackoverflow.com/questions/156046/show-a-form-without-stealing-focus-in-c
		protected override bool ShowWithoutActivation
		{
			get { return true; }
		}

		public NotificationForm()
		{
			//todo when the window is not responding the icon appears for a moment even if ShowInTaskbar = false
			ShowDuration = TimeSpan.FromMilliseconds(10000);
			position = NotificationPosition.BottomRight;
			InitializeComponent();
			SetColorScheme();
			(this.components ?? (this.components = new Container())).Add(new ComponentWrapper(animation));
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			maxHeight = Size.Height;
			maxWidth = Size.Width;
			lblTitle.Font = MetroFonts.DefaultLight(18);
			lblMessage.Font = MetroFonts.Default(12);
			lnkMore.Font = MetroFonts.Default(12);
			lnkMore.Visible = false;
			animation.Animate += AnimationCallback;
			animation.FadeOutComplete += CloseTrigger;
			Localize();
		}

		public void Localize()
		{
			lnkMore.Text = Labels.NotificationMore + @"...";
		}

		private void SetColorScheme()
		{
			if (SystemInformation.HighContrast)
			{
				lnkMore.LinkColor = SystemColors.Highlight;
				pnlMsg.BackColor = SystemColors.Window;
				lblMessage.ActiveLinkColor = SystemColors.WindowText;
				lblMessage.LinkColor = SystemColors.WindowText;
				lblMessage.VisitedLinkColor = SystemColors.WindowText;
			}
			else
			{
				lnkMore.LinkColor = MetroColors.Black;
				pnlMsg.BackColor = Color.White;
				lblMessage.ActiveLinkColor = System.Drawing.Color.Black;
				lblMessage.LinkColor = System.Drawing.Color.Black;
				lblMessage.VisitedLinkColor = System.Drawing.Color.Black;
			}
		}
		/// <summary>
		///     method for changing the opacity of an image
		/// </summary>
		/// <param name="image">image to set opacity on</param>
		/// <param name="opacity">percentage of opacity</param>
		/// <param name="width">Width of result image</param>
		/// <param name="height">Height of result image</param>
		/// <returns></returns>
		public static Image SetImageOpacity(Image image, float opacity, int width, int height)
		{
			try
			{
				var bmp = new Bitmap(width, height);
				using (Graphics gfx = Graphics.FromImage(bmp))
				{
					var matrix = new ColorMatrix { Matrix33 = opacity };
					using (var attributes = new ImageAttributes())
					{
						attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
						gfx.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel,
							attributes);
					}
				}
				return bmp;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return null;
			}
		}

		public void Show(string title, MessageWithActions message)
		{
			if (Position == NotificationPosition.Hidden) return;
			lblTitle.Text = title;
			int msgHeight = lblMessage.Height;
			lblMessage.Text = message.GetText();
			lblMessage.Links.Clear();
			// GDI+ limitation, only 31 links are allowed: https://social.msdn.microsoft.com/Forums/windows/en-us/26c963ca-ac7e-4d6b-acf6-ff8ce3aa9b55/cant-have-more-then-32-links-in-linklabel
			foreach (var link in message.Links.Take(31)) 
			{
				lblMessage.Links.Add(link.StartPosition, link.EndPosition - link.StartPosition, link.Action).Description = link.Description;
			}

			if (lblMessage.GetPreferredSize(new Size(lblMessage.Width, lblMessage.Height + 500)).Height > lblMessage.Height || lblTitle.PreferredWidth > lblTitle.Width)
			{
				pnlInner.Height = msgHeight - lnkMore.Height;
				lnkMore.Visible = true;
			}

			if (ShowDuration.TotalMilliseconds > 0)
			{
				timerClose.Interval = (int)ShowDuration.TotalMilliseconds;
				timerClose.Start();
			}

			if (FadeAnimation.IsEnabled())
			{
				InitializeWindow(Width, maxHeight);
				AnimateWindow(Width, 0);
				Show();
				animation.FadeInDuration = FadeInDuration;
				animation.FadeIn();
			}
			else
			{
				InitializeWindow(Width, maxHeight);
				Show();
				AnimateWindow(Width, maxHeight);
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WinApi.WM_SETTINGCHANGE && m.WParam.ToInt32() == WinApi.SPI_WORKAREA)
			{
				AnimateWindow(Width, Height);
			}

			base.WndProc(ref m);
		}

		protected override void OnLoad(EventArgs e)
		{
			ClientSize = new Size(ClientSize.Width, 0);
			base.OnLoad(e);
		}

		private void AnimationCallback(double progress)
		{
			var height = (int)(maxHeight * progress);
			AnimateWindow(Width, height);
		}

		private void CloseTrigger(object sender, EventArgs e)
		{
			Close();
		}

		private void HandleClicked(object sender, EventArgs e)
		{
			if (notificationClickedFired) return; //can fire only once
			notificationClickedFired = true;
			TelemetryHelper.RecordFeature("Notification", "Click");
			RaiseNotificationClicked();
			Close();
		}

		private void HandleClosing(object sender, FormClosingEventArgs e)
		{
			timerClose.Stop();
			if (e.CloseReason == CloseReason.UserClosing && !animation.IsDisposed)
			{
				if (!isFadingOutStarted && FadeAnimation.IsEnabled())
				{
					isFadingOutStarted = true;
					animation.FadeOutDuration = FadeOutDuration;
					animation.FadeOut();
					e.Cancel = true;
					return;
				}

				if (isFadingOutStarted && animation.Playing)
				{
					e.Cancel = true;
					return;
				}
			}
			animation.Dispose();
		}

		private void HandleMoreClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			timerClose.Stop();
			TelemetryHelper.RecordFeature("Notification", "More");
			pnlInner.MinimumSize = new Size(pnlInner.Width, pnlInner.Height + lnkMore.Height);
			var heightDelta = Math.Max(lblMessage.PreferredHeight - lblMessage.Height, 0);
			var widthDelta = Math.Max(Math.Max(lblTitle.PreferredWidth - lblTitle.Width, lblMessage.PreferredWidth - lblMessage.Width), 0);
			lnkMore.Visible = false;
			maxWidth += widthDelta;
			if (maxHeight + heightDelta > 400)
			{
				pnlInner.AutoScroll = true;
				isScrollbarVisible = true;
				maxHeight = 400;
			}
			else
				maxHeight += heightDelta;
			lblMessage.MinimumSize = lblMessage.PreferredSize;
			AnimateWindow(maxWidth, maxHeight);
		}

		private void HandlePanelSizeChanged(object sender, EventArgs e)
		{
			if (isScrollbarVisible)
			{
				lblMessage.MinimumSize = lblMessage.GetPreferredSize(new Size(lblMessage.Width, lblMessage.Height + 500));
			}
		}

		private void HandlePainting(object sender, PaintEventArgs e)
		{
			if (!isScrollbarVisible			// when scrollbar is visible the watermark shown partly
				&& ConfigManager.IsWindows7) //On Windows XP this icon looks ugly (and might cause AccessViolationException)
			{
				e.Graphics.DrawImage(backgroundLogo, maxWidth - 60, maxHeight - 50);
			}
		}

		private void HandleTicked(object sender, EventArgs e)
		{
			if (!isFadingOutStarted && RectangleToScreen(ClientRectangle).Contains(Cursor.Position))
			{
				timerClose.Interval = 100;
				return;
			}

			timerClose.Stop();
			Close();
		}

		private void AnimateWindow(int width, int height)
		{
			Point topLeft = GetBasePosition(width, height);
			if (height < MinHeight)
			{
				topLeft.Y -= MinHeight - height;
				height = MinHeight;
			}

			if (Position == NotificationPosition.BottomLeft || Position == NotificationPosition.BottomRight)
			{
				AnimateWindowResize(topLeft, new Size(width, height));
			}
			else
			{
				AnimateWindowRegion(topLeft, new Size(width, height));
			}
		}

		private void AnimateWindowRegion(Point topLeft, Size size)
		{
			NativeMethods.SetWindowPos(Handle, (IntPtr)NativeMethods.SpecialWindowHandles.HWND_TOPMOST,
				topLeft.X, topLeft.Y, maxWidth, maxHeight,
				NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE | NativeMethods.SetWindowPosFlags.SWP_NOZORDER);
			Region = new Region(new Rectangle(0, 0, size.Width, size.Height));
		}

		private void AnimateWindowResize(Point topLeft, Size size)
		{
			NativeMethods.SetWindowPos(Handle, (IntPtr)NativeMethods.SpecialWindowHandles.HWND_TOPMOST,
				topLeft.X, topLeft.Y, size.Width, size.Height,
				NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE | NativeMethods.SetWindowPosFlags.SWP_NOZORDER);
			Region = null;
		}

		private Point GetBasePosition(int width, int height)
		{
			Rectangle wa = Screen.PrimaryScreen.WorkingArea;
			switch (position)
			{
				case NotificationPosition.TopLeft:
					return new Point(0, 0);
				case NotificationPosition.TopRight:
					return new Point(Math.Max(wa.Width - width, 0), 0);
				case NotificationPosition.Center:
					return new Point(Math.Max((wa.Width - width) / 2, 0), Math.Max((wa.Height - height) / 2, 0));
				case NotificationPosition.BottomLeft:
					return new Point(0, Math.Max(wa.Height - height, 0));
				case NotificationPosition.BottomRight:
					return new Point(Math.Max(wa.Width - width, 0), Math.Max(wa.Height - height, 0));
				default:
					log.Warn("Invalid position " + position);
					return new Point(0, 0);
			}
		}

		private void InitializeWindow(int width, int height)
		{
			maxWidth = width;
			maxHeight = height;
			Point topLeft = GetBasePosition(width, height);
			if (height < MinHeight)
			{
				topLeft.Y -= MinHeight - height;
			}

			NativeMethods.SetWindowPos(Handle, (IntPtr)NativeMethods.SpecialWindowHandles.HWND_TOPMOST,
				topLeft.X, topLeft.Y, maxWidth, maxHeight, NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE);
		}

		private void RaiseNotificationClicked()
		{
			EventHandler evt = NotificationClicked;
			if (evt != null) evt(this, EventArgs.Empty);
		}

		private void HandleCloseClicked(object sender, EventArgs e)
		{
			TelemetryHelper.RecordFeature("Notification", "Close");
			CloseFast();
		}

		public void CloseFast()
		{
			isFadingOutStarted = true;
			animation.Stop();
			Close();
		}

		#region Avoid exception from System.Windows.Forms.Form.UpdateLayered() (at least try...)

		private double? opacityOverride; //ensure we can change Opacity even if it throws
		public new double Opacity
		{
			get { return opacityOverride ?? base.Opacity; }
			set
			{
				try
				{
					base.Opacity = value;
					opacityOverride = null;
				}
				catch (Win32Exception ex)
				{
					opacityOverride = value;
					log.Warn("Error setting Opacity to " + value, ex);
				}
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			try
			{
				base.OnHandleCreated(e);
			}
			catch (Win32Exception ex)
			{
				log.Warn("Error creating handle", ex);
			}
		}

		public new Color TransparencyKey
		{
			get { return base.TransparencyKey; }
			set
			{
				try
				{
					base.TransparencyKey = value;
				}
				catch (Win32Exception ex)
				{
					log.Warn("Error setting TransparencyKey to " + value, ex);
				}
			}
		}

		public new bool AllowTransparency
		{
			get { return base.AllowTransparency; }
			set
			{
				try
				{
					base.AllowTransparency = value;
				}
				catch (Win32Exception ex)
				{
					log.Warn("Error setting AllowTransparency to " + value, ex);
				}
			}
		}

		#endregion

		private void HandleMessageLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			((Action)e.Link.LinkData)();
		}
	}
}