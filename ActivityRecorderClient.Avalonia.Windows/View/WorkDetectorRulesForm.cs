using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Rules;

namespace Tct.ActivityRecorderClient.View
{
	public partial class WorkDetectorRulesForm : FixedMetroForm
	{
		private readonly RuleRestrictions ruleRestrictions;
		private readonly bool canModifyRules = true;
		private ClientMenuLookup menuLookup = new ClientMenuLookup();
		public BindingList<WorkDetectorRule> Rules { get; private set; }

		public DesktopCapture DesktopCapture { get; set; }

		private bool rulesModified;

		public WorkDetectorRulesForm()
		{
			InitializeComponent();
			this.SetFormStartPositionCenterScreen();
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			Rules = new BindingList<WorkDetectorRule>();
			Rules.ListChanged += Rules_ListChanged;
			rulesGridView.ClipboardCopy += new EventHandler<ClipboardCopyEventArgs>(rulesGridView_ClipboardCopy);
			rulesGridView.DataSource = Rules;

			btnNew.Text = Labels.AutoRules_NewRule + "...";
			btnUp.Text = Labels.Up;
			btnDown.Text = Labels.Down;
			btnDelete.Text = Labels.Delete;
			btnCancel.Text = Labels.Cancel;
			btnRemoveInvalid.Text = Labels.AutoRules_RemoveInvalid;
			btnModify.Text = Labels.AutoRules_ModifyRule + "...";
			btnCopyToClipboard.Text = Labels.AutoRules_CopyToClipboard;
			cbAdvanced.Text = Labels.AutoRules_AdvancedSettings;
			relatedIdDataGridViewTextBoxColumn.HeaderText = Labels.AutoRules_HeaderRelatedId;
			relatedIdDataGridViewTextBoxColumn.ToolTipText = Labels.AutoRules_HeaderRelatedIdLong;
			nameDataGridViewTextBoxColumn.HeaderText = Labels.AutoRules_HeaderName;
			nameDataGridViewTextBoxColumn.ToolTipText = Labels.AutoRules_HeaderNameLong;
			titleRuleDataGridViewTextBoxColumn.HeaderText = Labels.AutoRules_HeaderTitleRule;
			titleRuleDataGridViewTextBoxColumn.ToolTipText = Labels.AutoRules_HeaderTitleRuleLong;
			processRuleDataGridViewTextBoxColumn.HeaderText = Labels.AutoRules_HeaderProcessRule;
			processRuleDataGridViewTextBoxColumn.ToolTipText = Labels.AutoRules_HeaderProcessRuleLong;
			isRegexDataGridViewCheckBoxColumn.HeaderText = Labels.AutoRules_HeaderIsRegex;
			isRegexDataGridViewCheckBoxColumn.ToolTipText = Labels.AutoRules_HeaderIsRegexLong;
			ignoreCaseDataGridViewCheckBoxColumn.HeaderText = Labels.AutoRules_HeaderIgnoreCase;
			ignoreCaseDataGridViewCheckBoxColumn.ToolTipText = Labels.AutoRules_HeaderIgnoreCaseLong;
			isEnabledDataGridViewCheckBoxColumn.HeaderText = Labels.AutoRules_HeaderIsEnabled;
			isEnabledDataGridViewCheckBoxColumn.ToolTipText = Labels.AutoRules_HeaderIsEnabledLong;
			ruleTypeDataGridViewComboBoxColumn.HeaderText = Labels.AutoRules_HeaderRuleType;
			ruleTypeDataGridViewComboBoxColumn.ToolTipText = Labels.AutoRules_HeaderRuleTypeLong;
			urlRuleDataGridViewTextBoxColumn.HeaderText = Labels.AutoRules_HeaderUrlRule;
			urlRuleDataGridViewTextBoxColumn.ToolTipText = Labels.AutoRules_HeaderUrlRuleLong;
			isPermanentDataGridViewCheckBoxColumn.HeaderText = Labels.AutoRules_HeaderIsPermanent;
			isPermanentDataGridViewCheckBoxColumn.ToolTipText = Labels.AutoRules_HeaderIsPermanentLong;
			createDateDataGridViewTextBoxColumn.HeaderText = Labels.AutoRules_HeaderCreateDate;
			createDateDataGridViewTextBoxColumn.ToolTipText = Labels.AutoRules_HeaderCreateDateLong;
			updateDateDataGridViewTextBoxColumn.HeaderText = Labels.AutoRules_HeaderUpdateDate;
			updateDateDataGridViewTextBoxColumn.ToolTipText = Labels.AutoRules_HeaderUpdateDateLong;
			BindWorkDetectorRuleType(ruleTypeDataGridViewComboBoxColumn);

			Text = Labels.AutoRules_Title;

			SetAdvancedView(); //set advanced visibility

			//this.ControlBox = false; //hax to avoid lag from UserActivityHook
			rulesGridView.DoubleClick += new EventHandler(rulesGridView_DoubleClick);
			rulesGridView.RowEnter += new DataGridViewCellEventHandler(rulesGridView_RowEnter);
			ruleRestrictions = ConfigManager.RuleRestrictions;
			if ((ruleRestrictions & RuleRestrictions.CannotCreateOrModifyRules) != 0)
			{
				canModifyRules = (ruleRestrictions & RuleRestrictions.CanModifyRuleTitle) != 0;
				processRuleDataGridViewTextBoxColumn.ReadOnly = true;
				titleRuleDataGridViewTextBoxColumn.ReadOnly = (ruleRestrictions & RuleRestrictions.CanModifyRuleTitle) == 0;
				urlRuleDataGridViewTextBoxColumn.ReadOnly = true;
				isPermanentDataGridViewCheckBoxColumn.ReadOnly = true;
				btnNew.Visible = false;
				btnModify.Visible = canModifyRules;
				btnUp.Visible = false;
				btnDown.Visible = false;
				btnRemoveInvalid.Visible = false;
			}
		}

