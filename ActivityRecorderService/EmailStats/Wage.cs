using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	//when currency introduced, replace decimal with Cost (decimal,currency)
	//but in that case we have to introduce currency conversations
	//(or store different currencies separately), so it will complicate things
	public abstract class Wage
	{
		public abstract TimeSpan Interval { get; }
		private readonly List<WageChange> changes = new List<WageChange>();

		protected Wage(decimal defaultWage)
		{
			changes.Add(new WageChange(DateTime.MinValue, defaultWage));
		}

		public void SetChange(DateTime startDate, decimal newWage)
		{
			var newChange = new WageChange(startDate, newWage);
			int idx = changes.BinarySearch(newChange, WageChange.StartDateComparer);
			if (idx >= 0) //found a change with same startdate
			{
				changes.RemoveAt(idx); //immutable so remove
				changes.Insert(idx, newChange);
			}
			else
			{
				int newIdx = ~idx;
				changes.Insert(newIdx, newChange);
			}
		}

		public decimal GetCostFor(DateTime startDate, DateTime endDate)
		{
			int idx = changes.BinarySearch(new WageChange(startDate, 0m), WageChange.StartDateComparer);
			int startIdx = idx >= 0 ? idx : ~idx - 1;
			DateTime currStart = startDate;
			DateTime currEnd;
			decimal sumWage = 0m;
			while (currStart != endDate)
			{
				decimal currentWage = changes[startIdx].Wage;
				currEnd = startIdx + 1 < changes.Count ? changes[startIdx + 1].StartDate : endDate;
				if (currEnd > endDate) currEnd = endDate;
				sumWage += (decimal)(currEnd.Ticks - currStart.Ticks) / Interval.Ticks * currentWage;

				startIdx++;
				currStart = currEnd;
			}
			return sumWage;
		}

		/// <summary>
		/// Immutable class that describes a wage change
		/// </summary>
		private class WageChange
		{
			public decimal Wage { get; private set; }
			public DateTime StartDate { get; private set; }

			public WageChange(DateTime startDate, decimal wage)
			{
				Wage = wage;
				StartDate = startDate;
			}

			public static readonly IComparer<WageChange> StartDateComparer = new WageChangeStartDateComparer();
			private class WageChangeStartDateComparer : IComparer<WageChange>
			{
				public int Compare(WageChange x, WageChange y)
				{
					Debug.Assert(x != null);
					Debug.Assert(y != null);
					if (x == null)
					{
						return y == null ? 0 : -1;
					}
					if (y == null) return 1;

					return DateTime.Compare(x.StartDate, y.StartDate);
				}
			}
		}
	}
}
