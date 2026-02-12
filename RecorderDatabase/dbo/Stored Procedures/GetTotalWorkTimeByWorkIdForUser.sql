CREATE PROCEDURE [dbo].[GetTotalWorkTimeByWorkIdForUser]
	(
	@userId int,
	@startDate datetime = NULL,
	@endDate datetime = NULL
	)
AS
	SET NOCOUNT ON
	
	IF NULLIF(object_id('tempdb..#result'), 0) IS NOT NULL DROP TABLE #result
	IF NULLIF(object_id('tempdb..#workTimeTable'), 0) IS NOT NULL DROP TABLE #workTimeTable
		
	create table #result (
	WorkId int NOT NULL PRIMARY KEY,
	TotalWorkTime bigint NOT NULL default (0),
	ComputerWorkTime bigint NOT NULL default (0),
	ComputerCorrectionTime bigint NOT NULL default (0),
	IvrWorkTime bigint NOT NULL default (0),
	IvrCorrectionTime bigint NOT NULL default (0),
	MobileWorkTime bigint NOT NULL default (0),
	MobileCorrectionTime bigint NOT NULL default (0),
	ManualWorkTime bigint NOT NULL default (0),
	HolidayTime bigint NOT NULL default (0),
	SickLeaveTime bigint NOT NULL default (0)
	)
	
	create table #workTimeTable (
	WorkId int NOT NULL PRIMARY KEY,
	WorkTime bigint NOT NULL default (0)
	)
	
	declare @c_WorkId int, @c_StartDate datetime, @c_EndDate datetime, @currStartDate datetime, @currEndDate datetime, @prevEndDate datetime
	
	IF @startDate IS NULL SET @startDate = '1900-01-01'
	IF @endDate IS NULL SET @endDate = '3000-01-01'
	
	-------------------------------------------------------------------------------------------------------
	--ComputerWorkTime
	-------------------------------------------------------------------------------------------------------
	--Assumes that AggregateWorkItemIntervals is up to date
	INSERT INTO #result (WorkId, ComputerWorkTime) EXEC dbo.GetWorkTimeByWorkIdForUser @userId=@userId, @startDate=@startDate, @endDate=@endDate

	-------------------------------------------------------------------------------------------------------
	--IvrWorkTime
	-------------------------------------------------------------------------------------------------------
	INSERT #workTimeTable EXEC dbo.GetIvrWorkTimeByWorkIdForUser @userId=@userId, @startDate=@startDate, @endDate=@endDate
			
	--BEGIN merge of IvrWorkTime
	UPDATE #result SET IvrWorkTime = IvrWorkTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, IvrWorkTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge

	-------------------------------------------------------------------------------------------------------
	--MobileWorkTime
	-------------------------------------------------------------------------------------------------------
	INSERT #workTimeTable EXEC dbo.GetMobileWorkTimeByWorkIdForUser @userId=@userId, @startDate=@startDate, @endDate=@endDate
			
	--BEGIN merge of MobileWorkTime
	UPDATE #result SET MobileWorkTime = MobileWorkTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, MobileWorkTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge

	-------------------------------------------------------------------------------------------------------
	--ManualWorkTime
	-------------------------------------------------------------------------------------------------------
	INSERT INTO #workTimeTable (WorkId, WorkTime)
	SELECT
		WorkId,
		SUM(dbo.GetIntersectDuration(@startDate, @endDate, StartDate, EndDate)) AS WorkTime
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND ManualWorkItemTypeId = 0 -- Manually added work time
	GROUP BY
		WorkId
			
	--BEGIN merge of ManualWorkTime
	UPDATE #result SET ManualWorkTime = ManualWorkTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, ManualWorkTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge

	-------------------------------------------------------------------------------------------------------
	--HolidayTime
	-------------------------------------------------------------------------------------------------------
	INSERT INTO #workTimeTable (WorkId, WorkTime)
	SELECT
		WorkId,
		SUM(dbo.GetIntersectDuration(@startDate, @endDate, StartDate, EndDate)) AS WorkTime
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND ManualWorkItemTypeId = 4 -- Holiday time
	GROUP BY
		WorkId
			
	--BEGIN merge of HolidayTime
	UPDATE #result SET HolidayTime = HolidayTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, HolidayTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge
	
	-------------------------------------------------------------------------------------------------------
	--SickLeaveTime
	-------------------------------------------------------------------------------------------------------
	INSERT INTO #workTimeTable (WorkId, WorkTime)
	SELECT
		WorkId,
		SUM(dbo.GetIntersectDuration(@startDate, @endDate, StartDate, EndDate)) AS WorkTime
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND ManualWorkItemTypeId = 5 -- Sick leave time
	GROUP BY
		WorkId
			
	--BEGIN merge of SickLeaveTime
	UPDATE #result SET SickLeaveTime = SickLeaveTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, SickLeaveTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge
			
	-------------------------------------------------------------------------------------------------------
	--ComputerCorrectionTime
	-------------------------------------------------------------------------------------------------------
	--get disjoint correction intervals, so a workitem will only be calculated once
	SET @prevEndDate = NULL
	
	DECLARE manualworkitem_cursor CURSOR FAST_FORWARD FOR 
	SELECT
		WorkId, StartDate, EndDate
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND (ManualWorkItemTypeId = 1 OR  ManualWorkItemTypeId = 3) -- Deleted Interval or Deleted Computer Interval
	ORDER BY StartDate
	
	OPEN manualworkitem_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM manualworkitem_cursor INTO 
			@c_WorkId,
			@c_StartDate,
			@c_EndDate

		IF @@FETCH_STATUS <> 0
			BREAK
		
		SET @currStartDate = @c_StartDate
		SET @currEndDate = @c_EndDate
			
		IF (@prevEndDate IS NOT NULL AND @currStartDate < @prevEndDate ) SET @currStartDate = @prevEndDate
		IF (@endDate < @currEndDate) SET @currEndDate = @endDate
		IF (@currEndDate < @currStartDate) CONTINUE
		
		
		INSERT #workTimeTable EXEC dbo.GetWorkTimeByWorkIdForUser @userId=@userId, @startDate=@currStartDate, @endDate=@currEndDate
		
		--BEGIN merge of ComputerCorrectionTime
		UPDATE #result SET ComputerCorrectionTime = ComputerCorrectionTime - w.WorkTime
		FROM #workTimeTable w
		JOIN #result r ON w.WorkId = r.WorkId
		
		INSERT INTO #result (WorkId, ComputerCorrectionTime) --this should not happen (when WorkTime is not 0)...
		SELECT WorkId, -1 * WorkTime
		FROM #workTimeTable w
		WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
		
		DELETE FROM #workTimeTable
		--END merge
	
		SET @prevEndDate = @currEndDate
	END
	
	CLOSE manualworkitem_cursor
	DEALLOCATE manualworkitem_cursor

	-------------------------------------------------------------------------------------------------------
	--IvrCorrectionTime
	-------------------------------------------------------------------------------------------------------
	--get disjoint correction intervals, so an ivrworkitem will only be calculated once
	SET @prevEndDate = NULL
	
	DECLARE manualworkitem_cursor CURSOR FAST_FORWARD FOR 
	SELECT
		WorkId, StartDate, EndDate
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND (ManualWorkItemTypeId = 1 OR  ManualWorkItemTypeId = 2) -- Deleted Interval or Deleted Ivr Interval
	ORDER BY StartDate
	
	OPEN manualworkitem_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM manualworkitem_cursor INTO 
			@c_WorkId,
			@c_StartDate,
			@c_EndDate

		IF @@FETCH_STATUS <> 0
			BREAK
		
		SET @currStartDate = @c_StartDate
		SET @currEndDate = @c_EndDate
			
		IF (@prevEndDate IS NOT NULL AND @currStartDate < @prevEndDate ) SET @currStartDate = @prevEndDate
		IF (@endDate < @currEndDate) SET @currEndDate = @endDate
		IF (@currEndDate < @currStartDate) CONTINUE
		
		
		INSERT #workTimeTable EXEC dbo.GetIvrWorkTimeByWorkIdForUser @userId=@userId, @startDate=@currStartDate, @endDate=@currEndDate
		
		--BEGIN merge of IvrCorrectionTime
		UPDATE #result SET IvrCorrectionTime = IvrCorrectionTime - w.WorkTime
		FROM #workTimeTable w
		JOIN #result r ON w.WorkId = r.WorkId
		
		INSERT INTO #result (WorkId, IvrCorrectionTime) --this should not happen (when WorkTime is not 0)...
		SELECT WorkId, -1 * WorkTime
		FROM #workTimeTable w
		WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
		
		DELETE FROM #workTimeTable
		--END merge
	
		SET @prevEndDate = @currEndDate
	END
	
	CLOSE manualworkitem_cursor
	DEALLOCATE manualworkitem_cursor

	-------------------------------------------------------------------------------------------------------
	--MobileCorrectionTime
	-------------------------------------------------------------------------------------------------------
	--get disjoint correction intervals, so an mobileworkitem will only be calculated once
	SET @prevEndDate = NULL
	
	DECLARE manualworkitem_cursor CURSOR FAST_FORWARD FOR 
	SELECT
		WorkId, StartDate, EndDate
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND (ManualWorkItemTypeId = 1 OR  ManualWorkItemTypeId = 6) -- Deleted Interval or Deleted Mobile Interval
	ORDER BY StartDate
	
	OPEN manualworkitem_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM manualworkitem_cursor INTO 
			@c_WorkId,
			@c_StartDate,
			@c_EndDate

		IF @@FETCH_STATUS <> 0
			BREAK
		
		SET @currStartDate = @c_StartDate
		SET @currEndDate = @c_EndDate
			
		IF (@prevEndDate IS NOT NULL AND @currStartDate < @prevEndDate ) SET @currStartDate = @prevEndDate
		IF (@endDate < @currEndDate) SET @currEndDate = @endDate
		IF (@currEndDate < @currStartDate) CONTINUE
		
		
		INSERT #workTimeTable EXEC dbo.GetMobileWorkTimeByWorkIdForUser @userId=@userId, @startDate=@currStartDate, @endDate=@currEndDate
		
		--BEGIN merge of IvrCorrectionTime
		UPDATE #result SET MobileCorrectionTime = MobileCorrectionTime - w.WorkTime
		FROM #workTimeTable w
		JOIN #result r ON w.WorkId = r.WorkId
		
		INSERT INTO #result (WorkId, MobileCorrectionTime) --this should not happen (when WorkTime is not 0)...
		SELECT WorkId, -1 * WorkTime
		FROM #workTimeTable w
		WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
		
		DELETE FROM #workTimeTable
		--END merge
	
		SET @prevEndDate = @currEndDate
	END
	
	CLOSE manualworkitem_cursor
	DEALLOCATE manualworkitem_cursor

	-------------------------------------------------------------------------------------------------------
	--TotalWorkTime
	-------------------------------------------------------------------------------------------------------
	UPDATE #result SET TotalWorkTime = ComputerWorkTime 
									+ ComputerCorrectionTime 
									+ IvrWorkTime 
									+ IvrCorrectionTime 
									+ MobileWorkTime
									+ MobileCorrectionTime
									+ ManualWorkTime
									+ HolidayTime
									+ SickLeaveTime
	
	-------------------------------------------------------------------------------------------------------
	SELECT * FROM #result
	-------------------------------------------------------------------------------------------------------
	RETURN
