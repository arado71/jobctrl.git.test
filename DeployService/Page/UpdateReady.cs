using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.DeployService.Page
{
	public partial class UpdateReady : WizardPage
	{
		public UpdateReady(IWizard wizard) :
			base(wizard)
		{
			InitializeComponent();
			NextPage = new Updating(Wizard);
		}
	}
}
