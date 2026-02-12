using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Communication;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class ClientKickManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly object thisLock = new object();
		private const int slowInterval = 30 * 1000;  //30 secs /**/8 GetPendingKick 168 bytes/call inside, in variables; but 60 packets 23941 bytes/call outside, in Ethernet packets
		private const int fastInterval = 1000;  //1 sec
		private bool isFast;
		private KickManagerState currentState = KickManagerState.GetPending;
		private ConfirmData confirmToSend;

		public event EventHandler<SingleValueEventArgs<ClientComputerKick>> ClientKicked;

		protected override int ManagerCallbackInterval
		{
			get { return isFast ? fastInterval : slowInterval; }
		}

		public ClientKickManager()
			: base(log)
		{
		}

		private void SetState(KickManagerState state, ConfirmData confData = null)
		{
			Debug.Assert(
				(confData != null && state == KickManagerState.SendConfirm)
				|| (confData == null && (state == KickManagerState.GetPending || state == KickManagerState.WaitForConfirm))
				);
			lock (thisLock)
			{
				Debug.Assert(
					(currentState == KickManagerState.GetPending && state == KickManagerState.WaitForConfirm)
					|| (currentState == KickManagerState.WaitForConfirm && state == KickManagerState.SendConfirm)
					|| (currentState == KickManagerState.SendConfirm && state == KickManagerState.GetPending)
					);
				currentState = state;
				confirmToSend = confData;
			}
		}

		private KickManagerState GetState(out ConfirmData confData)
		{
			lock (thisLock)
			{
				confData = currentState == KickManagerState.SendConfirm ? confirmToSend : null;
				return currentState;
			}
		}

		protected override void ManagerCallbackImpl()
		{
			ConfirmData confData;
			var currState = GetState(out confData);
			if (currState == KickManagerState.WaitForConfirm) return;
			try
			{
				var userId = ConfigManager.UserId;
				var compId = ConfigManager.EnvironmentInfo.ComputerId;
				switch (currState)
				{
					case KickManagerState.SendConfirm:
						Debug.Assert(isFast);
						ActivityRecorderClientWrapper.Execute(n => n.ConfirmKick(userId, compId, confData.KickId, confData.Result));
						log.Info("Client kick confimed (" + confData.KickId + "):" + confData.Result);
						SetState(KickManagerState.GetPending); //in theory there could be a race here if there is already a new confirm request and confirmToSend.KickId != confData.KickId but we won't receive Confirms in that way
						break;
					case KickManagerState.GetPending:
						var kick = ActivityRecorderClientWrapper.Execute(n => n.GetPendingKick(userId, compId));
						isFast = false;
						if (kick != null)
						{
							isFast = true;
							SetState(KickManagerState.WaitForConfirm);
							log.Info("Client kicked (" + kick.Id + ") '" + kick.Reason + "' by " + kick.CreatedByName + " (" + kick.CreatedBy + ")");
							OnClientKicked(kick); //we have to make sure that we receive a confirm (and only one) after this is raised
						}
						break;
					default:
						Debug.Fail("Invalid state");
						break;
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("communicate with the server", log, ex);
				isFast = true; //retry shortly
			}
		}

		public void ConfirmKickAsync(int kickId, KickResult result)
		{
			SetState(KickManagerState.SendConfirm, new ConfirmData(kickId, result));
		}

		private enum KickManagerState //Valid transitions are: GetPending -> WaitForConfirm -> SendConfirm -> GetPending ...etc
		{
			GetPending = 0,
			WaitForConfirm = 1,
			SendConfirm = 2,
		}

		private class ConfirmData
		{
			public readonly int KickId;
			public readonly KickResult Result;

			public ConfirmData(int kickId, KickResult result)
			{
				KickId = kickId;
				Result = result;
			}
		}

		private void OnClientKicked(ClientComputerKick kick)
		{
			var kicked = ClientKicked;
			if (kicked != null) kicked(this, SingleValueEventArgs.Create(kick));
		}

		public void MakeClientActiveAsync(bool isActive)
		{
			ThreadPool.QueueUserWorkItem(_ =>
				{
					Stop();
					lock (thisLock)
					{
						if (currentState == KickManagerState.GetPending || currentState == KickManagerState.SendConfirm) // sending confirmation allowed
						{
							ManagerCallbackImpl(); // check pending (or send last confirmation)
							if (currentState == KickManagerState.GetPending) // still not kicked
								try
								{
									var userId = ConfigManager.UserId;
									var compId = ConfigManager.EnvironmentInfo.ComputerId;
									ActivityRecorderClientWrapper.Execute(n => n.MakeClientActive(userId, compId, isActive));
									log.Info("Client has been made " + (isActive ? "active" : "inactive"));
								}
								catch (Exception ex)
								{
									WcfExceptionLogger.LogWcfError("communicate with the server", log, ex);
								}
						}
					}
					Start(ManagerCallbackInterval);
				});
		}
	}
}
