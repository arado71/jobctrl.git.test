namespace Tct.ActivityRecorderClient.View
{
	partial class ErrorReportingForm
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
			this.lblDescription = new MetroFramework.Controls.MetroLabel();
			this.btnCancel = new MetroFramework.Controls.MetroButton();
			this.btnSend = new MetroFramework.Controls.MetroButton();
			this.chbAttachLogs = new MetroFramework.Controls.MetroCheckBox();
			this.txbDescription = new MetroFramework.Controls.MetroTextBox();
			this.SuspendLayout();
			// 
			// lblDescription
			// 
			this.lblDescription.AutoSize = true;
			this.lblDescription.Location = new System.Drawing.Point(23, 60);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(110, 19);
			this.lblDescription.TabIndex = 0;
			this.lblDescription.Text = "Error description:";
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(475, 285);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseSelectable = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnSend.Location = new System.Drawing.Point(394, 285);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(75, 23);
			this.btnSend.TabIndex = 3;
			this.btnSend.Text = "Send";
			this.btnSend.UseSelectable = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// chbAttachLogs
			// 
			this.chbAttachLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chbAttachLogs.AutoSize = true;
			this.chbAttachLogs.Checked = true;
			this.chbAttachLogs.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chbAttachLogs.Enabled = false;
			this.chbAttachLogs.Location = new System.Drawing.Point(23, 240);
			this.chbAttachLogs.Name = "chbAttachLogs";
			this.chbAttachLogs.Size = new System.Drawing.Size(102, 15);
			this.chbAttachLogs.TabIndex = 2;
			this.chbAttachLogs.Text = "Attach log files";
			this.chbAttachLogs.UseSelectable = true;
			this.chbAttachLogs.Visible = false;
			this.chbAttachLogs.CheckedChanged += new System.EventHandler(this.chbAttachLogs_CheckedChanged);
			// 
			// txbDescription
			// 
			this.txbDescription.AcceptsReturn = true;
			this.txbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txbDescription.Lines = new string[0];
			this.txbDescription.Location = new System.Drawing.Point(23, 82);
			this.txbDescription.MaxLength = 4000;
			this.txbDescription.Multiline = true;
			this.txbDescription.Name = "txbDescription";
			this.txbDescription.PasswordChar = '\0';
			this.txbDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txbDescription.SelectedText = "";
			this.txbDescription.Size = new System.Drawing.Size(527, 152);
			this.txbDescription.TabIndex = 1;
			this.txbDescription.UseSelectable = true;
			this.txbDescription.TextChanged += new System.EventHandler(this.txbDescription_TextChanged);
			// 
			// ErrorReportingForm
			// 
			this.AcceptButton = this.btnSend;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(573, 331);
			this.Controls.Add(this.txbDescription);
			this.Controls.Add(this.chbAttachLogs);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.lblDescription);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ErrorReportingForm";
			this.Resizable = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Error Reporting";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MetroFramework.Controls.MetroLabel lblDescription;
		private MetroFramework.Controls.MetroCheckBox chbAttachLogs;
		private MetroFramework.Controls.MetroButton btnCancel;
		private MetroFramework.Controls.MetroButton btnSend;
		private MetroFramework.Controls.MetroTextBox txbDescription;


	}
}