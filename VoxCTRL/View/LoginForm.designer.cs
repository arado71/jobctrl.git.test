namespace VoxCTRL.View
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
			this.label1 = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.txtUserId = new System.Windows.Forms.TextBox();
			this.cbRememberMe = new System.Windows.Forms.CheckBox();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "User ID / E-mail";
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(70, 108);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// txtUserId
			// 
			this.txtUserId.Location = new System.Drawing.Point(100, 20);
			this.txtUserId.Name = "txtUserId";
			this.txtUserId.Size = new System.Drawing.Size(102, 20);
			this.txtUserId.TabIndex = 1;
			this.txtUserId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUserId_KeyPress);
			// 
			// cbRememberMe
			// 
			this.cbRememberMe.AutoSize = true;
			this.cbRememberMe.Checked = true;
			this.cbRememberMe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbRememberMe.Location = new System.Drawing.Point(54, 75);
			this.cbRememberMe.Name = "cbRememberMe";
			this.cbRememberMe.Size = new System.Drawing.Size(103, 17);
			this.cbRememberMe.TabIndex = 3;
			this.cbRememberMe.Text = "Emlékezzen rám";
			this.cbRememberMe.UseVisualStyleBackColor = true;
			this.cbRememberMe.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbRememberMe_KeyPress);
			// 
			// txtPassword
			// 
			this.txtPassword.Location = new System.Drawing.Point(100, 46);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.Size = new System.Drawing.Size(102, 20);
			this.txtPassword.TabIndex = 2;
			this.txtPassword.UseSystemPasswordChar = true;
			this.txtPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPassword_KeyPress);
			// 
			// lblPassword
			// 
			this.lblPassword.AutoSize = true;
			this.lblPassword.Location = new System.Drawing.Point(12, 49);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(36, 13);
			this.lblPassword.TabIndex = 0;
			this.lblPassword.Text = "Jelszó";
			// 
			// LoginForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(219, 140);
			this.Controls.Add(this.txtPassword);
			this.Controls.Add(this.lblPassword);
			this.Controls.Add(this.cbRememberMe);
			this.Controls.Add(this.txtUserId);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LoginForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "VoxCTRL - Login";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.TextBox txtUserId;
		private System.Windows.Forms.CheckBox cbRememberMe;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblPassword;
	}
}