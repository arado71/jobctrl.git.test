using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService.Caching.Works
{
	/// <summary>
	/// Thread-safe class for retriving Works and Projects.
	/// </summary>
	public class WorkHierarchyLite : PeriodicManager, IWorkHierarchyService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly string[] emptyProjects = new string[0];

		private readonly object thisLock = new object();
		private readonly LruCache<int, WorkOrProject> workOrProjectCache = new LruCache<int, WorkOrProject>(ConfigManager.CacheSizeWorkOrProjects);
		private readonly LruCache<ulong, string> cachedNames = new LruCache<ulong, string>(ConfigManager.CacheSizeWorkName);
		private HashSet<int> pendingIds = new HashSet<int>();
		private bool isDbBusy;

		public WorkHierarchyLite()
		{
			ManagerCallbackInterval = ConfigManager.WorkNameRefreshInterval;
		}

		protected override void ManagerCallbackImpl()
		{
			var sw = Stopwatch.StartNew();
			var ids = workOrProjectCache.Keys; //todo persist data so on restart we have some ids?
			var dataFromDb = GetWorkOrProjectWithParentsFromDb(ids);
			workOrProjectCache.Clear();
			foreach (var workOrProject in dataFromDb)
			{
				workOrProjectCache.TryAdd(workOrProject.Id, workOrProject);
			}
			cachedNames.Clear();
			log.Info("Refreshed work hierarchy cache key count " + ids.Count.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
		}

		public bool TryGetWork(int workId, out Work work)
		{
			work = GetWorkOrProject(workId).ToWork();
			return work != null;
		}

		public bool TryGetProject(int projectId, out Project project)
		{
			project = GetWorkOrProject(projectId).ToProject();
			return project != null;
		}

		public string GetWorkNameWithProjects(int workId, int targetLength = int.MaxValue)
		{
			var key = ((ulong)workId << 32) | (ulong)targetLength;
			string res;
			if (!cachedNames.TryGetValue(key, out res))
			{
				res = GetWorkNameWithProjectsImpl(workId, targetLength);
				cachedNames.TryAdd(key, res);
			}
			return res;
		}

		public string GetWorkNameWithProjectsImpl(int workId, int targetLength = int.MaxValue)
		{
			var work = GetWorkOrProject(workId);
			if (work == null)
			{
				log.ErrorAndFail("Unable to find work with id " + workId);
				return EmailStats.EmailStats.UnknownWork;
			}
			var workName = work.Name;
			var projectNames = new List<string>();
			var parentId = work.ParentId;
			while (parentId != null)
			{
				var project = GetWorkOrProject(parentId.Value);
				projectNames.Add(project.Name);
				parentId = project.ParentId;
			}
			if (projectNames.Count > 0)
			{
				projectNames.RemoveAt(projectNames.Count - 1); //root is the company's name
				projectNames.Reverse();
			}
			return WorkHierarchyBase.GetWorkAndProjectNamesWithEllipse(workName, projectNames.ToArray(), targetLength);
		}

		public string GetWorkName(int workId, int targetLength = int.MaxValue)
		{
			return WorkHierarchyBase.GetWorkAndProjectNamesWithEllipse(GetWorkOrProject(workId).Name, emptyProjects, targetLength);
		}

		private WorkOrProject GetWorkOrProject(int id)
		{
			WorkOrProject res;
			if (!workOrProjectCache.TryGetValue(id, out res))
			{
				res = GetWorkOrProjectFromDbCoordinated(id);
			}
			return res;
		}

		//only one thread can access the db, while that thread is working others just add their requets to a queue and wait.
		//If the thread accessing the db have finished, it signals the other waiters to process their accumulated data.
		private WorkOrProject GetWorkOrProjectFromDbCoordinated(int id)
		{
			WorkOrProject res = null;
			var isIdAdded = false;
			HashSet<int> current = null;
			while (true)
			{
				try
				{
					//Check for db access
					current = null; //if current is not null we can access the db
					lock (thisLock)
					{
						if (!isIdAdded) pendingIds.Add(id);
						isIdAdded = true;
						if (!isDbBusy)
						{
							isDbBusy = true;
							pendingIds.Add(id); //make sure our id is added to the current set when going to the db (possibly again) so we won't have infinite loops when id is invalid
							current = pendingIds;
							pendingIds = new HashSet<int>(); //create a new queue for pending requests
						}
						else //db is busy
						{
							Debug.Assert(current == null);
							Monitor.Wait(thisLock); //after waiting: either our id is processed or we try to get db access again
						}
					}

					//Access the db
					if (current != null) //we can access the db
					{
						Debug.Assert(current.Contains(id));
						var dataFromDb = GetWorkOrProjectWithParentsFromDb(current);
						foreach (var workOrProject in dataFromDb)
						{
							if (workOrProject.Id == id) res = workOrProject;
							workOrProjectCache.TryAdd(workOrProject.Id, workOrProject);
						}
						return res; //make sure we terminate even for invalid ids (res will be null then)
					}

					//Check if we got the result when we have no db access
					if (workOrProjectCache.TryGetValue(id, out res)) return res;
				}
				catch (Exception ex)
				{
					if (current != null)
					{
						log.Error("Failed to get workIds from db", ex);
						lock (thisLock)
						{
							pendingIds.UnionWith(current);
						}
					}
				}
				finally
				{
					if (current != null)
					{
						lock (thisLock)
						{
							isDbBusy = false;
							Monitor.PulseAll(thisLock);
						}
					}
				}
			}
		}

		private static List<WorkOrProject> GetWorkOrProjectWithParentsFromDb(IEnumerable<int> ids)
		{
			return StatsDbHelper.GetWorksOrProjects(ids);
		}
	}
}
