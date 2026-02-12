using log4net;
using System;
using System.Collections.Generic;

namespace JiraSyncTool.Jira.Model.Jc
{
	public class Assignment : IEquatable<Assignment>
	{

		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly IEqualityComparer<Assignment> UserTaskComparer = new UserTaskEqualityComparer();

		public static readonly Assignment[] EmptyArray = new Assignment[0];

		public User User { get; set; }
		public Task Task { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public TimeSpan? Duration { get; set; }

		public bool Equals(Assignment other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(other, this)) return true;
			return !ReferenceEquals(User, null) && User.Equals(other.User) && !ReferenceEquals(Task, null) && Task.Equals(other.Task);
		}

		public bool TryUpdate(Assignment other)
		{
			var requireUpdate = false;
			if (other.StartDate != StartDate)
			{
				log.DebugFormat("StartDate changed: {0} -> {1}", StartDate, other.StartDate);
				StartDate = other.StartDate;
				requireUpdate = true;
			}

			if (other.EndDate != EndDate)
			{
				log.DebugFormat("EndDate update is required: {0} -> {1}", EndDate, other.EndDate);
				EndDate = other.EndDate;
				requireUpdate = true;
			}

			if (other.Duration != Duration)
			{
				log.DebugFormat("Duration update is required for: {0} -> {1}", Duration, other.Duration);
				Duration = other.Duration;
				requireUpdate = true;
			}

			return requireUpdate;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Assignment);
		}

		public override int GetHashCode()
		{
			return User.GetHashCode() + 13 * Task.GetHashCode();
		}

		public override string ToString()
		{
			return (User != null ? User.ToString() : "NULL") + " - " + (Task != null ? Task.ToString() : "NULL");
		}

		public class UserTaskEqualityComparer : IEqualityComparer<Assignment>
		{
			public bool Equals(Assignment x, Assignment y)
			{
				if (ReferenceEquals(x, null) ^ ReferenceEquals(y, null)) return false;
				if (ReferenceEquals(x, y)) return true;
				return x.User != null && y.User != null && x.User.Id == y.User.Id && x.Task != null && y.Task != null &&
					   x.Task.Id == y.Task.Id;
			}

			public int GetHashCode(Assignment obj)
			{
				var h = 27;
				h += (obj.User != null ? obj.User.Id : 1);
				return h * 13 + (obj.Task != null ? obj.Task.Id : 1);
			}
		}
	}
}
