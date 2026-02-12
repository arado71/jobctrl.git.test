using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tct.ActivityRecorderService
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Data.Linq.Mapping;
	using System.Data.Linq;
	using System.Reflection;
	using Tct.ActivityRecorderService.EmailStats;
	using Dapper;
	using log4net;

	partial class JobControlDataClassesDataContext
	{
#if DEBUG
		private const int UserCountDebug = 2000;
#endif
#pragma warning disable 162
		public IEnumerable<UserId> GetActiveUserIds()
		{
#if DEBUG
			return Enumerable.Range(1, UserCountDebug).Select(n => new UserId() { Id = n, CompanyId = -1, GroupId = -1 });
#endif
			return Connection.Query<UserId>("exec [dbo].[Client_GetUserIDs]");
		}

		public IEnumerable<UserStatInfo> GetUserStatsInfo()
		{
#if DEBUG
			return Enumerable.Range(1, UserCountDebug).Select(n => GetUserStatInfoById(n));
#endif
			var query = Connection.QueryMultiple("[dbo].[Client_GetUsersData]", commandType: CommandType.StoredProcedure);
			var userStats = query.Read<UserStatInfoInt>();
			var targetWorkTimeIntervals = query.Read<UserIdTargetWorkTimeIntervalsInt>().ToDictionary(k => k.UserId, v => v.TargetWorkTimeIntervals);
			return userStats.Select(n =>
			{
				targetWorkTimeIntervals.TryGetValue(n.Id, out var interval);
				return UserStatInfoInt.GetUserStatInfo(n, interval);
			});
		}

		public UserStatInfo GetUserStatInfoById(int userId)
		{
			UserStatInfoInt result = null;
			string targetWorkTimeInterval;
#if DEBUG
			targetWorkTimeInterval = "<ArrayOfKeyValueOfdateTimeint xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><KeyValueOfdateTimeint><Key>2020-11-23T00:00:00</Key><Value>480</Value></KeyValueOfdateTimeint></ArrayOfKeyValueOfdateTimeint>";
			if (userId <= UserCountDebug) result = new UserStatInfoInt() { Id = userId, Name = "Job CTRL " + userId, FirstName = "Károly " + userId, LastName = "Job", Email = "unknown@tct.hu.zzz", EndOfDayMinutes = (short)TimeSpan.FromHours(3).TotalMinutes, TimeZoneData = TimeZoneInfo.Local.ToSerializedString(), CultureId = "hu-HU", TargetWorkTimeInMinutes = (int)TimeSpan.FromHours(8).TotalMinutes, FirstWorktime = DateTime.UtcNow, };
			if (userId == 13) result = new UserStatInfoInt() { Id = userId, Name = "Török Zoltán", FirstName = "Zoltán", LastName = "Török", Email = "rado.andras@tct.hu", EndOfDayMinutes = (short)TimeSpan.FromHours(0).TotalMinutes, TimeZoneData = TimeZoneInfo.Local.ToSerializedString(), ReportFreqType = (int)(ReportFrequency.Daily | ReportFrequency.Weekly | ReportFrequency.Monthly), CultureId = "en-US", TargetWorkTimeInMinutes = (int)TimeSpan.FromHours(5).TotalMinutes, FirstWorktime = new DateTime(2019, 1, 1) /*IsClientAcceptanceMessageNeeded = true, ClientAcceptanceMessage = "message <a href=\"page\">link</a>" */, AccessLevel = (int)UserAccessLevel.Reg, ClientDataStorageLimitInDays = 50, ScreenshotStorageLimitInDays = 30, CalendarId = 1, CompanyId = 2 };
			if (userId == 25) result = new UserStatInfoInt() { Id = userId, Name = "Török Zoltán", FirstName = "Zoltán", LastName = "Török", Email = "ztorok@tct.hu", EndOfDayMinutes = (short)TimeSpan.FromHours(3).TotalMinutes, TimeZoneData = TimeZoneInfo.Local.ToSerializedString(), CultureId = "hu-HU", TargetWorkTimeInMinutes = (int)TimeSpan.FromHours(8).TotalMinutes, FirstWorktime = DateTime.UtcNow, };
			if (userId == 383) result = new UserStatInfoInt() { Id = userId, Name = "Radóó András", FirstName = "András", LastName = "Radó", Email = "buzas.ferenc@tct.hu", EndOfDayMinutes = (short)TimeSpan.FromHours(3).TotalMinutes, TimeZoneData = TimeZoneInfo.Local.ToSerializedString(), CultureId = "hu-HU", TargetWorkTimeInMinutes = (int)TimeSpan.FromHours(8).TotalMinutes, FirstWorktime = new DateTime(2019, 1, 1), CalendarId = 1, CompanyId = 2, AccessLevel = 2 };
			if (dppAcceptanceDatesDictionary.ContainsKey(userId))
				result.ClientAcceptanceMessageAcceptedAt = dppAcceptanceDatesDictionary[userId];
#else
			var query = Connection.QueryMultiple("[dbo].[Client_GetUserDataById]", new { UserId = userId }, commandType: CommandType.StoredProcedure);
			result = query.Read<UserStatInfoInt>().SingleOrDefault();
			targetWorkTimeInterval = query.Read<UserIdTargetWorkTimeIntervalsInt>().SingleOrDefault()?.TargetWorkTimeIntervals;
#endif
			return result != null ? UserStatInfoInt.GetUserStatInfo(result, targetWorkTimeInterval) : null;
		}

#if DEBUG
		static Dictionary<int, DateTime> dppAcceptanceDatesDictionary = new Dictionary<int, DateTime>();
#endif

		public bool SetDppAcceptanceDate(int userId, DateTime acceptedAt)
		{
#if DEBUG
			lock (dppAcceptanceDatesDictionary)
			{
				if (dppAcceptanceDatesDictionary.ContainsKey(userId))
					return false;
				dppAcceptanceDatesDictionary[userId] = acceptedAt;
				return true;
			}
#else
			return 0 == ExecuteCommand("exec [dbo].[Client_SetClientMessageAcceptanceDate] @userId={0}, @ClientAcceptanceMessageAcceptedAt={1}", userId, acceptedAt);
#endif
		}

		public IEnumerable<int> GetUserIdsInSameCompany(int userId)
		{
#if DEBUG
			return Enumerable.Range(1, UserCountDebug);
#endif
			return ExecuteQuery<int>("exec [dbo].[Client_GetUserIdsInSameCompany] @userID={0}", userId);
		}

		public IEnumerable<MonitorableUser> GetMonitorableUsers(int userId)
		{
#if DEBUG
			if (userId == 13) return GetUserStatsInfo().Select(n => new MonitorableUser() { UserId = n.Id });
			else return new[] { GetUserStatInfoById(userId) }.Select(n => new MonitorableUser() { UserId = n.Id });
#endif
			return ExecuteQuery<MonitorableUser>("exec [dbo].[Client_GetWorkersOfSupervisor] @UserId={0}", userId);
		}

		public IEnumerable<Work> GetWorks()
		{
#if DEBUG
			return Enumerable.Empty<Work>()
				.Concat(new[]
				{
					new Work(){ Id = 2, ProjectId = 4, Name = "FULK Tm betöltéshez kapcsolódó feladatok (FULK - Hibák, kisebb módosítások)"},
					new Work(){ Id = 3, ProjectId = 3, Name = "Telenor mobil internet (ING fejlesztések 2010)"},
					new Work(){ Id = 4, ProjectId = 2, Name = "JobControl projekt (TCT Belso feladatok)"},
					new Work(){ Id = 5, ProjectId = 2, Name = "Belso Meeting"},
					new Work(){ Id = 6, ProjectId = 2, Name = "DDTS, DTM, adminisztráció"},
				}).Concat(GetPhantomWorks());
#endif
			return ExecuteQuery<Work>("exec [dbo].[Client_GetWorks]");
		}

#if DEBUG
		public IEnumerable<Work> GetPhantomWorks()
		{
			for (int i = 7; i < 6000; i++)
			{
				yield return new Work() { Id = i, Name = " Munka " + i, ProjectId = (i % 4) + 1 };
			}
		}
#endif

		public Work GetWorkById(int workId)
		{
#if DEBUG
			return GetWorks().Where(n => n.Id == workId).FirstOrDefault();
#endif
			return ExecuteQuery<Work>("exec [dbo].[Client_GetWorkById] @workID={0}", workId)
				.SingleOrDefault();
		}

		public IEnumerable<Project> GetProjects()
		{
#if DEBUG
			return Enumerable.Empty<Project>()
				.Concat(new[]
				{
					new Project(){ Id = 1, ParentId = null, Name = "Root"},
					new Project(){ Id = 2, ParentId = 1, Name = "TCT Belso feladatok"},
					new Project(){ Id = 3, ParentId = 1, Name = "INGCSAKHOSSZAN"},
					new Project(){ Id = 4, ParentId = 3, Name = "FULKMEGHOSSZABB"},
				});
#endif
			return ExecuteQuery<Project>("exec [dbo].[Client_GetProjects]");
		}

		public Project GetProjectById(int projectId)
		{
#if DEBUG
			return GetProjects().Where(n => n.Id == projectId).FirstOrDefault();
#endif
			return ExecuteQuery<Project>("exec [dbo].[Client_GetProjectById] @projectId={0}", projectId)
				.SingleOrDefault();
		}

		public IEnumerable<DetailedWork> GetDetailedWorksForUser(int userId)
		{
#if DEBUG
			return Enumerable.Empty<DetailedWork>()
				.Concat(new[] { 
					new DetailedWork() { 
						UserId = userId, Id = 4, Name = "Teszt4", Priority = 100, TargetTotalWorkTime = TimeSpan.FromHours(3), StartDate = new DateTime(2010, 12, 10), EndDate = new DateTime(2011, 1, 20),
					},
					new DetailedWork() { 
						UserId = userId, Id = 3, Name = "Teszt3", Priority = 300, TargetTotalWorkTime = TimeSpan.FromHours(2.01),
					},
					new DetailedWork() { 
						UserId = userId, Id = 2, Name = "Teszt2", Priority = null, StartDate = new DateTime(2010, 12, 10), EndDate = new DateTime(2011, 1, 1), CloseDate =  new DateTime(2010, 12, 12, 02, 23, 56),
					},
				});
#endif
			return ExecuteQuery<DetailedWorkInt>("exec [dbo].[Client_GetAssignedTasksForUser] @userId={0}", userId)
				.Select(n => DetailedWorkInt.GetDetailedWork(n, userId));
		}

		public TaskAndAssignmentDetails GetTaskAndAssignmentDetails(int userId, int workId)
		{
#if DEBUG
			return new TaskAndAssignmentDetails() { Id = workId, UserId = userId, Name = "Task " + workId };
#endif
			return ExecuteQuery<TaskAndAssignmentDetails>("exec [dbo].[Client_GetTaskDetails] @userId={0}, @taskId={1}", userId, workId).SingleOrDefault();
		}

		//quite a lot round-trips... not ideal
		public Wage GetWageData(int workId, int userId, int reportUserId, bool isInternal)
		{
#if DEBUG
			return new HourlyWage(workId);
#endif
			//The query results cannot be enumerated more than once. (be careful with deferred execution)
			var wageChanges = ExecuteQuery<HourlyWageChangeInt>("exec [dbo].[Client_GetHourlyWage] @workId={0}, @userId={1}, @reportUserId={2}, @isInternal={3}", workId, userId, reportUserId, isInternal).ToList();
			return HourlyWageChangeInt.GetHourlyWage(wageChanges);
		}

		public IEnumerable<int> GetReportableProjectRootsForUser(int reportUserId, bool isInternal)
		{
#if DEBUG
			return Enumerable.Empty<int>()
				.Concat(new[] { 1, 2, 3, 4, });
#endif
			return ExecuteQuery<int>("exec [dbo].[Client_GetReportableProjectRootsForUser] @reportUserId={0}, @isInternal={1}", reportUserId, isInternal);
		}

		public IEnumerable<ProjectEmailRequest> GetProjectEmailRequests()
		{
#if DEBUG
			return Enumerable.Empty<ProjectEmailRequest>();
#endif
			var multiResult = GetProjectEmailRequestsImpl();
			//the order should be the same as in the sproc
			var main = multiResult.GetResult<ProjectEmailRequestMainInt>();
			var proj = multiResult.GetResult<ProjectEmailRequestProjectsInt>();
			var email = multiResult.GetResult<ProjectEmailRequestEmailsInt>();

			return ProjectEmailRequest.CreateFrom(main, proj, email);
		}

		[Function(Name = "dbo.Client_GetAggregateEmailRequests")]
		[ResultType(typeof(AggregateEmailRequestUsersInt))]
		[ResultType(typeof(AggregateEmailRequestEmailsInt))]
		[ResultType(typeof(AggregateEmailRequestMainInt))]
		private IMultipleResults GetAggregateEmailRequestsImpl()
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod());
			return (IMultipleResults)result.ReturnValue;
		}

		public IEnumerable<AggregateEmailRequest> GetAggregateEmailRequests()
		{
#if DEBUG
			return new List<AggregateEmailRequest>
				{
					new AggregateEmailRequest
						{
							EmailsTo = new List<EmailTarget>
								{
									new EmailTarget {Address = "rado.andras@tct.hu", CultureId = "hu-hu"},
								},
							//Frequency = ReportFrequency.Daily | ReportFrequency.Weekly | ReportFrequency.Monthly,
							ReportId = 1,
							UserIds = new List<int> {13, 25}
						},
				};
#endif
			var multiResult = GetAggregateEmailRequestsImpl();
			//the order should be the same as in the sproc
			var main = multiResult.GetResult<AggregateEmailRequestMainInt>();
			var user = multiResult.GetResult<AggregateEmailRequestUsersInt>();
			var email = multiResult.GetResult<AggregateEmailRequestEmailsInt>();

			return AggregateEmailRequest.CreateFrom(main, user, email);
		}

		[Function(Name = "dbo.Client_GetProjectEmailRequests")]
		[ResultType(typeof(ProjectEmailRequestProjectsInt))]
		[ResultType(typeof(ProjectEmailRequestEmailsInt))]
		[ResultType(typeof(ProjectEmailRequestMainInt))]
		private IMultipleResults GetProjectEmailRequestsImpl()
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod());
			return (IMultipleResults)result.ReturnValue;
		}

		public MeetingData GetMeetingData(int userId)
		{
#if DEBUG
			var pending = new List<MeetingEntry>()
			{
				//new MeetingEntry() { Title = "MinMax", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, OrganizerName = "En", OrganizerId = userId },
				//new MeetingEntry() { Title = "MaxMin", StartDate = DateTime.MaxValue, EndDate = DateTime.MinValue, OrganizerName = "En", OrganizerId = userId },
				//new MeetingEntry() { Title = "tct workshop sad asd asd asd as das ds a d", StartDate = DateTime.UtcNow.Date.AddHours(13), EndDate = DateTime.UtcNow.Date.AddHours(14), OrganizerName = "Bela" },
			};
#else
			var pending = ExecuteQuery<MeetingEntry>("exec [dbo].[Client_GetPendingMeetings] @userId={0}", userId)
				.OrderBy(n => n.StartDate)
				.ThenBy(n => n.EndDate)
				.ThenBy(n => n.Id)
				.ToList();
#endif
			return new MeetingData()
			{
				PendingMeetings = pending,
			};
		}

		public CannedCloseReasons GetCannedCloseReasons(int userId)
		{
#if DEBUG
			var reasons = new List<UserReason>
			{
				new UserReason{ ReasonId = 1, ReasonItemName = "Csak mert megmondtam", ReasonItemId = 1, ReasonItemParentId = null, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Nyuszi szoros es nem borotvalkozik", ReasonItemId = 2, ReasonItemParentId = null, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Lorem ipsum et al doba upsz lorem ipsum et al doba upsz", ReasonItemId = 3, ReasonItemParentId = null, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Hosszu", ReasonItemId = 4, ReasonItemParentId = 1, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Rovid", ReasonItemId = 5, ReasonItemParentId = 1, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Hosszu", ReasonItemId = 6, ReasonItemParentId = 2, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Rovid", ReasonItemId = 7, ReasonItemParentId = 2, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Hosszu", ReasonItemId = 8, ReasonItemParentId = 3, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Rovid", ReasonItemId = 9, ReasonItemParentId = 3, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Sima ugyfel", ReasonItemId = 10, ReasonItemParentId = 4, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Pokroc ugyfel", ReasonItemId = 11, ReasonItemParentId = 4, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Ez egy hosszu indoklas", ReasonItemId = 12, ReasonItemParentId = 11, ReasonName = "Indoklas 1"},
				new UserReason{ ReasonId = 1, ReasonItemName = "Ez meg annal is hosszabb indoklas mert kell ilyen is", ReasonItemId = 13, ReasonItemParentId = 11, ReasonName = "Indoklas 1"},
			};
			var defaultReasons = new List<string> { reasons[0].ReasonItemName, reasons[1].ReasonItemName, reasons[2].ReasonItemName };
#else
			var reasons = ExecuteQuery<UserReason>("exec [dbo].[Client_GetUsersReasons] @userId={0}", userId)
				.OrderBy(u => u.ReasonItemName);
			var defaultReasons = ExecuteQuery<string>("exec [dbo].[Client_GetCannedCloseReasons] @userId={0}", userId)
				.Where(n => !string.IsNullOrEmpty(n))
				.ToList();
#endif
			return new CannedCloseReasons()
			{
				IsReadonly = false,
				TreeRoot = BuildReasonNodeTree(reasons),
				DefaultReasons = defaultReasons
			};
		}

		internal static List<CloseReasonNode> BuildReasonNodeTree(IEnumerable<UserReason> reasons)
		{
			var reasonLookup = reasons.ToLookup(n => n.ReasonItemParentId);
			var result = new List<CloseReasonNode>();
			var queue = new Queue<CloseReasonNode>();
			var visited = new HashSet<int>();
			foreach (var userReason in reasonLookup[null]) //get roots
			{
				var node = new CloseReasonNode() { NodeId = userReason.ReasonItemId, ReasonPart = userReason.ReasonItemName };
				result.Add(node);
				queue.Enqueue(node);
			}
			while (queue.Count > 0)
			{
				var currNode = queue.Dequeue();
				if (!visited.Add(currNode.NodeId)) continue; //this shouldn't happen as ReasonItemId should be PK (but you never know...)
				foreach (var userReason in reasonLookup[currNode.NodeId])
				{
					var node = new CloseReasonNode() { NodeId = userReason.ReasonItemId, ReasonPart = userReason.ReasonItemName };
					if (currNode.Children == null)
					{
						currNode.Children = new List<CloseReasonNode>();
					}
					currNode.Children.Add(node);
					queue.Enqueue(node);
				}
			}
			return result;
		}

#if DEBUG
		private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, List<Reason>> reasons = new System.Collections.Concurrent.ConcurrentDictionary<string, List<Reason>>();
#endif

		public int AddReason(int userId, int workId, string reason, int? reasonItemId, DateTime? creationDate = null)
		{
			if (reason == null && reasonItemId == null) throw new ArgumentNullException();
#if DEBUG
			var item = new List<Reason>() { new Reason { createdAt = creationDate ?? DateTime.Now, ReasonText = reason, ReasonItemId = reasonItemId } };
			return reasons.AddOrUpdate(userId + "_" + workId, item, (_, old) => old.Concat(item).ToList()).Count;
#endif
			int reasonCount;
			int ret = AddReasonInt(userId, workId, reason, reasonItemId, out reasonCount, creationDate);
			return ret == 0 ? reasonCount : ret;
		}

		[Function(Name = "dbo.Client_AddReason")]
		private int AddReasonInt(
			[Parameter(Name = "UserId", DbType = "Int")] int userId,
			[Parameter(Name = "WorkId", DbType = "Int")] int workId,
			[Parameter(Name = "Reason", DbType = "nvarchar(max)")] string reason,
			[Parameter(Name = "ReasonItemId", DbType = "Int")] int? reasonItemId,
			[Parameter(Name = "ReasonCount", DbType = "Int")] out int reasonCount,
			[Parameter(Name = "ReasonCreationDate", DbType = "datetime2(2)")] DateTime? reasonCreationDate
			)
		{
			reasonCount = 0;
			IExecuteResult result = ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), userId, workId, reason, reasonItemId, reasonCount, reasonCreationDate);
			reasonCount = (int)result.GetParameterValue(4);
			return (int)result.ReturnValue;
		}

		public ReasonStats GetReasonStats(int userId)
		{
#if DEBUG
			return new ReasonStats() { ReasonCountByWorkId = reasons.ToArray().Where(n => n.Key.StartsWith(userId + "_")).ToDictionary(n => int.Parse(n.Key.Substring(n.Key.IndexOf('_') + 1)), n => n.Value.Count) };
#endif
			return ReasonStatsInt.CreateFrom(ExecuteQuery<ReasonStatsInt>("exec [dbo].[Client_GetReasonStats] @UserId={0}", userId));
		}

		public bool HasUserGotCreditForInterval(int userId, DateTime startDate, DateTime endDate)
		{
#if DEBUG
			if (userId < 1) return false;
			return true;
#endif
			return !ExecuteQuery<DateTime>("exec [dbo].[Client_GetUnpaidDaysForUser] @userId={0}, @startDay={1}, @endDay={2}", userId.ToString(), startDate.Date, endDate.Date).Any();
		}

		public int? GetUserId(string email)
		{
#if DEBUG
			return 1;
#endif
			return ExecuteQuery<int>("SELECT Id FROM [dbo].[User] WHERE Email = {0} AND Status <> 2", email).SingleOrDefault();
		}

		public string GetSalt(int userId)
		{
#if DEBUG
			return "EWySTENt61nDpGpnWMbgOSal";
#endif
			return ExecuteQuery<string>("SELECT Salt FROM [dbo].[User] WHERE Status <> 2 AND Id={0}", userId)
				.SingleOrDefault();
		}

		public bool Validate(int userId, string hashedPassword)
		{
#if DEBUG
			//pass: 6B86B273FF34FCE19D6B804EFF5A3F5747ADA4EAA22F1D49C01E52DDB7875B4B
			if (userId <= UserCountDebug && hashedPassword == "5E17183A93CD1F5E0DC8D8C462D70F01427397E5557854DAE1D57C7965702C0D") return true;
			return false;
#endif
			//AND Type = 8 -- it is not appropriate to tell an Admin that her password is invalid just because she has no worker part
			return ExecuteQuery<string>("SELECT '1' FROM [dbo].[User] WHERE Status <> 2 AND Id={0} AND Pwdhash={1}", userId, hashedPassword)
				.SingleOrDefault() == "1";
		}

		public string GetAuthTicket(int userId)
		{
#if DEBUG
			return null;
#endif
			return ExecuteQuery<Guid>(
				"declare @ticket uniqueidentifier\n" +
				"exec [dbo].[GenerateClientLoginTicket] @UserId={0}, @Ticket=@ticket out\n" +
				"SELECT @ticket", userId).Select(n => n.ToString("D")).FirstOrDefault();
		}

		public bool ValidateTicket(int userId, Guid ticket)
		{
#if DEBUG
			return false;
#endif
			return ExecuteQuery<int>("exec [dbo].[Client_ValidateLoginTicket] @userId={0}, @ticket={1}", userId, ticket)
				.DefaultIfEmpty(1)
				.First() == 0;
		}

		public int? GetUserIdFromSid(string sid)
		{
#if DEBUG
			return 13;
#endif
			return ExecuteQuery<int?>(
				"declare @UserId int\n" +
				"exec [dbo].[Client_GetUserIdFromSid] @sid={0}, @userId=@UserId out\n" +
				"SELECT @UserId", sid).FirstOrDefault();
		}

		public IEnumerable<AllWorkItem> GetAllWorks(int userId)
		{
#if DEBUG
			return new List<AllWorkItem>
			{
				new AllWorkItem{ TaskId = -1, Name = "ceg nev kft.", OwnTask = false, ClosedAt = null, ParentId = null, Type = 0},
				new AllWorkItem{ TaskId = 1, Name = "gyoker", OwnTask = false, ClosedAt = null, ParentId = -1, Type = 1},
				new AllWorkItem{ TaskId = 2, Name = "alma", OwnTask = false, ClosedAt = null, ParentId = 1, Type = 2},
				new AllWorkItem{ TaskId = 3, Name = "korte", OwnTask = false, ClosedAt = DateTime.Now, ParentId = 2, Type = 2},
				new AllWorkItem{ TaskId = 4, Name = "szilva", OwnTask = false, ClosedAt = null, ParentId = 1, Type = 2},
				new AllWorkItem{ TaskId = 17714, Name = "Híváslisták központi tárolása", OwnTask = true,  ClosedAt = null, ParentId = 1, Type = 2},
				new AllWorkItem{ TaskId = 5, Name = "masik proj", OwnTask = false,  ClosedAt = null, ParentId = -1, Type = 1},
				new AllWorkItem{ TaskId = 6, Name = "masik work", OwnTask = false,  ClosedAt = null, ParentId = 5, Type = 2},
			};
#endif
			return ExecuteQuery<AllWorkItem>("exec [dbo].[Client_GetAllWorks] @UserId={0}", userId);
		}

		public TaskReasons GetTaskReasons(int userId)
		{
#if DEBUG
			return new TaskReasons() { ReasonsByWorkId = reasons.ToArray().Where(n => n.Key.StartsWith(userId + "_")).ToDictionary(n => int.Parse(n.Key.Substring(n.Key.IndexOf('_') + 1)), n => n.Value.Select(r => new Reason { ReasonItemId = r.ReasonItemId, ReasonText = r.ReasonText, createdAt = r.createdAt }).ToList()) };
#endif
			return TaskReasonRow.CreateFrom(ExecuteQuery<TaskReasonRow>("exec [dbo].[Client_GetTaskResponses] @UserId={0}", userId));
		}
