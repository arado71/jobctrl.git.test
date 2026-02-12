CREATE PROCEDURE [dbo].[ReportClientComputerVersion]
	(
	@userId int,
	@computerId int,
	@major int,
	@minor int,
	@build int,
	@revision int,
	@application nvarchar(50)
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @userId IS NULL OR @computerId IS NULL OR @major IS NULL OR @minor IS NULL OR @build IS NULL OR @revision IS NULL
	BEGIN
		RAISERROR('@userId, @computerId, @major, @minor, @build and @revision cannot be NULL', 16, 1)
		RETURN
	END

BEGIN TRAN

declare @cId int, @cMajor int, @cMinor int, @cBuild int, @cRevision int

SELECT @cId = [Id]
      ,@cMajor = [Major]
      ,@cMinor = [Minor]
      ,@cBuild = [Build]
      ,@cRevision = [Revision]
  FROM [dbo].[ClientComputerVersions] WITH (UPDLOCK, HOLDLOCK)
 WHERE [UserId] = @userId
   AND [ComputerId] = @computerId
   AND ISNULL([Application], 'JobCTRL') = @application
   AND [IsCurrent] = 1

IF (@@rowcount > 1)
BEGIN
	RAISERROR('More than one current versions', 16, 1)
	ROLLBACK
	RETURN
END

--same version
IF (@cMajor IS NOT NULL AND @major = @cMajor AND @minor = @cMinor AND @build = @cBuild AND @revision = @cRevision)
BEGIN
	UPDATE [dbo].[ClientComputerVersions]
	   SET [LastReceiveDate] = GETUTCDATE()
	 WHERE [UserId] = @userId
	   AND [ComputerId] = @computerId
	   AND ISNULL([Application], 'JobCTRL') = @application
	   AND [IsCurrent] = 1
END
ELSE --new version
BEGIN
	UPDATE [dbo].[ClientComputerVersions]
	   SET [IsCurrent] = 0
	 WHERE [UserId] = @userId
	   AND [ComputerId] = @computerId
	   AND ISNULL([Application], 'JobCTRL') = @application
	   AND [IsCurrent] = 1

	INSERT INTO [dbo].[ClientComputerVersions]
			   ([UserId]
			   ,[ComputerId]
			   ,[Major]
			   ,[Minor]
			   ,[Build]
			   ,[Revision]
			   ,[IsCurrent]
			   ,[FirstReceiveDate]
			   ,[LastReceiveDate]
			   ,[Application])
		 VALUES
			   (@userId
			   ,@computerId
			   ,@major
			   ,@minor
			   ,@build
			   ,@revision
			   ,1
			   ,GETUTCDATE()
			   ,GETUTCDATE()
			   ,@application)
END

COMMIT TRAN
	RETURN
