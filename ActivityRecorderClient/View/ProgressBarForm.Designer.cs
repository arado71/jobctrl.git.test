namespace Tct.ActivityRecorderClient.View
{
	partial class ProgressBarForm
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
			this.lblProgressText = new MetroFramework.Controls.MetroLabel();
			this.progressBar = new MetroFramework.Controls.MetroProgressBar();
			this.btnCancel = new MetroFramework.Controls.MetroButton();
			this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
			this.SuspendLayout();
			// 
			// lblProgressText
			// 
			this.lblProgressText.Location = new System.Drawing.Point(23, 60);
			this.lblProgressText.Name = "lblProgressText";
			this.lblProgressText.Size = new System.Drawing.Size(410, 23);
			this.lblProgressText.TabIndex = 0;
			this.lblProgressText.Text = "Progression text...";
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(26, 86);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(407, 23);
			this.progressBar.TabIndex = 1;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Location = new System.Drawing.Point(358, 126);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseSelectable = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// backgroundWorker
			// 
			this.backgroundWorker.WorkerReportsProgress = true;
			this.backgroundWorker.WorkerSupportsCancellation = true;
			this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
			this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
			this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
			// 
			// ProgressBarForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(456, 172);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.lblProgressText);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressBarForm";
			this.Resizable = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Title of progression";
			this.ResumeLayout(false);

		}

		#endregion

		private MetroFramework.Controls.MetroLabel lblProgressText;
		private MetroFramework.Controls.MetroProgressBar progressBar;
		private MetroFramework.Controls.MetroButton btnCancel;
		private System.ComponentModel.BackgroundWorker backgroundWorker;

	}
}