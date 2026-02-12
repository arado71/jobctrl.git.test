using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.WorktimeHistory
{
	internal class DateTimeEx
	{
		private static Func<DateTime> nowOverride = null; 

		public static DateTime UtcNow
		{
			get
			{
				return nowOverride != null ? nowOverride() : DateTime.UtcNow;
			}
		}

#if DEBUG
		public static void OverrideNow(Func<DateTime> overrideFunc)
		{
			nowOverride = overrideFunc;
		}
#endif
	}
}
