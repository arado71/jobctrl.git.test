using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using Tct.ActivityRecorderClient.Avalonia.UI.ViewModels;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Meeting.Adhoc;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View;
using Tct.ActivityRecorderClient.View.Presenters;

namespace Tct.ActivityRecorderClient.Avalonia.UI.Views;

public partial class OfflineWorkWindow : BaseWindow, IOfflineWorkView
{
	public OfflineWorkViewModel ViewModel { get; private set; }
	private AdhocMeetingService AdhocMeetingService { get; set; }
	public OfflineWorkPresenter Presenter { get; private set; }
	public DialogResult DialogResult { get; private set; }

	public bool IsRecordActionAvailable
	{
		get => AddButton.IsEnabled;
		set => AddButton.IsEnabled = value;
	}

	public bool IsSplitButtonEnabled { get => false; set { } }
	public bool? IsMergeButtonEnabled { get => false; set { } }
	public bool IsIntervalSplitterEnabled { get => false; set { } }

	public OfflineWorkWindow()
	{
		InitializeComponent();

		ViewModel = new OfflineWorkViewModel() { TaskSearchViewModel = new TaskSearchViewModel() };
		this.DataContext = ViewModel;

		SearchCombo.GetPropertyChangedObservable(ComboBox.TextProperty)
			.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(args => SearchTextChanged(args.NewValue)));
		SearchCombo.GetPropertyChangedObservable(ComboBox.IsDropDownOpenProperty)
			.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(args => IsDropDownOpenChanged()));

	}

	public OfflineWorkWindow(AdhocMeetingService adhocMeetingService) : this()
	{
		AdhocMeetingService = adhocMeetingService;
		Presenter = new OfflineWorkPresenter(this, adhocMeetingService, Platform.Factory.GetAddressBookService());
	}

	private void IsDropDownOpenChanged()
	{
		if (SearchCombo.IsDropDownOpen && SearchCombo.SelectedItem != null)
		{
			// if we have an exact match assume we want to see all options
			ViewModel.TaskSearchViewModel.SearchText = "";
		}
	}

	private void SearchTextChanged(object? value)
	{
		// if we have an exact match assume we want to see all options
		if (SearchCombo.SelectedItem != null)
		{
			// TODO: mac, but this causes some UI glitches... hence clearing in IsDropDownOpenChanged
			//ViewModel.TaskSearchViewModel.SearchText = "";
			return;
		}

		var searchText = (string)value ?? "";
		ViewModel.TaskSearchViewModel.SearchText = searchText;
		if (!SearchCombo.IsDropDownOpen)
		{
			SearchCombo.IsDropDownOpen = true;
		}
	}

	public void RunOnGui(Action action)
	{
		Dispatcher.UIThread.Post(action);
	}

	public void SetAlternativeMenu(Action<WorkDataEventArgs> click, string caption)
	{
		// TODO: mac
	}

	public void ShowView()
	{
		this.Show();
	}

	public void ActivateView()
	{
		this.Show();
		this.Activate();
	}

	public void PopupView()
	{
		this.WindowState = WindowState.Normal;
		if (!this.IsVisible)
		{
			this.Show();
		}
	}

	public void AbortAndClose(bool isForce)
	{
		if (!isForce)
		{
			if (WindowState == WindowState.Minimized)
			{
				WindowState = WindowState.Normal;
			}
			DialogResult = DialogResult.Cancel;
		}
		else
		{
			DialogResult = DialogResult.Abort;
		}
		Close();
	}

	public void ShowMessageBox(string message, string title)
	{
		Dispatcher.UIThread.InvokeAsync(async () =>
		{
			await MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
			{
				ContentTitle = title,
				ContentMessage = message,
				Topmost = true,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
				Icon = MsBox.Avalonia.Enums.Icon.Info,
				ButtonDefinitions = new List<ButtonDefinition>
								{
									new ButtonDefinition { Name = "OK", },
								},
			}).ShowWindowDialogAsync(this);
		});
	}

	public void AddMeetingCard(MeetingInfo info, Guid after)
	{
		throw new NotImplementedException();
	}

	public void DeleteMeetingCard(Guid id)
	{
		throw new NotImplementedException();
	}

	public void UpdateTotal(string totalSumText)
	{
		throw new NotImplementedException();
	}

	public void HandleUserActivity(bool isMouseActivity)
	{
		throw new NotImplementedException();
	}

	public bool ShowCloseConfirmationDialog()
	{
		throw new NotImplementedException();
	}

	public void UpdateStopWatch(bool visible)
	{
		throw new NotImplementedException();
	}

	public int AddInterval(DateTime start, CardStyle type, bool isDraggable = false)
	{
		throw new NotImplementedException();
	}

	public void ModifyIntervalColor(int index, CardStyle type)
	{
		throw new NotImplementedException();
	}

	public void ModifyIntervalEnd(int index, bool isDraggable)
	{
		throw new NotImplementedException();
	}

	public void ModifyIntervalTime(int index, DateTime time)
	{
		throw new NotImplementedException();
	}

	public int InsertInterval(int index, DateTime start, CardStyle type, bool isDraggable)
	{
		throw new NotImplementedException();
	}

	public void RemoveInterval(int index)
	{
		throw new NotImplementedException();
	}

	public void DropdownTaskList(Guid id)
	{
		throw new NotImplementedException();
	}
}