using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Hotkeys;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.WorktimeHistory;

namespace Tct.ActivityRecorderClient.ProjectSync
{
	public abstract class ProjectSyncService : IProjectSyncService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		protected readonly WorkTimeService worktimeService;
		protected INotificationService notificationService;
		private static string ProjectSyncAuthPath { get { return "ProjectOnline-" + ConfigManager.UserId; } }
		private readonly object singleProcessLock = new object();
		private ProjectSyncUpdateManager syncUpdateManager;
		public Action<int,int> UpdateProgress { get; set; }

		public abstract void ShowSync();

		public abstract void ShowInfo(string text);

		protected ProjectSyncService(WorkTimeService worktimeService, INotificationService notificationService)
		{
			this.worktimeService = worktimeService;
			this.notificationService = notificationService;
			syncUpdateManager = new ProjectSyncUpdateManager(this);
			syncUpdateManager.Start(20000);
		}

		protected bool StartProcessIfStopped()
		{
			var res = Monitor.TryEnter(singleProcessLock);
			log.Debug("Monitor TryEnter=" + res);
			return res;
		}

		protected void EndProcess()
		{
			Monitor.Exit(singleProcessLock);
			log.Debug("Monitor exited");
		}

		public ProjectCredentials LoadCredentials()
		{
			if (!ProtectedDataSerializationHelper.Exists(ProjectSyncAuthPath)) return null;

			ProjectCredentials credentials;
			if (ProtectedDataSerializationHelper.Load(ProjectSyncAuthPath, out credentials))
			{
				return credentials;
			}

			return null;
		}

		public void SaveCredentials(ProjectCredentials credentials)
		{
			ProtectedDataSerializationHelper.Save(ProjectSyncAuthPath, credentials);
		}

		public SyncContext CreateContext()
		{
			try
			{
				var projectContext = new ProjectContext(ConfigManager.MsProjectAddress)
				{
					Credentials = CredentialCache.DefaultCredentials,
				};
				return CreateContext(projectContext);
			}
			catch (Exception ex)
			{
				log.Error("CreateContext failed", ex);
				throw;
			}
		}

		public SyncContext CreateContext(string username, string password)
		{
			try
			{
				var securePassword = new SecureString();
				foreach (var c in password)
				{
					securePassword.AppendChar(c);
				}

				var projectContext = new ProjectContext(ConfigManager.MsProjectAddress)
				{
					Credentials = username.Contains("@") ? (ICredentials)new SharePointOnlineCredentials(username, securePassword) : new NetworkCredential(username, securePassword),
				};
				return CreateContext(projectContext);
			}
			catch (Exception ex)
			{
				log.Error("CreateContext failed", ex);
				throw;
			}
		}

		private SyncContext CreateContext(ProjectContext projectContext)
		{
			log.Debug("started");
			var syncContext = new SyncContext()
			{
				ProjectContext = projectContext,
				Periods = GetPeriods(projectContext).ToArray(),
				Self = EnterpriseResource.GetSelf(projectContext),
			};

			projectContext.Load(syncContext.Self, x => x.Assignments, x => x.Assignments.IncludeWithDefaultProperties(a => a.Task));
			projectContext.ExecuteQuery();
			
			// workaround for load a lot of projects (more than 20) with child objects
			// based on https://social.technet.microsoft.com/Forums/azure/en-US/4fab5f62-5955-4257-af0f-a5e1fa58dca7/error-reading-project-custom-fields-via-csom-too-many-projects?forum=projectonline

			const int projectBlockSize = 20;

            //Query for all the projects first
			projectContext.Load(projectContext.Projects, proj => proj.Include(p => p.Id));
			projectContext.ExecuteQuery();
			var allIds = projectContext.Projects.Select(p => p.Id).ToArray();

            //get the number of blocks we will have
            int numBlocks = allIds.Length / projectBlockSize + 1;
            //Query all the child objects in blocks of 20
			for (int i = 0; i < numBlocks; i++)
			{
				log.DebugFormat("{0}/{1} projects loaded", i * projectBlockSize, allIds.Length);

				var idBlock = allIds.Skip(i * projectBlockSize).Take(projectBlockSize).ToArray();
				var block = new Guid[projectBlockSize]; //Zero'd Guid Array
				Array.Copy(idBlock, block, idBlock.Length);

				//some elements will be Zero'd guids at the end
				projectContext.Load(projectContext.Projects, x => x.Where(p =>
					p.Id == block[0] || p.Id == block[1] ||
					p.Id == block[2] || p.Id == block[3] ||
					p.Id == block[4] || p.Id == block[5] ||
					p.Id == block[6] || p.Id == block[7] ||
					p.Id == block[8] || p.Id == block[9] ||
					p.Id == block[10] || p.Id == block[11] ||
					p.Id == block[12] || p.Id == block[13] ||
					p.Id == block[14] || p.Id == block[15] ||
					p.Id == block[16] || p.Id == block[17] ||
					p.Id == block[18] || p.Id == block[19]
					).IncludeWithDefaultProperties(p => p.Tasks.IncludeWithDefaultProperties(t => t.Id), p => p.Assignments));

				//Success: The block will return the Project, Task and Assignments
				projectContext.ExecuteQuery();
				if (UpdateProgress != null)
					UpdateProgress(i, numBlocks);
			}

			//
			
			//projectContext.Load(projectContext.Projects, x => x.IncludeWithDefaultProperties(p => p.Tasks.IncludeWithDefaultProperties(t => t.Id)));
			//projectContext.ExecuteQuery();
			syncContext.TaskGuidAssignmentGuidLookup = syncContext.Self.Assignments.ToDictionary(x => x.Task.ProjectTaskId, x => x.Id);
			syncContext.TaskGuidProjectGuidLookup =
				projectContext.Projects.SelectMany(x => x.Tasks, (x, y) => new { Task = y.Id, Project = x.Id })
					.ToDictionary(x => x.Task, x => x.Project);

			log.Debug("finished");
			return syncContext;
		}

