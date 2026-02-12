using Avalonia.Controls;
using Avalonia.Svg.Skia;
using System.Collections.ObjectModel;
using System.Reflection;
using Avalonia.Input;
using System.Diagnostics;
using UserControlAV = Avalonia.Controls.UserControl;
using Avalonia.Media; // Added for Color and Brush
using System.Collections.Generic; // Added for EqualityComparer
using System.Linq; // Added for Any() and First()

namespace ActivityRecorderClientAV
{
    public class TaskItemAV
    {
        public string? Name { get; set; }
        public string? IconPath { get; set; }

        // Task-specific properties
        public string? Initials { get; set; }
        public string? CircleColor { get; set; }
        public string? FolderPath { get; set; }
        public string? Priority { get; set; }
        public string? DaysLeftInfo { get; set; }
        public double DaysProgress { get; set; }
        public string? HoursLeftInfo { get; set; }
        public double HoursProgress { get; set; }
        public string? ProgressBarColor { get; set; }

        public TaskItemAV(string name, string? iconPath = null)
        {
            Name = name;
            IconPath = iconPath;
        }

        // Constructor for a full task item
        public TaskItemAV(string name, string initials, string circleColor, string folderPath, string priority,
                          string daysLeftInfo, double daysProgress, string hoursLeftInfo, double hoursProgress,
                          string progressBarColor)
        {
            Name = name;
            Initials = initials;
            CircleColor = circleColor;
            FolderPath = folderPath;
            Priority = priority;
            DaysLeftInfo = daysLeftInfo;
            DaysProgress = daysProgress;
            HoursLeftInfo = hoursLeftInfo;
            HoursProgress = hoursProgress;
            ProgressBarColor = progressBarColor;
            IconPath = null; // Tasks don't use IconPath in this context
        }

        public TaskItemAV()
        {
            // Default constructor
        }
    }

    public class TaskFolderViewModelAV
    {
        public ObservableCollection<TaskItemAV> TaskItems { get; set; }
        public TaskItemAV? SelectedItem { get; set; }

        public TaskFolderViewModelAV()
        {
            TaskItems = new ObservableCollection<TaskItemAV>();
        }

        public void AddItem(TaskItemAV item)
        {
            TaskItems.Add(item);
            Debug.WriteLine($"Added item: {item.Name}. Total items: {TaskItems.Count}");
        }

        public void InsertItem(int index, TaskItemAV item)
        {
            if (index >= 0 && index <= TaskItems.Count)
            {
                TaskItems.Insert(index, item);
                Debug.WriteLine($"Inserted item: {item.Name} at index {index}. Total items: {TaskItems.Count}");
            }
            else
            {
                Debug.WriteLine($"Error: Cannot insert item at invalid index {index}. List has {TaskItems.Count} items.");
            }
        }

        public void DeleteList()
        {
            TaskItems.Clear();
            SelectedItem = null;
            Debug.WriteLine("All items cleared from the list.");
        }

