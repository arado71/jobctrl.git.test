EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
ALTER PROCEDURE [dbo].[UpsertVoiceRecordings]
	(
	@id int output,
	@clientId uniqueidentifier,
	@userId int,
	@workId int,
	@startDate datetime,
	@endDate datetime,
	@duration int,
	@codec int,
	@name nvarchar(200),
	@extension varchar(10),
	@offset int,
	@length int
	)
AS
	SET XACT_ABORT ON
	SET NOCOUNT ON

	BEGIN TRAN
	
	DECLARE @res int = 0
	
	IF @offset = 0 --insert
	BEGIN
		INSERT INTO [dbo].[VoiceRecordings]
				   ([ClientId]
				   ,[UserId]
				   ,[WorkId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[Duration]
				   ,[Codec]
				   ,[Name]
				   ,[Extension]
				   ,[Offset])
			 VALUES
				   (@clientId
				   ,@userId
				   ,@workId
				   ,@startDate
				   ,@endDate
				   ,@duration
				   ,@codec
				   ,@name
				   ,@extension
				   ,@length)

		SET @id = SCOPE_IDENTITY()
		SET @res = 1
	END
	ELSE --update
	BEGIN
		DECLARE @OutTable TABLE (Id int)
		UPDATE [dbo].[VoiceRecordings]
		   SET [LastReceiveDate] = GETUTCDATE()
			  --,[ClientId] = @clientId
			  --,[UserId] = @userId
			  --,[WorkId] = @workId
			  --,[StartDate] = @startDate
			  ,[EndDate] = @endDate
			  ,[Duration] = @duration
			  --,[Codec] = @codec
			  ,[Name] = @name
			  --,[Extension] = @extension
			  ,[Offset] = @offset + @length
		OUTPUT Deleted.Id INTO @OutTable
		 WHERE [ClientId] = @clientId
		   AND [UserId] = @userId
		   AND [Offset] = @offset
		   AND [EndDate] IS NULL
		
		SET @res = @@rowcount
		
		IF (@res = 0) --check for dupes
		BEGIN
			SET @id = (SELECT Id
						 FROM [dbo].[VoiceRecordings]
						WHERE [ClientId] = @clientId
						  AND [UserId] = @userId
						  AND [EndDate] = @endDate
						  AND [Duration] = @duration
						  AND [Offset] = @offset + @length)
		END 
		ELSE
		BEGIN
			SET @id = (SELECT Id FROM @OutTable)
		END
		
		IF (@id IS NULL)
		BEGIN
			RAISERROR(''Cannot find data to update'', 16, 1)
			ROLLBACK
			RETURN 0
		END
	END

	COMMIT TRAN
	RETURN @res
' 