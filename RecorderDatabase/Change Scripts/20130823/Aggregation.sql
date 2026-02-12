EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
--this sproc should not be called directly, its only called by [UpdateHourlyAggregateWorkItems]
ALTER PROCEDURE [dbo].[MergeAggregateWorkItemIntervals]
	(
	@startDate datetime
	)
AS
	SET NOCOUNT ON

IF @startDate IS NULL RETURN

SET XACT_ABORT ON
BEGIN TRAN

declare
@lastEndDate datetime,
@lastId bigint,
@c_firstId bigint,
@c_secondId bigint,
@c_secondEndDate datetime

-- StartDate and PhaseId is unique in WorkItems so it should be unique in AggregateWorkItemIntervals too.
-- That means that we can only find at most one match for each interval to merge. (no dupes on join)
-- Without the PhaseId we could use row_number to only fetch the first match if there are more.
DECLARE merge_cursor CURSOR LOCAL STATIC FORWARD_ONLY FOR 
SELECT f.[Id]
      ,s.[Id]
      ,s.[EndDate]
  FROM [dbo].[AggregateWorkItemIntervals] f (TABLOCKX)
  JOIN [dbo].[AggregateWorkItemIntervals] s
		ON 	f.[EndDate] = s.[StartDate]
		AND f.[WorkId] = s.[WorkId]
		AND f.[UserId] = s.[UserId]
		AND f.[GroupId] = s.[GroupId]
		AND f.[CompanyId] = s.[CompanyId]
		AND f.[PhaseId] = s.[PhaseId]
		AND f.[ComputerId] = s.[ComputerId]
		AND f.[Id] <> s.[Id]
 WHERE 
        f.[EndDate] >= @startDate 
        AND s.[EndDate] >= @startDate
ORDER BY f.[PhaseId], f.[WorkId], f.[UserId], f.[GroupId], f.[CompanyId], f.[ComputerId], f.[EndDate] DESC
OPTION (RECOMPILE)

OPEN merge_cursor

WHILE 1=1
BEGIN
	FETCH NEXT FROM merge_cursor INTO 
		@c_firstId,
		@c_secondId,
		@c_secondEndDate

	IF @@FETCH_STATUS <> 0
		BREAK

	--delete the second interval
	DELETE FROM [dbo].[AggregateWorkItemIntervals] WHERE Id = @c_secondId
	IF @@rowcount = 0 CONTINUE --already deleted

	--extend the first interval
	IF @c_secondId = @lastId SET @c_secondEndDate = @lastEndDate
	UPDATE [dbo].[AggregateWorkItemIntervals] 
	   SET [EndDate] = @c_secondEndDate,
		   [UpdateDate] = GETUTCDATE()
	 WHERE Id = @c_firstId

	SET @lastEndDate = @c_secondEndDate
	SET @lastId = @c_firstId

END

CLOSE merge_cursor
DEALLOCATE merge_cursor
	
COMMIT TRAN	
	RETURN
' 

EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
--this sproc should not be called directly, its only called by [UpdateHourlyAggregateWorkItems]
ALTER PROCEDURE [dbo].[MergeAggregateIdleIntervals]
	(
	@startDate datetime
	)
AS
	SET NOCOUNT ON

IF @startDate IS NULL RETURN

SET XACT_ABORT ON
BEGIN TRAN

declare
@lastEndDate datetime,
@lastId bigint,
@c_firstId bigint,
@c_secondId bigint,
@c_secondEndDate datetime

-- StartDate and PhaseId is unique in WorkItems so it should be unique in AggregateIdleIntervals too.
-- That means that we can only find at most one match for each interval to merge. (no dupes on join)
-- Without the PhaseId we could use row_number to only fetch the first match if there are more.
DECLARE merge_cursor CURSOR LOCAL STATIC FORWARD_ONLY FOR 
SELECT f.[Id]
      ,s.[Id]
      ,s.[EndDate]
  FROM [dbo].[AggregateIdleIntervals] f (TABLOCKX)
  JOIN [dbo].[AggregateIdleIntervals] s
		ON 	f.[EndDate] = s.[StartDate]
		AND f.[WorkId] = s.[WorkId]
		AND f.[UserId] = s.[UserId]
		AND f.[GroupId] = s.[GroupId]
		AND f.[CompanyId] = s.[CompanyId]
		AND f.[PhaseId] = s.[PhaseId]
		AND f.[ComputerId] = s.[ComputerId]
 		AND f.[Id] <> s.[Id]
 WHERE 
        f.[EndDate] >= @startDate 
        AND s.[EndDate] >= @startDate
