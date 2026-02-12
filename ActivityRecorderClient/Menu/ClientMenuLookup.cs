using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.Menu
{
	public class ClientMenuLookup
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<int, WorkDataWithParentNames> workDataDict = new Dictionary<int, WorkDataWithParentNames>();
		private readonly Dictionary<string, WorkDataWithParentNames> workDataExtMappingDict = new Dictionary<string, WorkDataWithParentNames>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<int, WorkDataWithParentNames> projectDataDict = new Dictionary<int, WorkDataWithParentNames>();
		private readonly Dictionary<string, WorkDataWithParentNames> projectDataExtMappingDict = new Dictionary<string, WorkDataWithParentNames>(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> ignoredWorkExtMapping = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> ignoredProjectExtMapping = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private List<HashSet<int>> workIdsByProject = new List<HashSet<int>>();
		private readonly Dictionary<int, CategoryData> allCategoriesById = new Dictionary<int, CategoryData>();
		private readonly Dictionary<int, WorkDataWithParentNames> projectByWorkId = new Dictionary<int, WorkDataWithParentNames>();
		private readonly HashSet<int> dynamicWorkIds = new HashSet<int>();
		protected CompositeMapping ignoreCaseExternalCompositeMapping = null;

		public Dictionary<int, CategoryData> AllCategoriesById { get { return allCategoriesById; /*returning a copy would be the proper thing to do*/ } }
		public Dictionary<int, WorkDataWithParentNames> WorkDataById { get { return workDataDict; /*returning a copy would be the proper thing to do*/ } }
		public Dictionary<int, WorkDataWithParentNames> ProjectDataById { get { return projectDataDict; /*returning a copy would be the proper thing to do*/ } }
		public Dictionary<string, WorkDataWithParentNames> WorkDataExtMapping { get { return workDataExtMappingDict; /*returning a copy would be the proper thing to do*/ } }
		public HashSet<string> IgnoredWorkExtMapping { get { return ignoredWorkExtMapping; /*returning a copy would be the proper thing to do*/ } }
		public Dictionary<string, WorkDataWithParentNames> ProjectExtMapping { get { return projectDataExtMappingDict; /*returning a copy would be the proper thing to do*/ } }
		public HashSet<string> IgnoredProjectExtMapping { get { return ignoredProjectExtMapping; /*returning a copy would be the proper thing to do*/ } }
		public Dictionary<int, WorkDataWithParentNames> ProjectByWorkId { get { return projectByWorkId; } }

		private WorkDataWithParentNames defaultWork;
		public WorkDataWithParentNames DefaultWork {
			get
			{
				return defaultWork;
			}

			set
			{
				Debug.Assert(value == null || (value.WorkData != null && value.WorkData.Id != null && WorkDataById.ContainsKey(value.WorkData.Id.Value)));
				if (value != null)
				{
					value.WorkData.IsDefault = true;
				}

				defaultWork = value;
			} 
		}

		protected ClientMenu clientMenu;
		public ClientMenu ClientMenu
		{
			get { return clientMenu; }
			set
			{
				if (clientMenu == value) return;
				clientMenu = value;
				workDataDict.Clear();
				workDataExtMappingDict.Clear();
				ignoredWorkExtMapping.Clear();
				projectDataDict.Clear();
				projectByWorkId.Clear();
				projectDataExtMappingDict.Clear();
				ignoredProjectExtMapping.Clear();
				allCategoriesById.Clear();
				dynamicWorkIds.Clear();
				ignoreCaseExternalCompositeMapping = null;
				if (clientMenu.CategoriesById != null)
				{
					foreach (var keyValuePair in clientMenu.CategoriesById)
					{
						allCategoriesById.Add(keyValuePair.Key, keyValuePair.Value);
					}
				}
				foreach (var workData in MenuHelper.FlattenDistinctWorkDataThatHasId(clientMenu, true))
				{
					workDataDict.Add(workData.WorkData.Id.Value, workData);
					if (workData.WorkData.CategoryId.HasValue && !allCategoriesById.ContainsKey(workData.WorkData.CategoryId.Value))
					{
						allCategoriesById.Add(
							workData.WorkData.CategoryId.Value,
							new CategoryData()
							{
								Id = workData.WorkData.CategoryId.Value,
								Name = Labels.UnknownCategory,
							}
						);
					}
				}
				if (clientMenu.ExternalWorkIdMapping != null)
				{
					foreach (var keyValuePair in clientMenu.ExternalWorkIdMapping)
					{
						if (keyValuePair.Key == null) continue;
						WorkDataWithParentNames work;
						if (keyValuePair.Value == -1) //ignored id
						{
							ignoredWorkExtMapping.Add(keyValuePair.Key);
						}
						else if (workDataDict.TryGetValue(keyValuePair.Value, out work)) //valid mapping
						{
							try
							{
								workDataExtMappingDict.Add(keyValuePair.Key, work);
							}
							catch (ArgumentException)
							{
								if (workDataExtMappingDict[keyValuePair.Key].WorkData.Id != work.WorkData.Id)
								{
									log.ErrorAndFail(keyValuePair.Key + " workkey is already in ExternalWorkIdMapping with value " + workDataExtMappingDict[keyValuePair.Key].WorkData.Id + " new value " + work.WorkData.Id + " ignored");
								}
							}
						}
						else
						{
							//else we have a workid without valid work 
							//we won't add to ignore list so we can assign the work for us later
							log.Debug("Ignoring invalid workId in ExternalWorkIdMapping " + keyValuePair.Value);
						}
					}
				}
				foreach (var workData in MenuHelper.FlattenDistinctWorkDataThatHasProjectId(clientMenu))
				{
					projectDataDict.Add(workData.WorkData.ProjectId.Value, workData);
					if (workData.WorkData.Children == null) continue;
					workData.WorkData.Children.RemoveAll(c => c == null); // workaround if any null child exists
					foreach (var projectWork in workData.WorkData.Children.Where(c => c.Id.HasValue))
					{
						projectByWorkId.Add(projectWork.Id.Value, workData);
					}
				}
				workIdsByProject = MenuHelper.GetWorkIdsByProject(clientMenu);
				if (clientMenu.ExternalProjectIdMapping != null)
				{
					foreach (var keyValuePair in clientMenu.ExternalProjectIdMapping)
					{
						if (keyValuePair.Key == null) continue;
						WorkDataWithParentNames project;
						if (keyValuePair.Value == -1) //ignored id
						{
							ignoredProjectExtMapping.Add(keyValuePair.Key);
						}
						else if (projectDataDict.TryGetValue(keyValuePair.Value, out project))
						{
							try
							{
								projectDataExtMappingDict.Add(keyValuePair.Key, project);
							}
							catch (ArgumentException)
							{
								if (projectDataExtMappingDict[keyValuePair.Key].WorkData.ProjectId != project.WorkData.ProjectId)
								{
									log.ErrorAndFail(keyValuePair.Key + " projectkey is already in ExternalProjectIdMapping with value " + projectDataExtMappingDict[keyValuePair.Key].WorkData.ProjectId + " new value " + project.WorkData.ProjectId + " ignored");
								}
							}
						}
						else
						{
							log.Debug("Ignoring invalid workId in ExternalProjectIdMapping " + keyValuePair.Value);
						}
					}
				}
				if (clientMenu.ExternalCompositeMapping != null)
				{
					ignoreCaseExternalCompositeMapping = new CompositeMapping();
					CopyCompositeMapping(clientMenu.ExternalCompositeMapping, ignoreCaseExternalCompositeMapping, workDataDict);
				}
				foreach (var workId in GetDynWorkIds())
				{
					dynamicWorkIds.Add(workId);
				}
			}
		}

		private static void CopyCompositeMapping(CompositeMapping src, CompositeMapping dst, Dictionary<int, WorkDataWithParentNames> workDataDict) //menu xml cannot be too deep so we can recurse
		{
			Debug.Assert(dst != null);
			if (src == null) return;
			if (src.WorkIdByKey != null)
			{
				if (dst.WorkIdByKey == null) dst.WorkIdByKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				foreach (var kvp in src.WorkIdByKey)
				{
					if (kvp.Value != -1 && !workDataDict.ContainsKey(kvp.Value))
					{
						log.Debug("Ignoring invalid workId in CompositeMapping " + kvp.Value);
						continue;
					}
					try
					{
						dst.WorkIdByKey.Add(kvp.Key, kvp.Value);
					}
					catch (ArgumentException)
					{
						if (kvp.Value != dst.WorkIdByKey[kvp.Key])
						{
							log.ErrorAndFail(kvp.Key + " workkey is already in dict with value " + dst.WorkIdByKey[kvp.Key] + " new value " + kvp.Value + " ignored");
						}
					}
				}
			}
			if (src.ChildrenByKey != null)
			{
				if (dst.ChildrenByKey == null) dst.ChildrenByKey = new Dictionary<string, CompositeMapping>(StringComparer.OrdinalIgnoreCase);
				foreach (var kvp in src.ChildrenByKey)
				{
					CompositeMapping curr;
					if (!dst.ChildrenByKey.TryGetValue(kvp.Key, out curr))
					{
						curr = new CompositeMapping();
						dst.ChildrenByKey.Add(kvp.Key, curr);
					}
					CopyCompositeMapping(kvp.Value, curr, workDataDict);
				}
			}
		}

		public WorkDataWithParentNames GetWorkDataWithParentNames(int id)
		{
			WorkDataWithParentNames result;
			return workDataDict.TryGetValue(id, out result) ? result : null;
		}

		public List<WorkDataWithParentNames> GetWorksForCategoryId(int currentWorkId, int categoryId)
		{
			var currProj = workIdsByProject.Where(n => n.Contains(currentWorkId)).FirstOrDefault();
			if (currProj == null) return null;
			return currProj
				.Select(n => GetWorkDataWithParentNames(n))
				.Where(n => n != null)
				.Where(n => n.WorkData.CategoryId == categoryId)
				.ToList();
		}

		private static readonly List<WorkData> emptyWorkData = Enumerable.Empty<WorkData>().ToList();
		public List<WorkData> GetWorksForProjectId(int projectId)
		{
			WorkDataWithParentNames rootProj;
			if (!projectDataDict.TryGetValue(projectId, out rootProj))
			{
				return emptyWorkData;
			}
			var result = new List<WorkData>();
			var projStack = new Stack<WorkData>();
			projStack.Push(rootProj.WorkData);
			while (projStack.Count != 0)
			{
				var proj = projStack.Pop();
				if (proj == null || proj.Children == null) continue;
				foreach (var workData in proj.Children)
				{
					if (workData.Id.HasValue) //work
					{
						result.Add(workData);
					}
					if (workData.ProjectId.HasValue) //project
					{
						projStack.Push(workData);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Returns the <see cref="WorkDataWithParentNames"/> from the ClientMenu associated with the provided <see cref="AssignData"/> key.
		/// </summary>
		/// <param name="key">The key of the dynamic task.</param>
		/// <param name="ignored">True if the task is ignored (rejected).</param>
		/// <returns><see cref="WorkDataWithParentNames"/> created from the provided keys if it exists in the ClientMenu, otherwise null.</returns>
		public WorkDataWithParentNames GetWorkForAssignData(AssignData key, out bool ignored)
		{
			WorkDataWithParentNames result;
			if (key.Work != null)
			{
				ignored = IgnoredWorkExtMapping.Contains(key.Work.WorkKey);
				return ignored || !WorkDataExtMapping.TryGetValue(key.Work.WorkKey, out result) ? null : result;
			}
			else if (key.Project != null)
			{
				ignored = IgnoredProjectExtMapping.Contains(key.Project.ProjectKey);
				return ignored || !ProjectExtMapping.TryGetValue(key.Project.ProjectKey, out result) ? null : result;
			}
			else if (key.Composite != null)
			{
				return GetWorkForCompositeKey(key.Composite.WorkKey, key.Composite.ProjectKeys, out ignored);
			}
			else
			{
				log.ErrorAndFail("Invalid AssignData " + key);
				ignored = true; //don't retry
				return null;
			}
		}

		/// <summary>
		/// Returns the <see cref="WorkDataWithParentNames"/> from the ClientMenu associated with the provided composite keys.
		/// </summary>
		/// <param name="workKey">The key identifier of the dynamic task.</param>
		/// <param name="projectKeys">The project key identifiers of the dynamic task.</param>
		/// <param name="ignored">True if the task is ignored (rejected).</param>
		/// <returns><see cref="WorkDataWithParentNames"/> created from the provided keys if it exists in the ClientMenu, otherwise null.</returns>
		public WorkDataWithParentNames GetWorkForCompositeKey(string workKey, IEnumerable<string> projectKeys, out bool ignored)
		{
			var currLevel = ignoreCaseExternalCompositeMapping;
			foreach (var projectKey in projectKeys)
			{
				if (currLevel == null) break;
				if (projectKey == null) break;
				if (currLevel.ChildrenByKey == null || !currLevel.ChildrenByKey.TryGetValue(projectKey, out currLevel))
				{
					currLevel = null;
				}
			}
			int workId;
			if (currLevel == null || currLevel.WorkIdByKey == null || !currLevel.WorkIdByKey.TryGetValue(workKey, out workId))
			{
				ignored = false;
				return null;
			}
			if (workId == -1)
			{
				ignored = true;
				return null;
			}
			WorkDataWithParentNames result;
			if (!WorkDataById.TryGetValue(workId, out result))
			{
				result = null;
			}
			ignored = false;
			return result;
		}

		public bool IsDynamicWork(int workId)
		{
			return dynamicWorkIds.Contains(workId);
		}

		private IEnumerable<int> GetDynWorkIds()
		{
			return WorkDataExtMapping.Values.Select(n => n.WorkData.Id.Value)
				.Concat(ProjectExtMapping.Values.SelectMany(n => GetWorksForProjectId(n.WorkData.ProjectId.Value).Select(m => m.Id.Value)))
				.Concat(GetDynCompositeWorkIds());
		}

		private IEnumerable<int> GetDynCompositeWorkIds()
		{
			var queue = new Queue<CompositeMapping>();
			var curr = ClientMenu == null ? null : ClientMenu.ExternalCompositeMapping;
			while (curr != null)
			{
				if (curr.WorkIdByKey != null)
				{
					foreach (var workId in curr.WorkIdByKey.Values)
					{
						yield return workId;
					}
				}
				if (curr.ChildrenByKey != null)
				{
					foreach (var child in curr.ChildrenByKey.Values)
					{
						if (child == null) continue;
						queue.Enqueue(child);
					}
				}
				curr = queue.Count > 0 ? queue.Dequeue() : null;
			}
		}
	}
}
