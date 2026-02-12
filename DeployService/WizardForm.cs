using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using Tct.DeployService.Page;

namespace Tct.DeployService
{
	public partial class WizardForm : Form, IWizard
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(WizardForm));
		private readonly TaskFactory taskFactory = new TaskFactory();
		protected Session session = new Session(); 
		protected WizardPage currentPage = null;
		protected Stack<WizardPage> history = new Stack<WizardPage>();

		public Session Session
		{
			get { return session; }
		}

		public WizardForm()
		{
			InitializeComponent();
			btnNext.Text = Resource.ButtonNext;
			btnBack.Text = Resource.ButtonBack;
			btnFinish.Text = Resource.ButtonFinish;
			Go(new Welcome(this));
		}

		public void GoNext()
		{
			if (InvokeRequired)
			{
				Invoke(new Action(GoNext));
				return;
			}

			Go(CurrentPage.NextPage);
		}

		public void GoBack()
		{
			if (InvokeRequired)
			{
				Invoke(new Action(GoBack));
				return;
			}

			CurrentPage = history.Pop();
			btnBack.Enabled = history.Count != 0;
			btnNext.Visible = true;
			btnFinish.Visible = false;
		}

		public void Go(WizardPage page)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => Go(page)));
				return;
			}

			if (CurrentPage != null)
			{
				history.Push(CurrentPage);
				CurrentPage.OnNext();
			}

			btnBack.Enabled = history.Count != 0;
			CurrentPage = page;
			btnNext.Visible = true;
			btnFinish.Visible = false;
		}

		public void Finish(bool allowBack = false)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => Finish(allowBack)));
				return;
			}

			Debug.Assert(currentPage != null);
			btnBack.Visible = allowBack;
			btnNext.Visible = false;
			btnFinish.Visible = true;
		}

		public WizardPage CurrentPage
		{
			get
			{
				return currentPage;
			}

			protected set
			{
				if (currentPage != null)
				{
					currentPage.PropertyChanged -= HandlePagePropertyChanged;
					pContent.Controls.RemoveAt(0);
				}

				var child = value;
				pContent.Controls.Add(child);
				child.Dock = DockStyle.Fill;
				currentPage = value;
				btnNext.Enabled = currentPage.NextPage != null;
				currentPage.PropertyChanged += HandlePagePropertyChanged;
				taskFactory.StartNew(() => 
				{
					try
					{
						currentPage.Process();
					}
					catch (Exception e)
					{
						logger.Fatal(e.Message);
						logger.Fatal(e.StackTrace);
						Go(new Failed(this, e.Message));
					}
				});
			}
		}

		private void HandlePagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => HandlePagePropertyChanged(sender, e)));
				return;
			}

			switch (e.PropertyName)
			{
				case "NextPage":
					btnNext.Enabled = currentPage.NextPage != null;
					break;
			}
		}

		private void HandleNextClicked(object sender, EventArgs e)
		{
			GoNext();
		}

		private void HandleBackClicked(object sender, EventArgs e)
		{
			GoBack();
		}

		private void HandleFinishClicked(object sender, EventArgs e)
		{
			Close();
		}
	}
}
