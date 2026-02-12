using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService
{
	public class VersionCacheManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ConcurrentDictionary<int, Binary> dictionary = new ConcurrentDictionary<int, Binary>(); //lazy could further reduce db access
		private Binary lastVersion;	//Initial null value means -1L to include version 0 (Versions beyond 0x7FFFFFFFFFFFFFFF won't be loaded at initial run)

		private readonly Func<ClientSetting, Binary> versionSelector;
		private readonly Func<CachedVersionElement, Binary> cveVersionSelector;
		private readonly string versionSelectSql;

		private VersionCacheManager(Func<ClientSetting, Binary> versionSelector, Func<CachedVersionElement, Binary> cveVersionSelector, string versionSelectSql)
			: base(log)
		{
			this.versionSelector = versionSelector;
			this.versionSelectSql = versionSelectSql;
			this.cveVersionSelector = cveVersionSelector;
			ManagerCallbackInterval = ConfigManager.VersionCacheInterval;
		}

		public static VersionCacheManager GetMenuVersionCacheManager()
		{
			return new VersionCacheManager(n => n.MenuVersion, n => n.MenuVersion, "select UserId, MenuVersion from dbo.ClientSettings where MenuVersion > {0}");
		}

		public static VersionCacheManager GetWorkDetectorRulesVersionCacheManager()
		{
			return new VersionCacheManager(n => n.WorkDetectorRulesVersion, n => n.WorkDetectorRulesVersion, "select UserId, WorkDetectorRulesVersion from dbo.ClientSettings where WorkDetectorRulesVersion > {0}");
		}

		public static VersionCacheManager GetCensorRulesVersionCacheManager()
		{
			return new VersionCacheManager(n => n.CensorRulesVersion, n => n.CensorRulesVersion, "select UserId, CensorRulesVersion from dbo.ClientSettings where CensorRulesVersion > {0}");
		}

		public static VersionCacheManager GetClientSettingsVersionCacheManager()
		{
			return new VersionCacheManager(n => n.ClientSettingsVersion, n => n.ClientSettingsVersion, "select UserId, ClientSettingsVersion from dbo.ClientSettings where ClientSettingsVersion > {0}");
		}

		public static VersionCacheManager GetCollectorRulesVersionCacheManager()
		{
			return new VersionCacheManager(n => n.CollectorRulesVersion, n => n.CollectorRulesVersion, "select UserId, CollectorRulesVersion from dbo.ClientSettings where CollectorRulesVersion > {0}");
		}

		public Binary Get(int userId, ActivityRecorderDataClassesDataContext contex)
		{
			return dictionary.GetOrAdd(userId, uId => contex.ClientSettings.Where(n => n.UserId == uId).Select(versionSelector).SingleOrDefault());
		}

		public void Remove(int userId)
		{
			Binary _;
			dictionary.TryRemove(userId, out _);
		}

		protected override void ManagerCallbackImpl()
		{
			List<CachedVersionElement> updates;
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				updates = context.ExecuteQuery<CachedVersionElement>(versionSelectSql, lastVersion ?? ((object)-1L)).ToList();
			}
			foreach (var update in updates)
			{
				var dbVer = cveVersionSelector(update);
				dictionary.AddOrUpdate(update.UserId, dbVer, (uId, dicVer) => (dicVer == null || dbVer.ToLong() > dicVer.ToLong()) ? dbVer : dicVer);
				if (dbVer.ToLong() > (lastVersion == null ? -1L : lastVersion.ToLong())) lastVersion = dbVer;
			}
		}

#pragma warning disable 0649
		// ReSharper disable ClassNeverInstantiated.Local
		protected class CachedVersionElement
		{
			public int UserId;
			public Binary MenuVersion;
			public Binary WorkDetectorRulesVersion;
			public Binary CensorRulesVersion;
			public Binary ClientSettingsVersion;
			public Binary CollectorRulesVersion;
		}
		// ReSharper restore ClassNeverInstantiated.Local
#pragma warning restore 0649
	}
}
