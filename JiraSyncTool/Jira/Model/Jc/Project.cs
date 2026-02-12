using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace JiraSyncTool.Jira.Model.Jc
{
	public class Project : IId, IEquatable<Project>
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly IEqualityComparer<Project> IdComparer = new IdEqualityComparer();
		public static readonly IEqualityComparer<Project> NameComparer = new NameEqualityComparer();

		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public Project Parent { get; set; }
		public List<Project> ChildrenProjects { get; set; }
		public List<Task> ChildrenTasks { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public TimeSpan? Duration { get; set; }
		public bool IsClosed { get; set; }
		public bool IsFromServer { get { return Id != -1; } }
		public string ExtId { get; set; }
		public int? Priority { get; set; }

		public bool TryUpdate(Project target)
		{
			var updated = false;
			if (!string.Equals(Name, target.Name?.Trim(), StringComparison.CurrentCulture))
			{
				log.DebugFormat("Project {0} name changed: {1} -> {2}", Id, Name, target.Name);
				Name = target.Name;
				updated = true;
			}

			// If one of the strings is null, the other is empty, then don't update
			if (!(string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(target.Description)) &&
			    !string.Equals(Description, Regex.Replace(target.Description ?? "", @"\r\n", "\n"), StringComparison.CurrentCulture))
			{
				log.DebugFormat("Project {0} description changed: {1} -> {2}", Id, Description, target.Description);
				Description = target.Description;
				updated = true;
			}

			// StartDate, EndDate will be null in JC server, so we don't care about these values.
			//if (StartDate != target.StartDate)
			//{
			//	log.DebugFormat("Project {0} startdate changed: {1} -> {2}", Id, StartDate, target.StartDate);
			//	StartDate = target.StartDate;
			//	updated = true;
			//}

			//if (EndDate != target.EndDate)
			//{
			//	log.DebugFormat("Project {0} enddate changed: {1} -> {2}", Id, StartDate, EndDate);
			//	EndDate = target.EndDate;
			//	updated = true;
			//}

			if (Duration != target.Duration)
			{
				log.DebugFormat("Project {0} duration changed: {1} -> {2}", Id, Duration, target.Duration);
				Duration = target.Duration;
				updated = true;
			}
			if (Priority != target.Priority)
			{
				log.DebugFormat("Project {0} priority changed: {1} -> {2}", Id, Priority, target.Priority);
				Priority = target.Priority;
				updated = true;
			}

			return updated;
		}

		public bool Equals(Project other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(other, this)) return true;
			return Id != -1 && other.Id != -1 && Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Project);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return Name + " (" + Id + ")";
		}

		public class IdEqualityComparer : IEqualityComparer<Project>
		{
			public bool Equals(Project x, Project y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
				return x.Id != -1 && y.Id != -1 && x.Id == y.Id;
			}

			public int GetHashCode(Project obj)
			{
				return obj.GetHashCode();
			}
		}

        public class NameEqualityComparer : IEqualityComparer<Project> {
            public bool Equals(Project x, Project y) {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
                return string.Equals(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
            }

            public int GetHashCode(Project obj) {
                return obj.Name.GetHashCode();
            }
        }
        public class EndDateEqualityComparer : IEqualityComparer<Project> {
            public bool Equals(Project x, Project y) {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
                return x.EndDate.Equals(y.EndDate);
            }

            public int GetHashCode(Project obj) {
                return obj.Name.GetHashCode();
            }
        }
    }
}
