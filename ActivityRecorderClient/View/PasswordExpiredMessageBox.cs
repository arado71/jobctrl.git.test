using log4net;
using System;
using System.Drawing;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View
{
	public partial class PasswordExpiredMessageBox : FixedMetroForm, ILocalizableControl
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PASSWORD_EXPIRED_LINK = "/Account/ForgotYourPassword.aspx";

		private static PasswordExpiredMessageBox _instance;
		public static PasswordExpiredMessageBox Instance { get { return _instance ?? (_instance = new PasswordExpiredMessageBox()); } }

		private PasswordExpiredMessageBox()
		{
			InitializeComponent();
			Text = Labels.Login_PasswordExpiredTitle;
			linkLabel.Text = Labels.Login_PasswordExpiredClick;
			btnOk.Text = Labels.Ok;
			linkLabel.ForeColor = MetroFramework.MetroColors.Blue;
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
		}

		public void Localize()
		{
			Text = Labels.Login_PasswordExpiredTitle;
			linkLabel.Text = Labels.Login_PasswordExpiredClick;
			btnOk.Text = Labels.Ok;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void linkLabel_Click(object sender, EventArgs e)
		{
			try
			{
				TopMost = false;
				if (ConfigManager.IsAuthDataRequired)
				{
					var url = ConfigManager.WebsiteUrl + "Account/Login.aspx?url=" + Uri.EscapeDataString(PASSWORD_EXPIRED_LINK);
					System.Diagnostics.Process.Start(url);
				}
				else
				{
					RecentUrlQuery.Instance.OpenUrl(PASSWORD_EXPIRED_LINK);
				}
			} catch(Exception ex)
			{
				log.Error("Couldn't open link to change password.", ex);
				MessageBox.Show("Couldn't open link to change password. Please change from the browser.", "Error", MessageBoxButtons.OK);
			}
		}

		private void linkLabel_MouseEnter(object sender, EventArgs e)
		{
			linkLabel.ForeColor = Color.FromArgb(128, 128, 128);
		}

		private void linkLabel_MouseLeave(object sender, EventArgs e)
		{
			linkLabel.ForeColor = MetroFramework.MetroColors.Blue;
		}

		public DialogResult ShowDialog(bool topmost)
		{
			TopMost = topmost;
			var result = ShowDialog();
			TopMost = false;
			return result;
		}
	}
}
