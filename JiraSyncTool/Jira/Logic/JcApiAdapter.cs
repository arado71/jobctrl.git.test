using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JiraSyncTool.jobCTRLAPI;
using JiraSyncTool.Jira.Exceptions;
using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Jira.Model;
using JiraSyncTool.Jira.Model.Jc;
using JiraSyncTool.Jira.Utils;
using log4net;
using WorkTime = JiraSyncTool.Jira.Model.Jc.WorkTime;

namespace JiraSyncTool.Jira.Logic
{
	public class JcApiAdapter : IJcAdapter
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(JcApiAdapter));
		private readonly Guid authCode;
		private readonly Dictionary<int, Task> taskCache = new Dictionary<int, Task>();
		private readonly Dictionary<int, Project> projectCache = new Dictionary<int, Project>();
		private readonly Dictionary<int, User> userCache = new Dictionary<int, User>();
		private readonly List<Project> rootProjects = new List<Project>();
		private readonly API api = new API();
		private int? rootId = null;

		public JcApiAdapter(Guid authCode)
		{
			this.authCode = authCode;
		}

		public void Dispose()
		{
			api.Dispose();
		}

		public List<User> GetUsers()
		{
			EnsureUsersCached();
			return userCache.Values.ToList();
		}

		private void EnsureUsersCached()
		{
			if (userCache.Count == 0)
			{
				log.Info("Getting users from JC server...");
				var workers = GetUsersFromServer();
				userJcLookup.Clear();
				foreach (var worker in workers)
				{
					if (!string.IsNullOrEmpty(worker.Email))
					{
						userJcLookup.Add(worker.Email, worker.UserId);
					}

					userCache.Add(worker.UserId, ConvertUser(worker));
				}
				log.InfoFormat("Got {0} users.", userCache.Count);
			}
		}

		private IEnumerable<Worker_v2> GetUsersFromServer()
		{
			OrganizationHierarchy_v2 hierarchy = null;
			var callResult = api.Execute(n => n.GetOrganizationHierarchy_v2(authCode, false, out hierarchy), "GetOrganizationHierarchy");
			if (callResult != GetOrganizationHierarchyRet.OK)
			{
				log.ErrorFormat("Failed to get organization hierarchy: {0}", callResult);
				throw new UnexpectedResultException("GetOrganizationHierarchy_v2 call failed with " + callResult);
			}

			if (hierarchy == null) throw new UnexpectedResultException("Hierarchy root is null");
			return hierarchy.Root.Flatten(x => x.Children).OfType<Worker_v2>();
		}

		private Task_v4 GetRootTaskOrProjectFromServer()
		{
			TaskHierarchy_v4 hierarchy = null;
			var callResult = api.Execute(n => n.GetTaskHierarchy_v3(authCode, true, out hierarchy), "GetTaskHierarchy");
			if (callResult != GetTaskHierarchyRet.OK)
			{
				log.ErrorFormat("Failed to get task hierarchy: {0}", callResult);
				throw new UnexpectedResultException("GetTaskHierarchy call failed with " + callResult);
			}

			if (hierarchy == null) throw new UnexpectedResultException("Hierarchy root is invalid");
			return hierarchy.Root;
		}

		private void EnsureTaskHierarchy()
		{
			if (rootId == null) RebuildTaskHierarchy();
		}

		private bool IsJiraProjectOrIssue(string key)
		{
			if (string.IsNullOrEmpty(key))
				return false;
			if (char.IsLetter(key[0]))
				return true;
			return false;
		}

		private void RebuildTaskHierarchy()
		{
			rootId = null;
			projectCache.Clear();
			taskCache.Clear();
			rootProjects.Clear();
			issueJcProjectLookup.Clear();
			issueJcTaskLookup.Clear();
			var rootTask = GetRootTaskOrProjectFromServer();
			var processQueue = new Queue<Task_v4>();
			var parentOfProjectLookup = new Dictionary<int, int>();
			var parentOfTaskLookup = new Dictionary<int, int>();
			processQueue.Enqueue(rootTask);
			while (processQueue.Count > 0)
			{
				var currentTaskOrProject = processQueue.Dequeue();
				switch (currentTaskOrProject.Type)
				{
					case TaskType.Project:
						var project = ConvertProject(currentTaskOrProject);
						if (currentTaskOrProject.ParentId != null)
						{
							parentOfProjectLookup.Add(currentTaskOrProject.Id, currentTaskOrProject.ParentId.Value);
						}
						else
						{
							rootProjects.Add(project);
						}
						if (!string.IsNullOrEmpty(currentTaskOrProject.ExtId))
						{
							if (issueJcProjectLookup.ContainsKey(currentTaskOrProject.ExtId))
							{
								log.WarnFormat("2 projects ({0}; {1}) exists with the same extId ({2}). First is used."
									, issueJcProjectLookup[currentTaskOrProject.ExtId], currentTaskOrProject.Id, currentTaskOrProject.ExtId);
							}
							else
							{
								issueJcProjectLookup.Add(currentTaskOrProject.ExtId, currentTaskOrProject.Id);
							}
						}

						projectCache.Add(project.Id, project);
						break;
					case TaskType.Work:
						try
						{
							var task = ConvertTask(currentTaskOrProject);
							if (currentTaskOrProject.ParentId != null)
							{
								parentOfTaskLookup.Add(currentTaskOrProject.Id, currentTaskOrProject.ParentId.Value);
							}
							else
							{
								log.Warn("Invalid NULL parent for task " + task.Id);
							}
							if (!string.IsNullOrEmpty(currentTaskOrProject.ExtId))
							{
								if (issueJcTaskLookup.ContainsKey(currentTaskOrProject.ExtId))
								{
									log.WarnFormat("2 tasks ({0}; {1}) exists with the same extId ({2}). First is used."
										, issueJcTaskLookup[currentTaskOrProject.ExtId], currentTaskOrProject.Id, currentTaskOrProject.ExtId);
								}
								else
								{
									issueJcTaskLookup.Add(currentTaskOrProject.ExtId, currentTaskOrProject.Id);
								}
							}

							task.Assignments = currentTaskOrProject.AssignedTo.Where(x=>x.Status == TaskStatus.Active).Select(x => ConvertAssignment(task, x)).ToList();
							taskCache.Add(task.Id, task);
						}
						catch (IdNotFoundException)
						{
							log.Warn("IdNotFound: " + currentTaskOrProject.Id + " (" + currentTaskOrProject.Name + ") parent: " + currentTaskOrProject.ParentId);
						}
						break;
					case TaskType.Root:
						rootId = currentTaskOrProject.Id;
						break;
					default:
						throw new NotImplementedException("Unknown currentTask type: " + currentTaskOrProject.Type);
				}

				foreach (var child in currentTaskOrProject.Children)
				{
					processQueue.Enqueue(child);
				}
			}

			if (rootId == null) throw new UnexpectedResultException("Returned object hierarchy has no root");
			Project parentProject;
			foreach (var parentOfProject in parentOfProjectLookup)
			{
				if (!projectCache.TryGetValue(parentOfProject.Value, out parentProject))
				{
					if (parentOfProject.Value == rootId.Value)
					{
						rootProjects.Add(projectCache[parentOfProject.Key]);
						continue;
					}

					log.WarnFormat("ParentId of project {0} is not a valid JC project", parentOfProject.Key);
					continue;
				}

				projectCache[parentOfProject.Value].ChildrenProjects.Add(projectCache[parentOfProject.Key]);
			}

			foreach (var parentOfTask in parentOfTaskLookup)
			{
				if (!projectCache.TryGetValue(parentOfTask.Value, out parentProject))
				{
					log.WarnFormat("ParentId of task {0} is not a valid JC project", parentOfTask.Key);
					continue;
				}

				projectCache[parentOfTask.Value].ChildrenTasks.Add(taskCache[parentOfTask.Key]);
				//var project = ConvertProject(parentOfTask);
			}
		}

		public List<Project> GetRootProjects()
		{
			if (rootProjects.Count == 0) RebuildTaskHierarchy();
			return rootProjects;
		}


		public void Update(Assignment assignment)
		{
			var callResult =
				api.Execute(n => n.UpdateTaskAssignment_v3(authCode, assignment.User.Id, assignment.Task.Id, assignment.StartDate,
					assignment.EndDate,
					assignment.Duration != null ? (int?)assignment.Duration.Value.TotalMinutes : null), "UpdateTaskAssignment");
			if (callResult != UpdateTaskAssignmentRet_v3.OK)
			{
				throw new UnexpectedResultException("UpdateTaskAssignment call failed with " + callResult);
			}

			Task task;
			if (taskCache.TryGetValue(assignment.Task.Id, out task))
			{
				task.Assignments = new List<Assignment>{assignment};
			}
		}

		public int Create(Task task)
		{
			Debug.Assert(!taskCache.Values.Any(x => ReferenceEquals(x, task)), "Task already cached!");
			if (task == null) throw new ArgumentNullException("task");
			if (task.IsFromServer) throw new ArgumentException("Task already created");
			if (task.Parent == null) throw new ArgumentException("task has no parent");
			EnsureTaskHierarchy();
			if (!projectCache.ContainsKey(task.Parent.Id)) throw new IdNotFoundException("Task parent id not found");
			log.DebugFormat("Creating task {0} with parent {1}", task.Name,
					task.Parent != null ? task.Parent.Id.ToString() : "NULL");
			if (task.Parent == null) throw new ArgumentException("task.Parent property can't be null");

			var taskId = CreateTaskOnServer(task);
			taskCache.Add(taskId, task);
			projectCache[task.Parent.Id].ChildrenTasks.Add(task);
			return taskId;
		}

		private int CreateTaskOnServer(Task task)
		{
			int workId = -1;
			// TODO: Description is missing here.
			var callResult =
				api.Execute(n => n.CreateWork_v4(authCode, task.Parent.Id, task.Name, task.Description, false, new int[] { }, ((short?)task.Priority) ?? 500,
					null, // AssignmentStartDate
					null, // AssignmentEndDate
					null, // AssignmentPlannedWorktimeInMinutes
					null, null, null, null, 
					out workId), "CreateWork");
			if (callResult != CreateWorkRet_v4.OK)
			{
				log.ErrorFormat("Unable to create task, error {0}", callResult);
				throw new UnexpectedResultException("Failed to create task");
			}
			task.Id = workId;
			return workId;
		}

		public int Create(Project project)
		{
			Debug.Assert(!projectCache.Values.Any(x => ReferenceEquals(x, project)), "Project already cached!");
			if (project == null) throw new ArgumentNullException("project");
			if (project.IsFromServer) throw new ArgumentException("Project already created");
			EnsureTaskHierarchy();
			Debug.Assert(rootId != null);
			log.DebugFormat("Creating project {0} with parent {1}", project.Name,
					project.Parent != null ? project.Parent.Id.ToString() : "NULL");
			var parentId = project.Parent != null ? project.Parent.Id : rootId.Value;
			var projectId = CreateProjectOnServer(project, parentId);
			projectCache.Add(projectId, project);
			if (parentId == rootId.Value)
			{
				rootProjects.Add(project);
			}
			else
			{
				projectCache[parentId].ChildrenProjects.Add(project);
			}

			return projectId;
		}

		private int CreateProjectOnServer(Project project, int parentId)
		{
			if (!projectCache.ContainsKey(parentId) && parentId != rootId) throw new IdNotFoundException("ParentId not found in cache");
			int projectId = -1;
			var callResult =
				api.Execute(n => n.CreateProject_v2(authCode, parentId, project.Name, new ProjectParticipant[] { }, project.Description,
					(int?)project.Duration?.TotalMinutes,   // TargetPlannedWorktimeInMinutes
					null,   // targetStartDate
					null,   // TargetEndDate 
					null, "", project.Priority, out projectId), "CreateProject");
			if (callResult == CreateProjectRet_v2.OK)
			{
				project.Id = projectId;
				return projectId;
			}
			else
			{
				log.ErrorFormat("Unable to create project, error {0}", callResult);
				throw new UnexpectedResultException("CreateProject call failed with " + callResult);
			}
		}

		public void Create(Assignment assignment)
		{
			EnsureTaskHierarchy();
			var callResult = api.Execute(n => n.AssignUsersToTasks_v4(authCode, new[] { assignment.Task.Id }, new[] { assignment.User.Id },
					ProjHelper.GetMinutes(assignment.Duration), assignment.StartDate, assignment.EndDate), "AssignUserToTasks");
			if (callResult != AssignUsersToTasksRet_v4.OK)
			{
				log.ErrorFormat("AssignUsersToTasks({0}, {1}, {2}, {3}, null, null) failed with {4}", authCode, assignment.Task.Id, assignment.User.Id, ProjHelper.GetMinutes(assignment.Duration), callResult);
				throw new UnexpectedResultException("AssignUsersToTasks call failed with " + callResult);
			}
			assignment.Task.Assignments.Add(assignment);
		}

		public void Move(Project projectToMove, Project newParent)
		{
			//Debug.Assert(projectCache.Values.Any(x => ReferenceEquals(x, projectToMove)), "projectToMove parameter not in cache!");
			//Debug.Assert(projectCache.Values.Any(x => ReferenceEquals(x, newParent)), "newParent parameter not in cache!");
			//EnsureTaskHierarchy();
			//if (!projectToMove.IsFromServer) throw new InvalidOperationException("Only projects from server can be moved");
			//Debug.Assert(rootId.HasValue);
			//var parentId = newParent != null ? newParent.Id : rootId.Value;
			//if ((projectToMove.Parent == null && newParent == null) ||
			//	(projectToMove.Parent != null && newParent != null && projectToMove.Parent.Id == newParent.Id)) return;
			//var callResult = api.Execute(n => n.MoveTask_v3(authCode, projectToMove.Id, parentId), "MoveTask");
			//if (callResult != MoveTaskRet_v3.OK)
			//{
			//	log.ErrorFormat("MoveTask({0}, {1}, {2}) failed with {3}", authCode, projectToMove.Id, parentId, callResult);
			//	throw new UnexpectedResultException("MoveTask call failed with " + callResult);
			//}

			//if (projectToMove.Parent == null)
			//{
			//	rootProjects.Remove(projectToMove);
			//}
			//else
			//{
			//	projectCache[projectToMove.Parent.Id].ChildrenProjects.Remove(projectToMove);
			//}

			//if (newParent == null)
			//{
			//	rootProjects.Add(projectToMove);
			//}
			//else
			//{
			//	projectCache[newParent.Id].ChildrenProjects.Add(projectToMove);
			//}
		}

		public void Move(Task taskToMove, Project newParent)
		{
			//Debug.Assert(taskCache.Values.Any(x => ReferenceEquals(x, taskToMove)), "taskToMove parameter not in cache!");
			//Debug.Assert(projectCache.Values.Any(x => ReferenceEquals(x, newParent)), "newParent parameter not in cache!");
			//EnsureTaskHierarchy();
			//if (!taskToMove.IsFromServer) throw new InvalidOperationException("Only projects from server can be moved");
			//Debug.Assert(rootId.HasValue);
			//var parentId = newParent != null ? newParent.Id : rootId.Value;
			//if ((taskToMove.Parent == null && newParent == null) ||
			//	(taskToMove.Parent != null && newParent != null && taskToMove.Parent.Id == newParent.Id)) return;
			//var callResult = api.Execute(n => n.MoveTask_v3(authCode, taskToMove.Id, parentId), "MoveTask");
			//if (callResult != MoveTaskRet_v3.OK)
			//{
			//	log.ErrorFormat("MoveTask({0}, {1}, {2}) failed with {3}", authCode, taskToMove.Id, parentId, callResult);
			//	throw new UnexpectedResultException("MoveTask call failed with " + callResult);
			//}

			//if (taskToMove.Parent != null)
			//{
			//	projectCache[taskToMove.Parent.Id].ChildrenTasks.Remove(taskToMove);
			//}

			//if (newParent != null)
			//{
			//	projectCache[newParent.Id].ChildrenTasks.Add(taskToMove);
			//}
		}

		public void Update(Task task)
		{
			Debug.Assert(taskCache.Values.Any(x => ReferenceEquals(x, task)), "task parameter not in cache!");
			if (!task.IsFromServer) throw new ArgumentException("Task is not from server");
			UpdateTaskOnServer(task);
		}

		private void UpdateTaskOnServer(Task task)
		{
			var callResult = api.Execute(n => n.UpdateWork_v4(authCode, task.Id, task.Name, task.Description, null, null,
				null,   // TargetPlannedWorktimeInMinutes
				null,   // TargetStartDate
				null,   // TargetEndDate
				null, true, task.Priority, null, false, null, null), "UpdateWork");
			if (callResult != UpdateWorkRet_v4.OK)
			{
				log.ErrorFormat("UpdateWork({0}, {1}, \"{2}\", \"\", null, null, {3}, {4}, {5}, null, true, {6}, null, false, null, null) failed with {7}",
					authCode, task.Id, task.Name, task.Duration.HasValue ? (int?)task.Duration.Value.TotalMinutes : null,
					task.StartDate, task.EndDate, task.Priority, callResult);
				throw new UnexpectedResultException("UpdateWork call failed with " + callResult);
			}
		}

		/// <summary>
		/// Updates <paramref name="project"/> properties on server.
		/// </summary>
		/// <param name="project"><see cref="Project"/> to close</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="project"/> is not from server.</exception>
		/// <exception cref="UnexpectedResultException">Thrown when update call fails on server.</exception>
		public void Update(Project project)
		{
			Debug.Assert(projectCache.Values.Any(x => ReferenceEquals(x, project)), "project parameter not in cache!");
			if (!project.IsFromServer) throw new ArgumentException("Project is not from server");
			UpdateProjectOnServer(project);
		}

		private void UpdateProjectOnServer(Project project)
		{
			var callResult = api.Execute(n => n.UpdateProject_v3(authCode, project.Id, project.Name, project.Description,
				(int?)project.Duration?.TotalMinutes, // TargetPlannedWorktimeInMinutes
				null, // TargetStartDate, 
				null, // TargetEndDate, 
				null, "", project.Priority, false, null), "UpdateProject");
			if (callResult != UpdateProjectRet_v3.OK)
			{
				log.ErrorFormat("Unable to update project {0}, error code: {1}", project.Id, callResult);
				throw new UnexpectedResultException("UpdateProject call failed with " + callResult);
			}
		}

		/// <summary>
		/// Closes project on server if open.
		/// </summary>
		/// <param name="project"><see cref="Project"/> to close</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="project"/> is not from server.</exception>
		/// <exception cref="UnexpectedResultException">Thrown when close call fails on server.</exception>
		public void EnsureClosed(Project project)
		{
			Debug.Assert(projectCache.Values.Any(x => ReferenceEquals(x, project)), "project parameter not in cache!");
			if (!project.IsFromServer) throw new ArgumentException("Project not from server");
			if (project.IsClosed)
			{
				log.DebugFormat("Project {0} was closed", project);
				return;
			}
			log.DebugFormat("Closing project {0}", project);
			CloseTaskOrProjectOnServer(project.Id);
			project.IsClosed = true;
		}

		/// <summary>
		/// Closes task on server if open.
		/// </summary>
		/// <param name="task">Task to close</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="task"/> is not from server.</exception>
		/// <exception cref="UnexpectedResultException">Thrown when close call fails on server.</exception>
		public void EnsureClosed(Task task)
		{
			//Debug.Assert(taskCache.Values.Any(x => ReferenceEquals(x, task)), "task parameter not in cache!");
			if (!task.IsFromServer) throw new ArgumentException("Task not from server");
			if (task.IsClosed)
			{
				log.DebugFormat("Task {0} was closed", task);
				return;
			}
			log.DebugFormat("Closing task {0}", task);
			CloseTaskOrProjectOnServer(task.Id);
			task.IsClosed = true;
		}

		private void CloseTaskOrProjectOnServer(int id)
		{
			var callResult = api.Execute(n => n.CloseTask(authCode, id), "CloseTask");
			if (callResult != CloseTaskRet.OK)
			{
				log.ErrorFormat("Failed to close {0}, error code: {1}", id, callResult);
				throw new UnexpectedResultException("CloseTask call failed with " + callResult);
			}
		}

		public void EnsureOpened(Project project)
		{
			Debug.Assert(projectCache.Values.Any(x => ReferenceEquals(x, project)), "project parameter not in cache!");
			if (!project.IsFromServer) throw new ArgumentException("Project not from server");
			if (!project.IsClosed) return;
			log.DebugFormat("Reopening project {0}", project);
			ReopenTaskOrProject(project.Id);
			project.IsClosed = false;
		}

		public void EnsureOpened(Task task)
		{
			//Debug.Assert(taskCache.Values.Any(x => ReferenceEquals(x, task)), "task parameter not in cache!");
			if (!task.IsFromServer) throw new ArgumentException("Task not from server");
			if (!task.IsClosed) return;
			log.DebugFormat("Reopening task {0}", task);
			ReopenTaskOrProject(task.Id);
			task.IsClosed = false;
		}

		private void ReopenTaskOrProject(int id)
		{
			var callResult = api.Execute(n => n.ReopenTask(authCode, id), "Reopen");
			if (callResult != ReopenTaskRet.OK)
			{
				log.ErrorFormat("Failed to reopen {0}, error code: {1}", id, callResult);
				throw new UnexpectedResultException("Reopen task call failed with " + callResult);
			}
		}

		public void Remove(Assignment assignmentToRemove)
		{
			var callResult =
				api.Execute(
					n => n.DeassignUsersFromTasks(authCode, new[] { assignmentToRemove.Task.Id }, new[] { assignmentToRemove.User.Id }),
					"DeassignUsersFromTasks");
			if (callResult != DeassignUsersFromTasksRet.OK)
			{
				throw new UnexpectedResultException("DeassignUsersFromTasks call failed with " + callResult);
			}
		}

		public User GetUser(int userId)
		{
			EnsureUsersCached();
			User user;
			if (!userCache.TryGetValue(userId, out user))
			{
				//log.WarnFormat("User {0} not found in JC", userId);
				return null;
			}

			return user;
		}

		public Task GetTask(int taskId)
		{
			EnsureTaskHierarchy();
			Task result;
			if (!taskCache.TryGetValue(taskId, out result))
			{
				throw new IdNotFoundException("Task id is invalid");
			}
			return result;
		}

		public Project GetProject(int projectId)
		{
			EnsureTaskHierarchy();
			Project result;
			if (!projectCache.TryGetValue(projectId, out result))
			{
				throw new IdNotFoundException("Project is invalid " + projectId);
			}

			return result;
		}

		private IEnumerable<ProjectReportItem_v2> TraverseBreadthFirst(ProjectReportItem_v2 startingTaskOrProject)
		{
			var tasksToVisit = new Queue<ProjectReportItem_v2>();
			tasksToVisit.Enqueue(startingTaskOrProject);
			while (tasksToVisit.Count > 0)
			{
				var currentTaskOrProject = tasksToVisit.Dequeue();
				foreach (var child in currentTaskOrProject.Children)
				{
					tasksToVisit.Enqueue(child);
				}
				yield return currentTaskOrProject;
			}
		}

		public List<Model.Jc.WorkTime> GetWorkTimes(Interval interval, int[] tasks)
		{
			log.Info("Getting worktimes from JC...");
			return ProcessDailyReport(GetWorkTimesFromServer(interval, tasks), tasks);
		}

		private List<Model.Jc.WorkTime> ProcessDailyReport(jobCTRLAPI.DailyWorktimeReport dailyWorktimeReport, int[] taskIds)
		{
			log.Info("Processing daily report...");
			List<Model.Jc.WorkTime> res = new List<Model.Jc.WorkTime>();
			EnsureUsersCached();
			foreach (var taskUserView in dailyWorktimeReport.TaskUserView)
			{
				if (!taskIds.Contains(taskUserView.TaskId)) continue;
				Task task = GetTask(taskUserView.TaskId);
				if (taskUserView.UserId == 0) continue;
				User user = GetUser(taskUserView.UserId);
				if (user == null) continue;
				foreach (var item in taskUserView.TimesByColumnList)
				{
					if (item.Key.Type != ReportColumnType.SingleDay) continue;
					long totalTime = item.Value.TotalGrossTotalTimeMs / 1000;
					if (totalTime == 0) continue;
					Model.Jc.WorkTime wt = new Model.Jc.WorkTime()
					{
						Duration = totalTime,
						StartDate = item.Key.Interval.StartDate.Date.AddHours(12),
						Task = task,
						UserId = user.Id,
						UserEmail = user.Email
					};
					res.Add(wt);
					
				}
			}
			log.Info("Daily report processed.");
			return res;
		}

		private jobCTRLAPI.DailyWorktimeReport GetWorkTimesFromServer(Interval interval, int[] tasks)
		{
			jobCTRLAPI.DailyWorktimeReport result = null;
			var start = interval.StartDate;
			var end = interval.EndDate.AddDays(-1); // Because the api adds a day to the endDate we have to subtract one
			var callResult = api.Execute(n => n.GetDailyWorktimeReport(authCode, start, end, tasks, GetUsers().Select(u => u.Id).ToArray(), null, out result), "GetDailyWorktimeReport");
			if (callResult != GetDailyWorktimeReportRet.OK)
			{
				log.ErrorFormat("Unable to get WorkTimeDailyReports with error {2}", callResult);
				throw new UnexpectedResultException("GetWorkTimeForUser call failed with " + callResult);
			}
			return result;
		}

		#region Object conversion

		private Project ConvertProject(Task_v4 project)
		{
			if (project.Type == TaskType.Root) return null;
			Debug.Assert(project.Type == TaskType.Project);
			return new Project
			{
				Id = project.Id,
				ExtId = project.ExtId,
				Name = project.Name,
				Description = project.Description,
				StartDate = project.TargetStartDate,
				EndDate = project.TargetEndDate,
				Duration = project.TargetPlannedWorkTimeInMinutes != null ? (TimeSpan?)TimeSpan.FromMinutes(project.TargetPlannedWorkTimeInMinutes.Value) : null,
				IsClosed = project.ClosedAt.HasValue,
				Priority = project.TaskPriority,
				ChildrenTasks = new List<Task>(),
				ChildrenProjects = new List<Project>(),
			};
		}

		private Task ConvertTask(Task_v4 task)
		{
			Debug.Assert(task.Type == TaskType.Work);
			Project parentProject = null;
			if (task.ParentId != null)
			{
				parentProject = GetProject(task.ParentId.Value);
			}

			return new Task
			{
				Id = task.Id,
				ExtId = task.ExtId,
				Name = task.Name,
				StartDate = task.TargetStartDate,
				EndDate = task.TargetEndDate,
				Duration = task.TargetPlannedWorkTimeInMinutes != null ? (TimeSpan?)TimeSpan.FromMinutes(task.TargetPlannedWorkTimeInMinutes.Value) : null,
				Priority = task.TaskPriority,
				IsClosed = task.ClosedAt.HasValue,
				Parent = parentProject,
				Description = task.Description
			};
		}

		private Assignment ConvertAssignment(Task task, TaskAssignment_v4 assignment)
		{
			Debug.Assert(assignment.UserId != null);
			var jcUser = GetUser(assignment.UserId.Value);
			return new Assignment
			{
				Task = task,
				User = jcUser,
				StartDate = assignment.StartDate,
				EndDate = assignment.EndDate,
				Duration = ProjHelper.GetDuration(assignment.PlannedWorkTimeInMinutes),
			};
		}

		private User ConvertUser(Worker_v2 worker)
		{
			if (worker == null) throw new ArgumentNullException("worker");
			return new User
			{
				Id = worker.UserId,
				FirstName = worker.FirstName,
				LastName = worker.LastName,
				Email = worker.Email,
			};
		}

		#endregion

		private readonly Dictionary<string, int> issueJcTaskLookup = new Dictionary<string, int>();
		private readonly Dictionary<string, int> issueJcProjectLookup = new Dictionary<string, int>();
		private readonly Dictionary<string, int> userJcLookup = new Dictionary<string, int>();

		public int? GetJcUserId(string email)
		{
			EnsureUsersCached();
			int result;
			return userJcLookup.TryGetValue(email, out result) ? (int?)result : null;
		}

		public int? GetJcProjectId(string projectKey)
		{
			EnsureTaskHierarchy();
			int result;
			return issueJcProjectLookup.TryGetValue(projectKey, out result) ? (int?)result : null;
		}

		public int? GetJcTaskId(string issueKey)
		{
			EnsureTaskHierarchy();
			int result;
			return issueJcTaskLookup.TryGetValue(issueKey, out result) ? (int?)result : null;
		}

		public string GetJiraIssueKey(int jcId)
		{
			EnsureTaskHierarchy();
			Task task;
			if (!taskCache.TryGetValue(jcId, out task))
			{
				return null;
			}
			return task.ExtId;
		}

		public int? GetJcProjectIdForIssue(string issueKey)
		{
			EnsureTaskHierarchy();
			int res;
			if (!issueJcProjectLookup.TryGetValue(issueKey, out res))
				return null;
			return res;
		}

		public void AddProjectMapping(string projectKey, int jcId)
		{
			EnsureTaskHierarchy();
			Project project;
			if (!projectCache.TryGetValue(jcId, out project))
			{
				throw new IdNotFoundException("Project " + jcId + " doesn't exist");
			}

			project.ExtId = projectKey;
			api.Execute(n => n.SetExternalIdForTask(authCode, jcId, project.ExtId), "SetExternalIdForTask");
			issueJcProjectLookup.Add(projectKey, jcId);
		}

		public void AddTaskMapping(string issueKey, int jcId)
		{
			EnsureTaskHierarchy();
			Task task;
			if (!taskCache.TryGetValue(jcId, out task))
			{
				throw new IdNotFoundException("Task " + jcId + " doesn't exist");
			}

			task.ExtId = issueKey;
			api.Execute(n => n.SetExternalIdForTask(authCode, jcId, task.ExtId), "SetExternalIdForTask");
			issueJcTaskLookup.Add(issueKey, jcId);
		}

		public void RemoveProjectMapping(string projectKey, int jcId)
		{
			EnsureTaskHierarchy();
			Project project;
			if (!projectCache.TryGetValue(jcId, out project))
			{
				throw new IdNotFoundException("Project " + jcId + " doesn't exist");
			}

			project.ExtId = "";
			api.Execute(n => n.SetExternalIdForTask(authCode, jcId, ""), "SetExternalIdForTask");
		}

		public void RemoveTaskMapping(string issueKey, int jcId)
		{
			EnsureTaskHierarchy();
			Project project;
			if (!projectCache.TryGetValue(jcId, out project))
			{
				throw new IdNotFoundException("Task " + jcId + " doesn't exist");
			}

			project.ExtId = "";
			api.Execute(n => n.SetExternalIdForTask(authCode, jcId, ""), "SetExternalIdForTask");
		}

		public void RemoveIssueProjectMapping(string issueKey, int jcId)
		{
			EnsureTaskHierarchy();
			Project project;
			if (!projectCache.TryGetValue(jcId, out project))
			{
				throw new IdNotFoundException("Project " + jcId + " doesn't exist");
			}

			project.ExtId = "";
			api.Execute(n => n.SetExternalIdForTask(authCode, jcId, ""), "SetExternalIdForTask");
		}
	}
}
