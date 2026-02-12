using System;
using System.Collections.Generic;
using JiraSyncTool.Jira.Utils;

namespace JiraSyncTool.Jira.Interface
{
	public interface ISyncDataContext: IDisposable
	{
		List<JiraConfig> GetConfigs();
	}
}