using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Model.Jira
{
	public class JiraProject
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "key")]
		public string Key { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		public Jc.Project ToJcProject()
		{
			return new Model.Jc.Project
			{
				Description = null, // Jira projects don't have description
				Duration = null,
				EndDate = null, // See ConvertProject remarks
				Id = -1,
				IsClosed = false,
				Name = Name,
				Parent = null,
				StartDate = null, // See ConvertProject remarks
				ChildrenProjects = new List<Model.Jc.Project>(),
				ChildrenTasks = new List<Model.Jc.Task>(),

			};
		}
	}
}
