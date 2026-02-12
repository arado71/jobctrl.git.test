using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderClient
{
	[DataContract]
	public partial struct StartEndDateTime : IEquatable<StartEndDateTime>
	{
		[DataMember]
		private readonly DateTime startDate;
		[DataMember]
		private readonly DateTime endDate;

		[IgnoreDataMember]
		public DateTime StartDate { get { return startDate; } }
		[IgnoreDataMember]
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

		public override string ToString()
		{
			return startDate.ToString(CultureInfo.InvariantCulture) + " " + endDate.ToString(CultureInfo.InvariantCulture);
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

		#endregion

		#region Operator== overloading

		public static bool operator ==(StartEndDateTime a, StartEndDateTime b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(StartEndDateTime a, StartEndDateTime b)
		{
			return !(a == b);
		}

		#endregion
	}
}
