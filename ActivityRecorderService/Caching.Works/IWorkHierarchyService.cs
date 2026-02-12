using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Caching.Works
{
	public interface IWorkHierarchyService
	{
		bool TryGetWork(int workId, out Work work);
		bool TryGetProject(int projectId, out Project project);
		string GetWorkNameWithProjects(int workId, int targetLength = int.MaxValue);
		string GetWorkName(int workId, int targetLength = int.MaxValue);
	}

	//public interface IWorkHierarchyBulkService : IWorkHierarchyService //todo do we need this? - performance could be better
	//{
	//	Dictionary<int, string> GetWorkNamesWithProjects(List<int> workIds, int targetLength = int.MaxValue);
	//	Dictionary<int, string> GetWorkNames(List<int> workIds, int targetLength = int.MaxValue);
	//}
}
