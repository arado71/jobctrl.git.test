using ActivityRecorderClient.Avalonia.ViewModels;
using ActivityRecorderClientAV;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Controller;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class TaskNavigatorViewModel : ViewModelBase
	{
		public readonly TaskViewModel RecentTasks = new() { Name = "Recent Tasks", IconPath = AppResourcesAV.BaseIconPath + "recent_tasks.svg" };
		public readonly TaskViewModel RecentProjects = new() { Name = "Recent Projects", IconPath = AppResourcesAV.BaseIconPath + "recent_projects.svg" };
		public readonly TaskViewModel Deadline = new() { Name = "Deadline", IconPath = AppResourcesAV.BaseIconPath + "deadline.svg" };
		public readonly TaskViewModel Priority = new() { Name = "Priority", IconPath = AppResourcesAV.BaseIconPath + "priority.svg" };
		public readonly TaskViewModel AllTask = new() { Name = "All Tasks", IconPath = AppResourcesAV.BaseIconPath + "all_tasks.svg" };

		private readonly TaskViewModel Root = new() { Name = "Root" };

		private ClientMenuLookup ClientMenuLookup { get; set; } = new();
		private CaptureCoordinator CaptureCoordinator { get; set; }
		private CurrentWorkController CurrentWorkController { get; set; }

		[ObservableProperty]
		private string userName = "";

		[ObservableProperty]
		private string userId = "";

		[ObservableProperty]
		private WorkTimeStats workTimeStats = new();

		[ObservableProperty]
		private TaskViewModel? currentFolder;

		[ObservableProperty]
		private TaskViewModel? currentTask;

		[ObservableProperty]
		private TaskViewModel? lastTask;

		[ObservableProperty]
		private ObservableCollection<TaskViewModel>? currentItems;

		public TaskSearchAutoCompleteViewModel TaskSearch { get; } = new();

		public TaskNavigatorViewModel()
		{
			CurrentFolder = Root;
			Root.Children.Add(RecentTasks);
			Root.Children.Add(RecentProjects);
			Root.Children.Add(Deadline);
			Root.Children.Add(Priority);
			Root.Children.Add(AllTask);
			UpdateCurrentItems();
			TaskSearch.PropertyChanged += TaskSearch_PropertyChanged;
		}

		private void TaskSearch_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TaskSearch.SelectedItem) && TaskSearch.SelectedItem != null)
			{
				int workId = TaskSearch.SelectedItem.WorkId;
				var task = FindTaskViewModelById(workId);
				if (task != null && ClientMenuLookup.WorkDataById.TryGetValue(workId, out var workData))
				{
					CurrentWorkController.UserStartWork(workData.WorkData);
					CurrentTask = task;
					SetLastTask();
				}
				else
				{
					// TODO: mac, error cannot swicth work
				}
			}
		}

		private TaskViewModel? FindTaskViewModelById(int workId)
		{
			var stack = AllTask.Children.ToList();
			while (stack.Count > 0)
			{
				var last = stack[stack.Count - 1];
				if (last.WorkId == workId)
				{
					return last;
				}
				stack.RemoveAt(stack.Count - 1);
				if (last.Children.Count > 0)
				{
					stack.AddRange(last.Children);
				}
			}
			return null;
		}

		public void SetCurrentTaskById(int? workId)
		{
			CurrentTask = workId == null ? null : FindTaskViewModelById(workId.Value);
			SetLastTask();
		}

		private void SetLastTask()
		{
			var lastTaskId = CurrentWorkController.LastUserSelectedOrPermWork?.Id;
			LastTask = lastTaskId != null ? FindTaskViewModelById(lastTaskId.Value) : null;
		}

		[RelayCommand]
		public void GoToHome() => CurrentFolder = Root;

		[RelayCommand]
		public void GoToParent() => CurrentFolder = CurrentFolder?.Parent ?? Root;

		[RelayCommand]
		public void SelectTask(TaskViewModel newValue)
		{
			if (newValue?.WorkId != null)
			{
				if (ClientMenuLookup.WorkDataById.TryGetValue(newValue.WorkId.Value, out var workData))
				{
					CurrentWorkController.UserStartWork(workData.WorkData);
					CurrentTask = newValue;
				}
				else
				{
					// TODO: mac, error cannot swicth work
				}
			}
			else
			{
				CurrentFolder = newValue;
			}
		}


		partial void OnCurrentFolderChanged(TaskViewModel? oldValue, TaskViewModel? newValue)
		{
			UpdateCurrentItems();
		}

		private void UpdateCurrentItems()
		{
			CurrentItems = CurrentFolder?.Children ?? Root.Children;
		}

		public void UpdateMenu(ClientMenuLookup lookup)
		{
			ClientMenuLookup = lookup;
			UpdateTasksAndProgress();
			TaskSearch.MenuChanged(lookup.ClientMenu);
			if (LastTask == null)
			{
				SetLastTask();
			}
		}

		public void UpdateTasksAndProgress()
		{
			if (ClientMenuLookup?.ClientMenu?.Works == null) return;
			SyncViewModel(RecentTasks, new WorkData { Children = GetRecentTasks() }, onlyChildren: true);
			SyncViewModel(RecentProjects, new WorkData { Children = GetRecentProjects() }, onlyChildren: true);
			SyncViewModel(Deadline, new WorkData { Children = GetDeadline() }, onlyChildren: true);
			SyncViewModel(Priority, new WorkData { Children = GetPriority() }, onlyChildren: true);
			SyncViewModel(AllTask, new WorkData { Children = ClientMenuLookup.ClientMenu.Works }, onlyChildren: true);
		}

		public void SetCaptureCoordinator(CaptureCoordinator captureCoordinator)
		{
			CaptureCoordinator = captureCoordinator;
			CurrentWorkController = captureCoordinator.CurrentWorkController;
			CaptureCoordinator.CurrentMenuChanged += CaptureCoordinator_CurrentMenuChanged;
			MenuQuery.Instance.SimpleWorkTimeStats.Changed += SimpleWorkTimeStats_Changed;
		}

		private void SimpleWorkTimeStats_Changed(object? sender, EventArgs e)
		{
			if (ClientMenuLookup?.ClientMenu?.Works == null) return;
			UpdateTasksAndProgress();
		}

		private void CaptureCoordinator_CurrentMenuChanged(object? sender, MenuEventArgs e)
		{
			this.UpdateMenu(e.MenuLookup);
		}

		const int MenuMaxSize = 50;

		private List<WorkData> GetDeadline()
		{
			IEnumerable<WorkDataWithParentNames> flatWorkData = MenuHelper.FlattenDistinctWorkDataThatHasId(ClientMenuLookup.ClientMenu);
			return flatWorkData
				.Where(n => n.WorkData.IsVisibleInMenu && n.WorkData != null && n.WorkData.EndDate.HasValue)
				.OrderBy(n => n.WorkData.EndDate!.Value)
				.ThenByDescending(n => n.WorkData.Priority ?? 0)
				.Take(MenuMaxSize)
				.Select(x => x.WorkData)
				.ToList();
		}

		private List<WorkData> GetPriority()
		{
			IEnumerable<WorkDataWithParentNames> flatWorkData = MenuHelper.FlattenDistinctWorkDataThatHasId(ClientMenuLookup.ClientMenu);
			return flatWorkData
				.Where(n => n.WorkData.IsVisibleInMenu && MenuBuilderHelper.HasPriority(n.WorkData))
				.OrderByDescending(n => n.WorkData.Priority!.Value)
				.Take(MenuMaxSize)
				.Select(x => x.WorkData)
				.ToList();
		}

		private List<WorkData> GetRecentProjects()
		{
			return RecentHelper.GetRecentIds()
				.Where(x => ClientMenuLookup.ProjectByWorkId.ContainsKey(x))
				.Select(x => ClientMenuLookup.ProjectByWorkId[x])
				.Distinct(WorkDataWithParentNames.WorkDataProjectIdComparer)
				.Select(x => x.WorkData)
				.ToList();
		}

		private List<WorkData> GetRecentTasks()
		{
			return RecentHelper.GetRecents()
				.Where(x => x.WorkData.IsVisibleInMenu)
				.Select(x => x.WorkData)
				.ToList();
		}

		private void SyncViewModel(TaskViewModel target, WorkData source, bool onlyChildren = false)
		{
			if (!onlyChildren)
			{
				SyncViewModelProps(target, source);
			}

			foreach (var invalid in target.Children.Where(n => n.WorkId == null && n.ProjectId == null).ToList())
			{
				target.Children.Remove(invalid);
			}
			var targetLu = target.Children.ToDictionary(item => (item.WorkId, item.ProjectId));
			var orderedTarget = new List<TaskViewModel>();

			foreach (var srcItem in source.Children ?? new List<WorkData>())
			{
				var key = (WorkId: srcItem.Id, srcItem.ProjectId);
				if (targetLu.TryGetValue(key, out var matchingTarget))
				{
					targetLu.Remove(key);
				}
				else
				{
					matchingTarget = new TaskViewModel() { WorkId = srcItem.Id, ProjectId = srcItem.ProjectId };
					target.Children.Add(matchingTarget);
				}
				SyncViewModel(matchingTarget, srcItem);
				orderedTarget.Add(matchingTarget);
			}

			// remove children not in source
			foreach (var toRemove in targetLu.Values)
			{
				target.Children.Remove(toRemove);
			}

			// make sure we have the right order
			for (var i = 0; i < orderedTarget.Count; i++)
			{
				var desired = orderedTarget[i];
				if (target.Children[i] != desired)
				{
					target.Children.Move(target.Children.IndexOf(desired), i);
				}
			}

		}

		private void SyncViewModelProps(TaskViewModel target, WorkData source)
		{
			SimpleWorkTimeStats simpleStats = MenuQuery.Instance.SimpleWorkTimeStats.Value;

			var workDataWithParentNames = source.Id.HasValue
				? ClientMenuLookup.WorkDataById[source.Id!.Value]
				: source.ProjectId.HasValue
				? ClientMenuLookup.ProjectDataById[source.ProjectId!.Value]
				: null;

			target.Name = source.Name;
			target.Priority = source.Priority?.ToString();

			string? daysLeftInfo = null;
			double daysProgress = 0;
			var hasDeadline = source.StartDate != null && source.EndDate != null;
			if (hasDeadline)
			{
				TimeSpan proc = DateTime.Now - source.StartDate.Value;
				TimeSpan total = source.EndDate.Value.AddDays(1) - source.StartDate.Value;
				daysProgress = proc.TotalMilliseconds / total.TotalMilliseconds;
				daysLeftInfo = FormatHelper.GetRemainingTime(total - proc);
				if ((total - proc).Ticks > 0)
				{
					daysLeftInfo += " / " + FormatHelper.GetDays(total);
				}
			}

			string? hoursLeftInfo = null;
			double hoursProgress = 0;
			var hasProgress = source.TargetTotalWorkTime != null;
			if (hasProgress && simpleStats != null && source.Id != null)
			{
				var proc = MenuBuilderHelper.GetWorkStatForId(simpleStats, source.Id.Value);
				hoursProgress = proc.TotalWorkTime.TotalMilliseconds / source.TargetTotalWorkTime.Value.TotalMilliseconds;
				hoursLeftInfo = proc.TotalWorkTime.ToHourMinuteString() + " / " + source.TargetTotalWorkTime.Value.ToHourMinuteString();
			}

			target.HoursLeftInfo = hoursLeftInfo;
			target.HoursProgress = hoursProgress;
			target.HoursProgressColor = hoursProgress > 1 ? "Red" : "Black";
			target.DaysLeftInfo = daysLeftInfo;
			target.DaysProgress = daysProgress;
			target.DaysProgressColor = daysProgress > 1 ? "Red" : "Black";
			target.CircleColor = IconColorGenerator.FromInt(source.Id ?? source.ProjectId ?? 0);
			target.FolderPath = workDataWithParentNames?.ParentNames != null ? string.Join(WorkDataWithParentNames.DefaultSeparator, workDataWithParentNames.ParentNames) : "";
			target.Initials = source.Name.Substring(0, Math.Min(2, source.Name.Length));
		}

		public void UpdateWorkTimeStats(WorkTimeStats stats)
		{
			WorkTimeStats.TodaysWorkTimeInMs = stats.TodaysWorkTimeInMs;
			WorkTimeStats.ThisWeeksWorkTimeInMs = stats.ThisWeeksWorkTimeInMs;
			WorkTimeStats.ThisMonthsWorkTimeInMs = stats.ThisMonthsWorkTimeInMs;
			WorkTimeStats.ThisQuarterWorkTimeInMs = stats.ThisQuarterWorkTimeInMs;
			WorkTimeStats.ThisYearWorkTimeInMs = stats.ThisYearWorkTimeInMs;
			WorkTimeStats.TodaysTargetNetWorkTimeInMs = stats.TodaysTargetNetWorkTimeInMs;
			WorkTimeStats.ThisWeeksTargetNetWorkTimeInMs = stats.ThisWeeksTargetNetWorkTimeInMs;
			WorkTimeStats.ThisWeeksTargetUntilTodayNetWorkTimeInMs = stats.ThisWeeksTargetUntilTodayNetWorkTimeInMs;
			WorkTimeStats.ThisMonthsTargetUntilTodayNetWorkTimeInMs = stats.ThisMonthsTargetUntilTodayNetWorkTimeInMs;
			WorkTimeStats.ThisMonthsTargetNetWorkTimeInMs = stats.ThisMonthsTargetNetWorkTimeInMs;
			WorkTimeStats.ThisQuarterTargetNetWorkTimeInMs = stats.ThisQuarterTargetNetWorkTimeInMs;
			WorkTimeStats.ThisQuarterTargetUntilTodayNetWorkTimeInMs = stats.ThisQuarterTargetUntilTodayNetWorkTimeInMs;
			WorkTimeStats.ThisYearTargetNetWorkTimeInMs = stats.ThisYearTargetNetWorkTimeInMs;
			WorkTimeStats.ThisYearTargetUntilTodayNetWorkTimeInMs = stats.ThisYearTargetUntilTodayNetWorkTimeInMs;
			UpdateTasksAndProgress();
		}

		public void UpdateTodaysStats(TimeSpan todaysWorkTime)
		{
			WorkTimeStats.TodaysWorkTimeInMs = (long)todaysWorkTime.TotalMilliseconds;
			UpdateTasksAndProgress();
		}
	}


	public static class IconColorGenerator
	{
		public static string FromInt(int value)
		{
			double hue = (value * 137.508) % 360;

			double saturation = 0.55;
			double lightness = 0.45;

			return HslToHex(hue, saturation, lightness);
		}

		private static string HslToHex(double h, double s, double l)
		{
			double c = (1 - Math.Abs(2 * l - 1)) * s;
			double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
			double m = l - c / 2;

			(double r, double g, double b) = h switch
			{
				< 60 => (c, x, 0.0),
				< 120 => (x, c, 0.0),
				< 180 => (0.0, c, x),
				< 240 => (0.0, x, c),
				< 300 => (x, 0.0, c),
				_ => (c, 0.0, x)
			};

			byte R = (byte)Math.Round((r + m) * 255);
			byte G = (byte)Math.Round((g + m) * 255);
			byte B = (byte)Math.Round((b + m) * 255);

			return $"#{R:X2}{G:X2}{B:X2}";
		}
	}

	public static class FormatHelper
	{
		public static string GetDays(TimeSpan timeSpan)
		{
			var days = timeSpan.GetDays();
			return string.Format("{0} {1}", days, GetForm(days, Labels.Day, Labels.DayPlural));
		}

		private static string GetForm(int qty, string singular, string plural, params object[] args)
		{
			return string.Format(qty != 1 ? plural : singular, args);
		}

		public static string GetRemainingTime(TimeSpan time)
		{
			if (time.Ticks > 0)
			{
				if (time.TotalHours < 24)
				{
					var hours = (int)Math.Floor(time.TotalHours);
					return string.Format(Labels.Left, string.Format("{0} {1}", hours, GetForm(hours, Labels.HourSingular, Labels.HourPlural)));
				}
				else
				{
					var days = time.GetDays();
					return string.Format(Labels.Left, string.Format("{0} {1}", days, GetForm(days, Labels.Day, Labels.DayPlural)));
				}
			}
			else
			{
				var days = time.GetDays();
				return GetForm(-days, Labels.OverdueSingular, Labels.OverduePlural, -days);
			}
		}

		public static string GetDesc(NavigationBase navigation)
		{
			if (navigation == null) return string.Empty;
			return (string.Join(WorkDataWithParentNames.DefaultSeparator, navigation.Path.ToArray()) + WorkDataWithParentNames.DefaultSeparator + navigation.Name) + Environment.NewLine
				+ (navigation.Priority.HasValue ? " " + Labels.WorkData_Priority + ": " + navigation.Priority : string.Empty)
				+ (navigation.StartDate.HasValue ? " " + Labels.WorkData_StartDate + ": " + navigation.StartDate.Value.ToShortDateString() : string.Empty)
				+ (navigation.EndDate.HasValue ? " " + Labels.WorkData_EndDate + ": " + navigation.EndDate.Value.ToShortDateString() : string.Empty) + Environment.NewLine
				+ (" " + Labels.WorkData_WorkedHours + ": " + navigation.UsedTime.ToHourMinuteString())
				+ (navigation.TotalTime.HasValue ? " " + Labels.WorkData_TargetHours + ": " + navigation.TotalTime.Value.ToHourMinuteString() : string.Empty);
		}
	}
}
