using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public partial class SmartToolStrip : ToolStripDropDown
	{
		private bool fade = true;
		private ToolStripControlHost host;
		private Control content;
		private const int frames = 5;
		private const int totalduration = 100;
		private const int frameduration = totalduration / frames;

		public SmartToolStrip(Control content)
		{
			InitializeComponent();
			if (content == null)
				throw new ArgumentException("content");
			this.content = content;
			fade = SystemInformation.IsMenuAnimationEnabled && SystemInformation.IsMenuFadeEnabled;
			host = new ToolStripControlHost(content);
			host.AutoSize = false;
			Padding = Margin = host.Padding = host.Margin = Padding.Empty;
			content.Location = Point.Empty;
			Items.Add(host);
			content.Disposed += delegate(object sender, EventArgs e)
			{
				content = null;
				Dispose(true);
			};
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if ((keyData & Keys.Alt) == Keys.Alt)
				return false;

			return base.ProcessDialogKey(keyData);
		}

		public void Show(Control control)
		{
			if(control == null)
				throw new ArgumentNullException("content");

			Show(control, control.ClientRectangle);
		}

		public void Show(Form f, Point p)
		{
			Show(f, new Rectangle(p, new Size(0, 0)));
		}


		private void Show(Control control, Rectangle area)
		{
			if (control == null)
				throw new ArgumentNullException("content");


			Point location = control.PointToScreen(new Point(area.Left, area.Top + area.Height));

			Rectangle screen = Screen.FromControl(control).WorkingArea;

			if (location.X + Size.Width > (screen.Left + screen.Width))
				location.X = (screen.Left + screen.Width) - Size.Width;

			if (location.Y + Size.Height > (screen.Top + screen.Height))
				location.Y -= Size.Height + area.Height;

			location = control.PointToClient(location);

			Show(control, location, ToolStripDropDownDirection.BelowRight);
		}

		protected override void SetVisibleCore(bool visible)
		{
			double opacity = Opacity;
			if (visible && fade) Opacity = 0;
			base.SetVisibleCore(visible);
			if (!visible || !fade) return;
			for (int i = 1; i <= frames; i++)
			{
				if (i > 1)
				{
					System.Threading.Thread.Sleep(frameduration);
				}
				Opacity = opacity * (double)i / (double)frames;
			}
			Opacity = opacity;
		}

		protected override void OnOpening(CancelEventArgs e)
		{
			if (content.IsDisposed || content.Disposing)
			{
				e.Cancel = true;
				return;
			}
			base.OnOpening(e);
		}

		protected override void OnOpened(EventArgs e)
		{
			content.Focus();

			base.OnOpened(e);
		}
	}
}
