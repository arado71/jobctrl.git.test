namespace Tct.ActivityRecorderClient.View
{
	partial class LoginSettings
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
			this.rbAutomatic = new MetroFramework.Controls.MetroRadioButton();
			this.rbCustom = new MetroFramework.Controls.MetroRadioButton();
			this.lblAddress = new MetroFramework.Controls.MetroLabel();
			this.tbAddress = new MetroFramework.Controls.MetroTextBox();
			this.lblUsername = new MetroFramework.Controls.MetroLabel();
			this.tbUsername = new MetroFramework.Controls.MetroTextBox();
			this.lblPassword = new MetroFramework.Controls.MetroLabel();
			this.tbPassword = new MetroFramework.Controls.MetroTextBox();
			this.btnSave = new MetroFramework.Controls.MetroButton();
			this.pCustom = new System.Windows.Forms.Panel();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			this.pCustom.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// rbAutomatic
			// 
			this.rbAutomatic.AutoSize = true;
			this.rbAutomatic.Checked = true;
			this.rbAutomatic.Location = new System.Drawing.Point(23, 63);
			this.rbAutomatic.Name = "rbAutomatic";
			this.rbAutomatic.Size = new System.Drawing.Size(79, 15);
			this.rbAutomatic.TabIndex = 0;
			this.rbAutomatic.TabStop = true;
			this.rbAutomatic.Text = "Automatic";
			this.rbAutomatic.UseSelectable = true;
			this.rbAutomatic.CheckedChanged += new System.EventHandler(this.HandleProxyModeChanged);
			// 
			// rbCustom
			// 
			this.rbCustom.AutoSize = true;
			this.rbCustom.Location = new System.Drawing.Point(23, 84);
			this.rbCustom.Name = "rbCustom";
			this.rbCustom.Size = new System.Drawing.Size(65, 15);
			this.rbCustom.TabIndex = 1;
			this.rbCustom.Text = "Custom";
			this.rbCustom.UseSelectable = true;
			// 
			// lblAddress
			// 
			this.lblAddress.AutoSize = true;
			this.lblAddress.Location = new System.Drawing.Point(21, 0);
			this.lblAddress.Name = "lblAddress";
			this.lblAddress.Size = new System.Drawing.Size(56, 19);
			this.lblAddress.TabIndex = 2;
			this.lblAddress.Text = "Address";
			// 
			// tbAddress
			// 
			this.tbAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbAddress.Lines = new string[0];
			this.tbAddress.Location = new System.Drawing.Point(21, 22);
			this.tbAddress.MaxLength = 32767;
			this.tbAddress.Name = "tbAddress";
			this.tbAddress.PasswordChar = '\0';
			this.tbAddress.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.tbAddress.SelectedText = "";
			this.tbAddress.Size = new System.Drawing.Size(185, 23);
			this.tbAddress.TabIndex = 3;
			this.tbAddress.UseSelectable = true;
			// 
			// lblUsername
			// 
			this.lblUsername.AutoSize = true;
			this.lblUsername.Location = new System.Drawing.Point(21, 48);
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size(68, 19);
			this.lblUsername.TabIndex = 4;
			this.lblUsername.Text = "Username";
			// 
			// tbUsername
			// 
			this.tbUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbUsername.Lines = new string[0];
			this.tbUsername.Location = new System.Drawing.Point(21, 70);
			this.tbUsername.MaxLength = 32767;
			this.tbUsername.Name = "tbUsername";
			this.tbUsername.PasswordChar = '\0';
			this.tbUsername.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.tbUsername.SelectedText = "";
			this.tbUsername.Size = new System.Drawing.Size(185, 23);
			this.tbUsername.TabIndex = 5;
			this.tbUsername.UseSelectable = true;
			// 
			// lblPassword
			// 
			this.lblPassword.AutoSize = true;
			this.lblPassword.Location = new System.Drawing.Point(21, 96);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(63, 19);
			this.lblPassword.TabIndex = 6;
			this.lblPassword.Text = "Password";
			// 
			// tbPassword
			// 
			this.tbPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbPassword.Lines = new string[0];
			this.tbPassword.Location = new System.Drawing.Point(21, 118);
			this.tbPassword.MaxLength = 32767;
			this.tbPassword.Name = "tbPassword";
			this.tbPassword.PasswordChar = '●';
			this.tbPassword.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.tbPassword.SelectedText = "";
			this.tbPassword.Size = new System.Drawing.Size(185, 23);
			this.tbPassword.TabIndex = 7;
			this.tbPassword.UseSelectable = true;
			this.tbPassword.UseSystemPasswordChar = true;
			// 
			// btnSave
			// 
			this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnSave.Location = new System.Drawing.Point(76, 277);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(102, 23);
			this.btnSave.TabIndex = 8;
			this.btnSave.Text = "Save";
			this.btnSave.UseSelectable = true;
			this.btnSave.Click += new System.EventHandler(this.HandleSaveClicked);
			// 
			// pCustom
			// 
			this.pCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pCustom.Controls.Add(this.lblAddress);
			this.pCustom.Controls.Add(this.tbAddress);
			this.pCustom.Controls.Add(this.lblUsername);
			this.pCustom.Controls.Add(this.tbUsername);
			this.pCustom.Controls.Add(this.lblPassword);
			this.pCustom.Controls.Add(this.tbPassword);
			this.pCustom.Enabled = false;
			this.pCustom.Location = new System.Drawing.Point(23, 105);
			this.pCustom.Name = "pCustom";
			this.pCustom.Size = new System.Drawing.Size(209, 166);
			this.pCustom.TabIndex = 9;
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			// 
			// LoginSettings
			// 
			this.AcceptButton = this.btnSave;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(240, 320);
			this.Controls.Add(this.pCustom);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.rbCustom);
			this.Controls.Add(this.rbAutomatic);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(240, 320);
			this.Name = "LoginSettings";
			this.Resizable = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Login Settings";
			this.TopMost = true;
			this.pCustom.ResumeLayout(false);
			this.pCustom.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MetroFramework.Controls.MetroRadioButton rbAutomatic;
		private MetroFramework.Controls.MetroRadioButton rbCustom;
		private MetroFramework.Controls.MetroLabel lblAddress;
		private MetroFramework.Controls.MetroTextBox tbAddress;
		private MetroFramework.Controls.MetroLabel lblUsername;
		private MetroFramework.Controls.MetroTextBox tbUsername;
		private MetroFramework.Controls.MetroLabel lblPassword;
		private MetroFramework.Controls.MetroTextBox tbPassword;
		private MetroFramework.Controls.MetroButton btnSave;
		private System.Windows.Forms.Panel pCustom;
		private System.Windows.Forms.ErrorProvider errorProvider1;
	}
}