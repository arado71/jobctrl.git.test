Create PROCEDURE [dbo].[GetIdForCollectedValueHard]
	(
	@value nvarchar(4000),
	@id int OUTPUT
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @value IS NULL
	BEGIN
		RAISERROR('@value cannot be NULL', 16, 1)
		RETURN
	END

declare @hashCode int
SET @hashCode = CHECKSUM(@value)


BEGIN TRAN

SELECT @id = [Id]
 FROM [dbo].[CollectedValueLookup]
WHERE [HashCode] = @hashCode
  AND [Value] = @value

IF @id IS NULL
BEGIN
	INSERT INTO [dbo].[CollectedValueLookup]
			   ([HashCode]
			   ,[Value])
		 VALUES
			   (@hashCode
			   ,@value)

	SET @id = SCOPE_IDENTITY()
END

COMMIT TRAN

	RETURN