using System;
using System.Threading;
using log4net;

namespace Tct.ActivityRecorderClient.Stability
{
	/// <summary>
	/// Simple Watchdog timer
	/// </summary>
	public class WatchdogManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly int checkInterval;
		private volatile int raiseInterval;
		private int relativeTime;

		public event EventHandler MissingReset;

		public int RaiseInterval
		{
			get
			{
				return raiseInterval;
			}

			set
			{
				raiseInterval = value;
			}
		}

		public WatchdogManager(int checkInterval, int raiseInterval)
			: base(log, false)
		{
			this.checkInterval = checkInterval;
			this.raiseInterval = raiseInterval;
		}

		public override void Start(int firstDueTime = 0)
		{
			Reset();
			base.Start(firstDueTime);
		}

		public void Reset()
		{
			Interlocked.Exchange(ref relativeTime, 0);
		}

		protected override void ManagerCallbackImpl()
		{
			var elapsed = Interlocked.Add(ref relativeTime, checkInterval); //not accurate because we might just had a Reset
			if (elapsed <= raiseInterval) return;
			Reset();
			OnMissingReset();
		}

		protected override int ManagerCallbackInterval
		{
			get { return checkInterval; }
		}

		private void OnMissingReset()
		{
			var del = MissingReset;
			if (del != null) del(this, EventArgs.Empty);
		}
	}
}
