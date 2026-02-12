using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public partial class MetroMessageBox : FixedMetroForm
	{
		public MetroMessageBox(string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OKCancel)
		{
			InitializeComponent();
			Text = title;
			lblMessage.Text = message;
			btnOk.Text = Labels.Ok;
			btnCancel.Text = Labels.Cancel;
			DialogResult = DialogResult.None;
			// hax
			if (buttons == MessageBoxButtons.OK)
			{
				btnCancel.Enabled = false;
				btnCancel.Visible = false;
				btnOk.Location = btnCancel.Location;
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		public static void Show(string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OKCancel)
		{
			var msgBox = new MetroMessageBox(title, message, buttons);
			msgBox.Show();
		}

		public static void Show(IWin32Window window, string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OKCancel)
		{
			var msgBox = new MetroMessageBox(title, message, buttons);
			msgBox.Show(window);
		}
	}
}
