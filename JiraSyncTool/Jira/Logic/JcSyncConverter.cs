using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Jira.Model.Jira;

namespace JiraSyncTool.Jira.Logic
{
	class JcSyncConverter
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly IJiraAdapter jiraAdapter;
		private readonly IJcAdapter jcAdapter;

		public JcSyncConverter(IJiraAdapter jiraAdapter, IJcAdapter jcAdapter)
		{
			this.jiraAdapter = jiraAdapter;
			this.jcAdapter = jcAdapter;
		}

		public Model.Jc.Project Convert(JiraProject jiraProject)
		{
			var jcId = jcAdapter.GetJcProjectId(jiraProject.Id);
			if (jcId != null)
			{
				var jcObject = jcAdapter.GetProject(jcId.Value);
				if (jcObject != null)
				{
					var result = jiraProject.ToJcProject();
					result.Id = jcId.Value;
					return result;
				}

				jcAdapter.RemoveProjectMapping(jiraProject.Id, jcId.Value);
				log.WarnFormat("Project mapping {0}-{1} no longer valid", jiraProject.Id, jcId.Value);
			}

			return jiraProject.ToJcProject();
		}

		public Model.Jc.User Convert(JiraUser jiraUser)
		{
			var jcId = jcAdapter.GetJcUserId(jiraUser.Email);
			if (jcId != null)
			{
				var jcUser = jcAdapter.GetUser(jcId.Value);
				if (jcUser != null)
				{
					var result = jiraUser.ToJcUser();
					result.Id = jcId.Value;
					return result;
				}
			}

			log.DebugFormat("Matching user {0} by email", jiraUser);
			return FindUserByEmail(jiraUser.Email);
		}

		private Model.Jc.User FindUserByEmail(string emailAddress)
		{
			return jcAdapter.GetUsers().FirstOrDefault(user => string.Equals(user.Email, emailAddress, StringComparison.CurrentCultureIgnoreCase));
		}

		public Model.Jc.Task ConvertToTask(JiraIssue jiraIssue)
		{
			if (jiraIssue.Key == null)
			{
				log.Error("Jira issue doesn't have key!");
				return null;
			}
			var jcId = jcAdapter.GetJcTaskId(jiraIssue.Key);
			if (jcId != null)
			{
				var jcObject = jcAdapter.GetTask(jcId.Value);
				if (jcObject != null)
				{
					var result = jiraIssue.ToJcTask();
					result.Id = jcId.Value;
					return result;
				}

				jcAdapter.RemoveTaskMapping(jiraIssue.Key, jcId.Value);
				log.WarnFormat("Task mapping {0}-{1} no longer valid", jiraIssue.Key, jcId.Value);
			}
			return jiraIssue.ToJcTask();
		}

		public Model.Jc.Project ConvertToProject(JiraIssue jiraIssue)
		{
			if (jiraIssue.Key == null)
			{
				log.Error("Jira issue doesn't have key!");
				return null;
			}
			var jcId = jcAdapter.GetJcProjectIdForIssue(jiraIssue.Key + "-P");
			if (jcId != null)
			{
				var jcObject = jcAdapter.GetProject(jcId.Value);
				if (jcObject != null)
				{
					var result = jiraIssue.ToJcProject();
					result.Id = jcId.Value;
					return result;
				}
				jcAdapter.RemoveIssueProjectMapping(jiraIssue.Key, jcId.Value);
				log.WarnFormat("Task mapping {0}-{1} no longer valid", jiraIssue.Key, jcId.Value);
			}
			return jiraIssue.ToJcProject();
		}
	}
}