		public void UpdateMenu(ClientMenuLookup value)
		{
			menuLookup = value;
			foreach (var form in this.OwnedForms.OfType<WorkDetectorRuleEditForm>().Where(n => !n.IsDisposed))
			{
				form.UpdateMenu(value);
			}
			ShowErrorForInvalidRelatedIds();
		}

		private int lastEnter = -1;
		private void rulesGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (lastEnter == e.RowIndex) return;
			lastEnter = e.RowIndex;
			ShowErrorForInvalidRelatedIds();
		}

		private void rulesGridView_DoubleClick(object sender, EventArgs e)
		{
			if (!canModifyRules) return;
			if (rulesGridView.SelectedCells.Count > 0 && rulesGridView.SelectedCells[0].ColumnIndex < 3)
			{
				//detect if the cursor is on the selected cell (we don't want to trigger this on the column header for example)
				int rowIdx;
				if (rulesGridView.SelectedRows.Count > 0)
				{
					rowIdx = rulesGridView.SelectedRows[0].Index;
				}
				else if (rulesGridView.SelectedCells.Count > 0)
				{
					rowIdx = rulesGridView.SelectedCells[0].RowIndex;
				}
				else
				{
					return;
				}
				if (!rulesGridView.GetRowDisplayRectangle(rowIdx, true).Contains(rulesGridView.PointToClient(Control.MousePosition))) return;
				btnModify_Click(btnModify, EventArgs.Empty);
			}
		}

		private void Rules_ListChanged(object sender, ListChangedEventArgs e)
		{
			var hasAdvanced = Rules
				.Where(n => RuleManagementService.IsAdvancedViewNeededFor(n))
				.Any();
			if (hasAdvanced)
			{
				cbAdvanced.Checked = true;
			}
		}

		private static void BindWorkDetectorRuleType(DataGridViewComboBoxColumn cb)
		{
			var listToBind = Enum.GetValues(typeof(WorkDetectorRuleType)).Cast<WorkDetectorRuleType>()
				.Select(n => new KeyValuePair<string, WorkDetectorRuleType>(RuleManagementService.GetShortNameFor(n), n))
				.ToList();
			cb.DisplayMember = "Key";
			cb.ValueMember = "Value";
			cb.DataSource = listToBind;
		}

		private void btnNew_Click(object sender, EventArgs e)
		{
			AddRule(null);
		}

		private void AddRule(DesktopCapture dc)
		{
			var aw = dc.GetActiveWindow();
			if (menuLookup.WorkDataById.Count == 0) return;
			var ruleToAdd = new WorkDetectorRule()
			{
				RuleType = WorkDetectorRuleType.TempStartWork,
				RelatedId = -1,
				ProcessRule = aw == null ? Labels.AutoRules_ExampleExe : aw.ProcessName,
				TitleRule = aw == null ? "*" : aw.Title,
				UrlRule = (aw == null || aw.Url == null) ? "*" : aw.Url,
				IsEnabled = true,
				IgnoreCase = true,
			};
			var result = EditWorkDetectorRule(Labels.AutoRules_NewRule, ruleToAdd);
			if (result != DialogResult.OK) return;

			InsertRuleToTheBeginning(ruleToAdd);
			rulesModified = true;
		}

