using ActivityRecorderClientAV;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using log4net;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Meeting.Adhoc;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View;
using Tct.ActivityRecorderClient.View.Presenters;

namespace Tct.ActivityRecorderClient.Avalonia.UI.Views;

public partial class OfflineWorkForm : BaseWindow, IOfflineWorkView
{
	private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	public OfflineWorkPresenter Presenter { get; private set; }
	public DialogResult DialogResult { get; private set; } = DialogResult.None;

	public bool IsRecordActionAvailable
	{
		get => AddButton.IsEnabled;
		set => AddButton.IsEnabled = value;
	}

	public bool IsSplitButtonEnabled { get => false; set { } }
	public bool? IsMergeButtonEnabled { get => false; set { } }
	public bool IsIntervalSplitterEnabled { get => false; set { } }

	public OfflineWorkForm()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			OfflineWorkCardContainer.Children.Add(new OfflineWorkCard() { MeetingInfo = new DesignMeetingInfo() });
			OfflineWorkCardContainer.Children.Add(new OfflineWorkCard() { MeetingInfo = new DesignMeetingInfo() });
			OfflineWorkCardContainer.Children.Add(new OfflineWorkCard() { MeetingInfo = new DesignMeetingInfo() });
		}
	}

	public OfflineWorkForm(AdhocMeetingService adhocMeetingService) : this()
	{
		Presenter = new OfflineWorkPresenter(this, adhocMeetingService, Platform.Factory.GetAddressBookService());
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

	public void RunOnGui(Action action)
	{
		Dispatcher.UIThread.Post(action);
	}

	public void SetAlternativeMenu(Action<WorkDataEventArgs> click, string caption)
	{
		// TODO: mac
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

	private int splitterCount = 0;
	public int AddInterval(DateTime start, CardStyle type, bool isDraggable = false)
	{
		// TODO mac: splitter
		return ++splitterCount;
	}

	public int InsertInterval(int index, DateTime start, CardStyle type, bool isDraggable)
	{
		// TODO mac: splitter
		return index;
	}

	public void ModifyIntervalColor(int index, CardStyle type)
	{
		// TODO mac: splitter
	}

	public void ModifyIntervalEnd(int index, bool isDraggable)
	{
		// TODO mac: splitter
	}

	public void ModifyIntervalTime(int index, DateTime time)
	{
		// TODO mac: splitter
	}

	public void RemoveInterval(int index)
	{
		// TODO mac: splitter
		--splitterCount;
	}


	public void AddMeetingCard(MeetingInfo info, Guid after)
	{
		DeleteButton.Focus();
		var card = new OfflineWorkCard();
		card.MeetingInfo = info;
		card.CanSelectWork = Presenter.CanSelectWork;
		card.RecentWorkIdsSelector = Presenter.RecentWorkIdsSelector;
		if (after == Guid.Empty)
		{
			OfflineWorkCardContainer.Children.Add(card);
			OfflineWorkCardScrollViewer.ScrollToEnd();
		}
		else
		{
			var idx = OfflineWorkCardContainer.Children.OfType<OfflineWorkCard>().ToList().FindIndex(n => n.MeetingInfo.Id == after);
			OfflineWorkCardContainer.Children.Insert(idx + 1, card);
		}
		card.CardClicked += card_Clicked;
		card.Validation += card_Validation;
		card.WorkSelectionChanged += card_WorkSelectionChanged;
		card.CardExpandStateChanged += card_CardExpandStateChanged;
		card.CardDeletedStateChanged += card_CardDeletedStateChanged;
		card.DataChanged += card_DataChanged;
		card.AddressbookSelected += card_AddressbookSelected;
		card.SizeChanged += card_SizeChanged;
	}

	public void DeleteMeetingCard(Guid id)
	{
		var found = OfflineWorkCardContainer.Children.OfType<OfflineWorkCard>().FirstOrDefault(ctrl => ctrl.MeetingInfo.Id == id);
		if (found != null)
		{
			OfflineWorkCardContainer.Children.Remove(found);
			// TODO mac, maybe stop leaking?
		}
	}

	public void DropdownTaskList(Guid id)
	{
		var found = OfflineWorkCardContainer.Children.OfType<OfflineWorkCard>().FirstOrDefault(ctrl => ctrl.MeetingInfo.Id == id);
		if (found != null)
		{
			found.OpenTaskDropdown();
		}
	}

	public void HandleUserActivity(bool isMouseActivity)
	{
		log.DebugFormat("OnAutoReturnFromMeeting (mouseact: {0})", isMouseActivity);
		// TODO: mac
		//if (Bounds.Contains(Cursor.Position)) return;
		//if (!isMouseActivity && formIsActive) return;
		//if (owner.IsExcludedFromObserving(Cursor.Position)) return;
		Presenter.AutoReturnFromMeeting();
	}

	public bool ShowCloseConfirmationDialog()
	{
		var result = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
		{
			ContentTitle = Labels.AddMeeting_AbortConfirmationTitle,
			ContentMessage = Labels.AddMeeting_AbortConfirmation,
			Topmost = true,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			Icon = MsBox.Avalonia.Enums.Icon.Info,
			ButtonDefinitions = new List<ButtonDefinition>
								{
									new ButtonDefinition { Name = "OK", },
									new ButtonDefinition { Name = "Cancel", },
								},
		}).ShowWindowDialog(this);
		return result == "OK";
	}

	public void UpdateStopWatch(bool visible)
	{
		TimerAnimation.IsVisible = visible;
	}

	public void UpdateTotal(string totalSumText)
	{
		BigTimerText.Text = totalSumText;
	}

	private void OnAddClick(object sender, RoutedEventArgs? e)
	{
		DialogResult = DialogResult.OK;
		Close();
	}

	private void OnDeleteClick(object sender, RoutedEventArgs? e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private OfflineWindowCloseReason Map(DialogResult result)
	{
		switch (result)
		{
			case DialogResult.None:
				return OfflineWindowCloseReason.QueryShutdown;
			case DialogResult.OK:
				return OfflineWindowCloseReason.SubmitWorks;
			case DialogResult.Cancel:
				return OfflineWindowCloseReason.CancelWorks;
			case DialogResult.Abort:
				return OfflineWindowCloseReason.RequestStop;
			default:
				throw new ArgumentOutOfRangeException(nameof(result), result, null);
		}
	}

	private void Window_Closing(object? sender, WindowClosingEventArgs e)
	{
		e.Cancel = !Presenter.ConfirmClosing(Map(DialogResult), App.IsShuttingDown);
	}

	private void Window_Closed(object? sender, EventArgs e)
	{
		Presenter.ViewClosed(!App.IsShuttingDown && DialogResult == DialogResult.None ? OfflineWindowCloseReason.CancelWorks : Map(DialogResult));
	}

	///////////////////////////////////////////////

	void card_Clicked(object sender, EventArgs e)
	{
		var card = sender as OfflineWorkCard;
		Debug.Assert(card != null);
		Presenter.CardClicked(card.MeetingInfo);
	}

	void card_CardExpandStateChanged(object sender, SingleValueEventArgs<bool> e)
	{
		if (!(sender is OfflineWorkCard card)) return;
		// TODO mac, do we need this?
	}

	void card_WorkSelectionChanged(object sender, SingleValueEventArgs<object> e)
	{
		if (!(sender is OfflineWorkCard card)) return;
		Presenter.WorkSelectionChanged(card.MeetingInfo, e.Value);
	}

	void card_Validation(object sender, EventArgs e)
	{
		if (!(sender is OfflineWorkCard card)) return;
		Presenter.ValidateInput(card.MeetingInfo);
	}

	private void card_CardDeletedStateChanged(object sender, SingleValueEventArgs<bool> e)
	{
		if (!(sender is OfflineWorkCard card)) return;
		Presenter.DeleteCard(card.MeetingInfo, e.Value);
	}

	private void card_DataChanged(object sender, EventArgs e)
	{
		if (!(sender is OfflineWorkCard card)) return;
		Presenter.MeetingInfoChanged(card.MeetingInfo);
	}

	private void card_AddressbookSelected(object sender, EventArgs e)
	{
		if (!(sender is OfflineWorkCard card)) return;
		// TODO: mac
		//Presenter.AddressbookSelected(card.MeetingInfo, Handle);
	}

	private void card_SizeChanged(object sender, EventArgs e)
	{
		// TODO mac, do we need this?
	}
}