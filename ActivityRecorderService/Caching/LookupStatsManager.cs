using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService.Caching
{
	public class LookupStatsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly LruCache<string, int> processCache;
		private readonly LruCache<string, int> titleCache;
		private readonly LruCache<string, int> urlCache;
		private int processHit;
		private int titleHit;
		private int urlHit;
		private int processMiss;
		private int titleMiss;
		private int urlMiss;
		private long processHitAll;
		private long titleHitAll;
		private long urlHitAll;
		private long processMissAll;
		private long titleMissAll;
		private long urlMissAll;

		public LookupStatsManager(LruCache<string, int> processCache, LruCache<string, int> titleCache, LruCache<string, int> urlCache)
			: base(log)
		{
			ManagerCallbackInterval = (int)TimeSpan.FromMinutes(20).TotalMilliseconds;
			this.processCache = processCache;
			this.titleCache = titleCache;
			this.urlCache = urlCache;
		}

		public void IncrementProcessHit()
		{
			Interlocked.Increment(ref processHit);
		}

		public void IncrementProcessMiss()
		{
			Interlocked.Increment(ref processMiss);
		}

		public void IncrementTitleHit()
		{
			Interlocked.Increment(ref titleHit);
		}

		public void IncrementTitleMiss()
		{
			Interlocked.Increment(ref titleMiss);
		}

		public void IncrementUrlHit()
		{
			Interlocked.Increment(ref urlHit);
		}

		public void IncrementUrlMiss()
		{
			Interlocked.Increment(ref urlMiss);
		}

		protected override void ManagerCallbackImpl()
		{
			var ph = Interlocked.Exchange(ref processHit, 0);
			var pm = Interlocked.Exchange(ref processMiss, 0);
			var th = Interlocked.Exchange(ref titleHit, 0);
			var tm = Interlocked.Exchange(ref titleMiss, 0);
			var uh = Interlocked.Exchange(ref urlHit, 0);
			var um = Interlocked.Exchange(ref urlMiss, 0);

			var pha = Interlocked.Add(ref processHitAll, ph);
			var pma = Interlocked.Add(ref processMissAll, pm);
			var tha = Interlocked.Add(ref titleHitAll, th);
			var tma = Interlocked.Add(ref titleMissAll, tm);
			var uha = Interlocked.Add(ref urlHitAll, uh);
			var uma = Interlocked.Add(ref urlMissAll, um);

			log.Info("Cache(C) H/M/H% Process(" + processCache.Count + ") " + ph + "/" + pm + "/" + (ph + pm == 0 ? 0d : (ph / (double)(ph + pm))).ToString("0.00%")
				+ " Title(" + titleCache.Count + ") " + th + "/" + tm + "/" + (th + tm == 0 ? 0d : (th / (double)(th + tm))).ToString("0.00%")
				+ " Url(" + urlCache.Count + ") " + uh + "/" + um + "/" + (uh + um == 0 ? 0d : (uh / (double)(uh + um))).ToString("0.00%")
				);

			log.Info("All Cache(C) H/M/H% Process(" + processCache.Count + ") " + pha + "/" + pma + "/" + (pha + pma == 0 ? 0d : (pha / (double)(pha + pma))).ToString("0.00%")
				+ " Title(" + titleCache.Count + ") " + tha + "/" + tma + "/" + (tha + tma == 0 ? 0d : (tha / (double)(tha + tma))).ToString("0.00%")
				+ " Url(" + urlCache.Count + ") " + uha + "/" + uma + "/" + (uha + uma == 0 ? 0d : (uha / (double)(uha + uma))).ToString("0.00%")
				);
		}
	}
}
