using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Menu.Management
{
	/// <summary>
	/// This class is a mess and was coded in a hurry... todo rethink?
	/// </summary>
	public class CoordinatedTaskReasonsManager : TaskReasonsManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object thisLock = new object();
		private int versionCurrent;
		private int versionSynced;
		private int numAdhocExecuting;
		private TaskReasons lastTaskReasons;

		public CoordinatedTaskReasonsManager()
			: base(log)
		{
		}

		public TaskReasons GetSyncedTaskReasons()
		{
			lock (thisLock)
			{
				return versionSynced == versionCurrent ? lastTaskReasons : null;
			}
		}

		protected override void ManagerCallbackImpl() //only one ManagerCallbackImpl can run at any time
		{
			int versionSaved;
			lock (thisLock)
			{
				if (numAdhocExecuting != 0) return;
				versionSaved = versionCurrent;
			}
			base.ManagerCallbackImpl(); //updating TaskReasons
			if (lastSendFailed) return; //don't sync on error
			lock (thisLock)
			{
				versionSynced = versionSaved;
			}
		}

		protected override void OnTaskReasonsChanged(TaskReasons value)
		{
			lock (thisLock)
			{
				lastTaskReasons = value;
			}
			base.OnTaskReasonsChanged(value);
		}

		public void Execute(Action action)
		{
			try
			{
				lock (thisLock)
				{
					numAdhocExecuting++;
					versionCurrent++;
				}
				action();
			}
			finally
			{
				bool refresh;
				lock (thisLock)
				{
					refresh = (--numAdhocExecuting == 0);
				}
				if (refresh) //we need to refresh stats
				{
					RestartTimer(); //there is a race here (which would cause more refreshes) but we mustn't call this while holding the lock. (due deadlock)
				}
			}
		}
	}
}
