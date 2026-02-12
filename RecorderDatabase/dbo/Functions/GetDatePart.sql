CREATE FUNCTION [dbo].[GetDatePart] 
(
	@date datetime
)
RETURNS datetime
WITH RETURNS NULL ON NULL INPUT
AS
BEGIN
	RETURN DATEADD(day, DATEDIFF(day, 0, @date), 0)
END
