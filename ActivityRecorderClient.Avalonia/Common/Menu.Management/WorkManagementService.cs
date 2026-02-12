using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderService.WorkManagement;

namespace Tct.ActivityRecorderClient.Menu.Management
{
	//todo get the full name of the work
	public abstract class WorkManagementService : IWorkManagementService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected readonly object thisLock = new object();
		private readonly CoordinatedTaskReasonsManager taskReasonsManager = new CoordinatedTaskReasonsManager();
		protected readonly CurrentWorkController currentWorkController;
		private readonly INotificationService notificationService;
		private readonly WorkItemManager workItemManager;
		private int lastCheck;
		private SimpleWorkTimeStats totalStats;
		private CannedCloseReasons closeReasons;
		private ClientMenuLookup clientMenuLookup = new ClientMenuLookup(); //this can be only used from the gui thread!
		protected TaskReasons taskReasons;

		protected abstract void DisplayWorkDetailsGuiImpl(WorkData work, CannedCloseReasons cannedReasons, WorkDetailsFormState formState, Action guiClosed);

		public event EventHandler MenuRefreshNeeded;

		protected WorkManagementService(INotificationService notificationService, CurrentWorkController currentWorkController, WorkItemManager workItemManager)
		{
			ResetLastChecked();
			this.notificationService = notificationService;
			this.currentWorkController = currentWorkController;
			this.workItemManager = workItemManager;
			foreach (int value in Enum.GetValues(typeof(NotificationTypes)))
			{
				displayedNotifications[value] = new HashSet<int>();
			}
			taskReasonsManager.TaskReasonsChanged += TaskReasonsChanged; //this is not water-tight (but good enough) we should know if stats are received (not when changed)
		}

		private int isDisposed;
		public void Dispose()
		{
			if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
			taskReasonsManager.TaskReasonsChanged -= TaskReasonsChanged;
		}

		private void ResetLastChecked()
		{
			Interlocked.Exchange(ref lastCheck, Environment.TickCount - checkInterval - 1);
		}

		private void TaskReasonsChanged(object sender, SingleValueEventArgs<TaskReasons> e) //called from BG
		{
			ResetLastChecked();
			lock (thisLock)
				taskReasons = e.Value;
			log.Debug("TaskReasons updated");
			var del = OnTaskReasonsChanged;
			if (del != null) del(this, e);
		}

		public event EventHandler<SingleValueEventArgs<TaskReasons>> OnTaskReasonsChanged;

		public void Start()
		{
			taskReasonsManager.LoadData();
			taskReasonsManager.Start();
		}

		public void Stop()
		{
			taskReasonsManager.Stop();
		}

		public void SetSimpleWorkTimeStats(SimpleWorkTimeStats stats) //called from BG or GUI
		{
			lock (thisLock)
			{
				totalStats = stats;
			}
			ResetLastChecked();
			log.Debug("Work time stats updated");
		}

		public void SetCannedCloseReasons(CannedCloseReasons reasons) //called from BG
		{
			lock (thisLock)
			{
				closeReasons = reasons;
			}
			if (reasons == null || reasons.TreeRoot == null)
			{
				log.Debug("Loaded close reasons are empty");
			}
			else
			{
				//reasons.DefaultReasons.ForEach(n => log.Debug("Loaded close reason: " + n));
				log.Debug("Loaded close reasons are " + (reasons.IsReadonly ? "" : "Not ") + "Readonly");
			}
		}

		public void UpdateMenu(ClientMenuLookup value) //called from the GUI
		{
			clientMenuLookup = value;
			ResetLastChecked();
			log.Debug("Menu updated");
		}

