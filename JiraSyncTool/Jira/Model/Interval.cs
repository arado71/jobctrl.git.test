using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Model
{
	public class Interval : IEquatable<Interval>
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public bool Equals(Interval other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(other, this)) return true;

			return other.StartDate == StartDate && other.EndDate == EndDate;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Interval);
		}

		public override int GetHashCode()
		{
			return StartDate.GetHashCode() + 13 * EndDate.GetHashCode();
		}
	}
}
