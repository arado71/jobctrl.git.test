using ActivityRecorderClient.Avalonia.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class TaskViewModel : ViewModelBase
	{
		[ObservableProperty]
		private int? workId;

		[ObservableProperty]
		private int? projectId;

		[ObservableProperty]
		private string name = "";

		[ObservableProperty]
		private string initials = "";

		[ObservableProperty]
		private string? circleColor;

		[ObservableProperty]
		private string folderPath = "";

		[ObservableProperty]
		private string? priority;

		[ObservableProperty]
		private string? daysLeftInfo;

		[ObservableProperty]
		private double daysProgress;

		[ObservableProperty]
		private string? daysProgressColor;

		[ObservableProperty]
		private string? hoursLeftInfo;

		[ObservableProperty]
		private double hoursProgress;

		[ObservableProperty]
		private string? hoursProgressColor;

		public ObservableCollection<TaskViewModel> Children { get; set; } = [];

		[ObservableProperty]
		private TaskViewModel? parent;

		[ObservableProperty]
		private string? iconPath;

		public TaskViewModel()
		{
			Children.CollectionChanged += OnChildrenChanged;
		}

		private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems?.Count > 0)
			{
				foreach (TaskViewModel child in e.NewItems)
				{
					child.Parent = this;
				}
			}

			if (e.OldItems?.Count > 0)
			{
				foreach (TaskViewModel child in e.OldItems)
				{
					if (child.Parent == this)
					{
						child.Parent = null;
					}
				}
			}
		}
	}
}