		private enum NotificationTypes
		{
			TargetTimeOver,
			TargetDateOver,
		}
		private readonly HashSet<int>[] displayedNotifications = new HashSet<int>[Enum.GetValues(typeof(NotificationTypes)).Length];
		private static readonly int checkInterval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
		public bool DisplayWarnNotificationIfApplicable()  //called from the GUI
		{
			if ((uint)(Environment.TickCount - Interlocked.CompareExchange(ref lastCheck, 0, 0)) < checkInterval) return false;
			Interlocked.Exchange(ref lastCheck, Environment.TickCount);
			DisplayReasonWorkGuisIfApplicable();
			SimpleWorkTimeStats stats;
			lock (thisLock)
			{
				stats = totalStats;
			}

			var applicableItems = Enum.GetValues(typeof(NotificationTypes))
				.Cast<int>()
				.Select(_ => new List<WorkDataWithParentNames>())
				.ToArray();
			foreach (var work in clientMenuLookup.WorkDataById.Values)
			{
				if (!work.WorkData.Id.HasValue) continue;
				var workId = work.WorkData.Id.Value;

				if (work.WorkData.EndDate.HasValue && work.WorkData.EndDate.Value.Date.AddDays(1) < DateTime.Now) //local times
				{
					var idx = (int)NotificationTypes.TargetDateOver;
					if (displayedNotifications[idx].Add(workId))
					{
						applicableItems[idx].Add(work);
					}
				}

				SimpleWorkTimeStat stat;
				if (stats != null
					&& work.WorkData.TargetTotalWorkTime.HasValue
					&& stats.Stats.TryGetValue(workId, out stat)
					&& work.WorkData.TargetTotalWorkTime.Value < stat.TotalWorkTime)
				{
					var idx = (int)NotificationTypes.TargetTimeOver;
					if (displayedNotifications[idx].Add(workId))
					{
						applicableItems[idx].Add(work);
					}
				}
			}

			MessageWithActions msg = null;
			foreach (NotificationTypes notType in Enum.GetValues(typeof(NotificationTypes)))
			{
				var items = applicableItems[(int)notType];
				if (items.Count == 0) continue;
				if (msg == null)
				{
					msg = new MessageWithActions();
				}
				else
				{
					msg.AppendLine();
				}
				msg.AppendLine(GetLabelFor(notType));
				foreach (var item in items)
				{
					msg.Append(" - ");
					var workData = item.WorkData;
					msg.Append(item.FullName, () =>
					{
						log.Debug("UI - Deadline work link clicked");
						DisplayWorkDetailsGui(workData);
					});
					msg.AppendLine();
				}
			}

			if (msg == null)
			{
				return false;
			}

			log.Info("Deadlines reached:" + Environment.NewLine + msg.GetText());
			notificationService.ShowNotification(NotificationKeys.WmsWarn + DateTime.UtcNow.Ticks, TimeSpan.Zero, Labels.WmsWarnTitle, msg);
			return true;
		}

		private readonly Dictionary<int, int> currentCloseNotifications = new Dictionary<int, int>();
		private void DisplayReasonWorkGuisIfApplicable() //called from the GUI
		{
			SimpleWorkTimeStats totStats;
			lock (thisLock)
			{
				totStats = totalStats;
			}
			var taskReasons = taskReasonsManager.GetSyncedTaskReasons();
			if (taskReasons == null || taskReasons.ReasonsByWorkId == null) return; //if we don't know that we have any reasons we cannot do anything

			foreach (var work in clientMenuLookup.WorkDataById.Values)
			{
				if (!work.WorkData.Id.HasValue) continue;
				var workId = work.WorkData.Id.Value;
				int reasonCount = 0;
				List<Reason> reasons;
				if (taskReasons.ReasonsByWorkId.TryGetValue(workId, out reasons))
				{
					reasonCount = reasons.Count;
				}
				int visibleReasons;
				if (currentCloseNotifications.TryGetValue(workId, out visibleReasons))
				{
					reasonCount += visibleReasons;
					continue; //if notification is visible don't show an other one
				}

				if (reasonCount == 0 //only displayed at most once
					&& work.WorkData.CloseReasonRequiredDate.HasValue
					&& work.WorkData.CloseReasonRequiredDate.Value < DateTime.UtcNow) //not local times
				{
					DisplayReasonWorkGui(work.WorkData);
					continue;
				}

				SimpleWorkTimeStat stat;
				if (totStats == null
					|| !work.WorkData.CloseReasonRequiredTime.HasValue
					|| !totStats.Stats.TryGetValue(workId, out stat)
					)
				{
					continue;
				}

				var time = stat.TotalWorkTime - work.WorkData.CloseReasonRequiredTime.Value;
				var repeatInterval = work.WorkData.CloseReasonRequiredTimeRepeatInterval.GetValueOrDefault(TimeSpan.Zero);
				var repeatCount = work.WorkData.CloseReasonRequiredTimeRepeatCount.GetValueOrDefault();
				var maxReasons = Math.Max(0, repeatCount) + 1;
				var reasonsNeeded = 0;
				while (time > TimeSpan.Zero)
				{
					reasonsNeeded++;
					if (repeatInterval <= TimeSpan.Zero || reasonsNeeded == maxReasons) //maxReasons is the upper limit of reasons
					{
						break;
					}
					time -= repeatInterval;
				}

				if (reasonCount < reasonsNeeded)
				{
					DisplayReasonWorkGui(work.WorkData);
				}
			}
		}

