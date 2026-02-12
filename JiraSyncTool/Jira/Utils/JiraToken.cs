using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Utils
{
	class JiraToken
	{
		public string Token { get; set; }

		public DateTime IssDate { get; set; }

		public DateTime ExpirationDate { get; set; }
	}
}
