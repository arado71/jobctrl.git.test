using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Model.Jira
{
	public class JiraUser
	{
		[JsonProperty(PropertyName = "name")]
		public string Username { get; set; }

		[JsonProperty(PropertyName = "emailAddress")]
		public string Email { get; set; }

		[JsonProperty(PropertyName = "accountId")]
		public string Key { get; set; }

		[JsonProperty(PropertyName = "displayName")]
		public string DisplayName { get; set; }

		public Jc.User ToJcUser()
		{
			string[] nameParts = DisplayName.Split(' ');
			return new Jc.User()
			{
				Email = Email,
				FirstName = nameParts.FirstOrDefault(),
				LastName = nameParts.Length > 0 ? string.Join(" ", nameParts.Skip(1)) : null
			};
		}
	}
}