		private IEnumerable<TimeSheetPeriod> GetPeriods(ProjectContext context)
		{
			context.Load(context.TimeSheetPeriods, c => c.Where(p => p.Start < DateTime.Now));
			context.ExecuteQuery();
			foreach (var period in context.TimeSheetPeriods)
			{
				yield return period;
			}
		}

		private DeviceWorkIntervalLookup GetJcDeviceWorks(DateTime localDay)
		{
			var localDayResult = worktimeService.GetLocalDayInterval(localDay);
			if (localDayResult.Exception != null)
			{
				log.Error("Error occured while fetching local day interval: ", localDayResult.Exception);
				throw localDayResult.Exception;
			}

			var statResult = worktimeService.GetStats(localDayResult.Result);
			if (statResult.Exception != null)
			{
				log.Error("Error occured while fetching stats: ", localDayResult.Exception);
				throw statResult.Exception;
			}

			return statResult.Result;
		}

		private IEnumerable<Tuple<WorkOrProjectWithParentNames, TimeSpan>> GetWorktimes(DateTime localDay)
		{
			var deviceWorks = GetJcDeviceWorks(localDay);
			var workLookupResult = worktimeService.GetWorkOrProjectWithParentNames(deviceWorks.WorkIds);
			if (workLookupResult.Exception != null)
			{
				log.Error("Error occured while fetching works: ", workLookupResult.Exception);
				throw workLookupResult.Exception;
			}

			var workLookup = workLookupResult.Result.ToDictionary(x => x.WorkOrProjectName.Id, x => x);
			foreach (var workId in deviceWorks.WorkIds)
			{
				WorkOrProjectWithParentNames workOrProjectName;
				if (!workLookup.TryGetValue(workId, out workOrProjectName) || workOrProjectName == null)
				{
					log.Warn("Failed to find task #" + workId);
					continue;
				}

				var totalTime = new TimeSpan(deviceWorks.WorkTimeById[workId].Sum(x => x.Value.Ticks));
				yield return Tuple.Create(workOrProjectName, totalTime);
			}
		}

		private TimeSheetLine EnsureTaskLine(string taskName, Guid taskId, Guid assignmentId, TimeSheetSyncContext context)
		{
			TimeSheetLine line;
			if (!context.AssignmentGuidLineLookup.TryGetValue(assignmentId, out line))
			{
				Guid projectGuid;
				if (!context.TaskGuidProjectGuidLookup.TryGetValue(taskId, out projectGuid))
				{
					log.Warn("Failed to get project of " + taskId);
				}

				var newId = Guid.NewGuid();
				var newLine = context.TimeSheet.Lines.Add(new TimeSheetLineCreationInformation
				{
					Id = newId,
					LineClass = TimeSheetLineClass.StandardLine,
					AssignmentId = assignmentId,
					TaskName = taskName,
					ProjectId = projectGuid,
				});
				context.TimeSheet.Update();
				context.ProjectContext.Load(context.TimeSheet.Lines, x => x.Where(y => y.LineClass == TimeSheetLineClass.StandardLine).IncludeWithDefaultProperties(l => l.LineClass, l => l.Assignment, l => l.Assignment.Id, l => l.Work));
				context.ProjectContext.ExecuteQuery();
				line = context.TimeSheet.Lines.Single(x => x.Id == newId);
				context.AssignmentGuidLineLookup.Add(assignmentId, line);
			}

			return line;
		}

