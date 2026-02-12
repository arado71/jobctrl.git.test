namespace Tct.ActivityRecorderClient.View
{
	partial class AcquireLogsMessageBox
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
			this.noButton = new MetroFramework.Controls.MetroButton();
			this.yesButton = new MetroFramework.Controls.MetroButton();
			this.rememberCheckBox = new MetroFramework.Controls.MetroCheckBox();
			this.SuspendLayout();
			// 
			// noButton
			// 
			this.noButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.noButton.Location = new System.Drawing.Point(202, 94);
			this.noButton.Name = "noButton";
			this.noButton.Size = new System.Drawing.Size(75, 23);
			this.noButton.TabIndex = 0;
			this.noButton.Text = "Nem";
			this.noButton.UseSelectable = true;
			this.noButton.Click += new System.EventHandler(this.noButton_Click);
			// 
			// yesButton
			// 
			this.yesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.yesButton.Location = new System.Drawing.Point(121, 94);
			this.yesButton.Name = "yesButton";
			this.yesButton.Size = new System.Drawing.Size(75, 23);
			this.yesButton.TabIndex = 1;
			this.yesButton.Text = "Igen";
			this.yesButton.UseSelectable = true;
			this.yesButton.Click += new System.EventHandler(this.yesButton_Click);
			// 
			// rememberCheckBox
			// 
			this.rememberCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.rememberCheckBox.AutoSize = true;
			this.rememberCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.rememberCheckBox.Location = new System.Drawing.Point(135, 73);
			this.rememberCheckBox.Name = "rememberCheckBox";
			this.rememberCheckBox.Size = new System.Drawing.Size(142, 15);
			this.rememberCheckBox.TabIndex = 2;
			this.rememberCheckBox.Text = "Emlékezzen a döntésre";
			this.rememberCheckBox.UseSelectable = true;
			// 
			// AcquireLogsMessageBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(300, 140);
			this.Controls.Add(this.rememberCheckBox);
			this.Controls.Add(this.yesButton);
			this.Controls.Add(this.noButton);
			this.MinimumSize = new System.Drawing.Size(300, 140);
			this.Name = "AcquireLogsMessageBox";
			this.Resizable = false;
			this.Text = "Do you send log files?";
			this.Load += new System.EventHandler(this.AcquireLogsMessageBox_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MetroFramework.Controls.MetroButton noButton;
		private MetroFramework.Controls.MetroButton yesButton;
		private MetroFramework.Controls.MetroCheckBox rememberCheckBox;
	}
}