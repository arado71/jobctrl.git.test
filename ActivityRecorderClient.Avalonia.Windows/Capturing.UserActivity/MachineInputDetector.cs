using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.UserActivity
{
	public class MachineInputDetector
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const int PROPER_INTERVAL = 200;
		private const int THRESHOLD_PERCENT = 80;
		private const int MIN_EVENTS = 10;
		private const int MAX_ACCEPTED_PERCENT = 30;

		private int lastEvent;
		private readonly List<int> events = new List<int>();

		public int NewEvent(int now)
		{
			lock (events)
			{
				if (now - lastEvent > PROPER_INTERVAL)
				{
					lastEvent = now;
					return Flush(1);
				}

				lastEvent = now;
				events.Add(now);
				if (events.Count < MIN_EVENTS) return 0;
				if (AnalyzeEvents()) return 0;
				var ret = events.Count;
				events.Clear();
				return ret;
			}
		}

		private int Flush(int addition)
		{
			var cnt = events.Count >= MIN_EVENTS && AnalyzeEvents() ? addition : events.Count + addition;
			events.Clear();
			return cnt;
		}

		public int FlushEvents()
		{
			lock (events)
			{
				return Flush(0);
			}
		}

		private bool AnalyzeEvents()
		{
			var gapsRaw = Enumerable.Range(0, events.Count - 1).Select(i => new { index = i, value = events[i + 1] - events[i]}).OrderBy(g => g.value).ToList();
			gapsRaw.RemoveAt(0);
			gapsRaw.RemoveAt(gapsRaw.Count - 1);
			var gaps = gapsRaw.OrderBy(g => g.index).Select(g => g.value).ToList();
			var avgGap = (int)Math.Round(gaps.Average());
			var acpCnt = avgGap > 0 ? gaps.Count(g => Math.Abs(avgGap - g) * 100 / avgGap > THRESHOLD_PERCENT) : 0;
			var isCadenced = acpCnt * 100 / gaps.Count <= MAX_ACCEPTED_PERCENT;
			if (log.IsVerboseEnabled())
				log.Verbose($"Analyzer result gaps: [{string.Join(",", gaps)}], avg: {avgGap}, diffs: [{(avgGap > 0 ? string.Join(",", gaps.Select(g => Math.Abs(avgGap - g) * 100 / avgGap)) : "")}] acp: {acpCnt} result: {isCadenced}");
			return isCadenced;
		}
	}
}
