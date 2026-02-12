using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyDataRouter.ProxyServiceReference;

namespace ProxyDataRouter
{
	static class DataMapper
	{
		#region ActivityRecorderServiceReference to ProxyServiceReference

		public static AuthData To(ActivityRecorderServiceReference.AuthData authData)
		{
			if (authData == null) return null;
			return new AuthData
			{
				AccessLevel = To(authData.AccessLevel),
				Email = authData.Email,
				ExtensionData = authData.ExtensionData,
				Id = authData.Id,
				Name = authData.Name,
				TimeZoneData = authData.TimeZoneData,
			};
		}

		public static UserAccessLevel To(ActivityRecorderServiceReference.UserAccessLevel accessLevel)
		{
			switch (accessLevel)
			{
				case ActivityRecorderServiceReference.UserAccessLevel.Undefined:
					return UserAccessLevel.Undefined;
				case ActivityRecorderServiceReference.UserAccessLevel.Reg:
					return UserAccessLevel.Reg;
				case ActivityRecorderServiceReference.UserAccessLevel.Adm:
					return UserAccessLevel.Adm;
				case ActivityRecorderServiceReference.UserAccessLevel.Spv:
					return UserAccessLevel.Spv;
				case ActivityRecorderServiceReference.UserAccessLevel.Wrk:
					return UserAccessLevel.Wrk;
				default:
					throw new ArgumentOutOfRangeException(nameof(accessLevel), accessLevel, null);
			}
		}

		public static ClientSetting To(ActivityRecorderServiceReference.ClientSetting setting)
		{
			if (setting == null) return null;
			return new ClientSetting
			{
				ExtensionData = setting.ExtensionData,
				AfterWorkTimeIdleInMins = setting.AfterWorkTimeIdleInMins,
				AutoReturnFromMeeting = setting.AutoReturnFromMeeting,
				BusyTimeThreshold = setting.BusyTimeThreshold,
				CaptureActiveWindowInterval = setting.CaptureActiveWindowInterval,
				CaptureDeadlockInMins = setting.CaptureDeadlockInMins,
				CaptureScreenShotInterval = setting.CaptureScreenShotInterval,
				CaptureWorkItemInterval = setting.CaptureWorkItemInterval,
				CoincidentalClientsEnabled = setting.CoincidentalClientsEnabled,
				CollectedItemAggregateInMins = setting.CollectedItemAggregateInMins,
				DataCollectionSettings = setting.DataCollectionSettings,
				DefaultWorkId = setting.DefaultWorkId,
				DiagnosticOperationMode = setting.DiagnosticOperationMode,
				DuringWorkTimeIdleInMins = setting.DuringWorkTimeIdleInMins,
				DuringWorkTimeIdleManualInterval = setting.DuringWorkTimeIdleManualInterval,
				EnabledFeature = setting.EnabledFeature,
				ForceCountdownRules = setting.ForceCountdownRules,
				IsAnonymModeEnabled = setting.IsAnonymModeEnabled,
				IsGoogleCalendarTrackingEnabled = setting.IsGoogleCalendarTrackingEnabled,
				IsInjectedInputAllowed = setting.IsInjectedInputAllowed,
				IsLotusNotesMeetingTrackingEnabled = setting.IsLotusNotesMeetingTrackingEnabled,
				IsManualMeetingStartsOnLock = setting.IsManualMeetingStartsOnLock,
				IsMeetingSubjectMandatory = setting.IsMeetingSubjectMandatory,
				IsMeetingTentativeSynced = setting.IsMeetingTentativeSynced,
				IsMeetingTrackingEnabled = setting.IsMeetingTrackingEnabled,
				IsMeetingUploadModifications = setting.IsMeetingUploadModifications,
				IsNotificationShown = setting.IsNotificationShown,
				IsOutlookAddinMailTrackingId = setting.IsOutlookAddinMailTrackingId,
				IsOutlookAddinMailTrackingUseSubject = setting.IsOutlookAddinMailTrackingUseSubject,
				IsOutlookAddinRequired = setting.IsOutlookAddinRequired,
				IsTodoListEnabled = setting.IsTodoListEnabled,
				JpegQuality = setting.JpegQuality,
				JpegScalePct = setting.JpegScalePct,
				MailTrackingSettings = setting.MailTrackingSettings,
				ManualWorkItemEditAgeLimit = setting.ManualWorkItemEditAgeLimit,
				MaxManualMeetingInterval = setting.MaxManualMeetingInterval,
				MaxOfflineWorkItems = setting.MaxOfflineWorkItems,
				MenuUpdateInterval = setting.MenuUpdateInterval,
				MouseMovingThreshold = setting.MouseMovingThreshold,
				MsProjectAddress = setting.MsProjectAddress,
				PluginFailThreshold = setting.PluginFailThreshold,
				RuleMatchingInterval = setting.RuleMatchingInterval,
				RuleRestrictions = setting.RuleRestrictions,
				TelemetryCollectedKeys = setting.TelemetryCollectedKeys,
				TelemetryMaxAgeInMins = setting.TelemetryMaxAgeInMins,
				TelemetryMaxCount = setting.TelemetryMaxCount,
				TimeSyncThreshold = setting.TimeSyncThreshold,
				VoxIsManualStartStopEnabled = setting.VoxIsManualStartStopEnabled,
				VoxIsNameMandatory = setting.VoxIsNameMandatory,
				VoxQuality = setting.VoxQuality,
				WorkTimeEndInMins = setting.WorkTimeEndInMins,
				WorkTimeStartInMins = setting.WorkTimeStartInMins,
			};
		}

		public static ClientMenu To(ActivityRecorderServiceReference.ClientMenu menu)
		{
			if (menu == null) return null;
			return new ClientMenu
			{
				ExtensionData = menu.ExtensionData,
				CategoriesById = menu.CategoriesById?.ToDictionary(k => k.Key, v => To(v.Value)),
				ExternalCompositeMapping = To(menu.ExternalCompositeMapping),
				ExternalProjectIdMapping = menu.ExternalProjectIdMapping,
				ExternalWorkIdMapping = menu.ExternalWorkIdMapping,
				Works = menu.Works?.Select(w => To(w)).ToArray(),
			};
		}

		public static WorkData To(ActivityRecorderServiceReference.WorkData workData)
		{
			if (workData == null) return null;
			return new WorkData
			{
				ExtensionData = workData.ExtensionData,
				Id = workData.Id,
				Name = workData.Name,
				ExternalWorkIdMapping = workData.ExternalWorkIdMapping,
				CategoryId = workData.CategoryId,
				Children = workData.Children?.Select(c => To(c)).ToArray(),
				CloseReasonRequiredDate = workData.CloseReasonRequiredDate,
				CloseReasonRequiredTime = workData.CloseReasonRequiredTime,
				CloseReasonRequiredTimeRepeatCount = workData.CloseReasonRequiredTimeRepeatCount,
				CloseReasonRequiredTimeRepeatInterval = workData.CloseReasonRequiredTimeRepeatInterval,
				Description = workData.Description,
				EndDate = workData.EndDate,
				ExtId = workData.ExtId,
				IsDefault = workData.IsDefault,
				IsForMobile = workData.IsForMobile,
				IsReadOnly = workData.IsReadOnly,
				ManualAddWorkDuration = workData.ManualAddWorkDuration,
				Priority = workData.Priority,
				ProjectId = workData.ProjectId,
				StartDate = workData.StartDate,
				TargetTotalWorkTime = workData.TargetTotalWorkTime,
				TaxId = workData.TaxId,
				TemplateRegex = workData.TemplateRegex,
				Type = workData.Type,
				VisibilityType = workData.VisibilityType,
			};
		}

		public static CompositeMapping To(ActivityRecorderServiceReference.CompositeMapping mapping)
		{
			if (mapping == null) return null;
			return new CompositeMapping
			{
				ExtensionData = mapping.ExtensionData,
				WorkIdByKey = mapping.WorkIdByKey,
				ChildrenByKey = mapping.ChildrenByKey?.ToDictionary(k => k.Key, v => To(v.Value)),
			};
		}

		public static CategoryData To(ActivityRecorderServiceReference.CategoryData categoryData)
		{
			if (categoryData == null) return null;
			return new CategoryData
			{
				Id = categoryData.Id,
				ExtensionData = categoryData.ExtensionData,
				Name = categoryData.Name,
			};
		}

