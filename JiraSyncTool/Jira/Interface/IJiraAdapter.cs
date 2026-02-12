using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JiraSyncTool.Jira.Model;
using JiraSyncTool.Jira.Model.Jira;

namespace JiraSyncTool.Jira.Interface
{
	public interface IJiraAdapter
	{
		List<JiraProject> GetProjects();
		List<JiraIssue> GetSubIssuesForIssue(string key);
		JiraUser GetUser(string key);
		List<JiraIssue> GetIssuesForProject(string projectKey, CancellationToken ct);
		bool HasSubIssues(string issueKey);
		bool IsClosedIssue(JiraIssue issue);
		List<JiraUser> GetUsers();
		List<JiraIssue> GetIssues(CancellationToken ct);
		List<Jira.Model.Jira.JiraWorklog> GetWorklogs(Interval interval, CancellationToken ct);
		List<Jira.Model.Jira.JiraWorklog> ConvertJcWorkTimes(List<Model.Jc.WorkTime> workTimes);
		void createWorklog(Model.Jira.JiraWorklog worklog);
		void deleteWorklog(Model.Jira.JiraWorklog worklog);
		bool HasUsers();
	}
}
