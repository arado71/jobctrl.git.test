using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Menu.Management
{
	/// <summary>
	/// Class for updating reason stats
	/// </summary>
	public class TaskReasonsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int callbackInterval = 15 * 60 * 1000;  //15 mins /**/1 GetTaskReasons 241 bytes/call inside, in variables; but 60 packets 24008 bytes/call outside, in Ethernet packets
		private const int callbackRetryInterval = 20 * 1000;  //20 secs
		private static string FilePath { get { return "TaskReasons-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<TaskReasons>> TaskReasonsChanged;

		private TaskReasons taskReasons = new TaskReasons();
		private TaskReasons TaskReasons
		{
			get { return taskReasons; } //we don't need to expose this atm. (and it's not thread-safe)
			set
			{
				if (value == null) //cannot save null value
				{
					TaskReasons = new TaskReasons();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(taskReasons, value)) return;
				log.Info("TaskReasons changed");
				taskReasons = value;
				IsolatedStorageSerializationHelper.Save(FilePath, value);
				OnTaskReasonsChanged(value);
			}
		}

		protected bool lastSendFailed;

		public TaskReasonsManager()
			: this(log)
		{
		}

		protected TaskReasonsManager(ILog log)
			: base(log)
		{
		}

		protected override int ManagerCallbackInterval
		{
			get { return lastSendFailed ? callbackRetryInterval : callbackInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				int userId = ConfigManager.UserId;
				var reasons = ActivityRecorderClientWrapper.Execute(n => n.GetTaskReasons(userId));
				lastSendFailed = false;
				TaskReasons = reasons;
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get TaskReasons", log, ex);
				lastSendFailed = true; //retry shortly
			}
		}

		public void LoadData()
		{
			log.Info("Loading TaskReasons from disk");
			TaskReasons value;
			if (IsolatedStorageSerializationHelper.Exists(FilePath)
				&& IsolatedStorageSerializationHelper.Load(FilePath, out value))
			{
				taskReasons = value;
			}
			OnTaskReasonsChanged(taskReasons); //always raise so we know the initial state
		}

		protected virtual void OnTaskReasonsChanged(TaskReasons value)
		{
			Debug.Assert(value != null);
			var del = TaskReasonsChanged;
			if (del == null) return;
			del(this, SingleValueEventArgs.Create(value));
		}
	}
}