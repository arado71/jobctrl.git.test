using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.View;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient.ProjectSync
{
	public class ProjectSyncWinService : ProjectSyncService
	{
		private ProjectSyncPeriodForm periodForm;
		private ProjectSyncForm loginForm = null;
		private readonly SynchronizationContext context;

		public ProjectSyncWinService(WorkTimeService worktimeService, INotificationService notificationService)
			: base(worktimeService, notificationService)
		{
			context = SynchronizationContext.Current;
		}

		public override void ShowInfo(string text)
		{
			context.Post(_ =>
				notificationService.ShowNotification("ProjectSyncService"+text.GetHashCode(), TimeSpan.FromMinutes(5), "Upload timesheet", text), null);
		}

		public override void ShowSync()
		{
			if (SynchronizationContext.Current == null)
			{
				// hax if not from gui thread
				context.Post(_ => ShowSync(), null);
				return;
			}
			if (periodForm != null && !periodForm.IsDisposed)
			{
				periodForm.BringToFront();

				return;
			}

			if (loginForm == null || loginForm.IsDisposed)
			{
				if (!StartProcessIfStopped())
				{
					ShowInfo("Timesheet upload still in progress in the background");
					return;
				}
				loginForm = new ProjectSyncForm(this);
				loginForm.Closed += HandleLoginClosed;
				loginForm.Show();
			}

			loginForm.BringToFront();
		}

		private void HandleLoginClosed(object sender, EventArgs e)
		{
			if (loginForm.DialogResult == DialogResult.OK)
			{
				if (periodForm == null || periodForm.IsDisposed)
				{
					periodForm = new ProjectSyncPeriodForm(this, loginForm.Context);
					periodForm.Closed += (o, args) => EndProcess();
					periodForm.Show();
				}
				
				periodForm.BringToFront();
			}
			else EndProcess();
		}
	}
}
