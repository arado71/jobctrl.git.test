using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View
{
	public partial class ExceptionDialog : ErrorReportingBaseForm
	{
		private readonly Exception error;

		public ExceptionDialog(Exception ex)
		{
			error = ex;

			InitializeComponent();

			try
			{
				Icon = Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
				pictureBox.Image = SystemIcons.Error.ToBitmap();
			}
			catch (Exception e)
			{
				log.Warn("Icon creation failed", e);
			}
			
			Text = Labels.ExceptionDialog_Title;
			lblGeneralErrorText.Text = Labels.ExceptionDialog_ErrorDescription;
			chbReportError.Text = Labels.ExceptionDialog_ReportError;
			btnDetails.Text = Labels.ExceptionDialog_Details + @"...";
			txbDescription.PromptText = Labels.ExceptionDialog_ErrorDescriptionPromptText;
			chbAttachLogs.Text = Labels.ErrorReporting_AttachLogs;
			lblSendStatus.Text = Labels.ExceptionDialog_SendStatus;
			btnQuit.Text = Labels.ExceptionDialog_Quit;
			btnContinue.Text = Labels.ExceptionDialog_Continue;
		}

		private void btnContinue_Click(object sender, EventArgs e)
		{
			if (chbReportError.Checked) SendErrorReport();
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void btnQuit_Click(object sender, EventArgs e)
		{
			if (chbReportError.Checked) SendErrorReport();
			DialogResult = DialogResult.Abort;
			Close();
		}

		private void chbReportError_CheckedChanged(object sender, EventArgs e)
		{
			btnDetails.Enabled = chbReportError.Checked;
			txbDescription.Enabled = chbReportError.Checked;
			chbAttachLogs.Enabled = chbReportError.Checked;
			lblSendStatus.Visible = chbReportError.Checked;
		}

		private void btnDetails_Click(object sender, EventArgs e)
		{
			MessageBox.Show(error.ToString(), Labels.ExceptionDialog_DetailsTitle, MessageBoxButtons.OK);
		}

		private void SendErrorReport()
		{
			var description = txbDescription.Text + Environment.NewLine + Environment.NewLine + error;
			var attachLogs = chbAttachLogs.Checked;
			ShowSendErrorReportForm(this, description, attachLogs);
		}
	}
}
