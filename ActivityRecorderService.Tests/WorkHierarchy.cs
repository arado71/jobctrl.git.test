using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Caching.Works;

namespace Tct.Tests.ActivityRecorderService
{
	public class WorkHierarchy : WorkHierarchyBase, IWorkHierarchyService
	{
		public static readonly WorkHierarchy Empty = new WorkHierarchy(Enumerable.Empty<Work>(), Enumerable.Empty<Project>());

		public Dictionary<int, Work> Works { get; private set; }
		public Dictionary<int, Project> Projects { get; private set; }

		public WorkHierarchy(Dictionary<int, Work> works, Dictionary<int, Project> projects)
		{
			Works = works;
			Projects = projects;
		}

		private WorkHierarchy(IEnumerable<Work> works, IEnumerable<Project> projects)
		{
			Works = works.ToDictionary(n => n.Id);
			Projects = projects.ToDictionary(n => n.Id);
		}

		protected internal override bool TryGetWork(int workId, out Work work)
		{
			return Works.TryGetValue(workId, out work);
		}

		protected internal override bool TryGetProject(int projectId, out Project project)
		{
			return Projects.TryGetValue(projectId, out project);
		}

		bool IWorkHierarchyService.TryGetWork(int workId, out Work work)
		{
			Work orig;
			if (TryGetWork(workId, out orig))
			{
				work = new Work() { Id = workId, Name = orig.Name, ProjectId = orig.ProjectId };
				return true;
			}
			work = null;
			return false;
		}

		bool IWorkHierarchyService.TryGetProject(int projectId, out Project project)
		{
			Project orig;
			if (TryGetProject(projectId, out orig))
			{
				project = new Project() { Id = orig.Id, Name = orig.Name, ParentId = orig.ParentId };
				return true;
			}
			project = null;
			return false;
		}
	}

	public static class WorkHierarchyServiceForTests
	{
		private static readonly IWorkHierarchyService wrapperService = new WrapperService();

		public static IWorkHierarchyService CurrentService { get; set; }

		public static void Reset()
		{
			CurrentService = new WorkHierarchyManagerDict();
		}

		static WorkHierarchyServiceForTests()
		{
			Reset();
			WorkHierarchyService.FactoryForInstance = () => wrapperService;
		}

		private class WrapperService : IWorkHierarchyService
		{
			public bool TryGetWork(int workId, out Work work)
			{
				return CurrentService.TryGetWork(workId, out work);
			}

			public bool TryGetProject(int projectId, out Project project)
			{
				return CurrentService.TryGetProject(projectId, out project);
			}

			public string GetWorkNameWithProjects(int workId, int targetLength = int.MaxValue)
			{
				return CurrentService.GetWorkNameWithProjects(workId, targetLength);
			}

			public string GetWorkName(int workId, int targetLength = int.MaxValue)
			{
				return CurrentService.GetWorkName(workId, targetLength);
			}
		}
	}
}