#pragma warning restore 162

		public DateTime? GetExpiryDay(int userId)
		{
#if DEBUG
			return userId == 25 ? DateTime.Today.AddDays(5) : (DateTime?)null;
#endif
			return ExecuteQuery<DateTime?>(
				"declare @localExpiryDay date\n" +
				"exec [dbo].[Client_GetExpiryDay] @UserId={0}, @localExpiryDay = @localExpiryDay OUTPUT\n" +
				"SELECT @localExpiryDay", userId).FirstOrDefault();
		}

		public string GetUpdatePackage(int userId, string currentVersion, string application)
		{
#if DEBUG
			var searchFor = @"c:\JobCTRL.msi";
			if (System.IO.File.Exists(searchFor))
			{
				return searchFor;
			}
			return null;
#else
			return ExecuteQuery<string>("exec [dbo].[Client_GetUpdatePackage] @UserId={0}, @CurrentVersion={1}, @Application={2}", userId,
								 currentVersion, application)
					.SingleOrDefault();
#endif
		}

		public IEnumerable<UserReportFrequency> GetAggregateMobileLocationRequests()
		{
#if DEBUG
			return Enumerable.Empty<UserReportFrequency>();
#endif
			return ExecuteQuery<UserReportFrequencyInt>("exec [dbo].[Client_GetAggregateMobileLocationRequests]").Select(UserReportFrequency.CreateFrom);
		}

