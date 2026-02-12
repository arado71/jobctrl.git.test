using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Tct.ActivityRecorderClient.Configuration;
using Tct.ActivityRecorderClient.SystemEvents;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Creates wokrItems from captured data on the client. Desktop Captures attached to workItems are received from outside.
	/// </summary>
	public class CaptureManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly long OneMsTicks = TimeSpan.FromMilliseconds(1).Ticks;

		public event EventHandler<WorkItemEventArgs> WorkItemCreated;
		public event EventHandler<SingleValueEventArgs<WorkStatusChange>> WorkStatusChanged;

		protected long TimeSyncThresholdTicks { get { return ConfigManager.TimeSyncThreshold * OneMsTicks; } }
		protected override int ManagerCallbackInterval { get { return ConfigManager.CaptureWorkItemInterval; } }

		private readonly object currentWorkItemLock = new object();
		private WorkItem currentWorkItem;
		private DateTime workIdStartDate;
		// TODO: mac
		//private IpDetector ipDetectorInstance;
		//Stopwatch causes bugs on variable speed CPUs (and it's not totally in sync on multi proc systems)
		//Use Environment.TickCount instead
		//private readonly Stopwatch sw = new Stopwatch();

		private readonly IUserActivityService userActivityService;

		public CaptureManager(IUserActivityService userActivity, ISystemEventsService systemEvents)
			: base(log)
		{
			if (userActivity == null) throw new ArgumentNullException();
			userActivityService = userActivity;
			//ipDetectorInstance = IpDetector.Instance;
			//systemEvents.SessionSwitch += (sender, eventargs) => ipDetectorInstance.HandleSessionSwitch();
		}

		public override void Stop()
		{
			base.Stop();
			StopWork();
		}

		public void StartWork(int workId, DesktopCapture currentCapture = null, AssignData assignData = null)
		{
			Debug.Assert(MenuCoordinator.IsWorkIdFromServer(workId) || assignData != null);
			int userId = ConfigManager.UserId;
			var newWorkItem = GetNewWorkItem(Guid.NewGuid(), workId, userId, assignData);
			FinishAndStartNewCurrentWorkItem(newWorkItem, false, currentCapture);
			if (MenuCoordinator.IsWorkIdFromServer(workId)) //only send status for real workIds
			{
				RaiseWorkStatusChanged(workId, newWorkItem.StartDate);
			}
		}

		public void StopWork()
		{
			var lastWorkItem = FinishAndStartNewCurrentWorkItem(null, false, null);
			if (lastWorkItem != null)
			{
				RaiseWorkStatusChanged(null, lastWorkItem.EndDate);
			}
		}

		public void SetDesktopCapture(DesktopCapture currentCapture)
		{
			if (currentCapture == null) return;
			lock (currentWorkItemLock)
			{
				if (currentWorkItem == null) return;
				currentWorkItem.DesktopCaptures.Add(currentCapture);
			}
		}

		public WorkItemRelativeTime GetWorkItemRelativeTime()
		{
			lock (currentWorkItemLock)
			{
				if (currentWorkItem == null) return null;
				return new WorkItemRelativeTime(currentWorkItem.WorkId, workIdStartDate, currentWorkItem.StartDate, lastTickCount);
			}
		}

		private int lastTickCount;
		//we don't care of the result of the first call, it won't be used as lastWorkItem should be null then
		private TimeSpan GetDurationSinceLastCall()
		{
			var prevTickCount = lastTickCount;
			lastTickCount = Environment.TickCount;
			var durationInMs = (uint)(lastTickCount - prevTickCount);
			return TimeSpan.FromMilliseconds(durationInMs);
		}

		private static readonly TimeSpan tooSmallInterval = TimeSpan.FromMilliseconds(10);
		private WorkItem FinishAndStartNewCurrentWorkItem(WorkItem newWorkItem, bool supressCreatedEvent, DesktopCapture currentCapture)
		{
			WorkItem lastWorkItem;
			lock (currentWorkItemLock)
			{
				var duration = GetDurationSinceLastCall();
				int mouseActivity;
				int keyboardActivity;
				userActivityService.GetAndResetCounters(out keyboardActivity, out mouseActivity);
				lastWorkItem = currentWorkItem;
				if (lastWorkItem != null)
				{
					Debug.Assert(duration >= TimeSpan.Zero);
					Debug.Assert(duration < TimeSpan.FromDays(3)); //this should be 10 mins but we could easily exceed that while debugging
					lastWorkItem.ComputerId = ConfigManager.EnvironmentInfo.ComputerId;
					lastWorkItem.IsRemoteDesktop = ConfigManager.EnvironmentInfo.IsRemoteDesktop;
					lastWorkItem.IsVirtualMachine = ConfigManager.EnvironmentInfo.IsVirtualMachine;
					if (!ConfigManager.ClientDataCollectionSettings.HasValue || ConfigManager.ClientDataCollectionSettings.Value.HasFlag(ClientDataCollectionSettings.PcActivity))
					{
						lastWorkItem.KeyboardActivity = keyboardActivity;
						lastWorkItem.MouseActivity = mouseActivity;
					}
					else
					{
						lastWorkItem.KeyboardActivity = keyboardActivity > 0 ? AppConfig.Current.ActivityValueWhenCollectingDisabled : 0;
						lastWorkItem.MouseActivity = mouseActivity > 0 ? AppConfig.Current.ActivityValueWhenCollectingDisabled : 0;
					}
					lastWorkItem.EndDate = lastWorkItem.StartDate + duration;
					//lastWorkItem.LocalIPAddresses = ipDetectorInstance.NetworkAdapterIPAddressesAndRDPClientAddress.ToList();
					if (newWorkItem != null)
					{
						SyncStartEndDate(newWorkItem, lastWorkItem);
						if (currentCapture != null) newWorkItem.DesktopCaptures.Add(currentCapture);
						//if this is a switch from local work to server work with same assignData then reuse pahseId
						ReusePhaseIdIfApplicable(newWorkItem, lastWorkItem);
						if (newWorkItem.StartDate < lastWorkItem.EndDate && newWorkItem.PhaseId == lastWorkItem.PhaseId)
						{
							//clock manipulation might cause unprocessable data if the paseId is unchanged
							//(StartDate, PhaseId) should be unique
							newWorkItem.PhaseId = Guid.NewGuid();
							log.Warn("Client clock was set back during work");
						}
						else if (lastWorkItem.EndDate - lastWorkItem.StartDate < tooSmallInterval && newWorkItem.PhaseId == lastWorkItem.PhaseId)
						{
							//too small interval can cause 0 long interval on the sql server which will violate
							//the (StartDate, PhaseId) unique key constraint
							newWorkItem.PhaseId = Guid.NewGuid();
							log.Warn("Too small interval was sent");
						}
					}
				}
				if (newWorkItem != null && (lastWorkItem == null || newWorkItem.WorkId != lastWorkItem.WorkId)) workIdStartDate = newWorkItem.StartDate; //todo detect same phaseId
				currentWorkItem = newWorkItem;
			}
			if (lastWorkItem != null)
			{
				if (newWorkItem == null || lastWorkItem.PhaseId != newWorkItem.PhaseId)
				{
					log.InfoFormat("User {0} stopped working on {1}", lastWorkItem.UserId, lastWorkItem.WorkId);
				}
				if (!supressCreatedEvent)
				{
					RaiseWorkItemCreated(lastWorkItem);
				}
			}
			if (newWorkItem != null && (lastWorkItem == null || lastWorkItem.PhaseId != newWorkItem.PhaseId))
			{
				log.InfoFormat("User {0} starts working on {1}", newWorkItem.UserId, newWorkItem.WorkId);
			}
			return lastWorkItem;
		}

		private static void ReusePhaseIdIfApplicable(WorkItem newWorkItem, WorkItem lastWorkItem)
		{
			if (MenuCoordinator.IsWorkIdFromServer(newWorkItem.WorkId)
				&& newWorkItem.AssignData != null)
			{
				if (!MenuCoordinator.IsWorkIdFromServer(lastWorkItem.WorkId)
					&& newWorkItem.AssignData.Equals(lastWorkItem.AssignData))
				{
					//When a local workId is created on the server and assigned to us reuse phaseId to reduce the number of aggregate workitem intervals
					log.Debug("Switch detected from " + lastWorkItem.WorkId + " to " + newWorkItem.WorkId);
					newWorkItem.PhaseId = lastWorkItem.PhaseId; //reuse phaseId to reduce aggregate workitem size
				}

				newWorkItem.AssignData = null; //we don't need AssignData for workId from server, it is a hax only to detect work assignment
			}
		}

		private void SyncStartEndDate(WorkItem newWorkItem, WorkItem lastWorkItem)
		{
			Debug.Assert(newWorkItem != null);
			Debug.Assert(lastWorkItem != null);
			//if (newWorkItem == null || lastWorkItem == null) return;
			var derivation = Math.Abs(lastWorkItem.EndDate.Ticks - newWorkItem.StartDate.Ticks);
			if (derivation < TimeSyncThresholdTicks)
			{
				newWorkItem.StartDate = lastWorkItem.EndDate;
			}
		}

		private static WorkItem GetNewWorkItem(Guid phaseId, int workId, int userId, AssignData assignData)
		{
			return new WorkItem()
			{
				Id = Guid.NewGuid(),
				StartDate = DateTime.UtcNow,
				DesktopCaptures = new List<DesktopCapture>(),
				PhaseId = phaseId,
				WorkId = workId,
				UserId = userId,
				AssignData = assignData,
			};
		}

		protected override void ManagerCallbackImpl()
		{
			WorkItem lastWorkItem;
			int lockTaken;
			lock (currentWorkItemLock)
			{
				if (currentWorkItem == null) return;
				lockTaken = Environment.TickCount;
				var newWorkItem = GetNewWorkItem(currentWorkItem.PhaseId, currentWorkItem.WorkId, currentWorkItem.UserId, currentWorkItem.AssignData);
				lastWorkItem = FinishAndStartNewCurrentWorkItem(newWorkItem, true, null); //don't raise while holding currentWorkItemLock
			}
			CheckExecTime(lockTaken, Environment.TickCount, "CaptureWorkItemImpl in Lock");
			if (lastWorkItem != null)
			{
				RaiseWorkItemCreated(lastWorkItem);
			}
		}

		private const int maxLockHoldTimeInfo = 5000;
		private const int maxLockHoldTimeWarn = 60000;
		private static void CheckExecTime(int start, int end, string errorMethod)
		{
			var duration = (uint)(end - start);
			if (duration > maxLockHoldTimeInfo)
			{
				if (duration > maxLockHoldTimeWarn)
				{
					log.Warn(errorMethod + " executed in " + duration + " ms");
				}
				else
				{
					log.Info(errorMethod + " executed in " + duration + " ms");
				}
			}
		}

		private void RaiseWorkItemCreated(WorkItem wi)
		{
			EventHandler<WorkItemEventArgs> created = WorkItemCreated;
			if (created == null) return;
			try
			{
				WorkItemEventArgs e = new WorkItemEventArgs(wi);
				created(this, e);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in RaiseWorkItemCreated", ex);
			}
		}

		private void RaiseWorkStatusChanged(int? workId, DateTime createDate)
		{
			var changed = WorkStatusChanged;
			if (changed == null) return;
			try
			{
				changed(this, SingleValueEventArgs.Create(new WorkStatusChange() { WorkId = workId, CreateDate = createDate }));
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in RaiseWorkStatusChanged", ex);
			}
		}

		public class WorkItemRelativeTime
		{
			public readonly int WorkId;
			public readonly DateTime FirstStartDate;

			private readonly DateTime lastStartDate;
			private readonly int lastTickCount;

			public WorkItemRelativeTime(int workId, DateTime firstStartDate, DateTime lastStartDate, int lastTickCount)
			{
				WorkId = workId;
				FirstStartDate = firstStartDate;
				this.lastStartDate = lastStartDate;
				this.lastTickCount = lastTickCount;
			}

			public DateTime GetNow()
			{
				return lastStartDate.AddMilliseconds((uint)(Environment.TickCount - lastTickCount));
			}
		}
	}
}
