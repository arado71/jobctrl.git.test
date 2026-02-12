using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.ProjectServer.Client;
using Tct.ActivityRecorderClient.ProjectSync;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient.View
{
	public partial class ProjectSyncPeriodForm : FixedMetroForm
	{
		private readonly SyncContext context;
		private readonly ProjectSyncService service;
		private readonly SynchronizationContext guiContext;
		private readonly List<TimeSheetPeriod> listedPeriods = new List<TimeSheetPeriod>();

		private void SetBusy(bool isBusy)
		{
			metroButton1.Enabled = !isBusy;
			cbPeriods.Enabled = !isBusy;
		}

		public ProjectSyncPeriodForm(ProjectSyncService service, SyncContext context)
		{
			guiContext = AsyncOperationManager.SynchronizationContext;
			InitializeComponent();
			Icon = Icon = Tct.ActivityRecorderClient.Properties.Resources.JobCtrl;
			this.context = context;
			this.service = service;
			var lastMonth = DateTime.Now.AddMonths(-1);
			foreach (var period in context.Periods.Where(p => p.Start >= lastMonth))
			{
				cbPeriods.Items.Add(period.Start.ToShortDateString() + " - " + period.End.ToShortDateString());
				listedPeriods.Add(period);
			}
		}

		private void HandleUploadClicked(object sender, EventArgs e)
		{
			var checkedPeriods = new List<TimeSheetPeriod>();
			for (var i = 0; i < cbPeriods.Items.Count; ++i)
			{
				if (cbPeriods.GetItemChecked(i))
				{
					checkedPeriods.Add(listedPeriods[i]);
				}
			}

			var shouldSubmit = cbSubmit.Checked;
			SetBusy(true);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					foreach (var period in checkedPeriods)
					{
						service.SyncWorkTime(period, shouldSubmit, context);
					}

					guiContext.Post(__ =>
					{
						SetBusy(false);
						MessageBox.Show(this, "Completed successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
						Close();
					}, null);
				}
				catch (ApplicationException ex)
				{
					string displayMessage;
					switch (ex.Message)
					{
						case "PeriodAlreadySubmitted":
							displayMessage = "One of selected periods already submitted!";
							break;
						case "TimesheetMissing":
							displayMessage = "Timesheet doesn't exist for period.\nPlease create it on 'Manage Timesheet' pane in PWA!";
							break;
						default:
							displayMessage = ex.Message;
							break;
					}
					guiContext.Post(__ =>
					{
						SetBusy(false);
						MessageBox.Show(this, displayMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}, null);
				}
				catch (Exception ex)
				{
					guiContext.Post(__ =>
					{
						SetBusy(false);
						MessageBox.Show(this, "Timesheet upload failed" + Environment.NewLine + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}, null);
				}
			}, null);
		}
	}
}
