CREATE PROCEDURE [dbo].[Client_SetWorkedUsersOnDay]
	(
	@UserId int,
	@Day datetime
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	--dummy placeholder sproc for CommitUsageStatsToEcomm
	--don't publish to LIVE servers

	IF @UserId < 0
	BEGIN
		RETURN 1 --test error case with negative userid
	END
	ELSE
	BEGIN
		RETURN 0
	END
