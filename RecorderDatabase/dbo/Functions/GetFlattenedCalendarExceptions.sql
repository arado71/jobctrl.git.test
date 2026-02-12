CREATE FUNCTION [dbo].[GetFlattenedCalendarExceptions] 
(	
	@calendarId int 
)
RETURNS @results TABLE (Date datetime, IsWorkDay bit)
AS
BEGIN

	INSERT INTO @results
	SELECT Date, IsWorkDay 
	FROM
	(
		SELECT ce.Date, ce.IsWorkDay, RANK() OVER (PARTITION BY ce.Date ORDER BY ic.Level) AS Rank
		FROM CalendarExceptions ce
		JOIN dbo.GetInheritedCalendarIds(@calendarId) ic ON ce.CalendarId = ic.Id
	) tmp
	WHERE
	Rank = 1
	
	RETURN
	/*
	Basically its the same as:
	SELECT ce.Date, ce.IsWorkDay, ic.Level
	FROM CalendarExceptions ce
	JOIN dbo.GetInheritedCalendarIds(@calendarId) ic ON ce.CalendarId = ic.Id
	JOIN
		(SELECT Date, MIN(Level) AS Level
		FROM CalendarExceptions ce
		JOIN dbo.GetInheritedCalendarIds(@calendarId) ic ON ce.CalendarId = ic.Id
		GROUP BY Date) filter ON filter.Date = ce.Date AND filter.Level = ic.Level
	*/
END
