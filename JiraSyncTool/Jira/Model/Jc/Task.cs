using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace JiraSyncTool.Jira.Model.Jc
{
	public class Task : IId, IEquatable<Task>
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly IEqualityComparer<Task> IdComparer = new IdEqualityComparer();
		public static readonly IEqualityComparer<Task> NameComparer = new NameEqualityComparer();

		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public TimeSpan? Duration { get; set; }
		public List<Assignment> Assignments { get; set; }
		public int? Priority { get; set; }
		public Project Parent { get; set; }
		public bool IsClosed { get; set; }
		public bool IsFromServer { get { return Id != -1; } }
		public string ExtId { get; set; }
		public string Description { get; set; }

		public bool TryUpdate(Task other)
		{
			var requireUpdate = false;
			if (!string.Equals(Name, other.Name?.Trim(), StringComparison.CurrentCulture))
			{
				log.DebugFormat("Name changed: {0} -> {1}", Name, other.Name);
				Name = other.Name;
				requireUpdate = true;
			}

			// StartDate, EndDate has to be null in JC server
			if (StartDate != null)
			{
				log.DebugFormat("StartDate changed: {0} -> {1}", StartDate, "null");
				StartDate = null;
				requireUpdate = true;
			}

			if (EndDate != null)
			{
				log.DebugFormat("EndDate changed: {0} -> {1}", EndDate, "null");
				EndDate = null;
				requireUpdate = true;
			}

			if (Duration != null)
			{
				log.DebugFormat("Duration change: {0} -> {1}", Duration, null);
				Duration = null;
				requireUpdate = true;
			}

			if (Priority != other.Priority && other.Priority != null)
			{
				log.DebugFormat("Priority changed: {0} -> {1}", Priority, other.Priority);
				Priority = other.Priority;
				requireUpdate = true;
			}

			if (!string.Equals(Description, Regex.Replace(other.Description ?? "", @"\r\n", "\n"), StringComparison.CurrentCulture))
			{
				log.DebugFormat("Description change: {0} -> {1}", Description, other.Description);
				Description = other.Description;
				requireUpdate = true;
			}

			return requireUpdate;
		}

		public bool Equals(Task other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(other, this)) return true;
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Task);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return Name + " (" + Id + ")";
		}

		public class IdEqualityComparer : IEqualityComparer<Task>
		{
			public bool Equals(Task x, Task y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
				return x.Id != -1 && y.Id != -1 && x.Id == y.Id;
			}

			public int GetHashCode(Task obj)
			{
				return obj.GetHashCode();
			}
		}

		public class NameEqualityComparer : IEqualityComparer<Task>
		{
			public bool Equals(Task x, Task y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
				return string.Equals(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
			}

			public int GetHashCode(Task obj)
			{
				return obj.Name.GetHashCode();
			}
		}
	}
}
