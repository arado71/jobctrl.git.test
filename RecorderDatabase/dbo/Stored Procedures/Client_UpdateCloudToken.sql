CREATE PROCEDURE [dbo].[Client_UpdateCloudToken]
	@UserId int,
	@SyncToken nvarchar(4000),
	@ServiceType smallint,
	@LastUpdateTime datetime,
	@LastCheckTime datetime
AS
	UPDATE [ClientUserCloudTokens] 
		SET 
			SyncToken = @SyncToken, 
			ServiceType = @ServiceType, 
			LastUpdateTime = @LastUpdateTime, 
			LastCheckTime = @LastCheckTime 
		WHERE UserId = @UserId

RETURN 0
