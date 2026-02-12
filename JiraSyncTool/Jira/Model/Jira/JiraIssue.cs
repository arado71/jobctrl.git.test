using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Model.Jira
{
	public class JiraIssue
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "key")]
		public string Key { get; set; }

		[JsonProperty(PropertyName = "fields")]
		private Fields fields = new Fields();

		[JsonIgnore]
		public long? OriginalEstimateSeconds { get { return fields.OriginalEstimateSeconds; } set { fields.OriginalEstimateSeconds = value; } }

		[JsonIgnore]
		public JiraProject Project { get { return fields.Project; } set { fields.Project = value; } }

		[JsonIgnore]
		public string Description { get { return fields.Description; } set { fields.Description = value; } }
		
		[JsonIgnore]
		public DateTime? DueDate { get { return fields.DueDate; } set { fields.DueDate = value; } }

		[JsonIgnore]
		public DateTime? Created { get { return fields.Created; } set { fields.Created = value; } }

		[JsonIgnore]
		public string Summary { get { return fields.Summary; } set { fields.Summary = value; } }

		[JsonIgnore]
		public JiraIssuePriority Priority { get { return fields.Priority; } set { fields.Priority = value; } }

		[JsonProperty(PropertyName = "issuetype")]
		public JiraIssueType IssueType { get; set; }

		[JsonIgnore]
		public JiraStatus Status { get { return fields.Status; } set { fields.Status = value; } }

		[JsonIgnore]
		public string ParentKey { get { return fields.Parent.Key; } set { fields.Parent.Key = value; } }

		[JsonIgnore]
		public DateTime? StartDate { get; set; }

		[JsonProperty(PropertyName = "assignee")]
		public JiraUser Assignee { get { return fields.Assignee; } set { fields.Assignee = value; } }

		public class Parent
		{
			[JsonProperty(PropertyName="key")]
			public string Key { get; set; }
		}

		public class Fields
		{
			[JsonIgnore]
			public Parent Parent { get { return parent; } set { parent = value; } }

			[JsonProperty(PropertyName = "parent")]
			private Parent parent = new Parent();

			[JsonProperty(PropertyName = "assignee")]
			public JiraUser Assignee { get; set; }

			[JsonProperty(PropertyName = "timeoriginalestimate")]
			public long? OriginalEstimateSeconds{get;set;}

			[JsonProperty(PropertyName = "project")]
			public JiraProject Project { get; set; }

			[JsonProperty(PropertyName = "description")]
			public string Description { get; set; }

			[JsonProperty(PropertyName = "dueDate")]
			public DateTime? DueDate { get; set; }

			[JsonProperty(PropertyName = "created")]
			public DateTime? Created { get; set; }

			[JsonProperty(PropertyName = "summary")]
			public string Summary { get; set; }

			[JsonProperty(PropertyName="priority")]
			public JiraIssuePriority Priority { get; set; }

			[JsonProperty(PropertyName = "status")]
			public JiraStatus Status { get; set; }
		}

		/// <summary>
		/// Returns a new JC Task from a Jira issue. The Assignment, Parent properties will not be set here.
		/// </summary>
		/// <returns></returns>
		public Jc.Task ToJcTask()
		{
			//TimeSpan duration = new TimeSpan();
			//if (jiraIssue.DueDate != null && jiraIssue.Created != null)
			//{
			//	duration = jiraIssue.DueDate.Value.Subtract(jiraIssue.Created.Value);
			//}
			string description = Regex.Replace(Description ?? "", @"\r\n", "\n");
			description = description.Substring(0, description.Length > 1000 ? 1000 : description.Length);
			string summary = Summary.Substring(0, Summary.Length > 100 ? 100 : Summary.Length);
			return new Jc.Task()
			{
				Assignments = new List<Jc.Assignment>(),
				Description = description,
				Duration = OriginalEstimateSeconds.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(OriginalEstimateSeconds.Value) : null,
				EndDate = Created.HasValue? DueDate?.Date : null,
				Id = -1,
				IsClosed = false,
				Name = summary,
				Parent = null,
				Priority = Priority == null ? null : (int?)Model.Jira.PriorityConverter.ConvertJiraPriorityToJcPriority(Priority.Name),
				StartDate = DueDate.HasValue ? StartDate ?? Created?.Date : null,
			};
		}

		/// <summary>
		/// Gets a new JC Project from a Jira issue instance. The Parent, ChildrenProjects, ChildrenTasks will not be set.
		/// </summary>
		/// <returns></returns>
		public Jc.Project ToJcProject()
		{
			string description = Regex.Replace(Description ?? "", @"\r\n", "\n");
			description = description.Substring(0, description.Length > 1000 ? 1000 : description.Length);
			string summary = Summary.Substring(0, Summary.Length > 100 ? 100 : Summary.Length);
			return new Jc.Project()
			{
				Description = description,
				Duration = OriginalEstimateSeconds.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(OriginalEstimateSeconds.Value) : null,
				EndDate = Created.HasValue ? DueDate?.Date : null,
				Id = -1,
				IsClosed = false,
				Name = summary,
				Parent = null,
				Priority = Model.Jira.PriorityConverter.ConvertJiraPriorityToJcPriority(Priority.Name),
				StartDate = DueDate.HasValue ? StartDate ?? Created?.Date : null,
				ChildrenProjects = new List<Jc.Project>(),
				ChildrenTasks = new List<Jc.Task>()
			};
		}
	}
}