		private TimeSheetLine EnsureTaskLine(string taskName, TimeSheetSyncContext context)
		{
			TimeSheetLine line;
			if (!context.TaskNameLineLookup.TryGetValue(taskName, out line))
			{
				var newId = Guid.NewGuid();
				context.TimeSheet.Lines.Add(new TimeSheetLineCreationInformation()
				{
					Id = newId,
					LineClass = TimeSheetLineClass.StandardLine,
					TaskName = taskName,
					ProjectId = Guid.Empty,
					AssignmentId = Guid.Empty,
				});
				context.TimeSheet.Update();
				context.ProjectContext.Load(context.TimeSheet.Lines, x => x.Where(y => y.LineClass == TimeSheetLineClass.StandardLine).IncludeWithDefaultProperties(l => l.LineClass, l => l.Assignment, l => l.Assignment.Id, l => l.Work));
				context.ProjectContext.ExecuteQuery();
				line = context.TimeSheet.Lines.Single(x => x.Id == newId);
				context.TaskNameLineLookup.Add(taskName, line);
			}

			return line;
		}

		private TimeSheetWork EnsureWork(DateTime localDate, TimeSheetLine line)
		{
			Debug.Assert(localDate.Date == localDate, "localDate parameter is not date");
			var adminWork = line.Work.FirstOrDefault(x => x.IsPropertyAvailable("Start") && x.Start == localDate);
			if (adminWork == null)
			{
				log.Warn("Work isn't created");
				adminWork = line.Work.Add(new TimeSheetWorkCreationInformation
				{
					Start = localDate,
					End = localDate.AddDays(1).AddSeconds(-1),
					ActualWork = "0h",
					NonBillableOvertimeWork = "0",
					NonBillableWork = "0",
					OvertimeWork = "0",
					PlannedWork = "0h",
					Comment = "Created by JobCTRL",
				});
			}

			return adminWork;
		}

		private void SetWork(string workName, DateTime localDate, TimeSpan workTime, TimeSheetSyncContext context)
		{
			var line = EnsureTaskLine(workName, context);
			var work = EnsureWork(localDate, line);
			work.ActualWorkTimeSpan = workTime;
		}

		private void SetWork(string workName, Guid msTaskId, Guid assignmentId, DateTime localDate, TimeSpan workTime,
			TimeSheetSyncContext context)
		{
			var line = EnsureTaskLine(workName, msTaskId, assignmentId, context);
			var work = EnsureWork(localDate, line);
			work.ActualWorkTimeSpan = workTime;
		}

		private void UpdateTimesheetDay(DateTime day, TimeSheetSyncContext context)
		{
			foreach (var worktime in GetWorktimes(day))
			{
				Guid msTaskId;
				if (!Guid.TryParse(worktime.Item1.WorkOrProjectName.ExtId, out msTaskId))
				{
					log.DebugFormat("External id '{0}' not recognized as valid Id", msTaskId);
					SetWork(worktime.Item1.FullName, day, worktime.Item2, context);
					continue;
				}

				Guid msAssignmentId;
				if (!context.TaskGuidAssignmentGuidLookup.TryGetValue(msTaskId, out msAssignmentId))
				{
					log.WarnFormat("External id '{0}' is not valid in MS Project", msTaskId);
					SetWork(worktime.Item1.FullName, day, worktime.Item2, context);
					continue;
				}

				SetWork(worktime.Item1.WorkOrProjectName.Name, msTaskId, msAssignmentId, day, worktime.Item2, context);
			}
		}

		private void UpdateTimesheet(bool shouldSubmit, TimeSheetSyncContext context)
		{
			foreach (var day in GetDays(context.Interval))
			{
				UpdateTimesheetDay(day, context);
			}

			context.TimeSheet.Update();
			if (context.TimeSheet.Status == TimeSheetStatus.Approved
				|| context.TimeSheet.Status == TimeSheetStatus.Submitted
				|| context.TimeSheet.Status == TimeSheetStatus.Rejected)
			{
				context.TimeSheet.Recall();
			}

			if (shouldSubmit) context.TimeSheet.Submit("Generated by JobCtrl");
			context.ProjectContext.ExecuteQuery();
		}

