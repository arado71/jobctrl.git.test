using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ClientErrorReporting;

namespace Tct.ActivityRecorderClient.View
{
	public class ErrorReportingBaseForm : FixedMetroForm
	{
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly IErrorReporter reporter = Platform.Factory.GetErrorReporter();

		protected void ShowSendErrorReportForm(IWin32Window owner, string description, bool attachLogs)
		{
			description = description.Truncate(4000);
			var done = false;

			while (!done)
			{
				done = ShowSendErrorReportFormImpl(owner, description, attachLogs);
			}
		}

		private bool ShowSendErrorReportFormImpl(IWin32Window owner, string description, bool attachLogs)
		{
			using (var waitForm = new ProgressBarForm())
			{
				waitForm.Text = Labels.ErrorReporting_ProgressTitle + @"...";
				waitForm.DoWorkAction = f => reporter.ReportClientError(description, attachLogs, waitForm.ReportProgress, f);
				waitForm.CancelConfirmAction = () => ConfirmCancel(waitForm);

				waitForm.ShowDialog(owner);

				if (waitForm.Result == null || waitForm.Result.Error != null)
				{
					if (waitForm.Result == null) log.ErrorAndFail("WaitForm's DoWorkAction failed.");
					var body = Labels.ErrorReporting_UnexpectedErrorBody +
							   (waitForm.Result != null ? (Environment.NewLine + waitForm.Result.Error) : "");
					var res = MessageBox.Show(owner, body, Labels.ErrorReporting_UnexpectedErrorTitle, MessageBoxButtons.RetryCancel);
					if (res == DialogResult.Retry) return false;
				}
			}
			return true;
		}

		private static bool ConfirmCancel(IWin32Window owner)
		{
			var res = MessageBox.Show(owner, Labels.ErrorReporting_UploadCancelConfirmBody, Labels.ErrorReporting_UploadCancelConfirmTitle, MessageBoxButtons.YesNo);
			return res == DialogResult.Yes;
		}
	}
}
