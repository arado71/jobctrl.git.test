CREATE PROCEDURE [dbo].[InsertCollectedItems]
(
	 @CollectedItems As [dbo].[CollectedItemsDataType] Readonly
	,@ReportServerAddress nvarchar(50) = null
)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON

	BEGIN DISTRIBUTED TRANSACTION TestTran

	INSERT INTO [dbo].[CollectedItems]
			   ([UserId]
			   ,[CreateDate]
			   ,[ComputerId]
			   ,[KeyId]
			   ,[ValueId])
		 SELECT
			   [UserId]
			   ,[CreateDate]
			   ,[ComputerId]
			   ,[KeyId]
			   ,[ValueId]
		FROM
				@CollectedItems 

	IF @ReportServerAddress IS NOT NULL
	BEGIN
		DECLARE @script nvarchar(1000);
		CREATE TABLE #CollectedItems 
		(
			 UserId INT
			,CreateDate DATETIME
			,ComputerId INT
			,KeyId INT
			,ValueId INT
		)

		INSERT INTO #CollectedItems (UserId, CreateDate, ComputerId, KeyId, ValueId)
			SELECT UserId, CreateDate, ComputerId, KeyId, ValueId
			FROM @CollectedItems

		SET @script = N'INSERT INTO [' + @ReportServerAddress + N'].[jobcontrol].[dbo].[CollectedItems]
							([UserId]
							,[CreateDate]
							,[ComputerId]
							,[KeyId]
							,[ValueId])
						SELECT
							[UserId]
							,[CreateDate]
							,[ComputerId]
							,[KeyId]
							,[ValueId]
						FROM
							#CollectedItems';

		EXECUTE sp_executesql @script;
	  END

	  COMMIT TRAN
END

