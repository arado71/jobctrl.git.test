using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public partial class GoogleAuthenticationForm : FixedMetroForm, ILocalizableControl
	{
		private static readonly ILog
			log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public GoogleAuthenticationForm()
		{
			InitializeComponent();
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
		}

		public void Localize()
		{
			firstRowMmetroLabel.Text = Labels.GoogleAuthenticationForm_FirstRow;
			secondRowMetroLabel.Text = Labels.GoogleAuthenticationForm_SecondRow;
			privacyMetroLink.Text = Labels.GoogleAuthenticationForm_PrivacyPolicy;
			cancelButton.Text = Labels.Cancel;
		}

		private void privacyMetroLink_Click(object sender, EventArgs e)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				string url = "";
				try
				{
					url = string.Format(ConfigManager.WebsiteUrl + "Media/Docs/JobCTRL_DataProtectionPolicy.pdf");
					System.Diagnostics.Process.Start(url);
				}
				catch (Exception ex)
				{
					log.Error("Unable to open url: " + url, ex);
				}
			});
		}

		private void GoogleAuthenticationForm_Load(object sender, EventArgs e)
		{
			SignInButton.Image = null;
			Localize();
		}

		private void SignInButton_MouseEnter(object sender, EventArgs e)
		{
			SignInButton.ImageIndex = 1;
		}

		private void SignInButton_MouseLeave(object sender, EventArgs e)
		{
			SignInButton.ImageIndex = 0;
		}

		private void SignInButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void SignInButton_MouseDown(object sender, MouseEventArgs e)
		{
			SignInButton.ImageIndex = 2;
		}

		private void SignInButton_MouseUp(object sender, MouseEventArgs e)
		{
			SignInButton.ImageIndex = 0;
		}
	}
}
