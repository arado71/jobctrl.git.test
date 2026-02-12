using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using log4net;

namespace Tct.ActivityRecorderService.WorkTimeHistory
{
	/// <summary>
	/// Class for retriving WorkTimeHistory from the db.
	/// </summary>
	public static class WorkTimeHistoryDbHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static WorkNames GetWorkNames(int userId, List<int> workIds)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default._jobcontrolConnectionString))
			{
				var workIdTable = new DataTable();
				workIdTable.Columns.Add("Id", typeof(int)); //IntIdTableType

				foreach (var wid in workIds)
				{
					workIdTable.Rows.Add(wid);
				}

				var result = conn.Query<WorkOrProjectName>("Client_GetWorkNames",
					new
					{
						UserId = userId,
						WorkIds = workIdTable,
					},
					commandType: CommandType.StoredProcedure)
					.EnsureList();

				log.Debug("Loaded " + result.Count.ToInvariantString() + " WorkNames for user " + userId.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
				return new WorkNames() { Names = result };
			}
		}

		public static ClientWorkTimeHistory GetWorkTimeHistory(int userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default._jobcontrolConnectionString))
			{
				var result = new ClientWorkTimeHistory();
				var query = conn.QueryMultiple("Client_GetWorkTimeHistory",
					new
					{
						UserId = userId,
						StartDate = startDate,
						EndDate = endDate,
						MinStartDate = startDate.AddDays(-2) //this is not appropriate but good enough atm.
					},
					commandType: CommandType.StoredProcedure);

				var settings = query.Read<UserSettings>().Single();
				result.IsModificationApprovalNeeded = settings.IsModificationApprovalNeeded;
				result.ModificationAgeLimit = settings.ModificationAgeLimit;
				result.ComputerIntervals = query.Read<ComputerInterval>().EnsureList();
				result.MobileIntervals = query.Read<MobileInterval>().EnsureList();
				result.ManualIntervals = query.Read<ManualInterval>().EnsureList();

				log.Debug("Loaded work time history Co/Mo/Iv/Ma "
					+ result.ComputerIntervals.Count.ToInvariantString() + "/"
					+ result.MobileIntervals.Count.ToInvariantString() + "/"
					+ result.ManualIntervals.Count.ToInvariantString()
					+ " for user " + userId.ToInvariantString() + " between " + startDate.ToInvariantString() + " and " + endDate.ToInvariantString() + " in " + sw.ToTotalMillisecondsString() + "ms");
				return result;
			}
		}

		public static UserSettings GetUserSettings(int userId)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default._jobcontrolConnectionString))
			{
				var query = conn.QueryMultiple("Client_GetUserSettings",
					new
					{
						UserId = userId,
					},
					commandType: CommandType.StoredProcedure);

				var settings = query.Read<UserSettings>().Single();

				log.Debug("Loaded UserSettings for user " + userId.ToInvariantString() + " set: " + settings + " in " + sw.ToTotalMillisecondsString() + "ms");
				return settings;
			}
		}
	}
}

