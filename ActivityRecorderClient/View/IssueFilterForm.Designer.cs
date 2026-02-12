namespace Tct.ActivityRecorderClient.View
{
	partial class IssueFilterForm
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
			this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
			this.tlpFilters = new System.Windows.Forms.TableLayoutPanel();
			this.lblSearchText = new System.Windows.Forms.Label();
			this.lblOwner = new System.Windows.Forms.Label();
			this.lblState = new System.Windows.Forms.Label();
			this.cbState = new System.Windows.Forms.ComboBox();
			this.cbOwner = new System.Windows.Forms.ComboBox();
			this.cbSearchText = new System.Windows.Forms.ComboBox();
			this.dgvIssues = new System.Windows.Forms.DataGridView();
			this.textChangeDelay = new System.Windows.Forms.Timer(this.components);
			this.queryDelay = new System.Windows.Forms.Timer(this.components);
			this.ColIssueId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColIssueName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColIssueCompany = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColIssueState = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColCreatedBy = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColModified = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColModifiedBy = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tlpMain.SuspendLayout();
			this.tlpFilters.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvIssues)).BeginInit();
			this.SuspendLayout();
			// 
			// tlpMain
			// 
			this.tlpMain.ColumnCount = 1;
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMain.Controls.Add(this.tlpFilters, 0, 0);
			this.tlpMain.Controls.Add(this.dgvIssues, 0, 1);
			this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpMain.Location = new System.Drawing.Point(20, 60);
			this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
			this.tlpMain.Name = "tlpMain";
			this.tlpMain.RowCount = 2;
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMain.Size = new System.Drawing.Size(560, 170);
			this.tlpMain.TabIndex = 0;
			// 
			// tlpFilters
			// 
			this.tlpFilters.ColumnCount = 3;
			this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpFilters.Controls.Add(this.lblSearchText, 0, 0);
			this.tlpFilters.Controls.Add(this.lblOwner, 2, 0);
			this.tlpFilters.Controls.Add(this.lblState, 1, 0);
			this.tlpFilters.Controls.Add(this.cbState, 1, 1);
			this.tlpFilters.Controls.Add(this.cbOwner, 2, 1);
			this.tlpFilters.Controls.Add(this.cbSearchText, 0, 1);
			this.tlpFilters.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpFilters.Location = new System.Drawing.Point(0, 0);
			this.tlpFilters.Margin = new System.Windows.Forms.Padding(0);
			this.tlpFilters.Name = "tlpFilters";
			this.tlpFilters.RowCount = 2;
			this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpFilters.Size = new System.Drawing.Size(560, 40);
			this.tlpFilters.TabIndex = 0;
			// 
			// lblSearchText
			// 
			this.lblSearchText.AutoSize = true;
			this.lblSearchText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblSearchText.Location = new System.Drawing.Point(3, 0);
			this.lblSearchText.Name = "lblSearchText";
			this.lblSearchText.Size = new System.Drawing.Size(302, 13);
			this.lblSearchText.TabIndex = 0;
			this.lblSearchText.Text = "SearchText:";
			this.lblSearchText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblOwner
			// 
			this.lblOwner.AutoSize = true;
			this.lblOwner.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblOwner.Location = new System.Drawing.Point(437, 0);
			this.lblOwner.Name = "lblOwner";
			this.lblOwner.Size = new System.Drawing.Size(120, 13);
			this.lblOwner.TabIndex = 1;
			this.lblOwner.Text = "Owner:";
			this.lblOwner.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblState
			// 
			this.lblState.AutoSize = true;
			this.lblState.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblState.Location = new System.Drawing.Point(311, 0);
			this.lblState.Name = "lblState";
			this.lblState.Size = new System.Drawing.Size(120, 13);
			this.lblState.TabIndex = 2;
			this.lblState.Text = "State:";
			this.lblState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cbState
			// 
			this.cbState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbState.FormattingEnabled = true;
			this.cbState.Location = new System.Drawing.Point(311, 16);
			this.cbState.Name = "cbState";
			this.cbState.Size = new System.Drawing.Size(120, 21);
			this.cbState.TabIndex = 3;
			// 
			// cbOwner
			// 
			this.cbOwner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbOwner.FormattingEnabled = true;
			this.cbOwner.Location = new System.Drawing.Point(437, 16);
			this.cbOwner.Name = "cbOwner";
			this.cbOwner.Size = new System.Drawing.Size(120, 21);
			this.cbOwner.TabIndex = 4;
			// 
			// cbSearchText
			// 
			this.cbSearchText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbSearchText.FormattingEnabled = true;
			this.cbSearchText.Location = new System.Drawing.Point(3, 16);
			this.cbSearchText.Name = "cbSearchText";
			this.cbSearchText.Size = new System.Drawing.Size(302, 21);
			this.cbSearchText.TabIndex = 5;
			this.cbSearchText.TextChanged += new System.EventHandler(this.cbSearchText_TextChanged);
			// 
			// dgvIssues
			// 
			this.dgvIssues.AllowUserToAddRows = false;
			this.dgvIssues.AllowUserToDeleteRows = false;
			this.dgvIssues.AllowUserToResizeRows = false;
			this.dgvIssues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvIssues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColIssueId,
            this.ColIssueName,
            this.ColIssueCompany,
            this.ColIssueState,
            this.ColUser,
            this.ColCreatedBy,
            this.ColModified,
            this.ColModifiedBy});
			this.dgvIssues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvIssues.Location = new System.Drawing.Point(0, 40);
			this.dgvIssues.Margin = new System.Windows.Forms.Padding(0);
			this.dgvIssues.MultiSelect = false;
			this.dgvIssues.Name = "dgvIssues";
			this.dgvIssues.ReadOnly = true;
			this.dgvIssues.RowHeadersVisible = false;
			this.dgvIssues.RowTemplate.ReadOnly = true;
			this.dgvIssues.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvIssues.Size = new System.Drawing.Size(560, 130);
			this.dgvIssues.TabIndex = 1;
			this.dgvIssues.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvIssues_CellDoubleClick);
			this.dgvIssues.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvIssues_CellFormatting);
			this.dgvIssues.SelectionChanged += new System.EventHandler(this.dgvIssues_SelectionChanged);
			// 
			// textChangeDelay
			// 
			this.textChangeDelay.Interval = 400;
			this.textChangeDelay.Tick += new System.EventHandler(this.textChangeDelay_Tick);
			// 
			// queryDelay
			// 
			this.queryDelay.Interval = 1000;
			this.queryDelay.Tick += new System.EventHandler(this.queryDelay_Tick);
			// 
			// ColIssueId
			// 
			this.ColIssueId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColIssueId.DataPropertyName = "IssueCode";
			this.ColIssueId.HeaderText = "Id";
			this.ColIssueId.Name = "ColIssueId";
			this.ColIssueId.ReadOnly = true;
			this.ColIssueId.Width = 41;
			// 
			// ColIssueName
			// 
			this.ColIssueName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColIssueName.DataPropertyName = "Name";
			this.ColIssueName.HeaderText = "Name";
			this.ColIssueName.Name = "ColIssueName";
			this.ColIssueName.ReadOnly = true;
			// 
			// ColIssueCompany
			// 
			this.ColIssueCompany.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColIssueCompany.DataPropertyName = "Company";
			this.ColIssueCompany.HeaderText = "Company";
			this.ColIssueCompany.Name = "ColIssueCompany";
			this.ColIssueCompany.ReadOnly = true;
			this.ColIssueCompany.Width = 76;
			// 
			// ColIssueState
			// 
			this.ColIssueState.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColIssueState.DataPropertyName = "State";
			this.ColIssueState.HeaderText = "State";
			this.ColIssueState.Name = "ColIssueState";
			this.ColIssueState.ReadOnly = true;
			this.ColIssueState.Width = 57;
			// 
			// ColUser
			// 
			this.ColUser.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColUser.DataPropertyName = "UserId";
			this.ColUser.HeaderText = "User";
			this.ColUser.MinimumWidth = 80;
			this.ColUser.Name = "ColUser";
			this.ColUser.ReadOnly = true;
			this.ColUser.Visible = false;
			this.ColUser.Width = 80;
			// 
			// ColCreatedBy
			// 
			this.ColCreatedBy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColCreatedBy.DataPropertyName = "CreatedByName";
			this.ColCreatedBy.HeaderText = "Created by";
			this.ColCreatedBy.Name = "CreatedBy";
			this.ColCreatedBy.ReadOnly = true;
			this.ColCreatedBy.Width = 83;
			// 
			// ColModified
			// 
			this.ColModified.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColModified.DataPropertyName = "Modified";
			this.ColModified.HeaderText = "Modified";
			this.ColModified.Name = "ColModified";
			this.ColModified.ReadOnly = true;
			this.ColModified.Width = 72;
			// 
			// ColModifiedBy
			// 
			this.ColModifiedBy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColModifiedBy.DataPropertyName = "ModifiedByName";
			this.ColModifiedBy.HeaderText = "Modified By";
			this.ColModifiedBy.Name = "ModifiedBy";
			this.ColModifiedBy.ReadOnly = true;
			this.ColModifiedBy.Width = 87;
			// 
			// IssueFilterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(600, 250);
			this.Controls.Add(this.tlpMain);
			this.MinimumSize = new System.Drawing.Size(600, 250);
			this.Name = "IssueFilterForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
			this.Text = "Issues";
			this.Activated += new System.EventHandler(this.IssueFilterFormActivated);
			this.Deactivate += new System.EventHandler(this.IssueFilterFormDeactivate);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.IssueFilterFormFormClosed);
			this.Load += new System.EventHandler(this.IssueFilterFormLoad);
			this.tlpMain.ResumeLayout(false);
			this.tlpFilters.ResumeLayout(false);
			this.tlpFilters.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvIssues)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tlpMain;
		private System.Windows.Forms.TableLayoutPanel tlpFilters;
		private System.Windows.Forms.Label lblSearchText;
		private System.Windows.Forms.Label lblOwner;
		private System.Windows.Forms.Label lblState;
		private System.Windows.Forms.ComboBox cbState;
		private System.Windows.Forms.ComboBox cbOwner;
		private System.Windows.Forms.ComboBox cbSearchText;
		private System.Windows.Forms.DataGridView dgvIssues;
		private System.Windows.Forms.Timer textChangeDelay;
		private System.Windows.Forms.Timer queryDelay;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColIssueId;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColIssueName;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColIssueCompany;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColIssueState;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColUser;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColCreatedBy;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColModified;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColModifiedBy;
	}
}