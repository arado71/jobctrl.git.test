using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class UserStats : IFilterableStats<UserStats>
	{
		[DataMember]
		public int UserId { get; set; }
		[DataMember]
		public int GroupId { get; set; }
		[DataMember]
		public int CompanyId { get; set; }
		[DataMember]
		public UserStatus Status { get; set; }
		[DataMember]
		public ActiveWindow LastActiveWindow { get; set; }
		[DataMember]
		public List<ScreenShot> LastScreenShots { get; set; }
		[DataMember]
		public int LastMouseActivity { get; set; }
		[DataMember]
		public int LastKeyboardActivity { get; set; }
		[DataMember]
		public UserWorkStats CurrentWork { get; set; }
		[DataMember]
		public List<UserWorkStats> RecentWorks { get; set; }
		[DataMember]
		public DateTime LastUpdateDate { get; set; }
		[DataMember(Order = 2)]
		public float AverageMouseActivity { get; set; }
		[DataMember(Order = 2)]
		public float AverageKeyboardActivity { get; set; }
		[DataMember(Order = 3)]
		public WorkTimeStats WorkTimeStats { get; set; }

		private DateTime LastRealActivity { get; set; }
		private List<ActivityStats> ActivityHistory { get; set; }
		private WorkTimeStatsBuilder WorkTimeStatsBuilder { get; set; }
		private DateTime LastReceiveDate { get; set; }
		private DateTime CurrentWorkEndDate { get; set; }
		private DateTime CurrentWorkEndDateLastUpdate { get; set; }
		private TimeSpan UserClockDiff { get; set; }

		private readonly Dictionary<int, UserWorkStats> workLookup = new Dictionary<int, UserWorkStats>();

		public UserStats()
		{
			RecentWorks = new List<UserWorkStats>();
			LastScreenShots = new List<ScreenShot>();
			ActivityHistory = new List<ActivityStats>();
			WorkTimeStatsBuilder = new WorkTimeStatsBuilder();
		}

		public void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			Debug.Assert(manualWorkItem != null);
			Debug.Assert(manualWorkItem.UserId == UserId);
			Debug.Assert(manualWorkItem.GroupId == GroupId);
			Debug.Assert(manualWorkItem.CompanyId == CompanyId);
			WorkTimeStatsBuilder.AddManualWorkItem(manualWorkItem);

			//todo fix CurrentWork can be null if we have ManualWorkItems only
		}

		public void AddWorkItem(WorkItem item)
		{
			Debug.Assert(item != null);
			Debug.Assert(item.UserId == UserId);
			Debug.Assert(item.GroupId == GroupId);
			Debug.Assert(item.CompanyId == CompanyId);
			WorkTimeStatsBuilder.AddWorkItem(item);
			LastRealActivity = GetLastRealActivity(item, LastRealActivity);
			if (LastReceiveDate < item.ReceiveDate) LastReceiveDate = item.ReceiveDate;
			AddActivityHistory(item);

			bool newData;
			var work = GetWorkAndSetCurrentIfApplicable(item.WorkId, item.StartDate, item.EndDate, out newData);
			work.AddWorkItem(item);
			if (!newData) return;

			var aw = item.ActiveWindows != null ? item.ActiveWindows.LastOrDefault() : null;//no sort for speed
			if (aw != null)
			{
				LastActiveWindow = aw;
			}
			if (item.ScreenShots != null && item.ScreenShots.Count != 0)
			{
				LastScreenShots.Clear();
				if (item.ScreenShots.Count == 1) //fast path
				{
					LastScreenShots.Add(item.ScreenShots[0]);
				}
				else if (item.ScreenShots.Count == 2) //fast path 2
				{
					if (item.ScreenShots[0].ScreenNumber != item.ScreenShots[1].ScreenNumber)
					{
						LastScreenShots.AddRange(item.ScreenShots);
					}
					else
					{
						LastScreenShots.Add(item.ScreenShots[1]);
					}
				}
				else //slow path
				{
					var screenShots = item.ScreenShots.ToLookup(n => n.ScreenNumber);
					foreach (var lookup in screenShots)
					{
						var lastShot = lookup.LastOrDefault(); //no sort for speed
						if (lastShot != null)
						{
							LastScreenShots.Add(lastShot);
						}
					}
				}
			}
			LastMouseActivity = item.MouseActivity;
			LastKeyboardActivity = item.KeyboardActivity;
			LastUpdateDate = item.ReceiveDate;
		}

		private static DateTime GetLastRealActivity(WorkItem item, DateTime previousRealActivity)
		{
			if (item.MouseActivity != 0 || item.KeyboardActivity != 0)
			{
				if (previousRealActivity < item.EndDate)
				{
					return item.EndDate;
				}
			}
			return previousRealActivity;
		}

		private UserWorkStats GetWorkAndSetCurrentIfApplicable(int workId, DateTime startDate, DateTime endDate, out bool newData)
		{
			UserWorkStats work;
			if (!workLookup.TryGetValue(workId, out work))
			{
				work = new UserWorkStats() { WorkId = workId, StartDate = startDate };
				workLookup.Add(work.WorkId, work);
			}
			Debug.Assert(work.WorkId == workId);

			newData = false;
			if (CurrentWork == null)
			{
				newData = true;
				CurrentWorkEndDate = endDate;
				CurrentWorkEndDateLastUpdate = DateTime.UtcNow;
				UserClockDiff = CurrentWorkEndDateLastUpdate - CurrentWorkEndDate;
				CurrentWork = work;
			}
			else if (CurrentWorkEndDate < endDate)
			{
				newData = true;
				CurrentWorkEndDate = endDate;
				CurrentWorkEndDateLastUpdate = DateTime.UtcNow;
				UserClockDiff = CurrentWorkEndDateLastUpdate - CurrentWorkEndDate;
				if (CurrentWork.WorkId != work.WorkId)
				{
					RecentWorks.Insert(0, CurrentWork);
					CurrentWork = work;
					RecentWorks.Remove(CurrentWork);
				}
			}
			else if (work != CurrentWork && !RecentWorks.Contains(work))
			{
				RecentWorks.Insert(0, work);
			}

			return work;
		}

		private UserStatus GetStatus()
		{
			//todo notworking signout handling?
			if (CurrentWorkEndDateLastUpdate < DateTime.UtcNow - DailyStatsBuilder.TimedOutAfter)
			{
				return UserStatus.Offline;
			}
			if (LastRealActivity < CurrentWorkEndDate - DailyStatsBuilder.IdleAfter)
			{
				return UserStatus.OnlineIdle;
			}
			return UserStatus.Online;
		}

		public WorkTimeStats GetWorkTimeStats()
		{
			return WorkTimeStatsBuilder.GetWorkTime();
		}

		public UserStats GetFilteredCopy(StatsFilter filter)
		{
			var result = new UserStats()
							{
								CompanyId = this.CompanyId,
								GroupId = this.GroupId,
								LastKeyboardActivity = this.LastKeyboardActivity,
								LastMouseActivity = this.LastMouseActivity,
								LastUpdateDate = this.LastUpdateDate,
								UserId = this.UserId,
							};
			//calculated fields
			float keyboardActivity;
			float mouseActivity;
			GetActivityAverages(out keyboardActivity, out mouseActivity);
			result.AverageKeyboardActivity = keyboardActivity;
			result.AverageMouseActivity = mouseActivity;
			result.Status = GetStatus();
			result.WorkTimeStats = GetWorkTimeStats();

			if (this.LastScreenShots != null && this.LastScreenShots.Count != 0)
			{
				result.LastScreenShots = new List<ScreenShot>();
				foreach (var screenShot in LastScreenShots)
				{
					result.LastScreenShots.Add(new ScreenShot()
												{
													Data = screenShot.Data,
													CreateDate = screenShot.CreateDate,
													ScreenNumber = screenShot.ScreenNumber,
													Extension = screenShot.Extension,
												});
				}
			}
			if (this.LastActiveWindow != null)
			{
				result.LastActiveWindow = new ActiveWindow()
											{
												ProcessName = this.LastActiveWindow.ProcessName,
												Title = this.LastActiveWindow.Title,
												CreateDate = this.LastActiveWindow.CreateDate,
											};
			}
			Debug.Assert(CurrentWork != null);
			if (CurrentWork.SatisfiesFilter(filter))
			{
				result.CurrentWork = this.CurrentWork.GetFilteredCopy(filter);
			}
			foreach (var recentWork in RecentWorks)
			{
				if (recentWork.SatisfiesFilter(filter))
				{
					result.RecentWorks.Add(recentWork.GetFilteredCopy(filter));
				}
			}
			return result;
		}

		public bool SatisfiesFilter(StatsFilter filter)
		{
			if (filter.GroupId.HasValue)
			{
				if (this.GroupId != filter.GroupId.Value) return false;
			}
			if (filter.CompanyId.HasValue)
			{
				if (this.CompanyId != filter.CompanyId.Value) return false;
			}
			if (filter.UserIds != null && filter.UserIds.Count != 0)
			{
				return (filter.UserIds.Contains(this.UserId));
			}
			return true;
		}

		//todo UserClockDiff is quite lame...?
		private void AddActivityHistory(WorkItem item)
		{
			var cutOff = DateTime.UtcNow - UserClockDiff - DailyStatsBuilder.ActivityAverageInterval;
			ActivityHistory.RemoveAll(n => n.StartDate < cutOff);
			if (item.StartDate < cutOff) return;
			ActivityHistory.Add(new ActivityStats()
									{
										StartDate = item.StartDate,
										Duration = item.EndDate - item.StartDate,
										KeyboardActivity = Math.Max(item.KeyboardActivity, 0),
										MouseActivity = Math.Max(item.MouseActivity, 0),
									});
		}

		private void GetActivityAverages(out float keyboardActivity, out float mouseActivity)
		{
			var cutOff = DateTime.UtcNow - UserClockDiff - DailyStatsBuilder.ActivityAverageInterval;
			ActivityHistory.RemoveAll(n => n.StartDate < cutOff);
			int mouseAct = 0;
			int keyboardAct = 0;
			foreach (var activity in ActivityHistory)
			{
				mouseAct += activity.MouseActivity;
				keyboardAct += activity.KeyboardActivity;
			}
			TimeSpan duration = DailyStatsBuilder.ActivityAverageInterval;
			if (duration.TotalMinutes != 0)
			{
				keyboardActivity = (float)(keyboardAct / duration.TotalMinutes);
				mouseActivity = (float)(mouseAct / duration.TotalMinutes);
			}
			else
			{
				keyboardActivity = 0;
				mouseActivity = 0;
			}
		}

		private class ActivityStats
		{
			public DateTime StartDate { get; set; }
			public TimeSpan Duration { get; set; }
			public int MouseActivity { get; set; }
			public int KeyboardActivity { get; set; }
		}
	}
}