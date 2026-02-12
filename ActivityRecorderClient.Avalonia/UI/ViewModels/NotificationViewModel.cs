using ActivityRecorderClient.Avalonia.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class NotificationViewModel : ViewModelBase
	{
		[ObservableProperty]
		private string title = "";

		[ObservableProperty]
		private string message = "";

		[ObservableProperty]
		private string? barColor;

		[ObservableProperty]
		private NotificationPosition position;

		[ObservableProperty]
		private TimeSpan showDuration;

		[ObservableProperty]
		private Action? notificationClicked;
	}
}
