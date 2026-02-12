using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Serialization;
using log4net;

namespace Tct.ActivityRecorderClient.Menu
{
	//todo we don't want to rebuild lookup two times when menu changes
	//we need to easily 'parse' and manipulate the menu
	/// <summary>
	/// Extended version of <see cref="ClientMenuLookup"/> to handle local menu manipulation.
	/// </summary>
	public class ClientMenuEditLookup : ClientMenuLookup
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		//todo remove suffix from names ? (and add to parent)
		public bool AddTempWorkToMenu(AssignData key, int tempWorkId)
		{
			Debug.Assert(key != null);
			if (WorkDataById.ContainsKey(tempWorkId)) return false;
			bool ignored;
			if (GetWorkForAssignData(key, out ignored) != null || ignored) return false; //key is already in the menu
			//we need new fake parent for all 3 types
			if (clientMenu == null) clientMenu = new ClientMenu() { Works = new List<WorkData>() };
			if (clientMenu.Works == null) clientMenu.Works = new List<WorkData>();
			if (key.Project != null)
			{
				log.Debug("Local dynamic Projects are not supported atm."); //we need the template to generate this locally
			}
			else if (key.Work != null)
			{
				//todo context? there should be a case insensitive dict projId+key -> workData in clientMenuLookp
				if (key.Work.ProjectId != null)
				{
					log.Debug("Local context based works are not supported atm.");
				}
				else
				{
					var tempWorks = clientMenu.Works.Where(n => n.ProjectId == -101).FirstOrDefault();
					if (tempWorks == null)
					{
						tempWorks = new WorkData() { Name = "P>>", ProjectId = -101, Children = new List<WorkData>() };
						clientMenu.Works.Add(tempWorks);
					}
					return CreateTempWorkFromWork(key, tempWorkId, tempWorks);
				}
			}
			else if (key.Composite != null)
			{
				var tempWorks = clientMenu.Works.Where(n => n.ProjectId == -100).FirstOrDefault();
				if (tempWorks == null)
				{
					tempWorks = new WorkData() { Name = "P>", ProjectId = -100, Children = new List<WorkData>() };
					clientMenu.Works.Add(tempWorks);
				}
				return CreateTempWorkFromComposite(key, tempWorkId, tempWorks);
			}
			else
			{
				log.ErrorAndFail("Invalid AssignData " + key);
			}
			return false;
		}

		private bool CreateTempWorkFromWork(AssignData data, int tempWorkId, WorkData parent)
		{
			var key = data.Work;
			Debug.Assert(key != null);
			var workKey = key.WorkKey;
			if (workKey.IsNullOrWhiteSpace()) throw new Exception("Invalid workKey");
			if (clientMenu.ExternalWorkIdMapping == null) clientMenu.ExternalWorkIdMapping = new Dictionary<string, int>();
			clientMenu.ExternalWorkIdMapping[workKey] = tempWorkId; //we use indexer to allow overwrite (i.e. reassign) of invalid workIds
			var workData = new WorkData() { Name = string.IsNullOrEmpty(key.WorkName) ? workKey : key.WorkName, WorkKey = workKey, Id = tempWorkId, AssignData = data }; //it is crucial to set AssignData so it can be saved into the WorkItems
			var workDataWithParent = new WorkDataWithParentNames() { ParentNames = new List<string> { parent.Name }, WorkData = workData };
			WorkDataExtMapping[workKey] = workDataWithParent; //we use indexer to allow overwrite (i.e. reassign) of invalid workIds
			WorkDataById.Add(workData.Id.Value, workDataWithParent); //this won't throw as it was already checked
			parent.Children.Add(workData);
			return true;
		}