ORDER BY f.[PhaseId], f.[WorkId], f.[UserId], f.[GroupId], f.[CompanyId], f.[ComputerId], f.[EndDate] DESC
OPTION (RECOMPILE)

OPEN merge_cursor

WHILE 1=1
BEGIN
	FETCH NEXT FROM merge_cursor INTO 
		@c_firstId,
		@c_secondId,
		@c_secondEndDate

	IF @@FETCH_STATUS <> 0
		BREAK

	--delete the second interval
	DELETE FROM [dbo].[AggregateIdleIntervals] WHERE Id = @c_secondId
	IF @@rowcount = 0 CONTINUE --already deleted

	--extend the first interval
	IF @c_secondId = @lastId SET @c_secondEndDate = @lastEndDate
	UPDATE [dbo].[AggregateIdleIntervals] 
	   SET [EndDate] = @c_secondEndDate,
		   [UpdateDate] = GETUTCDATE()
	 WHERE Id = @c_firstId

	SET @lastEndDate = @c_secondEndDate
	SET @lastId = @c_firstId

END

CLOSE merge_cursor
DEALLOCATE merge_cursor
	
COMMIT TRAN	
	RETURN
' 

EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
--this sproc should not be called directly, its only called by [UpdateHourlyAggregateWorkItems]
ALTER PROCEDURE [dbo].[UpdateHourlyAggregateWorkItemsFromId]
	(
	@StartId bigint,
	@EndId bigint OUTPUT,
	@MinStartDate datetime = NULL OUTPUT
	)
AS
	SET NOCOUNT ON

SET XACT_ABORT ON
BEGIN TRAN
SET @EndId = @StartId

declare @StartIdChk bigint
SET @StartIdChk = (SELECT ISNULL(MAX(LastAggregatedId),0) FROM dbo.AggregateLastWorkItem)
IF @StartId <> @StartIdChk
BEGIN
	RAISERROR(''UpdateHourlyAggregateWorkItemsFromId called with wrong StartId'',16,1)
	ROLLBACK
	RETURN
END

declare @MaxEndId bigint
SET @MaxEndId = (SELECT ISNULL(MAX(Id),0) FROM [dbo].[WorkItems] WITH (TABLOCK)) -- I suppose TABLOCK is enough and no TABLOCKX needed
--using TABLOCKX (without HOLDLOCK) would cause to hold the lock until the end of the transaction.

declare
@c_Id bigint,
@c_WorkId int,
@c_PhaseId uniqueidentifier,
@c_StartDate datetime,
@c_EndDate datetime,
@c_UserId int,
@c_GroupId int,
@c_CompanyId int,
@c_ComputerId int,
@c_MouseActivity int,
@c_KeyboardActivity int

DECLARE interval_cursor CURSOR LOCAL FAST_FORWARD FOR 
SELECT [Id]
      ,[WorkId]
      ,[PhaseId]
      ,[StartDate]
      ,[EndDate]
      ,[UserId]
      ,[GroupId]
      ,[CompanyId]
      ,[ComputerId]
      ,[MouseActivity]
      ,[KeyboardActivity]
  FROM [dbo].[WorkItems] --(TABLOCKX) we don''t need locking if we have the @MaxEndId
 WHERE [Id] > @StartId
   AND [Id] <= @MaxEndId


OPEN interval_cursor

