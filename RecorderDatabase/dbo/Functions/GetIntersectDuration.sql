CREATE FUNCTION [dbo].[GetIntersectDuration]
	(
	@startDate1 datetime, --NOT NULL
	@endDate1 datetime,   --NOT NULL
	@startDate2 datetime, --NOT NULL
	@endDate2 datetime    --NOT NULL
	)
RETURNS bigint
WITH RETURNS NULL ON NULL INPUT
AS
	BEGIN
	
	DECLARE @startDate datetime, @endDate datetime
	
	SET @startDate = 
	CASE
		WHEN @startDate1 < @startDate2 THEN @startDate2
		ELSE @startDate1
	END
	
	SET @endDate = 
	CASE
		WHEN @endDate1 < @endDate2 THEN @endDate1
		ELSE @endDate2
	END
	
	RETURN 
	CASE 
		WHEN @endDate < @startDate THEN 0
		ELSE CAST(DATEDIFF(SECOND, @startDate, @endDate) AS bigint) * 1000 + DATEPART(MILLISECOND, @endDate) - DATEPART(MILLISECOND, @startDate)
	END	

	END
