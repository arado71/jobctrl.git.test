using System;
using System.IO;
using System.Windows.Forms;

namespace Tct.DeployService.Page
{
	public partial class NotFound : WizardPage
	{
		public NotFound(IWizard wizard) : 
			base(wizard)
		{
			InitializeComponent();
		}

		public override void OnNext()
		{
			Wizard.Session.InstallPath = txtPath.Text;
		}

		private void PathChanged()
		{
			if (Directory.Exists(txtPath.Text) && InstallHelper.IsInstallPath(txtPath.Text))
			{
				NextPage = new UpdateReady(Wizard);
			}
			else
			{
				if (NextPage != null)
				{
					NextPage.Dispose();
					NextPage = null;
				}
			}
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			if (folderBrowser.ShowDialog(this) == DialogResult.OK)
			{
				txtPath.Text = folderBrowser.SelectedPath;
				PathChanged();
			}
		}

		private void HandlePathChanged(object sender, EventArgs e)
		{
			PathChanged();
		}
	}
}
