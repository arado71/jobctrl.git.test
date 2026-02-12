using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using log4net;

namespace Tct.ActivityRecorderService.Stats
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class DailyStats : IFilterableStats<DailyStats>
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		[DataMember]
		public DateTime Date { get; set; }
		[DataMember]
		public List<UserStats> Users { get; set; }

		private readonly Dictionary<UserId, UserStats> userLookup = new Dictionary<UserId, UserStats>();

		public DailyStats()
		{
			Users = new List<UserStats>();
		}

		public void AddWorkItem(WorkItem item)
		{
			var user = GetUser(item.UserId, item.GroupId, item.CompanyId);
			user.AddWorkItem(item);
			//LogUserStats(user);
		}

		public void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			//hax don't create entry if we have only ManualWorkItems because they are not handled in UserWorkStats atm. (i.e. NullRef will be thrown)
			var user = GetUser(manualWorkItem.UserId, manualWorkItem.GroupId, manualWorkItem.CompanyId, false);
			if (user == null) return;
			user.AddManualWorkItem(manualWorkItem);
		}

		public WorkTimeStats GetWorkTimeStatsForUser(int userId, int groupId, int companyId)
		{
			var uid = new UserId(userId, groupId, companyId);
			UserStats user;
			if (!userLookup.TryGetValue(uid, out user))
			{
				return null;
			}
			return user.GetWorkTimeStats();
		}

		private UserStats GetUser(int userId, int groupId, int companyId)
		{
			return GetUser(userId, groupId, companyId, true);
		}

		private UserStats GetUser(int userId, int groupId, int companyId, bool forceCreation)
		{
			var uid = new UserId(userId, groupId, companyId);
			UserStats user;
			if (!userLookup.TryGetValue(uid, out user))
			{
				if (!forceCreation) return null;
				user = new UserStats() { UserId = userId, GroupId = groupId, CompanyId = companyId };
				userLookup.Add(uid, user);
				Users.Add(user);
			}
			Debug.Assert(user.UserId == userId);
			Debug.Assert(user.GroupId == groupId);
			Debug.Assert(user.CompanyId == companyId);
			return user;
		}

		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		private class UserId : IEquatable<UserId>
		{
			private readonly int userId;
			private readonly int groupId;
			private readonly int companyId;

			public UserId(int userId, int groupId, int companyId)
			{
				this.userId = userId;
				this.groupId = groupId;
				this.companyId = companyId;
			}

			public bool Equals(UserId other)
			{
				return this.userId == other.userId
					&& this.groupId == other.groupId
					&& this.companyId == other.companyId;
			}

			public override bool Equals(object obj)
			{
				if (Object.ReferenceEquals(obj, null))
					return false;
				if (Object.ReferenceEquals(this, obj))
					return true;

				if (obj.GetType() != this.GetType())
					return false;

				return this.Equals(obj as UserId);
			}

			public override int GetHashCode()
			{
				return int.MinValue + userId + (groupId << 20) + companyId;
			}

			public static bool operator ==(UserId left, UserId right)
			{
				if (Object.ReferenceEquals(left, null))
					return Object.ReferenceEquals(right, null);
				return left.Equals(right);
			}

			public static bool operator !=(UserId left, UserId right)
			{
				return !(left == right);
			}
		}

		public DailyStats GetFilteredCopy(StatsFilter filter)
		{
			var result = new DailyStats() { Date = this.Date };
			foreach (var user in Users)
			{
				if (user.SatisfiesFilter(filter))
				{
					result.Users.Add(user.GetFilteredCopy(filter));
				}
			}
			return result;
		}

		public bool SatisfiesFilter(StatsFilter filter)
		{
			return true;
		}

		//for testing...
		// ReSharper disable UnusedMember.Local
		private static void LogUserStats(UserStats user)
		// ReSharper restore UnusedMember.Local
		{
			user = user.GetFilteredCopy(new StatsFilter(null, null, null));
			log.Warn("CW:" + user.CurrentWork.WorkId + " (" + TimeSpanToString(user.CurrentWork.WorkTime) + ")"
				+ " RW:" + string.Join(", ", user.RecentWorks.Select(n => "" + n.WorkId + "(" + TimeSpanToString(n.WorkTime) + ")").ToArray())
				+ " CT:" + TimeSpanToString(user.WorkTimeStats.ComputerWorkTime)
				+ " ST:" + TimeSpanToString(user.WorkTimeStats.SumWorkTime)
				+ " K,M(AK,AM):" + user.LastKeyboardActivity + "," + user.LastMouseActivity + "(" + user.AverageKeyboardActivity.ToString("0.00") + "," + user.AverageMouseActivity.ToString("0.00") + ")"
				+ " S:" + user.Status
				);
		}

		private static string TimeSpanToString(TimeSpan timeSpan)
		{
			return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Days * 24 + timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}
	}
}
