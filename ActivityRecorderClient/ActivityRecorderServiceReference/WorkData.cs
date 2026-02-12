using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class WorkData
	{
		public bool IsWorkIdFromServer { get { return Id.HasValue && MenuCoordinator.IsWorkIdFromServer(Id.Value); } }
		public string WorkKey { get; set; }
		public string ProjectKey { get; set; }
		public AssignData AssignData { get; set; }

		public bool IsVisibleInMenu
		{
			get { return (Visibility & WorkDataVisibilityType.HideInMenu) == 0; }
			set
			{
				if (!value) Visibility |= WorkDataVisibilityType.HideInMenu;
				else Visibility &= ~WorkDataVisibilityType.HideInMenu;
			}
		}

		public bool IsVisibleInRules
		{
			get { return (Visibility & WorkDataVisibilityType.HideInRules) == 0; }
			set
			{
				if (!value) Visibility |= WorkDataVisibilityType.HideInRules;
				else Visibility &= ~WorkDataVisibilityType.HideInRules;
			}
		}
		public bool IsVisibleInAdhocMeeting
		{
			get { return (Visibility & WorkDataVisibilityType.HideInAdhoc) == 0; }
			set
			{
				if (!value) Visibility |= WorkDataVisibilityType.HideInAdhoc;
				else Visibility &= ~WorkDataVisibilityType.HideInAdhoc;
			}
		}

		public bool IsVisibleInAdhocMeetingOnly
		{
			get { return IsVisibleInAdhocMeeting && !IsVisibleInMenu && !IsVisibleInRules; }
		}

		public WorkDataVisibilityType Visibility
		{
			get { return (WorkDataVisibilityType)VisibilityType.GetValueOrDefault(0); }
			set { VisibilityType = (int)value; }
		}

		[Flags]
		public enum WorkDataVisibilityType
		{
			None = 0,				//Visible everywhere (old functionality)
			HideInMenu = 1 << 0,	//don't show work in menu
			HideInRules = 1 << 1,	//don't show work for rules
			HideInAdhoc = 1 << 2,	//don't show work for adhoc/idle meeting
		}

		public WorkData Clone()
		{
			return (WorkData)MemberwiseClone();
		}
	}
}
