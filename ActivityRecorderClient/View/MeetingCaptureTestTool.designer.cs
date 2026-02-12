namespace Tct.ActivityRecorderClient.View
{
	partial class MeetingCaptureTestTool
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
			this.btnCaptureMeetings = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txbCalendarEmailAccounts = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
			this.label3 = new System.Windows.Forms.Label();
			this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
			this.txbFinishedMeetings = new System.Windows.Forms.TextBox();
			this.lblVersionInfo = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnCaptureMeetings
			// 
			this.btnCaptureMeetings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCaptureMeetings.Location = new System.Drawing.Point(489, 133);
			this.btnCaptureMeetings.Name = "btnCaptureMeetings";
			this.btnCaptureMeetings.Size = new System.Drawing.Size(125, 23);
			this.btnCaptureMeetings.TabIndex = 0;
			this.btnCaptureMeetings.Text = "Capture Meetings";
			this.btnCaptureMeetings.UseVisualStyleBackColor = true;
			this.btnCaptureMeetings.Click += new System.EventHandler(this.btnCaptureMeetings_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 58);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(126, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Calendar email accounts:";
			// 
			// txbCalendarEmailAccounts
			// 
			this.txbCalendarEmailAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbCalendarEmailAccounts.Location = new System.Drawing.Point(145, 55);
			this.txbCalendarEmailAccounts.Name = "txbCalendarEmailAccounts";
			this.txbCalendarEmailAccounts.Size = new System.Drawing.Size(469, 20);
			this.txbCalendarEmailAccounts.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 85);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Start date:";
			// 
			// dtpStartDate
			// 
			this.dtpStartDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dtpStartDate.CustomFormat = "yyyy.MM.dd HH:mm";
			this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpStartDate.Location = new System.Drawing.Point(145, 81);
			this.dtpStartDate.Name = "dtpStartDate";
			this.dtpStartDate.Size = new System.Drawing.Size(469, 20);
			this.dtpStartDate.TabIndex = 4;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 111);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(53, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "End date:";
			// 
			// dtpEndDate
			// 
			this.dtpEndDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dtpEndDate.CustomFormat = "yyyy.MM.dd HH:mm";
			this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpEndDate.Location = new System.Drawing.Point(145, 107);
			this.dtpEndDate.Name = "dtpEndDate";
			this.dtpEndDate.Size = new System.Drawing.Size(469, 20);
			this.dtpEndDate.TabIndex = 6;
			// 
			// txbFinishedMeetings
			// 
			this.txbFinishedMeetings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbFinishedMeetings.Location = new System.Drawing.Point(12, 162);
			this.txbFinishedMeetings.Multiline = true;
			this.txbFinishedMeetings.Name = "txbFinishedMeetings";
			this.txbFinishedMeetings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbFinishedMeetings.Size = new System.Drawing.Size(602, 166);
			this.txbFinishedMeetings.TabIndex = 7;
			// 
			// lblVersionInfo
			// 
			this.lblVersionInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblVersionInfo.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.lblVersionInfo.Location = new System.Drawing.Point(12, 15);
			this.lblVersionInfo.Name = "lblVersionInfo";
			this.lblVersionInfo.Size = new System.Drawing.Size(602, 26);
			this.lblVersionInfo.TabIndex = 8;
			this.lblVersionInfo.Text = "Szöveg az elsõ sorban\r\nSzöveg a második sorban";
			// 
			// MeetingCaptureTestTool
			// 
			this.AcceptButton = this.btnCaptureMeetings;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(626, 340);
			this.Controls.Add(this.lblVersionInfo);
			this.Controls.Add(this.txbFinishedMeetings);
			this.Controls.Add(this.dtpEndDate);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.dtpStartDate);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txbCalendarEmailAccounts);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnCaptureMeetings);
			this.Name = "MeetingCaptureTestTool";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Meeting Capture Test Tool";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MeetingCaptureClientForm_FormClosed);
			this.Load += new System.EventHandler(this.MeetingCaptureClientForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnCaptureMeetings;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txbCalendarEmailAccounts;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.DateTimePicker dtpStartDate;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.DateTimePicker dtpEndDate;
		private System.Windows.Forms.TextBox txbFinishedMeetings;
		private System.Windows.Forms.Label lblVersionInfo;
	}
}