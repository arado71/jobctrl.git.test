-- =============================================
-- We could use ISOLATION LEVEL SERIALIZABLE and do the operations on the two tables in the same order in one transaction 
-- i.e. Update [AggregateDailyWorkTimes] and then [AggregateDailyWorkTimesByWorkId], also when reading, first we select 
-- from [AggregateDailyWorkTimes] and then [AggregateDailyWorkTimesByWorkId] also in one SERIALIZABLE transaction.
-- But if we tolerate some race conditions we could avoid transactions and Update [AggregateDailyWorkTimesByWorkId] and
-- then [AggregateDailyWorkTimes], and read from [AggregateDailyWorkTimes] and then [AggregateDailyWorkTimesByWorkId].
-- So the version is updated as the last operation and is read as the first one.
-- In this case we could have the new data from [AggregateDailyWorkTimesByWorkId] with old data from [AggregateDailyWorkTimes]
-- including the version, but eventually we will have to get the latest version with consistent data.
-- =============================================
CREATE PROCEDURE [dbo].[UpdateDailyAggregateWorkTimeTables]
	@userId int,
	@day date,
	@netWorkTime int,
	@computerWorkTime int,
	@ivrWorkTime int,
	@mobileWorkTime int,
	@manualWorkTime int,
	@holidayTime int,
	@sickLeaveTime int,
	@oldVersion rowversion,
	@workTimesById WorkTimesById READONLY
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	;
	WITH a AS
	(
		SELECT *
		  FROM [dbo].[AggregateDailyWorkTimesByWorkId] WITH (HOLDLOCK)
		 WHERE [UserId] = @userId AND [Day] = @day
	)
	MERGE INTO a
	USING (
			SELECT [WorkId], [WorkTime]
			  FROM @workTimesById
			) AS i
		ON i.[WorkId] = a.[WorkId]
		WHEN NOT MATCHED THEN
			INSERT ([UserId], [Day], [WorkId], [TotalWorkTime]) VALUES (@userId, @day, i.[WorkId], i.[WorkTime])
		WHEN MATCHED THEN
			UPDATE SET a.[TotalWorkTime] = i.[WorkTime]
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
	;

	WITH a AS
	(
		SELECT *
		  FROM [dbo].[AggregateDailyWorkTimes] WITH (HOLDLOCK)
		 WHERE [UserId] = @userId AND [Day] = @day
	)
	MERGE INTO a
	USING (
			SELECT @userId AS [UserId], @day AS [Day]
			) AS i
		ON  i.[UserId] = a.[UserId] AND i.[Day] = a.[Day]
		WHEN NOT MATCHED THEN
			INSERT
			   ([UserId]
			   ,[Day]
			   ,[NetWorkTime]
			   ,[ComputerWorkTime]
			   ,[IvrWorkTime]
			   ,[MobileWorkTime]
			   ,[ManualWorkTime]
			   ,[HolidayTime]
			   ,[SickLeaveTime]
			   ,[IsValid])
			VALUES
			   (@userId
			   ,@day
			   ,@netWorkTime
			   ,@computerWorkTime
			   ,@ivrWorkTime
			   ,@mobileWorkTime
			   ,@manualWorkTime
			   ,@holidayTime
			   ,@sickLeaveTime
			   ,1)
		WHEN MATCHED THEN
			UPDATE SET [IsValid] = CASE WHEN a.Version = @oldVersion THEN 1 ELSE 0 END
			   ,[NetWorkTime] = @netWorkTime
			   ,[ComputerWorkTime] = @computerWorkTime
			   ,[IvrWorkTime] = @ivrWorkTime
			   ,[MobileWorkTime] = @mobileWorkTime
			   ,[ManualWorkTime] = @manualWorkTime
			   ,[HolidayTime] = @holidayTime
			   ,[SickLeaveTime] = @sickLeaveTime
	;

RETURN 0