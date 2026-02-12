CREATE PROCEDURE [dbo].[Client_SetCloudToken]
	@UserId int,
	@AuthToken nvarchar(4000)
AS
	DECLARE @res int

	BEGIN TRAN

	UPDATE [ClientUserCloudTokens]
		SET AuthToken = @AuthToken
		WHERE UserId = @UserId;

	SET @res = @@rowcount

	IF @res = 0
	BEGIN
		INSERT INTO [ClientUserCloudTokens] (UserId, AuthToken) 
			VALUES (@UserId, @AuthToken)
	END

	COMMIT TRAN

	RETURN 0
