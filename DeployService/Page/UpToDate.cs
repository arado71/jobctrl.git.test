namespace Tct.DeployService.Page
{
	public partial class UpToDate : WizardPage
	{
		public UpToDate(IWizard wizard) : 
			base(wizard)
		{
			InitializeComponent();
		}

		public override void Process()
		{
			Wizard.Finish(true);
		}
	}
}
