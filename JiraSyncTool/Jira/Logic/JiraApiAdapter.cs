using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Jira.Model;
using JiraSyncTool.Jira.Model.Jira;
using JiraSyncTool.Jira.Utils;

namespace JiraSyncTool.Jira.Logic
{
	class JiraApiAdapter : IJiraAdapter
	{
		private readonly Utils.IRestHelper restHelper = null;
		private List<JiraProject> projectCache = null;
		private Dictionary<string, List<JiraIssue>> issuesForProjectCache = null;
		private Dictionary<string, List<JiraIssue>> subIssuesForIssueCache = null;
		private List<JiraUser> usersCache = null;
		private Dictionary<string, Model.Jira.JiraStatus> statusCache = null;
		private List<JiraIssue> issueCache = null;
		private readonly string jobCTRLComment = "From JobCTRL\n";

		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public JiraApiAdapter(JiraConfig config)
		{
			if (config.IsServerJira)
			{
				restHelper = new JiraServerRestHelper(config); 
			}
			else
			{
				restHelper = new JiraCloudRestHelper(config);
			}
			jobCTRLComment = config.WorklogComment;
		}

		private void GetStatusesFromServer()
		{
			log.Info("Getting statuses from server...");
			try
			{
				if (restHelper == null)
				{
					log.Error("Rest client is not initialized.");
					return;
				}
				//var request = new RestRequest(StatusURL, Method.GET);
				//var response = restSharpClient.Execute(request);
				//JArray jo = JArray.Parse(response.Content);
				//var statuses = jo.ToObject<List<Model.Jira.JiraStatus>>();
				List<JiraStatus> statuses = restHelper.GetStatuses();
				statusCache = statuses.ToDictionary(s => s.Id, s => s);
				log.InfoFormat("Got {0} statuses", statuses.Count);
			}
			catch (Exception e)
			{
				log.Error("Something went wrong in getting statuses from server.", e);
				return;
			}
		}

		public bool IsClosedIssue(JiraIssue issue)
		{
			if (statusCache == null)
			{
				GetStatusesFromServer();
			}
			bool ret = false;
			Model.Jira.JiraStatus status;
			if (statusCache.TryGetValue(issue.Status.Id, out status))
			{
				ret = status.IsClosed;
			}
			return ret;
		}

		public List<JiraProject> GetProjects()
		{
			if (projectCache == null)
			{
				log.Info("Getting Jira projects from server...");
				projectCache = GetProjectsFromServer();
				log.InfoFormat("Got {0} projects", projectCache.Count);
			}
			return projectCache;
		}

		private List<JiraProject> GetProjectsFromServer()
		{
			log.Info("Getting projects from server...");
			List<JiraProject> ret = restHelper.GetProjects();
			log.InfoFormat("Got {0} projects", ret.Count);
			return ret;
		}

		private List<JiraUser> GetUsersFromServer()
		{
			log.Info("Getting users from server...");
			List<JiraUser> ret = new List<JiraUser>();
			foreach (var jiraProject in restHelper.GetProjects())
			{
				foreach (var user in restHelper.GetUsers(jiraProject.Key))
				{
					if(ret.All(x => x.Key != user.Key))
						ret.Add(user);
				}
			}
			log.InfoFormat("Got {0} users", ret.Count);
			return ret;
		}

		public JiraUser GetUser(string key)
		{
			if (usersCache == null)
			{
				usersCache = GetUsersFromServer();
			}
			if (key == null) return null;
			return usersCache.FirstOrDefault(j => j.Key.Equals(key));
		}

		public JiraUser GetUser(Model.Jc.User user)
		{
			if (usersCache == null)
			{
				usersCache = GetUsersFromServer();
			}
			if (user.Email == null) return null;
			return usersCache.FirstOrDefault(j => j.Email.Equals(user.Email));
		}

		/// <summary>
		/// Gets the issues which belongs to the specified project. If the project is empty, then returns null.
		/// </summary>
		/// <param name="projectKey">Project's key</param>
		/// <param name="ct">CancellationToken</param>
		/// <returns></returns>
		public List<JiraIssue> GetIssuesForProject(string projectKey, CancellationToken ct)
		{
			if (issuesForProjectCache == null)
			{
				issuesForProjectCache = GetIssuesForProjectsFromServer(ct);
			}
			List<JiraIssue> ret;
			if (issuesForProjectCache.TryGetValue(projectKey, out ret))
				return ret;
			return null;
		}

		private Dictionary<string, List<JiraIssue>> GetIssuesForProjectsFromServer(CancellationToken ct)
		{
			log.Info("Getting issues for projects from server...");
			ct.ThrowIfCancellationRequested();
			Dictionary<string, List<JiraIssue>> ret = new Dictionary<string, List<JiraIssue>>();
			List<JiraIssue> issues = restHelper.GetMainIssues(ct); //jiraClient.Issues.GetIssuesFromJqlAsync("issuetype in standardIssueTypes()").GetAwaiter().GetResult();
			int sum = 0;
			foreach (var issue in issues)
			{
				sum++;
				List<JiraIssue> issueList;
				if (ret.TryGetValue(issue.Project.Key, out issueList))
				{
					issueList.Add(issue);
				}
				else
				{
					List<JiraIssue> list = new List<JiraIssue>();
					list.Add(issue);
					ret.Add(issue.Project.Key, list);
				}
			}

			log.InfoFormat("Got {0} issues.", sum);
			return ret;
		}

