CREATE PROCEDURE [dbo].[Client_DeleteCloudToken]
	@UserId int
AS
	
	DELETE 
		FROM [ClientUserCloudTokens] 
		WHERE UserId = @UserId

RETURN 0
