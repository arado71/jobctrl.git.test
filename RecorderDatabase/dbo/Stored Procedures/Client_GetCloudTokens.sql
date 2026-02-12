CREATE PROCEDURE [dbo].[Client_GetCloudTokens]
	@SyncedBefore datetime
AS
	
	SELECT uct.UserId, AuthToken, SyncToken, ServiceType, LastUpdateTime, LastCheckTime, cs.IsMeetingTentativeSynced
		FROM [ClientUserCloudTokens] uct 
		LEFT JOIN ClientSettings cs ON uct.UserId = cs.UserId
		WHERE (LastCheckTime IS NULL OR LastCheckTime < @SyncedBefore) and cs.IsGoogleCalendarTrackingEnabled = 1

RETURN 0
