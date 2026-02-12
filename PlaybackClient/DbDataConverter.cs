using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using PlaybackClient.ActivityRecorderServiceReference;

namespace PlaybackClient
{
	/// <summary>
	/// Thread-safe class for converting Db data to WCF data
	/// </summary>
	public static class DbDataConverter
	{
		public static ManualWorkItem FromManualWorkItem(Tct.ActivityRecorderService.ManualWorkItem dbItem)
		{
			if (dbItem == null) return null;
			return new ManualWorkItem()
			{
				UserId = dbItem.UserId,
				WorkId = dbItem.WorkId,
				StartDate = dbItem.StartDate,
				EndDate = dbItem.EndDate,
				ManualWorkItemTypeId = (ManualWorkItemTypeEnum)dbItem.ManualWorkItemTypeId,
			};
		}

		public static WorkItem FromWorkItem(Tct.ActivityRecorderService.WorkItem dbItem)
		{
			if (dbItem == null) return null;
			return new WorkItem()
			{
				ComputerId = dbItem.ComputerId,
				UserId = dbItem.UserId,
				WorkId = dbItem.WorkId,
				StartDate = dbItem.StartDate,
				EndDate = dbItem.EndDate,
				IsRemoteDesktop = dbItem.IsRemoteDesktop,
				IsVirtualMachine = dbItem.IsVirtualMachine,
				KeyboardActivity = dbItem.KeyboardActivity,
				MouseActivity = dbItem.MouseActivity,
				PhaseId = dbItem.PhaseId,
				DesktopCaptures = FromScreenShots(dbItem.ScreenShots),
			};
		}

		private static List<DesktopCapture> FromScreenShots(EntitySet<Tct.ActivityRecorderService.ScreenShot> screenShots)
		{
			if (screenShots == null) return null;
			return screenShots.ToLookup(s => s.CreateDate).Select(l => DesktopCaptureFromScreenShot(l.ToList())).ToList();
		}

		private static DesktopCapture DesktopCaptureFromScreenShot(List<Tct.ActivityRecorderService.ScreenShot> screenShots)
		{
			if (screenShots == null) return null;
			return new DesktopCapture()
			{
				Screens = ScreensFromScreenShots(screenShots),
				DesktopWindows = new List<DesktopWindow>(),
			};
		}

		private static List<Screen> ScreensFromScreenShots(List<Tct.ActivityRecorderService.ScreenShot> screenShots)
		{
			if (screenShots == null) return null;
			return screenShots.Select(s => FromScreenShot(s)).ToList();
		}

		private static Screen FromScreenShot(Tct.ActivityRecorderService.ScreenShot dbItem)
		{
			if (dbItem == null) return null;
			return new Screen()
			{
				Height = dbItem.Height,
				Width = dbItem.Width,
				X = dbItem.X,
				Y = dbItem.Y,
				ScreenNumber = dbItem.ScreenNumber,
				Extension = dbItem.Extension,
				CreateDate = dbItem.CreateDate,
				ScreenShotPath = ConfigManager.SendScreenshots ? dbItem.ScreenShotPath : null, //don't load yet
				ScreenShotOffset = dbItem.ScreenShotOffset,
				ScreenShotLength = dbItem.ScreenShotLength,
			};
		}

	}
}
