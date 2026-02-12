using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using log4net;

namespace Tct.ActivityRecorderService.Maintenance
{
	public class FileCleanupManager : PeriodicManager
	{
	    private const int recordsToTake = 1000000;
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly FileCleanup fileCleanup;

		public FileCleanupManager(params IFileCleanup[] storages)
		{
            FileCleanup.OnAfterFileCleanup += FileCleanup_OnAfterFileCleanup;
			fileCleanup = new FileCleanup(storages);
			ManagerCallbackInterval = 6 * 60 * 60 * 1000; // Every 6 hours
		}

        void FileCleanup_OnAfterFileCleanup(object sender, FileCleanup.FileCleanupEventArgs e)
        {
        }

		protected override void ManagerCallbackImpl()
		{
			log.Debug("Loading configuration");
			ConfigurationManager.RefreshSection("fileCleanup");
			var configSettings = (FileCleanupSection)ConfigurationManager.GetSection("fileCleanup"); 
			if (configSettings == null || configSettings.Limits.Count == 0)
			{
				log.Debug("Cleanup section not found in configuration");
				if (configSettings == null)
					configSettings = new FileCleanupSection() { Limits = new LimitElementCollection() };
			}
			var limits = new LimitElementCollection();
			configSettings.Limits.Cast<ConfigurationElement>().ToList().ForEach(e => limits.Add((LimitElement)e));
			configSettings = new FileCleanupSection { Limits = limits }; // create a new writeable configuration (!)
			using (var context = new JobControlDataClassesDataContext())
			{
				foreach (var userLimitElement in context.GetUserStatsInfo()
															.Where(u => u.ScreenshotStorageLimitInDays > 0)
															.Select(u => new LimitElement { UserId = u.Id, Storage = Storage.Screenshot, MaxAge = $"{u.ScreenshotStorageLimitInDays}d" })
															.ToList())
				{
					configSettings.Limits.Add(userLimitElement);
				}
			}

			fileCleanup.Cleanup(configSettings);
		}

		
	}
}
