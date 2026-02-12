using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Processing
{
	public class Series
	{
		private double[] series;
		private double? min, max, sum, avg;

		public double Min { get { return GetCached(ref min, GetMin); } }
		public double Max { get { return GetCached(ref max, GetMax); } }
		public double Sum { get { return GetCached(ref sum, GetSum); } }
		public double Average { get { return GetCached(ref avg, GetAvg); } }
		public int Count { get { return series.Length; } }

		private double GetMin()
		{
			return series.Length > 0 ? series[0] : -1;
		}

		private double GetAvg()
		{
			return series.Length > 0 ? Sum / series.Length : -1;
		}

		private double GetMax()
		{
			return series.Length > 0 ? series[series.Length - 1] : -1;
		}

		private double GetSum()
		{
			return series.Length > 0 ? series.Sum() : -1;
		}

		private Series(double[] source)
		{
			series = source;
		}

		private double GetCached(ref double? val, Func<double> getFunc)
		{
			if (!val.HasValue)
			{
				val = getFunc();
			}

			return val.Value;
		}

		public static Series Create(IEnumerable<double> source)
		{
			return new Series(source.OrderBy(x => x).ToArray());
		}

		public double GetPercentile(double percentile)
		{
			if (series.Length == 0) return -1;
			if (percentile < 0 || percentile > 1) throw new ArgumentException("percentile");
			var n = (series.Length - 1) * percentile + 1;
			if (n == 1.0) return series[0];
			if (n == series.Length) return series[series.Length - 1];
			var k = (int)n;
			var d = n - k;
			return series[k - 1] + d * (series[k] - series[k - 1]);
		}
	}
}
