CREATE PROCEDURE [dbo].[UpdateHourlyAggregateWorkItems]
AS
	SET NOCOUNT ON

SET XACT_ABORT ON
declare @StartId bigint, @EndId bigint = 0, @MinStartDate datetime
declare @MaxEndId bigint
SET @MaxEndId = (SELECT ISNULL(MAX(Id),0) FROM [dbo].[WorkItems]) -- without any lock

WHILE @EndId < @MaxEndId
BEGIN
	BEGIN TRAN
	SET @StartId = (SELECT ISNULL(MAX(LastAggregatedId),0) FROM dbo.AggregateLastWorkItem WITH (TABLOCKX, HOLDLOCK))

	exec dbo.[UpdateHourlyAggregateWorkItemsFromId] @StartId, @EndId OUT, @MinStartDate OUT --MinStartDate not used anymore

	TRUNCATE TABLE dbo.AggregateLastWorkItem

	INSERT INTO dbo.AggregateLastWorkItem VALUES (@EndId)

	COMMIT TRAN	
END
RETURN
