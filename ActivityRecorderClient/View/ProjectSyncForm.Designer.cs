namespace Tct.ActivityRecorderClient.View
{
	partial class ProjectSyncForm
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
			this.txtUsername = new MetroFramework.Controls.MetroTextBox();
			this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
			this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
			this.txtPassword = new MetroFramework.Controls.MetroTextBox();
			this.metroButton1 = new MetroFramework.Controls.MetroButton();
			this.cbRemember = new MetroFramework.Controls.MetroCheckBox();
			this.metroProgressBar1 = new MetroFramework.Controls.MetroProgressBar();
			this.SuspendLayout();
			// 
			// txtUsername
			// 
			this.txtUsername.Lines = new string[0];
			this.txtUsername.Location = new System.Drawing.Point(23, 82);
			this.txtUsername.MaxLength = 32767;
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.PasswordChar = '\0';
			this.txtUsername.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.txtUsername.SelectedText = "";
			this.txtUsername.Size = new System.Drawing.Size(170, 23);
			this.txtUsername.TabIndex = 0;
			this.txtUsername.UseSelectable = true;
			// 
			// metroLabel1
			// 
			this.metroLabel1.AutoSize = true;
			this.metroLabel1.Location = new System.Drawing.Point(23, 60);
			this.metroLabel1.Name = "metroLabel1";
			this.metroLabel1.Size = new System.Drawing.Size(68, 19);
			this.metroLabel1.TabIndex = 1;
			this.metroLabel1.Text = "Username";
			// 
			// metroLabel2
			// 
			this.metroLabel2.AutoSize = true;
			this.metroLabel2.Location = new System.Drawing.Point(24, 112);
			this.metroLabel2.Name = "metroLabel2";
			this.metroLabel2.Size = new System.Drawing.Size(63, 19);
			this.metroLabel2.TabIndex = 2;
			this.metroLabel2.Text = "Password";
			// 
			// txtPassword
			// 
			this.txtPassword.Lines = new string[0];
			this.txtPassword.Location = new System.Drawing.Point(24, 135);
			this.txtPassword.MaxLength = 32767;
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.txtPassword.SelectedText = "";
			this.txtPassword.Size = new System.Drawing.Size(169, 23);
			this.txtPassword.TabIndex = 3;
			this.txtPassword.UseSelectable = true;
			// 
			// metroButton1
			// 
			this.metroButton1.Location = new System.Drawing.Point(70, 187);
			this.metroButton1.Name = "metroButton1";
			this.metroButton1.Size = new System.Drawing.Size(75, 23);
			this.metroButton1.TabIndex = 7;
			this.metroButton1.Text = "Login";
			this.metroButton1.UseSelectable = true;
			this.metroButton1.Click += new System.EventHandler(this.HandleLoginClicked);
			// 
			// cbRemember
			// 
			this.cbRemember.AutoSize = true;
			this.cbRemember.Location = new System.Drawing.Point(23, 164);
			this.cbRemember.Name = "cbRemember";
			this.cbRemember.Size = new System.Drawing.Size(134, 15);
			this.cbRemember.TabIndex = 5;
			this.cbRemember.Text = "Remember password";
			this.cbRemember.UseSelectable = true;
			// 
			// metroProgressBar1
			// 
			this.metroProgressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.metroProgressBar1.Location = new System.Drawing.Point(10, 215);
			this.metroProgressBar1.Name = "metroProgressBar1";
			this.metroProgressBar1.Size = new System.Drawing.Size(200, 12);
			this.metroProgressBar1.TabIndex = 8;
			this.metroProgressBar1.Visible = false;
			// 
			// ProjectSyncForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(219, 233);
			this.Controls.Add(this.metroProgressBar1);
			this.Controls.Add(this.cbRemember);
			this.Controls.Add(this.metroButton1);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.metroLabel2);
			this.Controls.Add(this.metroLabel1);
			this.Controls.Add(this.txtUsername);
			this.MaximizeBox = false;
			this.Name = "ProjectSyncForm";
			this.Resizable = false;
			this.Text = "Sync login";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MetroFramework.Controls.MetroTextBox txtUsername;
		private MetroFramework.Controls.MetroLabel metroLabel1;
		private MetroFramework.Controls.MetroLabel metroLabel2;
		private MetroFramework.Controls.MetroTextBox txtPassword;
		private MetroFramework.Controls.MetroButton metroButton1;
		private MetroFramework.Controls.MetroCheckBox cbRemember;
		private MetroFramework.Controls.MetroProgressBar metroProgressBar1;
	}
}