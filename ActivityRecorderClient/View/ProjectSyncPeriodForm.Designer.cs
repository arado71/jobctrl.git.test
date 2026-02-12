namespace Tct.ActivityRecorderClient.View
{
	partial class ProjectSyncPeriodForm
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
			this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
			this.metroButton1 = new MetroFramework.Controls.MetroButton();
			this.cbSubmit = new MetroFramework.Controls.MetroCheckBox();
			this.cbPeriods = new System.Windows.Forms.CheckedListBox();
			this.SuspendLayout();
			// 
			// metroLabel1
			// 
			this.metroLabel1.AutoSize = true;
			this.metroLabel1.Location = new System.Drawing.Point(23, 60);
			this.metroLabel1.Name = "metroLabel1";
			this.metroLabel1.Size = new System.Drawing.Size(186, 19);
			this.metroLabel1.TabIndex = 0;
			this.metroLabel1.Text = "Please select period to upload";
			// 
			// metroButton1
			// 
			this.metroButton1.Location = new System.Drawing.Point(80, 267);
			this.metroButton1.Name = "metroButton1";
			this.metroButton1.Size = new System.Drawing.Size(75, 23);
			this.metroButton1.TabIndex = 2;
			this.metroButton1.Text = "Upload";
			this.metroButton1.UseSelectable = true;
			this.metroButton1.Click += new System.EventHandler(this.HandleUploadClicked);
			// 
			// cbSubmit
			// 
			this.cbSubmit.AutoSize = true;
			this.cbSubmit.Location = new System.Drawing.Point(23, 246);
			this.cbSubmit.Name = "cbSubmit";
			this.cbSubmit.Size = new System.Drawing.Size(116, 15);
			this.cbSubmit.TabIndex = 1;
			this.cbSubmit.Text = "Submit timesheet";
			this.cbSubmit.UseSelectable = true;
			// 
			// cbPeriods
			// 
			this.cbPeriods.FormattingEnabled = true;
			this.cbPeriods.Location = new System.Drawing.Point(23, 82);
			this.cbPeriods.Name = "cbPeriods";
			this.cbPeriods.Size = new System.Drawing.Size(186, 154);
			this.cbPeriods.TabIndex = 0;
			// 
			// ProjectSyncPeriodForm
			// 
			this.AcceptButton = this.metroButton1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(232, 313);
			this.Controls.Add(this.cbPeriods);
			this.Controls.Add(this.cbSubmit);
			this.Controls.Add(this.metroButton1);
			this.Controls.Add(this.metroLabel1);
			this.MaximizeBox = false;
			this.Name = "ProjectSyncPeriodForm";
			this.Resizable = false;
			this.Text = "Period selection";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MetroFramework.Controls.MetroLabel metroLabel1;
		private MetroFramework.Controls.MetroButton metroButton1;
		private MetroFramework.Controls.MetroCheckBox cbSubmit;
		private System.Windows.Forms.CheckedListBox cbPeriods;
	}
}