/****** Object:  ForeignKey [FK_ClientNotifications_NotificationForms]    Script Date: 04/14/2014 10:26:43 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClientNotifications_NotificationForms]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
ALTER TABLE [dbo].[ClientNotifications] DROP CONSTRAINT [FK_ClientNotifications_NotificationForms]
GO
/****** Object:  Table [dbo].[ClientNotifications]    Script Date: 04/14/2014 10:26:43 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientNotifications]') AND type in (N'U'))
DROP TABLE [dbo].[ClientNotifications]
GO
/****** Object:  Table [dbo].[NotificationForms]    Script Date: 04/14/2014 10:26:43 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationForms]') AND type in (N'U'))
DROP TABLE [dbo].[NotificationForms]
GO
/****** Object:  Default [DF__ClientNotifications_CreateDate]    Script Date: 04/14/2014 10:26:43 ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientNotifications_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientNotifications_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientNotifications] DROP CONSTRAINT [DF__ClientNotifications_CreateDate]
END


End
GO
/****** Object:  Default [DF__NotificationForms_CreateDate]    Script Date: 04/14/2014 10:26:43 ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__NotificationForms_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[NotificationForms]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__NotificationForms_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[NotificationForms] DROP CONSTRAINT [DF__NotificationForms_CreateDate]
END


End
GO
/****** Object:  Table [dbo].[NotificationForms]    Script Date: 04/14/2014 10:26:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationForms]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NotificationForms](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CompanyId] [int] NOT NULL,
	[Data] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Description] [nvarchar](2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WorkId] [int] NULL,
	[CreatedBy] [int] NULL,
	[CreateDate] [datetime] NOT NULL,
	[DeleteDate] [datetime] NULL,
 CONSTRAINT [PK__NotificationForms_Id_Clust] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[ClientNotifications]    Script Date: 04/14/2014 10:26:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientNotifications]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClientNotifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[FormId] [int] NOT NULL,
	[CreatedBy] [int] NULL,
	[CreateDate] [datetime] NOT NULL,
	[SendDate] [datetime] NULL,
	[ReceiveDate] [datetime] NULL,
	[ShowDate] [datetime] NULL,
	[ConfirmDate] [datetime] NULL,
	[Result] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DeviceId] [bigint] NULL,
 CONSTRAINT [PK__ClientNotifications_Id_Clust] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ClientNotifications]') AND name = N'IX_ClientNotifications_UserId_ReceiveDate_Filtered')
CREATE NONCLUSTERED INDEX [IX_ClientNotifications_UserId_ReceiveDate_Filtered] ON [dbo].[ClientNotifications] 
(
	[UserId] ASC,
	[ReceiveDate] ASC
)
WHERE ([ReceiveDate] IS NULL)
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Default [DF__ClientNotifications_CreateDate]    Script Date: 04/14/2014 10:26:43 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientNotifications_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientNotifications_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientNotifications] ADD  CONSTRAINT [DF__ClientNotifications_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF__NotificationForms_CreateDate]    Script Date: 04/14/2014 10:26:43 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__NotificationForms_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[NotificationForms]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__NotificationForms_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[NotificationForms] ADD  CONSTRAINT [DF__NotificationForms_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  ForeignKey [FK_ClientNotifications_NotificationForms]    Script Date: 04/14/2014 10:26:43 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClientNotifications_NotificationForms]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
ALTER TABLE [dbo].[ClientNotifications]  WITH CHECK ADD  CONSTRAINT [FK_ClientNotifications_NotificationForms] FOREIGN KEY([FormId])
REFERENCES [dbo].[NotificationForms] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClientNotifications_NotificationForms]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
ALTER TABLE [dbo].[ClientNotifications] CHECK CONSTRAINT [FK_ClientNotifications_NotificationForms]
GO