WHILE 1=1
BEGIN
	FETCH NEXT FROM interval_cursor INTO 
		@c_Id,
		@c_WorkId,
		@c_PhaseId,
		@c_StartDate,
		@c_EndDate,
		@c_UserId,
		@c_GroupId,
		@c_CompanyId,
		@c_ComputerId,
		@c_MouseActivity,
		@c_KeyboardActivity

	IF @@FETCH_STATUS <> 0
		BREAK

	IF (@EndId<@c_Id)  SET @EndId = @c_Id
	IF (@c_MouseActivity<0) SET @c_MouseActivity = 0
	IF (@c_KeyboardActivity<0) SET @c_KeyboardActivity = 0
	IF (@c_StartDate>=@c_EndDate) CONTINUE
	IF (@MinStartDate IS NULL OR @MinStartDate > @c_StartDate) SET @MinStartDate = @c_StartDate
	declare @interval_StartDate datetime, @interval_EndDate datetime, @Curr_StartDate datetime, @Curr_EndDate datetime
	declare @Rem_MouseActivity int, @Rem_KeyboardActivity int

	SET @interval_StartDate =  CONVERT(CHAR(13), @c_StartDate, 126) + '':00:00''
	SET @interval_EndDate = CONVERT(CHAR(13), @c_EndDate, 126) + '':00:00''
	SET @Curr_StartDate = @c_StartDate
	SET @Rem_MouseActivity = @c_MouseActivity
	SET @Rem_KeyboardActivity = @c_KeyboardActivity

	WHILE (@interval_StartDate<=@interval_EndDate)
	BEGIN
		declare @duration int, @Curr_MouseActivity int, @Curr_KeyboardActivity int
		IF (@interval_StartDate=@interval_EndDate) --last interval
		BEGIN
			SET @Curr_EndDate = @c_EndDate
		END
		ELSE
		BEGIN
			SET @Curr_EndDate = DATEADD(hour,1,@interval_StartDate)
		END
		SET @duration = DATEDIFF(SECOND, @Curr_StartDate, @Curr_EndDate)*1000 + DATEPART(MILLISECOND, @Curr_EndDate) - DATEPART(MILLISECOND, @Curr_StartDate)
		--SET @duration = DATEDIFF(millisecond, @Curr_StartDate, @Curr_EndDate) -- not accurate enough
		IF (@duration<=0) BREAK
		IF (@Curr_EndDate = @c_EndDate) --last interval
		BEGIN
			SET @Curr_MouseActivity = @Rem_MouseActivity
			SET @Curr_KeyboardActivity = @Rem_KeyboardActivity
		END
		ELSE
		BEGIN
			declare @wholeDuration int
			SET @wholeDuration = DATEDIFF(SECOND, @c_StartDate, @c_EndDate)*1000 + DATEPART(MILLISECOND, @c_EndDate) - DATEPART(MILLISECOND, @c_StartDate)
			SET @Curr_MouseActivity = CAST(ROUND(CAST(@duration AS float) / @wholeDuration * @c_MouseActivity, 0) AS int)
			SET @Curr_KeyboardActivity = CAST(ROUND(CAST(@duration AS float) / @wholeDuration * @c_KeyboardActivity, 0) AS int)

			SET @Rem_MouseActivity = @Rem_MouseActivity - @Curr_MouseActivity
			SET @Rem_KeyboardActivity = @Rem_KeyboardActivity - @Curr_KeyboardActivity
		END

		IF EXISTS(SELECT NULL FROM [dbo].[AggregateWorkItems] 
								WHERE StartDate = @interval_StartDate --enddate should match
								  AND WorkId = @c_WorkId
								  AND UserId = @c_UserId
								  AND GroupId = @c_GroupId
								  AND CompanyId = @c_CompanyId
								  AND ComputerId = @c_ComputerId)
		BEGIN
			UPDATE [dbo].[AggregateWorkItems] SET
				   [WorkTime] = [WorkTime] + @duration,
				   [MouseActivity] = [MouseActivity] + @Curr_MouseActivity,
				   [KeyboardActivity] = [KeyboardActivity] + @Curr_KeyboardActivity,
				   [UpdateDate] = GETUTCDATE()
			 WHERE StartDate = @interval_StartDate --enddate should match
			   AND WorkId = @c_WorkId
			   AND UserId = @c_UserId
			   AND GroupId = @c_GroupId
			   AND CompanyId = @c_CompanyId
			   AND ComputerId = @c_ComputerId
		END
		ELSE
		BEGIN
			INSERT INTO [dbo].[AggregateWorkItems]
					   ([StartDate]
					   ,[EndDate]
					   ,[WorkTime]
					   ,[WorkId]
					   ,[UserId]
					   ,[GroupId]
					   ,[CompanyId]
					   ,[ComputerId]
					   ,[MouseActivity]
					   ,[KeyboardActivity])
				 VALUES
					   (@interval_StartDate
					   ,DATEADD(hour,1,@interval_StartDate)
					   ,@duration
					   ,@c_WorkId
					   ,@c_UserId
					   ,@c_GroupId
					   ,@c_CompanyId
					   ,@c_ComputerId
					   ,@Curr_MouseActivity
					   ,@Curr_KeyboardActivity)
		END

		SET @interval_StartDate = DATEADD(hour,1,@interval_StartDate)
		SET @Curr_StartDate = @interval_StartDate
	END
	
	--BEGIN update AggregateWorkItemIntervals
	UPDATE TOP (1) [dbo].[AggregateWorkItemIntervals] 
	   SET [EndDate] = @c_EndDate,
		   [UpdateDate] = GETUTCDATE()
	 WHERE EndDate = @c_StartDate --enddate should match
	   AND PhaseId = @c_PhaseId
	   AND WorkId = @c_WorkId
	   AND UserId = @c_UserId
	   AND GroupId = @c_GroupId
	   AND CompanyId = @c_CompanyId
	   AND ComputerId = @c_ComputerId
	   
	IF @@rowcount = 0
	BEGIN
		INSERT INTO [dbo].[AggregateWorkItemIntervals]
				   ([WorkId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[UserId]
				   ,[GroupId]
				   ,[CompanyId]
				   ,[PhaseId]
				   ,[ComputerId]
				   ,[CreateDate]
				   ,[UpdateDate])
			 VALUES
				   (@c_WorkId
				   ,@c_StartDate
				   ,@c_EndDate
				   ,@c_UserId
				   ,@c_GroupId
				   ,@c_CompanyId
				   ,@c_PhaseId
				   ,@c_ComputerId
				   ,GETUTCDATE()
				   ,GETUTCDATE())
	END	
	--END update AggregateWorkItemIntervals

	--BEGIN update AggregateIdleIntervals
	IF @c_MouseActivity = 0 AND @c_KeyboardActivity = 0
	BEGIN
		UPDATE TOP (1) [dbo].[AggregateIdleIntervals] 
		   SET [EndDate] = @c_EndDate,
			   [UpdateDate] = GETUTCDATE()
		 WHERE EndDate = @c_StartDate --enddate should match
		   AND PhaseId = @c_PhaseId
		   AND WorkId = @c_WorkId
		   AND UserId = @c_UserId
		   AND GroupId = @c_GroupId
		   AND CompanyId = @c_CompanyId
		   AND ComputerId = @c_ComputerId
	   
		IF @@rowcount = 0
		BEGIN
			INSERT INTO [dbo].[AggregateIdleIntervals]
					   ([WorkId]
					   ,[StartDate]
					   ,[EndDate]
					   ,[UserId]
					   ,[GroupId]
					   ,[CompanyId]
					   ,[PhaseId]
					   ,[ComputerId]
					   ,[CreateDate]
					   ,[UpdateDate])
				 VALUES
					   (@c_WorkId
					   ,@c_StartDate
					   ,@c_EndDate
					   ,@c_UserId
					   ,@c_GroupId
					   ,@c_CompanyId
					   ,@c_PhaseId
					   ,@c_ComputerId
					   ,GETUTCDATE()
					   ,GETUTCDATE())
		END
	END
	--END update AggregateIdleIntervals
END
CLOSE interval_cursor
DEALLOCATE interval_cursor

COMMIT TRAN	
	
	RETURN
' 