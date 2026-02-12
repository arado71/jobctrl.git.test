using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Menu
{
	public static class RecentClosedHelper
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static List<WorkDataWithParentNames> recents = null;

		public static event EventHandler Changed;

		private static string RecentWorksPath
		{
			get { return "RecentClosedWorks-" + ConfigManager.UserId; }
		}

		public static bool AddRecent(WorkDataWithParentNames work)
		{
			LoadRecents();
			var existing = recents.FirstOrDefault(x => x.WorkData.Id == work.WorkData.Id);
			if (existing != null)
			{
				recents.Remove(existing);
			}

			recents.Insert(0, new WorkDataWithParentNames(){ ParentNames  = work.ParentNames, WorkData = new WorkData() { Id = work.WorkData.Id, Name = work.WorkData.Name }});
			if (recents.Count > ConfigManager.LocalSettingsForUser.MenuRecentItemsCount)
			{
				recents.RemoveAt(recents.Count - 1);
			}

			SaveRecents();
			var evt = Changed;
			if (evt != null) evt(null, EventArgs.Empty);
			return true;
		}

		public static bool RemoveRecent(WorkDataWithParentNames work)
		{
			LoadRecents();
			var existing = recents.FirstOrDefault(x => x.WorkData.Id == work.WorkData.Id);
			if (existing != null)
			{
				recents.Remove(existing);
			}

			SaveRecents();
			var evt = Changed;
			if (evt != null) evt(null, EventArgs.Empty);
			return true;
		}

		public static void Clear()
		{
			recents = new List<WorkDataWithParentNames>();
			SaveRecents();
			var evt = Changed;
			if (evt != null) evt(null, EventArgs.Empty);
		}

		public static IList<WorkDataWithParentNames> GetRecents()
		{
			LoadRecents();
			return recents;
		}

		private static void LoadRecents()
		{
			if (recents == null)
			{
				if (IsolatedStorageSerializationHelper.Exists(RecentWorksPath))
				{
					IsolatedStorageSerializationHelper.Load(RecentWorksPath, out recents);
				}
				if (recents == null) recents = new List<WorkDataWithParentNames>();
			}
		}

		private static void SaveRecents()
		{
			if (recents != null)
			{
				IsolatedStorageSerializationHelper.Save(RecentWorksPath, recents);
			}
		}
	}
}