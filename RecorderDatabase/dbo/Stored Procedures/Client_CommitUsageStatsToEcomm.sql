CREATE PROCEDURE [dbo].[Client_CommitUsageStatsToEcomm]
AS

	SET NOCOUNT ON
	SET XACT_ABORT ON

	declare @c_LocalDate datetime,
			@c_UserId int,
			@c_Id int,
			@result int

	BEGIN TRAN

	DECLARE upd_cursor CURSOR FORWARD_ONLY FOR
	SELECT 
		Id,
		LocalDate,
		UserId
	FROM UsageStats (UPDLOCK)
	WHERE
		IsAcked = 0
		AND (ComputerWorkTime + IvrWorkTime + MobileWorkTime + ManuallyAddedWorkTime) > 300000 -- 5 mins

	OPEN upd_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM upd_cursor INTO 
			@c_Id,
			@c_LocalDate,
			@c_UserId

		IF @@FETCH_STATUS <> 0
			BREAK

		EXEC @result = [dbo].[Client_SetWorkedUsersOnDay] @userId = @c_UserId, @Day = @c_LocalDate
		IF @result = 0
		BEGIN
			UPDATE UsageStats
				SET IsAcked = 1
				WHERE Id = @c_Id
		END

	END

	CLOSE upd_cursor
	DEALLOCATE upd_cursor

	COMMIT TRAN
	RETURN 0
