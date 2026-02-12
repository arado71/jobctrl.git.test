namespace Tct.ActivityRecorderClient.View
{
	partial class FullScreenBorderAlertForm
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
			this.flashTimer = new System.Windows.Forms.Timer(this.components);
			this.hideTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// flashTimer
			// 
			this.flashTimer.Tick += new System.EventHandler(this.flashTimer_Tick);
			// 
			// hideTimer
			// 
			this.hideTimer.Interval = 200;
			this.hideTimer.Tick += new System.EventHandler(this.hideTimer_Tick);
			// 
			// FullScreenBorderAlertForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.GreenYellow;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "FullScreenBorderAlertForm";
			this.Opacity = 0.5D;
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Idle alert";
			this.TopMost = true;
			this.TransparencyKey = System.Drawing.Color.GreenYellow;
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer flashTimer;
		private System.Windows.Forms.Timer hideTimer;
	}
}