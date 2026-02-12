using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
	public static class DateTimeEx
	{
		public static Func<DateTime> UtcNow = () => DateTime.UtcNow;
		public static Func<DateTime> Now = () => DateTime.Now;
	}
}
