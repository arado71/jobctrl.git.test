namespace Tct.ActivityRecorderClient.View
{
	partial class LoginForm
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
			this.btnSettings = new MetroFramework.Controls.MetroButton();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.lnkForgotPassword = new System.Windows.Forms.LinkLabel();
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.label1 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lblStartInGreen = new MetroFramework.Controls.MetroLabel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.cbStartInGreen = new MetroFramework.Controls.MetroCheckBox();
			this.lblUserId = new MetroFramework.Controls.MetroLabel();
			this.txtUserId = new MetroFramework.Controls.MetroTextBox();
			this.lblPassword = new MetroFramework.Controls.MetroLabel();
			this.txtPassword = new MetroFramework.Controls.MetroTextBox();
			this.lblLang = new MetroFramework.Controls.MetroLabel();
			this.cbLanguage = new MetroFramework.Controls.MetroComboBox();
			this.cbRememberMe = new MetroFramework.Controls.MetroCheckBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnSettings
			// 
			this.btnSettings.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.icon_settings;
			this.btnSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnSettings.Location = new System.Drawing.Point(318, 26);
			this.btnSettings.Name = "btnSettings";
			this.btnSettings.Size = new System.Drawing.Size(26, 26);
			this.btnSettings.TabIndex = 8;
			this.btnSettings.UseSelectable = true;
			this.btnSettings.Click += new System.EventHandler(this.HandleSettingsClicked);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53F));
			this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblUserId, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.txtUserId, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblPassword, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.txtPassword, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblLang, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.cbLanguage, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.cbRememberMe, 1, 3);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(23, 63);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(325, 299);
			this.tableLayoutPanel1.TabIndex = 13;
			// 
			// panel2
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.panel2, 2);
			this.panel2.Controls.Add(this.lnkForgotPassword);
			this.panel2.Controls.Add(this.btnOk);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(3, 211);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(319, 73);
			this.panel2.TabIndex = 16;
			// 
			// lnkForgotPassword
			// 
			this.lnkForgotPassword.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(100)))), ((int)(((byte)(150)))));
			this.lnkForgotPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lnkForgotPassword.AutoSize = true;
			this.lnkForgotPassword.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lnkForgotPassword.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(156)))), ((int)(((byte)(221)))));
			this.lnkForgotPassword.Location = new System.Drawing.Point(0, 58);
			this.lnkForgotPassword.Margin = new System.Windows.Forms.Padding(0);
			this.lnkForgotPassword.Name = "lnkForgotPassword";
			this.lnkForgotPassword.Size = new System.Drawing.Size(114, 15);
			this.lnkForgotPassword.TabIndex = 7;
			this.lnkForgotPassword.TabStop = true;
			this.lnkForgotPassword.Text = "Elfelejtette jelszavát?";
			this.lnkForgotPassword.Click += new System.EventHandler(this.lnkForgotPassword_Click);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(156)))), ((int)(((byte)(221)))));
			this.btnOk.FontSize = MetroFramework.MetroButtonSize.Tall;
			this.btnOk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.btnOk.Location = new System.Drawing.Point(116, 16);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(86, 35);
			this.btnOk.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnOk.TabIndex = 6;
			this.btnOk.Text = "OK";
			this.btnOk.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnOk.UseCustomBackColor = true;
			this.btnOk.UseCustomForeColor = true;
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(179)))), ((int)(((byte)(179)))));
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.ForeColor = System.Drawing.Color.Gray;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(319, 2);
			this.label1.TabIndex = 13;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.lblStartInGreen);
			this.panel1.Controls.Add(this.panel3);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(157, 165);
			this.panel1.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(165, 40);
			this.panel1.TabIndex = 4;
			// 
			// lblStartInGreen
			// 
			this.lblStartInGreen.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblStartInGreen.ForeColor = System.Drawing.Color.Gray;
			this.lblStartInGreen.Location = new System.Drawing.Point(16, 0);
			this.lblStartInGreen.Margin = new System.Windows.Forms.Padding(0);
			this.lblStartInGreen.Name = "lblStartInGreen";
			this.lblStartInGreen.Size = new System.Drawing.Size(149, 46);
			this.lblStartInGreen.TabIndex = 11;
			this.lblStartInGreen.Text = "automatikus indítás munkastátuszban";
			this.lblStartInGreen.UseCustomForeColor = true;
			this.lblStartInGreen.WrapToLine = true;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.cbStartInGreen);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(16, 40);
			this.panel3.TabIndex = 12;
			// 
			// cbStartInGreen
			// 
			this.cbStartInGreen.Checked = true;
			this.cbStartInGreen.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbStartInGreen.Dock = System.Windows.Forms.DockStyle.Top;
			this.cbStartInGreen.FontSize = MetroFramework.MetroCheckBoxSize.Medium;
			this.cbStartInGreen.FontWeight = MetroFramework.MetroCheckBoxWeight.Light;
			this.cbStartInGreen.Location = new System.Drawing.Point(0, 0);
			this.cbStartInGreen.Name = "cbStartInGreen";
			this.cbStartInGreen.Size = new System.Drawing.Size(16, 16);
			this.cbStartInGreen.TabIndex = 5;
			this.cbStartInGreen.UseSelectable = true;
			// 
			// lblUserId
			// 
			this.lblUserId.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblUserId.ForeColor = System.Drawing.Color.Gray;
			this.lblUserId.Location = new System.Drawing.Point(3, 0);
			this.lblUserId.Name = "lblUserId";
			this.lblUserId.Size = new System.Drawing.Size(146, 46);
			this.lblUserId.TabIndex = 2;
			this.lblUserId.Text = "User ID / E-mail";
			this.lblUserId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblUserId.UseCustomForeColor = true;
			this.lblUserId.WrapToLine = true;
			// 
			// txtUserId
			// 
			this.txtUserId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtUserId.FontSize = MetroFramework.MetroTextBoxSize.Tall;
			this.txtUserId.Lines = new string[] {
        "user"};
			this.txtUserId.Location = new System.Drawing.Point(157, 5);
			this.txtUserId.Margin = new System.Windows.Forms.Padding(5);
			this.txtUserId.MaxLength = 32767;
			this.txtUserId.Name = "txtUserId";
			this.txtUserId.PasswordChar = '\0';
			this.txtUserId.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.txtUserId.SelectedText = "";
			this.txtUserId.Size = new System.Drawing.Size(163, 36);
			this.txtUserId.TabIndex = 0;
			this.txtUserId.Text = "user";
			this.txtUserId.UseSelectable = true;
			this.txtUserId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUserId_KeyPress);
			// 
			// lblPassword
			// 
			this.lblPassword.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblPassword.ForeColor = System.Drawing.Color.Gray;
			this.lblPassword.Location = new System.Drawing.Point(3, 46);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(146, 46);
			this.lblPassword.TabIndex = 4;
			this.lblPassword.Text = "Jelszó";
			this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblPassword.UseCustomForeColor = true;
			this.lblPassword.WrapToLine = true;
			// 
			// txtPassword
			// 
			this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPassword.FontSize = MetroFramework.MetroTextBoxSize.Tall;
			this.txtPassword.Lines = new string[0];
			this.txtPassword.Location = new System.Drawing.Point(157, 51);
			this.txtPassword.Margin = new System.Windows.Forms.Padding(5);
			this.txtPassword.MaxLength = 32767;
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '●';
			this.txtPassword.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.txtPassword.SelectedText = "";
			this.txtPassword.Size = new System.Drawing.Size(163, 36);
			this.txtPassword.TabIndex = 1;
			this.txtPassword.UseSelectable = true;
			this.txtPassword.UseSystemPasswordChar = true;
			this.txtPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPassword_KeyPress);
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			// 
			// lblLang
			// 
			this.lblLang.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblLang.ForeColor = System.Drawing.Color.Gray;
			this.lblLang.Location = new System.Drawing.Point(3, 92);
			this.lblLang.Name = "lblLang";
			this.lblLang.Size = new System.Drawing.Size(146, 45);
			this.lblLang.TabIndex = 6;
			this.lblLang.Text = "Nyelv";
			this.lblLang.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblLang.UseCustomForeColor = true;
			this.lblLang.WrapToLine = true;
			// 
			// cbLanguage
			// 
			this.cbLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbLanguage.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cbLanguage.FontSize = MetroFramework.MetroComboBoxSize.Tall;
			this.cbLanguage.FormattingEnabled = true;
			this.cbLanguage.ItemHeight = 29;
			this.cbLanguage.Location = new System.Drawing.Point(157, 97);
			this.cbLanguage.Margin = new System.Windows.Forms.Padding(5);
			this.cbLanguage.Name = "cbLanguage";
			this.cbLanguage.Size = new System.Drawing.Size(163, 35);
			this.cbLanguage.TabIndex = 2;
			this.cbLanguage.UseSelectable = true;
			// 
			// cbRememberMe
			// 
			this.cbRememberMe.Checked = true;
			this.cbRememberMe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbRememberMe.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbRememberMe.FontSize = MetroFramework.MetroCheckBoxSize.Medium;
			this.cbRememberMe.FontWeight = MetroFramework.MetroCheckBoxWeight.Light;
			this.cbRememberMe.ForeColor = System.Drawing.Color.Gray;
			this.cbRememberMe.Location = new System.Drawing.Point(157, 140);
			this.cbRememberMe.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
			this.cbRememberMe.Name = "cbRememberMe";
			this.cbRememberMe.Size = new System.Drawing.Size(165, 19);
			this.cbRememberMe.TabIndex = 3;
			this.cbRememberMe.Text = "jegyezzen meg";
			this.cbRememberMe.UseCustomForeColor = true;
			this.cbRememberMe.UseSelectable = true;
			this.cbRememberMe.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbRememberMe_KeyPress);
			// 
			// LoginForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(370, 370);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.btnSettings);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LoginForm";
			this.Resizable = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "JobCTRL - Login";
			this.TopMost = true;
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private MetroFramework.Controls.MetroButton btnSettings;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private MetroFramework.Controls.MetroLabel lblUserId;
		private MetroFramework.Controls.MetroTextBox txtUserId;
		private MetroFramework.Controls.MetroLabel lblPassword;
		private MetroFramework.Controls.MetroTextBox txtPassword;
		private MetroFramework.Controls.MetroLabel lblLang;
		private MetroFramework.Controls.MetroComboBox cbLanguage;
		private MetroFramework.Controls.MetroCheckBox cbRememberMe;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.LinkLabel lnkForgotPassword;
		private MetroFramework.Controls.MetroButton btnOk;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel1;
		private MetroFramework.Controls.MetroLabel lblStartInGreen;
		private System.Windows.Forms.Panel panel3;
		private MetroFramework.Controls.MetroCheckBox cbStartInGreen;
	}
}