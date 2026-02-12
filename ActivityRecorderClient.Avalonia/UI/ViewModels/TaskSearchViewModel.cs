using ActivityRecorderClient.Avalonia.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class TaskSearchViewModel : ViewModelBase
	{
		public ObservableCollection<TaskFlattenedViewModel> AllItems { get; } = new();

		public ObservableCollection<TaskFlattenedViewModel> FilteredItems { get; } = new();

		[ObservableProperty]
		private TaskFlattenedViewModel? selectedItem;

		[ObservableProperty]
		private string searchText = "";

		[ObservableProperty]
		private Func<WorkData, bool>? canSelectWork;

		private ClientMenu? clientMenu;

		partial void OnSearchTextChanged(string value)
		{
			ApplyFilter(value);
		}

		partial void OnCanSelectWorkChanged(Func<WorkData, bool>? value)
		{
			if (clientMenu != null)
			{
				MenuChanged(clientMenu);
			}
		}

		public void ApplyFilter(string? searchText)
		{
			var selectedItem = SelectedItem;
			SearchText = (searchText ?? "").Trim();
			FilteredItems.Clear();
			foreach (var item in AllItems.Where(item => item.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
			{
				FilteredItems.Add(item);
			}
			if (selectedItem != null)
			{
				SelectedItem = FilteredItems.FirstOrDefault(item => item.WorkId == selectedItem.WorkId);
			}
		}

		public void MenuChanged(ClientMenu menu)
		{
			clientMenu = menu;
			AllItems.Clear();
			foreach (var item in TaskFlattenedViewModel.CreateListFrom(menu, CanSelectWork))
			{
				AllItems.Add(item);
			}
			ApplyFilter(SearchText);
		}
	}

	public class DesignTaskSearchViewModel : TaskSearchViewModel
	{
		public DesignTaskSearchViewModel()
		{
			AllItems.Add(new TaskFlattenedViewModel { FullName = "TCT » TCT - PM » General - Meeting", Name = "General - Meeting", WorkId = 1 });
			AllItems.Add(new TaskFlattenedViewModel { FullName = "TCT » JobCTRL - DEV » Client dev", Name = "Client dev", WorkId = 2 });
			ApplyFilter("");
		}
	}

	public class DesignTaskSearchViewModelWithSelected : TaskSearchViewModel
	{
		public DesignTaskSearchViewModelWithSelected()
		{
			AllItems.Add(new TaskFlattenedViewModel { FullName = "TCT » TCT - PM » General - Meeting", Name = "General - Meeting", WorkId = 1 });
			AllItems.Add(new TaskFlattenedViewModel { FullName = "TCT » JobCTRL - DEV » Client dev", Name = "Client dev", WorkId = 2 });
			ApplyFilter("");
			SelectedItem = AllItems[0];
		}
	}
}
