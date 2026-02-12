using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.Caching.Works
{
	/// <summary>
	/// Thread-safe class for getting cached work names that are periodically refreshed and read from DB is missing.
	/// </summary>
	/// <remarks>
	/// It is not ideal that we cache all works (WorkHierarchyCacheDict) probably we want to use Lru (WorkHierarchyCacheLru) later? -> use WorkHierarchyLite
	/// </remarks>
	public class WorkHierarchyManagerDict : PeriodicManager, IWorkHierarchyService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly LruCache<string, string> cachedNames = new LruCache<string, string>(ConfigManager.CacheSizeWorkName);
		private volatile WorkHierarchyBase currentWorkHierarchy = new WorkHierarchyCacheDict(Enumerable.Empty<Work>(), Enumerable.Empty<Project>(), GetWorkById, GetProjectById);

		public WorkHierarchyManagerDict()
			: base(log)
		{
			ManagerCallbackInterval = ConfigManager.WorkNameRefreshInterval;
		}

		public string GetWorkNameWithProjects(int workId, int targetLength = int.MaxValue)
		{
			var key = "P" + workId + "-" + targetLength;
			string res;
			if (!cachedNames.TryGetValue(key, out res))
			{
				res = currentWorkHierarchy.GetWorkNameWithProjects(workId, targetLength);
				cachedNames.TryAdd(key, res);
			}
			return res;
		}

		public string GetWorkName(int workId, int targetLength = int.MaxValue)
		{
			if (targetLength == int.MaxValue) //don't cache ordinary work names to save some memory and it should be quite fast
			{
				return currentWorkHierarchy.GetWorkName(workId, targetLength);
			}
			var key = "W" + workId + "-" + targetLength;
			string res;
			if (!cachedNames.TryGetValue(key, out res))
			{
				res = currentWorkHierarchy.GetWorkName(workId, targetLength);
				cachedNames.TryAdd(key, res);
			}
			return res;
		}

		public bool TryGetWork(int workId, out Work work)
		{
			Work orig;
			if (currentWorkHierarchy.TryGetWork(workId, out orig))
			{
				work = new Work() { Id = workId, Name = orig.Name, ProjectId = orig.ProjectId };
				return true;
			}
			work = null;
			return false;
		}

		public bool TryGetProject(int projectId, out Project project)
		{
			Project orig;
			if (currentWorkHierarchy.TryGetProject(projectId, out orig))
			{
				project = new Project() { Id = orig.Id, Name = orig.Name, ParentId = orig.ParentId };
				return true;
			}
			project = null;
			return false;
		}

		public void Refresh()
		{
			ExecuteManagerCallbackImpl();
		}

		private static WorkHierarchyBase GetNewWorkHierarchy()
		{
			var workIds = StatsDbHelper.GetWorksById();
			var projectIds = StatsDbHelper.GetProjectsById();
			return new WorkHierarchyCacheDict(workIds, projectIds, GetWorkById, GetProjectById);
		}

		private static Work GetWorkById(int workId)
		{
			try
			{
				return StatsDbHelper.GetWorkById(workId);
			}
			catch (Exception ex)
			{
				log.Error("Unable to get work for id " + workId, ex);
				return null;
			}
		}

		private static Project GetProjectById(int projectId)
		{
			try
			{
				return StatsDbHelper.GetProjectById(projectId);
			}
			catch (Exception ex)
			{
				log.Error("Unable to get project for id " + projectId, ex);
				return null;
			}
		}

		protected override void ManagerCallbackImpl()
		{
			var sw = Stopwatch.StartNew();
			try
			{
				currentWorkHierarchy = GetNewWorkHierarchy();
				cachedNames.Clear();
			}
			catch (Exception ex)
			{
				log.Info("Unable to refresh work hierarchy", ex);
			}
			finally
			{
				log.Info("Refreshed work hierarchy in " + sw.Elapsed.ToTotalMillisecondsString() + "ms ");
			}
		}
	}
}