		private void InsertRuleToTheBeginning(WorkDetectorRule ruleToAdd)
		{
			int insertAt;
			if (!IsLearningRule(ruleToAdd)) //Add to the beginning if its not a learning rule
			{
				insertAt = 0;
			}
			else //if it's a learning rule then 
			{
				insertAt = 0;
				for (int i = 0; i < Rules.Count; i++)
				{
					if (IsLearningRule(Rules[i])) break; //insert after the last non-learning rule
					insertAt = i + 1;
				}
			}
			Rules.Insert(insertAt, ruleToAdd);
			SelectRow(insertAt); //select the newly inserted rule
		}

		private void InsertRuleToTheEnd(WorkDetectorRule ruleToAdd)
		{
			int insertAt;
			if (IsLearningRule(ruleToAdd)) //Add to the end if its a learning rule
			{
				insertAt = Rules.Count;
			}
			else //if it's not a learning rule then 
			{
				insertAt = 0;
				for (int i = 0; i < Rules.Count; i++)
				{
					if (IsLearningRule(Rules[i])) break; //insert after the last non-learning rule
					insertAt = i + 1;
				}
			}
			Rules.Insert(insertAt, ruleToAdd);
			SelectRow(insertAt); //select the newly inserted rule
		}

		private void SelectRow(int idx)
		{
			rulesGridView.ClearSelection();
			rulesGridView.Rows[idx].Selected = true;
			Debug.Assert(rulesGridView.Rows[idx].Cells[1].Visible);
			if (!rulesGridView.Rows[idx].Cells[1].Visible) return;
			rulesGridView.CurrentCell = rulesGridView.Rows[idx].Cells[1]; //0 might be invisible but 1 is not
		}

