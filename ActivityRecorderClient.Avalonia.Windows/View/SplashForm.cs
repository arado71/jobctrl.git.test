using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MetroFramework.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public partial class SplashForm : FixedMetroForm
	{
		public SplashForm()
		{
			InitializeComponent();
		}

		public SplashForm(string caption, string text)
		{
			InitializeComponent();

			//Text = caption;
			lblInfo.Text = text;
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (pbWork.Value == pbWork.Maximum)
				pbWork.Value = pbWork.Minimum;
			else
				pbWork.Value++;
		}

		

		private void SplashForm_Load(object sender, EventArgs e)
		{
			var s_SystemMenuHandle = WinApi.GetSystemMenu(this.Handle, false);
			WinApi.DeleteMenu(s_SystemMenuHandle, 6, 1024); //disable close button and remove CloseMenuItem from Form's Icon's menu
		}

	}
}
