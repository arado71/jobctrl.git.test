using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Menu.Management
{
	public class WorkManagementWinService : WorkManagementService
	{
		private readonly Form owner;

		public WorkManagementWinService(INotificationService notificationService, CurrentWorkController currentWorkController, WorkItemManager workItemManager, Form owner)
			: base(notificationService, currentWorkController, workItemManager)
		{
			this.owner = owner;
		}

		protected override void DisplayWorkDetailsGuiImpl(WorkData work, CannedCloseReasons cannedReasons, WorkDetailsFormState formState, Action guiClosed)
		{
			if (formState == WorkDetailsFormState.CreateWork)
			{
				var cform = new WorkDetailsForm()
				{
					Owner = owner,
					Service = this,
					WorkToModify = null,
					State = WorkDetailsFormState.CreateWork
				};
				cform.Show();
				TelemetryHelper.RecordFeature("WorkDetails", "Open");
				cform.BringToFront();
				cform.Focus();

				cform.FormClosed += (_, __) =>
				{
					TelemetryHelper.RecordFeature("WorkDetails", "Close");
					if (cform.StartWork != null)
					{
						TelemetryHelper.RecordFeature("WorkDetails", "StartNew");
						currentWorkController.StartOrQueueWork(cform.StartWork);
					}
					guiClosed();
				};
				cform.FormClosed += CloseWorkFormClosed;

				return;
			}

			if (!work.IsWorkIdFromServer) return;
			List<Reason> reasons = null;
			lock (thisLock)
			{
				if (taskReasons != null && taskReasons.ReasonsByWorkId != null && work.Id.HasValue)
					taskReasons.ReasonsByWorkId.TryGetValue(work.Id.Value, out reasons);
			}

			var form = new WorkDetailsForm
			{
				Service = this,
				Owner = owner,
				WorkToModify = work,
				State = formState,
				CloseWorkFunc = CloseWork,
				AddReasonFunc = AddReason,
				Reasons = reasons,
			};
			if (work.Id.HasValue) form.RefreshTotalWorkTime(GetTotalWorkTimeForWork(work.Id.Value));
			form.RefreshReasons(cannedReasons);
			var s = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
			form.DesktopLocation = new Point((s.Width - form.Width) / 2, (s.Height - form.Height) / 2);
			form.Show();
			TelemetryHelper.RecordFeature("WorkDetails", "Open");
			form.BringToFront();
			form.Focus();
			form.FormClosed += (_, __) =>
			{
				TelemetryHelper.RecordFeature("WorkDetails", "Close");
				guiClosed();
			};
			form.FormClosed += CloseWorkFormClosed;
		}

		private static void CloseWorkFormClosed(object sender, FormClosedEventArgs e)
		{
			var form = (Form)sender;
			form.Dispose();
		}
	}
}
