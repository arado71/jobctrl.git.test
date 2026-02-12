using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Jira.Utils;

namespace JiraSyncTool.Jira.Logic
{
	class JiraSyncDataContext : ISyncDataContext
	{
		private readonly SyncDataContext context;

		public JiraSyncDataContext()
		{
			context = new SyncDataContext();
		}

		public List<JiraConfig> GetConfigs()
		{
			return context.Client_GetJiraEnabledCompanies()
				.Where(x => x.CompanyId != null && x.JobCtrlAuthCode != null)
				.Select(x => new JiraConfig
				{
					JiraBaseUrl = x.BaseUrl,
					JiraAppKey = "com.jobctrl.jira",
					JiraOAuthClientId = x.OauthClientId,
					JiraSharedSecret = x.SharedSecret,
					CompanyId = x.CompanyId.Value,
					CompanyAuthCode = x.JobCtrlAuthCode.Value,
					TargetRootProjectId = x.JiraRootTaskId
				}).ToList();
		}

		public void Dispose()
		{
			context.Dispose();
		}
	}
}
