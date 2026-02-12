using ActivityRecorderClient.Avalonia.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class OfflineWorkViewModel : ViewModelBase
	{
		[ObservableProperty]
		private TimeSpan duration;
		
		[ObservableProperty]
		private DateTime startDate;
		
		[ObservableProperty]
		private DateTime endDate;

		[ObservableProperty]
		private string? subject;

		[ObservableProperty]
		private string? description;

		[ObservableProperty]
		private string? participants;

		[ObservableProperty]
		private TaskSearchViewModel taskSearchViewModel;

	}
}
