using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Jira.Model;
using JiraSyncTool.Jira.Model.Jc;
using JiraSyncTool.Jira.Model.Jira;
using JiraSyncTool.Jira.Utils;
using Task = JiraSyncTool.Jira.Model.Jc.Task;

namespace JiraSyncTool.Jira.Logic
{
	public class ApplicationSync
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ApplicationSync));

		private readonly JcSyncConverter syncConverter;
		private readonly IJiraAdapter jiraAdapter;
		private readonly IJcAdapter jcAdapter;
		private readonly int targetRootProjectId;

		public ApplicationSync(IJiraAdapter jiraAdapter, IJcAdapter jcAdapter, int targetRootProjectId)
		{
			this.jiraAdapter = jiraAdapter;
			this.jcAdapter = jcAdapter;
			this.targetRootProjectId = targetRootProjectId;
			syncConverter = new JcSyncConverter(jiraAdapter, jcAdapter);
		}

		public void SyncToJc(CancellationToken ct)
		{
			Stopwatch sw = Stopwatch.StartNew();
			log.Info("Synchronizing to JC...");
			SyncRootProjects(ct);
			log.InfoFormat("Synchronization to JC completed. Elapsed time: {0} ms", sw.Elapsed.TotalMilliseconds);
		}
		public void SyncToJira(CancellationToken ct, Interval interval)
		{
			Stopwatch sw = Stopwatch.StartNew();
			log.Info("Synchronizing to Jira...");
			syncWorklogsToJira(ct, interval);
			log.InfoFormat("Synchronization to Jira completed. Elapsed time: {0} ms", sw.Elapsed.TotalMilliseconds);
		}


		private List<Model.Jc.Project> GetRootProjects()
		{
			var rootProjectId = targetRootProjectId;
			if (rootProjectId == 0)
				return jcAdapter.GetRootProjects();
			return jcAdapter.GetProject(rootProjectId).ChildrenProjects;
		}

		private void SyncRootProjects(CancellationToken ct)
		{
			var jcRoot = targetRootProjectId > 0
				? jcAdapter.GetProject(targetRootProjectId)
				: null;
			log.Info("Syncing root projects");
			/// TODO: Filtering if necessary
			var jiraProjects = jiraAdapter.GetProjects();
			ct.ThrowIfCancellationRequested();
			var jcProjects = GetRootProjects();
			ct.ThrowIfCancellationRequested();

			Dictionary<Project, JiraProject> converted = jiraProjects.ToDictionary(jiraProject => syncConverter.Convert(jiraProject), jiraProject => jiraProject, ReferenceComparer<Model.Jc.Project>.Default);
			var difference = SyncHelper.CalculateDifferences(converted.Keys, jcProjects, Model.Jc.Project.IdComparer);
			int a = 0, c = 0, r = 0, sum = difference.Count;
			foreach (var added in difference.Added)
			{
				ct.ThrowIfCancellationRequested();
				var jiraProject = converted[added];
				if (added.IsFromServer)
				{
					var original = jcAdapter.GetProject(added.Id);
					jcAdapter.EnsureOpened(original);
					if (original.TryUpdate(added))
					{
						jcAdapter.Update(original);
					}
					jcAdapter.Move(original, jcRoot);
				}
				else
				{
					added.Parent = jcRoot;
					jcAdapter.Create(added);
					jcAdapter.AddProjectMapping(jiraProject.Id, added.Id);
				}
				log.Info(string.Format("Counter: add {0}/{1}, {2} sum", ++a, difference.AddedCounter, sum));
				var jiraIssues = jiraAdapter.GetIssuesForProject(converted[added].Key, ct);
				if (jiraIssues == null)
				{
					log.InfoFormat("Project ({0}) has no issues.", converted[added].Key);
					break;
				}
				SyncIssuesAsTasks(added, jiraIssues, ct);
				SyncIssuesAsProjects(added, jiraIssues, ct);
			}

			foreach (var common in difference.Common)
			{
				ct.ThrowIfCancellationRequested();
				var jiraProject = converted[common.Source];
				jcAdapter.EnsureOpened(common.Target);
				if (common.Target.TryUpdate(common.Source))
				{
					jcAdapter.Update(common.Target);
				}
				log.Info(string.Format("Counter: update {0}/{1}, {2} sum", ++c, difference.CommonCounter, sum));
				var jiraIssues = jiraAdapter.GetIssuesForProject(converted[common.Source].Key, ct);
				if (jiraIssues == null)
				{
					log.InfoFormat("Project ({0}) has no issues.", converted[common.Source].Key);
					SyncIssuesAsProjects(common.Target, new List<Model.Jira.JiraIssue>(), ct);
					SyncIssuesAsTasks(common.Target, new List<Model.Jira.JiraIssue>(), ct);
					continue;
				}
				SyncIssuesAsTasks(common.Target, jiraIssues, ct);
				SyncIssuesAsProjects(common.Target, jiraIssues, ct);
			}

			foreach (var removed in difference.Removed)
			{
				ct.ThrowIfCancellationRequested();
				jcAdapter.EnsureClosed(removed);
				log.Info(string.Format("Counter: remove {0}/{1}, {2} sum", ++r, difference.RemovedCounter, sum));
			}
		}

		private void SyncIssuesAsTasks(Model.Jc.Project parent, List<Model.Jira.JiraIssue> jiraIssues, CancellationToken ct)
		{
			log.DebugFormat("Synchronizing task {0}", parent);
			var jcChildTasks = parent.ChildrenTasks;
			Dictionary<Task, Model.Jira.JiraIssue> converted = jiraIssues.ToDictionary(jiraIssue => syncConverter.ConvertToTask(jiraIssue), jiraIssue => jiraIssue, ReferenceComparer<Model.Jc.Task>.Default);
			var difference = SyncHelper.CalculateDifferences(converted.Keys, jcChildTasks, Model.Jc.Task.IdComparer);
			foreach (var added in difference.Added)
			{
				ct.ThrowIfCancellationRequested();
				var jiraIssue = converted[added];
				log.DebugFormat("Adding task {0}", added);
				try
				{
					CreateOrMove(parent, jiraIssue.Key, added);
					if (jiraAdapter.IsClosedIssue(converted[added]))
					{
						jcAdapter.EnsureClosed(added);
						continue;
					}
					SyncAssignments(added, jiraIssue.Assignee?.Key, ct, added.StartDate, added.EndDate, added.Duration);
					setStartDateEndDateNull(added);
				}
				catch (Exception ex)
				{
					log.Error("Failed to create task " + added, ex);
				}
			}
			foreach (var common in difference.Common)
			{
				log.DebugFormat("Synchronizing task {0}", common.Source);
				ct.ThrowIfCancellationRequested();
				var jiraTask = converted[common.Source];
				if (jiraAdapter.IsClosedIssue(jiraTask))
				{
					jcAdapter.EnsureClosed(common.Target);
					continue;
				}
				try
				{
					jcAdapter.EnsureOpened(common.Target);
					if (common.Target.TryUpdate(common.Source))
					{
						jcAdapter.Update(common.Target);
					}
					SyncAssignments(common.Target, converted[common.Source].Assignee?.Key, ct, common.Source.StartDate, common.Source.EndDate, common.Source.Duration);
				}
				catch (Exception ex)
				{
					log.Error("Failed to synchronize task " + common.Source, ex);
				}

			}

			foreach (var removed in difference.Removed)
			{
				ct.ThrowIfCancellationRequested();
				log.DebugFormat("Removing task {0}", removed);
				try
				{
					jcAdapter.EnsureClosed(removed);
				}
				catch (Exception ex)
				{
					log.Error("Failed to remove task " + removed, ex);
				}
			}
		}

		private void SyncIssuesAsProjects(Model.Jc.Project parent, List<Model.Jira.JiraIssue> jiraIssues, CancellationToken ct)
		{
			var jcChildProjects = parent.ChildrenProjects;
			var standardIssuesWithoutSubIssues = jiraIssues.Where(jiraIssue => jiraAdapter.HasSubIssues(jiraIssue.Key));
			Dictionary<Project, Model.Jira.JiraIssue> converted = standardIssuesWithoutSubIssues.ToDictionary(jiraIssue => syncConverter.ConvertToProject(jiraIssue), jiraIssue => jiraIssue, ReferenceComparer<Model.Jc.Project>.Default);
			var difference = SyncHelper.CalculateDifferences(converted.Keys, jcChildProjects, Model.Jc.Project.IdComparer);
			foreach (var added in difference.Added)
			{
				ct.ThrowIfCancellationRequested();
				var issue = converted[added];
				log.DebugFormat("Adding project {0}", added);
				try
				{
					CreateOrMove(parent, issue.Key + "-P", added);
					if (jiraAdapter.IsClosedIssue(converted[added]))
					{
						jcAdapter.EnsureClosed(added);
						continue;
					}
					SyncSubIssues(converted[added], added, ct);
				}
				catch (Exception ex)
				{
					log.Error("Failed to create project " + added, ex);
				}
			}
			foreach (var common in difference.Common)
			{
				log.DebugFormat("Synchronizing project {0}", common.Source);
				ct.ThrowIfCancellationRequested();
				var jiraTask = converted[common.Source];
				if (jiraAdapter.IsClosedIssue(jiraTask))
				{
					jcAdapter.EnsureClosed(common.Target);
					continue;
				}
				try
				{
					jcAdapter.EnsureOpened(common.Target);
					if (common.Target.TryUpdate(common.Source))
					{
						jcAdapter.Update(common.Target);
					}
					SyncSubIssues(converted[common.Source], common.Target, ct);
				}
				catch (Exception ex)
				{
					log.Error("Failed to synchronize project " + common.Source, ex);
				}

			}

			foreach (var removed in difference.Removed)
			{
				ct.ThrowIfCancellationRequested();
				log.DebugFormat("Removing project {0}", removed);
				try
				{
					jcAdapter.EnsureClosed(removed);
				}
				catch (Exception ex)
				{
					log.Error("Failed to remove project " + removed, ex);
				}
			}

		}

		private void SyncSubIssues(Model.Jira.JiraIssue jiraIssue, Model.Jc.Project jcProject, CancellationToken ct)
		{
			log.DebugFormat("Synchronizing subissues for issue {0} ({1})", jiraIssue.Summary, jiraIssue.Key);
			var jiraIssues = jiraAdapter.GetSubIssuesForIssue(jiraIssue.Key);
			var jcChildTasks = jcProject.ChildrenTasks;
			Dictionary<Task, Model.Jira.JiraIssue> converted = jiraIssues.ToDictionary(iss => syncConverter.ConvertToTask(iss), iss => iss, ReferenceComparer<Model.Jc.Task>.Default);
			var difference = SyncHelper.CalculateDifferences(converted.Keys, jcChildTasks, Model.Jc.Task.IdComparer);
			foreach (var added in difference.Added)
			{
				ct.ThrowIfCancellationRequested();
				var issue = converted[added];
				log.DebugFormat("Adding task {0}", added);
				try
				{
					CreateOrMove(jcProject, issue.Key, added);
					if (jiraAdapter.IsClosedIssue(converted[added]))
					{
						jcAdapter.EnsureClosed(added);
						continue;
					}
					SyncAssignments(added, issue.Assignee?.Key, ct, added.StartDate, added.EndDate, added.Duration);
					setStartDateEndDateNull(added);
				}
				catch (Exception ex)
				{
					log.Error("Failed to create project " + added, ex);
				}
			}
			foreach (var common in difference.Common)
			{
				log.DebugFormat("Synchronizing task {0}", common.Source);
				ct.ThrowIfCancellationRequested();
				var jiraTask = converted[common.Source];
				if (jiraAdapter.IsClosedIssue(jiraTask))
				{
					jcAdapter.EnsureClosed(common.Target);
					continue;
				}
				try
				{
					jcAdapter.EnsureOpened(common.Target);
					if (common.Target.TryUpdate(common.Source))
					{
						jcAdapter.Update(common.Target);
					}
					SyncAssignments(common.Target, converted[common.Source].Assignee?.Key, ct, common.Source.StartDate, common.Source.EndDate, common.Source.Duration);
				}
				catch (Exception ex)
				{
					log.Error("Failed to synchronize task " + common.Source, ex);
				}

			}

			foreach (var removed in difference.Removed)
			{
				ct.ThrowIfCancellationRequested();
				log.DebugFormat("Removing task {0}", removed);
				try
				{
					jcAdapter.EnsureClosed(removed);
				}
				catch (Exception ex)
				{
					log.Error("Failed to remove task " + removed, ex);
				}
			}
		}

		private void CreateOrMove(Model.Jc.Project parent, string jiraIssueKey, Model.Jc.Project newChild)
		{
			if (newChild.IsFromServer)
			{
				var original = jcAdapter.GetProject(newChild.Id);
				log.DebugFormat("Project {0} already exists in {1}, moving to {2}", original, original.Parent, parent);
				jcAdapter.EnsureOpened(original);
				if (original.TryUpdate(newChild))
				{
					jcAdapter.Update(original);
				}

				jcAdapter.Move(original, parent);
			}
			else
			{
				log.DebugFormat("Creating project {0}", newChild);
				newChild.Parent = parent;
				jcAdapter.Create(newChild);
				jcAdapter.AddProjectMapping(jiraIssueKey, newChild.Id);
			}
		}

		private void CreateOrMove(Model.Jc.Project parent, string jiraIssueKey, Model.Jc.Task newChild)
		{
			if (newChild.IsFromServer)
			{
				var original = jcAdapter.GetTask(newChild.Id);
				log.DebugFormat("Task {0} already exists in {1}, moving to {2}", original, original.Parent, parent);
				jcAdapter.EnsureOpened(original);
				if (original.TryUpdate(newChild))
				{
					jcAdapter.Update(original);
				}

				jcAdapter.Move(original, parent);
			}
			else
			{
				log.DebugFormat("Creating task {0}", newChild);
				newChild.Parent = parent;
				jcAdapter.Create(newChild);
				jcAdapter.AddTaskMapping(jiraIssueKey, newChild.Id);
			}
		}

		/// <summary>
		/// Syncronizes the assignments for the <paramref name="task"/>.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="jiraAssigneeKey"></param>
		/// <param name="ct"></param>
		/// <param name="startDate">Start date of the assignment</param>
		/// <param name="endDate">End date of the assignment</param>
		private void SyncAssignments(Model.Jc.Task task, string jiraAssigneeKey, CancellationToken ct, DateTime? startDate, DateTime? endDate, TimeSpan? duration)
		{
			// If we cant get the users then do nothing.
			if (!jiraAdapter.HasUsers())
			{
				log.Error("We can't get users.");
				return;
			}
			var jiraUser = jiraAdapter.GetUser(jiraAssigneeKey);
			if (jiraUser == null)
			{
				removeAssignments(task.Assignments, ct);
				return;
			}
			int? id = jcAdapter.GetJcUserId(jiraUser.Email);
			if (!id.HasValue)
			{
				log.WarnFormat("User with email ({0}) doesn't exist in JC", jiraUser.Email);
				removeAssignments(task.Assignments, ct);
				return;
			}
			var jcUser = jcAdapter.GetUser(id.Value);
			Model.Jc.Assignment assignment = new Model.Jc.Assignment()
			{
				User = jcUser,
				Task = task,
				StartDate = startDate,
				EndDate = endDate,
				Duration = duration
			};
			List<Model.Jc.Assignment> convertedAssignments = new List<Model.Jc.Assignment>();
			convertedAssignments.Add(assignment);
			var difference = SyncHelper.CalculateDifferences(convertedAssignments, task.Assignments, Model.Jc.Assignment.UserTaskComparer);
			foreach (var removed in difference.Removed)
			{
				ct.ThrowIfCancellationRequested();
				log.Debug("Removing assignment, task: " + removed.Task?.Name + "; user: " + removed.User?.Email);
				jcAdapter.Remove(removed);
				task.Assignments.Remove(removed);
			}
			foreach (var added in difference.Added)
			{
				ct.ThrowIfCancellationRequested();
				jcAdapter.Create(added);
				// syncContext.AddUserMapping(new Guid(), added.User.Id);
			}

			foreach (var common in difference.Common)
			{
				ct.ThrowIfCancellationRequested();
				if (common.Target.TryUpdate(common.Source))
				{
					jcAdapter.Update(common.Target);
				}
			}
		}

		private void setStartDateEndDateNull(Model.Jc.Task task)
		{
			task.StartDate = null;
			task.EndDate = null;
		}

		private void removeAssignments(IEnumerable<Model.Jc.Assignment> assignments, CancellationToken ct)
		{
			foreach (var jcAssignment in assignments)
			{
				log.Info("Removing assignment");
				ct.ThrowIfCancellationRequested();
				jcAdapter.Remove(jcAssignment);
			}
			return;
		}

		private void syncWorklogsToJira(CancellationToken ct, Interval interval)
		{
			log.Info("Synchronizing worklogs to Jira...");
			var jiraUsers = jiraAdapter.GetUsers();
			ct.ThrowIfCancellationRequested();
			List<Model.Jira.JiraWorklog> jiraWorklogs = jiraAdapter.GetWorklogs(interval, ct);
			ct.ThrowIfCancellationRequested();
			List<int> taskIds = GetJcProjectIds();

			var jcWorkTimes = jcAdapter.GetWorkTimes(interval, taskIds.ToArray());
			List<Model.Jira.JiraWorklog> jcWorklogs = jiraAdapter.ConvertJcWorkTimes(jcWorkTimes);
			log.Info("Creating worklogs on Jira...");

			List<Model.Jira.JiraWorklog> deletableWorklogs = new List<Model.Jira.JiraWorklog>();
			foreach (var worklog in jiraWorklogs)
			{
				if (jiraWorklogs.Contains(worklog, new Model.Jira.JiraWorklog.StartDateEqualityComparer())
					&& !jcWorklogs.Contains(worklog, new Model.Jira.JiraWorklog.FullEqualityComparer()))
				{
					deletableWorklogs.Add(worklog);
				}
			}
			foreach (var worklog in deletableWorklogs)
			{
				jiraAdapter.deleteWorklog(worklog);
			}
			log.InfoFormat("{0} worklogs deleted.", deletableWorklogs.Count());
			var comparer = new Model.Jira.JiraWorklog.FullEqualityComparer();
			var creatableWorklogs = jcWorklogs.Except(jiraWorklogs, comparer);
			int sum = 0;
			foreach (var worklog in creatableWorklogs)
			{
				ct.ThrowIfCancellationRequested();
				sum++;
				jiraAdapter.createWorklog(worklog);
			}
			log.InfoFormat("{0} worklogs created.", sum);
		}

		private List<int> GetJcProjectIds()
		{
			log.Info("Getting project ids...");
			Stopwatch sw = Stopwatch.StartNew();
			List<int> taskIds = new List<int>();
			foreach (var rootProject in GetRootProjects())
			{
				if (rootProject.IsClosed) continue;
				foreach (var task in rootProject.ChildrenTasks)
				{
					taskIds.Add(task.Id);
				}
				foreach (var project in rootProject.ChildrenProjects)
				{
					foreach (var task in project.ChildrenTasks)
					{
						taskIds.Add(task.Id);
					}
				}
			}
			sw.Stop();
			log.InfoFormat("Got {0} project id. Took {1} ms.", taskIds.Count, sw.Elapsed.TotalMilliseconds);
			return taskIds;
		}
	}
}
