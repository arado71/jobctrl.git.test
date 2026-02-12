using System;
using System.Collections.Generic;
using System.Linq;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.Tests.ActivityRecorderService.StressTest
{
	public class WorkItemGenerator
	{
		private static readonly byte[] dmyShot = Enumerable.Repeat((byte)4, 50000).ToArray();

		private readonly Random rnd = new Random();

		private int UserId { get; set; }

		public DateTime StartDate { get; set; }
		public TimeSpan ScreenShotInterval { get; set; }
		public TimeSpan ActiveWindowInterval { get; set; }
		public Guid PhaseId { get; set; }
		public int ComputerId { get; set; }

		private int workId;
		public int WorkId
		{
			get { return workId; }
			set
			{
				workId = value;
				PhaseId = Guid.NewGuid();
			}
		}

		public WorkItemGenerator(int? userId = null)
		{
			StartDate = DateTime.Now.AddMinutes(rnd.NextDouble() * 10 - 5);
			ScreenShotInterval = TimeSpan.Zero;
			ActiveWindowInterval = TimeSpan.Zero;
			UserId = (userId.HasValue && userId.Value != 0) ? -Math.Abs(userId.Value) : -rnd.Next(1000) - 1;
			WorkId = -rnd.Next(1000);
			ComputerId = rnd.Next();
		}

		public IEnumerable<WorkItem> GetWorkItems()
		{
			var start = StartDate;
			var nextAw = ActiveWindowInterval;
			var nextSs = ScreenShotInterval;

			while (true)
			{
				var end = start.AddSeconds(5).AddMilliseconds(rnd.Next(2001) - 1000);

				var workItem = new WorkItem()
				{
					UserId = UserId,
					StartDate = start,
					EndDate = end,
					WorkId = WorkId,
					PhaseId = PhaseId,
					ComputerId = ComputerId,
					KeyboardActivity = rnd.Next(100),
					MouseActivity = rnd.Next(5000),
				};

				if (ActiveWindowInterval > TimeSpan.Zero && nextAw >= ActiveWindowInterval)
				{
					workItem.DesktopCaptures.Add(new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { new DesktopWindow() {IsActive = true, ProcessName = "devenv.exe", Title = "Tct.Tests.ActivityRecorderService", CreateDate = workItem.StartDate, }} });
					nextAw -= ActiveWindowInterval;
				}

				if (ScreenShotInterval > TimeSpan.Zero && nextSs >= ScreenShotInterval)
				{
					workItem.DesktopCaptures.Add(new DesktopCapture() { Screens = new List<Screen>() { new Screen() { ScreenShot = dmyShot, ScreenNumber = 0, Extension = "dmy", CreateDate = workItem.StartDate, } } });
					nextSs -= ScreenShotInterval;
				}

				nextAw += end - start;
				nextSs += end - start;

				yield return workItem;

				start = end;
			}
		}

		public ActivityRecorderClientWrapper GetClientWrapper()
		{
			return new ActivityRecorderClientWrapper(UserId);
		}

		public class ActivityRecorderClientWrapper : IDisposable
		{
			public readonly Tct.ActivityRecorderClient.ActivityRecorderServiceReference.ActivityRecorderClient Client;

			public ActivityRecorderClientWrapper(int userId)
			{
				Client = new Tct.ActivityRecorderClient.ActivityRecorderServiceReference.ActivityRecorderClient();
				Client.ClientCredentials.UserName.UserName = userId.ToString();
				Client.ClientCredentials.UserName.Password = "asd";
			}

			public void Dispose()
			{
				WcfClientDisposeHelper.Dispose(Client);
			}
		}
	}
}
