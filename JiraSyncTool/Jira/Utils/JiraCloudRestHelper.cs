using JiraSyncTool.Jira.Model;
using JiraSyncTool.Jira.Model.Jira;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JiraSyncTool.Jira.Utils
{
	class JiraCloudRestHelper : IRestHelper
	{
		private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly JiraAuthenticationHelper authHelper;
		private readonly string baseUrl;
		private readonly string appKey;
		private readonly string authServer = "https://oauth-2-authorization-server.services.atlassian.com";
		private readonly string appAuthKey;
		private readonly string oAuthClientId;
		private readonly Dictionary<string, JiraToken> bearerTokens;
		private readonly RestClient restClient;
		private string startDateCustomFieldName;
		private readonly string startDateName = "StartDate";

		public JiraCloudRestHelper(JiraConfig config)
		{
			baseUrl = config.JiraBaseUrl;
			appKey = config.JiraAppKey;
			authServer = config.JiraAuthServer;
			appAuthKey = config.JiraSharedSecret;
			oAuthClientId = config.JiraOAuthClientId;
			bearerTokens = new Dictionary<string, JiraToken>();
			authHelper = new JiraAuthenticationHelper(baseUrl, appKey, authServer, appAuthKey, oAuthClientId);
			restClient = new RestClient(baseUrl);
		}


		public List<JiraStatus> GetStatuses()
		{
			string methodUrl = "/rest/api/2/status";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl, Method.GET);
			request.AddHeader("Authorization", "JWT " + token);
			var response = restClient.Execute(request);
			JArray jArray = JArray.Parse(response.Content);
			return jArray.ToObject<List<JiraStatus>>();
		}

		public List<JiraProject> GetProjects()
		{
			string methodUrl = "/rest/api/2/project";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl, Method.GET);
			request.AddHeader("Authorization", "JWT " + token);
			var response = restClient.Execute(request);
			JArray jArray = JArray.Parse(response.Content);
			return jArray.ToObject<List<JiraProject>>();
		}

		public List<JiraUser> GetUsers(string projectKey)
		{
			string methodUrl = "/rest/api/2/user/assignable/search";
			string queryString = "project=" + projectKey;
			string token = authHelper.GetToken(methodUrl, JiraAuthenticationHelper.MethodType.GET, queryString);
			RestRequest request = new RestRequest(methodUrl + "?" + queryString, Method.GET);
			request.AddHeader("Authorization", "JWT " + token);
			var response = restClient.Execute(request);
			JArray jArray = JArray.Parse(response.Content);
			return jArray.ToObject<List<JiraUser>>();
		}

		public List<JiraIssue> GetMainIssues(CancellationToken ct)
		{
			List<JiraIssue> result = new List<JiraIssue>();
			int startAt = 0; int maxResults = 100; int total = 101;
			while (startAt < total)
			{
				ct.ThrowIfCancellationRequested();
				log.DebugFormat("Getting main issues: startAt: {0}, total: {1}, maxResults: {2}", startAt, total, maxResults);
				string methodUrl = "/rest/api/2/search";
				string queryString = (startAt == 0 ? "expand=names&" : "") + "jql=issuetype%20in%20standardIssueTypes%28%29&maxResults=" + maxResults + "&startAt=" + startAt;
				string token = authHelper.GetToken(methodUrl, JiraAuthenticationHelper.MethodType.GET, queryString);
				RestRequest request = new RestRequest(methodUrl + "?" + queryString, Method.GET);
				request.AddHeader("Authorization", "JWT " + token);
				var response = restClient.Execute(request);
				JObject res = JObject.Parse(response.Content);
				if (startAt == 0)
				{
					startDateCustomFieldName = res["names"]?.Cast<JProperty>().FirstOrDefault(n => ((string)n.Value).Equals(startDateName))?.Name;
				}
				JArray jArray = res.Value<JArray>("issues");
				//jArray.Children<JProperty>().ForEach(x =>
				//{
				//	x["StartDate"] = ((string) ((JProperty) x[startDateCustomFieldName]).Value);
				//});
				if (startDateCustomFieldName == null)
				{
					result.AddRange(jArray.ToObject<List<JiraIssue>>());
				}
				else
				{
					foreach (var child in jArray)
					{
						var issue = child.ToObject<JiraIssue>();
						var startDateCustomField = child["fields"][startDateCustomFieldName];
						issue.StartDate = startDateCustomField.Value<DateTime?>();
						result.Add(issue);
					}
				}

				maxResults = res.Value<int>("maxResults");
				startAt += maxResults;
				total = res.Value<int>("total");
			}
			return result;
		}

		public List<JiraIssue> GetSubIssues()
		{
			List<JiraIssue> result = new List<JiraIssue>();
			int startAt = 0; int maxResults = 100; int total = 101;
			while (startAt < total)
			{
				log.DebugFormat("Getting sub-issues: startAt: {0}, total: {1}, maxResults: {2}", startAt, total, maxResults);
				string methodUrl = "/rest/api/2/search";
				string queryString = (startAt == 0 ? "expand=names&" : "") + "jql=issuetype%20in%20subTaskIssueTypes%28%29&maxResults=" + maxResults + "&startAt=" + startAt;
				string token = authHelper.GetToken(methodUrl, JiraAuthenticationHelper.MethodType.GET, queryString);
				RestRequest request = new RestRequest(methodUrl + "?" + queryString, Method.GET);
				request.AddHeader("Authorization", "JWT " + token);
				var response = restClient.Execute(request);
				JObject res = JObject.Parse(response.Content);
				if (startAt == 0)
				{
					startDateCustomFieldName = res["names"]?.Cast<JProperty>().FirstOrDefault(n => ((string)n.Value).Equals(startDateName))?.Name;
				}
				JArray jArray = res.Value<JArray>("issues");
				if (startDateCustomFieldName == null)
				{
					result.AddRange(jArray.ToObject<List<JiraIssue>>());
				}
				else
				{
					foreach (var child in jArray)
					{
						var issue = child.ToObject<JiraIssue>();
						var startDateCustomField = child["fields"][startDateCustomFieldName];
						issue.StartDate = startDateCustomField.Value<DateTime?>();
						result.Add(issue);
					}
				}
				maxResults = res.Value<int>("maxResults");
				startAt += maxResults;
				total = res.Value<int>("total");
			}
			return result;
		}

		public List<JiraIssue> GetIssues(CancellationToken ct)
		{
			List<JiraIssue> result = new List<JiraIssue>();
			int startAt = 0; int maxResults = 100; int total = 101;
			while (startAt < total)
			{
				ct.ThrowIfCancellationRequested();
				log.DebugFormat("Getting issues: startAt: {0}, total: {1}, maxResults: {2}", startAt, total, maxResults);
				string methodUrl = "/rest/api/2/search";
				string queryString = (startAt == 0 ? "expand=names&" : "") + "jql=&maxResults=" + maxResults + "&startAt=" + startAt;
				string token = authHelper.GetToken(methodUrl, JiraAuthenticationHelper.MethodType.GET, queryString);
				RestRequest request = new RestRequest(methodUrl + "?" + queryString, Method.GET);
				request.AddHeader("Authorization", "JWT " + token);
				var response = restClient.Execute(request);
				JObject res = JObject.Parse(response.Content);
				if (startAt == 0)
				{
					startDateCustomFieldName = res["names"]?.Cast<JProperty>().FirstOrDefault(n => ((string)n.Value).Equals(startDateName))?.Name;
				}
				JArray jArray = res.Value<JArray>("issues");
				if (startDateCustomFieldName == null)
				{
					result.AddRange(jArray.ToObject<List<JiraIssue>>());
				}
				else
				{
					foreach (var child in jArray)
					{
						var issue = child.ToObject<JiraIssue>();
						var startDateCustomField = child["fields"][startDateCustomFieldName];
						issue.StartDate = startDateCustomField.Value<DateTime?>();
						result.Add(issue);
					}
				}
				maxResults = res.Value<int>("maxResults");
				startAt += maxResults;
				total = res.Value<int>("total");
			}
			return result;
		}

		public List<JiraWorklog> GetWorklogs(List<JiraIssue> issues, Interval interval, CancellationToken ct)
		{
			List<JiraWorklog> result = new List<JiraWorklog>();
			int counter = 0;
			int issueCount = issues.Count;
			Parallel.ForEach(issues, (issue, state) =>
			{
				if (ct.IsCancellationRequested)
				{
					state.Break();
				}
				Interlocked.Increment(ref counter);
				if (counter % 20 == 0)
				{
					log.Debug($"Getting worklogs for issues (counter: {counter}; all: {issueCount})...");
				}
				string methodUrl = "/rest/api/2/issue/" + issue.Key + "/worklog";
				string queryString = "";
				string token = authHelper.GetToken(methodUrl, JiraAuthenticationHelper.MethodType.GET, queryString);
				RestRequest request = new RestRequest(methodUrl + "?" + queryString, Method.GET);
				request.AddHeader("Authorization", "JWT " + token);
				var response = restClient.Execute(request);
				JArray jArray = JObject.Parse(response.Content).Value<JArray>("worklogs");
				List<JiraWorklog> worklogs = jArray.ToObject<List<JiraWorklog>>();
				if (worklogs != null && worklogs.Count > 0)
				{
					lock(result)
						result.AddRange(worklogs.Select(w => { w.IssueKey = issue.Key; return w; }).ToList());
				}
			});
			return result;
		}

		public void DeleteWorklog(JiraWorklog worklog)
		{
			string issueKey = worklog.IssueKey;
			int id = worklog.Id;
			string methodUrl = "/rest/api/2/issue/" + issueKey + "/worklog/" + id;
			string token = authHelper.GetToken(methodUrl, JiraAuthenticationHelper.MethodType.DELETE);
			RestRequest request = new RestRequest(methodUrl, Method.DELETE);
			request.AddHeader("Authorization", "JWT " + token);
			var response = restClient.Execute(request);
		}

		public void AddWorklog(JiraWorklog worklog, JiraUser user)
		{
			string methodUrl = "/rest/api/2/issue/" + worklog.IssueKey + "/worklog";
			JiraToken jt;
			if (!bearerTokens.TryGetValue(user.Key, out jt))
			{
				jt = authHelper.GetBearerToken(user.Key, methodUrl, JiraAuthenticationHelper.MethodType.POST);
			}
			else
			{
				if (DateTime.Now > jt.ExpirationDate)
					jt = authHelper.GetBearerToken(user.Key, methodUrl, JiraAuthenticationHelper.MethodType.POST);
			}

			if (jt == null)
			{
				log.Warn($"Couldn't get bearer token for user: {user.Key}: {user.Email} worklog: {worklog.Id}");
				return;
			}
			bearerTokens[user.Key] = jt;
			RestRequest request = new RestRequest(methodUrl, Method.POST);
			var serializedWorklog = JsonConvert.SerializeObject(worklog);
			//request.AddHeader("Content-Type", "application/json");
			request.Parameters.Clear();
			request.AddHeader("Authorization", "Bearer " + jt.Token);
			request.AddParameter("application/json", serializedWorklog, "application/json", ParameterType.RequestBody);

			//request.AddBody(deserializedWorklog);
			//request.RequestFormat = DataFormat.Json;

			var response = restClient.Execute(request);
		}
	}
}
