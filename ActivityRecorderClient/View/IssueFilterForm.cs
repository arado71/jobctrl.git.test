using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.View
{
	public partial class IssueFilterForm : FixedMetroForm
	{
		public class IssueBindingItem
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public string Company { get; set; }
			public string State { get; set; }
			public string User { get; set; }
			public string Modified { get; set; }
		}

		[Serializable]
		public class IssueFilterFormState
		{
			public System.Drawing.Point Location { get; set; }
			public System.Drawing.Size Size { get; set; }
			public bool Maximised { get; set; }
			public bool Minimised { get; set; }
			public string OwnerFilter { get; set; }
			public string StateFilter { get; set; }
			public string TextFilter { get; set; }

			public Dictionary<string, SortOrder> SortingDirections { get; set; }
		}

		private readonly Dictionary<IssueState, IssueStateItem> statesDict = new Dictionary<IssueState, IssueStateItem>();
		private SortableBindingList<IssueData> issueBindings = new SortableBindingList<IssueData>();
		private IssueManager parent;
		private static string IssueFilterFormStateFile { get { return "IssueFilterFormState-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<string>> IssueActionFired;

		public event EventHandler<SingleValueEventArgs<string>> IssueSelectionChanged;

		public IssueFilterForm()
		{
			InitializeComponent();
			dgvIssues.AutoGenerateColumns = false;
			Localize();
			cbState.Items.Clear();
			cbState.Items.Add(new IssueStateItem { Caption = Labels.IssueMgr_All });
			// ReSharper disable once CoVariantArrayConversion
			cbState.Items.AddRange(statesDict.Values.ToArray());
			cbState.SelectedIndex = 0;
			cbOwner.Items.Clear();
			cbOwner.Items.AddRange(new object[] { Labels.IssueMgr_All, Labels.IssueMgr_CreatedByMe, Labels.IssueMgr_ModifiedByMe });
			cbOwner.SelectedIndex = 0;
			cbState.SelectedIndexChanged += cbState_SelectedIndexChanged;
			cbOwner.SelectedIndexChanged += cbOwner_SelectedIndexChanged;
		}

		public IssueFilterForm(IssueManager parent)
			: this()
		{
			this.parent = parent;
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		private void Localize()
		{
			Text = Labels.IssueMgr_Issues;
			lblSearchText.Text = Labels.IssueMgr_SearchText + ":";
			lblState.Text = Labels.IssueMgr_State + ":";
			lblOwner.Text = Labels.IssueMgr_Owner + ":";
			dgvIssues.Columns[ColIssueName.Name].HeaderText = Labels.IssueMgr_Name;
			dgvIssues.Columns[ColIssueCompany.Name].HeaderText = Labels.IssueMgr_Company;
			dgvIssues.Columns[ColIssueState.Name].HeaderText = Labels.IssueMgr_State;
			dgvIssues.Columns[ColUser.Name].HeaderText = Labels.IssueMgr_Owner;
			dgvIssues.Columns[ColModified.Name].HeaderText = Labels.IssueMgr_Modified;
			dgvIssues.Columns[ColCreatedBy.Name].HeaderText = Labels.IssueMgr_CreatedBy;
			dgvIssues.Columns[ColModifiedBy.Name].HeaderText = Labels.IssueMgr_ModifiedBy;
			foreach (IssueState value in Enum.GetValues(typeof(IssueState)))
			{
				var stateItem = new IssueStateItem
				{
					Caption = Labels.ResourceManager.GetString("IssueMgr_State" + value),
					State = value,
				};
				statesDict.Add(value, stateItem);
			}
		}

		private void IssueFilterFormLoad(object sender, EventArgs e)
		{
			dgvIssues.DataSource = issueBindings;
			// ReSharper disable once AssignNullToNotNullAttribute
			dgvIssues.Sort(dgvIssues.Columns[ColModified.Name], ListSortDirection.Ascending);
			cbSearchText.Select();
			queryDelay.Start();
			RestoreState();
		}

		private void RestoreState()
		{
			IssueFilterFormState formState = new IssueFilterFormState();
			if (IsolatedStorageSerializationHelper.Exists(IssueFilterFormStateFile) &&
				IsolatedStorageSerializationHelper.Load(IssueFilterFormStateFile, out formState))
			{
				if (formState.Maximised)
				{
					WindowState = FormWindowState.Maximized;
					Location = formState.Location;
					Size = formState.Size;
				}
				else if (formState.Minimised)
				{
					WindowState = FormWindowState.Minimized;
					Location = formState.Location;
					Size = formState.Size;
				}
				else
				{
					Location = formState.Location;
					Size = formState.Size;
				}
				var columnOrders = formState.SortingDirections;

				if (columnOrders != null)
				{
					foreach (var columnOrder in columnOrders)
					{
						dgvIssues.Sort(dgvIssues.Columns[columnOrder.Key],
							columnOrder.Value == SortOrder.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending);
					}
				}

				cbOwner.Text = formState.OwnerFilter;
				cbState.Text = formState.StateFilter;
				cbSearchText.Text = formState.TextFilter;
				ApplyFilter();
			}
		}

		public List<IssueData> Issues
		{
			get { return issueBindings.ToList(); }
			set
			{
				var newBindings = new SortableBindingList<IssueData>(value.DeepClone());
				issueBindings = newBindings;
				ApplyFilter(false);
			}
		}

		private void ApplyFilter(bool first = true)
		{
			IEnumerable<IssueData> list = Issues;
			if (!string.IsNullOrEmpty(cbSearchText.Text))
			{
				var srText = cbSearchText.Text.ToLower();
				list =
					list.Where(
						l =>
							l.Name.ToLower().Contains(srText) || l.Company.ToLower().Contains(srText) ||
							l.IssueCode.ToLower().Contains(srText));
			}
			if (cbState.SelectedIndex > 0)
				list = list.Where(l => l.State == (int)((IssueStateItem)cbState.SelectedItem).State);
			if (cbOwner.SelectedIndex > 0)
				list = list.Where(l => (cbOwner.SelectedIndex == 1) ? (ConfigManager.UserId == l.CreatedByUserId) : (ConfigManager.UserId == l.UserId));
			var newBindings = new SortableBindingList<IssueData>(list);
			string selected = null;
			if (dgvIssues.SelectedRows.Count > 0)
				selected = dgvIssues.SelectedRows[0].Cells[ColIssueId.Name].Value as string;
			var columnOrders = dgvIssues.Columns.Cast<DataGridViewColumn>().Where(c => c.HeaderCell.SortGlyphDirection != SortOrder.None).ToDictionary(column => column.Name, column => column.HeaderCell.SortGlyphDirection);
			dgvIssues.DataSource = newBindings;
			foreach (var columnOrder in columnOrders)
			{
				dgvIssues.Sort(dgvIssues.Columns[columnOrder.Key],
					columnOrder.Value == SortOrder.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending);
			}
			// ReSharper disable once AssignNullToNotNullAttribute
			//dgvIssues.Sort(dgvIssues.Columns[ColModified.Name], ListSortDirection.Ascending);
			SelectIssue(selected);
			if (first)
			{
				queryDelay.Stop();
				queryDelay.Start();
			}
		}

		private void queryDelay_Tick(object sender, EventArgs e)
		{
			queryDelay.Stop();
			parent.QueryIssuesByFilter(!string.IsNullOrEmpty(cbSearchText.Text) ? cbSearchText.Text.ToLower() : null,
				cbState.SelectedIndex > 0 ? (int?)((IssueStateItem)cbState.SelectedItem).State : null,
				cbOwner.SelectedIndex > 0 ? (bool?)(cbOwner.SelectedIndex == 1) : null);
		}

		private void dgvIssues_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (dgvIssues.Columns[e.ColumnIndex].Name == ColUser.Name)
			{
				if ((e.Value is int) && (int)e.Value == ConfigManager.UserId)
					e.Value = ConfigManager.UserName;
				return;
			}
			if (dgvIssues.Columns[e.ColumnIndex].Name != ColIssueState.Name) return;
			if (!(e.Value is int)) return;
			IssueStateItem stateItem;
			if (statesDict.TryGetValue((IssueState)e.Value, out stateItem))
				e.Value = stateItem.Caption;
		}

		private void cbSearchText_TextChanged(object sender, EventArgs e)
		{
			textChangeDelay.Start();
		}

		private void textChangeDelay_Tick(object sender, EventArgs e)
		{
			textChangeDelay.Stop();
			ApplyFilter();
		}

		private void cbState_SelectedIndexChanged(object sender, EventArgs e)
		{
			ApplyFilter();
		}

		private void cbOwner_SelectedIndexChanged(object sender, EventArgs e)
		{
			ApplyFilter();
		}

		private void dgvIssues_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return; // rowIndex is -1 when clicked on header
			var id = dgvIssues.Rows[e.RowIndex].Cells[ColIssueId.Name].Value as string;
			var del = IssueActionFired;
			if (del != null)
				del(this, new SingleValueEventArgs<string>(id));
		}

		private void dgvIssues_SelectionChanged(object sender, EventArgs e)
		{
			if (dgvIssues.SelectedRows.Count < 1) return;
			var id = dgvIssues.SelectedRows[0].Cells[ColIssueId.Name].Value as string;
			OnIssueSelectionChanged(id);
		}

		private void IssueFilterFormActivated(object sender, EventArgs e)
		{
			dgvIssues_SelectionChanged(sender, e);
		}

		private void IssueFilterFormDeactivate(object sender, EventArgs e)
		{
			OnIssueSelectionChanged(null);
		}

		private void IssueFilterFormFormClosed(object sender, FormClosedEventArgs e)
		{
			OnIssueSelectionChanged(null);
			SaveState();
		}

		private void OnIssueSelectionChanged(string id)
		{
			var del = IssueSelectionChanged;
			if (del != null)
				del(this, new SingleValueEventArgs<string>(id));
		}

		public void RefreshIssue(IssueData issue)
		{
			var bindings = dgvIssues.DataSource as SortableBindingList<IssueData>;
			if (bindings == null) return;
			var found = bindings.SingleOrDefault(i => i.IssueCode == issue.IssueCode);
			if (found == null) return;
			var idx = bindings.IndexOf(found);
			bindings[idx] = issue;
			bindings.ResetItem(idx);
		}

		public void SelectIssue(string p)
		{
			if (!string.IsNullOrEmpty(p))
			{
				foreach (DataGridViewRow row in dgvIssues.Rows)
				{
					if (p == (string)row.Cells[ColIssueId.Name].Value)
					{
						row.Selected = true;
						dgvIssues.FirstDisplayedScrollingRowIndex = row.Index;
					}
				}
			}
		}

		private void SaveState()
		{
			var formState = new IssueFilterFormState();
			if (WindowState == FormWindowState.Maximized)
			{
				formState.Location = RestoreBounds.Location;
				formState.Size = RestoreBounds.Size;
				formState.Maximised = true;
				formState.Minimised = false;
			}
			else if (WindowState == FormWindowState.Normal)
			{
				formState.Location = Location;
				formState.Size = Size;
				formState.Maximised = false;
				formState.Minimised = false;
			}
			else
			{
				formState.Location = RestoreBounds.Location;
				formState.Size = RestoreBounds.Size;
				formState.Maximised = false;
				formState.Minimised = true;
			}
			var columnOrders = dgvIssues.Columns.Cast<DataGridViewColumn>().Where(c => c.HeaderCell.SortGlyphDirection != SortOrder.None).ToDictionary(column => column.Name, column => column.HeaderCell.SortGlyphDirection);
			formState.SortingDirections = columnOrders;

			formState.OwnerFilter = cbOwner.Text;
			formState.StateFilter = cbState.Text;
			formState.TextFilter = cbSearchText.Text;

			IsolatedStorageSerializationHelper.Save(IssueFilterFormStateFile, formState);

		}
	}

}
