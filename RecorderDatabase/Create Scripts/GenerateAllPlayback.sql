/****** Object:  Table [dbo].[PlaybackSchedules]    Script Date: 09/05/2013 11:35:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PlaybackSchedules]') AND type in (N'U'))
DROP TABLE [dbo].[PlaybackSchedules]
GO
/****** Object:  Default [DF_PlaybackSchedules_CreateDate]    Script Date: 09/05/2013 11:35:59 ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_PlaybackSchedules_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[PlaybackSchedules]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_PlaybackSchedules_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[PlaybackSchedules] DROP CONSTRAINT [DF_PlaybackSchedules_CreateDate]
END


End
GO
/****** Object:  Table [dbo].[PlaybackSchedules]    Script Date: 09/05/2013 11:35:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PlaybackSchedules]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PlaybackSchedules](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[FirstScheduleDate] [datetime] NOT NULL,
	[LastScheduleDate] [datetime] NULL,
	[TimeZoneId] [varchar](32) COLLATE Hungarian_CI_AS NULL,
	[ScheduleType] [tinyint] NOT NULL,
	[Interval] [int] NULL,
	[WeeklyEffectiveDays] [tinyint] NULL,
	[FirstDayOfWeek] [tinyint] NULL,
	[MonthlyEffectiveDay] [tinyint] NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_PlaybackSchedules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Default [DF_PlaybackSchedules_CreateDate]    Script Date: 09/05/2013 11:35:59 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_PlaybackSchedules_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[PlaybackSchedules]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_PlaybackSchedules_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[PlaybackSchedules] ADD  CONSTRAINT [DF_PlaybackSchedules_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
