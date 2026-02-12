namespace Tct.DeployService.Page
{
	public partial class Welcome : WizardPage
	{
		public Welcome(IWizard wizard) : 
			base(wizard)
		{
			InitializeComponent();
		}

		public override void Process()
		{
			if (string.IsNullOrEmpty(Wizard.Session.InstallPath))
			{
				NextPage = new NotFound(Wizard);
				return;
			}

			if (InstallHelper.PayloadVersion() <= Wizard.Session.InstalledVersion)
			{
				NextPage = new UpToDate(Wizard);
				return;
			}

			NextPage = new UpdateReady(Wizard);
		}
	}
}
