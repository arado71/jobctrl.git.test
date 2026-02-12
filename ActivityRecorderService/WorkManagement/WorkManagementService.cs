using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderService.WebsiteServiceReference;

namespace Tct.ActivityRecorderService.WorkManagement
{
	public class WorkManagementService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public async Task<ProjectManagementConstraints> GetProjectManagementConstraintsAsync(int userId, int projectId)
		{
#if DEBUG
			var debugRes = GetProjectManagementConstraintsDebug(userId, projectId);
			if (debugRes != null) return debugRes;
#endif
			using (var context = new JobControlDataClassesDataContext())
			{
				using (var client = new Website.WebsiteClientWrapper())
				{
					var limits = await client.Client.GetCreateWorkLimitsAsync(new GetCreateWorkLimitsRequest(GetTicket(context, userId), projectId));
					if (limits.errorMessage != null) throw new FaultException("Cannot get create work limits: " + limits.errorMessage);
					var fields = await client.Client.GetMandatoryFieldsAsync(new GetMandatoryFieldsRequest(GetTicket(context, userId)));
					if (fields.errorMessage != null) throw new FaultException("Cannot get mandatory fields: " + fields.errorMessage);
					var access = await client.Client.GetProjectAccessAsync(new GetProjectAccessRequest(GetTicket(context, userId), new[] { projectId }));
					if (access.errorMessage != null) throw new FaultException("Cannot get project permissions: " + access.errorMessage);

					ManagementFields taskMandatoryFields, assignmentMandatoryFields;
					ConvertMandatoryFields(fields.GetMandatoryFieldsResult, out taskMandatoryFields, out assignmentMandatoryFields);

					var accRes = access.GetProjectAccessResult;
					return new ProjectManagementConstraints()
					{
						ProjectId = projectId,
						ProjectManagementPermissions = (int)(accRes != null && accRes.Length == 1 && accRes[0].ProjectId == projectId && accRes[0].HasAccess
							? ProjectManagementPermissions.CreateWork | ProjectManagementPermissions.ModifyWork | ProjectManagementPermissions.CloseWork
							: ProjectManagementPermissions.None),
						WorkMandatoryFields = (int)(taskMandatoryFields | assignmentMandatoryFields), //merged mandatory fields sent to client
						TaskMandatoryFields = taskMandatoryFields,
						AssignmentMandatoryFields = assignmentMandatoryFields,
						WorkMaxEndDate = limits.GetCreateWorkLimitsResult.EndDateMax,
						WorkMinStartDate = limits.GetCreateWorkLimitsResult.StartDateMin,
						WorkMaxTargetCost = limits.GetCreateWorkLimitsResult.MaxTargetCost,
						WorkMaxTargetWorkTime = limits.GetCreateWorkLimitsResult.MaxTargetPlannedTimeInMinutes.HasValue
							? TimeSpan.FromMinutes(limits.GetCreateWorkLimitsResult.MaxTargetPlannedTimeInMinutes.Value)
							: new TimeSpan?(),
					};
				}
			}
		}

		private static void ConvertMandatoryFields(MandatoryFields fields, out ManagementFields taskMandatoryFields, out ManagementFields assigmentMandatoryFields)
		{
			taskMandatoryFields = ManagementFields.None;
			assigmentMandatoryFields = ManagementFields.None;

			taskMandatoryFields |= (fields.Task & TaskMandatoryFields.Category) != 0 ? ManagementFields.Category : 0;
			taskMandatoryFields |= (fields.Task & TaskMandatoryFields.Description) != 0 ? ManagementFields.Description : 0;
			taskMandatoryFields |= (fields.Task & TaskMandatoryFields.PlannedCost) != 0 ? ManagementFields.TargetCost : 0;
			taskMandatoryFields |= (fields.Task & TaskMandatoryFields.PlannedWorkTime) != 0 ? ManagementFields.TargetWorkTime : 0;
			taskMandatoryFields |= (fields.Task & TaskMandatoryFields.Priority) != 0 ? ManagementFields.Priority : 0;
			taskMandatoryFields |= (fields.Task & TaskMandatoryFields.StartEndDate) != 0 ? ManagementFields.StartEndDate : 0;

			assigmentMandatoryFields |= (fields.Assignment & AssignmentMandatoryFields.PlannedWorkTime) != 0 ? ManagementFields.TargetWorkTime : 0;
			assigmentMandatoryFields |= (fields.Assignment & AssignmentMandatoryFields.Priority) != 0 ? ManagementFields.Priority : 0;
			assigmentMandatoryFields |= (fields.Assignment & AssignmentMandatoryFields.StartEndDate) != 0 ? ManagementFields.StartEndDate : 0;
		}

