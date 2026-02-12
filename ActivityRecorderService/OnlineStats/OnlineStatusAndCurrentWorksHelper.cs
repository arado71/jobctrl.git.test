using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.MobileServiceReference;

namespace Tct.ActivityRecorderService.OnlineStats
{
	public static class OnlineStatusAndCurrentWorksHelper
	{
		public static void UpdateDetailedUserStatsFromIntervals(DetailedUserStats statsToUpdate, ComputerStatusCalculator compStatusCalculator, MobileStatusManager mobileStatusManager, UserStatInfo userStatInfo)
		{
			statsToUpdate.CurrentWorks = new List<WorkWithType>();
			statsToUpdate.Status = OnlineStatus.Offline;
			var smallOnlineThreshold = new StartEndDateTime(DateTime.UtcNow.AddMinutes(-0.1), DateTime.UtcNow.AddMinutes(0.1));
			var bigOnlineThreshold = new StartEndDateTime(DateTime.UtcNow.AddMinutes(-2), DateTime.UtcNow.AddMinutes(1));
			var ivrOnlineThreshold = new StartEndDateTime(DateTime.UtcNow.AddMinutes(-1.5), DateTime.UtcNow.AddMinutes(0.2)); //ivr info is updated only in every minute atm.
			var localDate = CalculatorHelper.GetLocalReportDate(DateTime.UtcNow, userStatInfo.TimeZone, userStatInfo.StartOfDayOffset);
			var wholeDayOnlineThreshold = CalculatorHelper.GetUtcStartEndTimeForReportFromLocalDate(ReportType.Daily, localDate, userStatInfo);

			foreach (var compStats in statsToUpdate.ComputerStatsByCompId.Values) //todo fix if a new computer sent StartCompWork but didn't sent any workitems yet then we won't see it online
			{
				var workId = compStatusCalculator.GetCurrentWorkId(compStats.ComputerId);
				if (workId != null)
				{
					compStats.IsOnline = true;
					statsToUpdate.Status |= OnlineStatus.OnlineComputer;
					statsToUpdate.CurrentWorks.Add(new WorkWithType() { Type = WorkType.Computer, WorkId = workId.Value });
				}
			}

			bool isUpToDate;
			var mobileWorks = mobileStatusManager.GetMobileWorksForUser(userStatInfo.Id, out isUpToDate);
			foreach (var mobileWork in mobileWorks)
			{
				DetailedMobileStats mobileStats;
				if (!statsToUpdate.MobileStatsByMobileId.TryGetValue(mobileWork.Imei, out mobileStats))
				{
					mobileStats = new DetailedMobileStats() { MobileId = mobileWork.Imei, TodaysWorkIntervalsByWorkId = new Dictionary<int, WorkWithIntervals>(), };
					statsToUpdate.MobileStatsByMobileId.Add(mobileWork.Imei, mobileStats);
				}
				mobileStats.IsOnline = isUpToDate && mobileWork.WorkId.HasValue;
				if (mobileWork.BatteryPercent != null || mobileWork.ConnectionType != null)
				{
					if (mobileStats.RecentMobileActivity == null) mobileStats.RecentMobileActivity = new MobileActivity();
					mobileStats.RecentMobileActivity.BatteryPercentage = mobileWork.BatteryPercent;
					mobileStats.RecentMobileActivity.ConnectionType = mobileWork.ConnectionType;
					mobileStats.RecentMobileActivity.LastCameraShotPath = mobileWork.LastCameraShotPath;
					statsToUpdate.BatteryPercent = mobileWork.BatteryPercent; //last wins
					statsToUpdate.ConnectionType = mobileWork.ConnectionType; //last wins
				}
				if (mobileStats.IsOnline)
				{
					statsToUpdate.Status |= mobileWork.DeviceType == DeviceType.Beacon ? OnlineStatus.OnlineBeacon : OnlineStatus.OnlineMobile;
					statsToUpdate.CurrentWorks.Add(new WorkWithType() { Type = WorkType.Mobile, WorkId = mobileWork.WorkId.Value });
				}
			}
			////status from mobile intervals only (but without clockskew data its worthless)
			//var onlineMobile = GetBestOnlineWorkId(statsToUpdate.MobileStats.TodaysWorkIntervalsByWorkId, bigOnlineThreshold);
			//if (onlineMobile != null)
			//{
			//    statsToUpdate.MobileStats.IsOnline = true;
			//    statsToUpdate.Status |= OnlineStatus.OnlineMobile;
			//    statsToUpdate.CurrentWorks.Add(new WorkWithType() { Type = WorkType.Mobile, WorkId = onlineMobile.Value });
			//}

			//do we want to indicate several works when overlapping ? (not atm.)
			var onlineManual = GetBestOnlineWorkId(statsToUpdate.ManuallyAddedStats.TodaysWorkIntervalsByWorkId, smallOnlineThreshold);
			if (onlineManual != null)
			{
				statsToUpdate.ManuallyAddedStats.IsOnline = true;
				statsToUpdate.Status |= OnlineStatus.OnlineManuallyAdded;
				statsToUpdate.CurrentWorks.Add(new WorkWithType() { Type = WorkType.ManuallyAdded, WorkId = onlineManual.Value });
			}

			var onlineHoliday = GetBestOnlineWorkId(statsToUpdate.HolidayStats.TodaysWorkIntervalsByWorkId, wholeDayOnlineThreshold);
			if (onlineHoliday != null)
			{
				statsToUpdate.HolidayStats.IsOnline = true;
				statsToUpdate.Status |= OnlineStatus.OnHoliday;
				statsToUpdate.CurrentWorks.Add(new WorkWithType() { Type = WorkType.Holiday, WorkId = onlineHoliday.Value });
			}

			var onlineSickLeave = GetBestOnlineWorkId(statsToUpdate.SickLeaveStats.TodaysWorkIntervalsByWorkId, wholeDayOnlineThreshold);
			if (onlineSickLeave != null)
			{
				statsToUpdate.SickLeaveStats.IsOnline = true;
				statsToUpdate.Status |= OnlineStatus.OnSickLeave;
				statsToUpdate.CurrentWorks.Add(new WorkWithType() { Type = WorkType.SickLeave, WorkId = onlineSickLeave.Value });
			}
		}

		private static bool IsIntersected(StartEndDateTime first, StartEndDateTime second)
		{
			var startMax = first.StartDate > second.StartDate ? first.StartDate : second.StartDate;
			var endMin = first.EndDate < second.EndDate ? first.EndDate : second.EndDate;
			return startMax < endMin;
		}

		private static int? GetBestOnlineWorkId(Dictionary<int, WorkWithIntervals> intervalsDict, StartEndDateTime onlineThreshold)
		{
			var intervalList = intervalsDict.Values.ToList();
			var maxEnd = DateTime.MinValue;
			int? result = null;
			foreach (var workWithInterval in intervalList)
			{
				var workId = workWithInterval.WorkId;
				foreach (var interval in workWithInterval.Intervals)
				{
					if (interval.EndDate > maxEnd &&
						IsIntersected(new StartEndDateTime(interval.StartDate, interval.EndDate), onlineThreshold))
					{
						maxEnd = interval.EndDate;
						result = workId;
					}
				}
			}
			return result;
		}
	}
}
