namespace Tct.ActivityRecorderClient.View
{
	partial class CountDownForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CountDownForm));
			this.lblTime = new System.Windows.Forms.Label();
			this.countDownTimer = new System.Windows.Forms.Timer(this.components);
			this.pnlDesc = new System.Windows.Forms.Panel();
			this.lblDesc = new Tct.ActivityRecorderClient.View.GrowLabel();
			this.lblTimeLeft = new System.Windows.Forms.Label();
			this.pnlDesc.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblTime
			// 
			this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblTime.Location = new System.Drawing.Point(22, 120);
			this.lblTime.Name = "lblTime";
			this.lblTime.Size = new System.Drawing.Size(288, 67);
			this.lblTime.TabIndex = 1;
			this.lblTime.Text = "01:30:23";
			// 
			// countDownTimer
			// 
			this.countDownTimer.Tick += new System.EventHandler(this.CountDownTimerTick);
			// 
			// pnlDesc
			// 
			this.pnlDesc.AutoScroll = true;
			this.pnlDesc.Controls.Add(this.lblDesc);
			this.pnlDesc.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlDesc.Location = new System.Drawing.Point(20, 60);
			this.pnlDesc.Name = "pnlDesc";
			this.pnlDesc.Size = new System.Drawing.Size(312, 51);
			this.pnlDesc.TabIndex = 2;
			// 
			// lblDesc
			// 
			this.lblDesc.Location = new System.Drawing.Point(12, 9);
			this.lblDesc.Name = "lblDesc";
			this.lblDesc.Size = new System.Drawing.Size(283, 78);
			this.lblDesc.TabIndex = 0;
			this.lblDesc.Text = resources.GetString("lblDesc.Text");
			this.lblDesc.UseMnemonic = false;
			// 
			// lblTimeLeft
			// 
			this.lblTimeLeft.AutoSize = true;
			this.lblTimeLeft.Location = new System.Drawing.Point(12, 113);
			this.lblTimeLeft.Name = "lblTimeLeft";
			this.lblTimeLeft.Size = new System.Drawing.Size(50, 13);
			this.lblTimeLeft.TabIndex = 3;
			this.lblTimeLeft.Text = "Time left:";
			// 
			// CountDownForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(352, 196);
			this.Controls.Add(this.lblTimeLeft);
			this.Controls.Add(this.pnlDesc);
			this.Controls.Add(this.lblTime);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(352, 196);
			this.MinimumSize = new System.Drawing.Size(352, 196);
			this.Name = "CountDownForm";
			this.Resizable = false;
			this.Text = "CountDownForm";
			this.Shown += new System.EventHandler(this.CountDownFormShown);
			this.pnlDesc.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private GrowLabel lblDesc;
		private System.Windows.Forms.Label lblTime;
		private System.Windows.Forms.Timer countDownTimer;
		private System.Windows.Forms.Panel pnlDesc;
		private System.Windows.Forms.Label lblTimeLeft;
	}
}