using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Caching.Works
{
	/// <summary>
	/// Simple thread-safe WorkHierarchy cache based on ConcurrentDictionary.
	/// </summary>
	/// <remarks>
	/// Reparenting can cause some issues.
	/// </remarks>
	public class WorkHierarchyCacheDict : WorkHierarchyBase
	{
		private readonly ConcurrentDictionary<int, Work> works = new ConcurrentDictionary<int, Work>();
		private readonly ConcurrentDictionary<int, Project> projects = new ConcurrentDictionary<int, Project>();

		private readonly Func<int, Work> getWorkById;
		private readonly Func<int, Project> getProjectById;

		public WorkHierarchyCacheDict(IEnumerable<Work> initialWorks, IEnumerable<Project> initialProjects, Func<int, Work> getWorkById, Func<int, Project> getProjectById)
			: this(initialWorks.ToDictionary(n => n.Id), initialProjects.ToDictionary(n => n.Id), getWorkById, getProjectById)
		{
		}

		public WorkHierarchyCacheDict(IEnumerable<KeyValuePair<int, Work>> initialWorks, IEnumerable<KeyValuePair<int, Project>> initialProjects, Func<int, Work> getWorkById, Func<int, Project> getProjectById)
		{
			if (getWorkById == null) throw new ArgumentNullException("getWorkById");
			if (getProjectById == null) throw new ArgumentNullException("getProjectById");

			works = new ConcurrentDictionary<int, Work>(initialWorks);
			projects = new ConcurrentDictionary<int, Project>(initialProjects);

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
