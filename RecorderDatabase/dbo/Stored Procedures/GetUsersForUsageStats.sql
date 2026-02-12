CREATE PROCEDURE [dbo].[GetUsersForUsageStats]
	(
	@startDate datetime = NULL,
	@endDate datetime = NULL
	)
AS
	SET NOCOUNT ON

	IF @startDate IS NULL SET @startDate = '1900-01-01'
	IF @endDate IS NULL SET @endDate = '3000-01-01'

	select u.UserId
	from dbo.ClientSettings u
	where exists (select 1 from dbo.AggregateWorkItemIntervals a where a.UserId = u.UserId and a.StartDate between @startDate and @endDate)
	
	UNION

	select u.UserId
	from dbo.ClientSettings u
	where exists (select 1 from dbo.MobileWorkItems a where a.UserId = u.UserId and a.StartDate between @startDate and @endDate)

	UNION

	select u.UserId
	from dbo.ClientSettings u
	where exists (select 1 from dbo.ManualWorkItems a where a.UserId = u.UserId and a.StartDate between @startDate and @endDate)

	UNION

	SELECT DISTINCT UserId 
	  FROM IvrWorkItems 
	WHERE StartDate < @endDate 
	   AND ((EndDate IS NOT NULL AND EndDate > @startDate)
			OR (EndDate IS NULL AND IvrLastCheckDate < MaxEndDate AND @startDate < IvrLastCheckDate)
			OR (EndDate IS NULL AND IvrLastCheckDate >= MaxEndDate AND @startDate < MaxEndDate))

	RETURN
