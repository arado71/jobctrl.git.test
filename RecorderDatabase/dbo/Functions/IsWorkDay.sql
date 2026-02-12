CREATE FUNCTION [dbo].[IsWorkDay] 
(
	@calendarId int,
	@date datetime
)
RETURNS bit
WITH RETURNS NULL ON NULL INPUT
AS
BEGIN
	declare @realDate datetime
	SET @realDate = dbo.GetDatePart(@date)
	
	DECLARE @result bit
	SET @result = (
		SELECT IsWorkDay FROM dbo.GetFlattenedCalendarExceptions(@calendarId) ce
		WHERE ce.Date = @realDate
	)
	
	IF (@result IS NULL)
	BEGIN
		SET @result = 
		CASE ((DATEPART(dw, @realDate) + @@DATEFIRST - 1) % 7)
			WHEN 1 THEN (SELECT IsMondayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 2 THEN (SELECT IsTuesdayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 3 THEN (SELECT IsWednesdayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 4 THEN (SELECT IsThursdayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 5 THEN (SELECT IsFridayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 6 THEN (SELECT IsSaturdayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 0 THEN (SELECT IsSundayWorkDay FROM Calendars WHERE Id = @calendarId)
			ELSE NULL
		END
	END

	RETURN @result

END
