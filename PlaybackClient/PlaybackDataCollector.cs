using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using log4net;

namespace PlaybackClient
{
	/// <summary>
	/// Thread-safe class for querying PlaybackData from the Db.
	/// </summary>
	public class PlaybackDataCollector : IPlaybackDataCollector, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Tct.ActivityRecorderService.Storage.StorageManager storageManager = new Tct.ActivityRecorderService.Storage.StorageManager();

		public PlaybackDataCollector()
		{
			storageManager.Start();
		}

		public PlaybackData GetDataFor(int userId, DateTime startDate, DateTime endDate)
		{
			var workItems = DbHelper.GetWorkItemsWithAllDataForUser(userId, startDate, endDate);
			SetScreenShotPaths(workItems);
			var manual = DbHelper.GetManualWorkItemsForUser(userId, startDate, endDate);
			var mobileWorkItems = DbHelper.GetMobileWorkItemsForUser(userId, startDate, endDate);
			var mobileLocations = DbHelper.GetMobileLocationsForUser(userId, startDate, endDate);
			return new PlaybackData()
			{
				UserId = userId,
				StartDate = startDate,
				EndDate = endDate,
				WorkItems = workItems.Select(n => DbDataConverter.FromWorkItem(n)).ToList(),
				ManualWorkItems = manual.Select(n => DbDataConverter.FromManualWorkItem(n)).ToList(),
				MobileWorkItems = mobileWorkItems,
				MobileLocations = mobileLocations,
			};
		}

		public PlaybackData GetDataForTest(int userId, DateTime startDate, DateTime endDate)
		{
			var workItems = DbHelper.GetWorkItemsWithAllDataForUser(userId, startDate, endDate);
			SetScreenShotPaths(workItems);
			var manual = DbHelper.GetManualWorkItemsForUser(userId, startDate, endDate);
			return new PlaybackData()
			{
				UserId = userId,
				StartDate = startDate,
				EndDate = endDate,
				WorkItems = workItems.Select(n => DbDataConverter.FromWorkItem(n)).ToList(),
				ManualWorkItems = manual.Select(n => DbDataConverter.FromManualWorkItem(n)).ToList(),
			};
		}

		private void SetScreenShotPaths(IEnumerable<WorkItem> workItems)
		{
			if (workItems == null) return;
			foreach (var workItem in workItems)
			{
					if (workItem.ScreenShots == null) continue;
					foreach (var screenShot in workItem.ScreenShots)
					{
						if (screenShot.Extension == null || screenShot.Extension == "*C*") continue; //no or censored screenshot
						try
						{
							var path = storageManager.GetPath(screenShot, out var offset, out var length, false);
							//log.Debug("scrpath: " + path);
							screenShot.ScreenShotPath = path;
							screenShot.ScreenShotOffset = offset >= 0 ? offset : (long?)null;
							screenShot.ScreenShotLength = offset >= 0 ? length : (int?) null;
						}
						catch (Exception ex)
						{
							log.Error("Unable to get screenshot path", ex);
						}
					}
			}
		}

		public void Dispose()
		{
			storageManager.Stop();
		}
	}
}
