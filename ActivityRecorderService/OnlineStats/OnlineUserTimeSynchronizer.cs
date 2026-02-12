using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.OnlineStats
{
	/// <summary>
	/// Calculates user's clock skew
	/// </summary>
	/// <remarks>It is not watertight but good enough for now...</remarks>
	public class OnlineUserTimeSynchronizer
	{
		private readonly TimeSpan maxClockSkew;
		private const int maxItems = 15;

		private readonly List<Measurement> recentMeasures = new List<Measurement>(maxItems + 1);
		public TimeSpan UserTimeDiff { get; private set; }

		public OnlineUserTimeSynchronizer(TimeSpan maxClockSkew)
		{
			this.maxClockSkew = maxClockSkew;
		}

		public void AddData(DateTime userTime, DateTime serverTime)
		{
			if (userTime + maxClockSkew < serverTime || userTime - maxClockSkew > serverTime) return; //old/invalid item not online data
			var curr = new Measurement() { MeasureDate = serverTime, TimeDiff = userTime - serverTime, };
			recentMeasures.Add(curr);
			if (recentMeasures.Count > maxItems) recentMeasures.RemoveAt(0);
			UserTimeDiff = new TimeSpan((long)(recentMeasures
				//.OrderBy(n => n.TimeDiff)
				//.Skip(3)
				//.Take(maxItems - 6)
				.Select(n => n.TimeDiff.Ticks)
				.DefaultIfEmpty(0)
				.Average() + 0.5d));
		}

		private class Measurement
		{
			public DateTime MeasureDate { get; set; }
			public TimeSpan TimeDiff { get; set; }
		}
	}
}
