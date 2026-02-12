using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	[Flags]
	public enum ReportFrequency
	{
		None = 0,
		Daily = 1 << 0,
		Weekly = 1 << 1,
		Monthly = 1 << 2
	}
}
