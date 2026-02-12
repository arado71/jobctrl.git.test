using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Menu.Selector
{
	/// <summary>
	/// Service for displaying work selection form. Called from the GUI, only one form can be displayed at a time.
	/// </summary>
	public class WorkSelectorWinService : IWorkSelectorService
	{
		private readonly Form owner;
		private WorkSelectorForm currentForm;

		public event EventHandler<SingleValueEventArgs<WorkDataWithParentNames>> WorkSelected;

		public WorkSelectorWinService(Form owner)
		{
			this.owner = owner;
		}

		public void ShowSelectWorkGui(ClientMenuLookup menuLookup, string title, string description)
		{
			if (currentForm != null && !currentForm.IsDisposed) return; //already shown
			currentForm = new WorkSelectorForm();
			currentForm.FormClosed += currentForm_FormClosed;
			currentForm.Show(owner, menuLookup, title, description);
		}

		private void currentForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (currentForm.SelectedWork != null)
			{
				OnWorkSelected(currentForm.SelectedWork);
			}
			currentForm = null;
		}

		public void UpdateMenu(ClientMenuLookup menuLookup)
		{
			if (currentForm == null || currentForm.IsDisposed) return; //no form
			currentForm.UpdateMenu(menuLookup);
		}

		private void OnWorkSelected(WorkDataWithParentNames workData)
		{
			var del = WorkSelected;
			if (del != null) del(this, SingleValueEventArgs.Create(workData));
		}
	}
}
