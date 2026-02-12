--this sproc should not be called directly, its only called by [UpdateHourlyAggregateWorkItems]
CREATE PROCEDURE [dbo].[UpdateHourlyAggregateWorkItemsFromId]
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
	RAISERROR('UpdateHourlyAggregateWorkItemsFromId called with wrong StartId',16,1)
	ROLLBACK
	RETURN
END

SET @StartId = (SELECT TOP 1 Id - 1 FROM [dbo].[WorkItems] WHERE Id > @StartId ORDER BY Id) -- set StartId to before next record

declare
@c_Id bigint = -1,
@c_WorkId int,
@c_PhaseId uniqueidentifier,
@c_StartDate datetime,
@c_EndDate datetime,
@c_UserId int,
@c_GroupId int,
@c_CompanyId int,
@c_ComputerId int,
@c_IsRemoteDesktop bit,
@c_IsVirtualMachine bit,
@c_MouseActivity int,
@c_KeyboardActivity int,
@is_cut_needed bit,
@is_afterLast bit,
@i_StartDate datetime = null,
@i_EndDate datetime

declare
@f_Id bigint,
@f_WorkId int,
@f_PhaseId uniqueidentifier,
@f_StartDate datetime,
@f_EndDate datetime,
@f_UserId int,
@f_GroupId int,
@f_CompanyId int,
@f_ComputerId int,
@f_IsRemoteDesktop bit,
@f_IsVirtualMachine bit,
@f_MouseActivity int,
@f_KeyboardActivity int,
@pi_StartDate datetime,
@pi_Id bigint

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
	  ,[IsRemoteDesktop]
	  ,[IsVirtualMachine]
      ,[MouseActivity]
      ,[KeyboardActivity]
  FROM [dbo].[WorkItems] --(TABLOCKX) we don't need locking if we have the @MaxEndId
  WITH (INDEX (PK_Workitems))
 WHERE [Id] > @StartId
   AND [Id] <= @StartId + 100
 ORDER BY UserId, ComputerId, PhaseId, WorkId, StartDate

OPEN interval_cursor