		public static WorkDetectorRule To(ActivityRecorderServiceReference.WorkDetectorRule rule)
		{
			if (rule == null) return null;
			return new WorkDetectorRule
			{
				ExtensionData = rule.ExtensionData,
				RuleType = To(rule.RuleType),
				RelatedId = rule.RelatedId,
				Name = rule.Name,
				IsEnabled = rule.IsEnabled,
				IsRegex = rule.IsRegex,
				IgnoreCase = rule.IgnoreCase,
				TitleRule = rule.TitleRule,
				ProcessRule = rule.ProcessRule,
				UrlRule = rule.UrlRule,
				IsPermanent = rule.IsPermanent,
				WorkSelector = To(rule.WorkSelector),
				KeySuffix = rule.KeySuffix,
				ServerId = rule.ServerId,
				ExtensionRulesByIdByKey = rule.ExtensionRulesByIdByKey,
				WindowScope = To(rule.WindowScope),
				IsEnabledInNonWorkStatus = rule.IsEnabledInNonWorkStatus,
				IsEnabledInProjectIds = rule.IsEnabledInProjectIds,
				ExtensionRuleParametersById = rule.ExtensionRuleParametersById?.ToDictionary(k => k.Key, v => v.Value?.Select(To).ToArray()),
				AdditionalActions = rule.AdditionalActions,
				FormattedNamedGroups = rule.FormattedNamedGroups,
				Children = rule.Children?.Select(To).ToArray(),
				IsDefault = rule.IsDefault,
			};
		}

		public static WorkDetectorRuleType To(ActivityRecorderServiceReference.WorkDetectorRuleType ruleType)
		{
			switch (ruleType)
			{
				case ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartWork:
					return WorkDetectorRuleType.TempStartWork;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.TempStopWork:
					return WorkDetectorRuleType.TempStopWork;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartCategory:
					return WorkDetectorRuleType.TempStartCategory;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.DoNothing:
					return WorkDetectorRuleType.DoNothing;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartProjectTemplate:
					return WorkDetectorRuleType.TempStartProjectTemplate;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartWorkTemplate:
					return WorkDetectorRuleType.TempStartWorkTemplate;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.EndTempEffect:
					return WorkDetectorRuleType.EndTempEffect;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.CreateNewRuleAndEndTempEffect:
					return WorkDetectorRuleType.CreateNewRuleAndEndTempEffect;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.CreateNewRuleAndTempStartWork:
					return WorkDetectorRuleType.CreateNewRuleAndTempStartWork;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartOrAssignWork:
					return WorkDetectorRuleType.TempStartOrAssignWork;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartOrAssignProject:
					return WorkDetectorRuleType.TempStartOrAssignProject;
				case ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartOrAssignProjectAndWork:
					return WorkDetectorRuleType.TempStartOrAssignProjectAndWork;
				default:
					throw new ArgumentOutOfRangeException(nameof(ruleType), ruleType, null);
			}
		}

		public static WorkSelector To(ActivityRecorderServiceReference.WorkSelector selector)
		{
			if (selector == null) return null;
			return new WorkSelector
			{
				ExtensionData = selector.ExtensionData,
				Name = selector.Name,
				IsRegex = selector.IsRegex,
				IgnoreCase = selector.IgnoreCase,
				Rule = selector.Rule,
				TemplateText = selector.TemplateText,
			};
		}

		public static WindowScopeType To(ActivityRecorderServiceReference.WindowScopeType scopeType)
		{
			switch (scopeType)
			{
				case ActivityRecorderServiceReference.WindowScopeType.Active:
					return WindowScopeType.Active;
				case ActivityRecorderServiceReference.WindowScopeType.VisibleOrActive:
					return WindowScopeType.VisibleOrActive;
				case ActivityRecorderServiceReference.WindowScopeType.Any:
					return WindowScopeType.Any;
				default:
					throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null);
			}
		}

		public static ExtensionRuleParameter To(ActivityRecorderServiceReference.ExtensionRuleParameter ruleParameter)
		{
			if (ruleParameter == null) return null;
			return new ExtensionRuleParameter { ExtensionData = ruleParameter.ExtensionData, Name = ruleParameter.Name, Value = ruleParameter.Value, };
		}

		public static WindowRule To(ActivityRecorderServiceReference.WindowRule rule)
		{
			if (rule == null) return null;
			return new WindowRule
			{
				ExtensionData = rule.ExtensionData,
				Name = rule.Name,
				IsEnabled = rule.IsEnabled,
				IsRegex = rule.IsRegex,
				IgnoreCase = rule.IgnoreCase,
				TitleRule = rule.TitleRule,
				ProcessRule = rule.ProcessRule,
				UrlRule = rule.UrlRule,
				ExtensionRulesByIdByKey = rule.ExtensionRulesByIdByKey,
				WindowScope = To(rule.WindowScope),
			};
		}

		public static CensorRule To(ActivityRecorderServiceReference.CensorRule rule)
		{
			if (rule == null) return null;
			return new CensorRule
			{
				ExtensionData = rule.ExtensionData,
				Name = rule.Name,
				IsEnabled = rule.IsEnabled,
				IsRegex = rule.IsRegex,
				IgnoreCase = rule.IgnoreCase,
				TitleRule = rule.TitleRule,
				ProcessRule = rule.ProcessRule,
				UrlRule = rule.UrlRule,
				RuleType = To(rule.RuleType),
			};
		}

		public static CensorRuleType To(ActivityRecorderServiceReference.CensorRuleType ruleType)
		{
			var result = CensorRuleType.None;
			if (ruleType.HasFlag(ActivityRecorderServiceReference.CensorRuleType.HideTitle))
				result |= CensorRuleType.HideTitle;
			if (ruleType.HasFlag(ActivityRecorderServiceReference.CensorRuleType.HideScreenShot))
				result |= CensorRuleType.HideScreenShot;
			if (ruleType.HasFlag(ActivityRecorderServiceReference.CensorRuleType.HideUrl))
				result |= CensorRuleType.HideUrl;
			if (ruleType.HasFlag(ActivityRecorderServiceReference.CensorRuleType.HideWindow))
				result |= CensorRuleType.HideWindow;

			return result;
		}

		public static ClientWorkTimeStats To(ActivityRecorderServiceReference.ClientWorkTimeStats stats)
		{
			if (stats == null) return null;
			return new ClientWorkTimeStats
			{
				ExtensionData = stats.ExtensionData,
				ThisMonthsWorkTime = To(stats.ThisMonthsWorkTime),
				ThisWeeksWorkTime = To(stats.ThisWeeksWorkTime),
				TodaysWorkTime = To(stats.TodaysWorkTime),
				ThisMonthsTargetNetWorkTime = stats.ThisMonthsTargetNetWorkTime,
				ThisMonthsTargetUntilTodayNetWorkTime = stats.ThisMonthsTargetUntilTodayNetWorkTime,
				ThisWeeksTargetNetWorkTime = stats.ThisWeeksTargetNetWorkTime,
				ThisWeeksTargetUntilTodayNetWorkTime = stats.ThisWeeksTargetUntilTodayNetWorkTime,
				TodaysTargetNetWorkTime = stats.TodaysTargetNetWorkTime,
			};
		}

		public static BriefNetWorkTimeStats To(ActivityRecorderServiceReference.BriefNetWorkTimeStats timeStats)
		{
			if (timeStats == null) return null;
			return new BriefNetWorkTimeStats
			{
				ExtensionData = timeStats.ExtensionData,
				ComputerWorkTime = timeStats.ComputerWorkTime,
				HolidayTime = timeStats.HolidayTime,
				IvrWorkTime = timeStats.IvrWorkTime,
				ManuallyAddedWorkTime = timeStats.ManuallyAddedWorkTime,
				MobileWorkTime = timeStats.MobileWorkTime,
				SickLeaveTime = timeStats.SickLeaveTime,
				NetWorkTime = timeStats.NetWorkTime,
			};
		}

		public static ClientComputerKick To(ActivityRecorderServiceReference.ClientComputerKick kick)
		{
			if (kick == null) return null;
			return new ClientComputerKick
			{
				ExtensionData = kick.ExtensionData,
				Id = kick.Id,
				UserId = kick.UserId,
				ComputerId = kick.ComputerId,
				Reason = kick.Reason,
				CreatedBy = kick.CreatedBy,
				CreateDate = kick.CreateDate,
				CreatedByName = kick.CreatedByName,
			};
		}

