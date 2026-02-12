using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;

namespace Reporter.Model
{
	public class Interval : IInterval
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public TimeSpan Duration { get { return EndDate - StartDate; } }

		public Interval()
		{
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MaxValue;
		}

		public Interval(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate;
			EndDate = endDate;
		}
	}
}
