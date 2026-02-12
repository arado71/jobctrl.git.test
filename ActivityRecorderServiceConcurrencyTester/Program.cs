using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.Tests.ActivityRecorderService
{
	class Program
	{
		private static readonly byte[] dmyShot = Enumerable.Repeat((byte)4, 50000).ToArray();
		private static WorkItem item = new WorkItem()
		{
			PhaseId = new Guid("11111111-1111-1111-1111-111111111111"),
			WorkId = 0, //service sleeps (WorkId) ms for each call
			StartDate = new DateTime(1900, 03, 28, 12, 00, 00),
			EndDate = new DateTime(1900, 03, 28, 13, 00, 00),
			//ActiveWindows = new List<ActiveWindow>()
			//{
			//    new ActiveWindow() { ProcessName = "devenv.exe", Title = "Tct.Tests.ActivityRecorderService" ,CreateDate = new DateTime(1900, 03, 28, 12, 00, 00),}
			//},
			//ScreenShots = new List<ScreenShot>()
			//{
			//    new ScreenShot(){ Data = dmyShot, ScreenNumber = 0, Extension = "dmy", CreateDate = new DateTime(1900, 03, 28, 12, 00, 00),}
			//}
		};

		private static int threadCount = 1;
		private static int outerLoopCount = 100;
		private static int innerLoopCount = 1;
		private static int serviceWaitInterval = 0;

		static void Main2(string[] args)
		{
			if (args.Length == 4)
			{
				int value;
				if (int.TryParse(args[0], out value))
				{
					if (value >= 1 && value <= 10) threadCount = value;
				}
				else
				{
					PrintUsage();
					return;
				}
				if (int.TryParse(args[1], out value))
				{
					outerLoopCount = value;
				}
				else
				{
					PrintUsage();
					return;
				}
				if (int.TryParse(args[2], out value))
				{
					innerLoopCount = value;
				}
				else
				{
					PrintUsage();
					return;
				}
				if (int.TryParse(args[3], out value))
				{
					serviceWaitInterval = value;
				}
				else
				{
					PrintUsage();
					return;
				}
			}
			else if (args.Length == 5)
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
			item.WorkId = serviceWaitInterval;
			Console.WriteLine("ThreadCount: " + threadCount);
			Console.WriteLine("OuterLoopCount: " + outerLoopCount);
			Console.WriteLine("InnerLoopCount: " + innerLoopCount);
			Console.WriteLine("ServiceWaitInterval: " + serviceWaitInterval);
			if (args.Length != 4)
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
						int iterOuter = outerLoopCount;
						while (--iterOuter >= 0)
						{
							using (var c = new ActivityRecorderClientWrapper())
							{
								int iterInner = innerLoopCount;
								while (--iterInner >= 0)
								{
									c.Client.AddWorkItemEx(item);
									Console.Write(currentThread);
								}
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("ERROR @ " + currentThread);
						Console.WriteLine(ex);
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
			Console.ReadKey(true);
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage: " + Assembly.GetExecutingAssembly().GetName().Name + " ThreadCount OuterLoopCount InnerLoopCount ServiceWaitInterval");
			Console.WriteLine("       " + Assembly.GetExecutingAssembly().GetName().Name + " ProcessCount ThreadCount OuterLoopCount InnerLoopCount ServiceWaitInterval");
		}
	}

	public class ActivityRecorderClientWrapper : IDisposable
	{
		public readonly Tct.ActivityRecorderClient.ActivityRecorderServiceReference.ActivityRecorderClient Client;

		public ActivityRecorderClientWrapper()
		{
			Client = new Tct.ActivityRecorderClient.ActivityRecorderServiceReference.ActivityRecorderClient();
			Client.ClientCredentials.UserName.UserName = "asd";
			Client.ClientCredentials.UserName.Password = "asd";
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}