CREATE FUNCTION [dbo].[GetIvrEndDateNotNull] 
(
	@endDate datetime,          --NULL
	@ivrLastCheckDate datetime, --NOT NULL
	@maxEndDate datetime        --NOT NULL
)
RETURNS datetime
AS
BEGIN
	RETURN CASE
		WHEN @endDate IS NOT NULL THEN @endDate
		ELSE CASE
				WHEN @ivrLastCheckDate < @maxEndDate THEN @ivrLastCheckDate
				ELSE @maxEndDate
				END
		END
END