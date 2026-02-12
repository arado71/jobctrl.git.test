using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using System;
using System.Globalization;
using Tct.ActivityRecorderClient.Avalonia.UI.ViewModels;
using UserControlAV = Avalonia.Controls.UserControl;

namespace ActivityRecorderClientAV
{
	public enum TaskDisplayMode
	{
		Active,
		Inactive,
	}

	public partial class TaskDisplayControlAV : UserControlAV
	{
		// Define Styled Properties for all bindable elements
		public static readonly StyledProperty<SvgImage?> IconSourceProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, SvgImage?>(nameof(IconSource));

		public static readonly StyledProperty<IBrush> TaskCircleFillProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, IBrush>(nameof(TaskCircleFill), Brushes.Gray);

		public static readonly StyledProperty<string?> TaskInitialsTextProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, string?>(nameof(TaskInitialsText));

		public static readonly StyledProperty<string?> TaskFolderTextProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, string?>(nameof(TaskFolderText));

		public static readonly StyledProperty<string?> TaskNameTextProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, string?>(nameof(TaskNameText));

		public static readonly StyledProperty<string?> PriorityTextProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, string?>(nameof(PriorityText));

		public static readonly StyledProperty<string?> DaysLeftTextProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, string?>(nameof(DaysLeftText));

		public static readonly StyledProperty<double> ProgressDaysValueProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, double>(nameof(ProgressDaysValue));

		public static readonly StyledProperty<IBrush> ProgressDaysForegroundProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, IBrush>(nameof(ProgressDaysForeground), Brushes.Blue);

		public static readonly StyledProperty<string?> HoursLeftTextProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, string?>(nameof(HoursLeftText));

		public static readonly StyledProperty<double> ProgressHoursValueProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, double>(nameof(ProgressHoursValue));

		public static readonly StyledProperty<IBrush> ProgressHoursForegroundProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, IBrush>(nameof(ProgressHoursForeground), Brushes.Blue);

		public static readonly StyledProperty<TaskDisplayMode> DisplayModeProperty =
			AvaloniaProperty.Register<TaskDisplayControlAV, TaskDisplayMode>(nameof(DisplayMode));

		// Public properties wrapping the Styled Properties
		public SvgImage? IconSource
		{
			get => GetValue(IconSourceProperty);
			set => SetValue(IconSourceProperty, value);
		}

		public IBrush TaskCircleFill
		{
			get => GetValue(TaskCircleFillProperty);
			set => SetValue(TaskCircleFillProperty, value);
		}

		public string? TaskInitialsText
		{
			get => GetValue(TaskInitialsTextProperty);
			set => SetValue(TaskInitialsTextProperty, value);
		}

		public string? TaskFolderText
		{
			get => GetValue(TaskFolderTextProperty);
			set => SetValue(TaskFolderTextProperty, value);
		}

		public string? TaskNameText
		{
			get => GetValue(TaskNameTextProperty);
			set => SetValue(TaskNameTextProperty, value);
		}

		public string? PriorityText
		{
			get => GetValue(PriorityTextProperty);
			set => SetValue(PriorityTextProperty, value);
		}

		public string? DaysLeftText
		{
			get => GetValue(DaysLeftTextProperty);
			set => SetValue(DaysLeftTextProperty, value);
		}

		public double ProgressDaysValue
		{
			get => GetValue(ProgressDaysValueProperty);
			set => SetValue(ProgressDaysValueProperty, value);
		}

		public IBrush ProgressDaysForeground
		{
			get => GetValue(ProgressDaysForegroundProperty);
			set => SetValue(ProgressDaysForegroundProperty, value);
		}

		public string? HoursLeftText
		{
			get => GetValue(HoursLeftTextProperty);
			set => SetValue(HoursLeftTextProperty, value);
		}

		public double ProgressHoursValue
		{
			get => GetValue(ProgressHoursValueProperty);
			set => SetValue(ProgressHoursValueProperty, value);
		}

		public IBrush ProgressHoursForeground
		{
			get => GetValue(ProgressHoursForegroundProperty);
			set => SetValue(ProgressHoursForegroundProperty, value);
		}

		public TaskDisplayMode DisplayMode
		{
			get => GetValue(DisplayModeProperty);
			set => SetValue(DisplayModeProperty, value);
		}

		public TaskDisplayControlAV()
		{
			InitializeComponent();
		}


		private TaskViewModel? lastTaskViewModel;
		protected override void OnDataContextChanged(EventArgs e)
		{
			if (lastTaskViewModel != null)
			{
				lastTaskViewModel.PropertyChanged -= TaskViewModel_PropertyChanged;
			}

			base.OnDataContextChanged(e);

			lastTaskViewModel = DataContext as TaskViewModel;

			UpdateValues(lastTaskViewModel);

			if (lastTaskViewModel != null)
			{
				lastTaskViewModel.PropertyChanged += TaskViewModel_PropertyChanged;
			}
		}

		private void TaskViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateValues(lastTaskViewModel);
		}

		private void UpdateValues(TaskViewModel? item)
		{
			if (item != null)
			{
				var circleColor = DisplayMode == TaskDisplayMode.Active ? (item.CircleColor ?? "Gray") : "Gray";
				TaskCircleFill = new SolidColorBrush(Color.Parse(circleColor));
				TaskInitialsText = item.Initials;
				TaskFolderText = item.FolderPath;
				TaskNameText = item.Name;
				PriorityText = item.Priority;
				DaysLeftText = item.DaysLeftInfo;
				ProgressDaysValue = item.DaysProgress;
				ProgressDaysForeground = new SolidColorBrush(Color.Parse(item.DaysProgressColor ?? "#0078D7"));
				HoursLeftText = item.HoursLeftInfo;
				ProgressHoursValue = item.HoursProgress;
				ProgressHoursForeground = new SolidColorBrush(Color.Parse(item.HoursProgressColor ?? "#0078D7"));
			}
			else
			{
				IconSource = null;
				TaskCircleFill = Brushes.Transparent;
				TaskInitialsText = string.Empty;
				TaskFolderText = string.Empty;
				TaskNameText = string.Empty;
				PriorityText = string.Empty;
				DaysLeftText = string.Empty;
				ProgressDaysValue = 0;
				ProgressDaysForeground = Brushes.Transparent;
				HoursLeftText = string.Empty;
				ProgressHoursValue = 0;
				ProgressHoursForeground = Brushes.Transparent;
			}
		}
	}
}