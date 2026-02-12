USE [JobControl]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Client_GetUserSettings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Client_GetUserSettings]
GO

/****** Object:  StoredProcedure [dbo].[Client_GetUserSettings]    Script Date: 2015.03.30. 10:08:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Client_GetUserSettings]
	@userId int
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	SELECT ~CanModifyWorkTimeWithoutApproval AS [IsModificationApprovalNeeded], ManualWorkItemEditAgeLimit AS [ModificationAgeLimitInHours]
	FROM UserEffectiveSettings
	WHERE [UserId] = @UserId

RETURN 0
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Client_GetWorkTimeHistory]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Client_GetWorkTimeHistory]
GO

/****** Object:  StoredProcedure [dbo].[Client_GetWorkTimeHistory]    Script Date: 2015.03.24. 10:36:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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


	SELECT mw.[Id], ISNULL(mw.[WorkId],0) AS [WorkId], mw.[StartDate], mw.[EndDate], mw.[ManualWorkItemTypeId] as [ManualWorkItemType], mw.[SourceId], mw.[Comment], m.[Title] AS [Subject], m.[Description], m.[MeetingId], NULL AS [PendingId], CAST(0 AS bit) AS [IsPendingDeleteAlso]
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
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Client_GetWorkNames]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Client_GetWorkNames]
GO


/****** Object:  StoredProcedure [dbo].[Client_GetWorkNames]    Script Date: 2015.03.24. 10:36:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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
GO