		private DialogResult EditWorkDetectorRule(string title, WorkDetectorRule ruleToEdit)
		{
			if (menuLookup.WorkDataById.Count == 0)
			{
				MessageBox.Show(Labels.AutoRules_NotificationNoWorkErrorBody, Labels.AutoRules_NotificationNoWorkErrorTitle);
				return DialogResult.Cancel;
			}
			using (var editForm = new WorkDetectorRuleEditForm())
			{
				editForm.Owner = this;
				editForm.Text = title;
				editForm.Edit(ruleToEdit, null, menuLookup,
					cbAdvanced.Checked ? WorkDetectorRuleEditForm.DisplayType.EditRuleAdvanced : WorkDetectorRuleEditForm.DisplayType.EditRule,
					true);
				return editForm.DialogResult;
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void btnUp_Click(object sender, EventArgs e)
		{
			int idx;
			var itemToMove = GetSelectedItem(out idx);
			if (idx < 1) return;
			Rules.RemoveAt(idx);
			Rules.Insert(idx - 1, itemToMove);
			SelectRow(idx - 1);
			rulesModified = true;
		}

		private void btnDown_Click(object sender, EventArgs e)
		{
			int idx;
			var itemToMove = GetSelectedItem(out idx);
			if (idx == -1 || idx == Rules.Count - 1) return;
			Rules.RemoveAt(idx);
			Rules.Insert(idx + 1, itemToMove);
			SelectRow(idx + 1);
			rulesModified = true;
		}

		private WorkDetectorRule GetSelectedItem(out int index)
		{
			index = -1;
			var items = GetSelectedItems(out var indexes);
			if (items == null || items.Length > 1) return null;
			index = indexes[0];
			return items[0];
		}

		private WorkDetectorRule[] GetSelectedItems(out int[] indexes)
		{
			indexes = new int[0];
			List<WorkDetectorRule> itemsSelected = null;
			if (rulesGridView.SelectedRows.Count > 0)
			{
				itemsSelected = rulesGridView.SelectedRows.OfType<DataGridViewRow>().Select(r => r.DataBoundItem).OfType<WorkDetectorRule>().ToList();
			}
			else if (rulesGridView.SelectedCells.Count > 0)
			{
				itemsSelected = rulesGridView.SelectedCells.OfType<DataGridViewCell>().Select(c => rulesGridView.Rows[c.RowIndex].DataBoundItem).OfType<WorkDetectorRule>().ToList();
			}
			if (itemsSelected == null) return null;
			indexes = itemsSelected.Select(i => Rules.IndexOf(i)).ToArray();
			return itemsSelected.ToArray();
		}

		private void rulesGridView_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			if (e.RowIndex >= Rules.Count) return; //why is this needed suddenly ?!
			var item = rulesGridView.Rows[e.RowIndex].DataBoundItem as WorkDetectorRule;
			try
			{
				if (item == null) throw new Exception(Labels.AutoRules_EmptyRuleError);
				if (string.IsNullOrEmpty(item.ProcessRule)) item.ProcessRule = item.IsRegex ? ".*" : "*";
				if (string.IsNullOrEmpty(item.TitleRule)) item.TitleRule = item.IsRegex ? "^$" : "";
				if (string.IsNullOrEmpty(item.UrlRule)) item.UrlRule = item.IsRegex ? "^$" : "";
				item.ValidateAndGetMatchers(menuLookup);
				rulesGridView.Rows[e.RowIndex].ErrorText = "";
			}
			catch (Exception ex)
			{
				rulesGridView.Rows[e.RowIndex].ErrorText = Labels.Error + "! " + Environment.NewLine + ex.Message;
				//rulesGridView.Rows[e.RowIndex].Cells["isEnabledDataGridViewCheckBoxColumn"].Value = false;
				e.Cancel = true;
			}
		}

		private void ShowErrorForInvalidRelatedIds()
		{
			var allValid = true;
			if (Rules.Count != rulesGridView.RowCount) return;
			for (int i = 0; i < Rules.Count && i < rulesGridView.RowCount; i++)
			{
				var curRule = Rules[i];
				if (RuleManagementService.IsWorkAvailableFor(curRule.RuleType))
				{
					var realWorkData = menuLookup.GetWorkDataWithParentNames(curRule.RelatedId);
					rulesGridView.Rows[i].ErrorText = realWorkData != null && realWorkData.WorkData.IsVisibleInRules ? "" : Labels.AutoRules_InvalidWorkTitle;
					allValid &= realWorkData != null && realWorkData.WorkData.IsVisibleInRules;
				}
				else if (RuleManagementService.IsCategoryAvailableFor(curRule.RuleType))
				{
					rulesGridView.Rows[i].ErrorText = menuLookup.AllCategoriesById.ContainsKey(curRule.RelatedId) ? "" : Labels.AutoRules_InvalidCategoryTitle;
				}
			}

			btnRemoveInvalid.Visible = !allValid;
		}

		private void WorkDetectorRulesForm_Shown(object sender, EventArgs e)
		{
			ShowErrorForInvalidRelatedIds();
			BringToFront();
			Focus();
			if (DesktopCapture == null) return;
			AddRule(DesktopCapture);
			//bring it to front so if we open it with a hotkey it will still be visible
			//but its not working...
			TopLevel = true;
			TopMost = true;
			TopMost = false;
		}

		private void btnModify_Click(object sender, EventArgs e)
		{
			int idx;
			var itemToModify = GetSelectedItem(out idx);
			if (itemToModify == null || menuLookup.WorkDataById.Count == 0) return;
			var result = EditWorkDetectorRule(Labels.AutoRules_ModifyRule, itemToModify);
			if (result != DialogResult.OK) return;
			itemToModify.UpdateDate = DateTime.Now;
			rulesGridView.Refresh(); //this is required so cell painting is called and the appropriate IsPermanent cell will be displayed
			//we need to updates filters
			var filterCol = rulesGridView.Columns.OfType<IFilterColumn>().FirstOrDefault();
			if (filterCol != null) filterCol.ApplyFilters();
			rulesModified = true;
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			var itemsToDelete = GetSelectedItems(out var idxs);
			if (itemsToDelete == null || idxs.Length == 0) return;
			foreach (var item in itemsToDelete)
			{
				Rules.Remove(item);
			}
			rulesModified = true;
		}

		private void cbAdvanced_CheckedChanged(object sender, EventArgs e)
		{
			SetAdvancedView();
			rulesGridView_SelectionChanged(this, EventArgs.Empty);
		}

		public static bool IsPermanentHidden(WorkDetectorRule rule)
		{
			return rule != null && !RuleManagementService.IsPermanentAvailableFor(rule.RuleType);
		}

		private void rulesGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			SetAdvancedView(); //hax because I cannot hide ruleTypeDataGridViewComboBoxColumn at the ctor...

			//Hide IsPermanent checkboxes where not applicable
			var isPermCol = rulesGridView.Columns[isPermanentDataGridViewCheckBoxColumn.Name];
			if (e.RowIndex < 0 || isPermCol == null) return; //header or invalid grid
			var item = rulesGridView.Rows[e.RowIndex].DataBoundItem as WorkDetectorRule;
			if (IsPermanentHidden(item) && e.ColumnIndex == isPermCol.Index) //paint only the background
			{
				e.PaintBackground(e.CellBounds, true);
				e.Handled = true;
			}
		}

