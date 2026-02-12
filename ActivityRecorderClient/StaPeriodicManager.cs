using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderClient
{
	/// <summary>
	/// PeriodicManager with a dedicated STA thread. Which is basically executing ManagerCallbackImpl after evey ManagerCallbackInterval.
	/// </summary>
	public abstract class StaPeriodicManager
	{
		private readonly ILog log;

		private readonly bool verboseLogging;
		private readonly string threadName;
		private readonly object thisLock = new object(); //protecting staThread, isStarted and ManagerCallbackImpl()
		private Thread staThread;
		private bool isRestartRequested;
		private bool isStarted;
		protected bool IsStarted
		{
			get
			{
				lock (thisLock)
				{
					return isStarted;
				}
			}
		}

		protected Thread StaThread { get { return staThread; } }

		protected StaPeriodicManager()
			: this(null, true, null)
		{
		}

		protected StaPeriodicManager(ILog log)
			: this(log, true, null)
		{
		}

		protected StaPeriodicManager(ILog log, bool verboseLogging)
			: this(log, verboseLogging, null)
		{
		}

		protected StaPeriodicManager(ILog log, bool verboseLogging, string threadName)
		{
			Debug.Assert(log != null, "No logger specified");
			this.log = log ?? LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			this.verboseLogging = verboseLogging;
			this.threadName = threadName;
		}

		private void ThreadLoop(object state)
		{
			int? firstDueTime = (int)state;
			lock (thisLock)
			{
				while (true)
				{
					if (!isStarted) return; //might be stopped in ManagerCallbackImpl() or right after Start() so avoid the unnecessary wait
					if (isRestartRequested)
					{
						firstDueTime = 0;
					}
					Monitor.Wait(thisLock, firstDueTime ?? ManagerCallbackInterval);
					isRestartRequested = false;
					firstDueTime = null;
					if (!isStarted) return; //if we are being awakened by Stop()
					try
					{
						if (verboseLogging) log.Debug("Calling ManagerCallbackImpl");
						ManagerCallbackImpl();
					}
					catch (Exception ex)
					{
						log.ErrorAndFail("Unexpected error in ManagerCallbackImpl", ex);
					}
				}
			}
		}

		protected abstract void ManagerCallbackImpl();
		protected abstract int ManagerCallbackInterval { get; }

		public virtual void Start(int firstDueTime = 0)
		{
			log.Info("Starting timer");
			lock (thisLock)
			{
				if (isStarted)
				{
					log.InfoAndFail("Timer is already started");
					return;
				}
				while (staThread != null) //Thread is stopping (If Start and Stop called from different threads)
				{
					log.InfoAndFail("Timer is not stopped yet");
					Monitor.Exit(thisLock); //let Stop set staThread to null
					Thread.Sleep(100);
					Monitor.Enter(thisLock);
				}
				staThread = new Thread(ThreadLoop);
				staThread.SetApartmentState(ApartmentState.STA);
				staThread.IsBackground = true;
				if (threadName != null) staThread.Name = threadName;
				staThread.Start(firstDueTime);
				isStarted = true;
			}
		}

		public virtual void Stop()
		{
			lock (thisLock)
			{
				if (isStarted == false)
				{
					log.InfoAndFail("Timer is already stopped");
					return;
				}
				isStarted = false;
				Monitor.Pulse(thisLock); //wake up ThreadLoop
			}
			staThread.Join(); //we have to release lock before Join to avoid deadlock
			lock (thisLock)
			{
				staThread = null;
			}
			log.Info("Stopped timer");
		}

		protected void RestartTimer() //we can do better than this, without blocking... but this is ok atm.
		{
			lock (thisLock)
			{
				if (!isStarted) return;
				isRestartRequested = true;
				Monitor.Pulse(thisLock); //wake up ThreadLoop (this might be lost, that is why we use isRestartRequested)
			}
		}
	}
}
