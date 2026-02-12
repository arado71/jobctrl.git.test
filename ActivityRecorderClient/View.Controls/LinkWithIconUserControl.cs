using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class LinkWithIconUserControl : UserControl
	{
		public new string Text
		{
			get { return linkLabel.Text; }
			set { linkLabel.Text = value; }
		}

		private Image _image;
		private Image _disabledImage;

		public Image Image
		{
			get { return _image; }
			set
			{
				pictureBox.Image = null;
				_image?.Dispose();
				_disabledImage?.Dispose();
				_image = value;
				_disabledImage = value == null ? null : ImageTransformations.DisableImage(_image);
				pictureBox.Image = _image;
			}
		}

		public string Url { get; set; }

		public LinkWithIconUserControl()
		{
			InitializeComponent();
			linkLabel.ForeColor = MetroFramework.MetroColors.Blue;
			foreach (Control control in Controls)
			{
				control.MouseEnter += LinkWithIconUserControl_MouseEnter;
				control.MouseLeave += LinkWithIconUserControl_MouseLeave;
			}
		}

		private void LinkWithIconUserControl_MouseEnter(object sender, EventArgs e)
		{
			linkLabel.ForeColor = Color.FromArgb(128, 128, 128);
			pictureBox.Image = _disabledImage;
		}

		private void LinkWithIconUserControl_MouseLeave(object sender, EventArgs e)
		{
			linkLabel.ForeColor = MetroFramework.MetroColors.Blue;
			pictureBox.Image = _image;
		}

		private void LinkWithIconUserControl_Click(object sender, EventArgs e)
		{
			OnClick(e);
		}
	}
}
