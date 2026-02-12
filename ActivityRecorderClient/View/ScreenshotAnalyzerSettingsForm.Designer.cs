namespace Tct.ActivityRecorderClient.View
{
	partial class ScreenshotAnalyzerSettingsForm
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.lblWorkTimeStat = new MetroFramework.Controls.MetroLabel();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Location = new System.Drawing.Point(24, 64);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(582, 55);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// lblWorkTimeStat
			// 
			this.lblWorkTimeStat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblWorkTimeStat.Location = new System.Drawing.Point(24, 140);
			this.lblWorkTimeStat.Name = "lblWorkTimeStat";
			this.lblWorkTimeStat.Size = new System.Drawing.Size(582, 22);
			this.lblWorkTimeStat.TabIndex = 1;
			this.lblWorkTimeStat.Text = "stat";
			this.lblWorkTimeStat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ScreenshotAnalyzerSettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(629, 175);
			this.Controls.Add(this.lblWorkTimeStat);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Movable = false;
			this.Name = "ScreenshotAnalyzerSettingsForm";
			this.Opacity = 0.9D;
			this.Resizable = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Settings";
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private MetroFramework.Controls.MetroLabel lblWorkTimeStat;
	}
}