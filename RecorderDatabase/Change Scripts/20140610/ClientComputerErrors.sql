/****** Object:  StoredProcedure [dbo].[UpsertClientComputerError]    Script Date: 06/10/2014 14:55:53 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpsertClientComputerError]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpsertClientComputerError]
GO
/****** Object:  Table [dbo].[ClientComputerErrors]    Script Date: 06/10/2014 14:55:53 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]') AND type in (N'U'))
DROP TABLE [dbo].[ClientComputerErrors]
GO
/****** Object:  Default [DF_ClientComputerErrors_FirstReceiveDate]    Script Date: 06/10/2014 14:55:53 ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerErrors_FirstReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerErrors_FirstReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerErrors] DROP CONSTRAINT [DF_ClientComputerErrors_FirstReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerErrors_LastReceiveDate]    Script Date: 06/10/2014 14:55:53 ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerErrors_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerErrors_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerErrors] DROP CONSTRAINT [DF_ClientComputerErrors_LastReceiveDate]
END


End
GO
/****** Object:  Table [dbo].[ClientComputerErrors]    Script Date: 06/10/2014 14:55:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClientComputerErrors](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[ComputerId] [int] NOT NULL,
	[Major] [int] NOT NULL,
	[Minor] [int] NOT NULL,
	[Build] [int] NOT NULL,
	[Revision] [int] NOT NULL,
	[Description] [nvarchar](4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[HasAttachment] [bit] NOT NULL,
	[FirstReceiveDate] [datetime] NOT NULL,
	[LastReceiveDate] [datetime] NOT NULL,
	[Offset] [int] NOT NULL,
	[IsCompleted] [bit] NOT NULL,
	[IsCancelled] [bit] NOT NULL,
 CONSTRAINT [PK_ClientComputerErrors] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]') AND name = N'IX_ClientComputerErrors_ClientId')
CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientComputerErrors_ClientId] ON [dbo].[ClientComputerErrors] 
(
	[ClientId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  StoredProcedure [dbo].[UpsertClientComputerError]    Script Date: 06/10/2014 14:55:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpsertClientComputerError]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Attila Borbely
-- =============================================
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
			RAISERROR(''Cannot find data to update'', 16, 1)
			ROLLBACK
			RETURN 0
		END
	END

	COMMIT TRAN
	RETURN @res
' 
END
GO
/****** Object:  Default [DF_ClientComputerErrors_FirstReceiveDate]    Script Date: 06/10/2014 14:55:53 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerErrors_FirstReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerErrors_FirstReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerErrors] ADD  CONSTRAINT [DF_ClientComputerErrors_FirstReceiveDate]  DEFAULT (getutcdate()) FOR [FirstReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerErrors_LastReceiveDate]    Script Date: 06/10/2014 14:55:53 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerErrors_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerErrors_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerErrors] ADD  CONSTRAINT [DF_ClientComputerErrors_LastReceiveDate]  DEFAULT (getutcdate()) FOR [LastReceiveDate]
END


End
GO
