using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderService
{
	//todo IDisposable
	public abstract class PeriodicManager : IStartable
	{
		private readonly ILog log;

		public int ManagerCallbackInterval { get; protected set; }
		private readonly Timer timer;
		private readonly object timerLock = new object();
		private bool isStarted;

		protected PeriodicManager()
			: this(null)
		{
		}

		protected PeriodicManager(ILog log)
		{
			this.log = log ?? LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			ManagerCallbackInterval = Timeout.Infinite;
			timer = new Timer(ManagerCallback, null, Timeout.Infinite, Timeout.Infinite);
		}

		private void ManagerCallback(object state)
		{
			lock (timerLock)
			{
				if (!isStarted) return;
				try
				{
					ManagerCallbackImpl();
				}
				catch (Exception ex)
				{
					log.Error("Unexpected error in ManagerCallbackImpl", ex);
				}
				finally
				{
					timer.Change(ManagerCallbackInterval, Timeout.Infinite);
				}
			}
		}

		protected abstract void ManagerCallbackImpl();

		public virtual void Start() => Start(0);
		public virtual void Start(int dueTime)
		{
			if (ManagerCallbackInterval == Timeout.Infinite)
			{
				log.Info("Manager is disabled");
				return;
			}
			log.Info("Starting timer");
			lock (timerLock)
			{
				isStarted = true;
				timer.Change(dueTime, Timeout.Infinite);
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

		protected void ExecuteManagerCallbackImpl()
		{
			lock (timerLock)
			{
				try
				{
					timer.Change(Timeout.Infinite, Timeout.Infinite); //don't pile up callbacks
					ManagerCallbackImpl();
				}
				catch (Exception ex)
				{
					log.Error("Unexpected error in ExecuteManagerCallbackImpl", ex);
				}
				finally
				{
					if (isStarted) timer.Change(ManagerCallbackInterval, Timeout.Infinite); //back to normal refreshes
				}
			}
		}
	}
}