#if DEBUG
		public static readonly List<WorktimeSchedulesItem> TimeSchedulesDataTest = new List<WorktimeSchedulesItem> {
			new WorktimeSchedulesItem{Day = DateTime.Today.AddDays(8), IsWorkDay = true, StartDate = DateTime.Today.AddDays(8).AddHours(8), EndDate = DateTime.Today.AddDays(8).AddHours(14)},
			new WorktimeSchedulesItem{Day = DateTime.Today.AddDays(8), IsWorkDay = true, StartDate = DateTime.Today.AddDays(8).AddHours(20), EndDate = DateTime.Today.AddDays(9).AddHours(3)},
			new WorktimeSchedulesItem{Day = DateTime.Today, IsWorkDay = true, StartDate = DateTime.Today.AddHours(8), EndDate = DateTime.Today.AddHours(12), ExtendedPropertiesJSON = "[{\"Name\":\"SpecificDailyOvertimeLimit\",\"Value\":\"02:00\"}]"},
			new WorktimeSchedulesItem{Day = DateTime.Today, IsWorkDay = true, StartDate = DateTime.Today.AddHours(13), EndDate = DateTime.Today.AddHours(17), ExtendedPropertiesJSON = "[{\"Name\":\"SpecificDailyOvertimeLimit\",\"Value\":\"02:00\"}]"},
			new WorktimeSchedulesItem{Day = DateTime.Today.AddDays(1), IsWorkDay = false, ExtendedPropertiesJSON = "[{\"Name\":\"SpecificDailyOvertimeLimit\",\"Value\":\"00:00\"}]"},
		};

		public static TimeZoneInfo TimeSchedulesTimeZoneTest { get; set; } = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(UTC+01:00) Budapest, Belgrád, Ljubljana, Pozsony, Prága;Közép-európai téli idõ ;Közép-európai nyári idõ ;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");
		public static int TimeSchedulesEndOfDayInMinutes { get; set; }
