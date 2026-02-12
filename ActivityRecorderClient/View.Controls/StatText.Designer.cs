namespace Tct.ActivityRecorderClient.View.Controls
{
    partial class StatText
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
			this.metroLink1 = new MetroFramework.Controls.MetroLink();
			this.pStatsBtn = new System.Windows.Forms.Panel();
			this.metroProgressSpinner = new MetroFramework.Controls.MetroProgressSpinner();
			this.timeLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// metroLink1
			// 
			this.metroLink1.BackColor = System.Drawing.Color.Transparent;
			this.metroLink1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.metroLink1.Dock = System.Windows.Forms.DockStyle.Right;
			this.metroLink1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
			this.metroLink1.Location = new System.Drawing.Point(105, 0);
			this.metroLink1.Name = "metroLink1";
			this.metroLink1.Size = new System.Drawing.Size(159, 23);
			this.metroLink1.TabIndex = 19;
			this.metroLink1.Text = "metroLink1112";
			this.metroLink1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.metroLink1.Theme = MetroFramework.MetroThemeStyle.Light;
			this.metroLink1.UseCustomBackColor = true;
			this.metroLink1.UseCustomForeColor = true;
			this.metroLink1.UseSelectable = true;
			this.metroLink1.Click += new System.EventHandler(this.StatText_Click);
			this.metroLink1.MouseEnter += new System.EventHandler(this.StatText_MouseEnter);
			this.metroLink1.MouseLeave += new System.EventHandler(this.StatText_MouseLeave);
			// 
			// pStatsBtn
			// 
			this.pStatsBtn.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.stats_blue;
			this.pStatsBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pStatsBtn.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pStatsBtn.Dock = System.Windows.Forms.DockStyle.Right;
			this.pStatsBtn.Location = new System.Drawing.Point(264, 0);
			this.pStatsBtn.Margin = new System.Windows.Forms.Padding(0);
			this.pStatsBtn.Name = "pStatsBtn";
			this.pStatsBtn.Size = new System.Drawing.Size(20, 23);
			this.pStatsBtn.TabIndex = 18;
			this.pStatsBtn.Click += new System.EventHandler(this.StatText_Click);
			this.pStatsBtn.MouseEnter += new System.EventHandler(this.StatText_MouseEnter);
			this.pStatsBtn.MouseLeave += new System.EventHandler(this.StatText_MouseLeave);
			// 
			// metroProgressSpinner
			// 
			this.metroProgressSpinner.Location = new System.Drawing.Point(58, 3);
			this.metroProgressSpinner.Maximum = 100;
			this.metroProgressSpinner.Name = "metroProgressSpinner";
			this.metroProgressSpinner.Size = new System.Drawing.Size(20, 20);
			this.metroProgressSpinner.TabIndex = 21;
			this.metroProgressSpinner.UseSelectable = true;
			// 
			// timeLabel
			// 
			this.timeLabel.AutoSize = true;
			this.timeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.timeLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.timeLabel.Location = new System.Drawing.Point(3, 5);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(34, 13);
			this.timeLabel.TabIndex = 22;
			this.timeLabel.Text = "00:00";
			this.timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// StatText
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.timeLabel);
			this.Controls.Add(this.metroProgressSpinner);
			this.Controls.Add(this.metroLink1);
			this.Controls.Add(this.pStatsBtn);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Name = "StatText";
			this.Size = new System.Drawing.Size(284, 23);
			this.Load += new System.EventHandler(this.StatText_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel pStatsBtn;
        private MetroFramework.Controls.MetroLink metroLink1;
		private MetroFramework.Controls.MetroProgressSpinner metroProgressSpinner;
		private System.Windows.Forms.Label timeLabel;
	}
}
