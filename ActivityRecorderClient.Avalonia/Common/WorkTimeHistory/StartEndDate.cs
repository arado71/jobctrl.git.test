using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService
{
	public struct StartEndDateTime : IEquatable<StartEndDateTime>
	{
		private readonly DateTime startDate;
		private readonly DateTime endDate;

		public DateTime StartDate { get { return startDate; } }
		public DateTime EndDate { get { return endDate; } }

		public StartEndDateTime(DateTime startDate, DateTime endDate)
		{
			this.startDate = startDate;
			this.endDate = endDate;
		}

		public TimeSpan Duration()
		{
			return endDate - startDate;
		}

		#region IEquatable<StartEndDateTime> Members

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(obj, null))
				return false;

			if (obj is StartEndDateTime)
			{
				return this.Equals((StartEndDateTime)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int result = 17;
			result = 31 * result + StartDate.GetHashCode();
			result = 31 * result + EndDate.GetHashCode();
			return result;
		}

		public bool Equals(StartEndDateTime other)
		{
			return this.startDate == other.startDate && this.endDate == other.endDate;
		}

		public override string ToString()
		{
			return $"{startDate} -> {endDate}";
		}

		#endregion
	}
}
