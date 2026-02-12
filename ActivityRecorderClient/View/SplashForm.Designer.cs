namespace Tct.ActivityRecorderClient.View
{
	partial class SplashForm
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
			this.pbWork = new System.Windows.Forms.ProgressBar();
			this.lblInfo = new System.Windows.Forms.Label();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// pbWork
			// 
			this.pbWork.Location = new System.Drawing.Point(30, 35);
			this.pbWork.Name = "pbWork";
			this.pbWork.Size = new System.Drawing.Size(275, 23);
			this.pbWork.TabIndex = 0;
			// 
			// lblInfo
			// 
			this.lblInfo.Location = new System.Drawing.Point(12, 9);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(311, 23);
			this.lblInfo.TabIndex = 1;
			this.lblInfo.Text = "A lezaras muvelet folyamatban, kerem varjon!";
			this.lblInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// SplashForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(335, 72);
			this.ControlBox = false;
			this.Controls.Add(this.lblInfo);
			this.Controls.Add(this.pbWork);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SplashForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.TopMost = true;
			this.Load += new System.EventHandler(this.SplashForm_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ProgressBar pbWork;
		private System.Windows.Forms.Label lblInfo;
		private System.Windows.Forms.Timer timer;
	}
}