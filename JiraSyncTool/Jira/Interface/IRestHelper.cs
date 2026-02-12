using System.Collections.Generic;
using System.Threading;
using JiraSyncTool.Jira.Model;
using JiraSyncTool.Jira.Model.Jira;

namespace JiraSyncTool.Jira.Utils
{
	interface IRestHelper
	{
		void AddWorklog(JiraWorklog worklog, JiraUser user);
		void DeleteWorklog(JiraWorklog worklog);
		List<JiraIssue> GetIssues(CancellationToken ct);
		List<JiraIssue> GetMainIssues(CancellationToken ct);
		List<JiraProject> GetProjects();
		List<JiraStatus> GetStatuses();
		List<JiraIssue> GetSubIssues();
		List<JiraUser> GetUsers(string projectKey);
		List<JiraWorklog> GetWorklogs(List<JiraIssue> issues, Interval interval, CancellationToken ct);
	}
}