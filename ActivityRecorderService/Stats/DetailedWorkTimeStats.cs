using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	public class DetailedWorkTimeStats : FullWorkTimeStats
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime? WorkStartDate { get; set; }
		public DateTime? WorkEndDate { get; set; }

		//public TimeSpan ComputerWorkTime { get; set; }
		//public TimeSpan IvrWorkTime { get; set; }
		//public TimeSpan SumWorkTime { get; set; }

		//public TimeSpan NetComputerWorkTime { get; set; }
		//public TimeSpan NetIvrWorkTime { get; set; }
		//public TimeSpan NetMobileWorkTime { get; set; }
		//public TimeSpan MobileWorkTime { get; set; }
		//public TimeSpan HolidayTime { get; set; }
		//public TimeSpan SickLeaveTime { get; set; }
		//public TimeSpan ManuallyAddedWorkTime { get; set; }
		//public TimeSpan ManuallyAddedTime { get { return ManuallyAddedWorkTime + SickLeaveTime + HolidayTime; } }

		//public TimeSpan ComputerCorrectionTime { get { return ComputerWorkTime - ComputerWorkTimeWithoutCorrection; } }
		//public TimeSpan IvrCorrectionTime { get { return IvrWorkTime - IvrWorkTimeWithoutCorrection; } }
		//public TimeSpan MobileCorrectionTime { get { return MobileWorkTime - MobileWorkTimeWithoutCorrection; } }
		//public TimeSpan ComputerWorkTimeWithoutCorrection { get; set; }
		//public TimeSpan IvrWorkTimeWithoutCorrection { get; set; }
		//public TimeSpan MobileWorkTimeWithoutCorrection { get; set; }

		public TimeSpan RemoteDesktopComputerWorkTime { get; set; }
		public TimeSpan VirtualMachineComputerWorkTime { get; set; }

		public List<ManualWorkItem> ManualWorkItems { get; set; }
		public List<WorkItem> WorkItems { get; set; }
		public List<MobileWorkItem> MobileWorkItems { get; set; }

		public Dictionary<int, TimeSpan> ComputerWorkTimeById { get; set; }
		public Dictionary<int, TimeSpan> IvrWorkTimeById { get; set; }
		public Dictionary<int, TimeSpan> MobileWorkTimeById { get; set; }
		public Dictionary<int, TimeSpan> AllWorkTimeById { get; set; }

		public override string ToString()
		{
			return "DetailedWorkTimeStats: " + StartDate + " - " + EndDate
				+ " Sum: " + SumWorkTime.ToHourMinuteSecondString()
				+ " Com: " + ComputerWorkTime.ToHourMinuteSecondString()
				+ " Mob: " + MobileWorkTime.ToHourMinuteSecondString()
				+ " NCo: " + NetComputerWorkTime.ToHourMinuteSecondString()
				+ " NMo: " + NetMobileWorkTime.ToHourMinuteSecondString()
				+ " CCo: " + ComputerCorrectionTime.ToHourMinuteSecondString()
				+ " CMo: " + MobileCorrectionTime.ToHourMinuteSecondString()
				+ " Hol: " + HolidayTime.ToHourMinuteString()
				+ " Sic: " + SickLeaveTime.ToHourMinuteString()
				+ " Man: " + ManuallyAddedWorkTime.ToHourMinuteString()
				+ " Wic: " + WorkItems.Count
				+ " Mac: " + ManualWorkItems.Count
				+ " Moc: " + MobileWorkItems.Count
				;
		}
	}
}
