CREATE FUNCTION [dbo].[GetInheritedCalendarIds] 
(	
	@calendarId int
)
RETURNS TABLE 
AS
RETURN 
	WITH InheritedCalendars(Id, InheritedFrom, Level)
	AS
	(
		-- Anchor member definition
		SELECT c.Id, c.InheritedFrom, 0 AS Level
		FROM Calendars AS c
		WHERE c.Id = @calendarId
		
		UNION ALL
		-- Recursive member definition
		SELECT c.Id, c.InheritedFrom, ic.Level + 1 AS Level
		FROM Calendars c
		JOIN InheritedCalendars AS ic ON c.Id = ic.InheritedFrom
	)
	-- Statement that executes the CTE
	SELECT Id, Level FROM InheritedCalendars
