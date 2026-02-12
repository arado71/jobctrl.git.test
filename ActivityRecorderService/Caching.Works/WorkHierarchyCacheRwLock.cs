using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tct.ActivityRecorderService.Caching.Works
{
	/// <summary>
	/// Simple thread-safe WorkHierarchy cache based on Dictionary protected by ReaderWriterLockSlim.
	/// </summary>
	/// <remarks>
	/// Reparenting can cause some issues.
	/// </remarks>
	public class WorkHierarchyCacheRwLock : WorkHierarchyBase
	{
		private readonly Dictionary<int, Work> works = new Dictionary<int, Work>();
		private readonly Dictionary<int, Project> projects = new Dictionary<int, Project>();
		private readonly ReaderWriterLockSlim rwLockWorks = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		private readonly ReaderWriterLockSlim rwLockProjects = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		private readonly Func<int, Work> getWorkById;
		private readonly Func<int, Project> getProjectById;

		public WorkHierarchyCacheRwLock(IEnumerable<Work> initialWorks, IEnumerable<Project> initialProjects, Func<int, Work> getWorkById, Func<int, Project> getProjectById)
		{
			if (getWorkById == null) throw new ArgumentNullException("getWorkById");
			if (getProjectById == null) throw new ArgumentNullException("getProjectById");

			works = initialWorks.ToDictionary(n => n.Id);
			projects = initialProjects.ToDictionary(n => n.Id);

			this.getWorkById = getWorkById;
			this.getProjectById = getProjectById;
		}

		protected internal override bool TryGetWork(int workId, out Work work)
		{
			rwLockWorks.EnterReadLock();
			try
			{
				if (works.TryGetValue(workId, out work))
				{
					return true;
				}
			}
			finally
			{
				rwLockWorks.ExitReadLock();
			}

			work = getWorkById(workId);
			if (work == null) return false;

			rwLockWorks.EnterWriteLock();
			try
			{
				works[workId] = work; //race is ok here...
				return true;
			}
			finally
			{
				rwLockWorks.ExitWriteLock();
			}
		}

		protected internal override bool TryGetProject(int projectId, out Project project)
		{
			rwLockProjects.EnterReadLock();
			try
			{
				if (projects.TryGetValue(projectId, out project))
				{
					return true;
				}
			}
			finally
			{
				rwLockProjects.ExitReadLock();
			}

			project = getProjectById(projectId);
			if (project == null) return false;

			rwLockProjects.EnterWriteLock();
			try
			{
				projects[projectId] = project; //race is ok here...
				return true;
			}
			finally
			{
				rwLockProjects.ExitWriteLock();
			}
		}
	}
}
