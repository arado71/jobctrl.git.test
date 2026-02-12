using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Model.Jira
{
	public class JiraStatus
	{
		[JsonProperty(PropertyName="description")]
		public string Description { get; set; }
		[JsonProperty(PropertyName = "iconUrl")]
		public string IconUrl { get; set; }
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }
		[JsonProperty(PropertyName = "statusCategory")]
		public JiraStatusCategory StatusCategory { get; set; }
		public bool IsClosed { get { return StatusCategory.IsClosedCategory; } }
	}
}
