using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using log4net;
using Timer = System.Timers.Timer;

namespace JiraSyncTool
{
	internal abstract class PeriodicManagerBase
	{
		private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly object locker = new object();
		private readonly CancellationTokenSource cts = new CancellationTokenSource();
		private Timer timer;
		protected int IntervalInMinutes;

		public PeriodicManagerBase()
		{
			Initialize();
			timer = new Timer
			{
				AutoReset = false,
				Interval = 1
			};
			timer.Elapsed += timer_Elapsed;
			timer.Start();

			IntervalInMinutes = Properties.Settings.Default.TimerIntervalInMinutes;
		}

		public virtual void Stop()
		{
			cts.Cancel();
			lock (locker)
			{
				timer.Dispose();
				timer = null;
			}
		}

		protected abstract void ExecuteOnTimer(CancellationToken token);

		protected virtual void Initialize() { }

		private void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				log.Debug("Timer elapsed");

				lock (locker)
				{
					ExecuteOnTimer(cts.Token);
				}
			}
			catch (Exception ex)
			{
				log.Error("Timer_Elapse() error.", ex);
			}
			finally
			{
				lock (locker)
				{
					if (timer != null)
					{
						timer.Interval = IntervalInMinutes * 60 * 1000;
						timer.Start();
					}
				}
			}
		}
	}
}