		/// <summary>
		/// Gets the subissues which's parent is specified issue. If the issue doesn't have any subissue, then returns null.
		/// </summary>
		/// <param name="key">Parent issue's key.</param>
		/// <returns></returns>
		public List<JiraIssue> GetSubIssuesForIssue(string key)
		{
			if (subIssuesForIssueCache == null)
				subIssuesForIssueCache = GetSubIssuesForIssuesFromServer();
			List<JiraIssue> ret;
			if (subIssuesForIssueCache.TryGetValue(key, out ret))
				return ret;
			return null;
		}

		private Dictionary<string, List<JiraIssue>> GetSubIssuesForIssuesFromServer()
		{
			log.Info("Getting subissues from server...");
			Dictionary<string, List<JiraIssue>> ret = new Dictionary<string, List<JiraIssue>>();
			List<JiraIssue> subIssues = restHelper.GetSubIssues();
			foreach (var issue in subIssues)
			{
				List<JiraIssue> issueList;
				 if (ret.TryGetValue(issue.ParentKey, out issueList))
				{
					issueList.Add(issue);
				}
				else
				{
					List<JiraIssue> list = new List<JiraIssue>();
					list.Add(issue);
					ret.Add(issue.ParentKey, list);
				}
			}
			log.InfoFormat("Got {0} subissues.", ret.Count);
			return ret;
		}

		public bool HasSubIssues(string issueKey)
		{
			if (subIssuesForIssueCache == null)
				subIssuesForIssueCache = GetSubIssuesForIssuesFromServer();
			return subIssuesForIssueCache.ContainsKey(issueKey);
		}


		public List<JiraUser> GetUsers()
		{
			if (usersCache == null)
			{
				GetUsersFromServer();
			}
			return usersCache;
		}


		public List<JiraIssue> GetIssues(CancellationToken ct)
		{
			if (issueCache == null)
				GetIssuesFromServer(ct);
			return issueCache;
		}

		private void GetIssuesFromServer(CancellationToken ct)
		{
			log.Info("Getting Jira issues from server...");
			issueCache = restHelper.GetIssues(ct);
			log.InfoFormat("Got {0} issues.", issueCache.Count);
		}

		/// <summary>
		/// Returns a dictionary in which the keys holds the Jira usernames.
		/// </summary>
		/// <returns></returns>
		public List<JiraWorklog> GetWorklogs(Interval interval, CancellationToken ct)
		{
			log.Info("Getting worklogs from Jira...");
			Stopwatch sw = Stopwatch.StartNew();
			if (issueCache == null)
				GetIssuesFromServer(ct);
			List<Model.Jira.JiraWorklog> ret = new List<Model.Jira.JiraWorklog>();
			List<JiraWorklog> worklogs = restHelper.GetWorklogs(issueCache, interval, ct);
			ct.ThrowIfCancellationRequested();
			if (worklogs != null)
			{
				var filteredWorklogs = worklogs.Where(wl => wl.StartDate >= interval.StartDate
						&& wl.StartDate <= interval.EndDate
						&& wl.Comment != null && wl.Comment.StartsWith(jobCTRLComment));
				ret.AddRange(filteredWorklogs);
			}
			sw.Stop();
			log.InfoFormat("Got {0} worklogs from Jira. Took {1} ms.", ret.Count, sw.Elapsed.TotalMilliseconds);
			return ret;
		}
		private void EnsureUsersCached()
		{
			if (usersCache == null)
				usersCache = GetUsersFromServer();
		}
		private void EnsureIssuesCached(CancellationToken ct)
		{
			if (issueCache == null)
				GetIssuesFromServer(ct);
		}

		public List<Model.Jira.JiraWorklog> ConvertJcWorkTimes(List<Model.Jc.WorkTime> workTimes)
		{
			if (usersCache == null)
				GetUsersFromServer();
			List<Model.Jira.JiraWorklog> res = new List<Model.Jira.JiraWorklog>();
			int prevTaskId = -1;
			long rolledSeconds = 0;

			foreach (var wt in workTimes.OrderBy(x => x.Task.Id))
			{
				if (wt == null || !usersCache.Any(u => u.Email == wt.UserEmail))
					continue;
				if (prevTaskId != wt.Task.Id)
				{
					prevTaskId = wt.Task.Id;
					rolledSeconds = 0;
				}

				if (wt.Duration + rolledSeconds > 0)
				{
					wt.Duration += rolledSeconds;
					rolledSeconds = 0;
				}
				rolledSeconds += wt.Remaining;
				JiraWorklog worklog = new JiraWorklog()
				{
					Author = usersCache.First(u => u.Email == wt.UserEmail).Key,
					Comment = jobCTRLComment + wt.Description,
					IssueKey = wt.Task.ExtId,
					StartDate = wt.StartDate,
					TimeSpent = wt.JiraDuration,
					TimeSpentSeconds = wt.Duration

				};
				res.Add(worklog);
			}
			return res;
		}

		public void createWorklog(Model.Jira.JiraWorklog worklog)
		{
			JiraUser user = GetUser(worklog.Author);
			restHelper.AddWorklog(worklog, user);
		}


		public void deleteWorklog(Model.Jira.JiraWorklog worklog)
		{
			log.InfoFormat("Delete worklog with issuekey: {0} id: {1}, timespent: {2}", worklog.IssueKey, worklog.Id, worklog.TimeSpent);
			restHelper.DeleteWorklog(worklog);
		}

		public bool HasUsers()
		{
			if (usersCache == null)
			{
				usersCache = GetUsersFromServer();
			}
			return usersCache.Count > 0;
		}
	}
}
