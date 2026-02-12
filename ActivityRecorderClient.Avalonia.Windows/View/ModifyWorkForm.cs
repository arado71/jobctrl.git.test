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
using Tct.ActivityRecorderClient.Menu.Management;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Properties;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Controls;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient.View
{
	public partial class ModifyWorkForm : FixedMetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private DeviceWorkInterval originalInterval = null;
		private readonly WorkTimeHistoryForm parent = null;
		private readonly IWorkTimeService service;

		public ModifyWorkForm()
		{
			InitializeComponent();
			lblStart.Text = Labels.Worktime_Start;
			lblEnd.Text = Labels.Worktime_End;
			lblWork.Text = Labels.AddMeeting_SelectedWork;
			lblComment.Text = Labels.ModifyWork_Comment;
			rbModify.Text = Labels.ModifyWork_Modify;
			rbDelete.Text = Labels.ModifyWork_Delete;
			toolTip1.SetToolTip(rbModify, Labels.ModifyWork_ModifyWorkDescription);
			toolTip1.SetToolTip(rbDelete, Labels.ModifyWork_DeleteWorkDescription);
			btnOk.Text = Labels.Ok;
			btnCancel.Text = Labels.Cancel;
			Text = Labels.ModifyWork_ModifyTitle;
			Icon = Resources.JobCtrl;
			workSelector.UpdateMenu(MenuQuery.Instance.ClientMenuLookup.Value);
			MenuQuery.Instance.ClientMenuLookup.Changed += HandleClientMenuUpdated;
			workSelector.SetCueBanner(Labels.ModifyWork_NoChange);
		}

		public ModifyWorkForm(IWorkTimeService service, WorkTimeHistoryForm parent)
			: this()
		{
			this.parent = parent;
			this.service = service;
		}

		public void SetWork(DeviceWorkInterval interval)
		{
			this.originalInterval = interval;
			dtpLocalStart.Value = interval.StartDate.FromUtcToLocal();
			dtpLocalEnd.Value = interval.EndDate.FromUtcToLocal();
			workSelector.SelectedItem = workSelector.Items.Cast<WorkDataWithParentNames>().FirstOrDefault(x => x.WorkData.Id == interval.WorkId);
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

		private void CloseWithSuccess(bool result)
		{
			Debug.Assert(result);
			log.Debug("Closing window with success");
			DialogResult = DialogResult.OK;
			Close();
		}

		private void ProcessFreeIntervalResponse(Interval interval, IList<Interval> newIntvals, WorkDataWithParentNames workData, string comment)
		{
			var oldIntervalLength = (interval.EndDate - interval.StartDate);
			var newIntervalLength = Interval.GetLength(newIntvals);
			if (newIntervalLength != oldIntervalLength)
			{
				if (newIntervalLength.Ticks == 0)
				{
					MessageBox.Show(Labels.ModifyWork_NoValidInterval, Labels.Warning, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}

				var text = string.Format(Labels.ModifyWork_Overlap, oldIntervalLength.ToHourMinuteSecondString(), newIntervalLength.ToHourMinuteSecondString(), newIntvals.Count);
				if (MessageBox.Show(this, text, Labels.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
				{
					return;
				}
			}

			BackgroundForcableQuery(x => service.ModifyWork(originalInterval, workData, newIntvals, comment, x), CloseWithSuccess, Labels.ModifyWork_FailModify);
		}


		private void ValidateAndModify()
		{
			var interval = GetValidatedInterval();
			var workData = GetValidatedWorkData(false);
			var comment = GetValidatedComments(true);
			if (interval == null || comment == null) return;
			BackgroundQuery(() => service.GetFreeIntervals(interval, originalInterval), x => ProcessFreeIntervalResponse(interval, x, workData, comment), Labels.ModifyWork_ConnectionError);
		
		}

		private void ValidateAndDelete()
		{
			var comment = GetValidatedComments(true);
			if (comment == null) return;
			//var dialogResult = MessageBox.Show(this,
			//	Labels.ModifyWork_SureDelete,
			//	Labels.Warning,
			//	MessageBoxButtons.YesNo,
			//	MessageBoxIcon.Question);
			//if (dialogResult != DialogResult.Yes) return;
			BackgroundForcableQuery(x => service.DeleteWork(originalInterval, comment, x), CloseWithSuccess, Labels.ModifyWork_FailDelete);
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (rbModify.Checked)
			{
				log.Debug("UI - Modify clicked");
				TelemetryHelper.RecordFeature("WorktimeModification", "ModifyWork");
				ValidateAndModify();
			}
			else
			{
				log.Debug("UI - Delete clicked");
				TelemetryHelper.RecordFeature("WorktimeModification", "DeleteWork");
				ValidateAndDelete();
			}
		}

		protected override void SetBusyImpl(bool isBusy)
		{
			btnOk.Enabled = !isBusy;
			btnCancel.Enabled = !isBusy;
		}

		private void HandleDateChanged(object sender, EventArgs e)
		{
			SetSelection(dtpLocalStart.Value, dtpLocalEnd.Value);
			dtpLocalStart.Format = dtpLocalStart.Value.Date == DateTime.Today ? DateTimePickerFormat.Time : DateTimePickerFormat.Custom;
			dtpLocalEnd.Format = dtpLocalEnd.Value.Date == DateTime.Today ? DateTimePickerFormat.Time : DateTimePickerFormat.Custom;
		}

		private void SetPropertyVisibility(bool isVisible)
		{
			workSelector.Visible = isVisible;
			dtpLocalStart.Visible = isVisible;
			dtpLocalEnd.Visible = isVisible;
			if (isVisible)
			{
				tableLayoutPanel1.RowStyles[1].SizeType = SizeType.AutoSize;
				tableLayoutPanel1.RowStyles[2].SizeType = SizeType.AutoSize;
				tableLayoutPanel1.RowStyles[3].SizeType = SizeType.AutoSize;
				tableLayoutPanel1.RowStyles[4].SizeType = SizeType.AutoSize;
			}
			else
			{
				for (int i = 1; i < 5; i++)
				{
					tableLayoutPanel1.RowStyles[i].SizeType = SizeType.Absolute;
					tableLayoutPanel1.RowStyles[i].Height = 0;
				}
			}
		}

		private void HandleRadioChanged(object sender, EventArgs e)
		{
			SetPropertyVisibility(rbModify.Checked);
		}
	}
}
