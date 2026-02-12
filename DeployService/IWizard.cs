namespace Tct.DeployService
{
	public interface IWizard
	{
		WizardPage CurrentPage { get; }
		Session Session { get; } 
		void GoNext();
		void GoBack();
		void Go(WizardPage page);
		void Finish(bool allowBack = false);
	}
}
