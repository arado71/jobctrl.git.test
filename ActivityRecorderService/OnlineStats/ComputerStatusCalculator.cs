using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.OnlineStats
{
	/// <summary>
	/// Calculates computer online/offline status for a user
	/// </summary>
	/// <remarks>Only handles computer statuses atm. and doesn't care about deleted intervals</remarks>
	//we don't handle clock change of the user but that should be ok
	//(because it quite rare and the effect of the change should fade away shortly)
	//todo use workItem length (from ClientSettings) for calculating timeouts
	public class ComputerStatusCalculator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly TimeSpan maxClockSkew = TimeSpan.FromMinutes(7.6); //I've seen 5:52 diff despite the max 5 min clockskew
		private static readonly TimeSpan timeOutInterval = TimeSpan.FromMinutes(2);

		private readonly int userId;
		private readonly Dictionary<int, ComputerStatus> computerStatuses = new Dictionary<int, ComputerStatus>();

		public int UserId { get { return userId; } }

		public ComputerStatusCalculator(int userId)
		{
			this.userId = userId;
		}

		public DateTime GetUserTime(int computerId)
		{
			ComputerStatus currStatus;
			if (!computerStatuses.TryGetValue(computerId, out currStatus))
			{
				return DateTime.UtcNow;
			}
			return DateTime.UtcNow + currStatus.TimeSynchronizer.UserTimeDiff;
		}

		public int? GetCurrentWorkId(int computerId)
		{
			ComputerStatus currStatus;
			if (!computerStatuses.TryGetValue(computerId, out currStatus))
			{
				return null; //invalid computer - offline
			}
			if (currStatus.CurrentWork == null) return null; //offline
			var userTime = DateTime.UtcNow + currStatus.TimeSynchronizer.UserTimeDiff;
			if (userTime - currStatus.UpdateDate > timeOutInterval) return null; //timed out
			return currStatus.CurrentWork.Value; //online
		}

		public void AddWorkItem(WorkItem item)
		{
			//don't sync time with workitems atm.
			//WorkItem's item.EndDate is az open interval and Start/StopWrok's date is closed
			//hax to imitate this is the item.EndDate.AddTicks(-1)
			//this is requred because if we receive the last workItem and then the StopWork signal
			//the two dates are the same... but offline should be the status.
			//the same problem exits if we change works
			ChangeComputerWork(item.WorkId, item.ComputerId, item.EndDate.AddTicks(-1), null, item.ReceiveDate);
		}

		public void StartComputerWork(int workId, int computerId, DateTime createDate, DateTime userTime, DateTime serverTime)
		{
			ChangeComputerWork(workId, computerId, createDate, userTime, serverTime);
		}

		public void StopComputerWork(int computerId, DateTime createDate, DateTime userTime, DateTime serverTime)
		{
			ChangeComputerWork(null, computerId, createDate, userTime, serverTime);
		}

		private void ChangeComputerWork(int? workId, int computerId, DateTime createDate, DateTime? userTime, DateTime serverTime)
		{
			if (!IsValid(createDate)) return;
			ComputerStatus currStatus;
			if (!computerStatuses.TryGetValue(computerId, out currStatus))
			{
				currStatus = new ComputerStatus() { ComputerId = computerId, UpdateDate = createDate, CurrentWork = workId };
				computerStatuses.Add(computerId, currStatus);
				if (userTime.HasValue) currStatus.TimeSynchronizer.AddData(userTime.Value, serverTime);
				var timeDiffStr = " (TimeDiff: " + currStatus.TimeSynchronizer.UserTimeDiff.TotalMilliseconds.ToString("0") + "ms)";
				log.Debug("User " + userId + " status is " + (workId == null ? "Offline" : "Online (" + workId + ")") + " CompId: " + currStatus.ComputerId + timeDiffStr);
			}
			else if (currStatus.UpdateDate < createDate)
			{
				currStatus.UpdateDate = createDate;
				if (userTime.HasValue) currStatus.TimeSynchronizer.AddData(userTime.Value, serverTime);
				if (currStatus.CurrentWork != workId)
				{
					var timeDiffStr = " (TimeDiff: " + currStatus.TimeSynchronizer.UserTimeDiff.TotalMilliseconds.ToString("0") + "ms)";
					log.Debug("User " + userId + " changed status to " + (workId == null ? "Offline" : "Online (" + workId + ")") + " CompId: " + currStatus.ComputerId + timeDiffStr);
					currStatus.CurrentWork = workId;
				}
			}
			else
			{
				if (userTime.HasValue) currStatus.TimeSynchronizer.AddData(userTime.Value, serverTime);
				//old data should be ok (and quite common) when uploading offline data
				//log.Debug("User " + userId + " sent old status " + (workId == null ? "Offline" : "Online (" + workId + ")") + " but current status is " + (currStatus.CurrentWork == null ? "Offline" : "Online (" + currStatus.CurrentWork + ")"));
			}
		}

		private static bool IsValid(DateTime userTime)
		{
			return userTime - maxClockSkew < DateTime.UtcNow && DateTime.UtcNow < userTime + maxClockSkew;
		}

		private class ComputerStatus
		{
			public int? CurrentWork { get; set; }
			public int ComputerId { get; set; }
			public DateTime UpdateDate { get; set; }
			public readonly OnlineUserTimeSynchronizer TimeSynchronizer = new OnlineUserTimeSynchronizer(maxClockSkew);
		}
	}
}
