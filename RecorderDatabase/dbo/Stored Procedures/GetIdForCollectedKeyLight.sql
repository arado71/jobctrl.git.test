Create PROCEDURE [dbo].[GetIdForCollectedKeyLight]
	(
	@Items [dbo].[CollectedItemsDataType] Readonly
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON


SELECT lup.[Id], items.[Key]
 FROM [dbo].[CollectedKeyLookup] as lup, @Items as items
	 WHERE lup.[HashCode] = CHECKSUM(items.[Key])
  AND lup.[Key] = items.[Key]


	RETURN