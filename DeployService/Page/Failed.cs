using System;

namespace Tct.DeployService.Page
{
	public partial class Failed : WizardPage
	{
		public Failed(IWizard wizard, string reason) : 
			base(wizard)
		{
			InitializeComponent();
			label2.Text += Environment.NewLine + reason;
		}

		public override void Process()
		{
			Wizard.Finish();
		}
	}
}
