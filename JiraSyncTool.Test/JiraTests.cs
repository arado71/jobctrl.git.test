using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JiraSyncTool.Jira.Interface;
using JiraSyncTool.Jira.Logic;
using JiraSyncTool.Jira.Model.Jc;
using JiraSyncTool.Jira.Model.Jira;
using Moq;
using Newtonsoft.Json;
using Xunit;
// ReSharper disable ReplaceWithSingleAssignment.True

namespace JiraSyncTool.Test
{
	public class JiraTests
	{
		public readonly string[] ClosedStatuses = { "Closed", "Closed2", "Closed3" };
		public readonly string[] IssuePrioritiesName = { "Highest", "High", "Medium", "Low", "Lowest" };
		public const int RootProjectId = 123456;

		public Mock<IJiraAdapter> JiraAdapterMock;
		public Mock<IJcAdapter> JcAdapterMock;

		private readonly List<Task> jcTasks = new List<Task>();
		private readonly List<Project> jcProjects = new List<Project>();
		private readonly List<Project> jcRootProjects = new List<Project>();
		private readonly List<User> jcUsers = new List<User>();
		private readonly List<Assignment> jcAssignments = new List<Assignment>();
		private readonly List<JiraStatus> issueStatuses = new List<JiraStatus>();
		private readonly List<JiraIssuePriority> issuePriorities = new List<JiraIssuePriority>();

		private readonly List<JiraProject> jiraProjects = new List<JiraProject>();
		private readonly List<JiraIssue> jiraIssues = new List<JiraIssue>();
		private readonly List<JiraUser> jiraUsers = new List<JiraUser>();

		private readonly CancellationTokenSource cts = new CancellationTokenSource();

		private void Initialize()
		{
			clearLists();
			setupIssueStatuses();
			setupIssuePriorities();
			setupJcRootProject();
			setupJiraAdapter();
			setupJcAdapter();
		}

		private void clearLists()
		{
			jcTasks.Clear();
			jcProjects.Clear();
			jcRootProjects.Clear();
			jcUsers.Clear();
			jcAssignments.Clear();

			jiraProjects.Clear();
			jiraIssues.Clear();
			jiraUsers.Clear();

			issuePriorities.Clear();
			issueStatuses.Clear();
		}

		private void setupIssueStatuses()
		{
			int i = 0;
			JiraStatusCategory jsc = new JiraStatusCategory()
			{
				Id = 1,
				Key = "To-Do"
			};
			while (i < 5)
			{
				JiraStatus js = new JiraStatus()
				{
					Description = "Some discription for status" + i,
					Id = i.ToString(),
					Name = "status" + i,
					StatusCategory = jsc
				};
				issueStatuses.Add(js);
				i++;
			}
			JiraStatusCategory closedJSC = new JiraStatusCategory()
			{
				Id = 2,
				Key = "Done"
			};
			foreach (var status in ClosedStatuses)
			{
				JiraStatus js = new JiraStatus()
				{
					Description = "Some discription for " + status,
					Id = i++.ToString(),
					Name = status,
					StatusCategory = closedJSC
				};
				issueStatuses.Add(js);               
			}
		}

		private void setupIssuePriorities()
		{
			int i = 0;
			foreach (var statusName in IssuePrioritiesName)
			{
				JiraIssuePriority jip = new JiraIssuePriority()
				{
					Name = statusName,
					Id = i++,
					Description = statusName + " priority description"
				};
				issuePriorities.Add(jip);
			}
		}

		private void setupJcRootProject()
		{
			var project = new Project()
			{
				Description = "JC root project",
				Id = RootProjectId,
				IsClosed = false,
				Name = "RootProject",
				ChildrenProjects = new List<Project>()
			};
			jcRootProjects.Add(project);
		}

