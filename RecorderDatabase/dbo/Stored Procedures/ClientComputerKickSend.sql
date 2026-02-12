CREATE PROCEDURE [dbo].[ClientComputerKickSend]
	(
	@id int,
	@userId int,
	@deviceId bigint,
	@sendDate datetime
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @id IS NULL OR @userId IS NULL OR @deviceId IS NULL OR @sendDate IS NULL
	BEGIN
		RAISERROR('@userId, @deviceId and @sendDate cannot be NULL', 16, 1)
		RETURN
	END

UPDATE [dbo].[ClientComputerKicks]
   SET [SendDate] = @sendDate
 WHERE [Id] = @id
   AND [UserId] = @userId
   AND [ComputerId] = @deviceId
   AND [SendDate] IS NULL

	RETURN @@ROWCOUNT

