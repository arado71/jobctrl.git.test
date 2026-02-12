using System;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View
{
	public partial class ErrorReportingForm : ErrorReportingBaseForm
	{
		public ErrorReportingForm()
		{
			InitializeComponent();
			Icon = Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			Text = Labels.ErrorReporting_Title;
			lblDescription.Text = Labels.ErrorReporting_ErrorDescription;
			chbAttachLogs.Text = Labels.ErrorReporting_AttachLogs;
			btnSend.Text = Labels.ErrorReporting_Send;
			btnCancel.Text = Labels.Cancel;
		}

		private void SendErrorReport()
		{
			var description = txbDescription.Text;
			var attachLogs = chbAttachLogs.Checked;
			ShowSendErrorReportForm(this, description, attachLogs);
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			SendErrorReport();
			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void txbDescription_TextChanged(object sender, EventArgs e)
		{
			btnSend.Enabled = !string.IsNullOrEmpty(txbDescription.Text) || chbAttachLogs.Checked;
		}

		private void chbAttachLogs_CheckedChanged(object sender, EventArgs e)
		{
			btnSend.Enabled = !string.IsNullOrEmpty(txbDescription.Text) || chbAttachLogs.Checked;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			if (DialogResult == DialogResult.OK || e.Cancel || e.CloseReason != CloseReason.UserClosing) return;
			if (string.IsNullOrEmpty(txbDescription.Text)) return; //don't need to confirm on empty text
			var res = MessageBox.Show(this, Labels.ErrorReporting_EditCancelConfirmBody, Labels.ErrorReporting_EditCancelConfirmTitle, MessageBoxButtons.YesNo);
			e.Cancel = res == DialogResult.No;
		}
	}
}

