using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.Serialization;
using log4net;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.SystemEvents;
using Tct.ActivityRecorderClient.Forms;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Thread-safe class for getting the date and time of the server, in order to calculate clock skew.
	/// </summary>
	/// <remarks>
	/// On OSX the Environment.TickCount will stop increasing when the computer is asleep. So that complicate things.
	/// </remarks>
	public class TimeManager : PeriodicManager, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int managerCallbackInterval = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
		private static readonly int reSyncInterval = (int)TimeSpan.FromHours(1).TotalMilliseconds;
		private static readonly long clockSkewWarnTicks = TimeSpan.FromMinutes(5).Ticks;

		public event EventHandler<SingleValueEventArgs<ClockSkewData>> ClockSkewError;
		public DateTime? ServerTime { get { return DateTime.UtcNow - GetCalculatedServerTimeDiff(); } }
		public bool IsTimeInvalid { get { return GetIsTimeInvalid(); } }

		private readonly ISystemEventsService systemEventsService;
		private readonly object thisLock = new object(); //for serverBaseTime and serverBaseTickCount
		private DateTime serverBaseTime = DateTime.MinValue; //this is needed so we can calculate the server time from TickCount
		private int serverBaseTickCount = Environment.TickCount - reSyncInterval - 1;

		public TimeManager(ISystemEventsService systemEvents)
			: base(log, false)
		{
			if (systemEvents == null) throw new ArgumentNullException();
			systemEventsService = systemEvents;
			systemEventsService.PowerModeChanged += SystemEventsServicePowerModeChanged;
		}

		private void SystemEventsServicePowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			//on Macs including Windows running on Parallels Desktop TickCount will stop counting in Sleep (which would cause problems)
			if (Environment.OSVersion.Platform == PlatformID.Win32NT && !ConfigManager.EnvironmentInfo.IsVirtualMachine) return; //IsVirtualMachine is thread-safe
			if (e.Mode != PowerModes.Suspend && e.Mode != PowerModes.Resume) return;
			InvalidateServerTime();
		}

		private void InvalidateServerTime()
		{
			lock (thisLock)
			{
				serverBaseTime = DateTime.MinValue;
			}
			log.Debug("Time of the server is invalidated");
		}

		protected override int ManagerCallbackInterval
		{
			get { return managerCallbackInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				bool updateNeeded;
				var srvTimeDiff = GetCalculatedServerTimeDiff();
				lock (thisLock)
				{
					updateNeeded = serverBaseTime == DateTime.MinValue //no data
						|| (uint)(Environment.TickCount - serverBaseTickCount) > reSyncInterval // or stale data
						|| Math.Abs(srvTimeDiff.Value.Ticks) > clockSkewWarnTicks; //handle when the VM suspension messes up the clocks
				}
				if (updateNeeded)
				{
					UpdateServerTime();
				}
				srvTimeDiff = GetCalculatedServerTimeDiff(); //there is a race here but that is ok we run this quite frequently
				LogDiffChangeIfApplicable(srvTimeDiff);
				if (!srvTimeDiff.HasValue) return;
				if (Math.Abs(srvTimeDiff.Value.Ticks) > clockSkewWarnTicks)
				{
					OnClockSkewError(new ClockSkewData() { ClientTime = DateTime.Now, ServerTime = DateTime.Now - srvTimeDiff.Value });
				}
			}
			finally
			{
				//prevent overflow
				Debug.Assert(reSyncInterval > managerCallbackInterval);
				lock (thisLock)
				{
					if (serverBaseTime != DateTime.MinValue
						&& (uint)(Environment.TickCount - serverBaseTickCount) > int.MaxValue)
					{
						serverBaseTickCount += reSyncInterval;
						serverBaseTime += TimeSpan.FromMilliseconds(reSyncInterval);
					}
				}
			}
		}

		private static readonly long logDiffChangeThresholdTicks = TimeSpan.FromMinutes(1).Ticks;
		private TimeSpan? lastSrvTimeDiff;
		private void LogDiffChangeIfApplicable(TimeSpan? srvTimeDiff)
		{
			if (srvTimeDiff == null)
			{
				log.Debug("Time diff is: N/A");
				lastSrvTimeDiff = null;
				return;
			}
			if (lastSrvTimeDiff == null
				|| Math.Abs(lastSrvTimeDiff.Value.Ticks - srvTimeDiff.Value.Ticks) > logDiffChangeThresholdTicks
				|| (Math.Abs(lastSrvTimeDiff.Value.Ticks) > clockSkewWarnTicks && Math.Abs(srvTimeDiff.Value.Ticks) <= clockSkewWarnTicks)
				|| (Math.Abs(lastSrvTimeDiff.Value.Ticks) <= clockSkewWarnTicks && Math.Abs(srvTimeDiff.Value.Ticks) > clockSkewWarnTicks)
				)
			{
				log.Info("Time diff is: " + srvTimeDiff.ToHourMinuteSecondString());
				lastSrvTimeDiff = srvTimeDiff;
			}
		}

		private void UpdateServerTime() //this is not too accurate but good enough for us
		{
			try
			{
				var clientTime = DateTime.UtcNow;
				var start = Environment.TickCount;
				var srvTime = ActivityRecorderClientWrapper.Execute(n =>
				{
					// TODO: mac
					//if (n.State == System.ServiceModel.CommunicationState.Created) n.Open(); //avoid async round trip as much as possible
					//we might have some delay until we reach this point (until we can acquire a proxy object) so capture time here
					clientTime = DateTime.UtcNow;
					start = Environment.TickCount;
					return n.GetServerTime(ConfigManager.UserId, ConfigManager.EnvironmentInfo.ComputerId, clientTime);
				});
				var roundTrip = (Environment.TickCount - start) / 2;
				lock (thisLock)
				{
					serverBaseTickCount = start + roundTrip;
					serverBaseTime = srvTime;
				}
				var cliTime = clientTime + TimeSpan.FromMilliseconds(roundTrip);
				log.DebugFormat("Client time was: {0} when the server time was: {1} (diff: {2}ms, rt: {3}ms)", cliTime, srvTime, (int)(cliTime - srvTime).TotalMilliseconds, roundTrip);
			}
			catch (Exception ex)
			{
				ClockSkewData clockData;
				if (ClockSkewHelper.IsClockSkewException(ex, out clockData))
				{
					//we have to update state... it won't be accurate but probably good enough
					if (clockData.ClientTime > clockData.ServerTime) clockData.ServerTime -= TimeSpan.FromTicks(clockSkewWarnTicks); //don't ask me why this is needed
					var diff = clockData.ClientTime - clockData.ServerTime; //dangerous local times...
					if (Math.Abs(diff.Ticks) <= clockSkewWarnTicks - 300) //we have to make sure that the user will be notified (but this code should never run unless the config is modified)
					{
						diff = diff.Ticks > 0 ? TimeSpan.FromTicks(clockSkewWarnTicks + 300) : TimeSpan.FromTicks(-clockSkewWarnTicks - 300); //300 to make sure that GetCalculatedServerTimeDiff() will return big enough diff
						log.Error("Clock skew was too litle so modifying it, Client: " + clockData.ClientTime + " Server: " + clockData.ServerTime);
						Debug.Fail("Clock skew was too litle");
					}
					lock (thisLock)
					{
						serverBaseTickCount = Environment.TickCount;
						serverBaseTime = DateTime.UtcNow - diff;
					}
					log.InfoFormat("ClockSkew error but Client time was: {0} when the server time was: {1}", clockData.ClientTime, clockData.ServerTime);
				}
				else
				{
					WcfExceptionLogger.LogWcfError("get server time", log, ex);
				}
			}
		}

		private TimeSpan? GetCalculatedServerTimeDiff()
		{
			lock (thisLock)
			{
				if (serverBaseTime == DateTime.MinValue) return null;
				var serverTime = serverBaseTime + TimeSpan.FromMilliseconds((uint)(Environment.TickCount - serverBaseTickCount));
				return DateTime.UtcNow - serverTime;
			}
		}

		//returns true if the user's clock is invalid for sure, if we don't know for sure or it's valid then returns false
		private bool GetIsTimeInvalid()
		{
			var srvTimeDiff = GetCalculatedServerTimeDiff();
			if (!srvTimeDiff.HasValue) return false; //we don't know
			return Math.Abs(srvTimeDiff.Value.Ticks) > clockSkewWarnTicks;
		}

		private void OnClockSkewError(ClockSkewData clockData)
		{
			var del = ClockSkewError;
			if (del == null) return;
			del(this, SingleValueEventArgs.Create(clockData));
		}

		private int isDisposed;
		public void Dispose()
		{
			if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
			systemEventsService.PowerModeChanged -= SystemEventsServicePowerModeChanged;
		}
	}
}
