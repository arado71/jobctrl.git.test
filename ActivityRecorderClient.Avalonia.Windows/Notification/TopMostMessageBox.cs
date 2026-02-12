using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Notification
{
	//idea from: http://www.codeproject.com/KB/install/TopMostMessageBox.aspx
	//and http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/f1fb9b84-a44a-4edd-93ab-c63ff3297bcc
	public static class TopMostMessageBox
	{
		public static DialogResult Show(string message)
		{
			return Show(message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		public static DialogResult Show(string message, string title)
		{
			return Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		public static DialogResult Show(string message, string title, MessageBoxButtons buttons)
		{
			return Show(message, title, buttons, MessageBoxIcon.None);
		}

		public static DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			// Create a host form that is a TopMost window which will be the 
			// parent of the MessageBox.
			// We do not want anyone to see this window so position it off the 
			// visible screen and make it as small as possible
			DialogResult result;
			using (var topmostForm = new Form())
			{
				topmostForm.Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
				topmostForm.Size = new System.Drawing.Size(1, 1);
				topmostForm.StartPosition = FormStartPosition.Manual;
				System.Drawing.Rectangle rect = SystemInformation.VirtualScreen;
				topmostForm.Location = new System.Drawing.Point(rect.Bottom + 10, rect.Right + 10);
				topmostForm.Show();
				// Make this form the active form and make it TopMost
				topmostForm.Focus();
				topmostForm.BringToFront();
				topmostForm.TopMost = true;
				// Finally show the MessageBox with the form just created as its owner
				result = MessageBox.Show(topmostForm, message, title, buttons, icon);
			}
			return result;
		}
	}
}