		private static void ValidateLimits(string name, ProjectManagementConstraints constraints, DateTime? startDate, DateTime? endDate, TimeSpan? targetWorkTimeDiff)
		{
			if (constraints.WorkMaxEndDate.HasValue
				&& endDate.HasValue
				&& endDate.Value > constraints.WorkMaxEndDate.Value)
			{
				throw new FaultException(name + " limit violated (" + constraints.WorkMaxEndDate.Value.ToString("yyyy-MM-dd") + ", " + endDate.Value.ToString("yyyy-MM-dd") + "): EndDate");
			}
			if (constraints.WorkMinStartDate.HasValue
				&& startDate.HasValue
				&& startDate.Value < constraints.WorkMinStartDate.Value)
			{
				throw new FaultException(name + " limit violated (" + constraints.WorkMinStartDate.Value.ToString("yyyy-MM-dd") + ", " + startDate.Value.ToString("yyyy-MM-dd") + "): StartDate");
			}
			if (constraints.WorkMaxTargetWorkTime.HasValue
				&& targetWorkTimeDiff.HasValue
				&& targetWorkTimeDiff.Value > constraints.WorkMaxTargetWorkTime.Value)
			{
				throw new FaultException(name + " limit violated (" + constraints.WorkMaxTargetWorkTime.Value + ", " + targetWorkTimeDiff.Value + "): TargetTotalWorkTime");
			}
			//we cannot set TargetCost atm. so it is not checked
		}

		private static void ValidateMandatoryFields(ManagementFields workMandatoryFields, WorkData workData)
		{
			if ((workMandatoryFields & ManagementFields.Category) != 0 && !workData.CategoryId.HasValue) throw new FaultException("Mandatory field missing: CategoryId");
			if ((workMandatoryFields & ManagementFields.Description) != 0 && string.IsNullOrEmpty(workData.Description)) throw new FaultException("Mandatory field missing: Description");
			if ((workMandatoryFields & ManagementFields.Priority) != 0 && !workData.Priority.HasValue) throw new FaultException("Mandatory field missing: Priority");
			if ((workMandatoryFields & ManagementFields.StartEndDate) != 0 && !workData.StartDate.HasValue) throw new FaultException("Mandatory field missing: StartDate");
			if ((workMandatoryFields & ManagementFields.StartEndDate) != 0 && !workData.EndDate.HasValue) throw new FaultException("Mandatory field missing: EndDate");
			if ((workMandatoryFields & ManagementFields.TargetWorkTime) != 0 && !workData.TargetTotalWorkTime.HasValue) throw new FaultException("Mandatory field missing: TargetTotalWorkTime");
			if ((workMandatoryFields & ManagementFields.TargetCost) != 0) throw new FaultException("Mandatory field missing: TargetCost"); //cannot manage works if TargetCost is required
		}

		public async Task<int> CreateWorkAsync(int userId, int projectId, WorkData workData)
		{
#if DEBUG
			var debugRes = CreateWorkDebug(userId, projectId, workData);
			if (debugRes.HasValue) return debugRes.Value;
#endif
			var constraints = await GetProjectManagementConstraintsAsync(userId, projectId);
			if (((ProjectManagementPermissions)constraints.ProjectManagementPermissions & ProjectManagementPermissions.CreateWork) == 0) throw new FaultException("CreateWork permission required");
			ValidateMandatoryFields((ManagementFields)constraints.WorkMandatoryFields, workData);
			ValidateLimits("CreateWork", constraints, workData.StartDate, workData.EndDate, workData.TargetTotalWorkTime);

			string ticket;
			using (var context = new JobControlDataClassesDataContext())
			{
				ticket = context.GetAuthTicket(userId);
			}
			using (var client = new Website.WebsiteClientWrapper())
			{
				var taskMandatoryFields = constraints.TaskMandatoryFields;

				var res = await client.Client.CreateWorkAndAssignmentAsync(new CreateWorkAndAssignmentRequest(
					new Guid(ticket), projectId, workData.Name, userId,
					(taskMandatoryFields & ManagementFields.StartEndDate) != 0 ? workData.StartDate.Value : new DateTime?(),
					(taskMandatoryFields & ManagementFields.StartEndDate) != 0 ? workData.EndDate.Value : new DateTime?(),
					null,
					(taskMandatoryFields & ManagementFields.TargetWorkTime) != 0 ? (int)workData.TargetTotalWorkTime.Value.TotalMinutes : new int?(),
					workData.Description,
					(taskMandatoryFields & ManagementFields.Priority) != 0 ? (short)workData.Priority.Value : new short?(),
					workData.ManualAddWorkDuration.HasValue ? (int)workData.ManualAddWorkDuration.Value.TotalMinutes : new int?(),
					workData.CategoryId, workData.IsForMobile, null,
					workData.StartDate, workData.EndDate, workData.TargetTotalWorkTime.HasValue ? (int)workData.TargetTotalWorkTime.Value.TotalMinutes : new int?(),
					workData.Priority.HasValue ? (short)workData.Priority.Value : new short?(), null, null, null, null));
				if (res.errorMessage != null) throw new FaultException("Cannot create work: " + res.errorMessage);
				return res.CreateWorkAndAssignmentResult; //workId
			}
		}

