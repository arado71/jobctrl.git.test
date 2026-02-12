using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	public class FullWorkTimeStats : WorkTimeStats
	{
		//public TimeSpan ComputerWorkTime { get; set; }
		//public TimeSpan IvrWorkTime { get; set; }
		//public TimeSpan SumWorkTime { get; set; }

		public TimeSpan NetComputerWorkTime { get; set; }
		public TimeSpan NetMobileWorkTime { get; set; }
		public TimeSpan MobileWorkTime { get; set; }
		public TimeSpan HolidayTime { get; set; }
		public TimeSpan SickLeaveTime { get; set; }
		public TimeSpan ManuallyAddedWorkTime { get; set; }
		public TimeSpan ManuallyAddedTime { get { return ManuallyAddedWorkTime + SickLeaveTime + HolidayTime; } }

		public TimeSpan ComputerCorrectionTime { get { return ComputerWorkTime - ComputerWorkTimeWithoutCorrection; } }
		public TimeSpan MobileCorrectionTime { get { return MobileWorkTime - MobileWorkTimeWithoutCorrection; } }
		public TimeSpan ComputerWorkTimeWithoutCorrection { get; set; }
		public TimeSpan MobileWorkTimeWithoutCorrection { get; set; }

		public FullWorkTimeStats CloneFullWorkTimeStats()
		{
			return new FullWorkTimeStats()
			{
				ComputerWorkTime = ComputerWorkTime,
				HolidayTime = HolidayTime,
				ManuallyAddedWorkTime = ManuallyAddedWorkTime,
				NetComputerWorkTime = NetComputerWorkTime,
				SickLeaveTime = SickLeaveTime,
				SumWorkTime = SumWorkTime,
				ComputerWorkTimeWithoutCorrection = ComputerWorkTimeWithoutCorrection,
				MobileWorkTimeWithoutCorrection = MobileWorkTimeWithoutCorrection,
				MobileWorkTime = MobileWorkTime,
				NetMobileWorkTime = NetMobileWorkTime,
			};
		}
	}
}
