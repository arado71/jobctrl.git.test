using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Meeting.CountDown
{
	public abstract class MeetingCountDownService : IMeetingCountDownService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public static string UnfinishedTimedTaskPath { get { return "UnfinishedTimedTask-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<ManualWorkItem>> ManualWorkItemCreated; //todo recentworks?
		public bool IsWorking { get { return CurrentWorkItem != null; } }

		public string StateString { get { return IsWorking ? "countdown" : null; } }
		public bool ResumeWorkOnClose { get; private set; }
		public bool IsPermanent { get; private set; }
		public ManualWorkItem CurrentWorkItem { get; private set; }
		protected INotificationService NotificationService { get; private set; }
		protected CurrentWorkController CurrentWorkController { get; private set; }

		private long StartTicks { get; set; }
		private long EndTicks { get; set; }
		private DateTime? PreviousEndDate = null;
		private bool isStopConfirmationShown;
		private bool isCountUp;
		private WorkData workData;
		private WorkDataWithParentNames workDataWithParentNames;
		protected DateTime endDate;
		protected MeetingCountDownService(INotificationService notificationService, CurrentWorkController currentWorkController)
		{
			NotificationService = notificationService;
			CurrentWorkController = currentWorkController;
		}

		protected abstract void StartWorkGui(WorkDataWithParentNames workDataWithParents, ManualWorkItem itemToCreate, Func<TimeSpan> getElapsedTime, bool isCountUp, Action<bool> onGuiClosed);
		protected abstract void StopWorkGui(bool isForced);

		public void StartWork(WorkData _workData, bool isPermanent, bool isForced)
		{
			workData = _workData;
			if (workData == null || workData.ManualAddWorkDuration == null || !workData.Id.HasValue) return;
			if (CurrentWorkItem != null && CurrentWorkItem.WorkId.HasValue && CurrentWorkItem.WorkId == workData.Id) return; //don't start the same work
			workDataWithParentNames = CurrentWorkController.GetWorkDataWithParentNames(workData.Id.Value);
			if (workDataWithParentNames == null)
			{
				log.ErrorAndFail("Unable to get workDataWithParents for work " + workData.Id);
				return;
			}
			var res = isForced
				? DialogResult.Yes
				: NotificationService.ShowMessageBox(string.Format(Labels.NotificationManualWorkCreateConfimBody, workData.Name),
					Labels.NotificationManualWorkCreateConfimTitle,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);
			log.Info("Manual work time creation of workId " + workData.Id.Value + " confirm: " + res + (isForced ? " Forced" : ""));
			if (res != DialogResult.Yes) return;
			using (CurrentWorkController.MutualWorkTypeCoordinator.StartStateChangeTransaction())
			{
				var currentMutualWork = CurrentWorkController.MutualWorkTypeCoordinator.RequestStopWork(isForced, "CountDown Start");
				if (!currentMutualWork.CanStartWork)
				{
					return;
					//we check after the create confirm msgbox (if we'd check before we'd need to double check here anyway because a form can be created while waiting for confirmation)
				}
				Debug.Assert(!IsWorking);
				IsPermanent = isPermanent;
				ResumeWorkOnClose = currentMutualWork.ResumeWorkOnClose;
				//check if we are effectively working before closing any forms
				isCountUp = workData.ManualAddWorkDuration.Value < TimeSpan.Zero;
				var duration = isCountUp ? -workData.ManualAddWorkDuration.Value : workData.ManualAddWorkDuration.Value;
				StartTicks = DateTime.Now.Ticks;
				EndTicks = StartTicks + (int)duration.TotalMilliseconds * TimeSpan.TicksPerMillisecond;

				var startDate = DateTime.UtcNow;
				endDate = startDate + duration;
				PreviousEndDate = null;
				var manualWorkItem = new ManualWorkItem()
				{
					Id = Guid.NewGuid(),
					UserId = ConfigManager.UserId,
					WorkId = workData.Id.Value,
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					StartDate = startDate,
					EndDate = endDate,
					AssignData = workData.AssignData,
					//todo Comment = ?
				};
				CurrentWorkItem = manualWorkItem; //we are working...
				log.Info("Created manual work time for workId " + workData.Id.Value + " between " + manualWorkItem.StartDate +
						 " and " + manualWorkItem.EndDate);

				IsolatedStorageSerializationHelper.Save(UnfinishedTimedTaskPath,
					new UnfinishedTimedTaskItem(workData, CurrentWorkItem, StartTicks, workDataWithParentNames, ResumeWorkOnClose, IsPermanent));

				StartWorkGui(workDataWithParentNames, manualWorkItem, () => GetElapsedTime(), isCountUp, CountDownFormFormClosed);
			}
		}

		private void LoadWork(UnfinishedTimedTaskItem tti)
		{
			using (CurrentWorkController.MutualWorkTypeCoordinator.StartStateChangeTransaction())
			{
				var currentMutualWork = CurrentWorkController.MutualWorkTypeCoordinator.RequestStopWork(false, "CountDown Start");
				if (!currentMutualWork.CanStartWork)
					return;
				isCountUp = tti.WorkData.ManualAddWorkDuration.HasValue && tti.WorkData.ManualAddWorkDuration.Value < TimeSpan.Zero;
				var duration = isCountUp ? -tti.WorkData.ManualAddWorkDuration.Value : tti.WorkData.ManualAddWorkDuration.Value;
				StartTicks = tti.StartTicks;
				EndTicks = tti.StartTicks + (int)duration.TotalMilliseconds * TimeSpan.TicksPerMillisecond;
				ResumeWorkOnClose = tti.ResumeWorkOnClose;
				CurrentWorkItem = tti.ManualWorkItem;
				IsPermanent = tti.IsPermanent;

				StartWorkGui(
					tti.WorkDataWithParentNames,
					tti.ManualWorkItem,
					() => GetElapsedTime(),
					isCountUp,
					CountDownFormFormClosed);
			}
		}

		private void CountDownFormFormClosed(bool isUserCloseWithoutStaringNew)
		{
			Debug.Assert(IsWorking);
			var tickCount = DateTime.Now.Ticks;
			if (tickCount < EndTicks)
			{
				CreateWorkItem(TimeSpan.FromTicks(tickCount - StartTicks));
				log.Info("Created amended manual work time for workId " + CurrentWorkItem.WorkId + " between " + CurrentWorkItem.StartDate + " and " + CurrentWorkItem.EndDate + " original enddate was " + CurrentWorkItem.OriginalEndDate);
			}
			CurrentWorkItem = null; //we are not working anymore
			if (isUserCloseWithoutStaringNew
				&& ResumeWorkOnClose)
			{
				log.Info("Resuming work");
				CurrentWorkController.UserResumeWork(); //resume work if user closed the form and was working before
			}
		}

		private TimeSpan GetElapsedTime()
		{
			var tickCount = DateTime.Now.Ticks;
			var r = IsWorking && EndTicks > tickCount
				? TimeSpan.FromTicks(tickCount - StartTicks)		// ticks since Start and now-
				: TimeSpan.FromTicks(EndTicks - StartTicks);		// ticks in declared period
			return r;
		}

		public void StopWork(bool isForced)
		{
			if (!IsWorking) return;
			if (!isForced && isStopConfirmationShown) return; //don't shown an other confirmation popup again.
			try
			{
				isStopConfirmationShown = true;
				StopWorkGui(isForced);
			}
			finally
			{
				isStopConfirmationShown = false;
			}
		}

		public MutualWorkTypeInfo RequestStopWork(bool isForced)
		{
			if (IsWorking)
			{
				StopWork(isForced);
			}
			return new MutualWorkTypeInfo() { CanStartWork = !IsWorking, ResumeWorkOnClose = ResumeWorkOnClose };
		}

		public void RequestKickWork()
		{
			if (!IsWorking) return;
			StopWork(false);
		}

		private void OnManualWorkItemCreated(ManualWorkItem manualWorkItem)
		{
			Debug.Assert(ManualWorkItemCreated != null);
			var del = ManualWorkItemCreated;
			if (del == null) return;
			del(this, SingleValueEventArgs.Create(manualWorkItem));
		}

		protected void CreateWorkItem(TimeSpan elapsed)
		{
			if (CurrentWorkItem == null) return;
			var orig = CurrentWorkItem;
			var manualWorkItem = new ManualWorkItem()
			{
				Id = Serialization.GuidHelper.IncreaseGuid(orig.Id), //hax increase Id so it will be loaded after the original so we will less likely hit the "Original item was not received yet" error
				UserId = orig.UserId,
				WorkId = orig.WorkId,
				ManualWorkItemTypeId = orig.ManualWorkItemTypeId,
				StartDate = orig.StartDate,
				EndDate = orig.StartDate + elapsed,
				OriginalEndDate = PreviousEndDate,
				AssignData = orig.AssignData,
			};
			if (endDate > DateTime.MinValue && manualWorkItem.EndDate > endDate) manualWorkItem.EndDate = endDate;
			CurrentWorkItem = manualWorkItem;
			OnManualWorkItemCreated(CurrentWorkItem);
			PreviousEndDate = manualWorkItem.EndDate;
		}

		public void CheckUnfinishedTimedTask()
		{
			UnfinishedTimedTaskItem tti;
			if (IsolatedStorageSerializationHelper.Exists(UnfinishedTimedTaskPath) &&
				IsolatedStorageSerializationHelper.Load(UnfinishedTimedTaskPath, out tti))
				LoadWork(tti);
		}

		protected void DeleteUnfinishedTimedTask()
		{
			if (IsolatedStorageSerializationHelper.Exists(UnfinishedTimedTaskPath))
				IsolatedStorageSerializationHelper.Delete(UnfinishedTimedTaskPath);
		}
	}
}