		public static RuleGeneratorData To(ActivityRecorderServiceReference.RuleGeneratorData data)
		{
			return data == null ? null : new RuleGeneratorData { ExtensionData = data.ExtensionData, Name = data.Name, Parameters = data.Parameters };
		}

		public static CannedCloseReasons To(ActivityRecorderServiceReference.CannedCloseReasons reasons)
		{
			return reasons == null ? null : new CannedCloseReasons { ExtensionData = reasons.ExtensionData, DefaultReasons = reasons.DefaultReasons, IsReadonly = reasons.IsReadonly, TreeRoot = reasons.TreeRoot?.Select(To).ToArray() };
		}

		public static CloseReasonNode To(ActivityRecorderServiceReference.CloseReasonNode reasonNode)
		{
			return new CloseReasonNode { ExtensionData = reasonNode.ExtensionData, Children = reasonNode.Children?.Select(To).ToArray(), NodeId = reasonNode.NodeId, ReasonPart = reasonNode.ReasonPart };
		}

		public static CloseWorkResult To(ActivityRecorderServiceReference.CloseWorkResult result)
		{
			switch (result)
			{
				case ActivityRecorderServiceReference.CloseWorkResult.Ok:
					return CloseWorkResult.Ok;
				case ActivityRecorderServiceReference.CloseWorkResult.ReasonRequired:
					return CloseWorkResult.ReasonRequired;
				case ActivityRecorderServiceReference.CloseWorkResult.AlreadyClosed:
					return CloseWorkResult.AlreadyClosed;
				case ActivityRecorderServiceReference.CloseWorkResult.UnknownError:
					return CloseWorkResult.UnknownError;
				default:
					throw new ArgumentOutOfRangeException(nameof(result), result, null);
			}
		}

		public static ReasonStats To(ActivityRecorderServiceReference.ReasonStats stats)
		{
			return stats == null ? null : new ReasonStats { ExtensionData = stats.ExtensionData, ReasonCountByWorkId = stats.ReasonCountByWorkId };
		}

		public static SimpleWorkTimeStats To(ActivityRecorderServiceReference.SimpleWorkTimeStats stats)
		{
			if (stats == null) return null;
			return new SimpleWorkTimeStats
			{
				ExtensionData = stats.ExtensionData,
				FromDate = stats.FromDate,
				Stats = stats.Stats?.ToDictionary(k => k.Key, v => To(v.Value)),
				ToDate = stats.ToDate,
				UserId = stats.UserId,
			};
		}

		public static SimpleWorkTimeStat To(ActivityRecorderServiceReference.SimpleWorkTimeStat stat)
		{
			return stat == null ? null : new SimpleWorkTimeStat { ExtensionData = stat.ExtensionData, TotalWorkTime = stat.TotalWorkTime, WorkId = stat.WorkId };
		}

		public static MeetingData To(ActivityRecorderServiceReference.MeetingData data)
		{
			return data == null ? null : new  MeetingData { ExtensionData = data.ExtensionData, PendingMeetings = data.PendingMeetings?.Select(To).ToArray(), CalendarEmailAccounts = data.CalendarEmailAccounts, LastSuccessfulSyncDate = data.LastSuccessfulSyncDate };
		}

		public static MeetingEntry To(ActivityRecorderServiceReference.MeetingEntry entry)
		{
			return entry == null ? null : new MeetingEntry
			{
				ExtensionData = entry.ExtensionData,
				EndDate = entry.EndDate,
				Id = entry.Id,
				OrganizerId = entry.OrganizerId,
				StartDate = entry.StartDate,
				Title = entry.Title,
				OrganizerEmail = entry.OrganizerEmail,
				OrganizerFirstName = entry.OrganizerFirstName,
				OrganizerLastName = entry.OrganizerLastName,
			};
		}

		public static AllWorkItem To(ActivityRecorderServiceReference.AllWorkItem item)
		{
			return item == null ? null : new AllWorkItem
			{
				ExtensionData = item.ExtensionData,
				ClosedAt = item.ClosedAt,
				Name = item.Name,
				OwnTask = item.OwnTask,
				ParentId = item.ParentId,
				TaskId = item.TaskId,
				Type = item.Type,
			};
		}

		public static AssignTaskResult To(ActivityRecorderServiceReference.AssignTaskResult result)
		{
			switch (result)
			{
				case ActivityRecorderServiceReference.AssignTaskResult.Ok:
					return AssignTaskResult.Ok;
				case ActivityRecorderServiceReference.AssignTaskResult.AccessDenied:
					return AssignTaskResult.AccessDenied;
				case ActivityRecorderServiceReference.AssignTaskResult.UnknownError:
					return AssignTaskResult.UnknownError;
				default:
					throw new ArgumentOutOfRangeException(nameof(result), result, null);
			}
		}

		public static TaskReasons To(ActivityRecorderServiceReference.TaskReasons reasons)
		{
			return reasons == null ? null : new TaskReasons { ExtensionData = reasons.ExtensionData, ReasonsByWorkId = reasons.ReasonsByWorkId?.ToDictionary(k => k.Key, v => v.Value.Select(To).ToArray()) };
		}

		public static Reason To(ActivityRecorderServiceReference.Reason reason)
		{
			return reason == null ? null : new Reason { ExtensionData = reason.ExtensionData, ReasonItemId = reason.ReasonItemId, ReasonText = reason.ReasonText, createdAt = reason.createdAt };
		}

		public static ApplicationUpdateInfo To(ActivityRecorderServiceReference.ApplicationUpdateInfo info)
		{
			return info == null ? null : new ApplicationUpdateInfo { ExtensionData = info.ExtensionData, ChunkCount = info.ChunkCount, FileId = info.FileId, Version = info.Version };
		}

		public static ProjectManagementConstraints To(ActivityRecorderServiceReference.ProjectManagementConstraints constraints)
		{
			return constraints == null ? null : new ProjectManagementConstraints
			{
				ExtensionData = constraints.ExtensionData,
				ProjectId = constraints.ProjectId,
				ProjectManagementPermissions = constraints.ProjectManagementPermissions,
				WorkMandatoryFields = constraints.WorkMandatoryFields,
				WorkMaxEndDate = constraints.WorkMaxEndDate,
				WorkMaxTargetCost = constraints.WorkMaxTargetCost,
				WorkMaxTargetWorkTime = constraints.WorkMaxTargetWorkTime,
				WorkMinStartDate = constraints.WorkMinStartDate,
			};
		}

		public static DailyWorkTimeStats To(ActivityRecorderServiceReference.DailyWorkTimeStats stats)
		{
			return stats == null ? null : new DailyWorkTimeStats
			{
				ExtensionData = stats.ExtensionData,
				ComputerWorkTime = stats.ComputerWorkTime,
				Day = stats.Day,
				HolidayTime = stats.HolidayTime,
				IvrWorkTime = stats.IvrWorkTime,
				ManuallyAddedWorkTime = stats.ManuallyAddedWorkTime,
				MobileWorkTime = stats.MobileWorkTime,
				NetWorkTime = stats.NetWorkTime,
				SickLeaveTime = stats.SickLeaveTime,
				TotalWorkTimeByWorkId = stats.TotalWorkTimeByWorkId,
				Version = stats.Version,
				PartialInterval = stats.PartialInterval,
			};
		}

		public static ClientWorkTimeHistory To(ActivityRecorderServiceReference.ClientWorkTimeHistory history)
		{
			return history == null ? null : new ClientWorkTimeHistory
			{
				ExtensionData = history.ExtensionData,
				ComputerIntervals = history.ComputerIntervals?.Select(To).ToArray(),
				IsModificationApprovalNeeded = history.IsModificationApprovalNeeded,
				IvrIntervals = history.IvrIntervals?.Select(To).ToArray(),
				ManualIntervals = history.ManualIntervals?.Select(To).ToArray(),
				MobileIntervals = history.MobileIntervals?.Select(To).ToArray(),
				ModificationAgeLimit = history.ModificationAgeLimit,
				TotalTimeInMs = history.TotalTimeInMs,
				StartTimeInMs = history.StartTimeInMs,
				EndTimeInMs = history.EndTimeInMs,
				StartEndDiffInMs = history.StartEndDiffInMs,
				LastComputerWorkitemEndTime = history.LastComputerWorkitemEndTime,
			};
		}