		private void setupJiraAdapter()
		{
			JiraAdapterMock = new Mock<IJiraAdapter>();
			// GetProjects()
			JiraAdapterMock.Setup(ja => ja.GetProjects()).Returns(jiraProjects);
			// GetSubIssuesForIssue(string issueKey)
			JiraAdapterMock.Setup(ja => ja.GetSubIssuesForIssue(It.IsAny<string>()))
				.Returns((string s) =>
				{
					return jiraIssues.Where(i => i.ParentKey == s).ToList();
				});
			// GetUser(string username)
			JiraAdapterMock.Setup(ja => ja.GetUser(It.IsAny<string>()))
				.Returns((string s) => jiraUsers.FirstOrDefault(u => u.Username == s));
			// GetIssuesForProject(string projectKey)
			JiraAdapterMock.Setup(ja => ja.GetIssuesForProject(It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns((string s, CancellationToken cts) => jiraIssues.Where(i => i.Project.Key == s).ToList());
			// HasSubIssues(string issueKey)
			JiraAdapterMock.Setup(ja => ja.HasSubIssues(It.IsAny<string>()))
				.Returns((string s) => jiraIssues.Any(i => s == i.ParentKey));
			// IsClosedIssue(string issueKey)
			JiraAdapterMock.Setup(ja => ja.IsClosedIssue(It.IsAny<JiraIssue>()))
				.Returns((JiraIssue issue) =>
				{
					if (issue.Status == null) return false;
					return ClosedStatuses.Contains(issue.Status.Name);
				});
		}

		private void setupJcAdapter()
		{
			JcAdapterMock = new Mock<IJcAdapter>();
			// GetUsers()
			JcAdapterMock.Setup(jc => jc.GetUsers())
				.Returns(() => jcUsers);
			// GetRootProjects()
			JcAdapterMock.Setup(jc => jc.GetRootProjects())
				.Returns(() => jcRootProjects);
			// Create(Task t)
			JcAdapterMock.Setup(jc => jc.Create(It.IsAny<Task>()))
				.Callback((Task t) =>
				{
					t.Id = jcTasks.Count;
					jcTasks.Add(t);
				})
				.Returns((Task t) => t.Id);
			// Create(Project p)
			JcAdapterMock.Setup(jc => jc.Create(It.IsAny<Project>()))
				.Callback((Project p) =>
				{
					p.Id = jcProjects.Count;
					jcProjects.Add(p);
				})
				.Returns((Project p) => p.Id);
			// Create(Assignment a)
			JcAdapterMock.Setup(jc => jc.Create(It.IsAny<Assignment>()))
				.Callback((Assignment a) =>
				{
					jcAssignments.Add(a);

				});
			// Update(Task t)
			JcAdapterMock.Setup(jc => jc.Update(It.IsAny<Task>()))
				.Callback((Task t) =>
				{
					var taskToUpdate = jcTasks.SingleOrDefault(task => task.Equals(t));
					taskToUpdate?.TryUpdate(t);
				});
			// Update(Project p)
			JcAdapterMock.Setup(jc => jc.Update(It.IsAny<Project>()))
				.Callback((Project p) =>
				{
					var projectToUpdate = jcProjects.SingleOrDefault(project => project.Equals(p));
					projectToUpdate?.TryUpdate(p);
				});
			// Update(Assignment a)
			JcAdapterMock.Setup(jc => jc.Update(It.IsAny<Assignment>()))
				.Callback((Assignment a) =>
				{
					var assignmentToUpdate = jcAssignments.SingleOrDefault(assignment => assignment.Equals(a));
					assignmentToUpdate?.TryUpdate(a);
				});
			// EnsureClosed(Project p)
			JcAdapterMock.Setup(jc => jc.EnsureClosed(It.IsAny<Project>()))
				.Callback((Project p) =>
				{
					foreach (var assignment in jcAssignments)
					{
						if (assignment.Task.Parent.Equals(p))
						{
							jcAssignments.Remove(assignment);
						}
					}
					foreach (var project in jcProjects)
					{
						if (p.Equals(project.Parent))
							JcAdapterMock.Object.EnsureClosed(project);
						if (p.Equals(project))
							project.IsClosed = true;
					}
					foreach (var task in jcTasks)
					{
						if (p.Equals(task.Parent))
							JcAdapterMock.Object.EnsureClosed(task);
					}
					p.IsClosed = true;
				});
			// EnsureClosed(Task t)
			JcAdapterMock.Setup(jc => jc.EnsureClosed(It.IsAny<Task>()))
				.Callback((Task t) =>
				{
					foreach (var assignment in jcAssignments)
					{
						if (assignment.Task.Equals(t))
						{
							jcAssignments.Remove(assignment);
						}
					}
					jcTasks.Single(task => task.Equals(t)).IsClosed = true;
					t.IsClosed = true;
				});

			// Remove(Assignment a)
			JcAdapterMock.Setup(jc => jc.Remove(It.IsAny<Assignment>()))
				.Callback((Assignment a) =>
				{
					var assignmentToRemove = jcAssignments.SingleOrDefault(assignment => assignment.Equals(a));
					if (assignmentToRemove != null)
						jcAssignments.Remove(assignmentToRemove);
				});
			// GetUser(int i)
			JcAdapterMock.Setup(jc => jc.GetUser(It.IsAny<int>()))
				.Returns((int i) => jcUsers.SingleOrDefault(u => u.Id == i));
			// GetTask(int i)
			JcAdapterMock.Setup(jc => jc.GetTask(It.IsAny<int>()))
				.Returns((int i) => jcTasks.SingleOrDefault(t => t.Id == i));
			// GetProject(int i)
			JcAdapterMock.Setup(jc => jc.GetProject(It.IsAny<int>()))
				.Returns((int i) =>
				{
					Project res = jcProjects.SingleOrDefault(p => p.Id == i) ?? jcRootProjects.SingleOrDefault(p => p.Id == i);
					return res;
				});
			// Move(Project projectToMove, Project newParent)
			JcAdapterMock.Setup(jc => jc.Move(It.IsAny<Project>(), It.IsAny<Project>()))
				.Callback((Project projectToMove, Project newParent) =>
				{
					var project = jcProjects.SingleOrDefault(p => p.Equals(projectToMove));
					if (project != null)
					{
						project.Parent = newParent;
					}
				});
			// Move(Task taskToMove, Project newParent)
			JcAdapterMock.Setup(jc => jc.Move(It.IsAny<Task>(), It.IsAny<Project>()))
				.Callback((Task taskToMove, Project newParent) =>
				{
					var task = jcTasks.SingleOrDefault(t => t.Equals(taskToMove));
					if (task != null)
					{
						task.Parent = newParent;
					}
				});
			// GetJcUserId(string username)
			JcAdapterMock.Setup(sdc => sdc.GetJcUserId(It.IsAny<string>()))
				.Returns((string email) =>
				{
					int? res = null;
					var user = jcUsers.SingleOrDefault(u => u.Email == email);
					if (user != null)
						res = user.Id;
					return res;
				});
			// GetJcProjectId(string projectKey)
			JcAdapterMock.Setup(sdc => sdc.GetJcProjectId(It.IsAny<string>()))
				.Returns((string projectKey) =>
				{
					int? res = null;
					var project = jcProjects.SingleOrDefault(p => p.ExtId == projectKey);
					if (project != null)
						res = project.Id;
					return res;
				});
			// GetJcTaskId(string issueKey)
			JcAdapterMock.Setup(sdc => sdc.GetJcTaskId(It.IsAny<string>()))
				.Returns((string issueKey) =>
				{
					int? res = null;
					var task = jcTasks.SingleOrDefault(t => t.ExtId == issueKey);
					if (task != null)
						res = task.Id;
					return res;
				});
			// GetJiraIssueKey(int jcId)
			JcAdapterMock.Setup(sdc => sdc.GetJiraIssueKey(It.IsAny<int>()))
				.Returns((int jcId) =>
				{
					string res = null;
					var task = jcTasks.SingleOrDefault(t => t.Id == jcId);
					if (task != null)
						res = task.ExtId;
					return res;
				});
			// GetJcProjectIdForIssue(string issueKey)
			JcAdapterMock.Setup(sdc => sdc.GetJcProjectIdForIssue(It.IsAny<string>()))
				.Returns((string issueKey) =>
				{
					int? res = null;
					var project = jcProjects.SingleOrDefault(p => p.ExtId == issueKey);
					if (project != null)
						res = project.Id;
					return res;
				});
			// AddProjectMapping(string projectKey, int jcId)
			JcAdapterMock.Setup(sdc => sdc.AddProjectMapping(It.IsAny<string>(), It.IsAny<int>()))
				.Callback((string projectKey, int jcId) =>
				{
					var project = jcProjects.SingleOrDefault(p => p.Id == jcId);
					if (project != null)
						project.ExtId = projectKey;
				});
			// AddTaskMapping(string issueKey, int jcId)
			JcAdapterMock.Setup(sdc => sdc.AddTaskMapping(It.IsAny<string>(), It.IsAny<int>()))
				.Callback((string issueKey, int jcId) =>
				{
					var task = jcTasks.SingleOrDefault(t => t.Id == jcId);
					if (task != null)
						task.ExtId = issueKey;
				});
			// RemoveProjectMapping(string projectKey, int jcId)
			JcAdapterMock.Setup(sdc => sdc.RemoveProjectMapping(It.IsAny<string>(), It.IsAny<int>()))
				.Callback((string projectKey, int jcId) =>
				{
					var project = jcProjects.SingleOrDefault(p => p.Id == jcId);
					if (project != null)
						project.ExtId = null;
				});
			// RemoveTaskMapping(string issueKey, int jcId)
			JcAdapterMock.Setup(sdc => sdc.RemoveTaskMapping(It.IsAny<string>(), It.IsAny<int>()))
				.Callback((string issueKey, int jcId) =>
				{
					var task = jcTasks.SingleOrDefault(t => t.Id == jcId);
					if (task != null)
						task.ExtId = null;
				});
			// RemoveIssueProjectMapping(string issueKey, int jcId)
			JcAdapterMock.Setup(sdc => sdc.RemoveIssueProjectMapping(It.IsAny<string>(), It.IsAny<int>()))
				.Callback((string issueKey, int jcId) =>
				{
					var project = jcProjects.SingleOrDefault(p => p.Id == jcId);
					if (project != null)
						project.ExtId = null;
				});
		}

		private void checkIfSyncCorrect(IJiraAdapter jiraAdapter)
		{
			// check Jira projects
			foreach (var jiraProject in jiraProjects)
			{
				bool hasJcProject = false;
				foreach (var jcProject in jcProjects)
				{
					if (compare(jcProject, jiraProject))
					{
						hasJcProject = true;
						break;
					}
				}
				Assert.True(hasJcProject, $"Jira project with key ({jiraProject.Key}) has no suitable JC project");
			}
			// check Jira issues
			foreach (var jiraIssue in jiraIssues)
			{
				bool hasJcTask = false;
				Task jcTaskForIssue = null;
				// check wether the issue has appropiate project in jc
				if (jiraAdapter.HasSubIssues(jiraIssue.Key))
				{
					bool hasJcProject = false;
					foreach (var jcProject in jcProjects)
					{
						if (compare(jcProject, jiraIssue))
						{
							hasJcProject = true;
							break;
						}
					}
					Assert.True(hasJcProject, $"Jira issue with key ({jiraIssue.Key}) has no suitable JC project.");
				}
				// check wether the issue has appropriate task in jc
				foreach (var jcTask in jcTasks)
				{
					if (compare(jcTask, jiraIssue))
					{
						jcTaskForIssue = jcTask;
						hasJcTask = true;
						break;
					}
				}
				Assert.True(hasJcTask, $"Jira issue with key ({jiraIssue.Key}) has no suitable JC task.");
				// Check if the assignments exist in jc
				Assert.True(jcUsers.Count == jiraUsers.Count, "The number of Jira users doesn't equal with the number of JC users.");
				if (jiraUsers.Count > 0 && jcUsers.Count > 0 && jcTaskForIssue != null)
				{
					var jiraUser = jiraUsers.SingleOrDefault(ju => ju.Username == jiraIssue.Assignee.Username);
					if (jiraUser == null)
					{
						Assert.True(false, "Jira user with username ({0}) doesn't exist.");
					}
					var jcUser = jcUsers.SingleOrDefault(ju => ju.Email == jiraUser.Email);
					if (jcUser == null)
					{
						Assert.True(false, "JC user with username ({0}) doesn't exist.");
					}
					if (jcAssignments.Any(a => a.Task.Equals(jcTaskForIssue) && a.User.Equals(jcUser)))
					{
						Assert.True(false, $"Assignment not exist for user ({jcUser.Id}) and task ({jcTaskForIssue.Id})");
					}
				}
			}
			// Check wether every JC project has an appropriate element in Jira
			foreach (var jcProject in jcProjects)
			{
				bool hasJiraProjectOrIssue = false;
				if (jcProject.IsClosed)
					continue;
				if (jcRootProjects.Any(rp => rp.Equals(jcProject.Parent)))
				{
					hasJiraProjectOrIssue = jiraProjects.Where(jp => compare(jcProject, jp)).Any();
					Assert.True(hasJiraProjectOrIssue, $"JC Project with id ({jcProject.Id}) has no suitable Jira project.");
				}
				else
				{
					hasJiraProjectOrIssue = jiraIssues.Where(ji => compare(jcProject, ji)).Any();
					Assert.True(hasJiraProjectOrIssue, $"JC Project with id ({jcProject.Id}) has no suitable Jira issue.");
				}
			}
			// check wether every JC task has an appropriate Jira issue
			foreach (var jcTask in jcTasks)
			{
				if (jcTask.IsClosed)
					continue;
				bool hasJiraIssue = jiraIssues.Where(ji => compare(jcTask, ji)).Any();
				Assert.True(hasJiraIssue, $"JC Task with id({jcTask.Id}) has no suitable Jira issue");
			}
			// check wether every assignment in JC belongs to Assignment in Jira
			foreach (var assignment in jcAssignments)
			{
				if (jiraIssues.Single(ji => ji.Key == assignment.Task.ExtId).Assignee.Username
					!= jiraUsers.Single(ju => ju.Email == assignment.User.Email).Username)
				{
					Assert.True(false, $"JC assignment not found in Jira. User: {assignment.User.Id}, Task: {assignment.Task.Id}");
				}
			}
		}

		private bool compare(Project jcProject, JiraIssue jiraIssue)
		{
			bool res = true;
			if (jcProject.Name != jiraIssue.Summary)
			{
				res = false;
			}
			if (jcProject.Description != jiraIssue.Description)
			{
				res = false;
			}
			if (jcProject.Priority != PriorityConverter.ConvertJiraPriorityToJcPriority(jiraIssue.Priority.Name))
			{
				res = false;
			}

			return res;
		}

		private bool compare(Task jcTask, JiraIssue jiraIssue)
		{
			bool res = true;
			if (!(string.IsNullOrEmpty(jcTask.Description) && string.IsNullOrEmpty(jiraIssue.Description))
				&& jcTask.Description != jiraIssue.Description)
			{
				res = false;
			}
			if (!(string.IsNullOrEmpty(jcTask.Name) && string.IsNullOrEmpty(jiraIssue.Summary))
				&& jcTask.Name != jiraIssue.Summary)
			{
				res = false;
			}
			if (jcTask.Priority != PriorityConverter.ConvertJiraPriorityToJcPriority(jiraIssue.Priority.Name))
			{
				res = false;
			}
			return res;
		}

		private bool compare(Project jcProject, JiraProject jiraProject)
		{
			bool res = true;
			if (jcProject.Name != jiraProject.Name)
			{
				res = false;
			}
			return res;
		}

		/// <summary>
		/// Creating 1 project with 1 issue.
		/// </summary>
		[Fact]
		public void JiraTest1()
		{
			Initialize();
			var project1 = new JiraProject()
			{
				Key = "TP",
				Name = "Test Project",
				Id = "1"
			};
			jiraProjects.Add(project1);

			var issue1 = new JiraIssue()
			{
				Created = DateTime.Now,
				Key = "TP-1",
				Project = project1,
				Priority = issuePriorities.First(),
				Status = issueStatuses.First(),
				Summary = "Some issue summary",
				Description = "Some issue description"
			};

			jiraIssues.Add(issue1);

			ApplicationSync sync = new ApplicationSync(JiraAdapterMock.Object, JcAdapterMock.Object, RootProjectId);
			sync.SyncToJc(cts.Token);
			checkIfSyncCorrect(JiraAdapterMock.Object);
		}

		/// <summary>
		/// Creating 1 project with 2 issues and 2 subissues for 1 issues. Sync 2x.
		/// </summary>
		[Fact]
		public void JiraTest2()
		{
			Initialize();
			var project1 = new JiraProject()
			{
				Key = "TP",
				Name = "Test Project",
				Id = "1"
			};
			jiraProjects.Add(project1);

			var issue1 = new JiraIssue()
			{
				Created = DateTime.Now,
				Key = "TP-1",
				Project = project1,
				Priority = issuePriorities.First(),
				Status = issueStatuses.First(),
				Summary = "Some issue1 summary",
				Description = "Some issue1 description"
			};

			jiraIssues.Add(issue1);


			var issue2 = new JiraIssue()
			{
				Created = DateTime.Now,
				Key = "TP-2",
				Project = project1,
				Priority = issuePriorities.Last(),
				ParentKey = issue1.Key,
				Status = issueStatuses.Last(),
				Summary = "Some issue2 summary",
				Description = "Some issue2 description"
			};
			jiraIssues.Add(issue2);


			var issue3 = new JiraIssue()
			{
				Created = DateTime.Now,
				Key = "TP-3",
				Project = project1,
				Priority = issuePriorities.First(),
				ParentKey = issue1.Key,
				Status = issueStatuses.First(),
				Summary = "Some issue3 summary",
				Description = "Some issue3 description"
			};

			jiraIssues.Add(issue3);


			var issue4 = new JiraIssue()
			{
				Created = DateTime.Now,
				Key = "TP-4",
				Project = project1,
				Priority = issuePriorities.Last(),
				Status = issueStatuses.Last(),
				Summary = "Some issue4 summary",
				Description = "Some issue4 description"
			};

			jiraIssues.Add(issue4);
			ApplicationSync sync = new ApplicationSync(JiraAdapterMock.Object, JcAdapterMock.Object, RootProjectId);
			sync.SyncToJc(cts.Token);
			checkIfSyncCorrect(JiraAdapterMock.Object);
			sync.SyncToJc(cts.Token);
			checkIfSyncCorrect(JiraAdapterMock.Object);
		}

		/// <summary>
		/// Adding 3 issue (1 normal, 2 subissues), then update the non-sub issue's status to 'Closed'.
		/// </summary>
		[Fact]
		public void JiraTest3()
		{
			Initialize();
			var project1 = new JiraProject()
			{
				Key = "TP",
				Name = "Test Project",
				Id = "1"
			};
			jiraProjects.Add(project1);

			var issue1 = new JiraIssue()
			{
				Created = DateTime.Now,
				Key = "TP-1",
				Project = project1,
				Priority = issuePriorities.First(),
				Status = issueStatuses.First(),
				Summary = "Some issue1 summary",
				Description = "Some issue1 description"
			};


			jiraIssues.Add(issue1);


			var issue2 = new JiraIssue()
			{
				Created = DateTime.Now,
				Key = "TP-2",
				Project = project1,
				Priority = issuePriorities.Last(),
				ParentKey = issue1.Key,
				Status = issueStatuses.Last(),
				Summary = "Some issue2 summary",
				Description = "Some issue2 description"
			};
			jiraIssues.Add(issue2);


			var issue3 = new JiraIssue()
			{
				Created = DateTime.Now,
				Key = "TP-3",
				Project = project1,
				Priority = issuePriorities.First(),
				ParentKey = issue1.Key,
				Status = issueStatuses.First(),
				Summary = "Some issue3 summary",
				Description = "Some issue3 description"
			};

			jiraIssues.Add(issue3);

			ApplicationSync sync = new ApplicationSync(JiraAdapterMock.Object, JcAdapterMock.Object, RootProjectId);
			sync.SyncToJc(cts.Token);
			checkIfSyncCorrect(JiraAdapterMock.Object);

			jiraIssues.Remove(issue1);
			issue1.Status = issueStatuses.First(rs => ClosedStatuses.Contains(rs.Name));
			jiraIssues.Add(issue1);
			sync.SyncToJc(cts.Token);
			checkIfSyncCorrect(JiraAdapterMock.Object);
		}


		/// <summary>
		/// Create a user and add Assignee to 1 issue.
		/// </summary>
		[Fact]
		public void JiraTest4()
		{
			Initialize();
			// JiraUser's have tto be created this way, because every property's setter method is private and there is no constructor.
			string jiraUser1Json = JsonConvert.SerializeObject(new { name = "testusername", emailAddress = "test@test.hu" });
			JiraUser jiraUser1 = JsonConvert.DeserializeObject<JiraUser>(jiraUser1Json);

			User jcUser = new User
			{
				Email = "test@test.hu",
				Id = 1
			};

			var project1 = new JiraProject()
			{
				Key = "TP",
				Name = "Test Project",
				Id = "1"
			};
			jiraProjects.Add(project1);

			var issue1 = new JiraIssue()
			{
				Assignee = jiraUser1,
				Created = DateTime.Now,
				Key = "TP-1",
				Project = project1,
				Priority = issuePriorities.First(),
				Status = issueStatuses.First(),
				Summary = "Some issue1 summary",
				Description = "Some issue1 description"
			};

			jiraIssues.Add(issue1);

			ApplicationSync sync = new ApplicationSync(JiraAdapterMock.Object, JcAdapterMock.Object, RootProjectId);
			sync.SyncToJc(cts.Token);
			checkIfSyncCorrect(JiraAdapterMock.Object);
		}

		/// <summary>
		/// Create 1 issue, 2 user. First, assign the first user to it, sync, then assign the other one.
		/// </summary>
		[Fact]
		public void JiraTest5()
		{
			Initialize();
			// JiraUser's have tto be created this way, because every property's setter method is private and there is no constructor.
			string jiraUser1Json = JsonConvert.SerializeObject(new { name = "testusername", emailAddress = "test@test.hu" });
			JiraUser jiraUser1 = JsonConvert.DeserializeObject<JiraUser>(jiraUser1Json);

			User jcUser1 = new User
			{
				Email = "test@test.hu",
				Id = 1
			};

			string jiraUser2Json = JsonConvert.SerializeObject(new { name = "testusername2", emailAddress = "test2@test.hu" });
			JiraUser jiraUser2 = JsonConvert.DeserializeObject<JiraUser>(jiraUser2Json);

			User jcUser2 = new User
			{
				Email = "test2@test.hu",
				Id = 2
			};

			var project1 = new JiraProject()
			{
				Key = "TP",
				Name = "Test Project",
				Id = "1"
			};
			jiraProjects.Add(project1);

			var issue1 = new JiraIssue()
			{
				Assignee = jiraUser1,
				Created = DateTime.Now,
				Key = "TP-1",
				Project = project1,
				Priority = issuePriorities.First(),
				Status = issueStatuses.First(),
				Summary = "Some issue1 summary",
				Description = "Some issue1 description"
			};

			jiraIssues.Add(issue1);

			ApplicationSync sync = new ApplicationSync(JiraAdapterMock.Object, JcAdapterMock.Object, RootProjectId);
			sync.SyncToJc(cts.Token);
			checkIfSyncCorrect(JiraAdapterMock.Object);

			issue1.Assignee = jiraUser2;

			sync.SyncToJc(cts.Token);
			checkIfSyncCorrect(JiraAdapterMock.Object);
		}

	}

}
