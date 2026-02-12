using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Model.Jira
{
	public class JiraStatusCategory
	{
		private const string CLOSED_STATUS_CATEGORY_NAME = "Done";

		public bool IsClosedCategory
		{
			get
			{
				return string.Equals(Key, CLOSED_STATUS_CATEGORY_NAME, StringComparison.OrdinalIgnoreCase);
			}
		}
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }
		[JsonProperty(PropertyName = "key")]
		public string Key { get; set; }
		[JsonProperty(PropertyName = "colorName")]
		public string ColorName { get; set; }
	}
}