WHILE @is_afterLast IS NULL
BEGIN
	FETCH NEXT FROM interval_cursor INTO 
		@f_Id,
		@f_WorkId,
		@f_PhaseId,
		@f_StartDate,
		@f_EndDate,
		@f_UserId,
		@f_GroupId,
		@f_CompanyId,
		@f_ComputerId,
		@f_IsRemoteDesktop,
		@f_IsVirtualMachine,
		@f_MouseActivity,
		@f_KeyboardActivity

	IF @@FETCH_STATUS <> 0
		SET @is_afterLast = 1

	IF (@EndId<@f_Id) SET @EndId = @f_Id

	IF (@f_StartDate>=@f_EndDate AND @is_afterLast IS NULL) CONTINUE

	IF (@c_id = -1) -- first row
	BEGIN
		SET @c_Id				= @f_Id
		SET @c_WorkId			= @f_WorkId
		SET @c_PhaseId			= @f_PhaseId
		SET @c_StartDate		= @f_StartDate
		SET @c_EndDate			= @f_EndDate
		SET @c_UserId			= @f_UserId
		SET @c_GroupId			= @f_GroupId
		SET @c_CompanyId		= @f_CompanyId
		SET @c_ComputerId		= @f_ComputerId
		SET @c_IsRemoteDesktop	= @f_IsRemoteDesktop
		SET @c_IsVirtualMachine	= @f_IsVirtualMachine
		SET @c_MouseActivity	= @f_MouseActivity
		SET @c_KeyboardActivity	= @f_KeyboardActivity
		SET @is_cut_needed = 0
	END
	ELSE
	IF (@c_UserId = @f_UserId
		AND @c_ComputerId = @f_ComputerId
		AND @c_GroupId = @f_GroupId
		AND @c_CompanyId = @f_CompanyId
		AND @c_PhaseId = @f_PhaseId
		AND @c_WorkId = @f_WorkId
		AND @c_IsRemoteDesktop = @f_IsRemoteDesktop
		AND @c_IsVirtualMachine = @f_IsVirtualMachine
		--AND (@c_MouseActivity + @c_KeyboardActivity = 0 AND @f_MouseActivity + @f_KeyboardActivity = 0 OR @c_MouseActivity + @c_KeyboardActivity <> 0 AND @f_MouseActivity + @f_KeyboardActivity <> 0)
		AND @c_EndDate = @f_StartDate) -- connected workitem
		AND @is_afterLast IS NULL
		SET @is_cut_needed = 0
	ELSE
		SET @is_cut_needed = 1

	IF @is_cut_needed = 1
	BEGIN
		IF (@MinStartDate IS NULL OR @MinStartDate > @c_StartDate) SET @MinStartDate = @c_StartDate

		UPDATE TOP (1) [dbo].[AggregateWorkItemIntervals] 
		   SET [EndDate] = @c_EndDate,
			   [UpdateDate] = GETUTCDATE(),
			   @pi_StartDate = StartDate,
			   @pi_Id = Id
		 WHERE EndDate = @c_StartDate --enddate should match
		   AND PhaseId = @c_PhaseId
		   AND WorkId = @c_WorkId
		   AND UserId = @c_UserId
		   AND GroupId = @c_GroupId
		   AND CompanyId = @c_CompanyId
		   AND ComputerId = @c_ComputerId
		   AND IsRemoteDesktop = @c_IsRemoteDesktop
		   AND IsVirtualMachine = @c_IsVirtualMachine

		IF @@rowcount = 0
		BEGIN
			UPDATE TOP (1) [dbo].[AggregateWorkItemIntervals] 
			   SET [StartDate] = @c_StartDate,
				   [UpdateDate] = GETUTCDATE()
			 WHERE StartDate = @c_EndDate --startdate should match
			   AND PhaseId = @c_PhaseId
			   AND WorkId = @c_WorkId
			   AND UserId = @c_UserId
			   AND GroupId = @c_GroupId
			   AND CompanyId = @c_CompanyId
			   AND ComputerId = @c_ComputerId
			   AND IsRemoteDesktop = @c_IsRemoteDesktop
			   AND IsVirtualMachine = @c_IsVirtualMachine

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
					   ,[IsRemoteDesktop]
					   ,[IsVirtualMachine]
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
					   ,@c_IsRemoteDesktop
					   ,@c_IsVirtualMachine
					   ,GETUTCDATE()
					   ,GETUTCDATE())

			END
		END
		ELSE
		BEGIN
			UPDATE TOP (1) [dbo].[AggregateWorkItemIntervals] 
			   SET [StartDate] = @pi_StartDate,
				   [UpdateDate] = GETUTCDATE()
			 WHERE StartDate = @c_EndDate --startdate should match
			   AND PhaseId = @c_PhaseId
			   AND WorkId = @c_WorkId
			   AND UserId = @c_UserId
			   AND GroupId = @c_GroupId
			   AND CompanyId = @c_CompanyId
			   AND ComputerId = @c_ComputerId
			   AND IsRemoteDesktop = @c_IsRemoteDesktop
			   AND IsVirtualMachine = @c_IsVirtualMachine
			
			IF @@rowcount > 0
			BEGIN
				DELETE FROM [dbo].[AggregateWorkItemIntervals]
					WHERE Id = @pi_Id
			END
		END
		--END update AggregateWorkItemIntervals
	END

	--BEGIN update AggregateIdleIntervals	
	IF (@f_MouseActivity <> 0 OR @f_KeyboardActivity <> 0 OR @is_cut_needed = 1) AND  @i_StartDate IS NOT NULL 
	BEGIN
		UPDATE TOP (1) [dbo].[AggregateIdleIntervals] 
		   SET [EndDate] = @i_EndDate,
			   [UpdateDate] = GETUTCDATE(),
			   @pi_StartDate = StartDate,
			   @pi_Id = Id
		 WHERE EndDate = @i_StartDate --enddate should match
		   AND PhaseId = @c_PhaseId
		   AND WorkId = @c_WorkId
		   AND UserId = @c_UserId
		   AND GroupId = @c_GroupId
		   AND CompanyId = @c_CompanyId
		   AND ComputerId = @c_ComputerId
	       AND IsRemoteDesktop = @c_IsRemoteDesktop
		   AND IsVirtualMachine = @c_IsVirtualMachine

		IF @@rowcount = 0
		BEGIN
			UPDATE TOP (1) [dbo].[AggregateIdleIntervals] 
			   SET [StartDate] = @c_StartDate,
				   [UpdateDate] = GETUTCDATE()
			 WHERE StartDate = @c_EndDate --startdate should match
			   AND PhaseId = @c_PhaseId
			   AND WorkId = @c_WorkId
			   AND UserId = @c_UserId
			   AND GroupId = @c_GroupId
			   AND CompanyId = @c_CompanyId
			   AND ComputerId = @c_ComputerId
			   AND IsRemoteDesktop = @c_IsRemoteDesktop
			   AND IsVirtualMachine = @c_IsVirtualMachine

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
						   ,[IsRemoteDesktop]
						   ,[IsVirtualMachine]
						   ,[CreateDate]
						   ,[UpdateDate])
					 VALUES
						   (@c_WorkId
						   ,@i_StartDate
						   ,@i_EndDate
						   ,@c_UserId
						   ,@c_GroupId
						   ,@c_CompanyId
						   ,@c_PhaseId
						   ,@c_ComputerId
						   ,@c_IsRemoteDesktop
						   ,@c_IsVirtualMachine
						   ,GETUTCDATE()
						   ,GETUTCDATE())
			END
		END
		ELSE
		BEGIN
			UPDATE TOP (1) [dbo].[AggregateIdleIntervals] 
			   SET [StartDate] = @pi_StartDate,
				   [UpdateDate] = GETUTCDATE()
			 WHERE StartDate = @c_EndDate --startdate should match
			   AND PhaseId = @c_PhaseId
			   AND WorkId = @c_WorkId
			   AND UserId = @c_UserId
			   AND GroupId = @c_GroupId
			   AND CompanyId = @c_CompanyId
			   AND ComputerId = @c_ComputerId
			   AND IsRemoteDesktop = @c_IsRemoteDesktop
			   AND IsVirtualMachine = @c_IsVirtualMachine
			
			IF @@rowcount > 0
			BEGIN
				DELETE FROM [dbo].[AggregateIdleIntervals]
					WHERE Id = @pi_Id
			END
		END

		SET @i_StartDate = NULL
	END

	IF @f_MouseActivity = 0 AND @f_KeyboardActivity = 0
	BEGIN
		IF @i_StartDate IS NULL SET @i_StartDate = @f_StartDate
		SET @i_EndDate = @f_EndDate
	END
	--END update AggregateIdleIntervals

	IF @is_cut_needed = 1
	BEGIN
		SET @c_Id				= @f_Id
		SET @c_WorkId			= @f_WorkId
		SET @c_PhaseId			= @f_PhaseId
		SET @c_StartDate		= @f_StartDate
		SET @c_EndDate			= @f_EndDate
		SET @c_UserId			= @f_UserId
		SET @c_GroupId			= @f_GroupId
		SET @c_CompanyId		= @f_CompanyId
		SET @c_ComputerId		= @f_ComputerId
		SET @c_IsRemoteDesktop	= @f_IsRemoteDesktop
		SET @c_IsVirtualMachine	= @f_IsVirtualMachine
		SET @c_MouseActivity	= @f_MouseActivity
		SET @c_KeyboardActivity	= @f_KeyboardActivity
	END
	ELSE
	BEGIN
		SET @c_EndDate = @f_EndDate
		SET @c_KeyboardActivity = @c_KeyboardActivity + @f_KeyboardActivity
		SET @c_MouseActivity = @c_MouseActivity + @f_MouseActivity
	END
END
CLOSE interval_cursor
DEALLOCATE interval_cursor

COMMIT TRAN	
	
	RETURN