		private bool CreateTempWorkFromComposite(AssignData data, int tempWorkId, WorkData parent)
		{
			var key = data.Composite;
			Debug.Assert(key != null);
			var workKey = key.WorkKey;
			if (workKey.IsNullOrWhiteSpace()) throw new Exception("Invalid workKey");
			var curr = parent;
			if (clientMenu.ExternalCompositeMapping == null)
			{
				clientMenu.ExternalCompositeMapping = new CompositeMapping();
				Debug.Assert(ignoreCaseExternalCompositeMapping == null);
				ignoreCaseExternalCompositeMapping = new CompositeMapping();
			}
			var currMap = clientMenu.ExternalCompositeMapping;
			var currMapI = ignoreCaseExternalCompositeMapping;
			var parentNames = new List<string> { parent.Name };
			foreach (var projectKey in key.ProjectKeys)
			{
				if (projectKey.IsNullOrWhiteSpace()) throw new Exception("Invalid projectKey");
				if (curr.Children == null) curr.Children = new List<WorkData>();
				var child = curr.Children.Where(n => string.Equals(n.ProjectKey, projectKey, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
				if (child == null)
				{
					child = new WorkData() { Name = projectKey, ProjectKey = projectKey };
					parentNames.Add(child.Name);
					curr.Children.Add(child);
				}

				if (currMap.ChildrenByKey == null) currMap.ChildrenByKey = new Dictionary<string, CompositeMapping>();
				var childMap = currMap.ChildrenByKey.Where(n => string.Equals(n.Key, projectKey, StringComparison.OrdinalIgnoreCase)).Select(n => n.Value).FirstOrDefault(); //we cannot use TryGetValue because case might differ
				if (childMap == null)
				{
					childMap = new CompositeMapping();
					currMap.ChildrenByKey.Add(projectKey, childMap);
				}

				if (currMapI.ChildrenByKey == null) currMapI.ChildrenByKey = new Dictionary<string, CompositeMapping>(StringComparer.OrdinalIgnoreCase);
				CompositeMapping childMapI;
				if (!currMapI.ChildrenByKey.TryGetValue(projectKey, out childMapI))
				{
					childMapI = new CompositeMapping();
					currMapI.ChildrenByKey.Add(projectKey, childMapI);
				}

				curr = child;
				currMap = childMap;
				currMapI = childMapI;
			}

			if (curr.Children == null) curr.Children = new List<WorkData>();
			var dst = curr.Children.Where(n => string.Equals(n.WorkKey, workKey, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			if (dst != null)
			{
				log.ErrorAndFail("Work with key " + workKey + " already exists for assignData " + data);
				return false;
			}
			dst = new WorkData() { Name = string.IsNullOrEmpty(key.WorkName) ? workKey : key.WorkName, WorkKey = workKey, Id = tempWorkId, AssignData = data }; //it is crucial to set AssignData so it can be saved into the WorkItems
			curr.Children.Add(dst);
			if (currMap.WorkIdByKey == null) currMap.WorkIdByKey = new Dictionary<string, int>();
			if (currMapI.WorkIdByKey == null) currMapI.WorkIdByKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			currMap.WorkIdByKey[workKey] = dst.Id.Value; //we use indexer to allow overwrite (i.e. reassign) of invalid workIds
			currMapI.WorkIdByKey[workKey] = dst.Id.Value; //we use indexer to allow overwrite (i.e. reassign) of invalid workIds
			var dstP = new WorkDataWithParentNames() { ParentNames = parentNames, WorkData = dst };
			WorkDataById.Add(dst.Id.Value, dstP); //this won't throw as it was already checked
			return true;
		}

		public bool RemoveKeyFromMenu(AssignData data)
		{
			if (data.Work != null)
			{
				var workKey = data.Work.WorkKey;
				if (workKey.IsNullOrWhiteSpace()) return false;
				var result = WorkDataExtMapping.Remove(workKey);
				if (result && clientMenu.ExternalWorkIdMapping != null)
				{
					foreach (var kvpToRemove in clientMenu.ExternalWorkIdMapping.Where(n => string.Equals(n.Key, workKey, StringComparison.OrdinalIgnoreCase)).ToArray())
					{
						clientMenu.ExternalWorkIdMapping.Remove(kvpToRemove.Key);
					}
				}
				return result;
			}
			else if (data.Project != null)
			{
				var projectKey = data.Project.ProjectKey;
				if (projectKey.IsNullOrWhiteSpace()) return false;
				var result = ProjectExtMapping.Remove(projectKey);
				if (result && clientMenu.ExternalProjectIdMapping != null)
				{
					foreach (var kvpToRemove in clientMenu.ExternalProjectIdMapping.Where(n => string.Equals(n.Key, projectKey, StringComparison.OrdinalIgnoreCase)).ToArray())
					{
						clientMenu.ExternalProjectIdMapping.Remove(kvpToRemove.Key);
					}
				}
				return result;
			}
			else if (data.Composite != null)
			{
				var key = data.Composite;
				Debug.Assert(key != null);
				var workKey = key.WorkKey;
				if (workKey.IsNullOrWhiteSpace()) return false;
				var currMap = clientMenu.ExternalCompositeMapping;
				var currMapI = ignoreCaseExternalCompositeMapping;
				return RemoveCompositeKeyFromMenu(currMap, currMapI, key.ProjectKeys, 0, workKey);
			}
			else
			{
				log.ErrorAndFail("Invalid AssignData " + data);
			}
			return false;
		}

		private bool RemoveCompositeKeyFromMenu(CompositeMapping currMap, CompositeMapping currMapI, List<string> projectKeys, int idx, string workKey)
		{
			if (idx < projectKeys.Count)
			{
				var projectKey = projectKeys[idx];
				if (projectKey.IsNullOrWhiteSpace()) return false;

				if (currMapI.ChildrenByKey == null) return false;
				CompositeMapping childMapI;
				if (!currMapI.ChildrenByKey.TryGetValue(projectKey, out childMapI)) return false; //if we cannot find it here it mustn't be in currMap either

				if (currMap.ChildrenByKey == null) return false;
				var result = false;
				foreach (var childMap in currMap.ChildrenByKey.Where(n => string.Equals(n.Key, projectKey, StringComparison.OrdinalIgnoreCase)).Select(n => n.Value).ToArray())
				{
					result |= RemoveCompositeKeyFromMenu(childMap, childMapI, projectKeys, idx + 1, workKey);
				}

				return result;
			}
			else //if (idx >= projectKeys.Count)
			{
				if (currMapI.WorkIdByKey == null) return false;
				if (currMap.WorkIdByKey == null) return false;
				var result = false;
				foreach (var keyToRemove in currMap.WorkIdByKey.Where(n => string.Equals(n.Key, workKey, StringComparison.OrdinalIgnoreCase)).Select(n => n.Key).ToArray())
				{
					result |= currMap.WorkIdByKey.Remove(keyToRemove);
				}
				if (result)
				{
					currMapI.WorkIdByKey.Remove(workKey);
				}
				return result;
			}
		}

		//nothing ensures that ClientMenuLookup is readonly but no one should change it
		public ClientMenuLookup GetReadonlyCopy()
		{
			//we don't modify existing WorkData so shallow copy would be enough (but KISS atm.)
			if (clientMenu == null) return new ClientMenuLookup();
			var res = new ClientMenuLookup() { ClientMenu = clientMenu.DeepClone() };
			if (DefaultWork != null)
			{
				Debug.Assert(DefaultWork.WorkData != null);
				Debug.Assert(DefaultWork.WorkData.Id != null);
				Debug.Assert(res.WorkDataById.ContainsKey(DefaultWork.WorkData.Id.Value));
				res.DefaultWork = res.WorkDataById[DefaultWork.WorkData.Id.Value];
			}

			return res;
		}
	}
}
