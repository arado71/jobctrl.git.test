SET NOCOUNT ON; 
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
SET XACT_ABORT ON;

SET IDENTITY_INSERT [dbo].[Screens] ON;

declare @maxId bigint,
		@currId bigint, 
		@nextId bigint, 
		@step bigint, 
		@msg nvarchar(4000),
		@cWorkItemId bigint,
		@cCreateDate datetime,
		@cUserId int,
		@cActiveWindowId bigint,
		@cProcessName nvarchar(100),
		@cTitle nvarchar(1000),
		@cUrl nvarchar(1000),
		@cScreenShotId bigint,
		@cScreenCreateDate datetime,
		@cScreenNumber tinyint,
		@cExtension varchar(10),
		@cCaptureId bigint,
		@cProcessNameId int,
		@cTitleId int,
		@cUrlId int,
		@cDesktopWindowId bigint,
		@cLastInsWItemId bigint,
		@cLastInsAWindowId bigint,
		@cLastInsSShotId bigint,
		@lastReportDate datetime

SET @step = 1000
SET @maxId = (SELECT MAX(Id) FROM WorkItems (TABLOCKX))
SET @currId = 1
SET @lastReportDate = (SELECT GETUTCDATE()-1)

WHILE @currId <= @maxId
BEGIN
	SET @nextId = @currId + @step - 1
	IF @nextId > @maxId
		SET @nextId = @maxId
	
	BEGIN TRAN
	
	SET @cLastInsWItemId = 0
	SET @cLastInsAWindowId = 0
	SET @cLastInsSShotId = 0
	
	DECLARE convert_cursor CURSOR FAST_FORWARD FOR 
	SELECT w.Id, w.StartDate, w.UserId, a.Id, a.ProcessName, a.Title, a.Url, s.Id, s.CreateDate, s.ScreenNumber, s.Extension
	FROM WorkItems w
	LEFT JOIN ActiveWindows a ON a.WorkItemId = w.Id
	LEFT JOIN ScreenShots s ON s.WorkItemId = w.Id
	WHERE w.Id >= @currId AND w.Id <= @nextId
	ORDER BY w.Id, s.Id, a.Id
	
	OPEN convert_cursor
	
	WHILE 1=1
	BEGIN
		FETCH NEXT FROM convert_cursor INTO 
			@cWorkItemId,
			@cCreateDate,
			@cUserId,
			@cActiveWindowId,
			@cProcessName,
			@cTitle,
			@cUrl,
			@cScreenShotId,
			@cScreenCreateDate,
			@cScreenNumber,
			@cExtension

		IF @@FETCH_STATUS <> 0
			BREAK
			
		IF @cLastInsWItemId < @cWorkItemId AND (@cActiveWindowId IS NOT NULL OR @cScreenShotId IS NOT NULL)
		BEGIN
			INSERT INTO [dbo].[DesktopCaptures] ([WorkItemId]) VALUES (@cWorkItemId)
			SET @cCaptureId = SCOPE_IDENTITY()
			SET @cLastInsWItemId = @cWorkItemId
			SET @cLastInsAWindowId = 0
			SET @cLastInsSShotId = 0
		END
		
		IF @cActiveWindowId IS NOT NULL AND @cLastInsAWindowId < @cActiveWindowId
		BEGIN
			exec GetIdForProcessName @cProcessName, @cProcessNameId output

			exec GetIdForTitle @cTitle, @cTitleId output

			SET @cUrlId = NULL
			IF @cUrl IS NOT NULL
				exec GetIdForUrl @cUrl, @cUrlId output		
			
			INSERT INTO [dbo].[DesktopWindows]
					   ([DesktopCaptureId]
					   ,[UserId]
					   ,[CreateDate]
					   ,[ProcessNameId]
					   ,[TitleId]
					   ,[UrlId]
					   ,[IsActive]
					   ,[X]
					   ,[Y]
					   ,[Width]
					   ,[Height]
					   ,[ClientArea]
					   ,[VisibleClientArea])
				 VALUES
					   (@cCaptureId
					   ,@cUserId
					   ,@cCreateDate
					   ,@cProcessNameId
					   ,@cTitleId
					   ,@cUrlId
					   ,1
					   ,0
					   ,0
					   ,0
					   ,0
					   ,0
					   ,0)

			SET @cDesktopWindowId = SCOPE_IDENTITY()
			
			INSERT INTO [dbo].[DesktopActiveWindows]
					   ([Id]
					   ,[DesktopCaptureId]
					   ,[UserId]
					   ,[CreateDate]
					   ,[ProcessNameId]
					   ,[TitleId]
					   ,[UrlId])
				 VALUES
					   (@cDesktopWindowId
					   ,@cCaptureId
					   ,@cUserId
					   ,@cCreateDate
					   ,@cProcessNameId
					   ,@cTitleId
					   ,@cUrlId)
					   
			SET @cLastInsAWindowId = @cActiveWindowId
		END

		IF @cScreenShotId IS NOT NULL AND @cLastInsSShotId < @cScreenShotId
		BEGIN
		
			INSERT INTO [dbo].[Screens]
					   ([Id]
					   ,[DesktopCaptureId]
					   ,[UserId]
					   ,[CreateDate]
					   ,[X]
					   ,[Y]
					   ,[Width]
					   ,[Height]
					   ,[ScreenNumber]
					   ,[Extension])
				 VALUES
					   (@cScreenShotId --important for path resolvers
					   ,@cCaptureId
					   ,@cUserId
					   ,@cScreenCreateDate --important for path resolvers
					   ,0
					   ,0
					   ,0
					   ,0
					   ,@cScreenNumber
					   ,@cExtension)
		
			SET @cLastInsSShotId = @cScreenShotId
		END

	END
	
	CLOSE convert_cursor
	DEALLOCATE convert_cursor

	COMMIT TRAN
	
	SET @currId = @nextId + 1
	
	IF @nextId = @maxId OR DATEDIFF(second,@lastReportDate,GETUTCDATE()) > 5
	BEGIN
		SET @lastReportDate = (SELECT GETUTCDATE())
		SET @msg = CONVERT(varchar, GETDATE(), 120) + ' - ' + CAST(@nextId AS varchar) + ' / ' + CAST(@maxId AS varchar) + ' ('+CAST(@nextId*100/@maxId AS varchar)+')' + CHAR(13) + CHAR(10)
		RAISERROR(@msg,10,1) WITH NOWAIT
	END
END

SET IDENTITY_INSERT [dbo].[Screens] OFF;

SELECT @maxId