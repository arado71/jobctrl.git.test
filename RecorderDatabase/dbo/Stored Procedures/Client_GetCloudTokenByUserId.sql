CREATE PROCEDURE [dbo].[Client_GetCloudTokenByUserId]
	@UserId int
AS
	
	SELECT AuthToken, SyncToken, ServiceType, LastUpdateTime, LastCheckTime 
		FROM [ClientUserCloudTokens] 
		WHERE UserId = @UserId

RETURN 0