		public static ComputerInterval To(ActivityRecorderServiceReference.ComputerInterval interval)
		{
			return interval == null ? null : new ComputerInterval
			{
				ExtensionData = interval.ExtensionData,
				ComputerId = interval.ComputerId,
				EndDate = interval.EndDate,
				StartDate = interval.StartDate,
				WorkId = interval.WorkId,
			};
		}

		public static IvrInterval To(ActivityRecorderServiceReference.IvrInterval interval)
		{
			return interval == null ? null : new IvrInterval
			{
				ExtensionData = interval.ExtensionData,
				EndDate = interval.EndDate,
				IsOngoing = interval.IsOngoing,
				StartDate = interval.StartDate,
				WorkId = interval.WorkId,
			};
		}

		public static ManualInterval To(ActivityRecorderServiceReference.ManualInterval interval)
		{
			return interval == null ? null : new ManualInterval
			{
				ExtensionData = interval.ExtensionData,
				Comment = interval.Comment,
				Description = interval.Description,
				EndDate = interval.EndDate,
				Id = interval.Id,
				IsEditable = interval.IsEditable,
				IsMeeting = interval.IsMeeting,
				IsPending = interval.IsPending,
				IsPendingDeleteAlso = interval.IsPendingDeleteAlso,
				ManualWorkItemType = To(interval.ManualWorkItemType),
				MeetingId = interval.MeetingId,
				PendingId = interval.PendingId,
				SourceId = interval.SourceId,
				StartDate = interval.StartDate,
				Subject = interval.Subject,
				WorkId = interval.WorkId,
			};
		}

