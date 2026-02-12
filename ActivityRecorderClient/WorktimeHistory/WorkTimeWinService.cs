using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	public class WorkTimeWinService : WorkTimeService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected ModifyIntervalForm intervalForm = null;
		protected ModifyWorkForm workForm = null;
		protected Form parent = null;
		private WorkTimeHistoryForm statsForm = null;

		public WorkTimeWinService(IWorkTimeQuery workTimeHistory, Form parent)
			: base(workTimeHistory)
		{
			this.parent = parent;
		}

		public override void ShowModification(DateTime? localDay = null)
		{
			DebugEx.EnsureGuiThread();
			TelemetryHelper.RecordFeature("WorktimeModification", "Open");
			if (statsForm != null && !statsForm.IsDisposed)
			{
				statsForm.BringToFront();
				return;
			}

			statsForm = new WorkTimeHistoryForm(this) { Owner = parent };
			statsForm.Closed += (_, __) =>
			{
				TelemetryHelper.RecordFeature("WorktimeModification", "Close");
				if (intervalForm != null && !intervalForm.IsDisposed)
				{
					intervalForm.Close();
					intervalForm = null;
				}

				if (workForm != null && !workForm.IsDisposed)
				{
					workForm.Close();
					workForm = null;
				}
			};
			statsForm.Show();
			statsForm.GenerateChart(localDay ?? DateTime.Today);
		}

		public override void ShowModifyWork(DeviceWorkInterval workInterval)
		{
			DebugEx.EnsureGuiThread();
			Point? winPos = null;
			if (intervalForm != null && !intervalForm.IsDisposed)
			{
				winPos = intervalForm.Location;
				intervalForm.Close();
			}

			if (workForm != null && !workForm.IsDisposed)
			{
				workForm.SetWork(workInterval);
				if (winPos != null) workForm.Location = winPos.Value;
				workForm.BringToFront();
				return;
			}

			if (workInterval.IsDeleted)
			{
				if (MessageBox.Show(parent, Labels.Worktime_UndeleteText, Labels.Worktime_UndeleteTitle, MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation) == DialogResult.Yes)
				{
					var context = SynchronizationContext.Current;
					ThreadPool.QueueUserWorkItem(_ =>
					{
						var res = UndeleteWork(workInterval);
						if (res.Exception != null)
						{
							log.Info("Error while calling UndeleteWork", res.Exception);
							context.Post(__ => MessageBox.Show(parent, Labels.ModifyWork_ConnectionError, Labels.Error, MessageBoxButtons.OK,
								MessageBoxIcon.Exclamation), null);
							return;
						}

						context.Post(__ => statsForm.Regenerate(), null);
					});
				}

				return;
			}

			workForm = new ModifyWorkForm(this, statsForm) { Owner = parent };
			workForm.Closed += (_, __) => { if (workForm != null && !workForm.IsDisposed && workForm.DialogResult == DialogResult.OK && statsForm !=null && !statsForm.IsDisposed) statsForm.Regenerate(); };
			workForm.SetWork(workInterval);
			workForm.Show();
			if (winPos != null) workForm.Location = winPos.Value;
		}

		public override void ShowModifyInterval(Interval interval)
		{
			DebugEx.EnsureGuiThread();
			Point? winPos = null;
			if (workForm != null && !workForm.IsDisposed)
			{
				winPos = workForm.Location;
				workForm.Close();
			}

			if (intervalForm != null && !intervalForm.IsDisposed)
			{
				intervalForm.SetInterval(interval.StartDate.FromUtcToLocal(), interval.EndDate.FromUtcToLocal());
				if (winPos != null) intervalForm.Location = winPos.Value;
				intervalForm.BringToFront();
				return;
			}

			intervalForm = new ModifyIntervalForm(this, statsForm) { Owner = parent };
			intervalForm.Closed += (s, _) => { var form = s as FixedMetroForm; if (form != null && form.DialogResult == DialogResult.OK && statsForm != null && !statsForm.IsDisposed) statsForm.Regenerate(); };
			intervalForm.SetInterval(interval.StartDate.FromUtcToLocal(), interval.EndDate.FromUtcToLocal());
			intervalForm.Show();
			if (winPos != null) intervalForm.Location = winPos.Value;
		}
	}
}