		private void SetAdvancedView()
		{
			btnCopyToClipboard.Visible = cbAdvanced.Checked;
			ruleTypeDataGridViewComboBoxColumn.Visible = cbAdvanced.Checked;
			isRegexDataGridViewCheckBoxColumn.Visible = cbAdvanced.Checked;
			isPermanentDataGridViewCheckBoxColumn.Visible = cbAdvanced.Checked;
			createDateDataGridViewTextBoxColumn.Visible = cbAdvanced.Checked;
			updateDateDataGridViewTextBoxColumn.Visible = cbAdvanced.Checked;
		}

		private void btnCopyToClipboard_Click(object sender, EventArgs e)
		{
			var selectedItemsData = GetSelectedItemsData();
			if (selectedItemsData == null)
			{
				MessageBox.Show(Labels.AutoRules_SelectRuleFirst);
			}
			else
			{
				Clipboard.SetText(selectedItemsData);
			}
		}

		private void rulesGridView_ClipboardCopy(object sender, ClipboardCopyEventArgs e)
		{
			var selectedItemsData = GetSelectedItemsData();
			if (selectedItemsData == null) return;
			e.ClipboardData = new DataObject(selectedItemsData);
		}

		private string GetSelectedItemsData()
		{
			int idx;
			var selectedItem = GetSelectedItem(out idx);
			if (selectedItem == null) return null;
			return selectedItem.ToSerializedString();
		}

		private void rulesGridView_SelectionChanged(object sender, EventArgs e)
		{
			var selectedItems = GetSelectedItems(out var idxs);
			btnDelete.Enabled = selectedItems != null && selectedItems.Length >= 1;
			btnCopyToClipboard.Enabled = idxs.Length == 1;
			btnModify.Enabled = idxs.Length == 1;
			var canMoveUp = selectedItems != null && idxs.Length == 1
				&& idxs[0] > 0
				&& (!IsLearningRule(selectedItems[0]) || IsLearningRule(Rules[idxs[0] - 1]));
			var canMoveDown = selectedItems != null && idxs.Length == 1
				&& idxs[0] < Rules.Count - 1
				&& (IsLearningRule(selectedItems[0]) || !IsLearningRule(Rules[idxs[0] + 1]));
			btnUp.Enabled = canMoveUp;
			btnDown.Enabled = canMoveDown;
		}

		private static bool IsLearningRule(WorkDetectorRule rule) //learning rules should be after all other rules
		{
			return RuleManagementService.IsLearning(rule.RuleType);
		}

		private void rulesGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
			if (e.RowIndex >= Rules.Count) return;
			rulesModified = true;
			if (isEnabledDataGridViewCheckBoxColumn.Index == e.ColumnIndex) return; //we don't care about IsEnabled change
			Rules[e.RowIndex].UpdateDate = DateTime.Now;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			if (DialogResult == DialogResult.OK || e.Cancel || e.CloseReason != CloseReason.UserClosing) return;
			if (!rulesModified) return;
			var res = MessageBox.Show(this, Labels.AutoRules_CancelConfirmBody, Labels.AutoRules_CancelConfirmTitle, MessageBoxButtons.YesNo);
			e.Cancel = res == DialogResult.No;
		}

		private void HandleRemoveInvalidClicked(object sender, EventArgs e)
		{
			for (int i = 0; i < Rules.Count; i++)
			{
				var currentRule = Rules[i];
				if (RuleManagementService.IsWorkAvailableFor(currentRule.RuleType) &&
				    !menuLookup.WorkDataById.ContainsKey(currentRule.RelatedId))
				{
					Rules.RemoveAt(i--);
					rulesModified = true;
				}
			}
		}
	}
}
