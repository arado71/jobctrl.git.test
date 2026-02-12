using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.Stats;

namespace Tct.ActivityRecorderService.OnlineStats
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class BriefUserStats
	{
		[DataMember]
		public int UserId { get; set; }
		[DataMember]
		public string UserName { get; set; }
		[DataMember]
		public OnlineStatus Status { get; set; }
		[DataMember]
		public DateTime? TodaysStartDate { get; set; } //only if had some work today
		[DataMember]
		public DateTime? TodaysEndDate { get; set; } //only if had some work today and currently Offline
		[DataMember]
		public BriefNetWorkTimeStats TodaysWorkTime { get; set; } //calculated from WorkItems
		[DataMember]
		public BriefNetWorkTimeStats ThisWeeksWorkTime { get; set; } //calculated from AggregateWorkItemIntervals
		[DataMember]
		public BriefNetWorkTimeStats ThisMonthsWorkTime { get; set; } //calculated from AggregateWorkItemIntervals
		[DataMember]
		public List<WorkWithType> CurrentWorks { get; set; }
		[DataMember]
		public Dictionary<int, BriefWorkStats> TodaysWorksByWorkId { get; set; }
		[DataMember]
		public string UserTimeZoneString { get; set; }
		//public TimeZoneInfo UserTimeZone { get; set; } //string ? (quite big and needs some known types)
		//public TotalWorkTimeStats TotalWorkTime { get; set; } //??BriefWorkTimeStats BriefNetWorkTimeStats
		[DataMember(Order = 1)]
		public List<int> OnlineComputers { get; set; } //good to know if we want to kick somebody

		[DataMember(Order = 2)]
		public TimeSpan TodaysTargetNetWorkTime { get; set; }
		[DataMember(Order = 2)]
		public TimeSpan ThisWeeksTargetNetWorkTime { get; set; }
		[DataMember(Order = 2)]
		public TimeSpan ThisMonthsTargetNetWorkTime { get; set; }

		[DataMember(Order = 2)]
		public TimeSpan ThisWeeksTargetUntilTodayNetWorkTime { get; set; } //including today
		[DataMember(Order = 2)]
		public TimeSpan ThisMonthsTargetUntilTodayNetWorkTime { get; set; } //including today

		[DataMember(Order = 3)]
		public bool HasComputerActivity { get; set; } //quick brief info for displaying icons
		[DataMember(Order = 3)]
		public bool HasRemoteDesktop { get; set; } //quick brief info for displaying icons
		[DataMember(Order = 3)]
		public bool HasVirtualMachine { get; set; } //quick brief info for displaying icons
		[DataMember(Order = 3)]
		public List<string> IPAddresses { get; set; } //quick brief info for displaying icons

		[DataMember(Order = 4)]
		public byte? BatteryPercent { get; set; }
		[DataMember(Order = 4)]
		public string ConnectionType { get; set; }
		[DataMember(Order = 5)]
		public List<string> LocalIPAddresses { get; set; } //quick brief info for displaying icons


		//note: if you add fields here you should also modify the BriefUserStats.Update method in the SL proj and OnlineStatsManager.GetBriefUserStats too
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class DetailedUserStats : BriefUserStats
	{
		[DataMember]
		public Dictionary<int, DetailedComputerStats> ComputerStatsByCompId { get; set; }
		[DataMember]
		public DetailedIntervalStats IvrStats { get; set; } = new DetailedIntervalStats { TodaysWorkIntervalsByWorkId = new Dictionary<int, WorkWithIntervals>() };
		[DataMember]
		public DetailedIntervalStats ManuallyAddedStats { get; set; }
		[DataMember]
		public DetailedIntervalStats HolidayStats { get; set; }
		[DataMember]
		public DetailedIntervalStats SickLeaveStats { get; set; }
		[DataMember]
		public Dictionary<long, DetailedMobileStats> MobileStatsByMobileId { get; set; }
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class DetailedIntervalStats
	{
		[DataMember]
		public bool IsOnline { get; set; }
		[DataMember]
		public Dictionary<int, WorkWithIntervals> TodaysWorkIntervalsByWorkId { get; set; }
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class DetailedComputerStats : DetailedIntervalStats
	{
		[DataMember]
		public int ComputerId { get; set; }
		[DataMember]
		public ComputerActivity RecentComputerActivity { get; set; }
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class DetailedMobileStats : DetailedIntervalStats
	{
		[DataMember]
		public long MobileId { get; set; }
		[DataMember]
		public MobileActivity RecentMobileActivity { get; set; }
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class MobileActivity
	{
		public MobileActivity()
		{
			RecentActivityBuilder = new MobileActivityBuilder();
		}

		[DataMember]
		public List<LocationInfo> Locations { get; set; }

		[DataMember(Order = 1)]
		public byte? BatteryPercentage { get; set; }

		[DataMember(Order = 1)]
		public string ConnectionType { get; set; }

		[DataMember(Order = 2)]
		public string LastCameraShotPath { get; set; }

		[DataMember(Order = 3)]
		public List<int> RecentActivityPerMinute { get; set; }

		[IgnoreDataMember]
		public MobileActivityBuilder RecentActivityBuilder { get; set; } //for calculating RecentActivities

		public MobileActivity Clone()
		{
			return new MobileActivity()
			{
				Locations = this.Locations == null ? null : this.Locations.ToList(),
				BatteryPercentage = this.BatteryPercentage,
				ConnectionType = this.ConnectionType,
				LastCameraShotPath = this.LastCameraShotPath,
				RecentActivityPerMinute = this.RecentActivityPerMinute == null ? null : this.RecentActivityPerMinute.ToList(),
			};
		}
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class LocationInfo
	{
		[DataMember]
		public DateTime CreateDate { get; set; }
		[DataMember]
		public double Latitude { get; set; }
		[DataMember]
		public double Longitude { get; set; }
		[DataMember]
		public double Accuracy { get; set; }
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ComputerActivity
	{
		[DataMember]
		public List<ScreenShot> LastScreenShots { get; set; }
		[DataMember]
		public ActiveWindow LastActiveWindow { get; set; }
		[DataMember]
		public int LastMouseActivity { get; set; }
		[DataMember]
		public int LastKeyboardActivity { get; set; }
		[DataMember]
		public List<int> RecentMouseActivityPerMinute { get; set; }
		[DataMember]
		public List<int> RecentKeyboardActivityPerMinute { get; set; }
		[DataMember(Order = 1)]
		public bool IsRemoteDesktop { get; set; }
		[DataMember(Order = 1)]
		public bool IsVirtualMachine { get; set; }
		[DataMember(Order = 2)]
		public string IPAddress { get; set; }
		[DataMember(Order = 3)]
		public List<string> LocalIPAddresses { get; set; }
		[IgnoreDataMember]
		public DateTime LastSnapshot { get; set; }
		[IgnoreDataMember]
		public AggregateActivityBuilder RecentActivityBuilder { get; set; } //for calculating RecentActivities

		public ComputerActivity Clone()
		{
			return new ComputerActivity()
			{
				LastScreenShots = this.LastScreenShots == null ? null : this.LastScreenShots.ToList(),
				LastActiveWindow = this.LastActiveWindow,
				LastMouseActivity = this.LastMouseActivity,
				LastKeyboardActivity = this.LastKeyboardActivity,
				RecentKeyboardActivityPerMinute = this.RecentKeyboardActivityPerMinute == null ? null : this.RecentKeyboardActivityPerMinute.ToList(),
				RecentMouseActivityPerMinute = this.RecentMouseActivityPerMinute == null ? null : this.RecentMouseActivityPerMinute.ToList(),
				LastSnapshot = this.LastSnapshot,
				IsRemoteDesktop = this.IsRemoteDesktop,
				IsVirtualMachine = this.IsVirtualMachine,
				IPAddress = this.IPAddress,
			};
		}
	}

	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum IntervalManualSubType
	{
		ManualWorkItem = 0,
		Meeting = 1,
		Mobile = 2,
		Beacon = 3,
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class Interval
	{
		[DataMember]
		public DateTime StartDate { get; set; }
		[DataMember]
		public DateTime EndDate { get; set; }
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		[DefaultValue(0)]
		public IntervalManualSubType SubType { get; set; }
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public string Subject { get; set; }
		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public string Comment { get; set; }
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class WorkWithIntervals
	{
		[DataMember]
		public int WorkId { get; set; }
		[DataMember]
		public List<Interval> Intervals { get; set; }
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class WorkWithType
	{
		[DataMember]
		public int WorkId { get; set; }
		[DataMember]
		public WorkType Type { get; set; }
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class BriefWorkStats
	{
		[DataMember]
		public int WorkId { get; set; }
		[DataMember]
		public string WorkName { get; set; } //convenient
		[DataMember]
		public BriefWorkTimeStats WorkTimeStats { get; set; }
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class BriefNetWorkTimeStats : BriefWorkTimeStats
	{
		[DataMember]
		public TimeSpan NetWorkTime { get; set; }

		public BriefNetWorkTimeStats Clone()
		{
			return new BriefNetWorkTimeStats()
			{
				NetWorkTime = this.NetWorkTime,
				ComputerWorkTime = this.ComputerWorkTime,
				ManuallyAddedWorkTime = this.ManuallyAddedWorkTime,
				HolidayTime = this.HolidayTime,
				SickLeaveTime = this.SickLeaveTime,
				MobileWorkTime = this.MobileWorkTime,
			};
		}
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class BriefWorkTimeStats
	{
		[DataMember]
		public TimeSpan ComputerWorkTime { get; set; }
		[DataMember]
		public TimeSpan ManuallyAddedWorkTime { get; set; }
		[DataMember]
		public TimeSpan HolidayTime { get; set; }
		[DataMember]
		public TimeSpan SickLeaveTime { get; set; }
		[DataMember]
		public TimeSpan MobileWorkTime { get; set; }
	}

	//public class WorkTypeKey //??
	//{
	//    public WorkType Type { get; set; }
	//    public string SubType { get; set; } //like computerid, mobile number, imei etc ?
	//}

	[Flags]
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum OnlineStatus
	{
		[EnumMember]
		Offline = 0,
		[EnumMember]
		OnlineComputer = 1 << WorkType.Computer,
		[EnumMember]
		OnlineIvr = 1 << WorkType.Ivr,
		[EnumMember]
		OnlineManuallyAdded = 1 << WorkType.ManuallyAdded,
		[EnumMember]
		OnHoliday = 1 << WorkType.Holiday, //all day status?
		[EnumMember]
		OnSickLeave = 1 << WorkType.SickLeave, //all day status?
		[EnumMember]
		OnlineMobile = 1 << WorkType.Mobile,
		[EnumMember]
		OnlineBeacon = 2 << WorkType.Mobile, // we don't want to add a new worktype for beacon workitems (it's only a subtype)
	}

	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum WorkType
	{
		[EnumMember]
		Computer = 0,
		[EnumMember]
		Ivr,
		[EnumMember]
		ManuallyAdded,
		[EnumMember]
		Holiday,
		[EnumMember]
		SickLeave,
		[EnumMember]
		Mobile
	}
}