		public async Task UpdateWorkAsync(int userId, WorkData workData)
		{
#if DEBUG
			var debugRes = UpdateWorkDebug(userId, workData);
			if (debugRes) return;
#endif
			//todo description backward compatibility ?

			using (var context = new JobControlDataClassesDataContext())
			{
				var work = context.GetWorkById(workData.Id.Value);
				var constraints = await GetProjectManagementConstraintsAsync(userId, work.ProjectId);
				if (((ProjectManagementPermissions)constraints.ProjectManagementPermissions & ProjectManagementPermissions.ModifyWork) == 0) throw new FaultException("ModifyWork permission required");
				ValidateMandatoryFields((ManagementFields)constraints.WorkMandatoryFields, workData);

				var details = context.GetTaskAndAssignmentDetails(userId, workData.Id.Value);

				using (var client = new Website.WebsiteClientWrapper())
				{
					var taskMandatoryFields = constraints.TaskMandatoryFields;

					//check if we need to loosen constraints to be able to update the assignment
					var taskStartDate = (taskMandatoryFields & ManagementFields.StartEndDate) != 0 || details.TaskStartDate.HasValue
						? Min(workData.StartDate, details.TaskStartDate).Value
						: details.TaskStartDate;
					var taskEndDate = (taskMandatoryFields & ManagementFields.StartEndDate) != 0 || details.TaskEndDate.HasValue
						? Max(workData.EndDate, details.TaskEndDate).Value
						: details.TaskEndDate;
					var taskTotalWorkTimeDiff = Diff(details.AssigmentTargetTotalWorkTime, workData.TargetTotalWorkTime); //(taskMandatoryFields & ManagementFields.TargetWorkTime) != 0) enforced earlier
					var taskTotalWorkTime = (taskMandatoryFields & ManagementFields.TargetWorkTime) != 0 || details.TaskTargetTotalWorkTime.HasValue
						? (details.TaskTargetTotalWorkTime ?? TimeSpan.Zero) + (taskTotalWorkTimeDiff ?? TimeSpan.Zero)
						: details.TaskTargetTotalWorkTime;
					var taskPriority = (taskMandatoryFields & ManagementFields.Priority) != 0 ? (short)workData.Priority.Value : details.TaskPriority;

					if (workData.Name != details.Name
						|| taskStartDate != details.TaskStartDate
						|| taskEndDate != details.TaskEndDate
						|| taskTotalWorkTime != details.TaskTargetTotalWorkTime
						|| workData.Description != details.TaskDescription
						|| workData.Priority != taskPriority
						|| workData.ManualAddWorkDuration != details.TaskManualAddWorkDuration
						|| workData.CategoryId != details.TaskCategoryId
						|| workData.IsForMobile != details.TaskIsForMobile)
					{
						ValidateLimits("UpdateTask", constraints, taskStartDate, taskEndDate, taskTotalWorkTimeDiff);
						var resUw = await client.Client.UpdateWorkAsync(GetTicket(context, userId),
							workData.Id.Value, workData.Name, true,
							taskStartDate, taskEndDate,
							details.TaskTargetCost,
							taskTotalWorkTime.HasValue ? (int)taskTotalWorkTime.Value.TotalMinutes : new int?(),
							workData.Description,
							taskPriority,
							workData.ManualAddWorkDuration.HasValue ? (int)workData.ManualAddWorkDuration.Value.TotalMinutes : new int?(),
							workData.CategoryId, workData.IsForMobile, details.TaskCloseAfterInactiveHours);
						if (resUw.UpdateWorkResult != null) throw new FaultException("Cannot update task: " + resUw.UpdateWorkResult);
					}

					ValidateLimits("UpdateAssignment", constraints, workData.StartDate, workData.EndDate, taskTotalWorkTimeDiff);
					var resUa = await client.Client.UpdateAssignmentAsync(GetTicket(context, userId),
						workData.Id.Value, userId,
						workData.StartDate, workData.EndDate,
						workData.TargetTotalWorkTime.HasValue ? (int)workData.TargetTotalWorkTime.Value.TotalMinutes : new int?(),
						workData.Priority.HasValue ? (short)workData.Priority.Value : new short?(),
						workData.CloseReasonRequiredTime.HasValue ? (int)workData.CloseReasonRequiredTime.Value.TotalMinutes : new int?(),
						workData.CloseReasonRequiredDate,
						workData.CloseReasonRequiredTimeRepeatInterval.HasValue ? (int)workData.CloseReasonRequiredTimeRepeatInterval.Value.TotalMinutes : new int?(),
						workData.CloseReasonRequiredTimeRepeatCount);
					if (resUa.UpdateAssignmentResult != null) throw new FaultException("Cannot update assignment: " + resUa.UpdateAssignmentResult);
				}
			}
		}

