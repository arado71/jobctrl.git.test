using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Jira.Utils;

namespace JiraSyncTool.Jira
{
	class DataContext: ISyncDataContext
	{
		public DataContext()
		{

		}
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public List<JiraConfig> GetConfigs()
		{
			throw new NotImplementedException();
		}
	}
}
