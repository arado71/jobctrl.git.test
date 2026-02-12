namespace Tct.ActivityRecorderClient.View
{
	partial class WorkDetectorRuleEditForm
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
			this.lblTitleRule = new System.Windows.Forms.Label();
			this.txtTitleRule = new System.Windows.Forms.TextBox();
			this.txtProcessRule = new System.Windows.Forms.TextBox();
			this.lblProcessRule = new System.Windows.Forms.Label();
			this.txtUrlRule = new System.Windows.Forms.TextBox();
			this.lblUrlRule = new System.Windows.Forms.Label();
			this.cbRuleType = new System.Windows.Forms.ComboBox();
			this.lblRuleType = new System.Windows.Forms.Label();
			this.lblWork = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.cbPermanent = new System.Windows.Forms.CheckBox();
			this.cbRegex = new System.Windows.Forms.CheckBox();
			this.cbIgnoreCase = new System.Windows.Forms.CheckBox();
			this.cbEnabled = new System.Windows.Forms.CheckBox();
			this.cbCategories = new System.Windows.Forms.ComboBox();
			this.lblCategory = new System.Windows.Forms.Label();
			this.splitAdvanced = new System.Windows.Forms.SplitContainer();
			this.cbWindowScope = new System.Windows.Forms.ComboBox();
			this.lblWindowScope = new System.Windows.Forms.Label();
			this.btnEditPlugins = new System.Windows.Forms.Button();
			this.cbWorks = new Tct.ActivityRecorderClient.View.WorkSelectorComboBox();
			this.cbOkValid = new System.Windows.Forms.ComboBox();
			this.btnDontShowAgain = new System.Windows.Forms.Button();
			this.cbDontShow = new System.Windows.Forms.ComboBox();
			this.cbAdvancedView = new System.Windows.Forms.CheckBox();
			this.lblHelp = new System.Windows.Forms.Label();
			this.btnWindowRules = new System.Windows.Forms.Button();
			this.splitAdvanced.Panel1.SuspendLayout();
			this.splitAdvanced.Panel2.SuspendLayout();
			this.splitAdvanced.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblTitleRule
			// 
			this.lblTitleRule.Location = new System.Drawing.Point(12, 6);
			this.lblTitleRule.Name = "lblTitleRule";
			this.lblTitleRule.Size = new System.Drawing.Size(114, 13);
			this.lblTitleRule.TabIndex = 0;
			this.lblTitleRule.Text = "Cimsor szabaly:";
			this.lblTitleRule.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtTitleRule
			// 
			this.txtTitleRule.Location = new System.Drawing.Point(132, 3);
			this.txtTitleRule.Name = "txtTitleRule";
			this.txtTitleRule.Size = new System.Drawing.Size(262, 20);
			this.txtTitleRule.TabIndex = 1;
			// 
			// txtProcessRule
			// 
			this.txtProcessRule.Location = new System.Drawing.Point(132, 29);
			this.txtProcessRule.Name = "txtProcessRule";
			this.txtProcessRule.Size = new System.Drawing.Size(262, 20);
			this.txtProcessRule.TabIndex = 3;
			// 
			// lblProcessRule
			// 
			this.lblProcessRule.Location = new System.Drawing.Point(12, 32);
			this.lblProcessRule.Name = "lblProcessRule";
			this.lblProcessRule.Size = new System.Drawing.Size(114, 13);
			this.lblProcessRule.TabIndex = 2;
			this.lblProcessRule.Text = "Processz szabaly:";
			this.lblProcessRule.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtUrlRule
			// 
			this.txtUrlRule.Location = new System.Drawing.Point(132, 55);
			this.txtUrlRule.Name = "txtUrlRule";
			this.txtUrlRule.Size = new System.Drawing.Size(262, 20);
			this.txtUrlRule.TabIndex = 5;
			// 
			// lblUrlRule
			// 
			this.lblUrlRule.Location = new System.Drawing.Point(12, 58);
			this.lblUrlRule.Name = "lblUrlRule";
			this.lblUrlRule.Size = new System.Drawing.Size(114, 13);
			this.lblUrlRule.TabIndex = 4;
			this.lblUrlRule.Text = "URL szabaly:";
			this.lblUrlRule.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cbRuleType
			// 
			this.cbRuleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbRuleType.FormattingEnabled = true;
			this.cbRuleType.Location = new System.Drawing.Point(132, 107);
			this.cbRuleType.Name = "cbRuleType";
			this.cbRuleType.Size = new System.Drawing.Size(262, 21);
			this.cbRuleType.TabIndex = 6;
			this.cbRuleType.SelectedValueChanged += new System.EventHandler(this.cbRuleType_SelectedValueChanged);
			// 
			// lblRuleType
			// 
			this.lblRuleType.Location = new System.Drawing.Point(12, 110);
			this.lblRuleType.Name = "lblRuleType";
			this.lblRuleType.Size = new System.Drawing.Size(114, 13);
			this.lblRuleType.TabIndex = 7;
			this.lblRuleType.Text = "Szabaly tipusa:";
			this.lblRuleType.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblWork
			// 
			this.lblWork.Location = new System.Drawing.Point(12, 6);
			this.lblWork.Name = "lblWork";
			this.lblWork.Size = new System.Drawing.Size(114, 13);
			this.lblWork.TabIndex = 8;
			this.lblWork.Text = "Munka:";
			this.lblWork.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(416, 34);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 12;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(527, 34);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 11;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// cbPermanent
			// 
			this.cbPermanent.AutoSize = true;
			this.cbPermanent.Location = new System.Drawing.Point(419, 5);
			this.cbPermanent.Name = "cbPermanent";
			this.cbPermanent.Size = new System.Drawing.Size(70, 17);
			this.cbPermanent.TabIndex = 13;
			this.cbPermanent.Text = "Vegleges";
			this.cbPermanent.UseVisualStyleBackColor = true;
			// 
			// cbRegex
			// 
			this.cbRegex.AutoSize = true;
			this.cbRegex.Location = new System.Drawing.Point(419, 31);
			this.cbRegex.Name = "cbRegex";
			this.cbRegex.Size = new System.Drawing.Size(70, 17);
			this.cbRegex.TabIndex = 14;
			this.cbRegex.Text = "Regularis";
			this.cbRegex.UseVisualStyleBackColor = true;
			// 
			// cbIgnoreCase
			// 
			this.cbIgnoreCase.AutoSize = true;
			this.cbIgnoreCase.Location = new System.Drawing.Point(419, 57);
			this.cbIgnoreCase.Name = "cbIgnoreCase";
			this.cbIgnoreCase.Size = new System.Drawing.Size(148, 17);
			this.cbIgnoreCase.TabIndex = 15;
			this.cbIgnoreCase.Text = "Kis/nagybetu megegyezik";
			this.cbIgnoreCase.UseVisualStyleBackColor = true;
			// 
			// cbEnabled
			// 
			this.cbEnabled.AutoSize = true;
			this.cbEnabled.Location = new System.Drawing.Point(419, 83);
			this.cbEnabled.Name = "cbEnabled";
			this.cbEnabled.Size = new System.Drawing.Size(50, 17);
			this.cbEnabled.TabIndex = 16;
			this.cbEnabled.Text = "Aktiv";
			this.cbEnabled.UseVisualStyleBackColor = true;
			// 
			// cbCategories
			// 
			this.cbCategories.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbCategories.FormattingEnabled = true;
			this.cbCategories.Location = new System.Drawing.Point(132, 3);
			this.cbCategories.Name = "cbCategories";
			this.cbCategories.Size = new System.Drawing.Size(470, 21);
			this.cbCategories.TabIndex = 17;
			this.cbCategories.Visible = false;
			// 
			// lblCategory
			// 
			this.lblCategory.Location = new System.Drawing.Point(12, 6);
			this.lblCategory.Name = "lblCategory";
			this.lblCategory.Size = new System.Drawing.Size(114, 13);
			this.lblCategory.TabIndex = 18;
			this.lblCategory.Text = "Munka:";
			this.lblCategory.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// splitAdvanced
			// 
			this.splitAdvanced.IsSplitterFixed = true;
			this.splitAdvanced.Location = new System.Drawing.Point(7, 87);
			this.splitAdvanced.Margin = new System.Windows.Forms.Padding(0);
			this.splitAdvanced.Name = "splitAdvanced";
			this.splitAdvanced.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitAdvanced.Panel1
			// 
			this.splitAdvanced.Panel1.Controls.Add(this.btnWindowRules);
			this.splitAdvanced.Panel1.Controls.Add(this.cbWindowScope);
			this.splitAdvanced.Panel1.Controls.Add(this.lblWindowScope);
			this.splitAdvanced.Panel1.Controls.Add(this.btnEditPlugins);
			this.splitAdvanced.Panel1.Controls.Add(this.lblProcessRule);
			this.splitAdvanced.Panel1.Controls.Add(this.lblTitleRule);
			this.splitAdvanced.Panel1.Controls.Add(this.cbEnabled);
			this.splitAdvanced.Panel1.Controls.Add(this.txtTitleRule);
			this.splitAdvanced.Panel1.Controls.Add(this.cbIgnoreCase);
			this.splitAdvanced.Panel1.Controls.Add(this.txtProcessRule);
			this.splitAdvanced.Panel1.Controls.Add(this.cbRegex);
			this.splitAdvanced.Panel1.Controls.Add(this.lblUrlRule);
			this.splitAdvanced.Panel1.Controls.Add(this.cbPermanent);
			this.splitAdvanced.Panel1.Controls.Add(this.txtUrlRule);
			this.splitAdvanced.Panel1.Controls.Add(this.cbRuleType);
			this.splitAdvanced.Panel1.Controls.Add(this.lblRuleType);
			this.splitAdvanced.Panel1MinSize = 130;
			// 
			// splitAdvanced.Panel2
			// 
			this.splitAdvanced.Panel2.Controls.Add(this.cbWorks);
			this.splitAdvanced.Panel2.Controls.Add(this.cbOkValid);
			this.splitAdvanced.Panel2.Controls.Add(this.btnDontShowAgain);
			this.splitAdvanced.Panel2.Controls.Add(this.cbDontShow);
			this.splitAdvanced.Panel2.Controls.Add(this.cbAdvancedView);
			this.splitAdvanced.Panel2.Controls.Add(this.lblCategory);
			this.splitAdvanced.Panel2.Controls.Add(this.cbCategories);
			this.splitAdvanced.Panel2.Controls.Add(this.btnOk);
			this.splitAdvanced.Panel2.Controls.Add(this.lblWork);
			this.splitAdvanced.Panel2.Controls.Add(this.btnCancel);
			this.splitAdvanced.Panel2MinSize = 62;
			this.splitAdvanced.Size = new System.Drawing.Size(624, 196);
			this.splitAdvanced.SplitterDistance = 130;
			this.splitAdvanced.SplitterWidth = 1;
			this.splitAdvanced.TabIndex = 21;
			// 
			// cbWindowScope
			// 
			this.cbWindowScope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbWindowScope.FormattingEnabled = true;
			this.cbWindowScope.Location = new System.Drawing.Point(132, 81);
			this.cbWindowScope.Name = "cbWindowScope";
			this.cbWindowScope.Size = new System.Drawing.Size(262, 21);
			this.cbWindowScope.TabIndex = 18;
			// 
			// lblWindowScope
			// 
			this.lblWindowScope.Location = new System.Drawing.Point(12, 84);
			this.lblWindowScope.Name = "lblWindowScope";
			this.lblWindowScope.Size = new System.Drawing.Size(114, 13);
			this.lblWindowScope.TabIndex = 19;
			this.lblWindowScope.Text = "Ablak allapota:";
			this.lblWindowScope.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// btnEditPlugins
			// 
			this.btnEditPlugins.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.btnEditPlugins.Location = new System.Drawing.Point(579, 3);
			this.btnEditPlugins.Name = "btnEditPlugins";
			this.btnEditPlugins.Size = new System.Drawing.Size(15, 15);
			this.btnEditPlugins.TabIndex = 17;
			this.btnEditPlugins.Text = "P";
			this.btnEditPlugins.UseVisualStyleBackColor = true;
			this.btnEditPlugins.Click += new System.EventHandler(this.btnEditPlugins_Click);
			// 
			// cbWorks
			// 
			this.cbWorks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cbWorks.DropDownWidth = 470;
			this.cbWorks.FormattingEnabled = true;
			this.cbWorks.IntegralHeight = false;
			this.cbWorks.Location = new System.Drawing.Point(132, 3);
			this.cbWorks.MaxDropDownItems = 30;
			this.cbWorks.Name = "cbWorks";
			this.cbWorks.Size = new System.Drawing.Size(470, 21);
			this.cbWorks.TabIndex = 23;
			// 
			// cbOkValid
			// 
			this.cbOkValid.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbOkValid.FormattingEnabled = true;
			this.cbOkValid.Location = new System.Drawing.Point(484, 35);
			this.cbOkValid.Name = "cbOkValid";
			this.cbOkValid.Size = new System.Drawing.Size(37, 21);
			this.cbOkValid.TabIndex = 22;
			// 
			// btnDontShowAgain
			// 
			this.btnDontShowAgain.Location = new System.Drawing.Point(350, 34);
			this.btnDontShowAgain.Name = "btnDontShowAgain";
			this.btnDontShowAgain.Size = new System.Drawing.Size(128, 23);
			this.btnDontShowAgain.TabIndex = 21;
			this.btnDontShowAgain.Text = "Ne kerdezzen ra";
			this.btnDontShowAgain.UseVisualStyleBackColor = true;
			this.btnDontShowAgain.Click += new System.EventHandler(this.btnDontShowAgain_Click);
			// 
			// cbDontShow
			// 
			this.cbDontShow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbDontShow.FormattingEnabled = true;
			this.cbDontShow.Location = new System.Drawing.Point(250, 35);
			this.cbDontShow.Name = "cbDontShow";
			this.cbDontShow.Size = new System.Drawing.Size(94, 21);
			this.cbDontShow.TabIndex = 20;
			// 
			// cbAdvancedView
			// 
			this.cbAdvancedView.AutoSize = true;
			this.cbAdvancedView.Location = new System.Drawing.Point(131, 36);
			this.cbAdvancedView.Name = "cbAdvancedView";
			this.cbAdvancedView.Size = new System.Drawing.Size(113, 17);
			this.cbAdvancedView.TabIndex = 19;
			this.cbAdvancedView.Text = "Halado beallitasok";
			this.cbAdvancedView.UseVisualStyleBackColor = true;
			this.cbAdvancedView.CheckedChanged += new System.EventHandler(this.cbAdvancedView_CheckedChanged);
			// 
			// lblHelp
			// 
			this.lblHelp.Location = new System.Drawing.Point(28, 59);
			this.lblHelp.Name = "lblHelp";
			this.lblHelp.Size = new System.Drawing.Size(600, 27);
			this.lblHelp.TabIndex = 22;
			this.lblHelp.Text = "Kerem adja meg a letrehozando szabaly parametereit";
			// 
			// btnWindowRules
			// 
			this.btnWindowRules.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.btnWindowRules.Location = new System.Drawing.Point(579, 29);
			this.btnWindowRules.Name = "btnWindowRules";
			this.btnWindowRules.Size = new System.Drawing.Size(15, 15);
			this.btnWindowRules.TabIndex = 20;
			this.btnWindowRules.Text = "W";
			this.btnWindowRules.UseVisualStyleBackColor = true;
			this.btnWindowRules.Click += new System.EventHandler(this.btnWindowRules_Click);
			// 
			// WorkDetectorRuleEditForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(640, 290);
			this.Controls.Add(this.lblHelp);
			this.Controls.Add(this.splitAdvanced);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(640, 290);
			this.MinimumSize = new System.Drawing.Size(640, 290);
			this.Name = "WorkDetectorRuleEditForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Automatikus szabaly letrehozasa";
			this.splitAdvanced.Panel1.ResumeLayout(false);
			this.splitAdvanced.Panel1.PerformLayout();
			this.splitAdvanced.Panel2.ResumeLayout(false);
			this.splitAdvanced.Panel2.PerformLayout();
			this.splitAdvanced.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblTitleRule;
		private System.Windows.Forms.TextBox txtTitleRule;
		private System.Windows.Forms.TextBox txtProcessRule;
		private System.Windows.Forms.Label lblProcessRule;
		private System.Windows.Forms.TextBox txtUrlRule;
		private System.Windows.Forms.Label lblUrlRule;
		private System.Windows.Forms.ComboBox cbRuleType;
		private System.Windows.Forms.Label lblRuleType;
		private System.Windows.Forms.Label lblWork;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.CheckBox cbPermanent;
		private System.Windows.Forms.CheckBox cbRegex;
		private System.Windows.Forms.CheckBox cbIgnoreCase;
		private System.Windows.Forms.CheckBox cbEnabled;
		private System.Windows.Forms.ComboBox cbCategories;
		private System.Windows.Forms.Label lblCategory;
		private System.Windows.Forms.SplitContainer splitAdvanced;
		private System.Windows.Forms.CheckBox cbAdvancedView;
		private System.Windows.Forms.Label lblHelp;
		private System.Windows.Forms.Button btnDontShowAgain;
		private System.Windows.Forms.ComboBox cbDontShow;
		private System.Windows.Forms.Button btnEditPlugins;
		private System.Windows.Forms.ComboBox cbOkValid;
		private System.Windows.Forms.ComboBox cbWindowScope;
		private System.Windows.Forms.Label lblWindowScope;
		private WorkSelectorComboBox cbWorks;
		private System.Windows.Forms.Button btnWindowRules;
	}
}