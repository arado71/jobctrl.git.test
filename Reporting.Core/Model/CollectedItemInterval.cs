using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Model
{
	public class CollectedItemInterval : Interval
	{
		public Dictionary<string, string> Values { get; set; }

		public CollectedItemInterval()
		{
			Values = new Dictionary<string, string>();
		}

		public CollectedItemInterval(DateTime startDate, DateTime endDate) : base(startDate, endDate)
		{
		}
	}
}
