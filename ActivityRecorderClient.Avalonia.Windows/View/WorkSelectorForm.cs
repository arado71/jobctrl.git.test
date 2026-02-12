using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.View
{
	public partial class WorkSelectorForm : Form
	{
		private ClientMenuLookup menuLookup = new ClientMenuLookup();

		public WorkDataWithParentNames SelectedWork { get; private set; }

		public WorkSelectorForm()
		{
			InitializeComponent();
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			btnCancel.Text = Labels.Cancel;
			btnOk.Text = Labels.Ok;
			lblWork.Text = Labels.Work + ":";
		}

		private void InitializeWorksCombo()
		{
			var listToBind = MenuHelper.FlattenDistinctWorkDataThatHasId(menuLookup.ClientMenu, true)
				.Where(n => ConfigManager.LocalSettingsForUser.ShowDynamicWorks || !menuLookup.IsDynamicWork(n.WorkData.Id.Value))
				.Where(n => n.WorkData.IsWorkIdFromServer)
				.Select(n => new KeyValuePair<string, int>(n.FullName + " (" + n.WorkData.Id + ")", n.WorkData.Id.Value))
				.ToList();
			cbWorks.DisplayMember = "Key";
			cbWorks.ValueMember = "Value";
			cbWorks.DataSource = listToBind;
			cbWorks.SetComboScrollWidth(n => ((KeyValuePair<string, int>)n).Key);
		}

		public void Show(IWin32Window owner, ClientMenuLookup menuLookup, string title, string description)
		{
			this.Text = title;
			lblDescription.Text = description;
			UpdateMenu(menuLookup);
			this.Show(owner);
		}

		public void UpdateMenu(ClientMenuLookup menuLookup)
		{
			this.menuLookup = menuLookup;

			var droppedW = cbWorks.DroppedDown;
			var selWork = cbWorks.SelectedValue;
			InitializeWorksCombo();
			cbWorks.SelectedValue = selWork ?? -1; //musn't use null value
			cbWorks.DroppedDown = droppedW; //if width changed then dropdown would disappear (so show it again)
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			WorkDataWithParentNames workDataP;
			if (cbWorks.SelectedValue == null
				|| !menuLookup.WorkDataById.TryGetValue((int)cbWorks.SelectedValue, out workDataP))
			{
				MessageBox.Show(this, Labels.WorkSelector_SelectWorkFirstBody, Labels.WorkSelector_SelectWorkFirstTitle);
				return;
			}

			DialogResult = DialogResult.OK;
			SelectedWork = workDataP;
			this.Close();
		}
	}
}
