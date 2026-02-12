/****** Object:  StoredProcedure [dbo].[M_InsertData]    Script Date: 10/03/2013 19:11:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_InsertData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[M_InsertData]
GO
/****** Object:  StoredProcedure [dbo].[M_InsertLocation]    Script Date: 10/03/2013 19:11:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_InsertLocation]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[M_InsertLocation]
GO
/****** Object:  StoredProcedure [dbo].[M_InsertWorkItem]    Script Date: 10/03/2013 19:11:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_InsertWorkItem]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[M_InsertWorkItem]
GO
/****** Object:  Table [dbo].[MobileClientLocations]    Script Date: 10/03/2013 19:11:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MobileClientLocations]') AND type in (N'U'))
DROP TABLE [dbo].[MobileClientLocations]
GO
/****** Object:  UserDefinedTableType [dbo].[MobileLocationTableType]    Script Date: 10/03/2013 19:11:31 ******/
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'MobileLocationTableType' AND ss.name = N'dbo')
DROP TYPE [dbo].[MobileLocationTableType]
GO
/****** Object:  UserDefinedTableType [dbo].[MobilePhoneCallTableType]    Script Date: 10/03/2013 19:11:31 ******/
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'MobilePhoneCallTableType' AND ss.name = N'dbo')
DROP TYPE [dbo].[MobilePhoneCallTableType]
GO
/****** Object:  Table [dbo].[MobileWorkItems]    Script Date: 10/03/2013 19:11:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MobileWorkItems]') AND type in (N'U'))
DROP TABLE [dbo].[MobileWorkItems]
GO
/****** Object:  UserDefinedTableType [dbo].[MobileWorkItemTableType]    Script Date: 10/03/2013 19:11:31 ******/
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'MobileWorkItemTableType' AND ss.name = N'dbo')
DROP TYPE [dbo].[MobileWorkItemTableType]
GO
/****** Object:  StoredProcedure [dbo].[M_InsertPhoneCall]    Script Date: 10/03/2013 19:11:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_InsertPhoneCall]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[M_InsertPhoneCall]
GO
/****** Object:  Default [DF_MobileClientLocations_CreateDate]    Script Date: 10/03/2013 19:11:30 ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_MobileClientLocations_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[MobileClientLocations]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_MobileClientLocations_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[MobileClientLocations] DROP CONSTRAINT [DF_MobileClientLocations_CreateDate]
END


End
GO
/****** Object:  StoredProcedure [dbo].[M_InsertPhoneCall]    Script Date: 10/03/2013 19:11:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_InsertPhoneCall]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[M_InsertPhoneCall]

@userid int,
@phonenumber varchar(20),
@startdate datetime,
@enddate datetime,
@isinbound bit,
@phonecallid bigint = 0 output

as

return 0' 
END
GO
/****** Object:  UserDefinedTableType [dbo].[MobileWorkItemTableType]    Script Date: 10/03/2013 19:11:31 ******/
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'MobileWorkItemTableType' AND ss.name = N'dbo')
CREATE TYPE [dbo].[MobileWorkItemTableType] AS TABLE(
	[Id] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Imei] [bigint] NOT NULL,
	[WorkId] [int] NOT NULL,
	[SessionId] [uniqueidentifier] NOT NULL,
	[CallId] [int] NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[ReceiveDate] [datetime] NOT NULL
)
GO
/****** Object:  Table [dbo].[MobileWorkItems]    Script Date: 10/03/2013 19:11:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MobileWorkItems]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MobileWorkItems](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[WorkId] [int] NOT NULL,
	[SessionId] [uniqueidentifier] NOT NULL,
	[Imei] [bigint] NOT NULL,
	[FirstReceiveDate] [datetime] NOT NULL,
	[LastReceiveDate] [datetime] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[CallId] [bigint] NULL,
 CONSTRAINT [PK_MobileWorkItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  UserDefinedTableType [dbo].[MobilePhoneCallTableType]    Script Date: 10/03/2013 19:11:31 ******/
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'MobilePhoneCallTableType' AND ss.name = N'dbo')
CREATE TYPE [dbo].[MobilePhoneCallTableType] AS TABLE(
	[Id] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[PhoneNumber] [varchar](20) COLLATE Hungarian_CI_AS NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[IsInbound] [bit] NOT NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[MobileLocationTableType]    Script Date: 10/03/2013 19:11:31 ******/
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'MobileLocationTableType' AND ss.name = N'dbo')
CREATE TYPE [dbo].[MobileLocationTableType] AS TABLE(
	[Id] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Imei] [bigint] NOT NULL,
	[WorkId] [int] NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[Accuracy] [real] NOT NULL,
	[Date] [datetime] NOT NULL
)
GO
/****** Object:  Table [dbo].[MobileClientLocations]    Script Date: 10/03/2013 19:11:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MobileClientLocations]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MobileClientLocations](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Imei] [bigint] NOT NULL,
	[WorkId] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Longitude] [float] NOT NULL,
	[Latitude] [float] NOT NULL,
	[Accuracy] [real] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_MobileClientLocations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  StoredProcedure [dbo].[M_InsertWorkItem]    Script Date: 10/03/2013 19:11:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_InsertWorkItem]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[M_InsertWorkItem]
	
@userid int,
@workid int,
@sessionid uniqueidentifier,
@imei bigint,
@receivedate datetime,
@startdate datetime,
@enddate datetime,
@callid bigint = null,
@lastenddate datetime = null output

AS

SET NOCOUNT ON

-- Get the workitem''s end date of the current session
select	@lastenddate = enddate
from mobileworkitems
where sessionid = @sessionid

-- Check whether last upload was successful but the client didn''t received the acknowlendgement of it
if @lastenddate is not null and (@startdate > @lastenddate OR @enddate <= @lastenddate)
	return 1
	
if @lastenddate is not null
begin
		-- Extend the existing workitem
		update mobileworkitems
		set lastreceivedate = @receivedate,
			enddate = @enddate
		where sessionid = @sessionid
end
else
begin
	-- Start a new workItem
	insert into mobileworkitems (userid, workid, sessionid, imei, firstreceivedate, lastreceivedate, startdate, enddate, callid)
	values (@userid, @workid, @sessionid, @imei, @receivedate, @receivedate, @startdate, @enddate, @callid)
end
	
return 0

' 
END
GO
/****** Object:  StoredProcedure [dbo].[M_InsertLocation]    Script Date: 10/03/2013 19:11:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_InsertLocation]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[M_InsertLocation]

@userid int,
@workid int,
@imei bigint,
@longitude float,
@latitude float,
@accuracy real,
@date datetime

as

/* if location not exists */
if NOT EXISTS
(
	SELECT 1
	FROM MobileClientLocations
	WHERE UserId = @userid AND Imei = @imei AND [Date] = @date
)
begin
	/* insert location */
	INSERT INTO mobileclientlocations
		(userid, imei, workid, longitude, latitude, accuracy, [date])
	VALUES
		(@userid, @imei, @workid, @longitude, @latitude, @accuracy, @date)
