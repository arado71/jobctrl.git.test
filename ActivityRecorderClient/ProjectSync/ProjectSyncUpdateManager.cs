using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.ProjectSync
{
	public class ProjectSyncUpdateManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int checkingInterval = (int)TimeSpan.FromHours(1).TotalMilliseconds;
		private static string lastrunFile { get { return "ProjectSyncUpdateStatus-" + ConfigManager.UserId; } }

		private ProjectSyncService service;

		public ProjectSyncUpdateManager(ProjectSyncService service) : base(log)
		{
			this.service = service;
		}

		protected override void ManagerCallbackImpl()
		{
			if (string.IsNullOrEmpty(ConfigManager.MsProjectAddress)) return;
			try
			{
				DateTime lastRun;
				if (!IsolatedStorageSerializationHelper.Exists(lastrunFile)
					|| !IsolatedStorageSerializationHelper.Load(lastrunFile, out lastRun))
				{
					lastRun = DateTime.Now.AddDays(-1);
				}
				// logic
				var needRun = lastRun.Date != DateTime.Now.Date;
				if (!needRun) return;
				var isNewWeek = lastRun != DateTime.MinValue &&
								CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(lastRun, CalendarWeekRule.FirstDay,
									DayOfWeek.Monday)
								!=
								CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay,
									DayOfWeek.Monday);
				
				service.AutoSync(lastRun.Date, isNewWeek);

				lastRun = DateTime.Now; // local time!
				IsolatedStorageSerializationHelper.Save(lastrunFile, lastRun);

			}
			catch (Exception ex)
			{
				log.Error("scheduling failed", ex);
			}
		}

		protected override int ManagerCallbackInterval
		{
			get { return checkingInterval; }
		}
	}
}
