CREATE FUNCTION [dbo].[GetDayRange]
(
	@start date, --inclusive
	@end date    --inclusive
)
RETURNS TABLE
RETURN
(
	WITH e1(n) AS
	(
		SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL 
		SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL 
		SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1
	), -- 10
	e2(n) AS (SELECT 1 FROM e1 CROSS JOIN e1 AS b), -- 10*10
	e3(n) AS (SELECT 1 FROM e2 CROSS JOIN e2 AS c) -- 100*100
	--numbers -> SELECT n = ROW_NUMBER() OVER (ORDER BY n) FROM e3 ORDER BY n;
	--SELECT TOP (DATEDIFF(DAY, @start, @end) + 1) [Day] = DATEADD(DAY, ROW_NUMBER() OVER (ORDER BY n)-1, @start) FROM e3 ORDER BY [Day]
	SELECT DATEADD(DAY, Number-1, @start) AS [Day] 
	  FROM
		(
		SELECT TOP (DATEDIFF(DAY, @start, @end) + 1) n = ROW_NUMBER() OVER (ORDER BY n) 
		  FROM e3
		 ORDER BY n
		) O(Number)

)