end
return 0
' 
END
GO
/****** Object:  StoredProcedure [dbo].[M_InsertData]    Script Date: 10/03/2013 19:11:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[M_InsertData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'



CREATE PROCEDURE [dbo].[M_InsertData]
	@PhoneCallList dbo.MobilePhoneCallTableType READONLY
	,@WorkItemList dbo.MobileWorkItemTableType READONLY
	,@LocationList dbo.MobileLocationTableType READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	/* tables to return client-side ids */
	declare @ackphonecallids table(oldCallId int, newCallId bigint)
	declare @ackworkitemids table(workItemId int) 
	declare @acklocationids table(locationId int) 
	
	declare @pcId int
	declare @pcUserId int
	declare @pcPhoneNumber varchar(20)
	declare @pcStartDate datetime
	declare @pcEndDate datetime
	declare @pcIsInbound bit
	
	/* cursor to iterate over phonecall list */
	declare phoneCallCursor cursor for
		SELECT Id, UserId, PhoneNumber, StartDate, EndDate, IsInbound FROM @PhoneCallList
	
	open phoneCallCursor
	fetch phoneCallCursor into @pcId, @pcUserId, @pcPhoneNumber, @pcStartDate, @pcEndDate, @pcIsInbound 
	
	/* insert phone calls and get old-new phonecallid pairs */
	while @@FETCH_STATUS = 0
	begin	
		declare @newCallId bigint
		
		EXEC [dbo].[M_InsertPhoneCall]
			@userid = @pcUserId,
			@phonenumber = @pcPhoneNumber,
			@startdate = @pcStartDate,
			@enddate = @pcEndDate,
			@isinbound = @pcIsInbound,
			@phonecallid = @newCallId OUTPUT
			
		INSERT INTO @ackphonecallids (oldCallId, newCallId)
		VALUES (@pcId, @newCallId)	
		
		fetch phoneCallCursor into @pcId, @pcUserId, @pcPhoneNumber, @pcStartDate, @pcEndDate, @pcIsInbound
	end
	
	close phoneCallCursor
	deallocate phoneCallCursor
	
	/* select work item list into a writable table  */
	declare @writableWorkItemList table(Id int, UserID int, Imei nvarchar(20), WorkId int, SessionId uniqueidentifier, CallId int, StartDate datetime, EndDate datetime, ReceiveDate datetime)
	INSERT INTO @writableWorkItemList
		SELECT * 
			FROM @WorkItemList
	
	/* update work item list with new callids */
	UPDATE @writableWorkItemList
	SET CallId = P.newCallId
	FROM @writableWorkItemList W 
	INNER JOIN @ackphonecallids P ON W.CallId = P.oldCallId
	
	/* work item check, insert or update workitem? */
	MERGE INTO MobileWorkItems M
	USING @writableWorkItemList W
	ON M.SessionId = W.SessionId
	WHEN MATCHED AND W.StartDate <= M.EndDate AND W.EndDate > M.EndDate THEN
		UPDATE SET M.LastReceiveDate = W.ReceiveDate
				  ,M.EndDate = W.EndDate
	WHEN NOT MATCHED THEN
		INSERT (UserId, Imei, WorkId, SessionId, CallId, StartDate, EndDate, FirstReceiveDate, LastReceiveDate)
		VALUES (W.UserId, W.Imei, W.WorkId, W.SessionId, W.CallId, W.StartDate, W.EndDate, W.ReceiveDate, W.ReceiveDate)
	OUTPUT W.Id INTO @ackworkitemids;
	
	declare @locationId int
	declare @locationUserId int
	declare @locationImei bigint
	declare @locationWorkId int
	declare @locationLatitude float
	declare @locationLongitude float
	declare @locationAccuracy real
	declare @locationDate datetime
		
	/* cursor to iterate over location list */
	declare locationCursor cursor for
		SELECT Id, UserId, Imei, WorkId, Latitude, Longitude, Accuracy, [Date] FROM @LocationList
		
	open locationCursor
	fetch locationCursor into @locationId, @locationUserId, @locationImei, @locationWorkId, @locationLatitude, @locationLongitude, @locationAccuracy, @locationDate	
	
	/* insert location and get location ids */
	while @@FETCH_STATUS = 0
	begin
		EXEC [dbo].[M_InsertLocation]
			@userid = @locationUserId,
			@workid = @locationWorkId,
			@imei = @locationImei,
			@longitude = @locationLongitude,
			@latitude = @locationLatitude,
			@accuracy = @locationAccuracy,
			@date = @locationDate
		
		INSERT INTO @acklocationids(locationId)
		VALUES (@locationId)
		
		fetch locationCursor into @locationId, @locationUserId, @locationImei, @locationWorkId, @locationLatitude, @locationLongitude, @locationAccuracy, @locationDate
	end
	
	close  locationCursor
	deallocate locationCursor

	SELECT oldCallId FROM @ackphonecallids
	SELECT workItemId FROM @ackworkitemids
	SELECT locationId FROM @acklocationids
END

' 
END
GO
/****** Object:  Default [DF_MobileClientLocations_CreateDate]    Script Date: 10/03/2013 19:11:30 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_MobileClientLocations_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[MobileClientLocations]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_MobileClientLocations_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[MobileClientLocations] ADD  CONSTRAINT [DF_MobileClientLocations_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
