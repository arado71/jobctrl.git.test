using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Capturing.UserActivity;

namespace MouseKeyboardActivityDetector
{
	public partial class MouseKeyboardActivityForm : Form
	{
		private UserActivityHook hook = new UserActivityHook();
		private int mouseActivity = 0;
		private int keyboardActivity = 0;
		private int injectedMouseActivity = 0;
		private int injectedKeyboardActivity = 0;
		public MouseKeyboardActivityForm()
		{
			InitializeComponent();
			hook.StartKeyboardHook();
			hook.StartMouseHook();
			hook.KeyUp += (sender, e, injected) =>
			{
				Invoke(new Action(() =>
				{
					if (injected) injectedKeyboardActivityLabel.Text = (++injectedKeyboardActivity).ToString();
					else keyboardActivityLabel.Text = (++keyboardActivity).ToString();
				}));
			};
			hook.OnMouseActivity += (sender, e, injected) =>
			{
				Invoke(new Action(() =>
				{
					if (injected) injectedMouseActivityLabel.Text = (++injectedMouseActivity).ToString();
					else mouseActivityLabel.Text = (++mouseActivity).ToString();
				}));
			};
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			hook.StopKeyboardHook(false);
			hook.StopMouseHook(false);
		}
	}
}
