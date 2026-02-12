using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiraSyncTool.Jira.Utils;

namespace JiraSyncTool.Jira.Model.Jira
{
	public class JiraWorklog
	{
		[JsonIgnore]
		public string IssueKey { get; set; }

		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }
		
		[JsonIgnore]
		public long TimeSpentSeconds { get; set; }

		[JsonProperty(PropertyName = "author")]
		private AuthorClass AuthorObject{ get; set; }

		[JsonProperty(PropertyName = "started")]
		[JsonConverter(typeof(DateFixupJsonConverter))]
		public DateTime StartDate { get; set; }

		[JsonProperty(PropertyName = "timeSpent")]
		public string TimeSpent { get; set; }

		[JsonProperty(PropertyName = "comment")]
		public string Comment { get; set; }
		[JsonIgnore]
		public string Author { get { return AuthorObject.Key; } set { if (AuthorObject == null) AuthorObject = new AuthorClass(); AuthorObject.Key = value; } }
		private class AuthorClass
		{
			[JsonProperty(PropertyName = "accountId")]
			public string Key { get; set; }
		}

		public class FullEqualityComparer : IEqualityComparer<JiraWorklog>
		{
			public bool Equals(JiraWorklog x, JiraWorklog y)
			{
				if ((x == null && y != null) || (x != null && y == null)) return false;
				if (x == null && y == null) return true;
				if (x.Equals(y)) return true;
				if (x.IssueKey != y.IssueKey) return false;
				if (x.Author != y.Author) return false;

				if (x.StartDate != y.StartDate) return false;
				if (x.TimeSpent != y.TimeSpent) return false;
				if (x.Comment != y.Comment) return false;
				return true;
			}

			public int GetHashCode(JiraWorklog obj)
			{
				return obj == null ? 0 : obj.IssueKey.GetHashCode();
			}
		}

		/// <summary>
		/// Helper class to filter deletable worklogs.
		/// </summary>
		internal class StartDateEqualityComparer : IEqualityComparer<JiraWorklog>
		{
			public bool Equals(JiraWorklog x, JiraWorklog y)
			{
				if ((x == null && y != null) || (x != null && y == null)) return false;
				if (x == null && y == null) return true;
				// if the two reference is the same, then return false
				if (x.Equals(y)) return false;
				if (x.IssueKey != y.IssueKey) return false;
				if (x.Author != y.Author) return false;

				if (x.StartDate != y.StartDate) return false;
				return true;
			}
			public int GetHashCode(JiraWorklog obj)
			{
				return obj == null ? 0 : obj.IssueKey.GetHashCode();
			}
		}
	}
}
