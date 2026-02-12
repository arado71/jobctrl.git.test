using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace Tct.ActivityRecorderClient.Update
{
	/// <summary>
	/// Helper class to call TaskScheduler with late binding
	/// </summary>
	public static class TaskSchedulerHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string TaskRunLevel_Highest = "Highest";

		public static bool CheckDesiredTaskSettings(TaskSettings current)
		{
			return !current.StopIfGoingOnBatteries && !current.DisallowStartIfOnBatteries && current.Priority == ProcessPriorityClass.Normal && current.ExecutionTimeLimit == TimeSpan.Zero && current.Enabled;
		}

		public static TaskService CreateServiceClassInstance()
		{
			return new TaskService();
		}

		public static IEnumerable<Task> GetRootFolderTasks(TaskService taskService)
		{
			var rootFolder = taskService.RootFolder;
			var tasks = rootFolder.Tasks;
			return tasks.AsQueryable().OfType<Task>();
		}

		public static string GetTaskName(Task task)
		{
			return task.Name;
		}

		public static string GetExecActionPath(Task task)
		{
			var taskDef = task.Definition;
			var actions = taskDef.Actions;
			if (actions.Count != 1) return null;
			var firstAction = actions.First();
			var path = (firstAction as ExecAction)?.Path;
			return path;
		}

		public static string GetExecActionArgs(Task task)
		{
			var taskDef = task.Definition;
			var actions = taskDef.Actions;
			if (actions.Count != 1) return null;
			var firstAction = actions.First();
			var args = (firstAction as ExecAction)?.Arguments;
			return args;
		}

		public static string GetPrincipalRunLevel(Task task)
		{
			var taskDef = task.Definition;
			var principal = taskDef.Principal;
			var runLevel = principal.RunLevel;
			return runLevel.ToString();
		}

		public static TaskSettings GetTaskSettings(Task task)
		{
			var taskDef = task.Definition;
			var settings = taskDef.Settings;
			return settings;
		}

		public static void DeleteTask(TaskService taskService, string name)
		{
			var rootFolder = taskService.RootFolder;
			rootFolder.DeleteTask(name, false);
		}

		public static TaskDefinition CreateNewTask(TaskService taskService, string path, string runLevel, string args)
		{
			var taskDef = taskService.NewTask();
			var principal = taskDef.Principal;
			principal.RunLevel = (TaskRunLevel)Enum.Parse(typeof(TaskRunLevel), runLevel);
			var settings = taskDef.Settings;
			settings.DisallowStartIfOnBatteries = false;
			settings.Priority = ProcessPriorityClass.Normal;
			settings.ExecutionTimeLimit = TimeSpan.Zero;
			settings.StopIfGoingOnBatteries = false;
			var actions = taskDef.Actions;
			var execAction = new ExecAction(path, args, Path.GetDirectoryName(path));
			actions.Add(execAction);
			return taskDef;
		}

		public static void RegisterTaskDefinition(TaskService taskService, string path, TaskDefinition taskDef)
		{
			var rootFolder = taskService.RootFolder;
			rootFolder.RegisterTaskDefinition(path, taskDef);
		}

		public static void RunTask(Task task)
		{
			task.Run();
		}

	}
}
