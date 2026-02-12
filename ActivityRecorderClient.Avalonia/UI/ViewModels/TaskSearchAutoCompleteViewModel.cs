using ActivityRecorderClient.Avalonia.ViewModels;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class TaskSearchAutoCompleteViewModel : ViewModelBase
	{
		public ObservableCollection<TaskFlattenedViewModel> AllItems { get; } = new();

		[ObservableProperty]
		private TaskFlattenedViewModel? selectedItem;

		public void MenuChanged(ClientMenu menu)
		{
			AllItems.Clear();
			foreach (var item in TaskFlattenedViewModel.CreateListFrom(menu, null))
			{
				AllItems.Add(item);
			}
		}
	}
}