#endif

		public IEnumerable<WorktimeSchedulesItem> GetWorktimeSchedulesForUser(int userId, DateTime startDateInclusive, DateTime endDateInclusive, out TimeZoneInfo TimeZone, out int EndOfDayInMinutes)
		{
#if DEBUG
			TimeZone = TimeSchedulesTimeZoneTest;
			EndOfDayInMinutes = TimeSchedulesEndOfDayInMinutes;
			return TimeSchedulesDataTest.Where(t => startDateInclusive <= t.Day && t.Day <= endDateInclusive || startDateInclusive <= t.StartDate.Date && t.StartDate.Date <= endDateInclusive || startDateInclusive <= t.EndDate.Date && t.EndDate.Date <= endDateInclusive);
#endif
			var pars = new DynamicParameters();
			pars.Add("UserId", userId);
			pars.Add("StartDayInclusive", startDateInclusive);
			pars.Add("EndDayInclusive", endDateInclusive);
			pars.Add("TimeZoneData", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);
			pars.Add("EndOfDayInMinutes", dbType: DbType.Int16, direction: ParameterDirection.Output);
			var res = Connection.Query<WorktimeSchedulesItem>("Client_GetWorktimeSchedulesForUser", pars, commandType: CommandType.StoredProcedure);
			TimeZone = TimeZoneInfo.FromSerializedString(pars.Get<string>("TimeZoneData"));
			EndOfDayInMinutes = pars.Get<short>("EndOfDayInMinutes");
			return res;
		}

		public void InsertGlobalActivityEvent(int globalEventTypeId, string eventDetails)
		{
#if !DEBUG
			Connection.Execute("[dbo].[InsertGlobalActivityEvent]", new { globalEventTypeId, eventDetails }, commandType: CommandType.StoredProcedure);
#endif
		}

	}

	public class UserId
	{
		public int Id { get; set; }
		public int GroupId { get; set; }
		public int CompanyId { get; set; }
	}

	public class UserStatInfo
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Email { get; set; }
		public TimeSpan StartOfDayOffset { get; set; }
		public TimeZoneInfo TimeZone { get; set; }
		public int CalendarId { get; set; }
		public ReportFrequency ReportFreqType { get; set; }
		public List<string> AggregateEmails { get; set; }
		public string CultureId { get; set; }
		public TimeSpan TargetWorkTime { get; set; }
		public DateTime FirstWorkDay { get; set; }
		public int CompanyId { get; set; }
		public UserAccessLevel AccessLevel { get; set; }
		public string ClientAcceptanceMessage { get; set; }
		public bool IsClientAcceptanceMessageNeeded { get; set; }
		public DateTime? ClientAcceptanceMessageAcceptedAt { get; set; }
		public int DataStorageLimitInDays { get; set; }
		public int ScreenshotStorageLimitInDays { get; set; }
		public int LowestLevelOfInactivityInMins { get; set; }
		public List<TargetWorkTimeInterval> TargetWorkTimeIntervals { get; set; }
	}

	internal class UserStatInfoInt
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public int Id { get; set; }
		public string Name { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Email { get; set; }
		public string AggregateEmail { get; set; }
		public short EndOfDayMinutes { get; set; }
		public string TimeZoneData { get; set; }
		public int CalendarId { get; set; }
		public int ReportFreqType { get; set; }
		public string CultureId { get; set; }
		public int TargetWorkTimeInMinutes { get; set; }
		public DateTime FirstWorktime { get; set; }
		public int CompanyId { get; set; }
		public int AccessLevel { get; set; }
		public string ClientAcceptanceMessage { get; set; }
		public bool IsClientAcceptanceMessageNeeded { get; set; }
		public DateTime? ClientAcceptanceMessageAcceptedAt { get; set; }
		public int ClientDataStorageLimitInDays { get; set; }
		public int ScreenshotStorageLimitInDays { get; set; }
		public int LowestLevelOfInactivityInMins { get; set; }

		internal static UserStatInfo GetUserStatInfo(UserStatInfoInt info, string interval)
		{
			if (info == null) return null;
			TargetWorkTimeIntervalsInt targetWorkTimeIntervals;
			if (interval != null)
			{
				var serializer = new XmlSerializer(typeof(TargetWorkTimeIntervalsInt));
				using (TextReader reader = new StringReader(interval))
				{
					targetWorkTimeIntervals = (TargetWorkTimeIntervalsInt) serializer.Deserialize(reader);
					if (targetWorkTimeIntervals.Items?.Count == 0) targetWorkTimeIntervals = null;
				}
			}
			else targetWorkTimeIntervals = null;
			var result = new UserStatInfo()
			{
				Id = info.Id,
				Email = info.Email,
				FirstName = info.FirstName,
				LastName = info.LastName,
				CalendarId = info.CalendarId,
				ReportFreqType = (ReportFrequency)info.ReportFreqType,
				AggregateEmails = info.AggregateEmail == null
					? null
					: info.AggregateEmail.Split(EmailHelper.Separators, StringSplitOptions.RemoveEmptyEntries)
						.Distinct()
						.ToList(),
				CultureId = info.CultureId,
				ClientAcceptanceMessage = info.ClientAcceptanceMessage,
				IsClientAcceptanceMessageNeeded = info.IsClientAcceptanceMessageNeeded,
				ClientAcceptanceMessageAcceptedAt = info.ClientAcceptanceMessageAcceptedAt,
				CompanyId = info.CompanyId,
				AccessLevel = (UserAccessLevel)info.AccessLevel,
				DataStorageLimitInDays = info.ClientDataStorageLimitInDays,
				ScreenshotStorageLimitInDays = info.ScreenshotStorageLimitInDays,
				LowestLevelOfInactivityInMins = info.LowestLevelOfInactivityInMins,
				TargetWorkTimeIntervals = targetWorkTimeIntervals?.Items?.Select(t => new TargetWorkTimeInterval { StartDate = t.StartDate, TargetWorkTime = TimeSpan.FromMinutes(t.TargetWorkTimeMins) }).OrderBy(t => t.StartDate).ToList()
			};
			try
			{
				var ci = new CultureInfo(info.CultureId);
				result.Name = ci.GetCultureSpecificName(info.FirstName, info.LastName);
			}
			catch (Exception)
			{
				result.Name = CultureInfo.InvariantCulture.GetCultureSpecificName(info.FirstName, info.LastName);
			}
			try
			{
				result.StartOfDayOffset = TimeSpan.FromMinutes(info.EndOfDayMinutes);
			}
			catch (Exception ex)
			{
				result.StartOfDayOffset = TimeSpan.FromHours(3);
				log.Error("Unable to parse TimeSpan from EndOfDayMinutes: " + info.EndOfDayMinutes, ex);
			}
			try
			{
				result.TimeZone = TimeZoneInfo.FromSerializedString(info.TimeZoneData);
			}
			catch (Exception ex)
			{
				result.TimeZone = TimeZoneInfo.Local; //omg ;]
				log.Error("Unable to parse TimeZoneInfo from TimeZoneData:" + info.TimeZoneData, ex);
			}
			try
			{
				result.TargetWorkTime = TimeSpan.FromMinutes(info.TargetWorkTimeInMinutes);
			}
			catch (Exception ex)
			{
				result.TargetWorkTime = TimeSpan.FromHours(8);
				log.Error("Unable to parse TimeSpan from TargetWorkTimeInMinutes: " + info.TargetWorkTimeInMinutes, ex);
			}
			result.FirstWorkDay = CalculatorHelper.GetLocalReportDate(info.FirstWorktime, result.TimeZone, result.StartOfDayOffset);
			return result;
		}
	}

	public class Work
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int ProjectId { get; set; }
	}

	public class Project
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int? ParentId { get; set; }
	}

	public class DetailedWork : Work //contains user specific data for a work
	{
		public int UserId { get; set; }
		public int? Priority { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public TimeSpan? TargetTotalWorkTime { get; set; }
		public DateTime? CloseDate { get; set; }
	}

	public class TaskAndAssignmentDetails
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Name { get; set; }
		public short? AssignmentPriority { get; set; }
		public DateTime? AssignmentStartDate { get; set; }
		public DateTime? AssignmentEndDate { get; set; }
		public int? AssignmentPlannedWorkTimeInMinutes { get; set; }
		public short? TaskPriority { get; set; }
		public DateTime? TaskStartDate { get; set; }
		public DateTime? TaskEndDate { get; set; }
		public int? TaskPlannedWorkTimeInMinutes { get; set; }
		public decimal? TaskTargetCost { get; set; }
		public short? TaskMeetingDurationInMinutes { get; set; }
		public int? TaskCategoryId { get; set; }
		public bool? TaskIsForMobile { get; set; }
		public short? TaskCloseAfterInactiveHours { get; set; }
		public string TaskDescription { get; set; }

		public TimeSpan? AssigmentTargetTotalWorkTime
		{
			get
			{
				return AssignmentPlannedWorkTimeInMinutes.HasValue
					? TimeSpan.FromMinutes(AssignmentPlannedWorkTimeInMinutes.Value)
					: new TimeSpan?();
			}
			private set { ;}
		}

		public TimeSpan? TaskTargetTotalWorkTime
		{
			get
			{
				return TaskPlannedWorkTimeInMinutes.HasValue
					? TimeSpan.FromMinutes(TaskPlannedWorkTimeInMinutes.Value)
					: new TimeSpan?();
			}
			private set { ;}
		}

		public TimeSpan? TaskManualAddWorkDuration
		{
			get
			{
				return TaskMeetingDurationInMinutes.HasValue
					? TimeSpan.FromMinutes(TaskMeetingDurationInMinutes.Value)
					: new TimeSpan?();
			}
			private set { ;}
		}
	}

	internal class DetailedWorkInt
	{
		public int TaskID { get; set; }
		public short? Priority { get; set; }
		public int? PlannedWorkTimeInMinutes { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime? ClosedAt { get; set; }
		public string Name { get; set; }
		public int? ParentID { get; set; }
		public int? ExtID { get; set; }
		public string ParentName { get; set; }

		internal static DetailedWork GetDetailedWork(DetailedWorkInt work, int userId)
		{
			if (work == null) return null;
			return new DetailedWork()
			{
				UserId = userId,
				ProjectId = work.ParentID.Value, //should not be null for works
				Id = work.TaskID,
				Name = work.Name,
				Priority = work.Priority,
				StartDate = work.StartDate,
				EndDate = work.EndDate,
				TargetTotalWorkTime = work.PlannedWorkTimeInMinutes.HasValue ? TimeSpan.FromMinutes(work.PlannedWorkTimeInMinutes.Value) : new TimeSpan?(),
				CloseDate = work.ClosedAt,
			};
		}
	}

	internal class HourlyWageChangeInt
	{
		public DateTime? StartDate { get; set; }
		public decimal Wage { get; set; }

		internal static HourlyWage GetHourlyWage(IEnumerable<HourlyWageChangeInt> wageChanges)
		{
			if (wageChanges == null || !wageChanges.Any()) return null;
			//ideally we should treat as error if there is an interval where wage is not set
			var result = new HourlyWage(0); //hax until the wage is set it's zero
			foreach (var change in wageChanges)
			{
				result.SetChange(change.StartDate ?? DateTime.MinValue, change.Wage);
			}
			return result;
		}
	}

	internal class ProjectEmailRequestMainInt
	{
		public int ReportId { get; set; }
		public int ReportUserId { get; set; }
		public bool IsInternal { get; set; }
		public int ReportFreqType { get; set; }
	}

	internal class ProjectEmailRequestProjectsInt
	{
		public int ReportId { get; set; }
		public int ProjectRootId { get; set; }
	}

	internal class ProjectEmailRequestEmailsInt
	{
		public int ReportId { get; set; }
		public string To { get; set; }
		public string CultureId { get; set; }
	}

	public class ProjectEmailRequest
	{
		public int ReportId { get; set; }
		public int ReportUserId { get; set; }
		public bool IsInternal { get; set; }
		public ReportFrequency Frequency { get; set; }
		public List<int> ProjectRootIds { get; set; }
		public List<EmailTarget> EmailsTo { get; set; }

		internal static IEnumerable<ProjectEmailRequest> CreateFrom(
			IEnumerable<ProjectEmailRequestMainInt> main,
			IEnumerable<ProjectEmailRequestProjectsInt> projs,
			IEnumerable<ProjectEmailRequestEmailsInt> emails
			)
		{
			var mainDict = main.ToDictionary(n => n.ReportId);
			var projLook = projs.ToLookup(n => n.ReportId);
			var emailLook = emails.ToLookup(n => n.ReportId);

			foreach (var mainInt in mainDict)
			{
				yield return new ProjectEmailRequest()
				{
					ReportId = mainInt.Key,
					IsInternal = mainInt.Value.IsInternal,
					ReportUserId = mainInt.Value.ReportUserId,
					Frequency = (ReportFrequency)mainInt.Value.ReportFreqType,
					ProjectRootIds = projLook[mainInt.Key].Select(n => n.ProjectRootId).ToList(),
					EmailsTo = emailLook[mainInt.Key].Select(n => new EmailTarget { Address = n.To, CultureId = n.CultureId }).ToList(),
				};
			}
		}
	}

	internal class AggregateEmailRequestMainInt
	{
		public long ReportId { get; set; }
		public int ReportFreqType { get; set; }
	}

	internal class AggregateEmailRequestUsersInt
	{
		public long ReportId { get; set; }
		public int UserId { get; set; }
	}

	internal class AggregateEmailRequestEmailsInt
	{
		public long ReportId { get; set; }
		public string To { get; set; }
		public string CultureId { get; set; }
	}

	public class EmailTarget
	{
		public string Address { get; set; }
		public string CultureId { get; set; }
	}

	public class AggregateEmailRequest
	{
		public int ReportId { get; set; }
		public ReportFrequency Frequency { get; set; }
		public List<int> UserIds { get; set; }
		public List<EmailTarget> EmailsTo { get; set; }

		internal static IEnumerable<AggregateEmailRequest> CreateFrom(
			IEnumerable<AggregateEmailRequestMainInt> main,
			IEnumerable<AggregateEmailRequestUsersInt> users,
			IEnumerable<AggregateEmailRequestEmailsInt> emails
			)
		{
			var mainDict = main.ToDictionary(n => n.ReportId);
			var userLook = users.ToLookup(n => n.ReportId);
			var emailLook = emails.ToLookup(n => n.ReportId);

			foreach (var mainInt in mainDict)
			{
				yield return new AggregateEmailRequest()
				{
					ReportId = (int)mainInt.Key,
					Frequency = (ReportFrequency)mainInt.Value.ReportFreqType,
					UserIds = userLook[mainInt.Key].Select(n => n.UserId).ToList(),
					EmailsTo = emailLook[mainInt.Key].Select(n => new EmailTarget { Address = n.To, CultureId = n.CultureId }).ToList(),
				};
			}
		}
	}

	public class MonitorableUser
	{
		public int UserId { get; set; }
		public bool ScreenShotsHidden { get; set; }
	}

	public class ReasonStatsInt
	{
		public int WorkId { get; set; }
		public int ReasonCount { get; set; }

		internal static ReasonStats CreateFrom(IEnumerable<ReasonStatsInt> data)
		{
			return new ReasonStats()
			{
				ReasonCountByWorkId = data.ToDictionary(n => n.WorkId, n => n.ReasonCount),
			};
		}
	}

	public class UserReason
	{
		public int ReasonId { get; set; }
		public string ReasonName { get; set; }
		public string ReasonItemName { get; set; }
		public int ReasonItemId { get; set; }
		public int? ReasonItemParentId { get; set; }
	}

	public class AllWorkItem
	{
		public int TaskId { get; set; }
		public int? ParentId { get; set; }
		public string Name { get; set; }
		public DateTime? ClosedAt { get; set; }
		public bool OwnTask { get; set; }
		public byte Type { get; set; }
	}

	public class TaskReasonRow
	{
		public int TaskId { get; set; }
		public int? ReasonItemId { get; set; }
		public string ReasonText { get; set; }
		public DateTime createdAt { get; set; }

		internal static TaskReasons CreateFrom(IEnumerable<TaskReasonRow> rows)
		{
			return new TaskReasons
			{
				ReasonsByWorkId = rows
								   .GroupBy(r => r.TaskId)
								   .ToDictionary(g => g.Key, g => g
																   .Select(v => new Reason
																   {
																	   ReasonItemId = v.ReasonItemId,
																	   ReasonText = v.ReasonText,
																	   createdAt = v.createdAt
																   })
																   .ToList())
			};
		}
	}

	public class TaskReasons
	{
		public Dictionary<int, List<Reason>> ReasonsByWorkId { get; set; }
	}

	public class Reason
	{
		public int? ReasonItemId { get; set; }
		public string ReasonText { get; set; }
		public DateTime createdAt { get; set; }
	}

	public class UserReportFrequencyInt
	{
		public int UserId { get; set; }
		public int ReportFreqType { get; set; }
	}

	public class UserReportFrequency
	{
		public int UserId { get; set; }
		public ReportFrequency Frequency { get; set; }

		public static UserReportFrequency CreateFrom(UserReportFrequencyInt source)
		{
			return new UserReportFrequency
				{
					UserId = source.UserId,
					Frequency = (ReportFrequency)source.ReportFreqType,
				};
		}
	}

	public class WorktimeSchedulesItem
	{
		public DateTime Day { get; set; }
		public bool IsInOffice { get; set; }
		public bool IsWorkDay { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int TaskId { get; set; }
		public bool IsOvertime { get; set; }
		public string ExtendedPropertiesJSON { get; set; }

		public TimeSpan? SpecificDailyOvertimeLimit => ExtendedPropertiesJSON != null ? JArray.Parse(ExtendedPropertiesJSON)
			.Where(i => i["Name"].ToString() == "SpecificDailyOvertimeLimit").Select(i => (TimeSpan?)TimeSpan.Parse(i["Value"].ToString()))
			.FirstOrDefault() : null;
	}

	internal class UserIdTargetWorkTimeIntervalsInt
	{
		public int UserId { get; set; }
		public string TargetWorkTimeIntervals { get; set; }
	}

	[XmlRoot("ArrayOfKeyValueOfdateTimeint", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
	public class TargetWorkTimeIntervalsInt
	{
		[XmlElement("KeyValueOfdateTimeint")]
		public List<TargetWorkTimeIntervalInt> Items { get; set; }
	}

	public class TargetWorkTimeIntervalInt
	{
		[XmlElement("Key")]
		public DateTime StartDate { get; set; }
		[XmlElement("Value")]
		public int TargetWorkTimeMins { get; set; }
	}

	public class TargetWorkTimeInterval
	{
		public DateTime StartDate { get; set; }
		public TimeSpan TargetWorkTime { get; set; }

		public override bool Equals(object obj)
		{
			if (!(obj is TargetWorkTimeInterval other)) return false;
			return StartDate.Equals(other.StartDate) && TargetWorkTime.Equals(other.TargetWorkTime);
		}

		public override int GetHashCode()
		{
			return 5 * StartDate.GetHashCode() + 17 * TargetWorkTime.GetHashCode();
		}
	}

	//todo this file is too big do some refactoring
}