		public static ManualWorkItemTypeEnum To(ActivityRecorderServiceReference.ManualWorkItemTypeEnum type)
		{
			switch (type)
			{
				case ActivityRecorderServiceReference.ManualWorkItemTypeEnum.AddWork:
					return ManualWorkItemTypeEnum.AddWork;
				case ActivityRecorderServiceReference.ManualWorkItemTypeEnum.DeleteInterval:
					return ManualWorkItemTypeEnum.DeleteInterval;
				case ActivityRecorderServiceReference.ManualWorkItemTypeEnum.DeleteIvrInterval:
					return ManualWorkItemTypeEnum.DeleteIvrInterval;
				case ActivityRecorderServiceReference.ManualWorkItemTypeEnum.DeleteComputerInterval:
					return ManualWorkItemTypeEnum.DeleteComputerInterval;
				case ActivityRecorderServiceReference.ManualWorkItemTypeEnum.AddHoliday:
					return ManualWorkItemTypeEnum.AddHoliday;
				case ActivityRecorderServiceReference.ManualWorkItemTypeEnum.AddSickLeave:
					return ManualWorkItemTypeEnum.AddSickLeave;
				case ActivityRecorderServiceReference.ManualWorkItemTypeEnum.DeleteMobileInterval:
					return ManualWorkItemTypeEnum.DeleteMobileInterval;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
				
		}

		public static MobileInterval To(ActivityRecorderServiceReference.MobileInterval interval)
		{
			return interval == null ? null : new MobileInterval
			{
				ExtensionData = interval.ExtensionData,
				EndDate = interval.EndDate,
				Imei = interval.Imei,
				StartDate = interval.StartDate,
				WorkId = interval.WorkId,
			};
		}

		public static WorkNames To(ActivityRecorderServiceReference.WorkNames names)
		{
			return names == null ? null : new WorkNames { ExtensionData = names.ExtensionData, Names = names.Names?.Select(To).ToArray() };
		}

		public static WorkOrProjectName To(ActivityRecorderServiceReference.WorkOrProjectName name)
		{
			return name == null ? null : new WorkOrProjectName
			{
				ExtensionData = name.ExtensionData,
				Name = name.Name,
				Id = name.Id,
				ProjectId = name.ProjectId,
				ParentId = name.ParentId,
				CategoryId = name.CategoryId,
				ExtId = name.ExtId,
			};
		}

		public static CollectorRules To(ActivityRecorderServiceReference.CollectorRules rules)
		{
			return rules == null ? null : new CollectorRules { ExtensionData = rules.ExtensionData, Rules = rules.Rules?.Select(To).ToArray() };
		}

		public static CollectorRule To(ActivityRecorderServiceReference.CollectorRule rule)
		{
			return rule == null ? null : new CollectorRule
			{
				ExtensionData = rule.ExtensionData,
				Name = rule.Name,
				IsEnabled = rule.IsEnabled,
				IsRegex = rule.IsRegex,
				IgnoreCase = rule.IgnoreCase,
				TitleRule = rule.TitleRule,
				ProcessRule = rule.ProcessRule,
				UrlRule = rule.UrlRule,
				ServerId = rule.ServerId,
				ExtensionRulesByIdByKey = rule.ExtensionRulesByIdByKey,
				WindowScope = To(rule.WindowScope),
				FormattedNamedGroups = rule.FormattedNamedGroups,
				Children = rule.Children?.Select(To).ToArray(),
				CapturedKeys = rule.CapturedKeys,
				ExtensionRuleParametersById = rule.ExtensionRuleParametersById?.ToDictionary(k => k.Key, v => v.Value?.Select(To).ToArray()),
			};
		}

		public static NotificationData To(ActivityRecorderServiceReference.NotificationData data)
		{
			return data == null ? null : new NotificationData
			{
				ExtensionData = data.ExtensionData,
				Form = To(data.Form),
				FormId = data.FormId,
				Id = data.Id,
				Name = data.Name,
				WorkId = data.WorkId,
			};
		}

		public static JcForm To(ActivityRecorderServiceReference.JcForm form)
		{
			return form == null ? null : new JcForm { ExtensionData = form.ExtensionData, BeforeShowActions = form.BeforeShowActions, CloseButtonId = form.CloseButtonId, MessageBox = To(form.MessageBox) };
		}

		public static JcMessageBox To(ActivityRecorderServiceReference.JcMessageBox box)
		{
			return box == null ? null : new JcMessageBox { ExtensionData = box.ExtensionData, Buttons = box.Buttons?.Select(To).ToArray(), Text = box.Text, Title = box.Title };
		}

		public static JcButton To(ActivityRecorderServiceReference.JcButton button)
		{
			return button == null ? null : new JcButton { ExtensionData = button.ExtensionData, Id = button.Id, Text = button.Text };
		}

		public static ArrayOfMessage To(ActivityRecorderServiceReference.ArrayOfMessage messages)
		{
			if (messages == null) return null;
			var list = new ArrayOfMessage();
			list.AddRange(messages.Select(To));
			return list;
		}

		public static Message To(ActivityRecorderServiceReference.Message message)
		{
			return message == null ? null : new Message
			{
				ExtensionData = message.ExtensionData,
				Id = message.Id,
				TargetUserId = message.TargetUserId,
				Type = message.Type,
				Content = message.Content,
				Target = To(message.Target),
				CreatedAt = message.CreatedAt,
				LastUpdatedAt = message.LastUpdatedAt,
				PCLastSentAt = message.PCLastSentAt,
				MobileLastSentAt = message.MobileLastSentAt,
				PCLastReadAt = message.PCLastReadAt,
				MobileLastReadAt = message.MobileLastReadAt,
				DeletedAt = message.DeletedAt,
				ExpiryInHours = message.ExpiryInHours,
			};
		}

		public static MessageTargets To(ActivityRecorderServiceReference.MessageTargets targets)
		{
			var result = new MessageTargets();
			if (targets.HasFlag(ActivityRecorderServiceReference.MessageTargets.PC))
				result |= MessageTargets.PC;
			if (targets.HasFlag(ActivityRecorderServiceReference.MessageTargets.Mobile))
				result |= MessageTargets.Mobile;
			return result;
		}

		public static WorkTimeStats To(ActivityRecorderServiceReference.WorkTimeStats stats)
		{
			if (stats == null) return null;
			return new WorkTimeStats
			{
				ExtensionData = stats.ExtensionData,
				TodaysWorkTimeInMs = stats.TodaysWorkTimeInMs,
				ThisWeeksWorkTimeInMs = stats.ThisWeeksWorkTimeInMs,
				ThisMonthsWorkTimeInMs = stats.ThisMonthsWorkTimeInMs,
				TodaysTargetNetWorkTimeInMs = stats.TodaysTargetNetWorkTimeInMs,
				ThisWeeksTargetNetWorkTimeInMs = stats.ThisWeeksTargetNetWorkTimeInMs,
				ThisMonthsTargetNetWorkTimeInMs = stats.ThisMonthsTargetNetWorkTimeInMs,
				ThisWeeksTargetUntilTodayNetWorkTimeInMs = stats.ThisWeeksTargetUntilTodayNetWorkTimeInMs,
				ThisMonthsTargetUntilTodayNetWorkTimeInMs = stats.ThisMonthsTargetUntilTodayNetWorkTimeInMs,
				LastComputerWorkitemEndTime = stats.LastComputerWorkitemEndTime,
				EndOfTodayInUtc = stats.EndOfTodayInUtc,
			};
		}

		public static IssueData To(ActivityRecorderServiceReference.IssueData data)
		{
			return data == null ? null : new IssueData
			{
				ExtensionData = data.ExtensionData,
				IssueCode = data.IssueCode,
				Name = data.Name,
				Company = data.Company,
				State = data.State,
				UserId = data.UserId,
				Modified = data.Modified,
				ModifiedByName = data.ModifiedByName,
				CreatedByName = data.CreatedByName,
				CreatedByUserId = data.CreatedByUserId,
			};
		}

		public static TodoListDTO To(ActivityRecorderServiceReference.TodoListDTO dto)
		{
			if (dto == null) return null;
			return new TodoListDTO
			{
				ExtensionData = dto.ExtensionData,
				Id = dto.Id,
				Date = dto.Date,
				TodoListItems = dto.TodoListItems?.Select(To).ToArray(),
				UserId = dto.UserId,
				LockLastTakenAt = dto.LockLastTakenAt,
				CreatedAt = dto.CreatedAt,
			};
		}

		public static TodoListItemDTO To(ActivityRecorderServiceReference.TodoListItemDTO item)
		{
			return item == null ? null : new TodoListItemDTO
			{
				ExtensionData = item.ExtensionData,
				Id = item.Id,
				ListId = item.ListId,
				Name = item.Name,
				Priority = item.Priority,
				Status = To(item.Status),
				CreatedAt = item.CreatedAt,
			};
		}

		public static TodoListItemStatusDTO To(ActivityRecorderServiceReference.TodoListItemStatusDTO status)
		{
			return status == null ? null : new TodoListItemStatusDTO { ExtensionData = status.ExtensionData, Id = status.Id, Name = status.Name };
		}

		public static TodoListToken To(ActivityRecorderServiceReference.TodoListToken token)
		{
			return token == null ? null : new TodoListToken { ExtensionData = token.ExtensionData, IsAcquired = token.IsAcquired, EditedByLastName = token.EditedByLastName, EditedByFirstName = token.EditedByFirstName };
		}

		public static AcceptanceData To(ActivityRecorderServiceReference.AcceptanceData data)
		{
			return data == null ? null : new AcceptanceData { ExtensionData = data.ExtensionData, Message = data.Message, AcceptedAt = data.AcceptedAt };
		}

		public static CloudTokenData To(ActivityRecorderServiceReference.CloudTokenData data)
		{
			return data == null ? null : new CloudTokenData { ExtensionData = data.ExtensionData, GoogleCalendarToken = data.GoogleCalendarToken };
		}

		#endregion

		#region ProxyServiceReference to ActivityRecorderServiceReference 

		public static ActivityRecorderServiceReference.ClientMenu To(ClientMenu menu)
		{
			return new ActivityRecorderServiceReference.ClientMenu
			{
				ExtensionData = menu.ExtensionData,
				CategoriesById = menu.CategoriesById?.ToDictionary(k => k.Key, v => To(v.Value)),
				ExternalCompositeMapping = To(menu.ExternalCompositeMapping),
				ExternalProjectIdMapping = menu.ExternalProjectIdMapping,
				ExternalWorkIdMapping = menu.ExternalWorkIdMapping,
				Works = menu.Works?.Select(w => To(w)).ToArray(),
			};
		}

		public static ActivityRecorderServiceReference.WorkData To(WorkData workData)
		{
			return new ActivityRecorderServiceReference.WorkData
			{
				ExtensionData = workData.ExtensionData,
				Id = workData.Id,
				Name = workData.Name,
				ExternalWorkIdMapping = workData.ExternalWorkIdMapping,
				CategoryId = workData.CategoryId,
				Children = workData.Children?.Select(c => To(c)).ToArray(),
				CloseReasonRequiredDate = workData.CloseReasonRequiredDate,
				CloseReasonRequiredTime = workData.CloseReasonRequiredTime,
				CloseReasonRequiredTimeRepeatCount = workData.CloseReasonRequiredTimeRepeatCount,
				CloseReasonRequiredTimeRepeatInterval = workData.CloseReasonRequiredTimeRepeatInterval,
				Description = workData.Description,
				EndDate = workData.EndDate,
				ExtId = workData.ExtId,
				IsDefault = workData.IsDefault,
				IsForMobile = workData.IsForMobile,
				IsReadOnly = workData.IsReadOnly,
				ManualAddWorkDuration = workData.ManualAddWorkDuration,
				Priority = workData.Priority,
				ProjectId = workData.ProjectId,
				StartDate = workData.StartDate,
				TargetTotalWorkTime = workData.TargetTotalWorkTime,
				TaxId = workData.TaxId,
				TemplateRegex = workData.TemplateRegex,
				Type = workData.Type,
				VisibilityType = workData.VisibilityType,
			};
		}

		public static ActivityRecorderServiceReference.CompositeMapping To(CompositeMapping mapping)
		{
			return new ActivityRecorderServiceReference.CompositeMapping
			{
				ExtensionData = mapping.ExtensionData,
				WorkIdByKey = mapping.WorkIdByKey,
				ChildrenByKey = mapping.ChildrenByKey?.ToDictionary(k => k.Key, v => To(v.Value)),
			};
		}

		public static ActivityRecorderServiceReference.CategoryData To(CategoryData categoryData)
		{
			return new ActivityRecorderServiceReference.CategoryData
			{
				Id = categoryData.Id,
				ExtensionData = categoryData.ExtensionData,
				Name = categoryData.Name,
			};
		}

		public static ActivityRecorderServiceReference.WorkItem To(WorkItem workItem)
		{
			return new ActivityRecorderServiceReference.WorkItem
			{
				ExtensionData = workItem.ExtensionData,
				StartDate = workItem.StartDate,
				EndDate = workItem.EndDate,
				UserId = workItem.UserId,
				ComputerId = workItem.ComputerId,
				DesktopCaptures = workItem.DesktopCaptures?.Select(To).ToArray(),
				IsRemoteDesktop = workItem.IsRemoteDesktop,
				WorkId = workItem.WorkId,
				PhaseId = workItem.PhaseId,
				IsVirtualMachine = workItem.IsVirtualMachine,
				KeyboardActivity = workItem.KeyboardActivity,
				LocalIPAddresses = workItem.LocalIPAddresses,
				MouseActivity = workItem.MouseActivity,
			};
		}

		public static ActivityRecorderServiceReference.DesktopCapture To(DesktopCapture desktopCapture)
		{
			return new ActivityRecorderServiceReference.DesktopCapture
			{
				ExtensionData = desktopCapture.ExtensionData,
				DesktopWindows = desktopCapture.DesktopWindows?.Select(To).ToArray(),
				Screens = desktopCapture.Screens?.Select(To).ToArray(),
			};
		}

		public static ActivityRecorderServiceReference.DesktopWindow To(DesktopWindow window)
		{
			return new ActivityRecorderServiceReference.DesktopWindow
			{
				ExtensionData = window.ExtensionData,
				ClientArea = window.ClientArea,
				CreateDate = window.CreateDate,
				Height = window.Height,
				IsActive = window.IsActive,
				ProcessName = window.ProcessName,
				Title = window.Title,
				Url = window.Url,
				VisibleClientArea = window.VisibleClientArea,
				Width = window.Width,
				X = window.X,
				Y = window.Y,
			};
		}

		public static ActivityRecorderServiceReference.Screen To(Screen screen)
		{
			return new ActivityRecorderServiceReference.Screen
			{
				ExtensionData = screen.ExtensionData,
				CreateDate = screen.CreateDate,
				X = screen.X,
				Y = screen.Y,
				Width = screen.Width,
				Height = screen.Height,
				ScreenNumber = screen.ScreenNumber,
				Extension = screen.Extension,
				ScreenShot = screen.ScreenShot,
				EncodeMaster = screen.EncodeMaster,
				EncodeZipped = screen.EncodeZipped,
				EncodeBitmapId = screen.EncodeBitmapId,
				EncodeEncoderBitmapId = screen.EncodeEncoderBitmapId,
				EncodeJpgQuality = screen.EncodeJpgQuality,
				EncodeVersion = screen.EncodeVersion,
			};
		}

		public static ActivityRecorderServiceReference.WorkDetectorRule To(WorkDetectorRule rule)
		{
			return new ActivityRecorderServiceReference.WorkDetectorRule
			{
				ExtensionData = rule.ExtensionData,
				RuleType = To(rule.RuleType),
				RelatedId = rule.RelatedId,
				Name = rule.Name,
				IsEnabled = rule.IsEnabled,
				IsRegex = rule.IsRegex,
				IgnoreCase = rule.IgnoreCase,
				TitleRule = rule.TitleRule,
				ProcessRule = rule.ProcessRule,
				UrlRule = rule.UrlRule,
				IsPermanent = rule.IsPermanent,
				WorkSelector = To(rule.WorkSelector),
				KeySuffix = rule.KeySuffix,
				ServerId = rule.ServerId,
				ExtensionRulesByIdByKey = rule.ExtensionRulesByIdByKey,
				WindowScope = To(rule.WindowScope),
				IsEnabledInNonWorkStatus = rule.IsEnabledInNonWorkStatus,
				IsEnabledInProjectIds = rule.IsEnabledInProjectIds,
				ExtensionRuleParametersById = rule.ExtensionRuleParametersById?.ToDictionary(k => k.Key, v => v.Value?.Select(To).ToArray()),
				AdditionalActions = rule.AdditionalActions,
				FormattedNamedGroups = rule.FormattedNamedGroups,
				Children = rule.Children.Select(To).ToArray(),
				IsDefault = rule.IsDefault,
			};
		}

		public static ActivityRecorderServiceReference.WorkDetectorRuleType To(WorkDetectorRuleType ruleType)
		{
			switch (ruleType)
			{
				case WorkDetectorRuleType.TempStartWork:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartWork;
				case WorkDetectorRuleType.TempStopWork:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.TempStopWork;
				case WorkDetectorRuleType.TempStartCategory:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartCategory;
				case WorkDetectorRuleType.DoNothing:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.DoNothing;
				case WorkDetectorRuleType.TempStartProjectTemplate:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartProjectTemplate;
				case WorkDetectorRuleType.TempStartWorkTemplate:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartWorkTemplate;
				case WorkDetectorRuleType.EndTempEffect:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.EndTempEffect;
				case WorkDetectorRuleType.CreateNewRuleAndEndTempEffect:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.CreateNewRuleAndEndTempEffect;
				case WorkDetectorRuleType.CreateNewRuleAndTempStartWork:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.CreateNewRuleAndTempStartWork;
				case WorkDetectorRuleType.TempStartOrAssignWork:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartOrAssignWork;
				case WorkDetectorRuleType.TempStartOrAssignProject:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartOrAssignProject;
				case WorkDetectorRuleType.TempStartOrAssignProjectAndWork:
					return ActivityRecorderServiceReference.WorkDetectorRuleType.TempStartOrAssignProjectAndWork;
				default:
					throw new ArgumentOutOfRangeException(nameof(ruleType), ruleType, null);
			}
		}

		public static ActivityRecorderServiceReference.WorkSelector To(WorkSelector selector)
		{
			return new ActivityRecorderServiceReference.WorkSelector
			{
				ExtensionData = selector.ExtensionData,
				Name = selector.Name,
				IsRegex = selector.IsRegex,
				IgnoreCase = selector.IgnoreCase,
				Rule = selector.Rule,
				TemplateText = selector.TemplateText,
			};
		}

		public static ActivityRecorderServiceReference.WindowScopeType To(WindowScopeType scopeType)
		{
			switch (scopeType)
			{
				case WindowScopeType.Active:
					return ActivityRecorderServiceReference.WindowScopeType.Active;
				case WindowScopeType.VisibleOrActive:
					return ActivityRecorderServiceReference.WindowScopeType.VisibleOrActive;
				case WindowScopeType.Any:
					return ActivityRecorderServiceReference.WindowScopeType.Any;
				default:
					throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null);
			}
		}

