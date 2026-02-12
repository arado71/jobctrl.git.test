using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Meeting;

namespace Tct.ActivityRecorderClient.Serialization
{
	public static class WorkItemSerializationHelperConstants
	{
		public static readonly Type[] AllKnownUploadItemTypes = [
			typeof(WorkItem),
			typeof(ManualMeetingItem),
			typeof(PostponedMeetingItem),
			typeof(AcceptanceDateItem),
			typeof(AggregateCollectedItems),
			typeof(CollectedItem),
			typeof(ManualWorkItem),
			typeof(ParallelWorkItem),
			typeof(ReasonItem),
			typeof(TelemetryItem),
			];
	}

	public static class WorkItemSerializationHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly string StoreDir = "WorkItems";

		static WorkItemSerializationHelper()
		{
			IsolatedStorageSerializationHelper.CreateDir(StoreDir);
		}

		public static bool Save(IUploadItem itemToSave)
		{
			if (itemToSave == null) return true;
			string path = GetPath(itemToSave);
			return IsolatedStorageSerializationHelper.Save(path, itemToSave, WorkItemSerializationHelperConstants.AllKnownUploadItemTypes);
		}

		public static bool Delete(IUploadItem itemToDelete)
		{
			if (itemToDelete == null) return true;
			string path = GetPath(itemToDelete);
			return IsolatedStorageSerializationHelper.Delete(path);
		}

		public static List<string> GetPersistedWorkItemPaths()
		{
			var result = new List<string>();
			foreach (var fileName in IsolatedStorageSerializationHelper.GetFileNames(Path.Combine(StoreDir, "u" + ConfigManager.UserId + "_*")))
			{
				string filePath = Path.Combine(StoreDir, fileName);
				var fileCreationDateTime = IsolatedStorageSerializationHelper.GetCreationTime(filePath);
				if (fileCreationDateTime.HasValue && DateTime.Now.Subtract(fileCreationDateTime.Value) > TimeSpan.FromDays(60))
				{
					IsolatedStorageSerializationHelper.Delete(filePath);
				}
				else
				{
					result.Add(filePath);
				}
			}
			result.Sort();
			return result;
		}

		public static IUploadItem LoadWorkItem(string path)
		{
			IUploadItem itemLoaded;
			if (IsolatedStorageSerializationHelper.Load(path, out itemLoaded, WorkItemSerializationHelperConstants.AllKnownUploadItemTypes) && itemLoaded.UserId == ConfigManager.UserId)
			{
				return itemLoaded;
			}
			return null;
		}

		public static bool IsSuccessfullyPersisted(IUploadItem workItem, out string path)
		{
			try
			{
				path = GetPath(workItem);
				var persistedWorkItem = LoadWorkItem(path);
				if (workItem is WorkItem)
				{
					return XmlSerializationHelper.AreTheSame((WorkItem)workItem, (WorkItem)persistedWorkItem);
				}
				else if (workItem is ManualWorkItem)
				{
					return XmlSerializationHelper.AreTheSame((ManualWorkItem)workItem, (ManualWorkItem)persistedWorkItem);
				}
				else if (workItem is ManualMeetingItem)
				{
					return XmlSerializationHelper.AreTheSame((ManualMeetingItem)workItem, (ManualMeetingItem)persistedWorkItem);
				}
				else if (workItem is ParallelWorkItem)
				{
					return XmlSerializationHelper.AreTheSame((ParallelWorkItem)workItem, (ParallelWorkItem)persistedWorkItem);
				}
				else if (workItem is CollectedItem)
				{
					Debug.Fail("Should not be called atm.");
					return XmlSerializationHelper.AreTheSame((CollectedItem)workItem, (CollectedItem)persistedWorkItem);
				}
				else if (workItem is AggregateCollectedItems)
				{
					return XmlSerializationHelper.AreTheSame((AggregateCollectedItems)workItem, (AggregateCollectedItems)persistedWorkItem);
				}
				else if (workItem is ReasonItem)
				{
					return XmlSerializationHelper.AreTheSame((ReasonItem)workItem, (ReasonItem)persistedWorkItem);
				}
				else if (workItem is TelemetryItem)
				{
					return XmlSerializationHelper.AreTheSame((TelemetryItem)workItem, (TelemetryItem)persistedWorkItem);
				}
				else
				{
					return XmlSerializationHelper.AreTheSame(workItem, persistedWorkItem);
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to serialize workItem", ex);
				path = null;
				return false;
			}
		}

		private static string GetPath(IUploadItem item)
		{
			return Path.Combine(StoreDir, "u" + item.UserId + "_" + item.StartDate.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + item.Id);
		}
	}
}
