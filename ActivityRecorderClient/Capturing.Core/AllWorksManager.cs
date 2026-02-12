using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Retrieves all works from the service and persists them
	/// </summary>
	public class AllWorksManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int callbackInterval = 24 * 60 * 60 * 1000;  //24 hours
		private const int callbackRetryInterval = 60 * 1000;  //60 secs
		private static string FilePath { get { return "AllWorks-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<List<AllWorkItem>>> AllWorksChanged;

		private AllWorksData allWorkData = new AllWorksData { WorkItems = new List<AllWorkItem>() };

		private List<AllWorkItem> AllWorks
		{
			get { return allWorkData.WorkItems; }
			set
			{
				if (value != null)
				{
					log.Info("AllWorks changed");
					allWorkData.WorkItems = value;
					allWorkData.LastRefreshTime = DateTime.Now;
					IsolatedStorageSerializationHelper.Save(FilePath, allWorkData);
				}
				else
					allWorkData.WorkItems = new List<AllWorkItem>();
			}
		}

		public bool Available { get { return allWorkData.WorkItems.Count > 0; } }

		private bool lastSendFailed;

		public AllWorksManager()
			: base(log)
		{
		}

		public static List<WorkData> GenWorkDatas(List<AllWorkItem> allWorks, bool filterOwnTasks, bool filterClosedTasks)
		{
			var result = new List<WorkData>();
			var groups = allWorks
							.Where(c => c.ParentId.HasValue)
							.GroupBy(c => c.ParentId)
							.Select(g => new
							{
								Key = g.Key.Value,
								Value = g
									.Where(c => c.Type <= 1
										|| ((!filterOwnTasks || c.OwnTask)
											  && (filterClosedTasks || !c.ClosedAt.HasValue)))
									.ToList()
							})
							.Where(g => g.Value.Count > 0)
							.ToDictionary(g => g.Key, g => g.Value);
			foreach (var item in allWorks.Where(c => !c.ParentId.HasValue))
				result.Add(CollectData(groups, item));
#if DEBUG
			int cnt = groups.Sum(g => g.Value.Count());
			log.Debug(cnt + " workdata processed");
#endif
			return result;
		}

		private static WorkData CollectData(Dictionary<int, List<AllWorkItem>> groups, AllWorkItem data)
		{
			var result = new WorkData { Id = data.Type > 1 ? data.TaskId : (int?)null, Name = data.Name, Type = data.Type };
			var children = new List<WorkData>();
			List<AllWorkItem> group;
			if (groups.TryGetValue(data.TaskId, out group))
				foreach (var item in group)
				{
					var collected = CollectData(groups, item);
					if (collected.Id.HasValue || collected.Children != null)
						children.Add(collected);
				}
			if (children.Count > 0)
				result.Children = children;
			return result;
		}

		public void SynchronizeDataAsync()
		{
			ThreadPool.QueueUserWorkItem(_ => RestartTimer(), null);
		}

		private void SynchronizeDataInt()
		{
			try
			{
				int userId = ConfigManager.UserId;
				AllWorks = ActivityRecorderClientWrapper.Execute(n => n.GetAllWorks(userId));
				lastSendFailed = false;
				OnAllWorksChanged(AllWorks);
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get AllWorks", log, ex);
				lastSendFailed = true; //retry shortly
			}
		}

		public static AssignTaskResult TryAssignTask(WorkData workData)
		{
			Debug.Assert(workData.Id.HasValue);
			try
			{
				int userId = ConfigManager.UserId;
				int taskId = workData.Id.Value;
				var res = ActivityRecorderClientWrapper.Execute(n => n.AssignTask(userId, taskId));
				log.Info("Assign work " + taskId + " result was " + res);
				return res;
			}
			catch (Exception ex)
			{
				log.Info("Assign work " + workData.Id + " failed");
				WcfExceptionLogger.LogWcfError("AssignTask", log, ex);
				return AssignTaskResult.UnknownError;
			}
		}

		public void Start()
		{
			LoadData();
			var timeToNextRefresh = allWorkData.LastRefreshTime.HasValue ? (allWorkData.LastRefreshTime.Value - DateTime.Now).TotalMilliseconds + callbackInterval : 0;
			if (timeToNextRefresh < 0d)
				timeToNextRefresh = 0d;
			base.Start((int)timeToNextRefresh);
			log.Debug("first refresh after " + timeToNextRefresh + " ms");
		}

		public override void Stop()
		{
			base.Stop();
			AllWorks = null;

		}

		protected override int ManagerCallbackInterval
		{
			get { return lastSendFailed ? callbackRetryInterval : callbackInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			SynchronizeDataInt();
		}

		public void LoadData()
		{
			log.Info("Loading AllWorks from disk");
			AllWorksData value;
			if (IsolatedStorageSerializationHelper.Exists(FilePath)
				&& IsolatedStorageSerializationHelper.Load(FilePath, out value))
			{
				allWorkData = value;
				OnAllWorksChanged(allWorkData.WorkItems);
			}
		}

		private void OnAllWorksChanged(List<AllWorkItem> value)
		{
			Debug.Assert(value != null);
			var del = AllWorksChanged;
			if (del == null) return;
			del(this, SingleValueEventArgs.Create(value));
		}
	}

	[Serializable()]
	public class AllWorksData
	{
		public DateTime? LastRefreshTime { get; set; }
		public List<AllWorkItem> WorkItems { get; set; }
	}
}