		private static string GetLabelFor(NotificationTypes value)
		{
			switch (value)
			{
				case NotificationTypes.TargetTimeOver:
					return Labels.WmsWarnTargetTimeOverBody;
				case NotificationTypes.TargetDateOver:
					return Labels.WmsWarnTargetDateOverBody;
				default:
					Debug.Fail("No name for " + value);
					return Enum.GetName(typeof(NotificationTypes), value);
			}
		}

		public void DisplayCloseWorkGui(WorkData workToClose)
		{
			if (!workToClose.IsWorkIdFromServer) return;
			int shown;
			if (!currentCloseNotifications.TryGetValue(workToClose.Id.Value, out shown))
			{
				shown = 0;
			}
			currentCloseNotifications[workToClose.Id.Value] = shown + 1;
			DisplayWorkDetailsGuiImpl(workToClose, GetCloseReasons(), WorkDetailsFormState.CloseWork, () => GuiClosed(workToClose));
		}

		public void DisplayReasonWorkGui(WorkData workToComment)
		{
			if (!workToComment.IsWorkIdFromServer) return;
			int shown;
			if (!currentCloseNotifications.TryGetValue(workToComment.Id.Value, out shown))
			{
				shown = 0;
			}
			currentCloseNotifications[workToComment.Id.Value] = shown + 1;
			DisplayWorkDetailsGuiImpl(workToComment, GetCloseReasons(), WorkDetailsFormState.AddReason, () => GuiClosed(workToComment));
		}

		public void DisplayWorkDetailsGui(WorkData workToShow)
		{
			if (!workToShow.IsWorkIdFromServer) return;
			int shown;
			if (!currentCloseNotifications.TryGetValue(workToShow.Id.Value, out shown))
			{
				shown = 0;
			}
			currentCloseNotifications[workToShow.Id.Value] = shown + 1;
			DisplayWorkDetailsGuiImpl(workToShow, GetCloseReasons(), WorkDetailsFormState.Information, () => GuiClosed(workToShow));
		}

		public void DisplayCreateWorkGui()
		{
			DisplayWorkDetailsGuiImpl(null, null, WorkDetailsFormState.CreateWork, () => { });
		}

		public void DisplayUpdateWorkGui(WorkData workToUpdate)
		{
			DisplayWorkDetailsGuiImpl(workToUpdate, null, WorkDetailsFormState.UpdateWork, () => { }); //we should create a copy of workToUpdate but KISS and we promise not to modify it
		}

		public TimeSpan? GetTotalWorkTimeForWork(int workId)
		{
			lock (thisLock)
			{
				SimpleWorkTimeStat workStat;
				if (totalStats == null || !totalStats.Stats.TryGetValue(workId, out workStat)) return null;
				return workStat.TotalWorkTime;
			}
		}

		protected void GuiClosed(WorkData work)
		{
			int shown;
			if (currentCloseNotifications.TryGetValue(work.Id.Value, out shown))
			{
				if (shown == 1)
				{
					currentCloseNotifications.Remove(work.Id.Value);
				}
				else
				{
					currentCloseNotifications[work.Id.Value] = shown - 1;
				}
			}
		}

