using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;

namespace JC.Removal.Component
{
	class TaskSchedulerComponent: BaseComponent
	{
		private const string startTaskName = "JcStart";
		private const string userFriendlyName = "Task scheduler";

		public override string GetUserFriendlyName()
		{
			return userFriendlyName;
		}

		public override bool Remove(out string error)
		{
			try
			{
				using (var ts = new TaskService())
				{
					var rootFolderTasks = ts.RootFolder.Tasks;
					var jcTask = rootFolderTasks.SingleOrDefault(t => t.Name == startTaskName);
					if (jcTask != null)
					{
						ts.RootFolder.DeleteTask(startTaskName);
						error = null;
						return true;
					}

					error = null;
					return true;
				}
			}
			catch (Exception ex)
			{
				error = ex.ToString();
				return false;
			}
		}

		public override bool IsInstalled()
		{
			using (var ts = new TaskService())
			{
				var rootFolderTasks = ts.RootFolder.Tasks;
				var jcTask = rootFolderTasks.SingleOrDefault(t => t.Name == startTaskName);
				if (jcTask != null)
				{
					return true;
				}
				return false;
			}
		}

		public override string[] GetProcessesNames()
		{
			return new string[0];
		}
	}
}
