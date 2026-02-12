using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.OnlineStats
{
	public static class MobileHelper
	{
		public static long GetMobileId(string imeiStr)
		{
			long imei;
			if (!long.TryParse(imeiStr, out imei))
			{
				//we don't store these so we can use GetHashCode() and get away with it if its implementation changes
				imei = -1 * Math.Abs((long)(imeiStr ?? "").GetHashCode()); //when imei cannot be read then some other identifier is used (use a negative number for this)
			}
			return imei;
		}
	}
}