		private CannedCloseReasons GetCloseReasons()
		{
			CannedCloseReasons reasons;
			lock (thisLock)
			{
				reasons = closeReasons;
			}
			return reasons;
		}

		protected CloseResult CloseWork(ModifyParams closeParams) //this should never throw, executed on bg thread
		{
			var result = new CloseResult();
			var workId = -1;
			int? selectedReasonId;
			try
			{
				taskReasonsManager.Execute(() =>
				{
					//todo check permission ?
					var reason = (closeParams.Reason ?? "").Trim();
					if (string.IsNullOrEmpty(reason)) reason = null;
					workId = closeParams.WorkData.Id.Value;
					selectedReasonId = closeParams.SelectedReasonId;
					var closeResult = ActivityRecorderClientWrapper.Execute(n => n.CloseWork(ConfigManager.UserId, workId, reason, selectedReasonId));
					result.Result = closeResult;
					log.Info("Close work id: " + workId + " reason: " + reason + " result: " + closeResult);
					if (closeResult == CloseWorkResult.Ok || closeResult == CloseWorkResult.AlreadyClosed)
					{
						OnMenuRefreshNeeded(); //this shouldn't take long because GUI is disabled
					}
				});
			}
			catch (Exception ex)
			{
				log.Error("Unable to close work id: " + workId, ex); //we always log on error
				//WcfExceptionLogger.LogWcfError("close work", log, ex);
				result.Exception = ex;
			}
			return result;
		}

