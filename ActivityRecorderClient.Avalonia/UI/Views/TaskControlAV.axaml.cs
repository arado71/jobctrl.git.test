using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using System;
using System.Diagnostics; // Added: For Ellipse
using Tct.ActivityRecorderClient.Avalonia.UI.ViewModels;
using UserControlAV = Avalonia.Controls.UserControl;

namespace ActivityRecorderClientAV
{
    // Enum for Display Modes
    public enum DisplayModes
    {
        BoxTask,
        BrowserFolder,
        BrowserTask
    }

    public partial class TaskControlAV : UserControlAV
    {

        public static readonly StyledProperty<DisplayModes> DisplayModeProperty = AvaloniaProperty.Register<TaskControlAV,
            DisplayModes>(nameof(DisplayMode),DisplayModes.BoxTask // optional: default value
    );

    public DisplayModes DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }


        /*
        public static readonly StyledProperty<TaskDisplayMode> DisplayModeProperty =
            AvaloniaProperty.Register<TaskDisplayControlAV, TaskDisplayMode>(nameof(DisplayMode));

        public TaskDisplayMode DisplayMode
        {
            get => GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }
        */

        public TaskControlAV()
        {
            InitializeComponent();
        }

        private void OnTaskItemClicked(object? sender, PointerPressedEventArgs e)
        {
			Debug.WriteLine($"Clicked ----");
			if (DataContext is TaskViewModel taskItem) Debug.WriteLine($"Clicked on: {taskItem.Name}");
        }
    }
    
}