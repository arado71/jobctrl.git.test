using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Caching.Works
{
	/// <summary>
	/// Simple thread-safe WorkHierarchy cache based on LruCache.
	/// </summary>
	/// <remarks>
	/// Reparenting can cause some issues.
	/// </remarks>
	public class WorkHierarchyCacheLru : WorkHierarchyBase
	{
		private readonly LruCache<int, Work> works;
		private readonly LruCache<int, Project> projects;

		private readonly Func<int, Work> getWorkById;
		private readonly Func<int, Project> getProjectById;

		public WorkHierarchyCacheLru(IEnumerable<Work> initialWorks, IEnumerable<Project> initialProjects, Func<int, Work> getWorkById, Func<int, Project> getProjectById, int workCap = 500000, int projCap = 100000)
		{
			if (getWorkById == null) throw new ArgumentNullException("getWorkById");
			if (getProjectById == null) throw new ArgumentNullException("getProjectById");

			works = new LruCache<int, Work>(workCap);
			projects = new LruCache<int, Project>(projCap);

			foreach (var work in initialWorks)
			{
				works.TryAdd(work.Id, work);
			}

			foreach (var project in initialProjects)
			{
				projects.TryAdd(project.Id, project);
			}

			this.getWorkById = getWorkById;
			this.getProjectById = getProjectById;
		}

		protected internal override bool TryGetWork(int workId, out Work work)
		{
			if (!works.TryGetValue(workId, out work))
			{
				work = getWorkById(workId);
				if (work != null)
				{
					works.TryAdd(workId, work);
				}
			}
			return work != null;
		}

		protected internal override bool TryGetProject(int projectId, out Project project)
		{
			if (!projects.TryGetValue(projectId, out project))
			{
				project = getProjectById(projectId);
				if (project != null)
				{
					projects.TryAdd(projectId, project);
				}
			}
			return project != null;
		}
	}
}
