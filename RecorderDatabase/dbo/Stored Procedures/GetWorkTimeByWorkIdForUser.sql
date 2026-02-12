CREATE PROCEDURE [dbo].[GetWorkTimeByWorkIdForUser]
	(
	@userId int,
	@startDate datetime,
	@endDate datetime,
	@workId int = NULL
	)
AS
	SET NOCOUNT ON
	
	IF @userId IS NULL OR @startDate IS NULL OR @endDate IS NULL
	BEGIN
		RAISERROR('@userId, @startDate and @endDate cannot be NULL', 16, 1)
		RETURN
	END
	
	declare @result table (
	WorkId int NOT NULL,
	WorkTime bigint NOT NULL
	)
	
	
	INSERT INTO
		@result
	SELECT 
		WorkId, 
		SUM(dbo.GetIntersectDuration(@startDate, @endDate, StartDate, EndDate)) AS WorkTime
	FROM 
		AggregateWorkItemIntervals 
	WHERE 
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -7, @startDate)
	AND EndDate > @startDate
	AND WorkId = ISNULL(@workId, WorkId)
	GROUP BY WorkId
		
	SELECT * FROM @result

	RETURN
	
	
/* without the function...
	SELECT
		items.WorkId AS WorkId,
		SUM(
		CASE
			WHEN items.StartDate < items.EndDate THEN CAST(DATEDIFF(SECOND, items.StartDate, items.EndDate) AS bigint) * 1000 + DATEPART(MILLISECOND, items.EndDate) - DATEPART(MILLISECOND, items.StartDate)
			ELSE 0
		END
		) AS WorkTime
	FROM
		(SELECT 
			WorkId,
			CASE
				WHEN StartDate < @startDate THEN @startDate
				ELSE StartDate
			END AS StartDate,
			CASE
				WHEN EndDate < @endDate THEN EndDate
				ELSE @endDate
			END AS EndDate
		FROM 
			WorkItems 
		WHERE 
			UserId = @userId
		AND StartDate > DATEADD(minute, -5, @startDate) --hax because there is no index on EndDate
		AND StartDate < @endDate
		AND WorkId = ISNULL(@workId, WorkId)) AS items
	GROUP BY WorkId
*/
