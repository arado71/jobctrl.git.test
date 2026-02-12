using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.OnlineStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class DesktopLayoutVisualizerTests
	{
		[Fact]
		public void CanUseMultipleThreads()
		{
			var capture = new DesktopCapture()
			{
				Screens = new List<Screen>()
				{
					new Screen() { X = 0, Y = 0, Width = 800, Height = 600,},
				},
			};

			ThreadPool.SetMinThreads(20, 20);
			Parallel.For(0, 100, new ParallelOptions() { MaxDegreeOfParallelism = 20 }, i =>
				{
					var shots = DesktopLayoutVisualizer.GetScreenShotsFromCapture(capture);
					Assert.NotEmpty(shots);
				});
		}

		[Fact]
		public void Performance()
		{
			var capture = new DesktopCapture()
			{
				Screens = new List<Screen>()
				{
					new Screen() { X = 0, Y = 0, Width = 1680, Height = 1050,},
					//new Screen() { X = 1680, Y = 0, Width = 1680, Height = 1050,},
				},
			};

			var factor = 1 / 3f;

			var shots = DesktopLayoutVisualizer.GetScreenShotsFromCapture(capture, factor); //JIT
			var st = Environment.TickCount;
			for (int i = 0; i < 100; i++)
			{
				shots = DesktopLayoutVisualizer.GetScreenShotsFromCapture(capture, factor);
			}
			Console.WriteLine(Environment.TickCount - st);
			//System.IO.File.WriteAllBytes("c:\\tempscr.png", shots[0].Data.ToArray());
			//factor time without png encode and full for 100 runs and 1 screen
			//100 - 2.0  / 4.9s
			//50  - 0.7  / 1.5s
			//33  - 0.48 / 0.86
			//25  - 0.4  / 0.64
		}
	}
}
