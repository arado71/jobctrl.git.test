using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JiraSyncTool.Jira.Model;
using JiraSyncTool.Jira.Model.Jc;

namespace JiraSyncTool.Jira.Interface
{
	public interface IJcAdapter : IDisposable
	{
		int? GetJcUserId(string email);
		int? GetJcProjectId(string projectKey);
		int? GetJcTaskId(string issueKey);
		string GetJiraIssueKey(int jcId);
		int? GetJcProjectIdForIssue(string issueKey);
		void AddProjectMapping(string projectKey, int jcId);
		void AddTaskMapping(string issueKey, int jcId);
		void RemoveProjectMapping(string projectKey, int jcId);
		void RemoveTaskMapping(string issueKey, int jcId);
		void RemoveIssueProjectMapping(string issueKey, int jcId);
		List<User> GetUsers();
		List<Project> GetRootProjects();
		int Create(Task task);
		int Create(Project project);
		void Create(Assignment assignment);
		void Update(Task task);
		void Update(Project project);
		void Update(Assignment assignment);
		void EnsureClosed(Project project);
		void EnsureClosed(Task task);
		void EnsureOpened(Task task);
		void EnsureOpened(Project project);
		void Remove(Assignment assignmentToRemove);
		User GetUser(int userId);
		Task GetTask(int taskId);
		Project GetProject(int id);
		void Move(Project projectToMove, Project newParent);
		void Move(Task taskToMove, Project newParent);
		List<WorkTime> GetWorkTimes(Interval interval, int[] tasks);
	}
}
