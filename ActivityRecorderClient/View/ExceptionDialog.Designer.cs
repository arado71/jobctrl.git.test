namespace Tct.ActivityRecorderClient.View
{
	partial class ExceptionDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionDialog));
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.lblGeneralErrorText = new MetroFramework.Controls.MetroLabel();
			this.btnContinue = new MetroFramework.Controls.MetroButton();
			this.btnQuit = new MetroFramework.Controls.MetroButton();
			this.chbReportError = new MetroFramework.Controls.MetroCheckBox();
			this.btnDetails = new MetroFramework.Controls.MetroButton();
			this.txbDescription = new MetroFramework.Controls.MetroTextBox();
			this.chbAttachLogs = new MetroFramework.Controls.MetroCheckBox();
			this.lblSendStatus = new MetroFramework.Controls.MetroLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.Location = new System.Drawing.Point(23, 63);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(64, 64);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox.TabIndex = 0;
			this.pictureBox.TabStop = false;
			// 
			// lblGeneralErrorText
			// 
			this.lblGeneralErrorText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblGeneralErrorText.Location = new System.Drawing.Point(93, 63);
			this.lblGeneralErrorText.Name = "lblGeneralErrorText";
			this.lblGeneralErrorText.Size = new System.Drawing.Size(555, 84);
			this.lblGeneralErrorText.TabIndex = 0;
			this.lblGeneralErrorText.Text = resources.GetString("lblGeneralErrorText.Text");
			this.lblGeneralErrorText.WrapToLine = true;
			// 
			// btnContinue
			// 
			this.btnContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnContinue.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnContinue.Location = new System.Drawing.Point(492, 418);
			this.btnContinue.Name = "btnContinue";
			this.btnContinue.Size = new System.Drawing.Size(75, 23);
			this.btnContinue.TabIndex = 6;
			this.btnContinue.Text = "Continue";
			this.btnContinue.UseSelectable = true;
			this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
			// 
			// btnQuit
			// 
			this.btnQuit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnQuit.Location = new System.Drawing.Point(573, 418);
			this.btnQuit.Name = "btnQuit";
			this.btnQuit.Size = new System.Drawing.Size(75, 23);
			this.btnQuit.TabIndex = 7;
			this.btnQuit.Text = "Quit";
			this.btnQuit.UseSelectable = true;
			this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
			// 
			// chbReportError
			// 
			this.chbReportError.AutoSize = true;
			this.chbReportError.Checked = true;
			this.chbReportError.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chbReportError.Location = new System.Drawing.Point(23, 152);
			this.chbReportError.Margin = new System.Windows.Forms.Padding(5);
			this.chbReportError.Name = "chbReportError";
			this.chbReportError.Size = new System.Drawing.Size(112, 15);
			this.chbReportError.TabIndex = 1;
			this.chbReportError.Text = "Send error report";
			this.chbReportError.UseSelectable = true;
			this.chbReportError.CheckedChanged += new System.EventHandler(this.chbReportError_CheckedChanged);
			// 
			// btnDetails
			// 
			this.btnDetails.Location = new System.Drawing.Point(48, 177);
			this.btnDetails.Margin = new System.Windows.Forms.Padding(5);
			this.btnDetails.Name = "btnDetails";
			this.btnDetails.Size = new System.Drawing.Size(75, 23);
			this.btnDetails.TabIndex = 2;
			this.btnDetails.Text = "Details...";
			this.btnDetails.UseSelectable = true;
			this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
			// 
			// txbDescription
			// 
			this.txbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbDescription.Lines = new string[0];
			this.txbDescription.Location = new System.Drawing.Point(48, 210);
			this.txbDescription.Margin = new System.Windows.Forms.Padding(5);
			this.txbDescription.MaxLength = 4000;
			this.txbDescription.Multiline = true;
			this.txbDescription.Name = "txbDescription";
			this.txbDescription.PasswordChar = '\0';
			this.txbDescription.PromptText = "Optional error description";
			this.txbDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbDescription.SelectedText = "";
			this.txbDescription.Size = new System.Drawing.Size(600, 136);
			this.txbDescription.TabIndex = 3;
			this.txbDescription.UseSelectable = true;
			// 
			// chbAttachLogs
			// 
			this.chbAttachLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chbAttachLogs.AutoSize = true;
			this.chbAttachLogs.Checked = true;
			this.chbAttachLogs.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chbAttachLogs.Location = new System.Drawing.Point(48, 356);
			this.chbAttachLogs.Margin = new System.Windows.Forms.Padding(5);
			this.chbAttachLogs.Name = "chbAttachLogs";
			this.chbAttachLogs.Size = new System.Drawing.Size(102, 15);
			this.chbAttachLogs.TabIndex = 4;
			this.chbAttachLogs.Text = "Attach log files";
			this.chbAttachLogs.UseSelectable = true;
			// 
			// lblSendStatus
			// 
			this.lblSendStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblSendStatus.Location = new System.Drawing.Point(48, 384);
			this.lblSendStatus.Margin = new System.Windows.Forms.Padding(8);
			this.lblSendStatus.Name = "lblSendStatus";
			this.lblSendStatus.Size = new System.Drawing.Size(543, 23);
			this.lblSendStatus.TabIndex = 5;
			this.lblSendStatus.Text = "Error report will submitted befor you quit or continue.";
			// 
			// ExceptionDialog
			// 
			this.AcceptButton = this.btnQuit;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnContinue;
			this.ClientSize = new System.Drawing.Size(671, 464);
			this.CloseBox = false;
			this.Controls.Add(this.lblSendStatus);
			this.Controls.Add(this.chbAttachLogs);
			this.Controls.Add(this.txbDescription);
			this.Controls.Add(this.btnDetails);
			this.Controls.Add(this.chbReportError);
			this.Controls.Add(this.btnQuit);
			this.Controls.Add(this.btnContinue);
			this.Controls.Add(this.lblGeneralErrorText);
			this.Controls.Add(this.pictureBox);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExceptionDialog";
			this.Resizable = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Unexpected Application Error";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox;
		private MetroFramework.Controls.MetroLabel lblGeneralErrorText;
		private MetroFramework.Controls.MetroButton btnContinue;
		private MetroFramework.Controls.MetroButton btnQuit;
		private MetroFramework.Controls.MetroCheckBox chbReportError;
		private MetroFramework.Controls.MetroButton btnDetails;
		private MetroFramework.Controls.MetroTextBox txbDescription;
		private MetroFramework.Controls.MetroCheckBox chbAttachLogs;
		private MetroFramework.Controls.MetroLabel lblSendStatus;
	}
}