/****** Object:  StoredProcedure [dbo].[Client_GetUserDataById]    Script Date: 2015.07.20. 12:41:09 ******/
DROP PROCEDURE [dbo].[Client_GetUserDataById]
GO

/****** Object:  StoredProcedure [dbo].[Client_GetUserDataById]    Script Date: 2015.07.20. 12:41:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[Client_GetUserDataById]
	@userID int
AS
BEGIN

	SELECT DISTINCT u.Id
		,case when u.status <> 2 then u.Email else null end as Email
		,dbo.ConcatenateEmailsForUser(u.Id) as AggregateEmail
		,RTRIM(LTRIM(u.LastName + ' ' + u.FirstName)) as Name
		,u.LastName
		,u.FirstName
		,tz.TimeZoneData
		,u.EndOfDayMinutes
		,ues.CalendarId
		,case when us.IsWorktimeReportNeeded = 1 then us.WorktimeReportFrequency else 0 end as ReportFreqType
		,u.CultureId
		,CAST(ues.TargetWorkTimeInMinutes as int) as TargetWorkTimeInMinutes
		,ISNULL(u.FirstWorktime, u.CreatedOn) as FirstWorktime
	FROM dbo.[User] u
	join dbo.UserEffectiveSettings ues ON ues.UserId = u.Id
	join dbo.TimeZones tz on tz.Id = ues.TimeZoneId
	join dbo.UserSettings us on us.UserId = u.Id
	WHERE u.Id = @userID -- status 2: deleted; Type 8: Worker
	
	return 0
	
END


GO


/****** Object:  StoredProcedure [dbo].[Client_GetUsersData]    Script Date: 2015.07.20. 12:41:35 ******/
DROP PROCEDURE [dbo].[Client_GetUsersData]
GO

/****** Object:  StoredProcedure [dbo].[Client_GetUsersData]    Script Date: 2015.07.20. 12:41:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Client_GetUsersData]
AS
BEGIN
	SELECT DISTINCT u.Id
		,case when u.status <> 2 then u.Email else null end as Email
		,RTRIM(LTRIM(u.LastName + ' ' + u.FirstName)) as Name
		,u.LastName
		,u.FirstName
		,tz.TimeZoneData
		,u.EndOfDayMinutes
		,ues.CalendarId
		,case when us.IsWorktimeReportNeeded = 1 then us.WorktimeReportFrequency else 0 end as ReportFreqType
		,u.CultureId
		,CAST(ues.TargetWorkTimeInMinutes as int) as TargetWorkTimeInMinutes
		,ISNULL(u.FirstWorktime, u.CreatedOn) as FirstWorktime
	FROM dbo.[User] u
	join dbo.UserEffectiveSettings ues ON ues.UserId = u.Id
	join dbo.TimeZones tz on tz.Id = ues.TimeZoneId
	join dbo.UserSettings us on us.UserId = u.Id
	
	return 0

END


GO