		public void SyncWorkTime(TimeSheetPeriod period, bool shouldSubmit, SyncContext context)
		{
			try
			{
				var timesheetSyncContext = new TimeSheetSyncContext(context)
				{
					TimeSheet = period.TimeSheet, // from other context
					Interval = new Interval(period.Start, period.End)
				};
				timesheetSyncContext.ProjectContext.Load(timesheetSyncContext.TimeSheet, x => x.Status, x => x.Lines);
				timesheetSyncContext.ProjectContext.Load(timesheetSyncContext.TimeSheet.Lines, x => x.IncludeWithDefaultProperties(l => l.Work));
				try
				{
					timesheetSyncContext.ProjectContext.ExecuteQuery();
				}
				catch (ServerException ex)
				{
					if (ex.ServerErrorTypeName == "Microsoft.SharePoint.Client.ClientServiceException") 
						throw new ApplicationException("TimesheetMissing");
					throw;
				}
				// This is a workaround, becuase if we try to load all at once, Assignments will be null everywhere
				foreach (var line in timesheetSyncContext.TimeSheet.Lines)
				{
					timesheetSyncContext.ProjectContext.Load(line, x => x.Assignment, x => x.Assignment.Id);
					timesheetSyncContext.ProjectContext.ExecuteQuery();
				}

				timesheetSyncContext.ProjectContext.ExecuteQuery();
				timesheetSyncContext.AssignmentGuidLineLookup = period.TimeSheet.Lines.Where(x => x.LineClass == TimeSheetLineClass.StandardLine && x.Assignment.ServerObjectIsNull == false).ToDictionary(x => x.Assignment.Id, x => x);
				timesheetSyncContext.TaskNameLineLookup = period.TimeSheet.Lines.Where(x => x.LineClass == TimeSheetLineClass.StandardLine && x.Assignment.ServerObjectIsNull == true).ToLookup(x => x.TaskName).ToDictionary(x => x.Key, x => x.FirstOrDefault());
				try
				{
					UpdateTimesheet(shouldSubmit, timesheetSyncContext);
				}
				catch (ServerException ex)
				{
					if (ex.Message.Contains("GeneralSecurityAccessDenied"))
						throw new ApplicationException("PeriodAlreadySubmitted");
					throw;
				}
			}
			catch (Exception ex)
			{
				log.Error("SyncWorkTime failed", ex);
				throw;
			}
		}

		/// <summary>
		/// Returns the days in an interval, where StartDate is inclusive, EndDate is exclusive
		/// </summary>
		/// <param name="localInterval"></param>
		/// <returns></returns>
		private IEnumerable<DateTime> GetDays(Interval localInterval)
		{
			DateTime currentDate = localInterval.StartDate.Date;
			while (currentDate < localInterval.EndDate)
			{
				yield return currentDate;
				currentDate = currentDate.AddDays(1);
			}
		}

		private class TimeSheetSyncContext : SyncContext
		{
			public TimeSheet TimeSheet { get; set; }
			public Interval Interval { get; set; }
			public Dictionary<Guid, TimeSheetLine> AssignmentGuidLineLookup { get; set; }
			public Dictionary<string, TimeSheetLine> TaskNameLineLookup { get; set; }

			public TimeSheetSyncContext(SyncContext context)
				: base(context)
			{
			}

		}

		internal void AutoSync(DateTime effDate, bool isNewWeek)
		{
			var credentials = LoadCredentials();
			if (credentials == null || string.IsNullOrEmpty(credentials.Password))
			{
				log.Warn("no stored password, go to manual upload...");
				ShowSync();
				return;
			}

			if (StartProcessIfStopped())
			{
				try
				{
					log.DebugFormat("started, date:{0}, newWeek:{1}", effDate.ToInvariantShortDateString(), isNewWeek);
					ShowInfo("Process started");
					var context = CreateContext(credentials.Username, credentials.Password);
					var neededPeriods = context.Periods.Where(p => p.End >= effDate && (isNewWeek || p.Start <= effDate)).ToList();
					foreach (var period in neededPeriods)
						try
						{
							SyncWorkTime(period, isNewWeek, context);
						}
						catch (ApplicationException ex)
						{
							string displayMessage;
							switch (ex.Message)
							{
								case "PeriodAlreadySubmitted":
									displayMessage = "Period [{0}-{1}] already submitted!";
									break;
								case "TimesheetMissing":
									displayMessage = "Timesheet doesn't exist for period [{0}-{1}].\nPlease create it on 'Manage Timesheet' pane in PWA!";
									break;
								default:
									displayMessage = "An error occurred during uploading timesheet";
									break;
							}
							ShowInfo(string.Format( displayMessage, period.Start.ToShortDateString(), period.End.ToShortDateString()));
							throw;
						}
					ShowInfo("Process finished");
					log.Debug("finished");
				}
				catch (ApplicationException ex)
				{
					throw;
				}
				catch (Exception ex)
				{
					ShowInfo("An error occurred during uploading timesheet");
					throw;
				}
				finally
				{
					EndProcess();
				}
			}
			else
			{
				log.Debug("manual update in progress, autosync skipped");
			}
		}
	}
}
