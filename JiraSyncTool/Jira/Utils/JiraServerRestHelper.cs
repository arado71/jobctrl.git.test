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
	class JiraServerRestHelper : IRestHelper
	{
		private const string INCLUDE_MAIN_ISSUES_PARAM_NAME = "includeMainIssues";
		private const string INCLUDE_SUB_ISSUES_PARAM_NAME = "includeSubIssues";
		private const string START_DATE_PARAM_NAME = "startDate";
		private const string END_DATE_PARAM_NAME = "endDate";
		private const string ISSUE_KEY_PARAM_NAME = "issueKey";
		private const string JWT_PARAM_NAME = "jwt";
		private const string PARAM_DATE_FORMAT = "yyyy-MM-dd";
		private const string SECRET_KEY = "sYriex2wKSX7naJp4ercD2u/Ep6wAs2n9wysKYMYX7PXU+Hr/Xds54KdxYHGBSJN2AcMGyvK+2vVhKUTT+ktu4";

		private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly string baseUrl;
		private readonly string appKey;
		private readonly string authServer = "https://oauth-2-authorization-server.services.atlassian.com";
		private readonly string appAuthKey;
		private readonly string oAuthClientId;
		private readonly Dictionary<string, JiraToken> bearerTokens;
		private readonly RestClient restClient;
		private readonly JiraAuthenticationHelper authHelper;

		public JiraServerRestHelper(JiraConfig config)
		{
			baseUrl = config.JiraBaseUrl;
			appKey = config.JiraAppKey;
			authServer = config.JiraAuthServer;
			appAuthKey = config.JiraSharedSecret;
			oAuthClientId = config.JiraOAuthClientId;
			bearerTokens = new Dictionary<string, JiraToken>();
			restClient = new RestClient(baseUrl);
			authHelper = new JiraAuthenticationHelper(baseUrl, appKey, null, SECRET_KEY, null);
		}

		public void AddWorklog(JiraWorklog worklog, JiraUser user)
		{
			string issueKey = worklog.IssueKey;
			int id = worklog.Id;
			string methodUrl = "/plugins/servlet/worklog";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl + $"?issueKey={issueKey}&jwt={token}", Method.POST);
			var serializedWorklog = JsonConvert.SerializeObject(worklog);
			request.AddParameter("application/json", serializedWorklog, "application/json", ParameterType.RequestBody);
			var response = restClient.Execute(request);
		}

		public void DeleteWorklog(JiraWorklog worklog)
		{
			string issueKey = worklog.IssueKey;
			int id = worklog.Id;
			string methodUrl = "/plugins/servlet/worklog";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl + $"?issueKey={issueKey}&jwt={token}", Method.DELETE);
			var serializedWorklog = JsonConvert.SerializeObject(worklog);
			request.AddParameter("application/json", serializedWorklog, "application/json", ParameterType.RequestBody);
			var response = restClient.Execute(request);
		}

		public List<JiraIssue> GetIssues(CancellationToken ct)
		{
			List<JiraIssue> result = new List<JiraIssue>();
			ct.ThrowIfCancellationRequested();
			log.Debug("Getting issues...");
			string methodUrl = "/plugins/servlet/issues";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl, Method.GET);
			request.AddParameter(INCLUDE_MAIN_ISSUES_PARAM_NAME, true);
			request.AddParameter(INCLUDE_SUB_ISSUES_PARAM_NAME, true);
			request.AddParameter(JWT_PARAM_NAME, token);
			var response = restClient.Execute(request);
			JArray jArray = JArray.Parse(response.Content);
			result.AddRange(jArray.ToObject<List<JiraIssue>>());
			return result;
		}

		public List<JiraIssue> GetMainIssues(CancellationToken ct)
		{
			List<JiraIssue> result = new List<JiraIssue>();
			ct.ThrowIfCancellationRequested();
			log.Debug("Getting main issues...");
			string methodUrl = "/plugins/servlet/issues";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl, Method.GET);
			request.AddParameter(INCLUDE_MAIN_ISSUES_PARAM_NAME, true);
			request.AddParameter(INCLUDE_SUB_ISSUES_PARAM_NAME, false);
			request.AddParameter(JWT_PARAM_NAME, token);
			var response = restClient.Execute(request);
			JArray jArray = JArray.Parse(response.Content);
			result.AddRange(jArray.ToObject<List<JiraIssue>>());
			return result;
		}

		public List<JiraProject> GetProjects()
		{
			log.Debug("Getting projects...");
			string methodUrl = "/plugins/servlet/project";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl, Method.GET);
			request.AddParameter(JWT_PARAM_NAME, token);
			var response = restClient.Execute(request);
			JArray jArray = JArray.Parse(response.Content);
			GC.KeepAlive(request);
			return jArray.ToObject<List<JiraProject>>();
		}

		public List<JiraStatus> GetStatuses()
		{
			log.Debug("Getting statuses...");
			string methodUrl = "/plugins/servlet/status";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl, Method.GET);
			request.AddParameter(JWT_PARAM_NAME, token);
			var response = restClient.Execute(request);
			JArray jArray = JArray.Parse(response.Content);
			return jArray.ToObject<List<JiraStatus>>();
		}

		public List<JiraIssue> GetSubIssues()
		{
			List<JiraIssue> result = new List<JiraIssue>();
			log.Debug("Getting subissues...");
			string methodUrl = "/plugins/servlet/issues";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl, Method.GET);
			request.AddParameter(INCLUDE_MAIN_ISSUES_PARAM_NAME, false);
			request.AddParameter(INCLUDE_SUB_ISSUES_PARAM_NAME, true);
			request.AddParameter(JWT_PARAM_NAME, token);
			var response = restClient.Execute(request);
			JArray jArray = JArray.Parse(response.Content);
			result.AddRange(jArray.ToObject<List<JiraIssue>>());
			return result;
		}

		public List<JiraUser> GetUsers(string projectKey)
		{
			log.Debug("Getting users...");
			string methodUrl = "/plugins/servlet/user";
			string token = authHelper.GetToken(methodUrl);
			RestRequest request = new RestRequest(methodUrl, Method.GET);
			request.AddParameter(JWT_PARAM_NAME, token);
			var response = restClient.Execute(request);
			JArray jArray = JArray.Parse(response.Content);
			return jArray.ToObject<List<JiraUser>>();
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
				string methodUrl = "/plugins/servlet/worklog";
				string token = authHelper.GetToken(methodUrl);
				RestRequest request = new RestRequest(methodUrl, Method.GET);
				request.AddParameter(START_DATE_PARAM_NAME, interval.StartDate.ToString(PARAM_DATE_FORMAT));
				request.AddParameter(END_DATE_PARAM_NAME, interval.EndDate.ToString(PARAM_DATE_FORMAT));
				request.AddParameter(ISSUE_KEY_PARAM_NAME, issue.Key);
				request.AddParameter(JWT_PARAM_NAME, token);
				var response = restClient.Execute(request);
				JArray jArray = JArray.Parse(response.Content);
				List<JiraWorklog> worklogs = jArray.ToObject<List<JiraWorklog>>();
				if (worklogs != null && worklogs.Count > 0)
				{
					lock (result)
						result.AddRange(worklogs.Select(w => { w.IssueKey = issue.Key; return w; }).ToList());
				}
			});
			return result;
		}
	}
}