		public static ActivityRecorderServiceReference.ExtensionRuleParameter To(ExtensionRuleParameter ruleParameter)
		{
			return new ActivityRecorderServiceReference.ExtensionRuleParameter { ExtensionData = ruleParameter.ExtensionData, Name = ruleParameter.Name, Value = ruleParameter.Value, };
		}

		public static ActivityRecorderServiceReference.WindowRule To(WindowRule rule)
		{
			return new ActivityRecorderServiceReference.WindowRule
			{
				ExtensionData = rule.ExtensionData,
				Name = rule.Name,
				IsEnabled = rule.IsEnabled,
				IsRegex = rule.IsRegex,
				IgnoreCase = rule.IgnoreCase,
				TitleRule = rule.TitleRule,
				ProcessRule = rule.ProcessRule,
				UrlRule = rule.UrlRule,
				ExtensionRulesByIdByKey = rule.ExtensionRulesByIdByKey,
				WindowScope = To(rule.WindowScope),
			};
		}

		public static ActivityRecorderServiceReference.CensorRule To(CensorRule rule)
		{
			return new ActivityRecorderServiceReference.CensorRule
			{
				ExtensionData = rule.ExtensionData,
				Name = rule.Name,
				IsEnabled = rule.IsEnabled,
				IsRegex = rule.IsRegex,
				IgnoreCase = rule.IgnoreCase,
				TitleRule = rule.TitleRule,
				ProcessRule = rule.ProcessRule,
				UrlRule = rule.UrlRule,
				RuleType = To(rule.RuleType),
			};
		}

		public static ActivityRecorderServiceReference.CensorRuleType To(CensorRuleType ruleType)
		{
			var result = ActivityRecorderServiceReference.CensorRuleType.None;
			if (ruleType.HasFlag(CensorRuleType.HideTitle))
				result |= ActivityRecorderServiceReference.CensorRuleType.HideTitle;
			if (ruleType.HasFlag(CensorRuleType.HideScreenShot))
				result |= ActivityRecorderServiceReference.CensorRuleType.HideScreenShot;
			if (ruleType.HasFlag(CensorRuleType.HideUrl))
				result |= ActivityRecorderServiceReference.CensorRuleType.HideUrl;
			if (ruleType.HasFlag(CensorRuleType.HideWindow))
				result |= ActivityRecorderServiceReference.CensorRuleType.HideWindow;
			return result;
		}

		public static ActivityRecorderServiceReference.ManualWorkItem To(ManualWorkItem workItem)
		{
			return new ActivityRecorderServiceReference.ManualWorkItem
			{
				ExtensionData = workItem.ExtensionData,
				ManualWorkItemTypeId = To(workItem.ManualWorkItemTypeId),
				WorkId = workItem.WorkId,
				StartDate = workItem.StartDate,
				EndDate = workItem.EndDate,
				UserId = workItem.UserId,
				Comment = workItem.Comment,
				OriginalEndDate = workItem.OriginalEndDate,
			};
		}

