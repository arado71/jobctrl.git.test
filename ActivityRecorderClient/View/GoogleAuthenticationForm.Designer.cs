namespace Tct.ActivityRecorderClient.View
{
	partial class GoogleAuthenticationForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GoogleAuthenticationForm));
			this.SignInButton = new System.Windows.Forms.Button();
			this.googleButtonsImageList = new System.Windows.Forms.ImageList(this.components);
			this.firstRowMmetroLabel = new MetroFramework.Controls.MetroLabel();
			this.secondRowMetroLabel = new MetroFramework.Controls.MetroLabel();
			this.privacyMetroLink = new MetroFramework.Controls.MetroLink();
			this.cancelButton = new MetroFramework.Controls.MetroButton();
			this.SuspendLayout();
			// 
			// SignInButton
			// 
			this.SignInButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SignInButton.BackColor = System.Drawing.Color.White;
			this.SignInButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.SignInButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
			this.SignInButton.FlatAppearance.BorderSize = 0;
			this.SignInButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
			this.SignInButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
			this.SignInButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.SignInButton.ForeColor = System.Drawing.Color.White;
			this.SignInButton.ImageIndex = 0;
			this.SignInButton.ImageList = this.googleButtonsImageList;
			this.SignInButton.Location = new System.Drawing.Point(315, 102);
			this.SignInButton.Margin = new System.Windows.Forms.Padding(0);
			this.SignInButton.Name = "SignInButton";
			this.SignInButton.Size = new System.Drawing.Size(192, 47);
			this.SignInButton.TabIndex = 0;
			this.SignInButton.UseVisualStyleBackColor = false;
			this.SignInButton.Click += new System.EventHandler(this.SignInButton_Click);
			this.SignInButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SignInButton_MouseDown);
			this.SignInButton.MouseEnter += new System.EventHandler(this.SignInButton_MouseEnter);
			this.SignInButton.MouseLeave += new System.EventHandler(this.SignInButton_MouseLeave);
			this.SignInButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SignInButton_MouseUp);
			// 
			// googleButtonsImageList
			// 
			this.googleButtonsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("googleButtonsImageList.ImageStream")));
			this.googleButtonsImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.googleButtonsImageList.Images.SetKeyName(0, "btn_google_signin_light_normal_web.png");
			this.googleButtonsImageList.Images.SetKeyName(1, "btn_google_signin_light_focus_web.png");
			this.googleButtonsImageList.Images.SetKeyName(2, "btn_google_signin_light_pressed_web.png");
			// 
			// firstRowMmetroLabel
			// 
			this.firstRowMmetroLabel.AutoSize = true;
			this.firstRowMmetroLabel.Location = new System.Drawing.Point(15, 15);
			this.firstRowMmetroLabel.Name = "firstRowMmetroLabel";
			this.firstRowMmetroLabel.Size = new System.Drawing.Size(474, 19);
			this.firstRowMmetroLabel.TabIndex = 5;
			this.firstRowMmetroLabel.Text = "A naptár szinkronizáció működéséhez a Google fiók hozzákapcsolása szükséges.";
			// 
			// secondRowMetroLabel
			// 
			this.secondRowMetroLabel.AutoSize = true;
			this.secondRowMetroLabel.Location = new System.Drawing.Point(15, 38);
			this.secondRowMetroLabel.Name = "secondRowMetroLabel";
			this.secondRowMetroLabel.Size = new System.Drawing.Size(306, 19);
			this.secondRowMetroLabel.TabIndex = 6;
			this.secondRowMetroLabel.Text = "Kattintson az engedélyezési folyamat elkezdéséhez!";
			// 
			// privacyMetroLink
			// 
			this.privacyMetroLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.privacyMetroLink.Location = new System.Drawing.Point(15, 60);
			this.privacyMetroLink.Name = "privacyMetroLink";
			this.privacyMetroLink.Size = new System.Drawing.Size(180, 23);
			this.privacyMetroLink.TabIndex = 7;
			this.privacyMetroLink.Text = "Adatvédelmi nyilatkozat";
			this.privacyMetroLink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.privacyMetroLink.UseSelectable = true;
			this.privacyMetroLink.Click += new System.EventHandler(this.privacyMetroLink_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(509, 102);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 46);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Mégsem";
			this.cancelButton.UseSelectable = true;
			// 
			// GoogleAuthenticationForm
			// 
			this.AcceptButton = this.SignInButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(598, 168);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.privacyMetroLink);
			this.Controls.Add(this.secondRowMetroLabel);
			this.Controls.Add(this.firstRowMmetroLabel);
			this.Controls.Add(this.SignInButton);
			this.MinimumSize = new System.Drawing.Size(598, 168);
			this.Name = "GoogleAuthenticationForm";
			this.Load += new System.EventHandler(this.GoogleAuthenticationForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button SignInButton;
		private System.Windows.Forms.ImageList googleButtonsImageList;
		private MetroFramework.Controls.MetroLabel firstRowMmetroLabel;
		private MetroFramework.Controls.MetroLabel secondRowMetroLabel;
		private MetroFramework.Controls.MetroLink privacyMetroLink;
		private MetroFramework.Controls.MetroButton cancelButton;
	}
}