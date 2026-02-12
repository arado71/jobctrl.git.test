using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class RandomTests
	{
		[Fact]
		public void OneIsValid()
		{
			var res = RandomHelper.Next(1);
			Assert.Equal(0, res);
		}

		[Fact(Skip = "Proper test would take long time")]
		public void SimpleDistribution()
		{
			var size = 100;
			var res = new int[size];
			for (int i = 0; i < int.MaxValue / 100; i++)
			{
				res[RandomHelper.Next(size)]++;
			}
			for (int i = 0; i < size; i++)
			{
				Console.WriteLine(res[i]);
			}
			Console.WriteLine((res.Max() - res.Min()) / ((double)res.Max()));
			Assert.True((res.Max() - res.Min()) / ((double)res.Max()) < 0.02);
		}
	}
}
