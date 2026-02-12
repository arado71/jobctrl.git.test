using Avalonia.Controls;
using Avalonia.Svg.Skia;
using System.Collections.ObjectModel;
using System.Reflection;
using Avalonia.Input;
using System.Diagnostics;
using UserControlAV = Avalonia.Controls.UserControl;
using Avalonia.Media; // Added for Color and Brush
using System.Linq; // Added for Any() and First()

namespace ActivityRecorderClientAV
{
    public class TaskControlItemAV
    {
        public string? FolderName { get; set; }
        public string? TaskName { get; set; }


        public TaskControlItemAV(string foldername, string taskname)
        {
            FolderName = foldername;
            TaskName = taskname;

        }

        public TaskControlItemAV()
        {
            // Default constructor
        }
    }

    public class BrowserViewModelAV
    {
        public ObservableCollection<TaskControlItemAV> TaskItems { get; set; }
        public TaskItemAV? SelectedItem { get; set; }

        public BrowserViewModelAV()
        {
            // TaskItems = new ObservableCollection<TaskControlItemAV>();
            TaskItems = []; // simplified version of the line above
        }
    }

    // public partial class TaskControlAV : UserControlAV
    // {
    //     public TaskControlAV()
    //     {
    //         InitializeComponent();
    //     }

    //     private void OnTaskItemClicked(object? sender, PointerPressedEventArgs e)
    //     {
    //         if (DataContext is TaskControlItemAV taskItem) Debug.WriteLine($"Clicked on: {taskItem.TaskName}");
    //     }
    // }


}