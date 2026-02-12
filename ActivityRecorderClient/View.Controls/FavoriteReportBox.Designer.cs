namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class FavoriteReportBox
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.nameSmartLabel = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.pictureBox1.Location = new System.Drawing.Point(3, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(60, 59);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FavoriteReportBox_MouseClick);
			// 
			// nameSmartLabel
			// 
			this.nameSmartLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.nameSmartLabel.AutoWrap = false;
			this.nameSmartLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.nameSmartLabel.FontSize = 10F;
			this.nameSmartLabel.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(179)))), ((int)(((byte)(179)))));
			this.nameSmartLabel.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Left;
			this.nameSmartLabel.Location = new System.Drawing.Point(69, 3);
			this.nameSmartLabel.Name = "nameSmartLabel";
			this.nameSmartLabel.Size = new System.Drawing.Size(215, 59);
			this.nameSmartLabel.TabIndex = 2;
			this.nameSmartLabel.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
			this.nameSmartLabel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FavoriteReportBox_MouseClick);
			// 
			// FavoriteReportBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.Controls.Add(this.nameSmartLabel);
			this.Controls.Add(this.pictureBox1);
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Name = "FavoriteReportBox";
			this.Size = new System.Drawing.Size(287, 66);
			this.Load += new System.EventHandler(this.FavoriteReportBox_Load);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FavoriteReportBox_MouseClick);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.PictureBox pictureBox1;
		private SmartLabel nameSmartLabel;
	}
}