		private static Guid GetTicket(JobControlDataClassesDataContext context, int userId)
		{
			return new Guid(context.GetAuthTicket(userId));
		}

		private static DateTime? Min(DateTime? date1, DateTime? date2)
		{
			if (!date1.HasValue) return date2;
			if (!date2.HasValue) return date1;
			return date1.Value < date2.Value ? date1 : date2;
		}

		private static DateTime? Max(DateTime? date1, DateTime? date2)
		{
			if (!date1.HasValue) return date2;
			if (!date2.HasValue) return date1;
			return date1.Value > date2.Value ? date1 : date2;
		}

		private static TimeSpan? Diff(TimeSpan? oldValue, TimeSpan? newValue)
		{
			if (!newValue.HasValue) return -oldValue;
			if (!oldValue.HasValue) return newValue;
			return newValue - oldValue;
		}

		public CloseWorkResult CloseWork(int userId, int workId, string reason, int? reasonItemId)
		{
#if DEBUG
			var debugRes = CloseWorkDebug(userId, workId, reason, reasonItemId);
			if (debugRes.HasValue) return debugRes.Value;
#endif
			string ticket;
			using (var context = new JobControlDataClassesDataContext())
			{
				//todo optimize ?
				//temp disable
				//var work = context.GetWorkById(workId);
				//var constraints = GetProjectManagementConstraints(userId, work.ProjectId);
				//if (((ProjectManagementPermissions)constraints.ProjectManagementPermissions & ProjectManagementPermissions.CloseWork) == 0) throw new FaultException("CloseWork permission required");
				ticket = context.GetAuthTicket(userId);
			}
			using (var client = new Website.WebsiteClientWrapper())
			{
				CloseWorkResult result;
				var res = client.Client.CloseTaskForUser(new Guid(ticket), workId, reason, reasonItemId);
				switch (res)
				{
					case CloseTaskForUserRet.OK:
						result = CloseWorkResult.Ok;
						break;
					case CloseTaskForUserRet.AlreadyClosed:
						result = CloseWorkResult.AlreadyClosed;
						break;
					case CloseTaskForUserRet.Other:
						result = CloseWorkResult.UnknownError;
						break;
					case CloseTaskForUserRet.ReasonRequired:
						result = CloseWorkResult.ReasonRequired;
						break;
					default:
						result = CloseWorkResult.UnknownError;
						log.Warn("CloseWork unknown result " + res);
						break;
				}
				return result;
			}
		}

#if DEBUG
		private static ProjectManagementConstraints GetProjectManagementConstraintsDebug(int userId, int projectId)
		{
			string newVersion;
			var svc = new ActivityRecorderService();
			var menu = svc.GetClientMenu(userId, "", out newVersion);
			var projectIds = new HashSet<int>();
			var counter = 0;
			var works = new Queue<WorkData>();
			works.Enqueue(new WorkData("root", null, null) { Children = menu.Works });
			while (works.Count > 0)
			{
				var curr = works.Dequeue();
				if (curr.Children == null) continue;
				for (int i = 0; i < curr.Children.Count; i++)
				{
					if (curr.Children[i].ProjectId.HasValue) projectIds.Add(curr.Children[i].ProjectId.Value);
					works.Enqueue(curr.Children[i]);
					counter++;
				}
			}

			if (!projectIds.Contains(projectId)) throw new FaultException("Cannot find project");

			return new ProjectManagementConstraints()
			{
				ProjectId = projectId,
				ProjectManagementPermissions = (int)(ProjectManagementPermissions.CreateWork | ProjectManagementPermissions.ModifyWork | ProjectManagementPermissions.CloseWork),
				WorkMandatoryFields = (int)(ManagementFields.Priority | ManagementFields.StartEndDate | ManagementFields.TargetWorkTime | ManagementFields.Description),
				WorkMinStartDate = null,
				WorkMaxEndDate = null,
				WorkMaxTargetCost = null,
				WorkMaxTargetWorkTime = null,
			};
		}

