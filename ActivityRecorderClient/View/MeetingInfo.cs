using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.View
{
	public class MeetingInfo : INotifyPropertyChanged
	{
		private Guid id;
		protected OfflineWorkType offlineWorkType;
		protected CardStyle cardStyle;
		protected Tuple<string, string> durationText; // item1: short format, item2: long format (for tooltip/hint)
		protected Tuple<string, string> startTimeText, endTimeText; // item1: short format, item2: long format (for tooltip/hint)
		protected NavigationWork navigationWork;
		protected string subject;
		protected string comment;
		protected string participants;
		protected ClientMenuLookup clientMenuLookup;
		protected bool isTaskSelectionInvalid, isSubjectLengthInvalid, isDescriptionLengthInvalid, isEmailsLengthInvalid, isEmailFormatError, isDurationInvalid, isDurationExceedLimit, isAddressbookVisible, isWaitCursorOnAddressbook, hasBadge;
		public Guid Id { get => id; set { id = value; OnPropertyChanged(); } }
		public OfflineWorkType OfflineWorkType { get => offlineWorkType; set { offlineWorkType = value; OnPropertyChanged(); } }
		public CardStyle CardStyle { get => cardStyle; set { cardStyle = value; OnPropertyChanged(); } }
		public Tuple<string, string> DurationText { get => durationText; set { durationText = value; OnPropertyChanged(); } }
		public Tuple<string, string> StartTimeText { get => startTimeText; set { startTimeText = value; OnPropertyChanged(); } }
		public Tuple<string, string> EndTimeText { get => endTimeText; set { endTimeText = value; OnPropertyChanged(); } }
		public NavigationWork NavigationWork { get => navigationWork; set { navigationWork = value; OnPropertyChanged(); } }
		public string Subject { get => subject; set { subject = value; OnPropertyChanged(); } }
		public string Comment { get => comment; set { comment = value; OnPropertyChanged(); } }
		public string Participants { get => participants; set { participants = value; OnPropertyChanged(); } }
		public ClientMenuLookup ClientMenuLookup { get => clientMenuLookup; set { clientMenuLookup = value; OnPropertyChanged(); } }
		public bool IsTaskSelectionInvalid { get => isTaskSelectionInvalid; set { isTaskSelectionInvalid = value; OnPropertyChanged(); } }
		public bool IsSubjectLengthInvalid { get => isSubjectLengthInvalid; set { isSubjectLengthInvalid = value; OnPropertyChanged(); } }
		public bool IsDescriptionLengthInvalid { get => isDescriptionLengthInvalid; set { isDescriptionLengthInvalid = value; OnPropertyChanged(); } }
		public bool IsEmailsLengthInvalid { get => isEmailsLengthInvalid; set { isEmailsLengthInvalid = value; OnPropertyChanged(); } }
		public bool IsEmailFormatError { get => isEmailFormatError; set { isEmailFormatError = value; OnPropertyChanged(); } }
		public bool IsDurationInvalid { get => isDurationInvalid; set { isDurationInvalid = value; OnPropertyChanged(); } }
		public bool IsDurationExceedLimit { get => isDurationExceedLimit; set { isDurationExceedLimit = value; OnPropertyChanged(); } }
		public bool IsAddressbookVisible { get => isAddressbookVisible; set { isAddressbookVisible = value; OnPropertyChanged(); } }
		public bool IsWaitCursorOnAddressbook { get => isWaitCursorOnAddressbook; set { isWaitCursorOnAddressbook = value; OnPropertyChanged(); } }
		public bool HasBadge { get => hasBadge; set { hasBadge = value; OnPropertyChanged(); } }
		public bool IsAnyInvalid => isTaskSelectionInvalid || isSubjectLengthInvalid || isDescriptionLengthInvalid || isEmailsLengthInvalid 
		                                  || isEmailFormatError || isDurationInvalid || isDurationExceedLimit; 

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public enum OfflineWorkType
	{
		AfterInactivity,
		ManuallyStarted,
	}

	public enum CardStyle
	{
		Normal,
		Selected,
		Deleted,
		Incomplete,
		CannotAccountable,
		None,
	}
}