		public static ActivityRecorderServiceReference.ManualWorkItemTypeEnum To(ManualWorkItemTypeEnum type)
		{
			switch (type)
			{
				case ManualWorkItemTypeEnum.AddWork:
					return ActivityRecorderServiceReference.ManualWorkItemTypeEnum.AddWork;
				case ManualWorkItemTypeEnum.DeleteInterval:
					return ActivityRecorderServiceReference.ManualWorkItemTypeEnum.DeleteInterval;
				case ManualWorkItemTypeEnum.DeleteIvrInterval:
					return ActivityRecorderServiceReference.ManualWorkItemTypeEnum.DeleteIvrInterval;
				case ManualWorkItemTypeEnum.DeleteComputerInterval:
					return ActivityRecorderServiceReference.ManualWorkItemTypeEnum.DeleteComputerInterval;
				case ManualWorkItemTypeEnum.AddHoliday:
					return ActivityRecorderServiceReference.ManualWorkItemTypeEnum.AddHoliday;
				case ManualWorkItemTypeEnum.AddSickLeave:
					return ActivityRecorderServiceReference.ManualWorkItemTypeEnum.AddSickLeave;
				case ManualWorkItemTypeEnum.DeleteMobileInterval:
					return ActivityRecorderServiceReference.ManualWorkItemTypeEnum.DeleteMobileInterval;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		public static ActivityRecorderServiceReference.ClientComputerInfo To(ClientComputerInfo info)
		{
			if (info == null) return null;
			return new ActivityRecorderServiceReference.ClientComputerInfo
			{
				ExtensionData = info.ExtensionData,
				UserId = info.UserId,
				ComputerId = info.ComputerId,
				OSMajor = info.OSMajor,
				OSMinor = info.OSMinor,
				OSBuild = info.OSBuild,
				OSRevision = info.OSRevision,
				IsNet4Available = info.IsNet4Available,
				IsNet45Available = info.IsNet45Available,
				HighestNetVersionAvailable = info.HighestNetVersionAvailable,
			};
		}

		public static ActivityRecorderServiceReference.ClientComputerError To(ClientComputerError error)
		{
			if (error == null) return null;
			return new ActivityRecorderServiceReference.ClientComputerError
			{
				ExtensionData = error.ExtensionData,
				ClientId = error.ClientId,
				UserId = error.UserId,
				ComputerId = error.ComputerId,
				Major = error.Major,
				Minor = error.Minor,
				Build = error.Build,
				Revision = error.Revision,
				Description = error.Description,
				HasAttachment = error.HasAttachment,
				Offset = error.Offset,
				IsCompleted = error.IsCompleted,
				IsCancelled = error.IsCancelled,
				Features = error.Features,
				Data = error.Data,
			};
		}

		public static ActivityRecorderServiceReference.KickResult To(KickResult result)
		{
			switch (result)
			{
				case KickResult.Ok:
					return ActivityRecorderServiceReference.KickResult.Ok;
				case KickResult.AlreadyOffline:
					return ActivityRecorderServiceReference.KickResult.AlreadyOffline;
				case KickResult.UnknownError:
					return ActivityRecorderServiceReference.KickResult.UnknownError;
				default:
					throw new ArgumentOutOfRangeException(nameof(result), result, null);
			}
		}

		public static ActivityRecorderServiceReference.AssignWorkData To(AssignWorkData data)
		{
			if (data == null) return null;
			return new ActivityRecorderServiceReference.AssignWorkData
			{
				ExtensionData = data.ExtensionData,
				WorkKey = data.WorkKey,
				ServerRuleId = data.ServerRuleId,
				WorkName = data.WorkName,
				ProjectId = data.ProjectId,
				Description = data.Description,
			};
		}

		public static ActivityRecorderServiceReference.ReasonItem To(ReasonItem item)
		{
			if (item == null) return null;
			return new ActivityRecorderServiceReference.ReasonItem
			{
				ExtensionData = item.ExtensionData,
				Reason = item.Reason,
				ReasonItemId = item.ReasonItemId,
				StartDate = item.StartDate,
				UserId = item.UserId,
				WorkId = item.WorkId,
			};
		}

		public static ActivityRecorderServiceReference.FinishedMeetingData To(FinishedMeetingData data)
		{
			return data == null ? null : new ActivityRecorderServiceReference.FinishedMeetingData { ExtensionData = data.ExtensionData, FinishedMeetings = data.FinishedMeetings?.Select(To).ToArray(), LastQueryIntervalEndDate = data.LastQueryIntervalEndDate };
		}

		public static ActivityRecorderServiceReference.FinishedMeetingEntry To(FinishedMeetingEntry entry)
		{
			return entry == null ? null : new ActivityRecorderServiceReference.FinishedMeetingEntry
			{
				ExtensionData = entry.ExtensionData,
				Attendees = entry.Attendees?.Select(To).ToArray(),
				CreationTime = entry.CreationTime,
				Description = entry.Description,
				EndTime = entry.EndTime,
				Id = entry.Id,
				IsInFuture = entry.IsInFuture,
				LastmodificationTime = entry.LastmodificationTime,
				Location = entry.Location,
				OldStartTime = entry.OldStartTime,
				StartTime = entry.StartTime,
				Status = To(entry.Status),
				Title = entry.Title,
			};
		}

		public static ActivityRecorderServiceReference.MeetingAttendee To(MeetingAttendee attendee)
		{
			return attendee == null ? null : new ActivityRecorderServiceReference.MeetingAttendee { ExtensionData = attendee.ExtensionData, Email = attendee.Email, ResponseStatus = To(attendee.ResponseStatus), Type = To(attendee.Type) };
		}

		public static ActivityRecorderServiceReference.MeetingCrudStatus? To(MeetingCrudStatus? status)
		{
			if (!status.HasValue) return null;
			switch (status)
			{
				case MeetingCrudStatus.Created:
					return ActivityRecorderServiceReference.MeetingCrudStatus.Created;
				case MeetingCrudStatus.Updated:
					return ActivityRecorderServiceReference.MeetingCrudStatus.Updated;
				case MeetingCrudStatus.Deleted:
					return ActivityRecorderServiceReference.MeetingCrudStatus.Deleted;
				default:
					throw new ArgumentOutOfRangeException(nameof(status), status, null);
			}
		}

		public static ActivityRecorderServiceReference.MeetingAttendeeResponseStatus To(MeetingAttendeeResponseStatus status)
		{
			switch (status)
			{
				case MeetingAttendeeResponseStatus.ResponseNone:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseNone;
				case MeetingAttendeeResponseStatus.ResponseOrganized:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseOrganized;
				case MeetingAttendeeResponseStatus.ResponseTentative:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseTentative;
				case MeetingAttendeeResponseStatus.ResponseAccepted:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseAccepted;
				case MeetingAttendeeResponseStatus.ResponseDeclined:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseDeclined;
				case MeetingAttendeeResponseStatus.ResponseNotResponded:
					return ActivityRecorderServiceReference.MeetingAttendeeResponseStatus.ResponseNotResponded;
				default:
					throw new ArgumentOutOfRangeException(nameof(status), status, null);
			}
		}

		public static ActivityRecorderServiceReference.MeetingAttendeeType To(MeetingAttendeeType type)
		{
			switch (type)
			{
				case MeetingAttendeeType.Organizer:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Organizer;
				case MeetingAttendeeType.Required:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Required;
				case MeetingAttendeeType.Optional:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Optional;
				case MeetingAttendeeType.Resource:
					return ActivityRecorderServiceReference.MeetingAttendeeType.Resource;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		public static ActivityRecorderServiceReference.ManualMeetingData To(ManualMeetingData data)
		{
			return data == null ? null : new ActivityRecorderServiceReference.ManualMeetingData
			{
				ExtensionData = data.ExtensionData,
				AttendeeEmails = data.AttendeeEmails,
				Description = data.Description,
				EndTime = data.EndTime,
				IncludedIdleMinutes = data.IncludedIdleMinutes,
				Location = data.Location,
				OnGoing = data.OnGoing,
				OriginalStartTime = data.OriginalStartTime,
				StartTime = data.StartTime,
				Title = data.Title,
				WorkId = data.WorkId,
			};
		}

		public static ActivityRecorderServiceReference.AssignProjectData To(AssignProjectData data)
		{
			return data == null ? null : new ActivityRecorderServiceReference.AssignProjectData
			{
				ExtensionData = data.ExtensionData,
				ProjectKey = data.ProjectKey,
				ServerRuleId = data.ServerRuleId,
				ProjectName = data.ProjectName,
				Description = data.Description,
			};
		}

		public static ActivityRecorderServiceReference.AssignCompositeData To(AssignCompositeData data)
		{
			return data == null ? null : new ActivityRecorderServiceReference.AssignCompositeData
			{
				ExtensionData = data.ExtensionData,
				WorkKey = data.WorkKey,
				ProjectKeys = data.ProjectKeys,
				ServerRuleId = data.ServerRuleId,
				WorkName = data.WorkName,
				Description = data.Description,
			};
		}

		public static ActivityRecorderServiceReference.ParallelWorkItem To(ParallelWorkItem item)
		{
			return item == null ? null : new ActivityRecorderServiceReference.ParallelWorkItem
			{
				ExtensionData = item.ExtensionData,
				ParallelWorkItemTypeId = To(item.ParallelWorkItemTypeId),
				WorkId = item.WorkId,
				StartDate = item.StartDate,
				EndDate = item.EndDate,
				UserId = item.UserId,
			};
		}

		public static ActivityRecorderServiceReference.ParallelWorkItemTypeEnum To(ParallelWorkItemTypeEnum type)
		{
			switch (type)
			{
				case ParallelWorkItemTypeEnum.IEBusy:
					return ActivityRecorderServiceReference.ParallelWorkItemTypeEnum.IEBusy;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		public static ActivityRecorderServiceReference.WorkTimeModifications To(WorkTimeModifications modifications)
		{
			return modifications == null ? null : new ActivityRecorderServiceReference.WorkTimeModifications { ExtensionData = modifications.ExtensionData, Comment = modifications.Comment, ManualIntervalModifications = modifications.ManualIntervalModifications?.Select(To).ToArray() };
		}

		public static ActivityRecorderServiceReference.ManualIntervalModification To(ManualIntervalModification modification)
		{
			return modification == null ? null : new ActivityRecorderServiceReference.ManualIntervalModification { ExtensionData = modification.ExtensionData, NewItem = To(modification.NewItem), OriginalItem = To(modification.OriginalItem) };
		}

		public static ActivityRecorderServiceReference.ManualInterval To(ManualInterval interval)
		{
			return interval == null ? null : new ActivityRecorderServiceReference.ManualInterval
			{
				ExtensionData = interval.ExtensionData,
				Comment = interval.Comment,
				Description = interval.Description,
				EndDate = interval.EndDate,
				Id = interval.Id,
				IsEditable = interval.IsEditable,
				IsMeeting = interval.IsMeeting,
				IsPending = interval.IsPending,
				IsPendingDeleteAlso = interval.IsPendingDeleteAlso,
				ManualWorkItemType = To(interval.ManualWorkItemType),
				MeetingId = interval.MeetingId,
				PendingId = interval.PendingId,
				SourceId = interval.SourceId,
				StartDate = interval.StartDate,
				Subject = interval.Subject,
				WorkId = interval.WorkId,
			};
		}

		public static ActivityRecorderServiceReference.NotificationResult To(NotificationResult result)
		{
			return result == null ? null : new ActivityRecorderServiceReference.NotificationResult
			{
				ExtensionData = result.ExtensionData,
				ConfirmDate = result.ConfirmDate,
				Id = result.Id,
				Result = result.Result,
				ShowDate = result.ShowDate,
				UserId = result.UserId,
			};
		}

		public static ActivityRecorderServiceReference.CollectorRules To(CollectorRules rules)
		{
			return rules == null ? null : new ActivityRecorderServiceReference.CollectorRules { ExtensionData = rules.ExtensionData, Rules = rules.Rules?.Select(To).ToArray() };
		}

		public static ActivityRecorderServiceReference.CollectorRule To(CollectorRule rule)
		{
			return rule == null ? null : new ActivityRecorderServiceReference.CollectorRule
			{
				ExtensionData = rule.ExtensionData,
				Name = rule.Name,
				IsEnabled = rule.IsEnabled,
				IsRegex = rule.IsRegex,
				IgnoreCase = rule.IgnoreCase,
				TitleRule = rule.TitleRule,
				ProcessRule = rule.ProcessRule,
				UrlRule = rule.UrlRule,
				ServerId = rule.ServerId,
				ExtensionRulesByIdByKey = rule.ExtensionRulesByIdByKey,
				WindowScope = To(rule.WindowScope),
				FormattedNamedGroups = rule.FormattedNamedGroups,
				Children = rule.Children?.Select(To).ToArray(),
				CapturedKeys = rule.CapturedKeys,
				ExtensionRuleParametersById = rule.ExtensionRuleParametersById?.ToDictionary(k => k.Key, v => v.Value?.Select(To).ToArray()),
			};
		}

		public static ActivityRecorderServiceReference.CollectedItem To(CollectedItem item)
		{
			return item == null ? null : new ActivityRecorderServiceReference.CollectedItem
			{
				ExtensionData = item.ExtensionData,
				CapturedValues = item.CapturedValues,
				ComputerId = item.ComputerId,
				CreateDate = item.CreateDate,
				UserId = item.UserId,
			};
		}

		public static ActivityRecorderServiceReference.AggregateCollectedItems To(AggregateCollectedItems items)
		{
			return items == null ? null : new ActivityRecorderServiceReference.AggregateCollectedItems
			{
				ExtensionData = items.ExtensionData,
				ComputerId = items.ComputerId,
				CreateDate = items.CreateDate,
				Items = items.Items?.Select(To).ToArray(),
				KeyLookup = items.KeyLookup,
				UserId = items.UserId,
				ValueLookup = items.ValueLookup,
			};
		}

		public static ActivityRecorderServiceReference.IssueData To(IssueData data)
		{
			return data == null ? null : new ActivityRecorderServiceReference.IssueData
			{
				ExtensionData = data.ExtensionData,
				IssueCode = data.IssueCode,
				Name = data.Name,
				Company = data.Company,
				State = data.State,
				UserId = data.UserId,
				Modified = data.Modified,
				ModifiedByName = data.ModifiedByName,
				CreatedByName = data.CreatedByName,
				CreatedByUserId = data.CreatedByUserId,
			};
		}

		public static ActivityRecorderServiceReference.TelemetryItem To(TelemetryItem item)
		{
			return item == null ? null : new ActivityRecorderServiceReference.TelemetryItem
			{
				ExtensionData = item.ExtensionData,
				ComputerId = item.ComputerId,
				EndDate = item.EndDate,
				EventNameValueOccurences = item.EventNameValueOccurences,
				StartDate = item.StartDate,
				UserId = item.UserId,
			};
		}

		public static ActivityRecorderServiceReference.CollectedItemIdOnly To(CollectedItemIdOnly item)
		{
			return item == null ? null : new ActivityRecorderServiceReference.CollectedItemIdOnly { ExtensionData = item.ExtensionData, CapturedValues = item.CapturedValues, CreateDate = item.CreateDate };
		}

		public static ActivityRecorderServiceReference.Snippet To(Snippet snippet)
		{
			return snippet == null ? null : new ActivityRecorderServiceReference.Snippet
			{
				ExtensionData = snippet.ExtensionData,
				ImageData = snippet.ImageData,
				Content = snippet.Content,
				Guid = snippet.Guid,
				UserId = snippet.UserId,
				CreatedAt = snippet.CreatedAt,
				RuleId = snippet.RuleId,
				IsBadData = snippet.IsBadData,
				ProcessName = snippet.ProcessName,
				ProcessedAt = snippet.ProcessedAt,
				Quality = snippet.Quality,
			};
		}

		public static ActivityRecorderServiceReference.TodoListDTO To(TodoListDTO dto)
		{
			if (dto == null) return null;
			return new ActivityRecorderServiceReference.TodoListDTO
			{
				ExtensionData = dto.ExtensionData,
				Id = dto.Id,
				Date = dto.Date,
				TodoListItems = dto.TodoListItems?.Select(To).ToArray(),
				UserId = dto.UserId,
				LockLastTakenAt = dto.LockLastTakenAt,
				CreatedAt = dto.CreatedAt,
			};
		}

		public static ActivityRecorderServiceReference.TodoListItemDTO To(TodoListItemDTO item)
		{
			return item == null ? null : new ActivityRecorderServiceReference.TodoListItemDTO
			{
				ExtensionData = item.ExtensionData,
				Id = item.Id,
				ListId = item.ListId,
				Name = item.Name,
				Priority = item.Priority,
				Status = To(item.Status),
				CreatedAt = item.CreatedAt,
			};
		}

		public static ActivityRecorderServiceReference.TodoListItemStatusDTO To(TodoListItemStatusDTO status)
		{
			return status == null ? null : new ActivityRecorderServiceReference.TodoListItemStatusDTO { ExtensionData = status.ExtensionData, Id = status.Id, Name = status.Name };
		}

		#endregion
	}
}
