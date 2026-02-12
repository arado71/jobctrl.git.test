namespace ActivityRecorderClientAV
{
	public abstract class SvgIconAV
	{
		public abstract string LightThemeFile { get; }
		public abstract string DarkThemeFile { get; }

		public string GetFileName(bool isLightTheme) =>
			isLightTheme ? LightThemeFile : DarkThemeFile;
	}

	public class SvgSettingsIcon : SvgIconAV
	{
		public override string LightThemeFile => "settings.svg";
		public override string DarkThemeFile => "settings_dark.svg";
	}

	public class SvgExitIcon : SvgIconAV
	{
		public override string LightThemeFile => "exit.svg";
		public override string DarkThemeFile => "exit_dark.svg";
	}

	public class SvgGlobeIcon : SvgIconAV
	{
		public override string LightThemeFile => "globe.svg";
		public override string DarkThemeFile => "globe_dark.svg";
	}

	public class SvgMoreIcon : SvgIconAV
	{
		public override string LightThemeFile => "more.svg";
		public override string DarkThemeFile => "more_dark.svg";
	}

	public class SvgChartIcon : SvgIconAV
	{
		public override string LightThemeFile => "chart.svg";
		public override string DarkThemeFile => "chart_dark.svg";
	}

	public class SvgToDoIcon : SvgIconAV
	{
		public override string LightThemeFile => "todo.svg";
		public override string DarkThemeFile => "todo_dark.svg";
	}

	public class SvgMessagesIcon : SvgIconAV
	{
		public override string LightThemeFile => "messages.svg";
		public override string DarkThemeFile => "messages_dark.svg";
	}

	public class SvgSearchIcon : SvgIconAV
	{
		public override string LightThemeFile => "search.svg";
		public override string DarkThemeFile => "search_dark.svg";
	}

	public class SvgHomeIcon : SvgIconAV
	{
		public override string LightThemeFile => "home.svg";
		public override string DarkThemeFile => "home_dark.svg";
	}

	public class SvgUpIcon : SvgIconAV
	{
		public override string LightThemeFile => "folder_up.svg";
		public override string DarkThemeFile => "folder_up_dark.svg";
	}

	public class SvgAddTaskIcon : SvgIconAV
	{
		public override string LightThemeFile => "square-plus.svg";
		public override string DarkThemeFile => "square-plus_dark.svg";
	}

	public class SvgIssuesIcon : SvgIconAV
	{
		public override string LightThemeFile => "issues.svg";
		public override string DarkThemeFile => "issues_dark.svg";
	}

	public class SvgDebugIcon : SvgIconAV
	{
		public override string LightThemeFile => "debug.svg";
		public override string DarkThemeFile => "debug_dark.svg";
	}

	public class SvgCloseIcon : SvgIconAV
	{
		public override string LightThemeFile => "close.svg";
		public override string DarkThemeFile => "close_dark.svg";
	}

	public class SvgRecentTasksIcon : SvgIconAV
	{
		public override string LightThemeFile => "recent_tasks.svg";
		public override string DarkThemeFile => "recent_tasks_dark.svg";
	}

	public class SvgRecentProjectsIcon : SvgIconAV
	{
		public override string LightThemeFile => "recent_projects.svg";
		public override string DarkThemeFile => "recent_projects_dark.svg";
	}

	public class SvgDeadlineIcon : SvgIconAV
	{
		public override string LightThemeFile => "deadline.svg";
		public override string DarkThemeFile => "deadline_dark.svg";
	}

	public class SvgPriorityIcon : SvgIconAV
	{
		public override string LightThemeFile => "priority.svg";
		public override string DarkThemeFile => "priority_dark.svg";
	}

	public class SvgProgressIcon : SvgIconAV
	{
		public override string LightThemeFile => "progress.svg";
		public override string DarkThemeFile => "progress_dark.svg";
	}

	public class SvgAllTasksIcon : SvgIconAV
	{
		public override string LightThemeFile => "all_tasks.svg";
		public override string DarkThemeFile => "all_tasks_dark.svg";
	}

	public class SvgThemeIcon : SvgIconAV
	{
		public override string LightThemeFile => "sun.svg";
		public override string DarkThemeFile => "moon.svg";
	}

	public class SvgSystemThemeIcon : SvgIconAV
	{
#if WINDOWS
		public override string LightThemeFile => "windows.svg";
		public override string DarkThemeFile => "windows_dark.svg";
#elif MACOS
            public override string LightThemeFile => "apple.svg";
            public override string DarkThemeFile => "apple_dark.svg";
#elif LINUX
            //public override string LightThemeFile => "linux.svg";
            //public override string DarkThemeFile => "linux_dark.svg";
#endif
	}

	public class SvgWorkstateOnIcon : SvgIconAV
	{
		public override string LightThemeFile => "toggle-on.svg";
		public override string DarkThemeFile => "toggle-on_dark.svg";
	}

	public class SvgWorkstateOffIcon : SvgIconAV
	{
		public override string LightThemeFile => "toggle-off.svg";
		public override string DarkThemeFile => "toggle-off_dark.svg";
	}

	public class SvgExpandPlusIcon : SvgIconAV
	{
		public override string LightThemeFile => "plus.svg";
		public override string DarkThemeFile => "plus_dark.svg";
	}

	public class SvgExpandMinusIcon : SvgIconAV
	{
		public override string LightThemeFile => "minus.svg";
		public override string DarkThemeFile => "minus_dark.svg";
	}

	public class SvgTrashCanIcon : SvgIconAV
	{
		public override string LightThemeFile => "trash-can.svg";
		public override string DarkThemeFile => "trash-can_dark.svg";
	}

	public class SvgLeftRightIcon : SvgIconAV
	{
		public override string LightThemeFile => "left-right.svg";
		public override string DarkThemeFile => "left-right_dark.svg";
	}

	public class SvgCheckIcon : SvgIconAV
	{
		public override string LightThemeFile => "check.svg";
		public override string DarkThemeFile => "check_dark.svg";
	}

	public class SvgHandPointerIcon : SvgIconAV
	{
		public override string LightThemeFile => "hand-pointer.svg";
		public override string DarkThemeFile => "hand-pointer_dark.svg";
	}

}