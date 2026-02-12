using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tct.ActivityRecorderService
{
	public static class LinqHelper
	{
		//http://www.codeproject.com/KB/cs/LinqColumnAttributeTricks.aspx
		public static int? GetLengthLimit(Type type, string field)
		{
			try
			{
				PropertyInfo prop = type.GetProperty(field);
				// Find the Linq 'Column' attribute
				// e.g. [Column(Storage="_FileName", DbType="NChar(256) NOT NULL", CanBeNull=false)]
				object[] info = prop.GetCustomAttributes(typeof(ColumnAttribute), true);
				// Assume there is just one
				if (info.Length == 1)
				{
					ColumnAttribute ca = (ColumnAttribute)info[0];
					string dbtype = ca.DbType;

					if (dbtype.StartsWith("NChar") || dbtype.StartsWith("NVarChar"))
					{
						int index1 = dbtype.IndexOf("(");
						int index2 = dbtype.IndexOf(")");
						string dblen = dbtype.Substring(index1 + 1, index2 - index1 - 1);
						int dblenint;
						if (int.TryParse(dblen, out dblenint))
						{
							return dblenint;
						}
					}
				}
			}
			catch { }
			return null;
		}

		public static DateTime ToSqlRoundTripDateTime(this DateTime original)
		{
			var timeTicks = original.TimeOfDay.Ticks;
			var sqlTimeTicks = (int)((timeTicks / 10000d * 0.3d) + 0.5d);
			var timeTicksBack = (long)((sqlTimeTicks / 0.3d) + 0.5d) * 10000L;
			Debug.Assert(timeTicksBack % 100000 == 0 || timeTicksBack % 100000 == 30000 || timeTicksBack % 100000 == 70000);
			return original.Date + TimeSpan.FromTicks(timeTicksBack);
		}

		private static DateTime ToSqlRoundTripDateTimeInAccurate(this DateTime original)
		{
			var originalWithoutTicks = new DateTime(original.Ticks / 10000L * 10000L);
			int mod10 = originalWithoutTicks.Millisecond % 10;
			if (mod10 == 2 || mod10 == 6 || mod10 == 9)
			{
				return originalWithoutTicks.AddMilliseconds(1);
			}
			else if (mod10 == 0 || mod10 == 3 || mod10 == 7)
			{
				return originalWithoutTicks;
			}
			else if (mod10 == 1 || mod10 == 4 || mod10 == 8)
			{
				return originalWithoutTicks.AddMilliseconds(-1);
			}
			else //if (mod10 == 5)
			{
				return originalWithoutTicks.AddMilliseconds(2);
			}
		}
	}
}