/*
CREATE PROCEDURE [dbo].[Client_GetWorkTimeHistory]
	@userId int,
	@startDate datetime,
	@endDate datetime,
	@minStartDate datetime
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON


	EXEC [dbo].[Client_GetUserSettings] @userId = @UserId


	SELECT [WorkId], [StartDate], [EndDate], [ComputerId]
	FROM [dbo].[AggregateWorkItemIntervals]
	WHERE
	 [UserId] = @UserId
	 AND @MinStartDate <= [StartDate] --no usable index on EndDate
	 AND [StartDate] < @EndDate
	 AND @StartDate < DATEADD(ms, 0, [EndDate]) --don't use index on EndDate


	SELECT [WorkId], [StartDate], [EndDate], [Imei]
	FROM [dbo].[MobileWorkItems]
	WHERE
	 [UserId] = @userId
	 AND @MinStartDate <= [StartDate] --no index on EndDate
	 AND [StartDate] < @EndDate
	 AND @StartDate < [EndDate]


	SELECT [WorkId], [StartDate], [dbo].[GetIvrEndDateNotNull]([EndDate], [IvrLastCheckDate], [MaxEndDate]) AS [EndDate], CASE WHEN [EndDate] IS NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS [IsOngoing]
	FROM [dbo].[IvrWorkItems]
	WHERE
	 [UserId] = @userId
	 AND @MinStartDate <= [StartDate] --no index on EndDate
	 AND [StartDate] < @EndDate
	 AND (([EndDate] IS NOT NULL AND @StartDate < [EndDate])
	 OR ([EndDate] IS NULL AND [IvrLastCheckDate] < [MaxEndDate] AND @StartDate < [IvrLastCheckDate])
	 OR ([EndDate] IS NULL AND [IvrLastCheckDate] >= [MaxEndDate] AND @StartDate < [MaxEndDate]))


	SELECT mw.[Id], ISNULL(mw.[WorkId],0) AS [WorkId], mw.[StartDate], mw.[EndDate], mw.[ManualWorkItemTypeId] AS [ManualWorkItemType], mw.[SourceId], mw.[Comment], m.[Title] AS [Subject], m.[Description], m.[MeetingId], NULL AS [PendingId], CAST(0 AS bit) AS [IsPendingDeleteAlso]
	FROM ManualWorkItems mw
	LEFT JOIN UsersToMeetings um1 ON um1.DeletionManualWorkItemId = mw.Id AND um1.UserId = @UserId
	LEFT JOIN UsersToMeetings um2 ON um2.ManualWorkItemId = mw.Id AND um2.UserId = @UserId
	LEFT JOIN Meetings m ON um2.MeetingId = m.MeetingId
	WHERE
	 mw.[UserId] = @UserId
	 AND @MinStartDate <= mw.[StartDate] -- no usable index on EndDate
	 AND mw.[StartDate] < @EndDate
	 AND @StartDate < DATEADD(ms, 0, mw.[EndDate]) --don't use index on EndDate

	UNION ALL

	SELECT 0 AS [Id], [WorkId], [StartDate], [EndDate], 0 AS [ManualWorkItemType], NULL AS [SourceId], [Comment], NULL AS [Subject], NULL AS [Description], NULL AS [MeetingId], [Id] AS [PendingId], CASE WHEN [ManualWorkItemTypeId] = -1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS [IsPendingDeleteAlso]
	FROM RequestedManualWorkItems 
	WHERE 
	 [UserId] = @UserId -- no usable index on any columns
	 AND [StartDate] < @EndDate
	 AND @StartDate < [EndDate]
	 AND [IsModified] = 0 
	 AND [RequestedManualWorkItemStatusId] = 0

RETURN 0
 */


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


/*
CREATE PROCEDURE [dbo].[Client_GetWorkNames]
	@userId int,
	@workIds IntIdTableType READONLY
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	declare @rootId int, @companyId int
	SET @companyId = (SELECT [CompanyId] FROM [dbo].[User] WHERE Id = @userId)
	SET @rootId = (SELECT [Id] FROM [dbo].[Tasks] WHERE [CompanyId] = @companyId AND Type = 0)

	;WITH parents ([Id], [ParentId], [Type], [Name], [CategoryId], [RecursionLevel])
	AS (
		SELECT t.[Id], t.[ParentId], t.[Type], t.[Name], t.[CategoryId], 1
		FROM [dbo].[Tasks] t
		JOIN @workIds w ON w.[Id] = t.[Id]
		LEFT JOIN [dbo].[NodeTasks] nt ON nt.[UserId] = @userId AND nt.[TaskId] = t.Id
		WHERE t.[CompanyId] = @companyId AND (t.[Type] > 2 OR (t.[Type] = 2 AND nt.[UserId] IS NOT NULL))

		UNION ALL
		
		SELECT t.[Id], t.[ParentId], t.[Type], t.[Name], NULL AS CategoryId , p.[RecursionLevel] + 1
		FROM [dbo].[Tasks] t
		JOIN parents p ON p.[ParentId] = t.[Id]
		WHERE t.[CompanyId] = @companyId AND t.[Type] = 1
	)
	SELECT CASE WHEN [Type] = 1 THEN NULL ELSE [Id] END AS [Id], CASE WHEN [Type] = 1 THEN [Id] ELSE NULL END AS [ProjectId], CASE WHEN [ParentId] = @rootId THEN NULL ELSE [ParentId] END AS [ParentId], [Name], [CategoryId]
	FROM parents

RETURN 0
*/


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


/*
CREATE PROCEDURE [dbo].[Client_GetUserSettings]
	@userId int
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	SELECT ~CanModifyWorkTimeWithoutApproval AS [IsModificationApprovalNeeded], CASE WHEN ManualWorkItemEditAgeLimit = 0 THEN 24*60 ELSE ManualWorkItemEditAgeLimit END AS [ModificationAgeLimitInHours]
	FROM UserEffectiveSettings
	WHERE [UserId] = @UserId

RETURN 0
*/