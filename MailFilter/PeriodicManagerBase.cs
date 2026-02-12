using System.Configuration;
using System.Threading;
using System;
using System.Diagnostics;
using System.Timers;
using log4net;
using Timer = System.Timers.Timer;

namespace Tct.MailFilterService
{
    public abstract class PeriodicManagerBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);	//TODO: adding log4net.dll to installer!!!
        private readonly object locker = new object();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Timer timer;
        protected int IntervalInMinutes;

        public PeriodicManagerBase()
        {
            timer = new Timer
            {
                AutoReset = false,
                Interval = 1
            };
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            IntervalInMinutes = ConfigurationManager.AppSettings["TimerIntervalInMinutes"] == null ? 60 : int.Parse(ConfigurationManager.AppSettings["TimerIntervalInMinutes"]);
        }

        public void Stop()
        {
            cts.Cancel();
            lock (locker)
            {
                timer.Dispose();
                timer = null;
            }
        }

        protected abstract void ExecuteOnTimer(CancellationToken token);

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
			Stopwatch sw = Stopwatch.StartNew();
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
                log.Error("timer_Elapsed()", ex);
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
					log.DebugFormat("Timer restarted, process took {0}", sw.Elapsed);
                }
            }
        }
    }
}
