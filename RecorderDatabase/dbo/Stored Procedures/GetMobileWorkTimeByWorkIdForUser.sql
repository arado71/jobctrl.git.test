CREATE PROCEDURE [dbo].[GetMobileWorkTimeByWorkIdForUser]
	(
	@userId int,
	@startDate datetime,
	@endDate datetime
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
		MobileWorkItems 
	WHERE 
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	GROUP BY WorkId
		
	SELECT * FROM @result

	RETURN
