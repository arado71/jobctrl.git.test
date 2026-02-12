CREATE PROCEDURE [dbo].[ClientComputerKickConfirm]
	(
	@id int,
	@userId int,
	@deviceId bigint,
	@confirmDate datetime,
	@result int
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @id IS NULL OR @userId IS NULL OR @deviceId IS NULL OR @confirmDate IS NULL OR @result IS NULL
	BEGIN
		RAISERROR('@id, @userId, @deviceId, @confirmDate and @result cannot be NULL', 16, 1)
		RETURN
	END

UPDATE [dbo].[ClientComputerKicks]
   SET [ConfirmDate] = @confirmDate
      ,[Result] = @result
 WHERE [Id] = @id
   AND [UserId] = @userId
   AND [ComputerId] = @deviceId
   AND [ConfirmDate] IS NULL
   AND [Result] IS NULL

	RETURN @@ROWCOUNT

