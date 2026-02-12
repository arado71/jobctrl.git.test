using System;
using System.ComponentModel;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ClientErrorReporting;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View
{
	public partial class ProgressBarForm : FixedMetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ProgressBarForm()
		{
			InitializeComponent();
			Icon = Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			btnCancel.Text = Labels.Cancel;
		}

		public Func<Func<bool>, bool> DoWorkAction { get; set; }

		public Func<bool> CancelConfirmAction { get; set; }

		public DoWorkResult Result { get; private set; }

		public void ReportProgress(ReportingProgress progress)
		{
			backgroundWorker.ReportProgress(Math.Min(Math.Max(progress.Value, 0), 100), progress);
		}

		protected override void OnShown(EventArgs e)
		{
			backgroundWorker.RunWorkerAsync();
			base.OnShown(e);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			e.Cancel = e.Cancel || CancelBackgroundWorkerIfNeeded();
		}

		private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var doWork = DoWorkAction;
			if (doWork != null)
			{
				if (!doWork(() => backgroundWorker.CancellationPending))
				{
					e.Cancel = true;
					return;
				}
			}
		}

		private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			progressBar.Value = Math.Min(Math.Max(e.ProgressPercentage, 0), 100);
			var progress = e.UserState as ReportingProgress;
			if (progress != null) lblProgressText.Text = string.Format("{0}/{1} - {2}", progress.CurrentPhase, progress.NumberOfPhases, progress.PhaseText);
		}

		private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null) log.ErrorAndFail("Background work completed with error.", e.Error);
			if (e.Cancelled) log.Info("Background work cancelled.");
			if (e.Error == null && !e.Cancelled) log.Info("Background work finished successfully.");
			progressBar.Value = 100;
			Result = new DoWorkResult() { Error = e.Error, Cancelled = e.Cancelled };
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			CancelBackgroundWorkerIfNeeded();
		}

		private bool CancelBackgroundWorkerIfNeeded()
		{
			if (!backgroundWorker.IsBusy) return false;
			if (CancelConfirmAction != null && !CancelConfirmAction()) return backgroundWorker.IsBusy;

			backgroundWorker.CancelAsync();
			return backgroundWorker.IsBusy;
		}

		public class DoWorkResult
		{
			public Exception Error { get; set; }

			public bool Cancelled { get; set; }

			//public T Result { get; set; }
		}
	}
}
