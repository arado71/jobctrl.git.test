Create PROCEDURE [dbo].[GetIdForCollectedKeyHard]
	(
	@key nvarchar(4000),
	@id int OUTPUT
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @key IS NULL
	BEGIN
		RAISERROR('@@key cannot be NULL', 16, 1)
		RETURN
	END

declare @hashCode int
SET @hashCode = CHECKSUM(@key)

BEGIN TRAN

SET @id = (
	SELECT TOP 1 [Id]
	  FROM [dbo].[CollectedKeyLookup] WITH (TABLOCKX, HOLDLOCK)
	 WHERE [HashCode] = @hashCode
	   AND [Key] = @key
	)

IF @id IS NULL
BEGIN
	INSERT INTO [dbo].[CollectedKeyLookup]
			   ([HashCode]
			   ,[Key])
		 VALUES
			   (@hashCode
			   ,@key)

	SET @id = SCOPE_IDENTITY()
END

COMMIT TRAN

	RETURN