using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	public interface IFilterableStats
	{
		bool SatisfiesFilter(StatsFilter filter);
	}

	public interface IFilterableStats<T> : IFilterableStats where T : IFilterableStats<T>
	{
		T GetFilteredCopy(StatsFilter filter);
	}
}
