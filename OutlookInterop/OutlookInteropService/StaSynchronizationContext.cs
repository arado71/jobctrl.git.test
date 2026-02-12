using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;

namespace OutlookInteropService
{
	public class StaSynchronizationContext
	{
		private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
		private readonly AutoResetEvent notifyPost = new AutoResetEvent(false);
		private readonly Thread actionHandler;

		private static readonly Lazy<StaSynchronizationContext> instance = new Lazy<StaSynchronizationContext>(() => new StaSynchronizationContext(), true);

		public static StaSynchronizationContext Current => instance.Value;

		protected StaSynchronizationContext()
		{
			actionHandler = new Thread(ProcessActions) { IsBackground = true };
			actionHandler.SetApartmentState(ApartmentState.STA);
			actionHandler.Start();
		}

		private void ProcessActions()
		{
			while (true)
			{
				while (actions.TryDequeue(out var action))
				{
					action();
				}
				notifyPost.WaitOne(5000);
			}
		}

		public void Post(Action action)
		{
			if (actionHandler.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				action();
				return;
			}
			actions.Enqueue(action);
			notifyPost.Set();
		}

		public void Send(Action action)
		{
			var completed = new ManualResetEvent(false);
			Post(() => {
				action();
				completed.Set();
			});
			completed.WaitOne(TimeSpan.FromSeconds(30));
		}
	}
}
