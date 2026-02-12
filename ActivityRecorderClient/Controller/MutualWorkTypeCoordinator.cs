using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.Controller
{
	/// <summary>
	/// Coordinates/signals mutually exclusive work type services on the GUI thread, to make sure only one is active.
	/// </summary>
	public class MutualWorkTypeCoordinator : IMutualWorkTypeService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan nfCreditRunOutDuration = TimeSpan.Zero;

		private readonly List<IMutualWorkTypeService> otherWorkTypeServices = new List<IMutualWorkTypeService>();
		private readonly TimeManager timeManager;
		private readonly INotificationService notificationService;
		
		private string cachedStateString;
		private int activeStateTransactions;

		public event EventHandler StateTransitionCompleted;

		private TimeSpan nfInvalidTimeCannotWorkDuration
		{
			get { return GetValidDuration(ConfigManager.LocalSettingsForUser.NotWorkingWarnDuration); }
		}

		public CreditRunOutState CreditRunOutState { get; set; }
		public int CreditRunOutRemainingDays { get; set; }

		public MutualWorkTypeCoordinator(TimeManager timeManager, INotificationService notificationService)
		{
			this.notificationService = notificationService;
			this.timeManager = timeManager;
		}

		public void Add(IMutualWorkTypeService otherWorkTypeService)
		{
			otherWorkTypeServices.Add(otherWorkTypeService);
		}

		public void Remove(IMutualWorkTypeService otherWorkTypeService)
		{
			otherWorkTypeServices.Remove(otherWorkTypeService);
		}

		public bool IsWorking
		{
			get { return requestInProgress || otherWorkTypeServices.Any(n => n.IsWorking); }
		}

		public string StateString
		{
			get
			{
				return string.Join(" ", otherWorkTypeServices.Select(x => x.StateString).Where(x => !string.IsNullOrEmpty(x)).ToArray());
			}
		}

		MutualWorkTypeInfo IMutualWorkTypeService.RequestStopWork(bool isForced)
		{
			return RequestStopWork(isForced, null);
		}

		bool requestInProgress; //handle reentrancy
		public MutualWorkTypeInfo RequestStopWork(bool isForced, string reason)
		{
			var sb = new StringBuilder();
			requestInProgress = true;
			try
			{
				return otherWorkTypeServices.Aggregate(
					new MutualWorkTypeInfo() { CanStartWork = CanStartWork(reason), ResumeWorkOnClose = false, }
					, (prev, n) =>
						{
							if (n.IsWorking)
							{
								sb.Append(" ").Append(n.GetType().Name).Append("-").Append(n.IsWorking ? "Working" : "NotWorking");
								var curr = n.RequestStopWork(isForced);
								prev.CanStartWork &= curr.CanStartWork;
								prev.ResumeWorkOnClose |= curr.ResumeWorkOnClose;
								sb.Append("->").Append(curr.CanStartWork ? "CanStart" : "CannotStart").Append(curr.ResumeWorkOnClose ? "-R" : "-N");
							}
							return prev;
						});
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in RequestStopWork", ex);
				throw;
			}
			finally
			{
				requestInProgress = false;
				log.Info("RequestStopWork (" + reason + ")" + sb);
			}
		}

		public void RequestKickWork()
		{
			var sb = new StringBuilder();
			requestInProgress = true;
			try
			{
				otherWorkTypeServices.ForEach( n =>
					{
						if (n.IsWorking)
						{
							sb.Append(" ").Append(n.GetType().Name).Append("-").Append(n.IsWorking ? "Working" : "NotWorking");
							n.RequestKickWork();
						}
					});
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in RequestKickWork", ex);
				throw;
			}
			finally
			{
				requestInProgress = false;
				log.Info("RequestKickWork " + sb);
			}
		}

		private void ReevaluateStates()
		{
			DebugEx.EnsureGuiThread();
			var currentState = StateString;
			if (!string.Equals(cachedStateString, currentState, StringComparison.OrdinalIgnoreCase))
			{
				cachedStateString = currentState;
				OnStateTransitionCompleted();
				log.DebugFormat("Application status changed to {0}", currentState);
			}
		}

		protected void OnStateTransitionCompleted()
		{
			var evt = StateTransitionCompleted;
			if (evt != null) evt(this, EventArgs.Empty);
		}

		private bool CanStartWork(string reason)
		{
			if (CreditRunOutState != CreditRunOutState.Settled)
			{
				ShowCreditRunOutNotification();
				if (CreditRunOutState == CreditRunOutState.RunOut)
				{
					log.Info("Cannot start work because credit run out (" + reason + ")");
					return false;
				}
			}

			if (timeManager.IsTimeInvalid)
			{
				log.Info("Cannot start work because client time is invalid (" + reason + ")");
				notificationService.ShowNotification(NotificationKeys.InvalidTimeCannotWork, nfInvalidTimeCannotWorkDuration,
					Labels.NotificationCannotStartUserSelectedWorkInvalidTimeTitle, Labels.NotificationCannotStartUserSelectedWorkInvalidTimeBody, CurrentWorkController.NotWorkingColor);
				return false;
			}

			return true;
		}

		private static TimeSpan GetValidDuration(int duration)
		{
			return duration > 0 ? TimeSpan.FromMilliseconds(duration) : TimeSpan.Zero;
		}

		public void ShowCreditRunOutNotification()
		{
			notificationService.HideNotification(NotificationKeys.CreditRunOut);
			switch (CreditRunOutState)
			{
				case CreditRunOutState.RunLow:
					notificationService.ShowNotification(NotificationKeys.CreditRunOut, nfCreditRunOutDuration,
														 Labels.CreditRunLowTitle,
														 string.Format(Labels.CreditRunLowBody, CreditRunOutRemainingDays));
					break;
				case CreditRunOutState.RunOut:
					notificationService.ShowNotification(NotificationKeys.CreditRunOut, nfCreditRunOutDuration,
														 Labels.CreditRunOutTitle,
														 Labels.CreditRunOutBody);
					break;
			}
		}

		public StateChangeTransaction StartStateChangeTransaction()
		{
			return new StateChangeTransaction(this);
		}

		public sealed class StateChangeTransaction : IDisposable
		{
			private readonly MutualWorkTypeCoordinator mutualCoordinator;

			public StateChangeTransaction(MutualWorkTypeCoordinator parent)
			{
				DebugEx.EnsureGuiThread();
				mutualCoordinator = parent;
				++mutualCoordinator.activeStateTransactions;
			}

			public void Dispose()
			{
				DebugEx.EnsureGuiThread();
				if ((--mutualCoordinator.activeStateTransactions) == 0)
				{
					mutualCoordinator.ReevaluateStates();
				}
			}
		}

	}
}
