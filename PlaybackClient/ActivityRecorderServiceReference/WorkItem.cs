using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace PlaybackClient.ActivityRecorderServiceReference
{
	partial class WorkItem
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void TryLoadScreenShots()
		{
			if (DesktopCaptures == null) return;
			foreach (var desktopCapture in DesktopCaptures)
			{
				if (desktopCapture.Screens == null) continue;
				foreach (var screen in desktopCapture.Screens)
				{
					if (screen.ScreenShot != null || screen.ScreenShotPath == null) continue;
					try
					{
						if (!screen.ScreenShotOffset.HasValue || !screen.ScreenShotLength.HasValue)
						{
							screen.ScreenShot = File.ReadAllBytes(screen.ScreenShotPath);
							return;
						}
						using (var stream = new FileStream(screen.ScreenShotPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
						{
							stream.Position = screen.ScreenShotOffset.Value;
							var buffer = new byte[screen.ScreenShotLength.Value];
							stream.Read(buffer, 0, screen.ScreenShotLength.Value);
							screen.ScreenShot = buffer;
						}
					}
					catch (Exception ex)
					{
						log.Error("Cannot load screenshot for " + this, ex);
					}
				}
			}
		}

		public override string ToString()
		{
			return "workItem userId: " + UserId + " workId: " + WorkId + " start: " + StartDate + " end: " + EndDate;
		}
	}
}