		private static bool UpdateWorkDebug(int userId, WorkData workData)
		{
			string newVersion;
			var svc = new ActivityRecorderService();
			var menu = svc.GetClientMenu(userId, "", out newVersion);
			Debug.Assert(workData.Children == null || workData.Children.Count == 0, "Don't send children"); //check Type ?
			Debug.Assert(!workData.ProjectId.HasValue, "One can only update works not projects atm.");
			Debug.Assert(workData.Id.HasValue, "Id missing");
			var workId = workData.Id.Value;
			var works = new Queue<WorkData>();
			works.Enqueue(new WorkData("root", null, null) { Children = menu.Works });
			while (works.Count > 0)
			{
				var curr = works.Dequeue();
				if (curr.Children == null) continue;
				for (int i = 0; i < curr.Children.Count; i++)
				{
					if (curr.Children[i].Id == workId)
					{
						Debug.Assert(curr.Children[i].Children == null || curr.Children[i].Children.Count == 0, "Updated children lost");
						curr.Children[i] = workData;
						svc.SetClientMenu(userId, menu);
						return true;
					}
					works.Enqueue(curr.Children[i]);
				}
			}
			throw new FaultException("Cannot update work");
		}

		private static int? CreateWorkDebug(int userId, int projectId, WorkData workData)
		{
			string newVersion;
			var svc = new ActivityRecorderService();
			var menu = svc.GetClientMenu(userId, "", out newVersion);
			Debug.Assert(workData.Children == null || workData.Children.Count == 0, "Don't send children"); //check Type ?
			Debug.Assert(!workData.ProjectId.HasValue, "One can only create works not projects atm.");
			var works = new Queue<WorkData>();
			var reservedIds = new HashSet<int>();
			works.Enqueue(new WorkData("root", null, null) { Children = menu.Works });
			var created = false;
			while (works.Count > 0)
			{
				var curr = works.Dequeue();
				if (curr.Children == null) continue;
				for (int i = 0; i < curr.Children.Count; i++)
				{
					if (curr.Children[i].Id.HasValue) reservedIds.Add(curr.Children[i].Id.Value);
					if (curr.Children[i].ProjectId.HasValue) reservedIds.Add(curr.Children[i].ProjectId.Value);
					if (curr.Children[i].ProjectId == projectId && !created)
					{
						curr.Children[i].Children.Add(workData);
						created = true;
					}
					works.Enqueue(curr.Children[i]);
				}
			}
			if (!created) throw new FaultException("Cannot create work");
			var id = 0;
			while (reservedIds.Contains(++id)) { }
			workData.Id = id;
			svc.SetClientMenu(userId, menu);
			return id;
		}

		private static CloseWorkResult? CloseWorkDebug(int userId, int workId, string reason, int? reasonItemId)
		{
			string newVersion;
			var svc = new ActivityRecorderService();
			var menu = svc.GetClientMenu(userId, "", out newVersion);
			var works = new Queue<WorkData>();
			works.Enqueue(new WorkData("root", null, null) { Children = menu.Works });
			while (works.Count > 0)
			{
				var curr = works.Dequeue();
				if (curr.Children == null) continue;
				for (int i = 0; i < curr.Children.Count; i++)
				{
					if (curr.Children[i].Id == workId)
					{
						if (workId % 2 == 1 && reason == null && !reasonItemId.HasValue)
						{
							return CloseWorkResult.ReasonRequired;
						}
						curr.Children.RemoveAt(i);
						svc.SetClientMenu(userId, menu);
						return CloseWorkResult.Ok;
					}
					works.Enqueue(curr.Children[i]);
				}
			}
			return CloseWorkResult.AlreadyClosed;
		}
#endif
	}
}
