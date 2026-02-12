using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient.View
{
	public partial class ModifyIntervalForm : FixedMetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly WorkTimeHistoryForm parent = null;
		private readonly IWorkTimeService service;

		public ModifyIntervalForm()
		{
			InitializeComponent();
			lblStart.Text = Labels.Worktime_Start;
			lblEnd.Text = Labels.Worktime_End;
			lblWork.Text = Labels.AddMeeting_SelectedWork;
			lblComment.Text = Labels.ModifyWork_Comment;
			rbAdd.Text = Labels.ModifyWork_Create;
			rbModify.Text = Labels.ModifyWork_Modify;
			rbDelete.Text = Labels.ModifyWork_Delete;
			toolTip1.SetToolTip(rbAdd, Labels.ModifyWork_AddIntervalDescription);
			toolTip1.SetToolTip(rbModify, Labels.ModifyWork_ModifyIntervalDescription);
			toolTip1.SetToolTip(rbDelete, Labels.ModifyWork_DeleteIntervalDescription);
			btnOk.Text = Labels.Ok;
			btnCancel.Text = Labels.Cancel;
			Text = Labels.ModifyWork_IntervalTitle;
			Icon = Resources.JobCtrl;
			if (ConfigManager.OnlyDesktopTasksInWorktimeMod)
				workSelector.CanSelectWork = d => (d.Visibility & WorkData.WorkDataVisibilityType.HideInMenu) == 0;
			workSelector.UpdateMenu(MenuQuery.Instance.ClientMenuLookup.Value);
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleClientMenuUpdated;
		}

		public ModifyIntervalForm(IWorkTimeService service, WorkTimeHistoryForm parent)
			: this()
		{
			this.parent = parent;
			this.service = service;
		}

		public void SetInterval(DateTime localStart, DateTime localEnd)
		{
			dtpLocalStart.Value = localStart;
			dtpLocalEnd.Value = localEnd;
		}

		protected void SetSelection(DateTime localStart, DateTime localEnd)
		{
			if (parent != null && !parent.IsDisposed)
			{
				parent.SetSelection(localStart, localEnd);
			}
		}

		protected void ClearSelection()
		{
			if (parent != null && !parent.IsDisposed)
			{
				parent.ResetSelection();
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void HandleClientMenuUpdated(object sender, EventArgs e)
		{
			workSelector.UpdateMenu(MenuQuery.Instance.ClientMenuLookup.Value);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			MenuQuery.Instance.ClientMenuLookup.Changed -= HandleClientMenuUpdated;
			ClearSelection();
			base.OnClosing(e);
		}

		private Interval GetValidatedInterval()
		{
			if (dtpLocalStart.Value >= dtpLocalEnd.Value)
			{
				errorProvider1.SetError(dtpLocalStart, Labels.ModifyWork_InvalidInterval);
				return null;
			}

			errorProvider1.SetError(dtpLocalStart, "");
			return new Interval { StartDate = dtpLocalStart.Value.FromLocalToUtc(), EndDate = dtpLocalEnd.Value.FromLocalToUtc() };
		}

		private WorkDataWithParentNames GetValidatedWorkData(bool mandatory)
		{
			var selectedItem = workSelector.SelectedItem as WorkDataWithParentNames;
			if (selectedItem == null && mandatory)
			{
				errorProvider1.SetError(workSelector, Labels.NewWork_Mandatory);
				return null;
			}

			errorProvider1.SetError(workSelector, "");
			return selectedItem;
		}

		private string GetValidatedComments(bool mandatory)
		{
			var comment = txtComment.Text;
			if (string.IsNullOrEmpty(comment) && mandatory)
			{
				errorProvider1.SetError(txtComment, Labels.NewWork_Mandatory);
				return null;
			}

			errorProvider1.SetError(txtComment, "");
			return comment;
		}

		private void ValidateAndAdd()
		{
			var interval = GetValidatedInterval();
			var comment = GetValidatedComments(true);
			var work = GetValidatedWorkData(true);
			if (interval == null || comment == null || work == null) return;
			BackgroundForcableQuery(x => service.CreateWork(work, interval, comment, x), CloseWithSuccess, Labels.ModifyWork_ConnectionError);
		}

		private void ValidateAndModify()
		{
			var interval = GetValidatedInterval();
			var comment = GetValidatedComments(true);
			var work = GetValidatedWorkData(true);
			if (work == null || comment == null || interval == null) return;
			BackgroundForcableQuery(x => service.ModifyInterval(interval, work, comment, x), CloseWithSuccess, Labels.ModifyWork_FailModify);
		}

		private void ValidateAndDelete()
		{
			var interval = GetValidatedInterval();
			var comment = GetValidatedComments(true);
			if (interval == null || comment == null) return;
			BackgroundForcableQuery(x => service.DeleteInterval(interval, comment, x), CloseWithSuccess, Labels.ModifyWork_FailDelete);
		}

		private void CloseWithSuccess(bool success)
		{
			Debug.Assert(success);
			log.Debug("Closing window with success");
			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (rbAdd.Checked)
			{
				log.Debug("UI - Add clicked");
				TelemetryHelper.RecordFeature("WorktimeModification", "AddInterval");
				ValidateAndAdd();
				return;
			}

			if (rbModify.Checked)
			{
				log.Debug("UI - Modify clicked");
				TelemetryHelper.RecordFeature("WorktimeModification", "ModifyInterval");
				ValidateAndModify();
				return;
			}

			if (rbDelete.Checked)
			{
				log.Debug("UI - Delete clicked");
				TelemetryHelper.RecordFeature("WorktimeModification", "DeleteInterval");
				ValidateAndDelete();
				return;
			}

			Debug.Fail("Unkown state");
		}

		protected override void SetBusyImpl(bool isBusy)
		{
			btnOk.Enabled = !isBusy;
			btnCancel.Enabled = !isBusy;
		}

		private void HandleDateChanged(object sender, EventArgs e)
		{
			SetSelection(dtpLocalStart.Value, dtpLocalEnd.Value);
		}

		private void SetPropertyVisibility(bool isVisible)
		{
			workSelector.Visible = isVisible;
			if (isVisible)
			{
				tableLayoutPanel1.RowStyles[3].SizeType = SizeType.AutoSize;
				tableLayoutPanel1.RowStyles[4].SizeType = SizeType.AutoSize;
			}
			else
			{
				tableLayoutPanel1.RowStyles[3].SizeType = SizeType.Absolute;
				tableLayoutPanel1.RowStyles[3].Height = 0;
				tableLayoutPanel1.RowStyles[4].SizeType = SizeType.Absolute;
				tableLayoutPanel1.RowStyles[4].Height = 0;
			}
		}

		private void HandleRadioChanged(object sender, EventArgs e)
		{
			SetPropertyVisibility(!rbDelete.Checked);
		}
	}
}
