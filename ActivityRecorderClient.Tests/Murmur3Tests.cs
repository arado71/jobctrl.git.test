using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class Murmur3Tests
	{
		[Fact]
		public void TestOutput_x86_128()
		{
			Assert.Equal(new byte[] { 43, 174, 52, 225, 223, 122, 240, 213, 24, 251, 39, 231, 89, 195, 25, 101 },
				Murmur3.ComputeHash(Encoding.UTF8.GetBytes("EXCEL.EXE")));
		}

		[Fact]
		public void Performance()
		{
			const int it = 1000; //1.911ms for 100000 on my comp

			var rnd = new Random(123);
			var bytes = new byte[1000];
			var sw = Stopwatch.StartNew();
			for (int i = 0; i < it; i++)
			{
				rnd.NextBytes(bytes);
				var res = Murmur3.ComputeHash(bytes);
			}
			Console.WriteLine(sw.Elapsed.TotalMilliseconds + "ms");
		}
	}
}
