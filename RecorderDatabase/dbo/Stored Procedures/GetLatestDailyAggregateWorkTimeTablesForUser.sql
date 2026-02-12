-- =============================================
-- There are several races here so be careful (more info in [UpdateDailyAggregateWorkTimeTables])
-- =============================================
CREATE PROCEDURE [dbo].[GetLatestDailyAggregateWorkTimeTablesForUser]
	@userId int,
	@oldVersion BINARY(8)
AS
	SET NOCOUNT ON

	IF NULLIF(object_id('tempdb..#firstresult'), 0) IS NOT NULL DROP TABLE #firstresult

	DECLARE @maxVersion BINARY(8) = (SELECT MIN_ACTIVE_ROWVERSION())

	SELECT t.[UserId]
		  ,t.[Day]
		  ,t.[TotalWorkTime]
		  ,t.[NetWorkTime]
		  ,t.[ComputerWorkTime]
		  ,t.[IvrWorkTime]
		  ,t.[MobileWorkTime]
		  ,t.[ManualWorkTime]
		  ,t.[HolidayTime]
		  ,t.[SickLeaveTime]
		  --,t.[IsValid]
		  ,t.[Version]
	  INTO #firstresult
	  FROM [dbo].[AggregateDailyWorkTimes] t
	 WHERE t.[UserId] = @userId
	   AND [Version] > @oldVersion
	   AND [Version] < @maxVersion
	   AND [IsValid] = 1

	SELECT * 
	  FROM #firstresult

	SELECT w.[UserId]
		  ,w.[Day]
		  ,w.[WorkId]
		  ,w.[TotalWorkTime]
	  FROM [dbo].[AggregateDailyWorkTimesByWorkId] w
	  JOIN #firstresult d ON d.[Day] = w.[Day]
	 WHERE w.[UserId] = @userId

RETURN 0