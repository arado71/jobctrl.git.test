using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderService.OnlineStats;

namespace Tct.ActivityRecorderService.Stats
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class TotalWorkTimeStat
	{
		[DataMember]
		public int WorkId { get; private set; }

		//[DataMember]
		//public string WorkName { get; set; }

		//[DataMember]
		//public int? Priority { get; set; } // Kulon workDetail class ?

		//[DataMember]
		//public DateTime? CreateDate { get; set; } //should not be null, but retrived from other db...

		//[DataMember]
		//public DateTime? EndDate { get; set; } //lehet ezeket nem is itt kene atvinni ????

		//[DataMember]
		//public DateTime? TargetEndDate { get; set; }

		//[DataMember]
		//public TimeSpan? TargetTotalWorkTime { get; set; }

		[DataMember]
		public TimeSpan ComputerWorkTime { get; private set; }

		[DataMember]
		public TimeSpan ComputerCorrectionTime { get; private set; }

		[DataMember]
		public TimeSpan IvrWorkTime { get; private set; }

		[DataMember]
		public TimeSpan IvrCorrectionTime { get; private set; }

		[DataMember(Order = 1)]
		public TimeSpan MobileWorkTime { get; private set; }

		[DataMember(Order = 1)]
		public TimeSpan MobileCorrectionTime { get; private set; }

		[DataMember]
		public TimeSpan ManualWorkTime { get; private set; }

		[DataMember]
		public TimeSpan HolidayTime { get; private set; }

		[DataMember]
		public TimeSpan SickLeaveTime { get; private set; }

		[DataMember]
		public TimeSpan TotalWorkTime { get; private set; }

		internal static TotalWorkTimeStat CreateFrom(GetTotalWorkTimeByWorkIdForUserResult sprocResult)
		{
			if (sprocResult == null) return null;
			return new TotalWorkTimeStat()
			{
				WorkId = sprocResult.WorkId,
				ComputerWorkTime = TimeSpan.FromMilliseconds(sprocResult.ComputerWorkTime),
				ComputerCorrectionTime = TimeSpan.FromMilliseconds(sprocResult.ComputerCorrectionTime),
				IvrWorkTime = TimeSpan.FromMilliseconds(sprocResult.IvrWorkTime),
				IvrCorrectionTime = TimeSpan.FromMilliseconds(sprocResult.IvrCorrectionTime),
				MobileWorkTime = TimeSpan.FromMilliseconds(sprocResult.MobileWorkTime),
				MobileCorrectionTime = TimeSpan.FromMilliseconds(sprocResult.MobileCorrectionTime),
				ManualWorkTime = TimeSpan.FromMilliseconds(sprocResult.ManualWorkTime),
				HolidayTime = TimeSpan.FromMilliseconds(sprocResult.HolidayTime),
				SickLeaveTime = TimeSpan.FromMilliseconds(sprocResult.SickLeaveTime),
				TotalWorkTime = TimeSpan.FromMilliseconds(sprocResult.TotalWorkTime),
			};
		}

		internal static TotalWorkTimeStat CreateFrom(SimpleWorkTimeStat simple)
		{
			if (simple == null) return null;
			return new TotalWorkTimeStat()
			{
				WorkId = simple.WorkId,
				TotalWorkTime = simple.TotalWorkTime,
			};
		}
	}
}
