CREATE PROCEDURE [dbo].[UpsertClientComputerError]
	@id int output,
	@clientId uniqueidentifier,
	@userId int,
	@computerId int,
	@major int,
	@minor int,
	@build int,
	@revision int,
	@description nvarchar(2000),
	@features nvarchar(2000),
	@hasAttachment bit,
	@offset int,
	@length int,
	@isCompleted bit,
	@isCancelled bit,
	@firstReceiveDate datetime output
AS
	SET XACT_ABORT ON
	SET NOCOUNT ON

	BEGIN TRAN
	
	DECLARE @res int = 0
	DECLARE @OutTable TABLE (Id int, FirstRecevieDate datetime)
	
	IF @offset = 0 --insert
	BEGIN
		INSERT INTO [dbo].[ClientComputerErrors]
				   ([ClientId]
				   ,[UserId]
				   ,[ComputerId]
				   ,[Major]
				   ,[Minor]
				   ,[Build]
				   ,[Revision]
				   ,[Description]
				   ,[EnabledFeatures]
				   ,[HasAttachment]
				   ,[Offset]
				   ,[IsCompleted]
				   ,[IsCancelled])
			 OUTPUT Inserted.Id, Inserted.FirstReceiveDate INTO @OutTable
			 VALUES
				   (@clientId
				   ,@userId
				   ,@computerId
				   ,@major
				   ,@minor
				   ,@build
				   ,@revision
				   ,@description
				   ,@features
				   ,@hasAttachment
				   ,@length
				   ,@isCompleted
				   ,@isCancelled)

		SELECT @id = Id, @firstReceiveDate = FirstRecevieDate FROM @OutTable
		SET @res = 1
	END
	ELSE --update
	BEGIN
		UPDATE [dbo].[ClientComputerErrors]
		   SET [LastReceiveDate] = GETUTCDATE()
			  --,[ClientId] = @clientId
			  --,[UserId] = @userId
			  --,[ComputerId] = @computerId
			  --,[Major] = @major
			  --,[Minor] = @minor
			  --,[Build] = @build
			  --,[Revision] = @revision
			  --,[Description] = @description
			  --,[HasAttachment] = @hasAttachment
			  ,[Offset] = @offset + @length
			  ,[IsCompleted] = @isCompleted
			  ,[IsCancelled] = @isCancelled
		OUTPUT Deleted.Id, Deleted.FirstReceiveDate INTO @OutTable
		 WHERE [ClientId] = @clientId
		   AND [UserId] = @userId
		   AND [Offset] = @offset
		   AND [IsCompleted] = 0
		
		SET @res = @@rowcount
		
		IF (@res = 0) --check for dupes
		BEGIN
			SELECT @id = Id, @firstReceiveDate = FirstReceiveDate
			  FROM [dbo].[ClientComputerErrors]
			 WHERE [ClientId] = @clientId
			   AND [UserId] = @userId
			   AND [Offset] = @offset + @length
			IF (@@rowcount<>1) SET @id = NULL
		END 
		ELSE
		BEGIN
			SELECT @id = Id, @firstReceiveDate = FirstRecevieDate FROM @OutTable
			IF (@@rowcount<>1) SET @id = NULL
		END
		
		IF (@id IS NULL)
		BEGIN
			RAISERROR('Cannot find data to update', 16, 1)
			ROLLBACK
			RETURN 0
		END
	END

	COMMIT TRAN
	RETURN @res
