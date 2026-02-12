using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Utils
{
	public class JiraConfig : IEquatable<JiraConfig>
	{
		public string WorklogComment { get; set; } = "From JobCTRL\n";
		public string JiraAuthServer { get; set; } = "https://oauth-2-authorization-server.services.atlassian.com";
		public string JiraBaseUrl { get; set; } = "https://jobctrl.atlassian.net";
		public string JiraAppKey { get; set; } = "com.jobctrl.jira";
		public string JiraOAuthClientId { get; set; } = "";
		public string JiraSharedSecret { get; set; } = "";
		public int JiraWorklogSyncInterval { get; set; } = 30 * 6;
		public int JcFullSyncInterval { get; set; } = 30 * 2;
		public int TargetRootProjectId { get; set; } = 0;
		public int CompanyId { get; set; }
		public Guid CompanyAuthCode { get; set; }
		public bool IsServerJira { get; set; }

		public bool Equals(JiraConfig other)
		{
			if (other == null) return false;
			if (!WorklogComment.Equals(other.WorklogComment)) return false;
			if (!JiraAuthServer.Equals(other.JiraAuthServer)) return false;
			if (!JiraBaseUrl.Equals(other.JiraBaseUrl)) return false;
			if (!JiraAppKey.Equals(other.JiraAppKey)) return false;
			if (!JiraOAuthClientId.Equals(other.JiraOAuthClientId)) return false;
			if (!JiraSharedSecret.Equals(other.JiraSharedSecret)) return false;
			if (!JiraWorklogSyncInterval.Equals(other.JiraWorklogSyncInterval)) return false;
			if (!JcFullSyncInterval.Equals(other.JcFullSyncInterval)) return false;
			if (!TargetRootProjectId.Equals(other.TargetRootProjectId)) return false;
			if (!CompanyId.Equals(other.CompanyId)) return false;
			if (CompanyAuthCode != other.CompanyAuthCode) return false;
			if (IsServerJira != other.IsServerJira) return false;
			return true;
		}
	}
}
