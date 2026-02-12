namespace Tct.ActivityRecorderClient.View
{
	partial class PasswordExpiredMessageBox
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
			this.linkLabel = new MetroFramework.Controls.MetroLink();
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.SuspendLayout();
			// 
			// linkLabel
			// 
			this.linkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.linkLabel.BackColor = System.Drawing.Color.Transparent;
			this.linkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.linkLabel.Location = new System.Drawing.Point(23, 63);
			this.linkLabel.Name = "linkLabel";
			this.linkLabel.Size = new System.Drawing.Size(347, 29);
			this.linkLabel.TabIndex = 1;
			this.linkLabel.Text = "metroLink1";
			this.linkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.linkLabel.Theme = MetroFramework.MetroThemeStyle.Light;
			this.linkLabel.UseCustomBackColor = true;
			this.linkLabel.UseCustomForeColor = true;
			this.linkLabel.UseSelectable = true;
			this.linkLabel.Click += new System.EventHandler(this.linkLabel_Click);
			this.linkLabel.MouseEnter += new System.EventHandler(this.linkLabel_MouseEnter);
			this.linkLabel.MouseLeave += new System.EventHandler(this.linkLabel_MouseLeave);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnOk.Location = new System.Drawing.Point(295, 98);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "Ok";
			this.btnOk.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// PasswordExpiredMessageBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(393, 144);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.linkLabel);
			this.Name = "PasswordExpiredMessageBox";
			this.Resizable = false;
			this.Text = "PasswordExpiredMessageBox";
			this.ResumeLayout(false);

		}

		#endregion

		private MetroFramework.Controls.MetroLink linkLabel;
		private MetroFramework.Controls.MetroButton btnOk;
	}
}