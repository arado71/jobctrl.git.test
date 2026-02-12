using System.ComponentModel;
using System.Windows.Forms;

namespace Tct.DeployService
{
	public partial class WizardPage : UserControl
	{
		protected IWizard Wizard { get; private set; }
		private WizardPage nextPage = null;

		public event PropertyChangedEventHandler PropertyChanged;

		public WizardPage()
		{
			InitializeComponent();
		}

		public WizardPage(IWizard wizard)
		{
			InitializeComponent();
			Wizard = wizard;
		}

		public WizardPage NextPage {
			get
			{
				return nextPage;
			}

			protected set
			{
				if (nextPage == value) return;
				nextPage = value;
				var evt = PropertyChanged;
				if (evt != null ) evt(this, new PropertyChangedEventArgs("NextPage"));
			}
		}

		public virtual void Process()
		{
		}

		public virtual void OnNext()
		{
		}
	}
}