        public void FillWithDemoData()
        {
            DeleteList(); // Clear existing items first

            // Add some demo FOLDER items
            AddItem(new TaskItemAV("Recent Tasks", AppResourcesAV.BaseIconPath + "recent_tasks.svg"));
            AddItem(new TaskItemAV("Recent Projects", AppResourcesAV.BaseIconPath + "recent_projects.svg"));
            AddItem(new TaskItemAV("Deadline", AppResourcesAV.BaseIconPath + "deadline.svg"));
            AddItem(new TaskItemAV("Priority", AppResourcesAV.BaseIconPath + "priority.svg"));
            AddItem(new TaskItemAV("All Tasks", AppResourcesAV.BaseIconPath + "all_tasks.svg"));

            
            // Add some demo TASK items with full details
            var demoTask1 = new TaskItemAV(
                name: "Cross-Platform UI Design",
                initials: "Cr",
                circleColor: "Orange",
                folderPath: "TCT » TCT - PM »",
                priority: "500",
                daysLeftInfo: "6 days left / 29 days",
                daysProgress: 79,
                hoursLeftInfo: "100:26 / 168:00",
                hoursProgress: 59,
                progressBarColor: "#0078D7"
            );
            AddItem(demoTask1);

            var demoTask2 = new TaskItemAV(
                name: "Implement Data Storage",
                initials: "DS",
                circleColor: "Blue",
                folderPath: "Backend » Database »",
                priority: "450",
                daysLeftInfo: "10 days left / 15 days",
                daysProgress: 50,
                hoursLeftInfo: "40:00 / 80:00",
                hoursProgress: 50,
                progressBarColor: "#FFC300" // Yellow
            );
            AddItem(demoTask2);

            var demoTask3 = new TaskItemAV(
                name: "Review Code Submissions",
                initials: "RC",
                circleColor: "Green",
                folderPath: "DevOps » CI/CD »",
                priority: "600",
                daysLeftInfo: "2 days left / 5 days",
                daysProgress: 80,
                hoursLeftInfo: "10:00 / 12:00",
                hoursProgress: 83,
                progressBarColor: "#4CAF50" // Green
            );
            AddItem(demoTask3);

            var demoTask4 = new TaskItemAV(
                name: "Review Code Submissions 2",
                initials: "RC",
                circleColor: "Green",
                folderPath: "DevOps » CI/CD »",
                priority: "600",
                daysLeftInfo: "2 days left / 5 days",
                daysProgress: 80,
                hoursLeftInfo: "10:00 / 12:00",
                hoursProgress: 83,
                progressBarColor: "#4CAF50" // Green
            );
            AddItem(demoTask4);

            var demoTask5 = new TaskItemAV(
                name: "Review Code Submissions 3",
                initials: "RC",
                circleColor: "Green",
                folderPath: "DevOps » CI/CD »",
                priority: "600",
                daysLeftInfo: "2 days left / 5 days",
                daysProgress: 80,
                hoursLeftInfo: "10:00 / 12:00",
                hoursProgress: 83,
                progressBarColor: "#4CAF50" // Green
            );
            AddItem(demoTask5);

            var demoTask6 = new TaskItemAV(
                name: "Review Code Submissions 4",
                initials: "RC",
                circleColor: "Green",
                folderPath: "DevOps » CI/CD »",
                priority: "600",
                daysLeftInfo: "2 days left / 5 days",
                daysProgress: 80,
                hoursLeftInfo: "10:00 / 12:00",
                hoursProgress: 83,
                progressBarColor: "#4CAF50" // Green
            );
            AddItem(demoTask6);

            var demoTask7 = new TaskItemAV(
                name: "Review Code Submissions 5",
                initials: "RC",
                circleColor: "Green",
                folderPath: "DevOps » CI/CD »",
                priority: "600",
                daysLeftInfo: "2 days left / 5 days",
                daysProgress: 80,
                hoursLeftInfo: "10:00 / 12:00",
                hoursProgress: 83,
                progressBarColor: "#4CAF50" // Green
            );
            AddItem(demoTask7);
            

            // Set the *first demo task* as the initial SelectedItem for the Task Box
            // so it's not empty on startup.
            // SelectedItem = demoTask1; // Or any specific task item you want to show initially.

            Debug.WriteLine("Filled list with demo folders and tasks.");
        }
    }

    public partial class TaskItemControlAV : UserControlAV
    {
        public TaskItemControlAV()
        {
            // InitializeComponent();
        }

		private void OnTaskItemClicked(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is TaskItemAV taskItem) Debug.WriteLine($"Clicked on: {taskItem.Name}");
        }

        public void UpdateIcon(string newIconPath)
        {
            // This method is from your old TaskItemControlAV.
            // If you are fully transitioning to TaskDisplayControlAV, this method might become obsolete
            // or need to be adapted if TaskItemControlAV is still used somewhere else for icons.
            // For TaskDisplayControlAV, the icon is set directly via SvgSource.Load.
        }
    }
}