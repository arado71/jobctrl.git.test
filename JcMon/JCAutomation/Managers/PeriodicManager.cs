using System;
using System.Diagnostics;
using System.Threading;
using log4net;

namespace JCAutomation.Managers
{
	public abstract class PeriodicManager
	{
		private readonly ILog log;

		private readonly Timer timer;
		private readonly object timerLock = new object();
		private readonly bool verboseLogging;
		private bool isStarted;
		protected bool IsStarted
		{
			get
			{
				lock (timerLock)
				{
					return isStarted;
				}
			}
			private set
			{
				lock (timerLock)
				{
					isStarted = value;
				}
			}
		}

		protected PeriodicManager()
			: this(null, true)
		{
		}

		protected PeriodicManager(ILog log)
			: this(log, true)
		{
		}

		protected PeriodicManager(ILog log, bool verboseLogging)
		{
			Debug.Assert(log != null, "No logger specified");
			this.log = log ?? LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			timer = new Timer(ManagerCallback, null, Timeout.Infinite, Timeout.Infinite);
			this.verboseLogging = verboseLogging;
		}

		private void ManagerCallback(object state)
		{
			lock (timerLock)
			{
				if (!isStarted) return;
				try
				{
					if (verboseLogging) log.Debug("Calling ManagerCallbackImpl");
					ManagerCallbackImpl();
				}
				catch (Exception ex)
				{
					log.Error("Unexpected error in ManagerCallbackImpl", ex);
				}
				finally
				{
					if (isStarted) //might be stopped in ManagerCallbackImpl() so avoid one unnecessary callback
					{
						timer.Change(ManagerCallbackInterval, Timeout.Infinite);
					}
				}
			}
		}

		protected abstract void ManagerCallbackImpl();
		protected abstract int ManagerCallbackInterval { get; }

		public virtual void Start(int firstDueTime = 0)
		{
			log.Info("Starting timer");
			lock (timerLock)
			{
				isStarted = true;
				timer.Change(firstDueTime, Timeout.Infinite);
			}
		}

		public virtual void Stop()
		{
			lock (timerLock)
			{
				isStarted = false;
				timer.Change(Timeout.Infinite, Timeout.Infinite);
			}
			log.Info("Stopped timer");
		}

		protected void RestartTimer() //we can do better than this, without blocking... but this is ok atm.
		{
			lock (timerLock)
			{
				if (!isStarted) return;
				timer.Change(0, Timeout.Infinite);
			}
		}
	}
}
