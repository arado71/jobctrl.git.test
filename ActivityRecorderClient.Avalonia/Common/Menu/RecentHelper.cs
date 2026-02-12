using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Menu
{
	public static class RecentHelper
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static List<WorkData> recents = null;
		private static WorkDataWithParentNames[] recentCache = null;
		private static long? recentVersion = null;

		public static event EventHandler RecentChanged;

		private static string RecentWorksPath
		{
			get { return "RecentWorks-" + ConfigManager.UserId; }
		}

		public static bool AddRecent(WorkData work)
		{
			LoadRecents();
			InvalidateCache();
			var existing = recents.FirstOrDefault(x => x.Id == work.Id);
			if (existing != null)
			{
				recents.Remove(existing);
			}

			recents.Insert(0, new WorkData { Id = work.Id, Name = work.Name });
			if (recents.Count > ConfigManager.LocalSettingsForUser.MenuRecentItemsCount)
			{
				recents.RemoveAt(recents.Count - 1);
			}

			SaveRecents();
			var evt = RecentChanged;
			if (evt != null) evt(null, EventArgs.Empty);
			return true;
		}

		public static void Clear()
		{
			recents = new List<WorkData>();
			InvalidateCache();
			SaveRecents();
			var evt = RecentChanged;
			if (evt != null) evt(null, EventArgs.Empty);
		}

		public static IEnumerable<int> GetRecentIds()
		{
			LoadRecents();
			return recents.Where(x => x.Id.HasValue).Select(x => x.Id.Value);
		}

		public static WorkDataWithParentNames[] GetRecents()
		{
			if (IsCacheValid())
			{
				return recentCache;
			}

			var lookup = MenuQuery.Instance.ClientMenuLookup;
			recentVersion = lookup.Version;
			return recentCache = FetchRecents(lookup.Value).ToArray();
		}

		private static void Cleanup()
		{
			ClientMenuLookup lookup = MenuQuery.Instance.ClientMenuLookup.Value;
			recents =
				new List<WorkData>(recents.Where(x => lookup.WorkDataById.ContainsKey(x.Id.Value)));
			InvalidateCache();
			SaveRecents();
			var evt = RecentChanged;
			if (evt != null) evt(null, EventArgs.Empty);
		}

		private static IEnumerable<WorkDataWithParentNames> FetchRecents(ClientMenuLookup lookup)
		{
			LoadRecents();
			if (lookup == null)
			{
				yield break;
			}

			bool needCleanup = false;
			foreach (var recent in recents)
			{
				if (lookup.WorkDataById.ContainsKey(recent.Id.Value))
				{
					yield return lookup.WorkDataById[recent.Id.Value];
					continue;
				}

				needCleanup = true;
				log.InfoFormat("Recent work {1} ({0}) not found", recent.Id, recent.Name);
			}

			if (needCleanup) Cleanup();
		}

		private static void InvalidateCache()
		{
			recentVersion = null;
		}

		private static bool IsCacheValid()
		{
			return recentVersion != null && recentVersion.Value == MenuQuery.Instance.ClientMenuLookup.Version;
		}

		private static void LoadRecents()
		{
			if (recents == null)
			{
				if (IsolatedStorageSerializationHelper.Exists(RecentWorksPath))
				{
					IsolatedStorageSerializationHelper.Load(RecentWorksPath, out recents);
				}
				if (recents == null) recents = new List<WorkData>();
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