CREATE PROCEDURE [dbo].[GetNextValueForSequence]
(
	@nextValue BINARY(8) OUTPUT
)
AS
DECLARE @outTable TABLE (outValue BINARY(8))
UPDATE [dbo].[RowVersionSequence] SET IsDummy=1 OUTPUT CAST(Deleted.Version AS BINARY(8)) AS VERSION INTO @outTable
IF (@@ROWCOUNT = 0) 
BEGIN
	BEGIN TRY
		INSERT INTO [dbo].[RowVersionSequence] (IsDummy) VALUES (1)
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() <> 2627)
		BEGIN
			DECLARE @ErrorMessage nvarchar(MAX), @ErrorSeverity INT, @ErrorState INT;
			SELECT @ErrorMessage = ERROR_MESSAGE() + ' Line ' + CAST(ERROR_LINE() AS nvarchar(5)), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
			raiserror (@ErrorMessage, @ErrorSeverity, @ErrorState);
		END
	END CATCH;
	UPDATE [dbo].[RowVersionSequence] SET IsDummy=1 OUTPUT CAST(Deleted.Version AS INT) AS VERSION INTO @outTable
END
SET @nextValue = (SELECT outValue FROM @outTable)
RETURN 0