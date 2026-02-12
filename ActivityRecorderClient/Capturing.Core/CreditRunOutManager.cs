using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Communication;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class CreditRunOutManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int callbackIntervalSettled = 12 * 60 * 60 * 1000;  //12 hours
		private const int callbackIntervalRunOut = 60 * 60 * 1000;  //60 mins
		private const int callbackRetryInterval = 3 * 60 * 1000;  //3 mins

		private bool lastSendFailed;
		private DateTime? _runOutDate = null;
		private volatile CreditRunOutState state;

		public int? RemainingDays
		{
			get { return _runOutDate.HasValue ? (_runOutDate.Value > DateTime.UtcNow ? (int?)(_runOutDate.Value.Date - DateTime.UtcNow.Date).TotalDays : (int?)0) : null; }
		}

		public CreditRunOutState State
		{
			get { return state; }
			private set { state = value; }
		}


		public event EventHandler<SingleValueEventArgs<CreditRunOutState>> StateChanged;

		public CreditRunOutManager()
			: base(log)
		{
			State = CreditRunOutState.Unknown;
			lastSendFailed = false;
		}

		private void UpdateExpiryDate()
		{
			try
			{
				var oldRemaining = RemainingDays;
				int userId = ConfigManager.UserId;
				_runOutDate = ActivityRecorderClientWrapper.Execute(n => n.GetExpiryDay(userId));
				log.Info("Current expiry date: " + (_runOutDate.HasValue ? _runOutDate.ToShortDateString() : "none"));
				lastSendFailed = false;
				var oldState = State;
				switch (RemainingDays)
				{
					case null:
						State = CreditRunOutState.Settled;
						break;
					case 0:
						State = CreditRunOutState.RunOut;
						break;
					default:
						State = CreditRunOutState.RunLow;
						break;
				}
				if (oldState != State || oldRemaining != RemainingDays)
				{
					if (oldState != State)
						log.Info("RunOutState changed");
					if (oldRemaining != RemainingDays)
						log.Info("Remaining days to expiration changed (" + (RemainingDays.HasValue ? RemainingDays.ToString() : "none") + ")");
					RaiseStateChanged();
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("GetExpiryDay", log, ex);
				lastSendFailed = true;
			}
		}

		protected override void ManagerCallbackImpl()
		{
			UpdateExpiryDate();
		}

		protected override int ManagerCallbackInterval
		{
			get
			{
				return lastSendFailed
						   ? callbackRetryInterval
						   : State == CreditRunOutState.Settled ? callbackIntervalSettled : callbackIntervalRunOut;
			}
		}

		private void RaiseStateChanged()
		{
			var handler = StateChanged;
			if (handler != null)
				handler(this, new SingleValueEventArgs<CreditRunOutState>(State));
		}
	}

	public enum CreditRunOutState
	{
		Unknown,
		Settled,
		RunLow,
		RunOut,
	}
}
