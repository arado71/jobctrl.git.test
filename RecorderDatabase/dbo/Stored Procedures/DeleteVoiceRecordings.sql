CREATE PROCEDURE [dbo].[DeleteVoiceRecordings]
	(
	@id int output,
	@clientId uniqueidentifier,
	@userId int,
	@deleteDate datetime output
	)
AS
	SET XACT_ABORT ON
	SET NOCOUNT ON

	BEGIN TRAN

	DECLARE @res int = 0
	DECLARE @OutTable TABLE (Id int, DeleteDate datetime)
	SET @id = NULL
	SET @deleteDate = NULL
	
	UPDATE [dbo].[VoiceRecordings]
	   SET [DeleteDate] = GETUTCDATE()
	OUTPUT Deleted.Id, Inserted.DeleteDate INTO @OutTable
	 WHERE [ClientId] = @clientId
	   AND [UserId] = @userId
	   AND [DeleteDate] IS NULL

	SET @res = @@rowcount
	
	IF (@res = 0) --check for dupes
	BEGIN
		SELECT @id = [Id]
			  ,@deleteDate = [DeleteDate]
		  FROM [dbo].[VoiceRecordings]
		 WHERE [ClientId] = @clientId
		   AND [UserId] = @userId
		   AND [DeleteDate] IS NOT NULL
	END
	ELSE
	BEGIN
		SELECT @id = [Id]
			  ,@deleteDate = [DeleteDate]
		  FROM @OutTable
	END
	
	IF (@id IS NULL)
	BEGIN
		RAISERROR('Cannot find data to delete', 16, 1)
		ROLLBACK
		RETURN 0
	END

	COMMIT TRAN
	RETURN @res
