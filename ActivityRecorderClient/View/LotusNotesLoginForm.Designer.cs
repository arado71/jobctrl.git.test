using MetroFramework.Controls;

namespace Tct.ActivityRecorderClient.View
{
	partial class LotusNotesLoginForm
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
			this.cbRememberMe = new System.Windows.Forms.CheckBox();
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cbRememberMe
			// 
			this.cbRememberMe.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.cbRememberMe.AutoSize = true;
			this.cbRememberMe.Checked = true;
			this.cbRememberMe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbRememberMe.Location = new System.Drawing.Point(93, 100);
			this.cbRememberMe.Name = "cbRememberMe";
			this.cbRememberMe.Size = new System.Drawing.Size(139, 17);
			this.cbRememberMe.TabIndex = 3;
			this.cbRememberMe.Text = "Maradjon bejelentkezve";
			this.cbRememberMe.UseVisualStyleBackColor = true;
			this.cbRememberMe.SizeChanged += new System.EventHandler(this.OnSizeChanged);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnOk.Enabled = false;
			this.btnOk.Location = new System.Drawing.Point(127, 133);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "OK";
			this.btnOk.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// txtPassword
			// 
			this.txtPassword.Location = new System.Drawing.Point(164, 71);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.Size = new System.Drawing.Size(127, 20);
			this.txtPassword.TabIndex = 2;
			this.txtPassword.UseSystemPasswordChar = true;
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			this.txtPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPassword_KeyPress);
			// 
			// lblPassword
			// 
			this.lblPassword.AutoSize = true;
			this.lblPassword.Location = new System.Drawing.Point(37, 74);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(102, 13);
			this.lblPassword.TabIndex = 0;
			this.lblPassword.Text = "Jelszó (Lotus Notes)";
			// 
			// LotusNotesLoginForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(325, 182);
			this.Controls.Add(this.lblPassword);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.cbRememberMe);
			this.Controls.Add(this.btnOk);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LotusNotesLoginForm";
			this.Resizable = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "JobCTRL - Lotus Notes Login";
			this.TopMost = true;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LotusNotesLoginForm_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox cbRememberMe;
		private MetroButton btnOk;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblPassword;
	}
}