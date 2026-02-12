using MetroFramework.Controls;

namespace Tct.ActivityRecorderClient.View
{
	partial class WorkDetectorRulesForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell dataGridViewFilterColumnHeaderCell1 = new Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell dataGridViewFilterColumnHeaderCell2 = new Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell dataGridViewFilterColumnHeaderCell3 = new Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell dataGridViewFilterColumnHeaderCell4 = new Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell dataGridViewFilterColumnHeaderCell5 = new Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell dataGridViewFilterColumnHeaderCell6 = new Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			this.RulesBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.btnNew = new MetroFramework.Controls.MetroButton();
			this.btnUp = new MetroFramework.Controls.MetroButton();
			this.btnDown = new MetroFramework.Controls.MetroButton();
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.btnCancel = new MetroFramework.Controls.MetroButton();
			this.btnModify = new MetroFramework.Controls.MetroButton();
			this.btnDelete = new MetroFramework.Controls.MetroButton();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.rulesGridView = new Tct.ActivityRecorderClient.View.ClipboardCopyDataGridView();
			this.ruleTypeDataGridViewComboBoxColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.relatedIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.nameDataGridViewTextBoxColumn = new Tct.ActivityRecorderClient.View.DataGridViewFilterTextBoxColumn();
			this.titleRuleDataGridViewTextBoxColumn = new Tct.ActivityRecorderClient.View.DataGridViewFilterTextBoxColumn();
			this.processRuleDataGridViewTextBoxColumn = new Tct.ActivityRecorderClient.View.DataGridViewFilterTextBoxColumn();
			this.urlRuleDataGridViewTextBoxColumn = new Tct.ActivityRecorderClient.View.DataGridViewFilterTextBoxColumn();
			this.isPermanentDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.isRegexDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ignoreCaseDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.isEnabledDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.updateDateDataGridViewTextBoxColumn = new Tct.ActivityRecorderClient.View.DataGridViewFilterTextBoxColumn();
			this.createDateDataGridViewTextBoxColumn = new Tct.ActivityRecorderClient.View.DataGridViewFilterTextBoxColumn();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.btnRemoveInvalid = new MetroFramework.Controls.MetroButton();
			this.btnCopyToClipboard = new MetroFramework.Controls.MetroButton();
			this.cbAdvanced = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.RulesBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rulesGridView)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// RulesBindingSource
			// 
			this.RulesBindingSource.AllowNew = true;
			this.RulesBindingSource.DataSource = typeof(Tct.ActivityRecorderClient.ActivityRecorderServiceReference.WorkDetectorRule);
			// 
			// btnNew
			// 
			this.btnNew.Location = new System.Drawing.Point(20, 8);
			this.btnNew.Name = "btnNew";
			this.btnNew.Size = new System.Drawing.Size(115, 23);
			this.btnNew.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnNew.TabIndex = 1;
			this.btnNew.Text = "Új szabály...";
			this.btnNew.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnNew.UseSelectable = true;
			this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
			// 
			// btnUp
			// 
			this.btnUp.Location = new System.Drawing.Point(20, 37);
			this.btnUp.Name = "btnUp";
			this.btnUp.Size = new System.Drawing.Size(115, 23);
			this.btnUp.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnUp.TabIndex = 2;
			this.btnUp.Text = "Fel";
			this.btnUp.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnUp.UseSelectable = true;
			this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
			// 
			// btnDown
			// 
			this.btnDown.Location = new System.Drawing.Point(20, 66);
			this.btnDown.Name = "btnDown";
			this.btnDown.Size = new System.Drawing.Size(115, 23);
			this.btnDown.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnDown.TabIndex = 3;
			this.btnDown.Text = "Le";
			this.btnDown.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnDown.UseSelectable = true;
			this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(2, 306);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(79, 23);
			this.btnOk.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnOk.TabIndex = 9;
			this.btnOk.Text = "OK";
			this.btnOk.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(83, 306);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(79, 23);
			this.btnCancel.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnCancel.TabIndex = 10;
			this.btnCancel.Text = "Mégse";
			this.btnCancel.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnCancel.UseSelectable = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnModify
			// 
			this.btnModify.Location = new System.Drawing.Point(20, 95);
			this.btnModify.Name = "btnModify";
			this.btnModify.Size = new System.Drawing.Size(115, 38);
			this.btnModify.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnModify.TabIndex = 4;
			this.btnModify.Text = "Munka módosítása...";
			this.btnModify.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnModify.UseSelectable = true;
			this.btnModify.Click += new System.EventHandler(this.btnModify_Click);
			// 
			// btnDelete
			// 
			this.btnDelete.Location = new System.Drawing.Point(20, 139);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(115, 23);
			this.btnDelete.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnDelete.TabIndex = 5;
			this.btnDelete.Text = "Törlés";
			this.btnDelete.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnDelete.UseSelectable = true;
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = new System.Drawing.Point(20, 60);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.rulesGridView);
			this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.flowLayoutPanel1);
			this.splitContainer1.Panel2.Controls.Add(this.cbAdvanced);
			this.splitContainer1.Panel2.Controls.Add(this.btnOk);
			this.splitContainer1.Panel2.Controls.Add(this.btnCancel);
			this.splitContainer1.Size = new System.Drawing.Size(984, 340);
			this.splitContainer1.SplitterDistance = 814;
			this.splitContainer1.TabIndex = 10;
			// 
			// rulesGridView
			// 
			this.rulesGridView.AllowUserToAddRows = false;
			this.rulesGridView.AllowUserToResizeRows = false;
			this.rulesGridView.AutoGenerateColumns = false;
			this.rulesGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.rulesGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ruleTypeDataGridViewComboBoxColumn,
            this.relatedIdDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn,
            this.titleRuleDataGridViewTextBoxColumn,
            this.processRuleDataGridViewTextBoxColumn,
            this.urlRuleDataGridViewTextBoxColumn,
            this.isPermanentDataGridViewCheckBoxColumn,
            this.isRegexDataGridViewCheckBoxColumn,
            this.ignoreCaseDataGridViewCheckBoxColumn,
            this.isEnabledDataGridViewCheckBoxColumn,
            this.updateDateDataGridViewTextBoxColumn,
            this.createDateDataGridViewTextBoxColumn});
			this.rulesGridView.DataSource = this.RulesBindingSource;
			this.rulesGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rulesGridView.Location = new System.Drawing.Point(12, 10);
			this.rulesGridView.Name = "rulesGridView";
			this.rulesGridView.Size = new System.Drawing.Size(790, 320);
			this.rulesGridView.TabIndex = 0;
			this.rulesGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.rulesGridView_CellPainting);
			this.rulesGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.rulesGridView_CellValueChanged);
			this.rulesGridView.RowValidating += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.rulesGridView_RowValidating);
			this.rulesGridView.SelectionChanged += new System.EventHandler(this.rulesGridView_SelectionChanged);
			// 
			// ruleTypeDataGridViewComboBoxColumn
			// 
			this.ruleTypeDataGridViewComboBoxColumn.DataPropertyName = "RuleType";
			this.ruleTypeDataGridViewComboBoxColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
			this.ruleTypeDataGridViewComboBoxColumn.HeaderText = "Típus";
			this.ruleTypeDataGridViewComboBoxColumn.Name = "ruleTypeDataGridViewComboBoxColumn";
			this.ruleTypeDataGridViewComboBoxColumn.ReadOnly = true;
			this.ruleTypeDataGridViewComboBoxColumn.Width = 90;
			// 
			// relatedIdDataGridViewTextBoxColumn
			// 
			this.relatedIdDataGridViewTextBoxColumn.DataPropertyName = "RelatedId";
			this.relatedIdDataGridViewTextBoxColumn.HeaderText = "Munka Id";
			this.relatedIdDataGridViewTextBoxColumn.Name = "relatedIdDataGridViewTextBoxColumn";
			this.relatedIdDataGridViewTextBoxColumn.ReadOnly = true;
			this.relatedIdDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.relatedIdDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.relatedIdDataGridViewTextBoxColumn.Width = 50;
			// 
			// nameDataGridViewTextBoxColumn
			// 
			this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
			this.nameDataGridViewTextBoxColumn.FilterString = "";
			dataGridViewFilterColumnHeaderCell1.ErrorText = "";
			dataGridViewFilterColumnHeaderCell1.FilterString = "";
			dataGridViewCellStyle1.Padding = new System.Windows.Forms.Padding(0, 0, 18, 0);
			dataGridViewFilterColumnHeaderCell1.Style = dataGridViewCellStyle1;
			dataGridViewFilterColumnHeaderCell1.Value = "Név";
			dataGridViewFilterColumnHeaderCell1.ValueType = typeof(object);
			this.nameDataGridViewTextBoxColumn.HeaderCell = dataGridViewFilterColumnHeaderCell1;
			this.nameDataGridViewTextBoxColumn.HeaderText = "Név";
			this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
			this.nameDataGridViewTextBoxColumn.ReadOnly = true;
			this.nameDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.nameDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.nameDataGridViewTextBoxColumn.Width = 180;
			// 
			// titleRuleDataGridViewTextBoxColumn
			// 
			this.titleRuleDataGridViewTextBoxColumn.DataPropertyName = "TitleRule";
			this.titleRuleDataGridViewTextBoxColumn.FilterString = "";
			dataGridViewFilterColumnHeaderCell2.ErrorText = "";
			dataGridViewFilterColumnHeaderCell2.FilterString = "";
			dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(0, 0, 18, 0);
			dataGridViewFilterColumnHeaderCell2.Style = dataGridViewCellStyle2;
			dataGridViewFilterColumnHeaderCell2.Value = "Címsor szabály";
			dataGridViewFilterColumnHeaderCell2.ValueType = typeof(object);
			this.titleRuleDataGridViewTextBoxColumn.HeaderCell = dataGridViewFilterColumnHeaderCell2;
			this.titleRuleDataGridViewTextBoxColumn.HeaderText = "Címsor szabály";
			this.titleRuleDataGridViewTextBoxColumn.Name = "titleRuleDataGridViewTextBoxColumn";
			this.titleRuleDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.titleRuleDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.titleRuleDataGridViewTextBoxColumn.ToolTipText = "Címsor szabály";
			this.titleRuleDataGridViewTextBoxColumn.Width = 160;
			// 
			// processRuleDataGridViewTextBoxColumn
			// 
			this.processRuleDataGridViewTextBoxColumn.DataPropertyName = "ProcessRule";
			this.processRuleDataGridViewTextBoxColumn.FilterString = "";
			dataGridViewFilterColumnHeaderCell3.ErrorText = "";
			dataGridViewFilterColumnHeaderCell3.FilterString = "";
			dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(0, 0, 18, 0);
			dataGridViewFilterColumnHeaderCell3.Style = dataGridViewCellStyle3;
			dataGridViewFilterColumnHeaderCell3.Value = "Processz szabály";
			dataGridViewFilterColumnHeaderCell3.ValueType = typeof(object);
			this.processRuleDataGridViewTextBoxColumn.HeaderCell = dataGridViewFilterColumnHeaderCell3;
			this.processRuleDataGridViewTextBoxColumn.HeaderText = "Processz szabály";
			this.processRuleDataGridViewTextBoxColumn.Name = "processRuleDataGridViewTextBoxColumn";
			this.processRuleDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.processRuleDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.processRuleDataGridViewTextBoxColumn.ToolTipText = "Processz szabály";
			// 
			// urlRuleDataGridViewTextBoxColumn
			// 
			this.urlRuleDataGridViewTextBoxColumn.DataPropertyName = "UrlRule";
			this.urlRuleDataGridViewTextBoxColumn.FilterString = "";
			dataGridViewFilterColumnHeaderCell4.ErrorText = "";
			dataGridViewFilterColumnHeaderCell4.FilterString = "";
			dataGridViewCellStyle4.Padding = new System.Windows.Forms.Padding(0, 0, 18, 0);
			dataGridViewFilterColumnHeaderCell4.Style = dataGridViewCellStyle4;
			dataGridViewFilterColumnHeaderCell4.Value = "Url szabály";
			dataGridViewFilterColumnHeaderCell4.ValueType = typeof(object);
			this.urlRuleDataGridViewTextBoxColumn.HeaderCell = dataGridViewFilterColumnHeaderCell4;
			this.urlRuleDataGridViewTextBoxColumn.HeaderText = "Url szabály";
			this.urlRuleDataGridViewTextBoxColumn.Name = "urlRuleDataGridViewTextBoxColumn";
			this.urlRuleDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.urlRuleDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.urlRuleDataGridViewTextBoxColumn.ToolTipText = "Url szabály";
			// 
			// isPermanentDataGridViewCheckBoxColumn
			// 
			this.isPermanentDataGridViewCheckBoxColumn.DataPropertyName = "IsPermanent";
			this.isPermanentDataGridViewCheckBoxColumn.HeaderText = "Végleges";
			this.isPermanentDataGridViewCheckBoxColumn.Name = "isPermanentDataGridViewCheckBoxColumn";
			this.isPermanentDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.isPermanentDataGridViewCheckBoxColumn.ToolTipText = "Végleges szabályváltás";
			this.isPermanentDataGridViewCheckBoxColumn.Width = 70;
			// 
			// isRegexDataGridViewCheckBoxColumn
			// 
			this.isRegexDataGridViewCheckBoxColumn.DataPropertyName = "IsRegex";
			this.isRegexDataGridViewCheckBoxColumn.HeaderText = "Reguláris";
			this.isRegexDataGridViewCheckBoxColumn.Name = "isRegexDataGridViewCheckBoxColumn";
			this.isRegexDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.isRegexDataGridViewCheckBoxColumn.ToolTipText = "Reguláris kifejezés";
			this.isRegexDataGridViewCheckBoxColumn.Width = 70;
			// 
			// ignoreCaseDataGridViewCheckBoxColumn
			// 
			this.ignoreCaseDataGridViewCheckBoxColumn.DataPropertyName = "IgnoreCase";
			this.ignoreCaseDataGridViewCheckBoxColumn.HeaderText = "Kis/nagybet. megegyezik";
			this.ignoreCaseDataGridViewCheckBoxColumn.Name = "ignoreCaseDataGridViewCheckBoxColumn";
			this.ignoreCaseDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.ignoreCaseDataGridViewCheckBoxColumn.ToolTipText = "Kis- és nagybetű megegyezik";
			this.ignoreCaseDataGridViewCheckBoxColumn.Width = 70;
			// 
			// isEnabledDataGridViewCheckBoxColumn
			// 
			this.isEnabledDataGridViewCheckBoxColumn.DataPropertyName = "IsEnabled";
			this.isEnabledDataGridViewCheckBoxColumn.HeaderText = "Aktív";
			this.isEnabledDataGridViewCheckBoxColumn.Name = "isEnabledDataGridViewCheckBoxColumn";
			this.isEnabledDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.isEnabledDataGridViewCheckBoxColumn.ToolTipText = "Aktív szabály";
			this.isEnabledDataGridViewCheckBoxColumn.Width = 50;
			// 
			// updateDateDataGridViewTextBoxColumn
			// 
			this.updateDateDataGridViewTextBoxColumn.DataPropertyName = "UpdateDate";
			this.updateDateDataGridViewTextBoxColumn.FilterString = "";
			dataGridViewFilterColumnHeaderCell5.ErrorText = "";
			dataGridViewFilterColumnHeaderCell5.FilterString = "";
			dataGridViewFilterColumnHeaderCell5.Style = dataGridViewCellStyle5;
			dataGridViewFilterColumnHeaderCell5.Value = "UpdateDate";
			dataGridViewFilterColumnHeaderCell5.ValueType = typeof(object);
			this.updateDateDataGridViewTextBoxColumn.HeaderCell = dataGridViewFilterColumnHeaderCell5;
			this.updateDateDataGridViewTextBoxColumn.HeaderText = "UpdateDate";
			this.updateDateDataGridViewTextBoxColumn.Name = "updateDateDataGridViewTextBoxColumn";
			this.updateDateDataGridViewTextBoxColumn.ReadOnly = true;
			this.updateDateDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.updateDateDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// createDateDataGridViewTextBoxColumn
			// 
			this.createDateDataGridViewTextBoxColumn.DataPropertyName = "CreateDate";
			this.createDateDataGridViewTextBoxColumn.FilterString = "";
			dataGridViewFilterColumnHeaderCell6.ErrorText = "";
			dataGridViewFilterColumnHeaderCell6.FilterString = "";
			dataGridViewFilterColumnHeaderCell6.Style = dataGridViewCellStyle6;
			dataGridViewFilterColumnHeaderCell6.Value = "CreateDate";
			dataGridViewFilterColumnHeaderCell6.ValueType = typeof(object);
			this.createDateDataGridViewTextBoxColumn.HeaderCell = dataGridViewFilterColumnHeaderCell6;
			this.createDateDataGridViewTextBoxColumn.HeaderText = "CreateDate";
			this.createDateDataGridViewTextBoxColumn.Name = "createDateDataGridViewTextBoxColumn";
			this.createDateDataGridViewTextBoxColumn.ReadOnly = true;
			this.createDateDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.createDateDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.btnNew);
			this.flowLayoutPanel1.Controls.Add(this.btnUp);
			this.flowLayoutPanel1.Controls.Add(this.btnDown);
			this.flowLayoutPanel1.Controls.Add(this.btnModify);
			this.flowLayoutPanel1.Controls.Add(this.btnDelete);
			this.flowLayoutPanel1.Controls.Add(this.btnRemoveInvalid);
			this.flowLayoutPanel1.Controls.Add(this.btnCopyToClipboard);
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 10);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(17, 5, 5, 5);
			this.flowLayoutPanel1.Size = new System.Drawing.Size(158, 261);
			this.flowLayoutPanel1.TabIndex = 12;
			// 
			// btnRemoveInvalid
			// 
			this.btnRemoveInvalid.Location = new System.Drawing.Point(20, 168);
			this.btnRemoveInvalid.Name = "btnRemoveInvalid";
			this.btnRemoveInvalid.Size = new System.Drawing.Size(115, 23);
			this.btnRemoveInvalid.TabIndex = 6;
			this.btnRemoveInvalid.Text = "Érvénytelen törlés";
			this.btnRemoveInvalid.UseSelectable = true;
			this.btnRemoveInvalid.Click += new System.EventHandler(this.HandleRemoveInvalidClicked);
			// 
			// btnCopyToClipboard
			// 
			this.btnCopyToClipboard.Location = new System.Drawing.Point(20, 197);
			this.btnCopyToClipboard.Name = "btnCopyToClipboard";
			this.btnCopyToClipboard.Size = new System.Drawing.Size(115, 23);
			this.btnCopyToClipboard.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnCopyToClipboard.TabIndex = 7;
			this.btnCopyToClipboard.Text = "Vágólapra";
			this.btnCopyToClipboard.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnCopyToClipboard.UseSelectable = true;
			this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
			// 
			// cbAdvanced
			// 
			this.cbAdvanced.AutoSize = true;
			this.cbAdvanced.Location = new System.Drawing.Point(22, 277);
			this.cbAdvanced.Name = "cbAdvanced";
			this.cbAdvanced.Size = new System.Drawing.Size(115, 17);
			this.cbAdvanced.TabIndex = 8;
			this.cbAdvanced.Text = "Haladó beállítások";
			this.cbAdvanced.UseVisualStyleBackColor = true;
			this.cbAdvanced.CheckedChanged += new System.EventHandler(this.cbAdvanced_CheckedChanged);
			// 
			// WorkDetectorRulesForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(1024, 420);
			this.Controls.Add(this.splitContainer1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(480, 420);
			this.Name = "WorkDetectorRulesForm";
			this.Resizable = false;
			this.Text = "Automatikus szabályok";
			this.Shown += new System.EventHandler(this.WorkDetectorRulesForm_Shown);
			((System.ComponentModel.ISupportInitialize)(this.RulesBindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.rulesGridView)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ClipboardCopyDataGridView rulesGridView;
		private System.Windows.Forms.BindingSource RulesBindingSource;
		private MetroButton btnNew;
		private MetroButton btnUp;
		private MetroButton btnDown;
		private MetroButton btnOk;
		private MetroButton btnCancel;
		private MetroButton btnModify;
		private MetroButton btnDelete;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.CheckBox cbAdvanced;
		private MetroButton btnCopyToClipboard;
		private System.Windows.Forms.DataGridViewComboBoxColumn ruleTypeDataGridViewComboBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn relatedIdDataGridViewTextBoxColumn;
		private DataGridViewFilterTextBoxColumn nameDataGridViewTextBoxColumn;
		private DataGridViewFilterTextBoxColumn titleRuleDataGridViewTextBoxColumn;
		private DataGridViewFilterTextBoxColumn processRuleDataGridViewTextBoxColumn;
		private DataGridViewFilterTextBoxColumn urlRuleDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn isPermanentDataGridViewCheckBoxColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn isRegexDataGridViewCheckBoxColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ignoreCaseDataGridViewCheckBoxColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn isEnabledDataGridViewCheckBoxColumn;
		private DataGridViewFilterTextBoxColumn updateDateDataGridViewTextBoxColumn;
		private DataGridViewFilterTextBoxColumn createDateDataGridViewTextBoxColumn;
		private MetroButton btnRemoveInvalid;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;

	}
}