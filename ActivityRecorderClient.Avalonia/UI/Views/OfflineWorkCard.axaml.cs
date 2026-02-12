using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Avalonia.UI.ViewModels;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Avalonia.UI.Views;

public partial class OfflineWorkCard : UserControl
{
	public MeetingInfo MeetingInfo { get => DataContext as MeetingInfo; set => DataContext = value; }
	public Func<WorkData, bool> CanSelectWork { get; set; }
	public Func<ClientMenuLookup, IEnumerable<int>> RecentWorkIdsSelector { get; set; }
	public event EventHandler CardClicked;
	public event EventHandler Validation;
	public event EventHandler<SingleValueEventArgs<object>> WorkSelectionChanged;
	public event EventHandler<SingleValueEventArgs<bool>> CardExpandStateChanged;
	public event EventHandler<SingleValueEventArgs<bool>> CardDeletedStateChanged;
	public event EventHandler DataChanged;
	public event EventHandler AddressbookSelected;

	public OfflineWorkCard()
	{
		InitializeComponent();
	}

	private void TrashButton_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		CardDeletedStateChanged?.Invoke(this, new SingleValueEventArgs<bool>(true));
	}

	private void UndoButton_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		CardDeletedStateChanged?.Invoke(this, new SingleValueEventArgs<bool>(false));
	}

	private void HandleClicked(object sender, EventArgs e)
	{
		CardClicked?.Invoke(this, EventArgs.Empty);
	}

	private TaskSearchViewModel previousViewModel = null;
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		cbTask.ViewModel = new TaskSearchViewModel();
		cbTask.ViewModel.MenuChanged(MeetingInfo.ClientMenuLookup.ClientMenu);
		cbTask.ViewModel.PropertyChanged += OnPropertyChanged;
		cbTask.ViewModel.CanSelectWork = CanSelectWork;

		if (previousViewModel != null)
		{
			previousViewModel.PropertyChanged -= OnPropertyChanged;
		}
		previousViewModel = cbTask.ViewModel;
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(TaskSearchViewModel.SelectedItem) && cbTask.ViewModel.SelectedItem != null)
		{
			var workDataWithParentNames = MeetingInfo.ClientMenuLookup.GetWorkDataWithParentNames(cbTask.ViewModel.SelectedItem.WorkId);
			WorkSelectionChanged?.Invoke(this, SingleValueEventArgs.Create<object>(workDataWithParentNames));
			DataChanged?.Invoke(this, EventArgs.Empty);
			Validation?.Invoke(this, EventArgs.Empty);
		}
	}

	public void OpenTaskDropdown()
	{
		cbTask.SearchCombo.IsDropDownOpen = true;
	}

}

public class DesignMeetingInfo : MeetingInfo
{
	public DesignMeetingInfo()
	{
		DurationText = Tuple.Create("02:32", "02:32:54");
		StartTimeText = Tuple.Create("13:53", "");
		EndTimeText = Tuple.Create("13:56", "");
		Subject = "Some subject";
		Comment = "This is a comment.";
		Participants = "xyz@tct.hu";
		NavigationWork = new ActivityRecorderClient.View.Navigation.NavigationWork(null,
			new WorkDataWithParentNames() { ParentNames = new() { "XY", "ZW" }, WorkData = new WorkData() { Id = 1, Name = "Task Name" } });
		ClientMenuLookup = new ClientMenuLookup() { ClientMenu = new ClientMenu() };
	}
}