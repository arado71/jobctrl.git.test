using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Jira.Utils;
using JiraSyncTool.jobCTRLAPI;
using System;
using System.Collections.Generic;

namespace JiraSyncTool.Jira.Logic
{
	class JiraFromJcApiSyncDataContext : ISyncDataContext
	{
		private readonly API api;
		private Guid authCode;

		public JiraFromJcApiSyncDataContext(Guid CompanyAuthCode)
		{
			api = new API();
			authCode = CompanyAuthCode;
		}

		public void Dispose()
		{
			api.Dispose();
		}

		public List<JiraConfig> GetConfigs()
		{
			var res = api.GetJiraToken(authCode, out var tokenDetails);
			if (res != GetJiraTokenRet.OK) return null;
			JiraConfig config = new JiraConfig
			{
				JiraBaseUrl = tokenDetails.BaseUrl,
				JiraAppKey = "com.jobctrl.jira",
				JiraOAuthClientId = tokenDetails.OauthClientId,
				JiraSharedSecret = tokenDetails.SharedSecret,
				CompanyId = -1,
				CompanyAuthCode = authCode,
				TargetRootProjectId = tokenDetails.JiraRootTaskId
			};
			return new List<JiraConfig> { config };
		}
	}
}
