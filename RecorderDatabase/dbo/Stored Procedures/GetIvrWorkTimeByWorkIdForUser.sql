CREATE PROCEDURE [dbo].[GetIvrWorkTimeByWorkIdForUser]
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
		ivrItems.WorkId AS WorkId,
		SUM(
		CASE
			WHEN ivrItems.StartDate < ivrItems.EndDate THEN CAST(DATEDIFF(SECOND, ivrItems.StartDate, ivrItems.EndDate) AS bigint) * 1000 + DATEPART(MILLISECOND, ivrItems.EndDate) - DATEPART(MILLISECOND, ivrItems.StartDate)
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
				WHEN EndDate IS NOT NULL THEN
					CASE
						WHEN EndDate < @endDate THEN EndDate
						ELSE @endDate
					END
				WHEN IvrLastCheckDate < MaxEndDate THEN
					CASE
						WHEN IvrLastCheckDate < @endDate THEN IvrLastCheckDate
						ELSE @endDate
					END
				ELSE
					CASE
						WHEN MaxEndDate < @endDate THEN MaxEndDate
						ELSE @endDate
					END
			END AS EndDate
		FROM
			IvrWorkItems
		WHERE
			UserId = @userId
		AND StartDate < @endDate --not in the future
		AND StartDate >= DATEADD(day, -2, @startDate)
		AND @startDate < ISNULL(EndDate,MaxEndDate)
		AND WorkId = ISNULL(@workId, WorkId)
		) AS ivrItems
	GROUP BY
		WorkId
		
	SELECT * FROM @result

	RETURN
