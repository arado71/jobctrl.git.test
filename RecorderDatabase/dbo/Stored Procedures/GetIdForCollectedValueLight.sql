Create PROCEDURE [dbo].[GetIdForCollectedValueLight]
	(
	@Items [dbo].[CollectedItemsDataType] Readonly
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON


	SELECT lup.[Id], items.[Value]
 FROM [dbo].[CollectedValueLookup] as lup, @Items as items
	 WHERE lup.[HashCode] = CHECKSUM(items.[Value])
  AND lup.[Value] = items.[Value]


	RETURN

