using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Tct.ActivityRecorderService.EmailStats;

namespace Tct.ActivityRecorderService.Ocr
{
	public class OcrStatsHelper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(OcrStatsHelper));
		public SampleStorage SampleStorage { get; private set; }
		public readonly Dictionary<StatsTypeEnum, int> stats;

		public OcrStatsHelper()
		{
			SampleStorage = new SampleStorage();
			stats = new Dictionary<StatsTypeEnum, int>();
		}

		public void AddStat(StatsTypeEnum key, int value)
		{
			if (stats.ContainsKey(key))
				stats[key] = value;
			else
				stats.Add(key, value);
		}

		public void RemoveStats()
		{
			if (stats == null) return;
			stats.Clear();
		}

		public void ResetStorage()
		{
			SampleStorage.Dispose();
			SampleStorage = new SampleStorage();
		}

		public void Dispose()
		{
			SampleStorage.Dispose();
		}
	}

	public enum StatsTypeEnum
	{
		CompanyId,
		NewSnippetsCount, 
		Iterations,
		ElapsedMinutes
	}

}
