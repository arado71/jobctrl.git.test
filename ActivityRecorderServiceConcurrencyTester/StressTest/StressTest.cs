using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.Tests.ActivityRecorderService.StressTest
{
	public class StressTest
	{
		private static int threadCount = 1;
		private static int outerLoopCount = 100;
		private static int innerLoopCount = 1;
		private static int screenShotInterval = 0;
		private static int useDb = 0;

		static void Main(string[] args)
		{
			bool paramsSet = false;
			bool stopAtEnd = true;
			if (args.Length == 5)
			{
				if (!int.TryParse(args[0], out threadCount) || threadCount < 1 || threadCount > 10
					|| !int.TryParse(args[1], out outerLoopCount)
					|| !int.TryParse(args[2], out innerLoopCount)
					|| !int.TryParse(args[3], out screenShotInterval)
					|| !int.TryParse(args[4], out useDb)
					)
				{
					PrintUsage();
					return;
				}
				paramsSet = true;
				stopAtEnd = false;
			}
			else if (args.Length == 6)
			{
				int value;
				if (int.TryParse(args[0], out value))
				{
					for (int i = 0; i < value; i++)
					{
						Process.Start(Assembly.GetExecutingAssembly().GetName().Name, string.Join(" ", args.Skip(1).ToArray()));
					}
					return;
				}
				else
				{
					PrintUsage();
					return;
				}
			}
			else if (args.Length != 0)
			{
				PrintUsage();
				return;
			}
			Console.WriteLine("ThreadCount: " + threadCount);
			Console.WriteLine("OuterLoopCount: " + outerLoopCount);
			Console.WriteLine("InnerLoopCount: " + innerLoopCount);
			Console.WriteLine("ScreenShotInterval: " + screenShotInterval);
			Console.WriteLine("UseDb: " + useDb);
			if (!paramsSet)
			{
				Console.WriteLine("Press any key to start.");
				Console.ReadKey(true);
			}
			EventWaitHandle[] handles = new EventWaitHandle[threadCount];
			for (int i = 0; i < threadCount; i++)
			{
				handles[i] = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(num =>
				{
					int currentThread = (int)num;
					try
					{
						var generator = new WorkItemGenerator()
						{
							ScreenShotInterval = TimeSpan.FromMilliseconds(screenShotInterval),
							ActiveWindowInterval = TimeSpan.FromSeconds(5),
						};
						if (useDb == 0)
						{
							generator.PhaseId = new Guid("11111111-1111-1111-1111-111111111111");
						}
						using (var etor = generator.GetWorkItems().GetEnumerator())
						{
							int iterOuter = outerLoopCount;
							while (--iterOuter >= 0)
							{
								using (var c = generator.GetClientWrapper())
								{
									int iterInner = innerLoopCount;
									while (--iterInner >= 0)
									{
										if (!etor.MoveNext())
										{
											throw new Exception("No more items"); //this should not happen because it's infinite
										}
										c.Client.AddWorkItemEx(etor.Current);
										Console.Write(currentThread);
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("ERROR @ " + currentThread);
						Console.WriteLine(ex);
						stopAtEnd = true;
					}
					finally
					{
						handles[currentThread].Set();
					}
				}, i);
			}
			WaitHandle.WaitAll(handles);
			Console.WriteLine("");
			Console.WriteLine("Done.");
			if (stopAtEnd)
			{
				Console.ReadKey(true);
			}
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage: " + Assembly.GetExecutingAssembly().GetName().Name + " ThreadCount[1-10] OuterLoopCount InnerLoopCount ScreenShotIntervalMs UseDb");
			Console.WriteLine("       " + Assembly.GetExecutingAssembly().GetName().Name + " ProcessCount ThreadCount OuterLoopCount InnerLoopCount ScreenShotIntervalMs UseDb");
		}
	}

}
