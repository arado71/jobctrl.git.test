namespace Tct.ActivityRecorderClient.View
{
	partial class IssuePropsForm
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
			this.lblCompany = new MetroFramework.Controls.MetroLabel();
			this.lblName = new MetroFramework.Controls.MetroLabel();
			this.lblState = new MetroFramework.Controls.MetroLabel();
			this.tlpIssueEdit = new System.Windows.Forms.TableLayoutPanel();
			this.cbCategory = new System.Windows.Forms.ComboBox();
			this.lblCategory = new MetroFramework.Controls.MetroLabel();
			this.cbCompany = new System.Windows.Forms.ComboBox();
			this.cbName = new System.Windows.Forms.ComboBox();
			this.cbState = new System.Windows.Forms.ComboBox();
			this.btnIssues = new MetroFramework.Controls.MetroButton();
			this.panelHideTimer = new System.Windows.Forms.Timer(this.components);
			this.formInactivationTimer = new System.Windows.Forms.Timer(this.components);
			this.tlpIssueEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblCompany
			// 
			this.lblCompany.AutoSize = true;
			this.lblCompany.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblCompany.Location = new System.Drawing.Point(3, 0);
			this.lblCompany.Name = "lblCompany";
			this.lblCompany.Size = new System.Drawing.Size(69, 27);
			this.lblCompany.TabIndex = 3;
			this.lblCompany.Text = "Company:";
			this.lblCompany.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblCompany.Visible = false;
			// 
			// lblName
			// 
			this.lblName.AutoSize = true;
			this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblName.Location = new System.Drawing.Point(3, 27);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(69, 27);
			this.lblName.TabIndex = 4;
			this.lblName.Text = "Name:";
			this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblState
			// 
			this.lblState.AutoSize = true;
			this.lblState.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblState.Location = new System.Drawing.Point(3, 81);
			this.lblState.Name = "lblState";
			this.lblState.Size = new System.Drawing.Size(69, 29);
			this.lblState.TabIndex = 5;
			this.lblState.Text = "State:";
			this.lblState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tlpIssueEdit
			// 
			this.tlpIssueEdit.BackColor = System.Drawing.Color.White;
			this.tlpIssueEdit.ColumnCount = 3;
			this.tlpIssueEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpIssueEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpIssueEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tlpIssueEdit.Controls.Add(this.cbCategory, 1, 2);
			this.tlpIssueEdit.Controls.Add(this.lblCategory, 0, 2);
			this.tlpIssueEdit.Controls.Add(this.lblCompany, 0, 0);
			this.tlpIssueEdit.Controls.Add(this.lblState, 0, 3);
			this.tlpIssueEdit.Controls.Add(this.lblName, 0, 1);
			this.tlpIssueEdit.Controls.Add(this.cbCompany, 1, 0);
			this.tlpIssueEdit.Controls.Add(this.cbName, 1, 1);
			this.tlpIssueEdit.Controls.Add(this.cbState, 1, 3);
			this.tlpIssueEdit.Controls.Add(this.btnIssues, 2, 3);
			this.tlpIssueEdit.Location = new System.Drawing.Point(0, 0);
			this.tlpIssueEdit.Margin = new System.Windows.Forms.Padding(0);
			this.tlpIssueEdit.Name = "tlpIssueEdit";
			this.tlpIssueEdit.RowCount = 5;
			this.tlpIssueEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpIssueEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpIssueEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpIssueEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpIssueEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpIssueEdit.Size = new System.Drawing.Size(432, 110);
			this.tlpIssueEdit.TabIndex = 6;
			// 
			// cbCategory
			// 
			this.cbCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tlpIssueEdit.SetColumnSpan(this.cbCategory, 2);
			this.cbCategory.FormattingEnabled = true;
			this.cbCategory.Location = new System.Drawing.Point(78, 57);
			this.cbCategory.Name = "cbCategory";
			this.cbCategory.Size = new System.Drawing.Size(351, 21);
			this.cbCategory.Sorted = true;
			this.cbCategory.TabIndex = 10;
			this.cbCategory.Visible = false;
			this.cbCategory.TextChanged += new System.EventHandler(this.ComboBoxTextChanged);
			this.cbCategory.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbCategoryKeyPress);
			// 
			// lblCategory
			// 
			this.lblCategory.AutoSize = true;
			this.lblCategory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblCategory.Location = new System.Drawing.Point(3, 54);
			this.lblCategory.Name = "lblCategory";
			this.lblCategory.Size = new System.Drawing.Size(69, 27);
			this.lblCategory.TabIndex = 9;
			this.lblCategory.Text = "Category:";
			this.lblCategory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblCategory.Visible = false;
			// 
			// cbCompany
			// 
			this.cbCompany.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tlpIssueEdit.SetColumnSpan(this.cbCompany, 2);
			this.cbCompany.FormattingEnabled = true;
			this.cbCompany.Location = new System.Drawing.Point(78, 3);
			this.cbCompany.Name = "cbCompany";
			this.cbCompany.Size = new System.Drawing.Size(351, 21);
			this.cbCompany.Sorted = true;
			this.cbCompany.TabIndex = 6;
			this.cbCompany.Visible = false;
			this.cbCompany.TextChanged += new System.EventHandler(this.ComboBoxTextChanged);
			this.cbCompany.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbCompanyKeyPress);
			// 
			// cbName
			// 
			this.cbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tlpIssueEdit.SetColumnSpan(this.cbName, 2);
			this.cbName.FormattingEnabled = true;
			this.cbName.Location = new System.Drawing.Point(78, 30);
			this.cbName.Name = "cbName";
			this.cbName.Size = new System.Drawing.Size(351, 21);
			this.cbName.Sorted = true;
			this.cbName.TabIndex = 7;
			this.cbName.TextChanged += new System.EventHandler(this.ComboBoxTextChanged);
			this.cbName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbNameKeyPress);
			// 
			// cbState
			// 
			this.cbState.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cbState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbState.FormattingEnabled = true;
			this.cbState.Location = new System.Drawing.Point(78, 84);
			this.cbState.Name = "cbState";
			this.cbState.Size = new System.Drawing.Size(271, 21);
			this.cbState.TabIndex = 7;
			this.cbState.TextChanged += new System.EventHandler(this.ComboBoxTextChanged);
			// 
			// btnIssues
			// 
			this.btnIssues.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(156)))), ((int)(((byte)(221)))));
			this.btnIssues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnIssues.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.btnIssues.Location = new System.Drawing.Point(362, 84);
			this.btnIssues.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.btnIssues.Name = "btnIssues";
			this.btnIssues.Size = new System.Drawing.Size(67, 23);
			this.btnIssues.TabIndex = 8;
			this.btnIssues.Text = "Issues";
			this.btnIssues.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnIssues.UseCustomBackColor = true;
			this.btnIssues.UseCustomForeColor = true;
			this.btnIssues.UseSelectable = true;
			this.btnIssues.Click += new System.EventHandler(this.btnIssues_Click);
			// 
			// panelHideTimer
			// 
			this.panelHideTimer.Interval = 300;
			this.panelHideTimer.Tick += new System.EventHandler(this.panelHideTimer_Tick);
			// 
			// formInactivationTimer
			// 
			this.formInactivationTimer.Interval = 1000;
			this.formInactivationTimer.Tick += new System.EventHandler(this.formInactivationTimer_Tick);
			// 
			// IssuePropsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(432, 111);
			this.Controls.Add(this.tlpIssueEdit);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "IssuePropsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "IssuePropsForm";
			this.Activated += new System.EventHandler(this.IssuePropsFormActivated);
			this.Deactivate += new System.EventHandler(this.IssuePropsFormDeactivate);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IssuePropsFormFormClosing);
			this.tlpIssueEdit.ResumeLayout(false);
			this.tlpIssueEdit.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private MetroFramework.Controls.MetroLabel lblCompany;
		private MetroFramework.Controls.MetroLabel lblName;
		private MetroFramework.Controls.MetroLabel lblState;
		private System.Windows.Forms.TableLayoutPanel tlpIssueEdit;
		private System.Windows.Forms.ComboBox cbCompany;
		private System.Windows.Forms.ComboBox cbName;
		private System.Windows.Forms.ComboBox cbState;
		private System.Windows.Forms.Timer panelHideTimer;
		private System.Windows.Forms.Timer formInactivationTimer;
		private MetroFramework.Controls.MetroButton btnIssues;
		private System.Windows.Forms.ComboBox cbCategory;
		private MetroFramework.Controls.MetroLabel lblCategory;
	}
}