		protected ReasonResult AddReason(ModifyParams addReasonParams) //this should never throw, executed on bg thread
		{
			var result = new ReasonResult();
			var workId = -1;
			string reason;
			try
			{
				reason = (addReasonParams.Reason ?? "").Trim();
				if (string.IsNullOrEmpty(reason)) reason = null;

				var reasonItem = new ReasonItem()
				{
					Id = Guid.NewGuid(),
					Reason = reason,
					ReasonItemId = addReasonParams.SelectedReasonId,
					StartDate = DateTime.UtcNow,
					UserId = ConfigManager.UserId,
					WorkId = addReasonParams.WorkData.Id.Value,
				};

				if (reasonItem.Reason == null && !reasonItem.ReasonItemId.HasValue) //we know it won't succeed without server round trip
				{
					result.ReasonRequired = true;
					log.Info("Reason required for add reason for work : " + workId);
					return result;
				}
				try
				{
					taskReasonsManager.Execute(() =>
					{
						var reasonCount = ActivityRecorderClientWrapper.Execute(n => n.AddReasonEx(reasonItem));
						result.ReasonCount = reasonCount;
						log.Info("Add reason work id: " + workId + " reasonId: " + reasonItem.ReasonItemId + " reason: " + reason +
						         " reasonCount: " + reasonCount);
					});
				}
				catch (CommunicationException ex)
				{
					log.Info("Failed to add reason immediately", ex);
					workItemManager.PersistAndSend(reasonItem);
					result.IsDeferred = true;
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to add reason work id: " + workId, ex); //we always log on error
				//WcfExceptionLogger.LogWcfError("close work", log, ex);
				result.Exception = ex;
			}
			return result;
		}

		private readonly object cacheLock = new object();
		private readonly Dictionary<int, ProjectManagementConstraints> cachedProjectConstraints = new Dictionary<int, ProjectManagementConstraints>(); //todo persist, expire?

		public ProjectManagementConstraints GetProjectManagementConstraintsOrCached(int projectId) //this should never throw, executed on bg thread
		{
			lock (cacheLock)
			{
				ProjectManagementConstraints constraints;
				if (cachedProjectConstraints.TryGetValue(projectId, out constraints)) return constraints;
			}
			return GetProjectManagementConstraints(projectId);
		}

		public ProjectManagementConstraints GetProjectManagementConstraintsCachedOnly(int projectId)
		{
			lock (cacheLock)
			{
				ProjectManagementConstraints constraints;
				return cachedProjectConstraints.TryGetValue(projectId, out constraints) ? constraints : null;
			}
		}

		public bool RemoveProjectManagementConstraintsCache(int projectId)
		{
			lock (cacheLock)
			{
				return cachedProjectConstraints.Remove(projectId);
			}
		}

		private ProjectManagementConstraints GetProjectManagementConstraints(int projectId)
		{
			try
			{
				var constraints = ActivityRecorderClientWrapper.Execute(n => n.GetProjectManagementConstraints(ConfigManager.UserId, projectId));
				lock (cacheLock)
				{
					cachedProjectConstraints[projectId] = constraints;
				}
				return constraints;
			}
			catch (Exception ex)
			{
				log.Error("Unable to GetProjectManagementConstraints projectId: " + projectId, ex); //we always log on error
				return null;
			}
		}

		public GeneralResult<int> CreateWork(int projectId, WorkData workData) //this should never throw, executed on bg thread
		{
			try
			{
				Validate(ProjectManagementPermissions.CreateWork, projectId, null, workData);

				var workId = ActivityRecorderClientWrapper.Execute(n => n.CreateWork(ConfigManager.UserId, projectId, workData));
				lock (cacheLock)
				{
					ProjectManagementConstraints constraints;
					if (cachedProjectConstraints.TryGetValue(projectId, out constraints))
					{
						//we don't care about constraints.WorkMaxTargetCost atm.
						if (constraints.WorkMaxTargetWorkTime.HasValue) constraints.WorkMaxTargetWorkTime -= workData.TargetTotalWorkTime;
					}
				}
				OnMenuRefreshNeeded();
				return new GeneralResult<int>() { Result = workId };
			}
			catch (Exception ex)
			{
				log.Error("Unable to CreateWork under projectId: " + projectId, ex); //we always log on error
				return new GeneralResult<int>() { Exception = ex };
			}
		}

		public GeneralResult<bool> UpdateWork(int projectId, WorkData original, WorkData updated) //this should never throw, executed on bg thread
		{
			try
			{
				Validate(ProjectManagementPermissions.ModifyWork, projectId, original, updated);

				ActivityRecorderClientWrapper.Execute(n => n.UpdateWork(ConfigManager.UserId, updated));
				lock (cacheLock)
				{
					ProjectManagementConstraints constraints;
					if (cachedProjectConstraints.TryGetValue(projectId, out constraints))
					{
						//we don't care about constraints.WorkMaxTargetCost atm.
						var tagetWorkTimeDiff = Diff(original.TargetTotalWorkTime, updated.TargetTotalWorkTime);
						if (constraints.WorkMaxTargetWorkTime.HasValue && tagetWorkTimeDiff.HasValue) constraints.WorkMaxTargetWorkTime -= tagetWorkTimeDiff;
					}
				}
				OnMenuRefreshNeeded();
				return new GeneralResult<bool>() { Result = true };
			}
			catch (Exception ex)
			{
				log.Error("Unable to UpdateWork workId: " + original.Id, ex); //we always log on error
				return new GeneralResult<bool>() { Exception = ex };
			}
		}

		public static TimeSpan? Diff(TimeSpan? oldValue, TimeSpan? newValue)
		{
			if (!newValue.HasValue) return -oldValue;
			if (!oldValue.HasValue) return newValue;
			return newValue - oldValue;
		}

		private void Validate(ProjectManagementPermissions permission, int projectId, WorkData originalWorkData, WorkData newWorkData)
		{
			var manConstraints = GetProjectManagementConstraintsOrCached(projectId);
			if (!ValidatePermission(manConstraints, permission))
			{
				throw new Exception("Access denied.");
			}
			string propertyName;
			if (!ValidateMandatoryFields((ManagementFields)manConstraints.WorkMandatoryFields, newWorkData, out propertyName))
			{
				throw new Exception("Mandatory field missing: " + propertyName);
			}
			if (!ValidateLimits(manConstraints, newWorkData.StartDate, newWorkData.EndDate, Diff(originalWorkData == null ? null : originalWorkData.TargetTotalWorkTime, newWorkData.TargetTotalWorkTime), out propertyName))
			{
				throw new Exception("Limit violated: " + propertyName);
			}
		}

		public static bool ValidateLimits(ProjectManagementConstraints constraints, DateTime? startDate, DateTime? endDate, TimeSpan? targetWorkTimeDiff, out string properyName)
		{
			properyName = null;
			if (startDate.HasValue ^ endDate.HasValue) //Both StartDate and EndDate must be set to null or to a value this is enforced by the web
			{
				properyName = "EndDate";
			}

			if (endDate.HasValue
				&& startDate.HasValue
				&& endDate.Value.AddDays(1) <= startDate.Value)
			{
				properyName = "EndDate";
			}

			if (constraints.WorkMaxEndDate.HasValue
				&& endDate.HasValue
				&& endDate.Value > constraints.WorkMaxEndDate.Value)
			{
				properyName = "EndDate";
			}

			if (constraints.WorkMinStartDate.HasValue
				&& startDate.HasValue
				&& startDate.Value < constraints.WorkMinStartDate.Value)
			{
				properyName = "StartDate";
			}
			if (constraints.WorkMaxTargetWorkTime.HasValue
				&& targetWorkTimeDiff.HasValue
				&& targetWorkTimeDiff.Value > constraints.WorkMaxTargetWorkTime.Value)
			{
				properyName = "TargetTotalWorkTime";
			}
			//we cannot set TargetCost atm. so it is not checked
			return properyName == null;
		}

		public static bool ValidateMandatoryFields(ManagementFields workMandatoryFields, WorkData workData, out string properyName)
		{
			properyName = null;
			if ((workMandatoryFields & ManagementFields.Category) != 0 && !workData.CategoryId.HasValue) properyName = "CategoryId";
			if ((workMandatoryFields & ManagementFields.Description) != 0 && string.IsNullOrEmpty(workData.Description)) properyName = "Description";
			if ((workMandatoryFields & ManagementFields.Priority) != 0 && !workData.Priority.HasValue) properyName = "Priority";
			if ((workMandatoryFields & ManagementFields.StartEndDate) != 0 && !workData.StartDate.HasValue) properyName = "StartDate";
			if ((workMandatoryFields & ManagementFields.StartEndDate) != 0 && !workData.EndDate.HasValue) properyName = "EndDate";
			if ((workMandatoryFields & ManagementFields.TargetWorkTime) != 0 && !workData.TargetTotalWorkTime.HasValue) properyName = "TargetTotalWorkTime";
			if ((workMandatoryFields & ManagementFields.TargetCost) != 0) properyName = "TargetCost"; //cannot manage works if TargetCost is required
			return properyName == null;
		}

		protected bool HasAccess(int projectId, ProjectManagementPermissions permission)
		{
			var manConstraints = GetProjectManagementConstraintsOrCached(projectId);
			return ValidatePermission(manConstraints, permission);
		}

		private const ProjectManagementPermissions createOrUpdate = ProjectManagementPermissions.CreateWork | ProjectManagementPermissions.ModifyWork;
		private static bool ValidatePermission(ProjectManagementConstraints constraints, ProjectManagementPermissions permission)
		{
			return constraints != null
				&& ((ProjectManagementPermissions)constraints.ProjectManagementPermissions & permission) != 0
				&& (createOrUpdate & permission) != 0 && ((ManagementFields)constraints.WorkMandatoryFields & ManagementFields.TargetCost) == 0; //if TargetCost is mandatory then we cannot create/update works
		}

		private void OnMenuRefreshNeeded()
		{
			var del = MenuRefreshNeeded;
			if (del != null) del(this, EventArgs.Empty);
		}

		public class ModifyParams
		{
			public WorkData WorkData { get; set; }
			public string Reason { get; set; }
			public int? SelectedReasonId { get; set; }
		}

		public class CloseResult
		{
			public CloseWorkResult Result { get; set; }
			public Exception Exception { get; set; }
		}

		public class ReasonResult
		{
			public int? ReasonCount { get; set; }
			public bool ReasonRequired { get; set; }
			public Exception Exception { get; set; }
			/// <summary>
			/// True if Reasons can't be sent for server immediately, but scheduled for later synchronization.
			/// </summary>
			public bool IsDeferred { get; set; }
		}
	}
}
