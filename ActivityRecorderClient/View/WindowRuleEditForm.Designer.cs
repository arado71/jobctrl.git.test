namespace Tct.ActivityRecorderClient.View
{
	partial class WindowRuleEditForm
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
			this.cbWindowScope = new System.Windows.Forms.ComboBox();
			this.lblWindowScope = new System.Windows.Forms.Label();
			this.btnEditPlugins = new System.Windows.Forms.Button();
			this.lblProcessRule = new System.Windows.Forms.Label();
			this.lblTitleRule = new System.Windows.Forms.Label();
			this.cbEnabled = new System.Windows.Forms.CheckBox();
			this.txtTitleRule = new System.Windows.Forms.TextBox();
			this.cbIgnoreCase = new System.Windows.Forms.CheckBox();
			this.txtProcessRule = new System.Windows.Forms.TextBox();
			this.cbRegex = new System.Windows.Forms.CheckBox();
			this.lblUrlRule = new System.Windows.Forms.Label();
			this.txtUrlRule = new System.Windows.Forms.TextBox();
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.btnCancel = new MetroFramework.Controls.MetroButton();
			this.lbWinRules = new System.Windows.Forms.ListBox();
			this.lblWinRules = new System.Windows.Forms.Label();
			this.btnAdd = new MetroFramework.Controls.MetroButton();
			this.btnRemove = new MetroFramework.Controls.MetroButton();
			this.SuspendLayout();
			// 
			// cbWindowScope
			// 
			this.cbWindowScope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbWindowScope.FormattingEnabled = true;
			this.cbWindowScope.Location = new System.Drawing.Point(143, 265);
			this.cbWindowScope.Name = "cbWindowScope";
			this.cbWindowScope.Size = new System.Drawing.Size(262, 21);
			this.cbWindowScope.TabIndex = 33;
			// 
			// lblWindowScope
			// 
			this.lblWindowScope.Location = new System.Drawing.Point(23, 268);
			this.lblWindowScope.Name = "lblWindowScope";
			this.lblWindowScope.Size = new System.Drawing.Size(114, 13);
			this.lblWindowScope.TabIndex = 34;
			this.lblWindowScope.Text = "Ablak allapota:";
			this.lblWindowScope.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// btnEditPlugins
			// 
			this.btnEditPlugins.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.btnEditPlugins.Location = new System.Drawing.Point(590, 187);
			this.btnEditPlugins.Name = "btnEditPlugins";
			this.btnEditPlugins.Size = new System.Drawing.Size(15, 15);
			this.btnEditPlugins.TabIndex = 32;
			this.btnEditPlugins.Text = "7";
			this.btnEditPlugins.UseVisualStyleBackColor = true;
			this.btnEditPlugins.Click += new System.EventHandler(this.btnEditPlugins_Click);
			// 
			// lblProcessRule
			// 
			this.lblProcessRule.Location = new System.Drawing.Point(23, 216);
			this.lblProcessRule.Name = "lblProcessRule";
			this.lblProcessRule.Size = new System.Drawing.Size(114, 13);
			this.lblProcessRule.TabIndex = 22;
			this.lblProcessRule.Text = "Processz szabaly:";
			this.lblProcessRule.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblTitleRule
			// 
			this.lblTitleRule.Location = new System.Drawing.Point(23, 190);
			this.lblTitleRule.Name = "lblTitleRule";
			this.lblTitleRule.Size = new System.Drawing.Size(114, 13);
			this.lblTitleRule.TabIndex = 20;
			this.lblTitleRule.Text = "Cimsor szabaly:";
			this.lblTitleRule.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cbEnabled
			// 
			this.cbEnabled.AutoSize = true;
			this.cbEnabled.Location = new System.Drawing.Point(430, 267);
			this.cbEnabled.Name = "cbEnabled";
			this.cbEnabled.Size = new System.Drawing.Size(50, 17);
			this.cbEnabled.TabIndex = 10;
			this.cbEnabled.Text = "Aktiv";
			this.cbEnabled.UseVisualStyleBackColor = true;
			// 
			// txtTitleRule
			// 
			this.txtTitleRule.Location = new System.Drawing.Point(143, 187);
			this.txtTitleRule.Name = "txtTitleRule";
			this.txtTitleRule.Size = new System.Drawing.Size(262, 20);
			this.txtTitleRule.TabIndex = 3;
			// 
			// cbIgnoreCase
			// 
			this.cbIgnoreCase.AutoSize = true;
			this.cbIgnoreCase.Location = new System.Drawing.Point(430, 241);
			this.cbIgnoreCase.Name = "cbIgnoreCase";
			this.cbIgnoreCase.Size = new System.Drawing.Size(32, 17);
			this.cbIgnoreCase.TabIndex = 30;
			this.cbIgnoreCase.Text = "9";
			this.cbIgnoreCase.UseVisualStyleBackColor = true;
			// 
			// txtProcessRule
			// 
			this.txtProcessRule.Location = new System.Drawing.Point(143, 213);
			this.txtProcessRule.Name = "txtProcessRule";
			this.txtProcessRule.Size = new System.Drawing.Size(262, 20);
			this.txtProcessRule.TabIndex = 4;
			// 
			// cbRegex
			// 
			this.cbRegex.AutoSize = true;
			this.cbRegex.Location = new System.Drawing.Point(430, 215);
			this.cbRegex.Name = "cbRegex";
			this.cbRegex.Size = new System.Drawing.Size(32, 17);
			this.cbRegex.TabIndex = 29;
			this.cbRegex.Text = "8";
			this.cbRegex.UseVisualStyleBackColor = true;
			// 
			// lblUrlRule
			// 
			this.lblUrlRule.Location = new System.Drawing.Point(23, 242);
			this.lblUrlRule.Name = "lblUrlRule";
			this.lblUrlRule.Size = new System.Drawing.Size(114, 13);
			this.lblUrlRule.TabIndex = 24;
			this.lblUrlRule.Text = "URL szabaly:";
			this.lblUrlRule.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtUrlRule
			// 
			this.txtUrlRule.Location = new System.Drawing.Point(143, 239);
			this.txtUrlRule.Name = "txtUrlRule";
			this.txtUrlRule.Size = new System.Drawing.Size(262, 20);
			this.txtUrlRule.TabIndex = 5;
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(418, 312);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(79, 23);
			this.btnOk.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnOk.TabIndex = 11;
			this.btnOk.Text = "OK";
			this.btnOk.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(499, 312);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(79, 23);
			this.btnCancel.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnCancel.TabIndex = 12;
			this.btnCancel.Text = "Mégse";
			this.btnCancel.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnCancel.UseSelectable = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// lbWinRules
			// 
			this.lbWinRules.FormattingEnabled = true;
			this.lbWinRules.Location = new System.Drawing.Point(143, 73);
			this.lbWinRules.Name = "lbWinRules";
			this.lbWinRules.Size = new System.Drawing.Size(320, 95);
			this.lbWinRules.TabIndex = 0;
			// 
			// lblWinRules
			// 
			this.lblWinRules.Location = new System.Drawing.Point(23, 73);
			this.lblWinRules.Name = "lblWinRules";
			this.lblWinRules.Size = new System.Drawing.Size(114, 13);
			this.lblWinRules.TabIndex = 38;
			this.lblWinRules.Text = "Egyeb ablakok:";
			this.lblWinRules.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// btnAdd
			// 
			this.btnAdd.Location = new System.Drawing.Point(499, 73);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(79, 23);
			this.btnAdd.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnAdd.TabIndex = 1;
			this.btnAdd.Text = "+";
			this.btnAdd.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnAdd.UseSelectable = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// btnRemove
			// 
			this.btnRemove.Location = new System.Drawing.Point(499, 112);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(79, 23);
			this.btnRemove.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnRemove.TabIndex = 2;
			this.btnRemove.Text = "-";
			this.btnRemove.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnRemove.UseSelectable = true;
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// WindowRuleEditForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(640, 363);
			this.Controls.Add(this.btnRemove);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.lblWinRules);
			this.Controls.Add(this.lbWinRules);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.cbWindowScope);
			this.Controls.Add(this.lblWindowScope);
			this.Controls.Add(this.btnEditPlugins);
			this.Controls.Add(this.lblProcessRule);
			this.Controls.Add(this.lblTitleRule);
			this.Controls.Add(this.cbEnabled);
			this.Controls.Add(this.txtTitleRule);
			this.Controls.Add(this.cbIgnoreCase);
			this.Controls.Add(this.txtProcessRule);
			this.Controls.Add(this.cbRegex);
			this.Controls.Add(this.lblUrlRule);
			this.Controls.Add(this.txtUrlRule);
			this.Name = "WindowRuleEditForm";
			this.Text = "10";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox cbWindowScope;
		private System.Windows.Forms.Label lblWindowScope;
		private System.Windows.Forms.Button btnEditPlugins;
		private System.Windows.Forms.Label lblProcessRule;
		private System.Windows.Forms.Label lblTitleRule;
		private System.Windows.Forms.CheckBox cbEnabled;
		private System.Windows.Forms.TextBox txtTitleRule;
		private System.Windows.Forms.CheckBox cbIgnoreCase;
		private System.Windows.Forms.TextBox txtProcessRule;
		private System.Windows.Forms.CheckBox cbRegex;
		private System.Windows.Forms.Label lblUrlRule;
		private System.Windows.Forms.TextBox txtUrlRule;
		private MetroFramework.Controls.MetroButton btnOk;
		private MetroFramework.Controls.MetroButton btnCancel;
		private System.Windows.Forms.ListBox lbWinRules;
		private System.Windows.Forms.Label lblWinRules;
		private MetroFramework.Controls.MetroButton btnAdd;
		private MetroFramework.Controls.MetroButton btnRemove;
	}
}