/****** Object:  ForeignKey [FK_CalendarExceptions_Calendar] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CalendarExceptions_Calendar]') AND parent_object_id = OBJECT_ID(N'[dbo].[CalendarExceptions]'))
ALTER TABLE [dbo].[CalendarExceptions] DROP CONSTRAINT [FK_CalendarExceptions_Calendar]
GO
/****** Object:  ForeignKey [FK_Calendars_Calendars] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Calendars_Calendars]') AND parent_object_id = OBJECT_ID(N'[dbo].[Calendars]'))
ALTER TABLE [dbo].[Calendars] DROP CONSTRAINT [FK_Calendars_Calendars]
GO
/****** Object:  ForeignKey [FK_ClientNotifications_NotificationForms] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClientNotifications_NotificationForms]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
ALTER TABLE [dbo].[ClientNotifications] DROP CONSTRAINT [FK_ClientNotifications_NotificationForms]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_DesktopCaptures] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_DesktopCaptures]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] DROP CONSTRAINT [FK_DesktopActiveWindows_DesktopCaptures]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_DesktopWindows] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_DesktopWindows]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] DROP CONSTRAINT [FK_DesktopActiveWindows_DesktopWindows]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_ProcessNameLookup] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_ProcessNameLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] DROP CONSTRAINT [FK_DesktopActiveWindows_ProcessNameLookup]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_TitleLookup] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_TitleLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] DROP CONSTRAINT [FK_DesktopActiveWindows_TitleLookup]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_UrlLookup] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_UrlLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] DROP CONSTRAINT [FK_DesktopActiveWindows_UrlLookup]
GO
/****** Object:  ForeignKey [FK_DesktopCaptures_WorkItems] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopCaptures_WorkItems]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopCaptures]'))
ALTER TABLE [dbo].[DesktopCaptures] DROP CONSTRAINT [FK_DesktopCaptures_WorkItems]
GO
/****** Object:  ForeignKey [FK_DesktopWindows_DesktopCaptures] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_DesktopCaptures]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows] DROP CONSTRAINT [FK_DesktopWindows_DesktopCaptures]
GO
/****** Object:  ForeignKey [FK_DesktopWindows_ProcessNameLookup] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_ProcessNameLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows] DROP CONSTRAINT [FK_DesktopWindows_ProcessNameLookup]
GO
/****** Object:  ForeignKey [FK_DesktopWindows_TitleLookup] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_TitleLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows] DROP CONSTRAINT [FK_DesktopWindows_TitleLookup]
GO
/****** Object:  ForeignKey [FK_DesktopWindows_UrlLookup] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_UrlLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows] DROP CONSTRAINT [FK_DesktopWindows_UrlLookup]
GO
/****** Object:  ForeignKey [FK_IvrLocations_IvrWorkItems] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IvrLocations_IvrWorkItems]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrLocations]'))
ALTER TABLE [dbo].[IvrLocations] DROP CONSTRAINT [FK_IvrLocations_IvrWorkItems]
GO
/****** Object:  ForeignKey [FK_IvrRules_IvrCustomRules] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IvrRules_IvrCustomRules]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrRules]'))
ALTER TABLE [dbo].[IvrRules] DROP CONSTRAINT [FK_IvrRules_IvrCustomRules]
GO
/****** Object:  ForeignKey [FK_IvrUserWorks_IvrRules] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IvrUserWorks_IvrRules]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrUserWorks]'))
ALTER TABLE [dbo].[IvrUserWorks] DROP CONSTRAINT [FK_IvrUserWorks_IvrRules]
GO
/****** Object:  ForeignKey [FK_ManualWorkItems_ManualWorkItemSource] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ManualWorkItems_ManualWorkItemSource]') AND parent_object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]'))
ALTER TABLE [dbo].[ManualWorkItems] DROP CONSTRAINT [FK_ManualWorkItems_ManualWorkItemSource]
GO
/****** Object:  ForeignKey [FK_ManualWorkItems_ManualWorkItemTypes] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ManualWorkItems_ManualWorkItemTypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]'))
ALTER TABLE [dbo].[ManualWorkItems] DROP CONSTRAINT [FK_ManualWorkItems_ManualWorkItemTypes]
GO
/****** Object:  ForeignKey [FK_ParallelWorkItems_ParallelWorkItemTypes] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ParallelWorkItems_ParallelWorkItemTypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[ParallelWorkItems]'))
ALTER TABLE [dbo].[ParallelWorkItems] DROP CONSTRAINT [FK_ParallelWorkItems_ParallelWorkItemTypes]
GO
/****** Object:  ForeignKey [FK_Screens_DesktopCaptures] ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Screens_DesktopCaptures]') AND parent_object_id = OBJECT_ID(N'[dbo].[Screens]'))
ALTER TABLE [dbo].[Screens] DROP CONSTRAINT [FK_Screens_DesktopCaptures]
GO
/****** Object:  Check [CK_UsageStats_LocalDate_DateOnly] ******/
IF  EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_UsageStats_LocalDate_DateOnly]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
BEGIN
IF  EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_UsageStats_LocalDate_DateOnly]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
ALTER TABLE [dbo].[UsageStats] DROP CONSTRAINT [CK_UsageStats_LocalDate_DateOnly]

END
GO
/****** Object:  Check [CK_UsageStats_StartDate_Less_Than_EndDate] ******/
IF  EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_UsageStats_StartDate_Less_Than_EndDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
BEGIN
IF  EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_UsageStats_StartDate_Less_Than_EndDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
ALTER TABLE [dbo].[UsageStats] DROP CONSTRAINT [CK_UsageStats_StartDate_Less_Than_EndDate]

END
GO
/****** Object:  View [dbo].[ActiveWindows] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ActiveWindows]'))
DROP VIEW [dbo].[ActiveWindows]
GO
/****** Object:  StoredProcedure [dbo].[InsertDesktopWindow] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertDesktopWindow]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertDesktopWindow]
GO
/****** Object:  UserDefinedFunction [dbo].[IsWorkDay] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsWorkDay]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IsWorkDay]
GO
/****** Object:  StoredProcedure [dbo].[GetActiveWindowsGrouppedForUser] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetActiveWindowsGrouppedForUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetActiveWindowsGrouppedForUser]
GO
/****** Object:  Table [dbo].[DesktopActiveWindows] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]') AND type in (N'U'))
DROP TABLE [dbo].[DesktopActiveWindows]
GO
/****** Object:  View [dbo].[ScreenShots] ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ScreenShots]'))
DROP VIEW [dbo].[ScreenShots]
GO
/****** Object:  Table [dbo].[Screens] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Screens]') AND type in (N'U'))
DROP TABLE [dbo].[Screens]
GO
/****** Object:  Table [dbo].[DesktopWindows] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DesktopWindows]') AND type in (N'U'))
DROP TABLE [dbo].[DesktopWindows]
GO
/****** Object:  UserDefinedFunction [dbo].[GetFlattenedCalendarExceptions] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFlattenedCalendarExceptions]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetFlattenedCalendarExceptions]
GO
/****** Object:  Table [dbo].[ClientSettings] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientSettings]') AND type in (N'U'))
DROP TABLE [dbo].[ClientSettings]
GO
/****** Object:  Table [dbo].[IvrUserWorks] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrUserWorks]') AND type in (N'U'))
DROP TABLE [dbo].[IvrUserWorks]
GO
/****** Object:  StoredProcedure [dbo].[UpdateHourlyAggregateWorkItems] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateHourlyAggregateWorkItems]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateHourlyAggregateWorkItems]
GO
/****** Object:  StoredProcedure [dbo].[UpdateHourlyAggregateWorkItemsFromId] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateHourlyAggregateWorkItemsFromId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateHourlyAggregateWorkItemsFromId]
GO
/****** Object:  StoredProcedure [dbo].[UpdateIvrWorkItemsWithComputerActivity] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateIvrWorkItemsWithComputerActivity]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateIvrWorkItemsWithComputerActivity]
GO
/****** Object:  StoredProcedure [dbo].[UpsertClientComputerError] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpsertClientComputerError]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpsertClientComputerError]
GO
/****** Object:  StoredProcedure [dbo].[UpsertVoiceRecordings] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpsertVoiceRecordings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpsertVoiceRecordings]
GO
/****** Object:  Table [dbo].[ParallelWorkItems] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ParallelWorkItems]') AND type in (N'U'))
DROP TABLE [dbo].[ParallelWorkItems]
GO
/****** Object:  StoredProcedure [dbo].[ReportClientComputerAddress] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReportClientComputerAddress]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReportClientComputerAddress]
GO
/****** Object:  StoredProcedure [dbo].[ReportClientComputerVersion] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReportClientComputerVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReportClientComputerVersion]
GO
/****** Object:  StoredProcedure [dbo].[GetTotalWorkTimeByWorkIdForUser] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTotalWorkTimeByWorkIdForUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetTotalWorkTimeByWorkIdForUser]
GO
/****** Object:  StoredProcedure [dbo].[GetWorkTimeByWorkIdForUser] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetWorkTimeByWorkIdForUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetWorkTimeByWorkIdForUser]
GO
/****** Object:  Table [dbo].[IvrLocations] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrLocations]') AND type in (N'U'))
DROP TABLE [dbo].[IvrLocations]
GO
/****** Object:  Table [dbo].[IvrRules] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrRules]') AND type in (N'U'))
DROP TABLE [dbo].[IvrRules]
GO
/****** Object:  StoredProcedure [dbo].[GetIvrWorkTimeByWorkIdForUser] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIvrWorkTimeByWorkIdForUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetIvrWorkTimeByWorkIdForUser]
GO
/****** Object:  StoredProcedure [dbo].[MergeAggregateIdleIntervals] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MergeAggregateIdleIntervals]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MergeAggregateIdleIntervals]
GO
/****** Object:  StoredProcedure [dbo].[MergeAggregateWorkItemIntervals] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MergeAggregateWorkItemIntervals]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MergeAggregateWorkItemIntervals]
GO
/****** Object:  Table [dbo].[ClientNotifications] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientNotifications]') AND type in (N'U'))
DROP TABLE [dbo].[ClientNotifications]
GO
/****** Object:  Table [dbo].[CalendarExceptions] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CalendarExceptions]') AND type in (N'U'))
DROP TABLE [dbo].[CalendarExceptions]
GO
/****** Object:  StoredProcedure [dbo].[ClientComputerKickSend] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerKickSend]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ClientComputerKickSend]
GO
/****** Object:  StoredProcedure [dbo].[ClientComputerKickConfirm] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerKickConfirm]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ClientComputerKickConfirm]
GO
/****** Object:  StoredProcedure [dbo].[GetIdForProcessName] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIdForProcessName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetIdForProcessName]
GO
/****** Object:  StoredProcedure [dbo].[GetIdForTitle] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIdForTitle]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetIdForTitle]
GO
/****** Object:  StoredProcedure [dbo].[GetIdForUrl] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIdForUrl]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetIdForUrl]
GO
/****** Object:  UserDefinedFunction [dbo].[GetInheritedCalendarIds] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetInheritedCalendarIds]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetInheritedCalendarIds]
GO
/****** Object:  StoredProcedure [dbo].[GetMobileWorkTimeByWorkIdForUser] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMobileWorkTimeByWorkIdForUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMobileWorkTimeByWorkIdForUser]
GO
/****** Object:  StoredProcedure [dbo].[GetNextValueForSequence] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNextValueForSequence]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetNextValueForSequence]
GO
/****** Object:  StoredProcedure [dbo].[CommitUsageStatsToEcomm] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CommitUsageStatsToEcomm]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CommitUsageStatsToEcomm]
GO
/****** Object:  Table [dbo].[DesktopCaptures] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DesktopCaptures]') AND type in (N'U'))
DROP TABLE [dbo].[DesktopCaptures]
GO
/****** Object:  StoredProcedure [dbo].[DeleteVoiceRecordings] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteVoiceRecordings]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteVoiceRecordings]
GO
/****** Object:  Table [dbo].[Storages] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Storages]') AND type in (N'U'))
DROP TABLE [dbo].[Storages]
GO
/****** Object:  Table [dbo].[TitleLookup] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TitleLookup]') AND type in (N'U'))
DROP TABLE [dbo].[TitleLookup]
GO
/****** Object:  Table [dbo].[DeadLetterItems] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeadLetterItems]') AND type in (N'U'))
DROP TABLE [dbo].[DeadLetterItems]
GO
/****** Object:  Table [dbo].[EmailStats] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailStats]') AND type in (N'U'))
DROP TABLE [dbo].[EmailStats]
GO
/****** Object:  StoredProcedure [dbo].[GetSchemaVersion] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSchemaVersion]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSchemaVersion]
GO
/****** Object:  UserDefinedFunction [dbo].[GetIntersectDuration] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIntersectDuration]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetIntersectDuration]
GO
/****** Object:  StoredProcedure [dbo].[Client_SetWorkedUsersOnDay] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Client_SetWorkedUsersOnDay]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Client_SetWorkedUsersOnDay]
GO
/****** Object:  Table [dbo].[ClientComputerAddresses] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerAddresses]') AND type in (N'U'))
DROP TABLE [dbo].[ClientComputerAddresses]
GO
/****** Object:  Table [dbo].[ClientComputerErrors] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]') AND type in (N'U'))
DROP TABLE [dbo].[ClientComputerErrors]
GO
/****** Object:  Table [dbo].[ClientComputerInfo] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerInfo]') AND type in (N'U'))
DROP TABLE [dbo].[ClientComputerInfo]
GO
/****** Object:  Table [dbo].[AggregateIdleIntervals] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AggregateIdleIntervals]') AND type in (N'U'))
DROP TABLE [dbo].[AggregateIdleIntervals]
GO
/****** Object:  Table [dbo].[AggregateLastWorkItem] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AggregateLastWorkItem]') AND type in (N'U'))
DROP TABLE [dbo].[AggregateLastWorkItem]
GO
/****** Object:  StoredProcedure [dbo].[GetUsersForUsageStats] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUsersForUsageStats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUsersForUsageStats]
GO
/****** Object:  Table [dbo].[AggregateWorkItemIntervals] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AggregateWorkItemIntervals]') AND type in (N'U'))
DROP TABLE [dbo].[AggregateWorkItemIntervals]
GO
/****** Object:  Table [dbo].[AggregateWorkItems] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AggregateWorkItems]') AND type in (N'U'))
DROP TABLE [dbo].[AggregateWorkItems]
GO
/****** Object:  Table [dbo].[ClientComputerKicks] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerKicks]') AND type in (N'U'))
DROP TABLE [dbo].[ClientComputerKicks]
GO
/****** Object:  Table [dbo].[Calendars] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Calendars]') AND type in (N'U'))
DROP TABLE [dbo].[Calendars]
GO
/****** Object:  Table [dbo].[ClientComputerVersions] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerVersions]') AND type in (N'U'))
DROP TABLE [dbo].[ClientComputerVersions]
GO
/****** Object:  Table [dbo].[ActiveDevices] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActiveDevices]') AND type in (N'U'))
DROP TABLE [dbo].[ActiveDevices]
GO
/****** Object:  Table [dbo].[MobileWorkItems] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MobileWorkItems]') AND type in (N'U'))
DROP TABLE [dbo].[MobileWorkItems]
GO
/****** Object:  Table [dbo].[NotificationForms] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotificationForms]') AND type in (N'U'))
DROP TABLE [dbo].[NotificationForms]
GO
/****** Object:  Table [dbo].[IvrWorkItems] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrWorkItems]') AND type in (N'U'))
DROP TABLE [dbo].[IvrWorkItems]
GO
/****** Object:  StoredProcedure [dbo].[GetLearningRuleGenerators] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLearningRuleGenerators]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLearningRuleGenerators]
GO
/****** Object:  UserDefinedFunction [dbo].[GetDatePart] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDatePart]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetDatePart]
GO
/****** Object:  Table [dbo].[IvrCustomRules] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrCustomRules]') AND type in (N'U'))
DROP TABLE [dbo].[IvrCustomRules]
GO
/****** Object:  Table [dbo].[RowVersionSequence] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RowVersionSequence]') AND type in (N'U'))
DROP TABLE [dbo].[RowVersionSequence]
GO
/****** Object:  Table [dbo].[ParallelWorkItemTypes] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ParallelWorkItemTypes]') AND type in (N'U'))
DROP TABLE [dbo].[ParallelWorkItemTypes]
GO
/****** Object:  Table [dbo].[ProcessNameLookup] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProcessNameLookup]') AND type in (N'U'))
DROP TABLE [dbo].[ProcessNameLookup]
GO
/****** Object:  Table [dbo].[UrlLookup] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UrlLookup]') AND type in (N'U'))
DROP TABLE [dbo].[UrlLookup]
GO
/****** Object:  Table [dbo].[UsageStats] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UsageStats]') AND type in (N'U'))
DROP TABLE [dbo].[UsageStats]
GO
/****** Object:  Table [dbo].[VoiceRecordings] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VoiceRecordings]') AND type in (N'U'))
DROP TABLE [dbo].[VoiceRecordings]
GO
/****** Object:  Table [dbo].[WorkItems] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkItems]') AND type in (N'U'))
DROP TABLE [dbo].[WorkItems]
GO
/****** Object:  Table [dbo].[ManualWorkItems] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]') AND type in (N'U'))
DROP TABLE [dbo].[ManualWorkItems]
GO
/****** Object:  Table [dbo].[ManualWorkItemSource] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItemSource]') AND type in (N'U'))
DROP TABLE [dbo].[ManualWorkItemSource]
GO
/****** Object:  Table [dbo].[ManualWorkItemTypes] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItemTypes]') AND type in (N'U'))
DROP TABLE [dbo].[ManualWorkItemTypes]
GO
/****** Object:  Default [DF_AggregateIdleIntervals_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateIdleIntervals_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateIdleIntervals]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateIdleIntervals_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateIdleIntervals] DROP CONSTRAINT [DF_AggregateIdleIntervals_CreateDate]
END


End
GO
/****** Object:  Default [DF_AggregateIdleIntervals_UpdateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateIdleIntervals_UpdateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateIdleIntervals]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateIdleIntervals_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateIdleIntervals] DROP CONSTRAINT [DF_AggregateIdleIntervals_UpdateDate]
END


End
GO
/****** Object:  Default [DF_AggregateWorkItemIntervals_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateWorkItemIntervals_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateWorkItemIntervals]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateWorkItemIntervals_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateWorkItemIntervals] DROP CONSTRAINT [DF_AggregateWorkItemIntervals_CreateDate]
END


End
GO
/****** Object:  Default [DF_AggregateWorkItemIntervals_UpdateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateWorkItemIntervals_UpdateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateWorkItemIntervals]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateWorkItemIntervals_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateWorkItemIntervals] DROP CONSTRAINT [DF_AggregateWorkItemIntervals_UpdateDate]
END


End
GO
/****** Object:  Default [DF_AggregateWorkItems_UpdateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateWorkItems_UpdateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateWorkItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateWorkItems_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateWorkItems] DROP CONSTRAINT [DF_AggregateWorkItems_UpdateDate]
END


End
GO
/****** Object:  Default [DF_AggregateWorkItems_UpdateDate_1] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateWorkItems_UpdateDate_1]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateWorkItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateWorkItems_UpdateDate_1]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateWorkItems] DROP CONSTRAINT [DF_AggregateWorkItems_UpdateDate_1]
END


End
GO
/****** Object:  Default [DF_CalendarExceptions_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_CalendarExceptions_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[CalendarExceptions]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_CalendarExceptions_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[CalendarExceptions] DROP CONSTRAINT [DF_CalendarExceptions_CreateDate]
END


End
GO
/****** Object:  Default [DF_Calendar_CreateData] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_Calendar_CreateData]') AND parent_object_id = OBJECT_ID(N'[dbo].[Calendars]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Calendar_CreateData]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Calendars] DROP CONSTRAINT [DF_Calendar_CreateData]
END


End
GO
/****** Object:  Default [DF_ClientComputerAddresses_FirstReceiveDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerAddresses_FirstReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerAddresses]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerAddresses_FirstReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerAddresses] DROP CONSTRAINT [DF_ClientComputerAddresses_FirstReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerAddresses_LastReceiveDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerAddresses_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerAddresses]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerAddresses_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerAddresses] DROP CONSTRAINT [DF_ClientComputerAddresses_LastReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerErrors_FirstReceiveDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerErrors_FirstReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerErrors_FirstReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerErrors] DROP CONSTRAINT [DF_ClientComputerErrors_FirstReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerErrors_LastReceiveDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerErrors_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerErrors_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerErrors] DROP CONSTRAINT [DF_ClientComputerErrors_LastReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerVersions_ReceiveDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerVersions_ReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerVersions]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerVersions_ReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerVersions] DROP CONSTRAINT [DF_ClientComputerVersions_ReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerVersions_LastReceiveDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerVersions_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerVersions]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerVersions_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerVersions] DROP CONSTRAINT [DF_ClientComputerVersions_LastReceiveDate]
END


End
GO
/****** Object:  Default [DF__ClientNotifications_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientNotifications_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientNotifications_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientNotifications] DROP CONSTRAINT [DF__ClientNotifications_CreateDate]
END


End
GO
/****** Object:  Default [DF_ClientSettings_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientSettings_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientSettings_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] DROP CONSTRAINT [DF_ClientSettings_CreateDate]
END


End
GO
/****** Object:  Default [DF__ClientSet__MenuV__2DE6D218] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientSet__MenuV__2DE6D218]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientSet__MenuV__2DE6D218]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] DROP CONSTRAINT [DF__ClientSet__MenuV__2DE6D218]
END


End
GO
/****** Object:  Default [DF__ClientSet__WorkD__2EDAF651] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientSet__WorkD__2EDAF651]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientSet__WorkD__2EDAF651]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] DROP CONSTRAINT [DF__ClientSet__WorkD__2EDAF651]
END


End
GO
/****** Object:  Default [DF__ClientSet__Censo__2FCF1A8A] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientSet__Censo__2FCF1A8A]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientSet__Censo__2FCF1A8A]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] DROP CONSTRAINT [DF__ClientSet__Censo__2FCF1A8A]
END


End
GO
/****** Object:  Default [DF__ClientSet__Clien__30C33EC3] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientSet__Clien__30C33EC3]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientSet__Clien__30C33EC3]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] DROP CONSTRAINT [DF__ClientSet__Clien__30C33EC3]
END


End
GO
/****** Object:  Default [DF_DeadLetterItems_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_DeadLetterItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[DeadLetterItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_DeadLetterItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[DeadLetterItems] DROP CONSTRAINT [DF_DeadLetterItems_CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrCustomRules_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrCustomRules_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrCustomRules]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrCustomRules_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrCustomRules] DROP CONSTRAINT [DF_IvrCustomRules_CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrLocations_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrLocations_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrLocations]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrLocations_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrLocations] DROP CONSTRAINT [DF_IvrLocations_CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrRules_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrRules_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrRules]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrRules_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrRules] DROP CONSTRAINT [DF_IvrRules_CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrRules_InstantNotificationEmail] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrRules_InstantNotificationEmail]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrRules]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrRules_InstantNotificationEmail]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrRules] DROP CONSTRAINT [DF_IvrRules_InstantNotificationEmail]
END


End
GO
/****** Object:  Default [DF_IvrUserWorks_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrUserWorks_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrUserWorks]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrUserWorks_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrUserWorks] DROP CONSTRAINT [DF_IvrUserWorks_CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrWorkItems_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrWorkItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrWorkItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrWorkItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrWorkItems] DROP CONSTRAINT [DF_IvrWorkItems_CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrWorkItems_InstantNotificationEmail] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrWorkItems_InstantNotificationEmail]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrWorkItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrWorkItems_InstantNotificationEmail]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrWorkItems] DROP CONSTRAINT [DF_IvrWorkItems_InstantNotificationEmail]
END


End
GO
/****** Object:  Default [DF_ManualWorkItems_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ManualWorkItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ManualWorkItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ManualWorkItems] DROP CONSTRAINT [DF_ManualWorkItems_CreateDate]
END


End
GO
/****** Object:  Default [DF__NotificationForms_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__NotificationForms_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[NotificationForms]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__NotificationForms_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[NotificationForms] DROP CONSTRAINT [DF__NotificationForms_CreateDate]
END


End
GO
/****** Object:  Default [DF_ParallelWorkItems_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ParallelWorkItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ParallelWorkItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ParallelWorkItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ParallelWorkItems] DROP CONSTRAINT [DF_ParallelWorkItems_CreateDate]
END


End
GO
/****** Object:  Default [DF_Storages_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_Storages_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[Storages]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Storages_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Storages] DROP CONSTRAINT [DF_Storages_CreateDate]
END


End
GO
/****** Object:  Default [DF_UsageStats_IsAcked] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_IsAcked]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_IsAcked]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] DROP CONSTRAINT [DF_UsageStats_IsAcked]
END


End
GO
/****** Object:  Default [DF_UsageStats_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] DROP CONSTRAINT [DF_UsageStats_CreateDate]
END


End
GO
/****** Object:  Default [DF_UsageStats_UpdateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_UpdateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] DROP CONSTRAINT [DF_UsageStats_UpdateDate]
END


End
GO
/****** Object:  Default [DF_UsageStats_IvrWorkTime] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_IvrWorkTime]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_IvrWorkTime]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] DROP CONSTRAINT [DF_UsageStats_IvrWorkTime]
END


End
GO
/****** Object:  Default [DF_UsageStats_MobileWorkTime] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_MobileWorkTime]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_MobileWorkTime]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] DROP CONSTRAINT [DF_UsageStats_MobileWorkTime]
END


End
GO
/****** Object:  Default [DF_UsageStats_ManuallyAddedWorkTime] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_ManuallyAddedWorkTime]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_ManuallyAddedWorkTime]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] DROP CONSTRAINT [DF_UsageStats_ManuallyAddedWorkTime]
END


End
GO
/****** Object:  Default [DF_VoiceRecordings_ReceiveDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_VoiceRecordings_ReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[VoiceRecordings]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_VoiceRecordings_ReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[VoiceRecordings] DROP CONSTRAINT [DF_VoiceRecordings_ReceiveDate]
END


End
GO
/****** Object:  Default [DF_VoiceRecordings_LastReceiveDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_VoiceRecordings_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[VoiceRecordings]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_VoiceRecordings_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[VoiceRecordings] DROP CONSTRAINT [DF_VoiceRecordings_LastReceiveDate]
END


End
GO
/****** Object:  Default [DF_WorkItems_CreateDate] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_WorkItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[WorkItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_WorkItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[WorkItems] DROP CONSTRAINT [DF_WorkItems_CreateDate]
END


End
GO
/****** Object:  Default [DF_WorkItems_IsRemoteDesktop] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_WorkItems_IsRemoteDesktop]') AND parent_object_id = OBJECT_ID(N'[dbo].[WorkItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_WorkItems_IsRemoteDesktop]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[WorkItems] DROP CONSTRAINT [DF_WorkItems_IsRemoteDesktop]
END


End
GO
/****** Object:  Default [DF_WorkItems_IsVirtualMachine] ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_WorkItems_IsVirtualMachine]') AND parent_object_id = OBJECT_ID(N'[dbo].[WorkItems]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_WorkItems_IsVirtualMachine]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[WorkItems] DROP CONSTRAINT [DF_WorkItems_IsVirtualMachine]
END


End
GO
/****** Object:  Table [dbo].[ManualWorkItemTypes] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItemTypes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ManualWorkItemTypes](
	[Id] [smallint] NOT NULL,
	[Name] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[IsWorkIdRequired] [bit] NOT NULL,
	[Description] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_ManualWorkItemTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[ManualWorkItemSource] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItemSource]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ManualWorkItemSource](
	[SourceId] [tinyint] NOT NULL,
	[Name] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_ManualWorkItemSource] PRIMARY KEY CLUSTERED 
(
	[SourceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[ManualWorkItems] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ManualWorkItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ManualWorkItemTypeId] [smallint] NOT NULL,
	[WorkId] [int] NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[CompanyId] [int] NOT NULL,
	[Comment] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreatedBy] [int] NULL,
	[SourceId] [tinyint] NULL,
 CONSTRAINT [PK_ManualWorkItems] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]') AND name = N'IX_ManualWorkItems_StartDateClust')
CREATE CLUSTERED INDEX [IX_ManualWorkItems_StartDateClust] ON [dbo].[ManualWorkItems] 
(
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]') AND name = N'IX_ManualWorkItems_DatesForOR')
CREATE NONCLUSTERED INDEX [IX_ManualWorkItems_DatesForOR] ON [dbo].[ManualWorkItems] 
(
	[EndDate] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Trigger [ManualWorkItemTypes_Validation] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItemTypes_Validation]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[ManualWorkItemTypes_Validation]
ON [dbo].[ManualWorkItemTypes]
FOR UPDATE --INSERT check is not needed
AS
BEGIN
	SET NOCOUNT ON;
	--WorkId cannot be NULL when IsWorkIdRequired is true
	IF EXISTS (	
		SELECT 1 FROM inserted i
		JOIN ManualWorkItems m ON i.Id = m.ManualWorkItemTypeId
		WHERE 
			m.WorkId IS NULL
			AND i.IsWorkIdRequired = 1
	)
	BEGIN
		RAISERROR(''WorkId cannot be NULL when IsWorkIdRequired is true'',16,1)
		ROLLBACK
		RETURN
	END
END
'
GO
/****** Object:  Trigger [ManualWorkItems_Validation] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[ManualWorkItems_Validation]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[ManualWorkItems_Validation]
ON [dbo].[ManualWorkItems]
FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	--WorkId cannot be NULL when IsWorkIdRequired is true
	IF EXISTS (	
		SELECT 1 FROM inserted i
		JOIN ManualWorkItemTypes t ON t.Id = i.ManualWorkItemTypeId
		WHERE 
			i.WorkId IS NULL
			AND t.IsWorkIdRequired = 1
	)
	BEGIN
		RAISERROR(''WorkId cannot be NULL when IsWorkIdRequired is true'',16,1)
		ROLLBACK
		RETURN
	END
	
	--EndDate should be greater than or equal to StartDate
	IF EXISTS (	
		SELECT 1 FROM inserted i
		WHERE
			i.EndDate < i.StartDate
	)
	BEGIN
		RAISERROR(''EndDate should be greater than or equal to StartDate'',16,1)
		ROLLBACK
		RETURN
	END
	
	--WorkId should be NULL when IsWorkIdRequired is false (as we don''t handle deletion by WorkId atm.)
	IF EXISTS (	
		SELECT 1 FROM inserted i
		JOIN ManualWorkItemTypes t ON t.Id = i.ManualWorkItemTypeId
		WHERE 
			i.WorkId IS NOT NULL
			AND t.IsWorkIdRequired = 0
	)
	BEGIN
		RAISERROR(''WorkId should be NULL when IsWorkIdRequired is false'',16,1)
		ROLLBACK
		RETURN
	END
END
'
GO
/****** Object:  Table [dbo].[WorkItems] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkItems]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[WorkItems](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[WorkId] [int] NOT NULL,
	[PhaseId] [uniqueidentifier] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[ReceiveDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[CompanyId] [int] NOT NULL,
	[ComputerId] [int] NOT NULL,
	[MouseActivity] [int] NOT NULL,
	[KeyboardActivity] [int] NOT NULL,
	[IsRemoteDesktop] [bit] NOT NULL,
	[IsVirtualMachine] [bit] NOT NULL,
 CONSTRAINT [PK_WorkItems] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON),
 CONSTRAINT [IX_WorkItems_Unique] UNIQUE NONCLUSTERED 
(
	[StartDate] ASC,
	[PhaseId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkItems]') AND name = N'IX_WorkItems_StartDateClust')
CREATE CLUSTERED INDEX [IX_WorkItems_StartDateClust] ON [dbo].[WorkItems] 
(
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[VoiceRecordings] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VoiceRecordings]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[VoiceRecordings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[WorkId] [int] NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NULL,
	[Duration] [int] NOT NULL,
	[Codec] [int] NOT NULL,
	[Name] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Extension] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[FirstReceiveDate] [datetime] NOT NULL,
	[LastReceiveDate] [datetime] NOT NULL,
	[Offset] [int] NOT NULL,
	[DeleteDate] [datetime] NULL,
 CONSTRAINT [PK_VoiceRecordings] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[VoiceRecordings]') AND name = N'IX_VoiceRecordings_UserId_StartDate_Clust')
CREATE CLUSTERED INDEX [IX_VoiceRecordings_UserId_StartDate_Clust] ON [dbo].[VoiceRecordings] 
(
	[UserId] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[VoiceRecordings]') AND name = N'IX_VoiceRecordings_ClientId')
CREATE UNIQUE NONCLUSTERED INDEX [IX_VoiceRecordings_ClientId] ON [dbo].[VoiceRecordings] 
(
	[ClientId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[UsageStats] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UsageStats]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UsageStats](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LocalDate] [datetime] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[ComputerWorkTime] [int] NOT NULL,
	[IsAcked] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[UpdateDate] [datetime] NOT NULL,
	[IvrWorkTime] [int] NOT NULL,
	[MobileWorkTime] [int] NOT NULL,
	[ManuallyAddedWorkTime] [int] NOT NULL,
 CONSTRAINT [PK_UsageStats] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON),
 CONSTRAINT [IX_UsageStats_UserId_LocalDate_Unique] UNIQUE NONCLUSTERED 
(
	[UserId] ASC,
	[LocalDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[UrlLookup] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UrlLookup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UrlLookup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HashCode] [int] NOT NULL,
	[Url] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_UrlLookup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[UrlLookup]') AND name = N'IX_UrlLookup_HashCode')
CREATE NONCLUSTERED INDEX [IX_UrlLookup_HashCode] ON [dbo].[UrlLookup] 
(
	[HashCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[ProcessNameLookup] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProcessNameLookup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ProcessNameLookup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HashCode] [int] NOT NULL,
	[ProcessName] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_ProcessNameLookup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProcessNameLookup]') AND name = N'IX_ProcessNameLookup_HashCode')
CREATE NONCLUSTERED INDEX [IX_ProcessNameLookup_HashCode] ON [dbo].[ProcessNameLookup] 
(
	[HashCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[ParallelWorkItemTypes] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ParallelWorkItemTypes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ParallelWorkItemTypes](
	[Id] [smallint] NOT NULL,
	[Name] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Description] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_ParallelWorkItemTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[RowVersionSequence] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RowVersionSequence]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[RowVersionSequence](
	[IsDummy] [bit] NOT NULL,
	[Version] [timestamp] NOT NULL,
 CONSTRAINT [PK__RowVersi__4251BB7C339FAB6E] PRIMARY KEY CLUSTERED 
(
	[IsDummy] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[IvrCustomRules] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrCustomRules]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IvrCustomRules](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Method] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Description] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Settings] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_IvrCustomRules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetDatePart] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDatePart]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE FUNCTION [dbo].[GetDatePart] 
(
	@date datetime
)
RETURNS datetime
WITH RETURNS NULL ON NULL INPUT
AS
BEGIN
	RETURN DATEADD(day, DATEDIFF(day, 0, @date), 0)
END
' 
END
GO
/****** Object:  StoredProcedure [dbo].[GetLearningRuleGenerators] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLearningRuleGenerators]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetLearningRuleGenerators]
	(
	@userId int,
	@oldVersion nvarchar(MAX),
	@newVersion nvarchar(MAX) OUTPUT
	)
AS
	SET NOCOUNT ON

	IF @userId IS NULL
	BEGIN
		RAISERROR(''@userId cannot be NULL'', 16, 1)
		RETURN
	END

	declare @result nvarchar(MAX)

	SET @newVersion = ''4''

	IF (@oldVersion IS NULL OR @oldVersion <> @newVersion)
	BEGIN
		SET @result = ''<?xml version="1.0" encoding="utf-8"?>
<ArrayOfRuleGeneratorData xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://jobctrl.com/">
  <RuleGeneratorData>
    <Name>IgnoreRuleGenerator</Name>
    <Parameters>{"IgnoreCase":true,"ProcessNamePattern":{"MatchingPattern":"^(winword|excel|powerpnt)[.]exe$","NegateMatch":false},"TitlePattern":{"MatchingPattern":"^(Opening|Megnyit\\u00E1s)\\s-\\s(?:Microsoft\\s)?(Word|Excel|PowerPoint)(?:\\s\\([\\p{L}\\s-]+\\))?$","NegateMatch":false},"UrlPattern":{"MatchingPattern":"^.*$","NegateMatch":false}}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>ReplaceGroupRuleGenerator</Name>
    <Parameters>{"IgnoreCase":true,"ProcessNameParams":[{"MatchingPattern":"^excel[.]exe$","ReplaceGroupName":null}],"TitleParams":[{"MatchingPattern":"^(?(?=(Microsoft Excel(?&lt;optBrac&gt;\\s\\([\\p{L}\\s-]+\\))?\\s-\\s))(Microsoft Excel(?&lt;optBrac&gt;\\s\\([\\p{L}\\s-]+\\))?\\s-\\s","ReplaceGroupName":null},{"MatchingPattern":"(?&lt;file&gt;.+?)","ReplaceGroupName":"file"},{"MatchingPattern":"(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?(?:[:]\\d+)?(?&lt;optBrac&gt;\\s\\s?\\[[\\p{L}\\s-]+\\])*$)|(","ReplaceGroupName":null},{"MatchingPattern":"(?&lt;file&gt;.+?)","ReplaceGroupName":"file"},{"MatchingPattern":"(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?(?:[:]\\d+)?(?&lt;optBrac&gt;\\s\\s?\\[[\\p{L}\\s-]+\\])*(?&lt;optBrac&gt;\\s\\([\\p{L}\\s-]+\\))?\\s-\\sExcel(?&lt;optBrac&gt;\\s\\([\\p{L}\\s-]+\\))?$))","ReplaceGroupName":null}],"UrlParams":[{"MatchingPattern":"^.*$","ReplaceGroupName":null}]}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>ReplaceGroupRuleGenerator</Name>
    <Parameters>{"IgnoreCase":true,"ProcessNameParams":[{"MatchingPattern":"^winword[.]exe$","ReplaceGroupName":null}],"TitleParams":[{"MatchingPattern":"^","ReplaceGroupName":null},{"MatchingPattern":"(?&lt;file&gt;.+?)","ReplaceGroupName":"file"},{"MatchingPattern":"(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?((?&lt;optPar&gt;\\s\\([\\p{L}\\s-]+\\))?(?&lt;optBrac&gt;\\s\\[[\\p{L}\\s-]+\\])?)*(?:[:]\\d+)?\\s-(?&lt;optMs&gt;\\sMicrosoft)?\\sWord(?&lt;optParEnd&gt;\\s\\([\\p{L}\\s-]+\\))?$","ReplaceGroupName":null}],"UrlParams":[{"MatchingPattern":"^.*$","ReplaceGroupName":null}]}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>ReplaceGroupRuleGenerator</Name>
    <Parameters>{"IgnoreCase":true,"ProcessNameParams":[{"MatchingPattern":"^powerpnt[.]exe$","ReplaceGroupName":null}],"TitleParams":[{"MatchingPattern":"^(?(?=((?&lt;optMs&gt;Microsoft\\s)?(?&lt;optOf&gt;Office\\s)?PowerPoint(?&lt;optText&gt;.*?)\\s[-\\u2013]\\s\\[))((?&lt;optMs&gt;Microsoft\\s)?(?&lt;optOf&gt;Office\\s)?PowerPoint(?&lt;optText&gt;.*?)\\s[-\\u2013]\\s\\[","ReplaceGroupName":null},{"MatchingPattern":"(?&lt;file&gt;.+?)","ReplaceGroupName":"file"},{"MatchingPattern":"(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?(?&lt;optBrac&gt;\\s\\s?\\[[\\p{L}\\s-]+\\])*(?:[:]\\d+)?\\](?&lt;optMsE&gt;\\s-\\sMicrosoft PowerPoint)?$)|(","ReplaceGroupName":null},{"MatchingPattern":"(?&lt;file&gt;.+?)","ReplaceGroupName":"file"},{"MatchingPattern":"(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?((?&lt;optPar&gt;\\s\\([\\p{L}\\s-]+\\))?(?&lt;optBrac&gt;\\s\\[[\\p{L}\\s-]+\\])?)*(?:[:]\\d+)?\\s-\\s(?&lt;optMs&gt;Microsoft\\s)?PowerPoint(?&lt;optParEnd&gt;\\s\\([\\p{L}\\s-]+\\))?$))","ReplaceGroupName":null}],"UrlParams":[{"MatchingPattern":"^.*$","ReplaceGroupName":null}]}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>IgnoreRuleGenerator</Name>
    <Parameters>{"IgnoreCase":true,"ProcessNamePattern":{"MatchingPattern":"^(winword|excel|powerpnt)[.]exe$","NegateMatch":false},"TitlePattern":{"MatchingPattern":"^.*$","NegateMatch":false},"UrlPattern":{"MatchingPattern":"^.*$","NegateMatch":false}}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>SimpleRuleGenerator</Name>
    <Parameters>{"IgnoreCase":true}</Parameters>
  </RuleGeneratorData>
</ArrayOfRuleGeneratorData>''
	END

	SELECT @result AS LearningRuleGenerators

	RETURN
' 
END
GO
/****** Object:  Table [dbo].[IvrWorkItems] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrWorkItems]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IvrWorkItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WorkId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NULL,
	[MaxEndDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[CompanyId] [int] NOT NULL,
	[PhoneNumber] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TrunkId] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[AutoEndOnComputerActivity] [bit] NOT NULL,
	[IvrLastCheckDate] [datetime] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LogoffMaxEndDate] [datetime] NULL,
	[InstantNotificationEmail] [bit] NOT NULL,
 CONSTRAINT [PK_IvrWorkItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[NotificationForms] ******/
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
/****** Object:  Table [dbo].[MobileWorkItems] ******/
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
	[Imei] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[FirstReceiveDate] [datetime] NOT NULL,
	[LastReceiveDate] [datetime] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
 CONSTRAINT [PK_MobileWorkItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[ActiveDevices] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActiveDevices]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ActiveDevices](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[DeviceId] [bigint] NULL,
	[FirstSeen] [datetime] NOT NULL,
	[LastSeen] [datetime] NOT NULL,
 CONSTRAINT [PK_ActiveDevices] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[ClientComputerVersions] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerVersions]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClientComputerVersions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ComputerId] [int] NOT NULL,
	[Major] [int] NOT NULL,
	[Minor] [int] NOT NULL,
	[Build] [int] NOT NULL,
	[Revision] [int] NOT NULL,
	[IsCurrent] [bit] NOT NULL,
	[FirstReceiveDate] [datetime] NOT NULL,
	[LastReceiveDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ClientComputerVersions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerVersions]') AND name = N'IX_ClientComputerVersions_UserId_ComputerId_IsCurrent')
CREATE NONCLUSTERED INDEX [IX_ClientComputerVersions_UserId_ComputerId_IsCurrent] ON [dbo].[ClientComputerVersions] 
(
	[UserId] ASC,
	[ComputerId] ASC,
	[IsCurrent] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[Calendars] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Calendars]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Calendars](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreateDate] [datetime] NOT NULL,
	[IsMondayWorkDay] [bit] NOT NULL,
	[IsTuesdayWorkDay] [bit] NOT NULL,
	[IsWednesdayWorkDay] [bit] NOT NULL,
	[IsThursdayWorkDay] [bit] NOT NULL,
	[IsFridayWorkDay] [bit] NOT NULL,
	[IsSaturdayWorkDay] [bit] NOT NULL,
	[IsSundayWorkDay] [bit] NOT NULL,
	[InheritedFrom] [int] NULL,
 CONSTRAINT [PK_Calendar] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[ClientComputerKicks] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerKicks]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClientComputerKicks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ComputerId] [bigint] NOT NULL,
	[Reason] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedBy] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[ExpirationDate] [datetime] NOT NULL,
	[SendDate] [datetime] NULL,
	[ConfirmDate] [datetime] NULL,
	[Result] [int] NULL,
 CONSTRAINT [PK_ClientComputerKicks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[AggregateWorkItems] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AggregateWorkItems]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AggregateWorkItems](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[WorkTime] [int] NOT NULL,
	[WorkId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[CompanyId] [int] NOT NULL,
	[ComputerId] [int] NOT NULL,
	[MouseActivity] [int] NOT NULL,
	[KeyboardActivity] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[UpdateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_AggregateWorkItems] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON),
 CONSTRAINT [IX_AggregateWorkItems_Unique] UNIQUE NONCLUSTERED 
(
	[StartDate] ASC,
	[WorkId] ASC,
	[UserId] ASC,
	[GroupId] ASC,
	[CompanyId] ASC,
	[ComputerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AggregateWorkItems]') AND name = N'IX_AggregateWorkItems_StartDateClust')
CREATE CLUSTERED INDEX [IX_AggregateWorkItems_StartDateClust] ON [dbo].[AggregateWorkItems] 
(
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[AggregateWorkItemIntervals] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AggregateWorkItemIntervals]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AggregateWorkItemIntervals](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[WorkId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[CompanyId] [int] NOT NULL,
	[PhaseId] [uniqueidentifier] NOT NULL,
	[ComputerId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[UpdateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_AggregateWorkItemIntervals] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AggregateWorkItemIntervals]') AND name = N'IX_AggregateWorkItemIntervals_StartDateClust')
CREATE CLUSTERED INDEX [IX_AggregateWorkItemIntervals_StartDateClust] ON [dbo].[AggregateWorkItemIntervals] 
(
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AggregateWorkItemIntervals]') AND name = N'IX_AggregateWorkItemIntervals_DateForOR')
CREATE NONCLUSTERED INDEX [IX_AggregateWorkItemIntervals_DateForOR] ON [dbo].[AggregateWorkItemIntervals] 
(
	[EndDate] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  StoredProcedure [dbo].[GetUsersForUsageStats] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUsersForUsageStats]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetUsersForUsageStats]
	(
	@startDate datetime = NULL,
	@endDate datetime = NULL
	)
AS
	SET NOCOUNT ON

	IF @startDate IS NULL SET @startDate = ''1900-01-01''
	IF @endDate IS NULL SET @endDate = ''3000-01-01''

	SELECT DISTINCT UserId 
	  FROM AggregateWorkItemIntervals 
	 WHERE StartDate < @endDate 
	   AND EndDate > @startDate
	
	UNION

	SELECT DISTINCT UserId 
	  FROM MobileWorkItems 
	 WHERE StartDate < @endDate 
	   AND EndDate > @startDate

	UNION

	SELECT DISTINCT UserId 
	  FROM ManualWorkItems 
	 WHERE StartDate < @endDate 
	   AND EndDate > @startDate

	UNION

	SELECT DISTINCT UserId 
	  FROM IvrWorkItems 
	 WHERE StartDate < @endDate 
	   AND ((EndDate IS NOT NULL AND EndDate > @startDate)
			OR (EndDate IS NULL AND IvrLastCheckDate < MaxEndDate AND @startDate < IvrLastCheckDate)
			OR (EndDate IS NULL AND IvrLastCheckDate >= MaxEndDate AND @startDate < MaxEndDate)
		   )

	RETURN
' 
END
GO
/****** Object:  Table [dbo].[AggregateLastWorkItem] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AggregateLastWorkItem]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AggregateLastWorkItem](
	[LastAggregatedId] [bigint] NOT NULL,
 CONSTRAINT [PK_AggregateLastWorkItem] PRIMARY KEY CLUSTERED 
(
	[LastAggregatedId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[AggregateIdleIntervals] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AggregateIdleIntervals]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AggregateIdleIntervals](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[WorkId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[CompanyId] [int] NOT NULL,
	[PhaseId] [uniqueidentifier] NOT NULL,
	[ComputerId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[UpdateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_AggregateIdleIntervals] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AggregateIdleIntervals]') AND name = N'IX_AggregateIdleIntervals_StartDateClust')
CREATE CLUSTERED INDEX [IX_AggregateIdleIntervals_StartDateClust] ON [dbo].[AggregateIdleIntervals] 
(
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AggregateIdleIntervals]') AND name = N'IX_AggregateIdleIntervals_EndDate_StartDate')
CREATE NONCLUSTERED INDEX [IX_AggregateIdleIntervals_EndDate_StartDate] ON [dbo].[AggregateIdleIntervals] 
(
	[EndDate] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[ClientComputerInfo] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClientComputerInfo](
	[UserId] [int] NOT NULL,
	[ComputerId] [int] NOT NULL,
	[OSMajor] [int] NOT NULL,
	[OSMinor] [int] NOT NULL,
	[OSBuild] [int] NOT NULL,
	[OSRevision] [int] NOT NULL,
	[IsNet4Available] [bit] NOT NULL,
	[IsNet45Available] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ClientComputerInfo] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[ComputerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[ClientComputerErrors] ******/
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
/****** Object:  Table [dbo].[ClientComputerAddresses] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerAddresses]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClientComputerAddresses](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ComputerId] [int] NOT NULL,
	[Address] [varchar](39) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IsCurrent] [bit] NOT NULL,
	[FirstReceiveDate] [datetime] NOT NULL,
	[LastReceiveDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ClientComputerAddresses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerAddresses]') AND name = N'IX_ClientComputerAddresses_UserId_ComputerId_IsCurrent')
CREATE NONCLUSTERED INDEX [IX_ClientComputerAddresses_UserId_ComputerId_IsCurrent] ON [dbo].[ClientComputerAddresses] 
(
	[UserId] ASC,
	[ComputerId] ASC,
	[IsCurrent] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  StoredProcedure [dbo].[Client_SetWorkedUsersOnDay] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Client_SetWorkedUsersOnDay]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[Client_SetWorkedUsersOnDay]
	(
	@UserIds varchar(4000),
	@Day datetime
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	--dummy placeholder sproc for CommitUsageStatsToEcomm
	--don''t publish to LIVE servers

	IF LEFT(@UserIds, 1) = ''-''
	BEGIN
		RETURN 1 --test error case with negative userid
	END
	ELSE
	BEGIN
		RETURN 0
	END
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetIntersectDuration] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIntersectDuration]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE FUNCTION [dbo].[GetIntersectDuration]
	(
	@startDate1 datetime, --NOT NULL
	@endDate1 datetime,   --NOT NULL
	@startDate2 datetime, --NOT NULL
	@endDate2 datetime    --NOT NULL
	)
RETURNS bigint
WITH RETURNS NULL ON NULL INPUT
AS
	BEGIN
	
	DECLARE @startDate datetime, @endDate datetime
	
	SET @startDate = 
	CASE
		WHEN @startDate1 < @startDate2 THEN @startDate2
		ELSE @startDate1
	END
	
	SET @endDate = 
	CASE
		WHEN @endDate1 < @endDate2 THEN @endDate1
		ELSE @endDate2
	END
	
	RETURN 
	CASE 
		WHEN @endDate < @startDate THEN 0
		ELSE CAST(DATEDIFF(SECOND, @startDate, @endDate) AS bigint) * 1000 + DATEPART(MILLISECOND, @endDate) - DATEPART(MILLISECOND, @startDate)
	END	

	END
' 
END
GO
/****** Object:  StoredProcedure [dbo].[GetSchemaVersion] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSchemaVersion]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetSchemaVersion] AS RETURN 1
' 
END
GO
/****** Object:  Table [dbo].[EmailStats] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailStats]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[EmailStats](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[LastSendDate] [datetime] NULL,
 CONSTRAINT [PK_EmailStats] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON),
 CONSTRAINT [IX_EmailStats_Unique] UNIQUE NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[DeadLetterItems] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeadLetterItems]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DeadLetterItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WorkId] [int] NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[ItemType] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ErrorText] [nvarchar](3000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_DeadLetterItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[TitleLookup] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TitleLookup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TitleLookup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HashCode] [int] NOT NULL,
	[Title] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_TitleLookup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TitleLookup]') AND name = N'IX_TitleLookup_HashCode')
CREATE NONCLUSTERED INDEX [IX_TitleLookup_HashCode] ON [dbo].[TitleLookup] 
(
	[HashCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[Storages] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Storages]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Storages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstId] [bigint] NOT NULL,
	[Algorithm] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Data] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Description] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Storages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON),
 CONSTRAINT [IX_Storages_FirstId_Unique] UNIQUE NONCLUSTERED 
(
	[FirstId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteVoiceRecordings] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteVoiceRecordings]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[DeleteVoiceRecordings]
	(
	@id int output,
	@clientId uniqueidentifier,
	@userId int,
	@deleteDate datetime output
	)
AS
	SET XACT_ABORT ON
	SET NOCOUNT ON

	BEGIN TRAN

	DECLARE @res int = 0
	DECLARE @OutTable TABLE (Id int, DeleteDate datetime)
	SET @id = NULL
	SET @deleteDate = NULL
	
	UPDATE [dbo].[VoiceRecordings]
	   SET [DeleteDate] = GETUTCDATE()
	OUTPUT Deleted.Id, Inserted.DeleteDate INTO @OutTable
	 WHERE [ClientId] = @clientId
	   AND [UserId] = @userId
	   AND [DeleteDate] IS NULL

	SET @res = @@rowcount
	
	IF (@res = 0) --check for dupes
	BEGIN
		SELECT @id = [Id]
			  ,@deleteDate = [DeleteDate]
		  FROM [dbo].[VoiceRecordings]
		 WHERE [ClientId] = @clientId
		   AND [UserId] = @userId
		   AND [DeleteDate] IS NOT NULL
	END
	ELSE
	BEGIN
		SELECT @id = [Id]
			  ,@deleteDate = [DeleteDate]
		  FROM @OutTable
	END
	
	IF (@id IS NULL)
	BEGIN
		RAISERROR(''Cannot find data to delete'', 16, 1)
		ROLLBACK
		RETURN 0
	END

	COMMIT TRAN
	RETURN @res
' 
END
GO
/****** Object:  Table [dbo].[DesktopCaptures] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DesktopCaptures]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DesktopCaptures](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[WorkItemId] [bigint] NOT NULL,
 CONSTRAINT [PK_DesktopCaptures] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  StoredProcedure [dbo].[CommitUsageStatsToEcomm] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CommitUsageStatsToEcomm]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[CommitUsageStatsToEcomm]
AS

	SET NOCOUNT ON
	SET XACT_ABORT ON

	declare @c_LocalDate datetime,
			@c_UserId int,
			@c_Id int,
			@result int

	BEGIN TRAN

	DECLARE upd_cursor CURSOR FORWARD_ONLY FOR
	SELECT 
		Id,
		LocalDate,
		UserId
	FROM UsageStats (UPDLOCK)
	WHERE
		IsAcked = 0
		AND (ComputerWorkTime + IvrWorkTime + MobileWorkTime + ManuallyAddedWorkTime) > 300000 -- 5 mins

	OPEN upd_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM upd_cursor INTO 
			@c_Id,
			@c_LocalDate,
			@c_UserId

		IF @@FETCH_STATUS <> 0
			BREAK

		declare @c_UserIdStr varchar(4000)
		SET @c_UserIdStr = CAST(@c_UserId AS varchar(11))

		EXEC @result = [dbo].[Client_SetWorkedUsersOnDay] @UserIds = @c_UserIdStr, @Day = @c_LocalDate
		IF @result = 0
		BEGIN
			UPDATE UsageStats
				SET IsAcked = 1
				WHERE Id = @c_Id
		END

	END

	CLOSE upd_cursor
	DEALLOCATE upd_cursor

	COMMIT TRAN
	RETURN 0
' 
END
GO
/****** Object:  StoredProcedure [dbo].[GetNextValueForSequence] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetNextValueForSequence]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetNextValueForSequence]
(
	@nextValue BINARY(8) OUTPUT
)
AS
DECLARE @outTable TABLE (outValue BINARY(8))
UPDATE [dbo].[RowVersionSequence] SET IsDummy=1 OUTPUT CAST(Deleted.Version AS BINARY(8)) AS VERSION INTO @outTable
IF (@@ROWCOUNT = 0) 
BEGIN
	BEGIN TRY
		INSERT INTO [dbo].[RowVersionSequence] (IsDummy) VALUES (1)
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() <> 2627)
		BEGIN
			DECLARE @ErrorMessage nvarchar(MAX), @ErrorSeverity INT, @ErrorState INT;
			SELECT @ErrorMessage = ERROR_MESSAGE() + '' Line '' + CAST(ERROR_LINE() AS nvarchar(5)), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
			raiserror (@ErrorMessage, @ErrorSeverity, @ErrorState);
		END
	END CATCH;
	UPDATE [dbo].[RowVersionSequence] SET IsDummy=1 OUTPUT CAST(Deleted.Version AS INT) AS VERSION INTO @outTable
END
SET @nextValue = (SELECT outValue FROM @outTable)
RETURN 0
' 
END
GO
/****** Object:  StoredProcedure [dbo].[GetMobileWorkTimeByWorkIdForUser] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMobileWorkTimeByWorkIdForUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetMobileWorkTimeByWorkIdForUser]
	(
	@userId int,
	@startDate datetime,
	@endDate datetime
	)
AS
	SET NOCOUNT ON

	IF @userId IS NULL OR @startDate IS NULL OR @endDate IS NULL
	BEGIN
		RAISERROR(''@userId, @startDate and @endDate cannot be NULL'', 16, 1)
		RETURN
	END

	declare @result table (
	WorkId int NOT NULL,
	WorkTime bigint NOT NULL
	)

	INSERT INTO
		@result
	SELECT 
		WorkId, 
		SUM(dbo.GetIntersectDuration(@startDate, @endDate, StartDate, EndDate)) AS WorkTime
	FROM 
		MobileWorkItems 
	WHERE 
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	GROUP BY WorkId
		
	SELECT * FROM @result

	RETURN
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetInheritedCalendarIds] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetInheritedCalendarIds]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE FUNCTION [dbo].[GetInheritedCalendarIds] 
(	
	@calendarId int
)
RETURNS TABLE 
AS
RETURN 
	WITH InheritedCalendars(Id, InheritedFrom, Level)
	AS
	(
		-- Anchor member definition
		SELECT c.Id, c.InheritedFrom, 0 AS Level
		FROM Calendars AS c
		WHERE c.Id = @calendarId
		
		UNION ALL
		-- Recursive member definition
		SELECT c.Id, c.InheritedFrom, ic.Level + 1 AS Level
		FROM Calendars c
		JOIN InheritedCalendars AS ic ON c.Id = ic.InheritedFrom
	)
	-- Statement that executes the CTE
	SELECT Id, Level FROM InheritedCalendars
' 
END
GO
/****** Object:  StoredProcedure [dbo].[GetIdForUrl] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIdForUrl]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetIdForUrl]
	(
	@url nvarchar(1000),
	@id int OUTPUT
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @url IS NULL
	BEGIN
		RAISERROR(''@url cannot be NULL'', 16, 1)
		RETURN
	END

declare @hashCode int
SET @hashCode = CHECKSUM(@url)

--optimize for common case, we don''t want to put index on (hash,value) or handle deadlocks so we won''t use UPDLOCK
SET @id = (
	SELECT [Id]
	  FROM [dbo].[UrlLookup]
	 WHERE [HashCode] = @hashCode
	   AND [Url] = @url
	)

IF @id IS NOT NULL
	RETURN

BEGIN TRAN

SET @id = (
	SELECT [Id]
	  FROM [dbo].[UrlLookup] WITH (TABLOCKX, HOLDLOCK)
	 WHERE [HashCode] = @hashCode
	   AND [Url] = @url
	)

IF @id IS NULL
BEGIN
	INSERT INTO [dbo].[UrlLookup]
			   ([HashCode]
			   ,[Url])
		 VALUES
			   (@hashCode
			   ,@url)

	SET @id = SCOPE_IDENTITY()
END

COMMIT TRAN

	RETURN
' 
END
GO
/****** Object:  StoredProcedure [dbo].[GetIdForTitle] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIdForTitle]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetIdForTitle]
	(
	@title nvarchar(1000),
	@id int OUTPUT
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @title IS NULL
	BEGIN
		RAISERROR(''@title cannot be NULL'', 16, 1)
		RETURN
	END

declare @hashCode int
SET @hashCode = CHECKSUM(@title)

--optimize for common case, we don''t want to put index on (hash,value) or handle deadlocks so we won''t use UPDLOCK
SET @id = (
	SELECT [Id]
	  FROM [dbo].[TitleLookup]
	 WHERE [HashCode] = @hashCode
	   AND [Title] = @title
	)

IF @id IS NOT NULL
	RETURN

BEGIN TRAN

SET @id = (
	SELECT [Id]
	  FROM [dbo].[TitleLookup] WITH (TABLOCKX, HOLDLOCK)
	 WHERE [HashCode] = @hashCode
	   AND [Title] = @title
	)

IF @id IS NULL
BEGIN
	INSERT INTO [dbo].[TitleLookup]
			   ([HashCode]
			   ,[Title])
		 VALUES
			   (@hashCode
			   ,@title)

	SET @id = SCOPE_IDENTITY()
END

COMMIT TRAN

	RETURN
' 
END
GO
/****** Object:  StoredProcedure [dbo].[GetIdForProcessName] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIdForProcessName]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetIdForProcessName]
	(
	@processName nvarchar(100),
	@id int OUTPUT
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @processName IS NULL
	BEGIN
		RAISERROR(''@processName cannot be NULL'', 16, 1)
		RETURN
	END

declare @hashCode int
SET @hashCode = CHECKSUM(@processName)

--optimize for common case, we don''t want to put index on (hash,value) or handle deadlocks so we won''t use UPDLOCK
SET @id = (
	SELECT [Id]
	  FROM [dbo].[ProcessNameLookup]
	 WHERE [HashCode] = @hashCode
	   AND [ProcessName] = @processName
	)

IF @id IS NOT NULL
	RETURN

BEGIN TRAN

SET @id = (
	SELECT [Id]
	  FROM [dbo].[ProcessNameLookup] WITH (TABLOCKX, HOLDLOCK)
	 WHERE [HashCode] = @hashCode
	   AND [ProcessName] = @processName
	)

IF @id IS NULL
BEGIN
	INSERT INTO [dbo].[ProcessNameLookup]
			   ([HashCode]
			   ,[ProcessName])
		 VALUES
			   (@hashCode
			   ,@processName)

	SET @id = SCOPE_IDENTITY()
END

COMMIT TRAN

	RETURN
' 
END
GO
/****** Object:  Trigger [Calendars_Validation] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[Calendars_Validation]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[Calendars_Validation]
ON [dbo].[Calendars]
AFTER INSERT, UPDATE --INSERT is needed because we can insert a row referencing itself
AS
BEGIN
	SET NOCOUNT ON;
	
	--probably we should lock table [dbo].[Calendars]
	declare
	@curr_id int,
	@ci_Id bigint,
	@ci_InheritedFrom int

	DECLARE inserted_cursor CURSOR FAST_FORWARD FOR 
	SELECT [Id]
		  ,[InheritedFrom]
	  FROM inserted
	  
	OPEN inserted_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM inserted_cursor INTO 
			@ci_Id,
			@ci_InheritedFrom

		IF @@FETCH_STATUS <> 0
			BREAK

		SET @curr_id = @ci_InheritedFrom
		WHILE @curr_id IS NOT NULL
		BEGIN
			IF (@curr_id = @ci_Id)
			BEGIN
				RAISERROR(''Cannot create circular reference'',16,1)
				ROLLBACK
				RETURN
			END
			SET @curr_id = (SELECT [InheritedFrom] FROM [dbo].[Calendars] WHERE [Id] = @curr_id)
		END
		
	END
	
	CLOSE inserted_cursor
	DEALLOCATE inserted_cursor

END
'
GO
/****** Object:  StoredProcedure [dbo].[ClientComputerKickConfirm] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerKickConfirm]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'
-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[ClientComputerKickConfirm]
	(
	@id int,
	@userId int,
	@deviceId bigint,
	@confirmDate datetime,
	@result int
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @id IS NULL OR @userId IS NULL OR @deviceId IS NULL OR @confirmDate IS NULL OR @result IS NULL
	BEGIN
		RAISERROR(''@id, @userId, @deviceId, @confirmDate and @result cannot be NULL'', 16, 1)
		RETURN
	END

UPDATE [dbo].[ClientComputerKicks]
   SET [ConfirmDate] = @confirmDate
      ,[Result] = @result
 WHERE [Id] = @id
   AND [UserId] = @userId
   AND [ComputerId] = @deviceId
   AND [ConfirmDate] IS NULL
   AND [Result] IS NULL

	RETURN @@ROWCOUNT

' 
END
GO
/****** Object:  StoredProcedure [dbo].[ClientComputerKickSend] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerKickSend]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'
-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[ClientComputerKickSend]
	(
	@id int,
	@userId int,
	@deviceId bigint,
	@sendDate datetime
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @id IS NULL OR @userId IS NULL OR @deviceId IS NULL OR @sendDate IS NULL
	BEGIN
		RAISERROR(''@userId, @deviceId and @sendDate cannot be NULL'', 16, 1)
		RETURN
	END

UPDATE [dbo].[ClientComputerKicks]
   SET [SendDate] = @sendDate
 WHERE [Id] = @id
   AND [UserId] = @userId
   AND [ComputerId] = @deviceId
   AND [SendDate] IS NULL

	RETURN @@ROWCOUNT

' 
END
GO
/****** Object:  Table [dbo].[CalendarExceptions] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CalendarExceptions]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CalendarExceptions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreateDate] [datetime] NOT NULL,
	[CalendarId] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[IsWorkDay] [bit] NOT NULL,
 CONSTRAINT [PK_CalendarExceptions] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON),
 CONSTRAINT [IX_CalendarExceptions_Unique] UNIQUE NONCLUSTERED 
(
	[Date] ASC,
	[CalendarId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CalendarExceptions]') AND name = N'IX_CalendarExceptions_Date_Clust')
CREATE CLUSTERED INDEX [IX_CalendarExceptions_Date_Clust] ON [dbo].[CalendarExceptions] 
(
	[Date] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[ClientNotifications] ******/
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
/****** Object:  StoredProcedure [dbo].[MergeAggregateWorkItemIntervals] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MergeAggregateWorkItemIntervals]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
--this sproc should not be called directly, its only called by [UpdateHourlyAggregateWorkItems]
CREATE PROCEDURE [dbo].[MergeAggregateWorkItemIntervals]
	(
	@startDate datetime
	)
AS
	SET NOCOUNT ON

IF @startDate IS NULL RETURN

SET XACT_ABORT ON
BEGIN TRAN

declare
@lastEndDate datetime,
@lastId bigint,
@c_firstId bigint,
@c_secondId bigint,
@c_secondEndDate datetime

-- StartDate and PhaseId is unique in WorkItems so it should be unique in AggregateWorkItemIntervals too.
-- That means that we can only find at most one match for each interval to merge. (no dupes on join)
-- Without the PhaseId we could use row_number to only fetch the first match if there are more.
DECLARE merge_cursor CURSOR LOCAL STATIC FORWARD_ONLY FOR 
SELECT f.[Id]
      ,s.[Id]
      ,s.[EndDate]
  FROM [dbo].[AggregateWorkItemIntervals] f (TABLOCKX)
  JOIN [dbo].[AggregateWorkItemIntervals] s
		ON 	f.[EndDate] = s.[StartDate]
		AND f.[WorkId] = s.[WorkId]
		AND f.[UserId] = s.[UserId]
		AND f.[GroupId] = s.[GroupId]
		AND f.[CompanyId] = s.[CompanyId]
		AND f.[PhaseId] = s.[PhaseId]
		AND f.[ComputerId] = s.[ComputerId]
		AND f.[Id] <> s.[Id]
 WHERE 
        f.[EndDate] >= @startDate 
        AND s.[EndDate] >= @startDate
ORDER BY f.[PhaseId], f.[WorkId], f.[UserId], f.[GroupId], f.[CompanyId], f.[ComputerId], f.[EndDate] DESC
OPTION (RECOMPILE)

OPEN merge_cursor

WHILE 1=1
BEGIN
	FETCH NEXT FROM merge_cursor INTO 
		@c_firstId,
		@c_secondId,
		@c_secondEndDate

	IF @@FETCH_STATUS <> 0
		BREAK

	--delete the second interval
	DELETE FROM [dbo].[AggregateWorkItemIntervals] WHERE Id = @c_secondId
	IF @@rowcount = 0 CONTINUE --already deleted

	--extend the first interval
	IF @c_secondId = @lastId SET @c_secondEndDate = @lastEndDate
	UPDATE [dbo].[AggregateWorkItemIntervals] 
	   SET [EndDate] = @c_secondEndDate,
		   [UpdateDate] = GETUTCDATE()
	 WHERE Id = @c_firstId

	SET @lastEndDate = @c_secondEndDate
	SET @lastId = @c_firstId

END

CLOSE merge_cursor
DEALLOCATE merge_cursor
	
COMMIT TRAN	
	RETURN
' 
END
GO
/****** Object:  StoredProcedure [dbo].[MergeAggregateIdleIntervals] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MergeAggregateIdleIntervals]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
--this sproc should not be called directly, its only called by [UpdateHourlyAggregateWorkItems]
CREATE PROCEDURE [dbo].[MergeAggregateIdleIntervals]
	(
	@startDate datetime
	)
AS
	SET NOCOUNT ON

IF @startDate IS NULL RETURN

SET XACT_ABORT ON
BEGIN TRAN

declare
@lastEndDate datetime,
@lastId bigint,
@c_firstId bigint,
@c_secondId bigint,
@c_secondEndDate datetime

-- StartDate and PhaseId is unique in WorkItems so it should be unique in AggregateIdleIntervals too.
-- That means that we can only find at most one match for each interval to merge. (no dupes on join)
-- Without the PhaseId we could use row_number to only fetch the first match if there are more.
DECLARE merge_cursor CURSOR LOCAL STATIC FORWARD_ONLY FOR 
SELECT f.[Id]
      ,s.[Id]
      ,s.[EndDate]
  FROM [dbo].[AggregateIdleIntervals] f (TABLOCKX)
  JOIN [dbo].[AggregateIdleIntervals] s
		ON 	f.[EndDate] = s.[StartDate]
		AND f.[WorkId] = s.[WorkId]
		AND f.[UserId] = s.[UserId]
		AND f.[GroupId] = s.[GroupId]
		AND f.[CompanyId] = s.[CompanyId]
		AND f.[PhaseId] = s.[PhaseId]
		AND f.[ComputerId] = s.[ComputerId]
 		AND f.[Id] <> s.[Id]
 WHERE 
        f.[EndDate] >= @startDate 
        AND s.[EndDate] >= @startDate
ORDER BY f.[PhaseId], f.[WorkId], f.[UserId], f.[GroupId], f.[CompanyId], f.[ComputerId], f.[EndDate] DESC
OPTION (RECOMPILE)

OPEN merge_cursor

WHILE 1=1
BEGIN
	FETCH NEXT FROM merge_cursor INTO 
		@c_firstId,
		@c_secondId,
		@c_secondEndDate

	IF @@FETCH_STATUS <> 0
		BREAK

	--delete the second interval
	DELETE FROM [dbo].[AggregateIdleIntervals] WHERE Id = @c_secondId
	IF @@rowcount = 0 CONTINUE --already deleted

	--extend the first interval
	IF @c_secondId = @lastId SET @c_secondEndDate = @lastEndDate
	UPDATE [dbo].[AggregateIdleIntervals] 
	   SET [EndDate] = @c_secondEndDate,
		   [UpdateDate] = GETUTCDATE()
	 WHERE Id = @c_firstId

	SET @lastEndDate = @c_secondEndDate
	SET @lastId = @c_firstId

END

CLOSE merge_cursor
DEALLOCATE merge_cursor
	
COMMIT TRAN	
	RETURN
' 
END
GO
/****** Object:  StoredProcedure [dbo].[GetIvrWorkTimeByWorkIdForUser] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetIvrWorkTimeByWorkIdForUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetIvrWorkTimeByWorkIdForUser]
	(
	@userId int,
	@startDate datetime,
	@endDate datetime,
	@workId int = NULL
	)
AS
	SET NOCOUNT ON
	
	IF @userId IS NULL OR @startDate IS NULL OR @endDate IS NULL
	BEGIN
		RAISERROR(''@userId, @startDate and @endDate cannot be NULL'', 16, 1)
		RETURN
	END

	declare @result table (
	WorkId int NOT NULL,
	WorkTime bigint NOT NULL
	)
	
	
	INSERT INTO
		@result
	SELECT
		ivrItems.WorkId AS WorkId,
		SUM(
		CASE
			WHEN ivrItems.StartDate < ivrItems.EndDate THEN CAST(DATEDIFF(SECOND, ivrItems.StartDate, ivrItems.EndDate) AS bigint) * 1000 + DATEPART(MILLISECOND, ivrItems.EndDate) - DATEPART(MILLISECOND, ivrItems.StartDate)
			ELSE 0
		END
		) AS WorkTime
	FROM
		(SELECT
			WorkId,
			CASE
				WHEN StartDate < @startDate THEN @startDate
				ELSE StartDate
			END AS StartDate,
			CASE
				WHEN EndDate IS NOT NULL THEN
					CASE
						WHEN EndDate < @endDate THEN EndDate
						ELSE @endDate
					END
				WHEN IvrLastCheckDate < MaxEndDate THEN
					CASE
						WHEN IvrLastCheckDate < @endDate THEN IvrLastCheckDate
						ELSE @endDate
					END
				ELSE
					CASE
						WHEN MaxEndDate < @endDate THEN MaxEndDate
						ELSE @endDate
					END
			END AS EndDate
		FROM
			IvrWorkItems
		WHERE
			UserId = @userId
		AND StartDate < @endDate --not in the future
		AND StartDate >= DATEADD(day, -2, @startDate)
		AND @startDate < ISNULL(EndDate,MaxEndDate)
		AND WorkId = ISNULL(@workId, WorkId)
		) AS ivrItems
	GROUP BY
		WorkId
		
	SELECT * FROM @result

	RETURN
' 
END
GO
/****** Object:  Table [dbo].[IvrRules] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrRules]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IvrRules](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StartTime] [int] NOT NULL,
	[EndTime] [int] NOT NULL,
	[IncrementInside] [int] NOT NULL,
	[IncrementOutside] [int] NOT NULL,
	[IncrementInsideMaxTime] [int] NULL,
	[AutoEndOnComputerActivity] [bit] NOT NULL,
	[Name] [nvarchar](1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreateDate] [datetime] NOT NULL,
	[LogoffMaxEndTime] [int] NULL,
	[CustomRuleId] [int] NULL,
	[InstantNotificationEmail] [bit] NOT NULL,
 CONSTRAINT [PK_IvrRules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[IvrLocations] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrLocations]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IvrLocations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IvrWorkItemId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[Ctn] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[State] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[X] [int] NULL,
	[Y] [int] NULL,
	[Radius] [int] NULL,
	[SubscriberState] [int] NULL,
	[Mcc] [int] NULL,
	[Mnc] [int] NULL,
	[Lac] [int] NULL,
	[Cellid] [int] NULL,
	[AgeOfLocation] [int] NULL,
	[Date] [datetime] NULL,
	[Cust] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Msid] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ReplyText] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_IvrLocations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  StoredProcedure [dbo].[GetWorkTimeByWorkIdForUser] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetWorkTimeByWorkIdForUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetWorkTimeByWorkIdForUser]
	(
	@userId int,
	@startDate datetime,
	@endDate datetime,
	@workId int = NULL
	)
AS
	SET NOCOUNT ON
	
	IF @userId IS NULL OR @startDate IS NULL OR @endDate IS NULL
	BEGIN
		RAISERROR(''@userId, @startDate and @endDate cannot be NULL'', 16, 1)
		RETURN
	END
	
	declare @result table (
	WorkId int NOT NULL,
	WorkTime bigint NOT NULL
	)
	
	
	INSERT INTO
		@result
	SELECT 
		WorkId, 
		SUM(dbo.GetIntersectDuration(@startDate, @endDate, StartDate, EndDate)) AS WorkTime
	FROM 
		AggregateWorkItemIntervals 
	WHERE 
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -7, @startDate)
	AND EndDate > @startDate
	AND WorkId = ISNULL(@workId, WorkId)
	GROUP BY WorkId
		
	SELECT * FROM @result

	RETURN
	
	
/* without the function...
	SELECT
		items.WorkId AS WorkId,
		SUM(
		CASE
			WHEN items.StartDate < items.EndDate THEN CAST(DATEDIFF(SECOND, items.StartDate, items.EndDate) AS bigint) * 1000 + DATEPART(MILLISECOND, items.EndDate) - DATEPART(MILLISECOND, items.StartDate)
			ELSE 0
		END
		) AS WorkTime
	FROM
		(SELECT 
			WorkId,
			CASE
				WHEN StartDate < @startDate THEN @startDate
				ELSE StartDate
			END AS StartDate,
			CASE
				WHEN EndDate < @endDate THEN EndDate
				ELSE @endDate
			END AS EndDate
		FROM 
			WorkItems 
		WHERE 
			UserId = @userId
		AND StartDate > DATEADD(minute, -5, @startDate) --hax because there is no index on EndDate
		AND StartDate < @endDate
		AND WorkId = ISNULL(@workId, WorkId)) AS items
	GROUP BY WorkId
*/
' 
END
GO
/****** Object:  StoredProcedure [dbo].[GetTotalWorkTimeByWorkIdForUser] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTotalWorkTimeByWorkIdForUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetTotalWorkTimeByWorkIdForUser]
	(
	@userId int,
	@startDate datetime = NULL,
	@endDate datetime = NULL
	)
AS
	SET NOCOUNT ON
	
	IF NULLIF(object_id(''tempdb..#result''), 0) IS NOT NULL DROP TABLE #result
	IF NULLIF(object_id(''tempdb..#workTimeTable''), 0) IS NOT NULL DROP TABLE #workTimeTable
		
	create table #result (
	WorkId int NOT NULL PRIMARY KEY,
	TotalWorkTime bigint NOT NULL default (0),
	ComputerWorkTime bigint NOT NULL default (0),
	ComputerCorrectionTime bigint NOT NULL default (0),
	IvrWorkTime bigint NOT NULL default (0),
	IvrCorrectionTime bigint NOT NULL default (0),
	MobileWorkTime bigint NOT NULL default (0),
	MobileCorrectionTime bigint NOT NULL default (0),
	ManualWorkTime bigint NOT NULL default (0),
	HolidayTime bigint NOT NULL default (0),
	SickLeaveTime bigint NOT NULL default (0)
	)
	
	create table #workTimeTable (
	WorkId int NOT NULL PRIMARY KEY,
	WorkTime bigint NOT NULL default (0)
	)
	
	declare @c_WorkId int, @c_StartDate datetime, @c_EndDate datetime, @currStartDate datetime, @currEndDate datetime, @prevEndDate datetime
	
	IF @startDate IS NULL SET @startDate = ''1900-01-01''
	IF @endDate IS NULL SET @endDate = ''3000-01-01''
	
	-------------------------------------------------------------------------------------------------------
	--ComputerWorkTime
	-------------------------------------------------------------------------------------------------------
	--Assumes that AggregateWorkItemIntervals is up to date
	INSERT INTO #result (WorkId, ComputerWorkTime) EXEC dbo.GetWorkTimeByWorkIdForUser @userId=@userId, @startDate=@startDate, @endDate=@endDate

	-------------------------------------------------------------------------------------------------------
	--IvrWorkTime
	-------------------------------------------------------------------------------------------------------
	INSERT #workTimeTable EXEC dbo.GetIvrWorkTimeByWorkIdForUser @userId=@userId, @startDate=@startDate, @endDate=@endDate
			
	--BEGIN merge of IvrWorkTime
	UPDATE #result SET IvrWorkTime = IvrWorkTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, IvrWorkTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge

	-------------------------------------------------------------------------------------------------------
	--MobileWorkTime
	-------------------------------------------------------------------------------------------------------
	INSERT #workTimeTable EXEC dbo.GetMobileWorkTimeByWorkIdForUser @userId=@userId, @startDate=@startDate, @endDate=@endDate
			
	--BEGIN merge of MobileWorkTime
	UPDATE #result SET MobileWorkTime = MobileWorkTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, MobileWorkTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge

	-------------------------------------------------------------------------------------------------------
	--ManualWorkTime
	-------------------------------------------------------------------------------------------------------
	INSERT INTO #workTimeTable (WorkId, WorkTime)
	SELECT
		WorkId,
		SUM(dbo.GetIntersectDuration(@startDate, @endDate, StartDate, EndDate)) AS WorkTime
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND ManualWorkItemTypeId = 0 -- Manually added work time
	GROUP BY
		WorkId
			
	--BEGIN merge of ManualWorkTime
	UPDATE #result SET ManualWorkTime = ManualWorkTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, ManualWorkTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge

	-------------------------------------------------------------------------------------------------------
	--HolidayTime
	-------------------------------------------------------------------------------------------------------
	INSERT INTO #workTimeTable (WorkId, WorkTime)
	SELECT
		WorkId,
		SUM(dbo.GetIntersectDuration(@startDate, @endDate, StartDate, EndDate)) AS WorkTime
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND ManualWorkItemTypeId = 4 -- Holiday time
	GROUP BY
		WorkId
			
	--BEGIN merge of HolidayTime
	UPDATE #result SET HolidayTime = HolidayTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, HolidayTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge
	
	-------------------------------------------------------------------------------------------------------
	--SickLeaveTime
	-------------------------------------------------------------------------------------------------------
	INSERT INTO #workTimeTable (WorkId, WorkTime)
	SELECT
		WorkId,
		SUM(dbo.GetIntersectDuration(@startDate, @endDate, StartDate, EndDate)) AS WorkTime
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND ManualWorkItemTypeId = 5 -- Sick leave time
	GROUP BY
		WorkId
			
	--BEGIN merge of SickLeaveTime
	UPDATE #result SET SickLeaveTime = SickLeaveTime + w.WorkTime
	FROM #workTimeTable w
	JOIN #result r ON w.WorkId = r.WorkId
	
	INSERT INTO #result (WorkId, SickLeaveTime)
	SELECT WorkId, WorkTime
	FROM #workTimeTable w
	WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
	
	DELETE FROM #workTimeTable
	--END merge
			
	-------------------------------------------------------------------------------------------------------
	--ComputerCorrectionTime
	-------------------------------------------------------------------------------------------------------
	--get disjoint correction intervals, so a workitem will only be calculated once
	SET @prevEndDate = NULL
	
	DECLARE manualworkitem_cursor CURSOR FAST_FORWARD FOR 
	SELECT
		WorkId, StartDate, EndDate
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND (ManualWorkItemTypeId = 1 OR  ManualWorkItemTypeId = 3) -- Deleted Interval or Deleted Computer Interval
	ORDER BY StartDate
	
	OPEN manualworkitem_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM manualworkitem_cursor INTO 
			@c_WorkId,
			@c_StartDate,
			@c_EndDate

		IF @@FETCH_STATUS <> 0
			BREAK
		
		SET @currStartDate = @c_StartDate
		SET @currEndDate = @c_EndDate
			
		IF (@prevEndDate IS NOT NULL AND @currStartDate < @prevEndDate ) SET @currStartDate = @prevEndDate
		IF (@endDate < @currEndDate) SET @currEndDate = @endDate
		IF (@currEndDate < @currStartDate) CONTINUE
		
		
		INSERT #workTimeTable EXEC dbo.GetWorkTimeByWorkIdForUser @userId=@userId, @startDate=@currStartDate, @endDate=@currEndDate
		
		--BEGIN merge of ComputerCorrectionTime
		UPDATE #result SET ComputerCorrectionTime = ComputerCorrectionTime - w.WorkTime
		FROM #workTimeTable w
		JOIN #result r ON w.WorkId = r.WorkId
		
		INSERT INTO #result (WorkId, ComputerCorrectionTime) --this should not happen (when WorkTime is not 0)...
		SELECT WorkId, -1 * WorkTime
		FROM #workTimeTable w
		WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
		
		DELETE FROM #workTimeTable
		--END merge
	
		SET @prevEndDate = @currEndDate
	END
	
	CLOSE manualworkitem_cursor
	DEALLOCATE manualworkitem_cursor

	-------------------------------------------------------------------------------------------------------
	--IvrCorrectionTime
	-------------------------------------------------------------------------------------------------------
	--get disjoint correction intervals, so an ivrworkitem will only be calculated once
	SET @prevEndDate = NULL
	
	DECLARE manualworkitem_cursor CURSOR FAST_FORWARD FOR 
	SELECT
		WorkId, StartDate, EndDate
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND (ManualWorkItemTypeId = 1 OR  ManualWorkItemTypeId = 2) -- Deleted Interval or Deleted Ivr Interval
	ORDER BY StartDate
	
	OPEN manualworkitem_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM manualworkitem_cursor INTO 
			@c_WorkId,
			@c_StartDate,
			@c_EndDate

		IF @@FETCH_STATUS <> 0
			BREAK
		
		SET @currStartDate = @c_StartDate
		SET @currEndDate = @c_EndDate
			
		IF (@prevEndDate IS NOT NULL AND @currStartDate < @prevEndDate ) SET @currStartDate = @prevEndDate
		IF (@endDate < @currEndDate) SET @currEndDate = @endDate
		IF (@currEndDate < @currStartDate) CONTINUE
		
		
		INSERT #workTimeTable EXEC dbo.GetIvrWorkTimeByWorkIdForUser @userId=@userId, @startDate=@currStartDate, @endDate=@currEndDate
		
		--BEGIN merge of IvrCorrectionTime
		UPDATE #result SET IvrCorrectionTime = IvrCorrectionTime - w.WorkTime
		FROM #workTimeTable w
		JOIN #result r ON w.WorkId = r.WorkId
		
		INSERT INTO #result (WorkId, IvrCorrectionTime) --this should not happen (when WorkTime is not 0)...
		SELECT WorkId, -1 * WorkTime
		FROM #workTimeTable w
		WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
		
		DELETE FROM #workTimeTable
		--END merge
	
		SET @prevEndDate = @currEndDate
	END
	
	CLOSE manualworkitem_cursor
	DEALLOCATE manualworkitem_cursor

	-------------------------------------------------------------------------------------------------------
	--MobileCorrectionTime
	-------------------------------------------------------------------------------------------------------
	--get disjoint correction intervals, so an mobileworkitem will only be calculated once
	SET @prevEndDate = NULL
	
	DECLARE manualworkitem_cursor CURSOR FAST_FORWARD FOR 
	SELECT
		WorkId, StartDate, EndDate
	FROM
		ManualWorkItems
	WHERE
		UserId = @userId
	AND StartDate < @endDate
	AND StartDate >= DATEADD(day, -2, @startDate)
	AND EndDate > @startDate
	AND (ManualWorkItemTypeId = 1 OR  ManualWorkItemTypeId = 6) -- Deleted Interval or Deleted Mobile Interval
	ORDER BY StartDate
	
	OPEN manualworkitem_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM manualworkitem_cursor INTO 
			@c_WorkId,
			@c_StartDate,
			@c_EndDate

		IF @@FETCH_STATUS <> 0
			BREAK
		
		SET @currStartDate = @c_StartDate
		SET @currEndDate = @c_EndDate
			
		IF (@prevEndDate IS NOT NULL AND @currStartDate < @prevEndDate ) SET @currStartDate = @prevEndDate
		IF (@endDate < @currEndDate) SET @currEndDate = @endDate
		IF (@currEndDate < @currStartDate) CONTINUE
		
		
		INSERT #workTimeTable EXEC dbo.GetMobileWorkTimeByWorkIdForUser @userId=@userId, @startDate=@currStartDate, @endDate=@currEndDate
		
		--BEGIN merge of IvrCorrectionTime
		UPDATE #result SET MobileCorrectionTime = MobileCorrectionTime - w.WorkTime
		FROM #workTimeTable w
		JOIN #result r ON w.WorkId = r.WorkId
		
		INSERT INTO #result (WorkId, MobileCorrectionTime) --this should not happen (when WorkTime is not 0)...
		SELECT WorkId, -1 * WorkTime
		FROM #workTimeTable w
		WHERE NOT EXISTS (SELECT 1 FROM #result r WHERE r.WorkId=w.WorkId)
		
		DELETE FROM #workTimeTable
		--END merge
	
		SET @prevEndDate = @currEndDate
	END
	
	CLOSE manualworkitem_cursor
	DEALLOCATE manualworkitem_cursor

	-------------------------------------------------------------------------------------------------------
	--TotalWorkTime
	-------------------------------------------------------------------------------------------------------
	UPDATE #result SET TotalWorkTime = ComputerWorkTime 
									+ ComputerCorrectionTime 
									+ IvrWorkTime 
									+ IvrCorrectionTime 
									+ MobileWorkTime
									+ MobileCorrectionTime
									+ ManualWorkTime
									+ HolidayTime
									+ SickLeaveTime
	
	-------------------------------------------------------------------------------------------------------
	SELECT * FROM #result
	-------------------------------------------------------------------------------------------------------
	RETURN
' 
END
GO
/****** Object:  StoredProcedure [dbo].[ReportClientComputerVersion] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReportClientComputerVersion]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[ReportClientComputerVersion]
	(
	@userId int,
	@computerId int,
	@major int,
	@minor int,
	@build int,
	@revision int
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @userId IS NULL OR @computerId IS NULL OR @major IS NULL OR @minor IS NULL OR @build IS NULL OR @revision IS NULL
	BEGIN
		RAISERROR(''@userId, @computerId, @major, @minor, @build and @revision cannot be NULL'', 16, 1)
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
   AND [IsCurrent] = 1

IF (@@rowcount > 1)
BEGIN
	RAISERROR(''More than one current versions'', 16, 1)
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
	   AND [IsCurrent] = 1
END
ELSE --new version
BEGIN
	UPDATE [dbo].[ClientComputerVersions]
	   SET [IsCurrent] = 0
	 WHERE [UserId] = @userId
	   AND [ComputerId] = @computerId
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
			   ,[LastReceiveDate])
		 VALUES
			   (@userId
			   ,@computerId
			   ,@major
			   ,@minor
			   ,@build
			   ,@revision
			   ,1
			   ,GETUTCDATE()
			   ,GETUTCDATE())
END

COMMIT TRAN
	RETURN
' 
END
GO
/****** Object:  StoredProcedure [dbo].[ReportClientComputerAddress] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReportClientComputerAddress]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[ReportClientComputerAddress]
	(
	@userId int,
	@computerId int,
	@address varchar(39)
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @userId IS NULL OR @computerId IS NULL
	BEGIN
		RAISERROR(''@userId and @computerId cannot be NULL'', 16, 1)
		RETURN
	END

BEGIN TRAN

declare @cId int, @cAddress varchar(39)

SELECT @cId = [Id]
      ,@cAddress = [Address]
  FROM [dbo].[ClientComputerAddresses] WITH (UPDLOCK, HOLDLOCK)
 WHERE [UserId] = @userId
   AND [ComputerId] = @computerId
   AND [IsCurrent] = 1

IF (@@rowcount > 1)
BEGIN
	RAISERROR(''More than one current versions'', 16, 1)
	ROLLBACK
	RETURN
END

--same version
IF (@cId IS NOT NULL AND ((@address IS NULL AND @cAddress IS NULL) OR (@address IS NOT NULL AND @cAddress IS NOT NULL AND @address = @cAddress)))
BEGIN
	UPDATE [dbo].[ClientComputerAddresses]
	   SET [LastReceiveDate] = GETUTCDATE()
	 WHERE [UserId] = @userId
	   AND [ComputerId] = @computerId
	   AND [IsCurrent] = 1
END
ELSE --new version
BEGIN
	UPDATE [dbo].[ClientComputerAddresses]
	   SET [IsCurrent] = 0
	 WHERE [UserId] = @userId
	   AND [ComputerId] = @computerId
	   AND [IsCurrent] = 1

	INSERT INTO [dbo].[ClientComputerAddresses]
			   ([UserId]
			   ,[ComputerId]
			   ,[Address]
			   ,[IsCurrent]
			   ,[FirstReceiveDate]
			   ,[LastReceiveDate])
		 VALUES
			   (@userId
			   ,@computerId
			   ,@address
			   ,1
			   ,GETUTCDATE()
			   ,GETUTCDATE())
END

COMMIT TRAN
	RETURN
' 
END
GO
/****** Object:  Table [dbo].[ParallelWorkItems] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ParallelWorkItems]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ParallelWorkItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParallelWorkItemTypeId] [smallint] NOT NULL,
	[WorkId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ParallelWorkItems] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ParallelWorkItems]') AND name = N'IX_ParallelWorkItems_UserId_StartDate_Clust')
CREATE CLUSTERED INDEX [IX_ParallelWorkItems_UserId_StartDate_Clust] ON [dbo].[ParallelWorkItems] 
(
	[UserId] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  StoredProcedure [dbo].[UpsertVoiceRecordings] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpsertVoiceRecordings]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[UpsertVoiceRecordings]
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
			  ,[WorkId] = @workId
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
						  AND [StartDate] = @startDate
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
END
GO
/****** Object:  StoredProcedure [dbo].[UpsertClientComputerError] ******/
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
/****** Object:  StoredProcedure [dbo].[UpdateIvrWorkItemsWithComputerActivity] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateIvrWorkItemsWithComputerActivity]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[UpdateIvrWorkItemsWithComputerActivity]
AS
	SET NOCOUNT ON

declare @result table (
	IvrWorkItemId int NOT NULL,
	StartDate datetime NOT NULL,
	OldEndDate datetime,
	NewEndDate datetime NOT NULL,
	UserId int NOT NULL,
	WorkId int NOT NULL,
	InstantNotificationEmail bit NOT NULL
	)
	
declare
@c_Id int,
@c_WorkId int,
@c_StartDate datetime,
@c_EndDate datetime,
@c_MaxEndDate datetime,
@c_UserId int,
@c_GroupId int,
@c_CompanyId int,
@c_PhoneNumber varchar(50),
@c_TrunkId varchar(50),
@c_AutoEndOnComputerActivity bit,
@c_InstantNotificationEmail bit

--TODO LogoffMaxEndDate in not handled

DECLARE ivrworkitem_cursor CURSOR FORWARD_ONLY FOR 	
SELECT [Id]
      ,[WorkId]
      ,[StartDate]
      ,[EndDate]
      ,[MaxEndDate]
      ,[UserId]
      ,[GroupId]
      ,[CompanyId]
      ,[PhoneNumber]
      ,[TrunkId]
      ,[AutoEndOnComputerActivity]
      ,[InstantNotificationEmail]
  FROM [dbo].[IvrWorkItems]
  WHERE
		[AutoEndOnComputerActivity] = 1
		--AND ([EndDate] IS NULL OR [StartDate] <> [EndDate]) -- if duration is 0 then no update needed
		AND (DATEDIFF(hour, [StartDate], [MaxEndDate]) < 200) --protect WorkItems from big queries
	
	
OPEN ivrworkitem_cursor

WHILE 1=1
BEGIN
	FETCH NEXT FROM ivrworkitem_cursor INTO 
		@c_Id,
		@c_WorkId,
		@c_StartDate,
		@c_EndDate,
		@c_MaxEndDate,
		@c_UserId,
		@c_GroupId,
		@c_CompanyId,
		@c_PhoneNumber,
		@c_TrunkId,
		@c_AutoEndOnComputerActivity,
		@c_InstantNotificationEmail

	IF @@FETCH_STATUS <> 0
		BREAK
		
	declare @computerStartDate datetime
	SET @computerStartDate = (	
								SELECT
									MIN(StartDate) 
								FROM 
									[dbo].[WorkItems]
								WHERE
									UserId = @c_UserId
									AND StartDate >= DATEADD(minute, 5, @c_StartDate)
									AND StartDate < ISNULL(@c_EndDate, @c_MaxEndDate)
							 )

	IF (@computerStartDate IS NOT NULL)
	BEGIN
		UPDATE [dbo].[IvrWorkItems]
		   SET [EndDate] = @computerStartDate
		 WHERE
			   [Id] = @c_Id
			   AND ((@c_EndDate IS NULL AND [EndDate] IS NULL) OR [EndDate] = @c_EndDate)
			   
		IF @@ROWCOUNT<>0
		BEGIN
			INSERT INTO @result (IvrWorkItemId,	StartDate, OldEndDate, NewEndDate, UserId, WorkId, InstantNotificationEmail) VALUES
			(@c_Id, @c_StartDate, @c_EndDate, @computerStartDate, @c_UserId, @c_WorkId, @c_InstantNotificationEmail)
		END
	END
END
CLOSE ivrworkitem_cursor
DEALLOCATE ivrworkitem_cursor	

SELECT * FROM @result
	
	RETURN
' 
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateHourlyAggregateWorkItemsFromId] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateHourlyAggregateWorkItemsFromId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
--this sproc should not be called directly, its only called by [UpdateHourlyAggregateWorkItems]
CREATE PROCEDURE [dbo].[UpdateHourlyAggregateWorkItemsFromId]
	(
	@StartId bigint,
	@EndId bigint OUTPUT,
	@MinStartDate datetime = NULL OUTPUT
	)
AS
	SET NOCOUNT ON

SET XACT_ABORT ON
BEGIN TRAN
SET @EndId = @StartId

declare @StartIdChk bigint
SET @StartIdChk = (SELECT ISNULL(MAX(LastAggregatedId),0) FROM dbo.AggregateLastWorkItem)
IF @StartId <> @StartIdChk
BEGIN
	RAISERROR(''UpdateHourlyAggregateWorkItemsFromId called with wrong StartId'',16,1)
	ROLLBACK
	RETURN
END

declare @MaxEndId bigint
SET @MaxEndId = (SELECT ISNULL(MAX(Id),0) FROM [dbo].[WorkItems] WITH (TABLOCK)) -- I suppose TABLOCK is enough and no TABLOCKX needed
--using TABLOCKX (without HOLDLOCK) would cause to hold the lock until the end of the transaction.

declare
@c_Id bigint,
@c_WorkId int,
@c_PhaseId uniqueidentifier,
@c_StartDate datetime,
@c_EndDate datetime,
@c_UserId int,
@c_GroupId int,
@c_CompanyId int,
@c_ComputerId int,
@c_MouseActivity int,
@c_KeyboardActivity int

DECLARE interval_cursor CURSOR LOCAL FAST_FORWARD FOR 
SELECT [Id]
      ,[WorkId]
      ,[PhaseId]
      ,[StartDate]
      ,[EndDate]
      ,[UserId]
      ,[GroupId]
      ,[CompanyId]
      ,[ComputerId]
      ,[MouseActivity]
      ,[KeyboardActivity]
  FROM [dbo].[WorkItems] --(TABLOCKX) we don''t need locking if we have the @MaxEndId
 WHERE [Id] > @StartId
   AND [Id] <= @MaxEndId


OPEN interval_cursor

WHILE 1=1
BEGIN
	FETCH NEXT FROM interval_cursor INTO 
		@c_Id,
		@c_WorkId,
		@c_PhaseId,
		@c_StartDate,
		@c_EndDate,
		@c_UserId,
		@c_GroupId,
		@c_CompanyId,
		@c_ComputerId,
		@c_MouseActivity,
		@c_KeyboardActivity

	IF @@FETCH_STATUS <> 0
		BREAK

	IF (@EndId<@c_Id)  SET @EndId = @c_Id
	IF (@c_MouseActivity<0) SET @c_MouseActivity = 0
	IF (@c_KeyboardActivity<0) SET @c_KeyboardActivity = 0
	IF (@c_StartDate>=@c_EndDate) CONTINUE
	IF (@MinStartDate IS NULL OR @MinStartDate > @c_StartDate) SET @MinStartDate = @c_StartDate
	declare @interval_StartDate datetime, @interval_EndDate datetime, @Curr_StartDate datetime, @Curr_EndDate datetime
	declare @Rem_MouseActivity int, @Rem_KeyboardActivity int

	SET @interval_StartDate =  CONVERT(CHAR(13), @c_StartDate, 126) + '':00:00''
	SET @interval_EndDate = CONVERT(CHAR(13), @c_EndDate, 126) + '':00:00''
	SET @Curr_StartDate = @c_StartDate
	SET @Rem_MouseActivity = @c_MouseActivity
	SET @Rem_KeyboardActivity = @c_KeyboardActivity

	WHILE (@interval_StartDate<=@interval_EndDate)
	BEGIN
		declare @duration int, @Curr_MouseActivity int, @Curr_KeyboardActivity int
		IF (@interval_StartDate=@interval_EndDate) --last interval
		BEGIN
			SET @Curr_EndDate = @c_EndDate
		END
		ELSE
		BEGIN
			SET @Curr_EndDate = DATEADD(hour,1,@interval_StartDate)
		END
		SET @duration = DATEDIFF(SECOND, @Curr_StartDate, @Curr_EndDate)*1000 + DATEPART(MILLISECOND, @Curr_EndDate) - DATEPART(MILLISECOND, @Curr_StartDate)
		--SET @duration = DATEDIFF(millisecond, @Curr_StartDate, @Curr_EndDate) -- not accurate enough
		IF (@duration<=0) BREAK
		IF (@Curr_EndDate = @c_EndDate) --last interval
		BEGIN
			SET @Curr_MouseActivity = @Rem_MouseActivity
			SET @Curr_KeyboardActivity = @Rem_KeyboardActivity
		END
		ELSE
		BEGIN
			declare @wholeDuration int
			SET @wholeDuration = DATEDIFF(SECOND, @c_StartDate, @c_EndDate)*1000 + DATEPART(MILLISECOND, @c_EndDate) - DATEPART(MILLISECOND, @c_StartDate)
			SET @Curr_MouseActivity = CAST(ROUND(CAST(@duration AS float) / @wholeDuration * @c_MouseActivity, 0) AS int)
			SET @Curr_KeyboardActivity = CAST(ROUND(CAST(@duration AS float) / @wholeDuration * @c_KeyboardActivity, 0) AS int)

			SET @Rem_MouseActivity = @Rem_MouseActivity - @Curr_MouseActivity
			SET @Rem_KeyboardActivity = @Rem_KeyboardActivity - @Curr_KeyboardActivity
		END

		IF EXISTS(SELECT NULL FROM [dbo].[AggregateWorkItems] 
								WHERE StartDate = @interval_StartDate --enddate should match
								  AND WorkId = @c_WorkId
								  AND UserId = @c_UserId
								  AND GroupId = @c_GroupId
								  AND CompanyId = @c_CompanyId
								  AND ComputerId = @c_ComputerId)
		BEGIN
			UPDATE [dbo].[AggregateWorkItems] SET
				   [WorkTime] = [WorkTime] + @duration,
				   [MouseActivity] = [MouseActivity] + @Curr_MouseActivity,
				   [KeyboardActivity] = [KeyboardActivity] + @Curr_KeyboardActivity,
				   [UpdateDate] = GETUTCDATE()
			 WHERE StartDate = @interval_StartDate --enddate should match
			   AND WorkId = @c_WorkId
			   AND UserId = @c_UserId
			   AND GroupId = @c_GroupId
			   AND CompanyId = @c_CompanyId
			   AND ComputerId = @c_ComputerId
		END
		ELSE
		BEGIN
			INSERT INTO [dbo].[AggregateWorkItems]
					   ([StartDate]
					   ,[EndDate]
					   ,[WorkTime]
					   ,[WorkId]
					   ,[UserId]
					   ,[GroupId]
					   ,[CompanyId]
					   ,[ComputerId]
					   ,[MouseActivity]
					   ,[KeyboardActivity])
				 VALUES
					   (@interval_StartDate
					   ,DATEADD(hour,1,@interval_StartDate)
					   ,@duration
					   ,@c_WorkId
					   ,@c_UserId
					   ,@c_GroupId
					   ,@c_CompanyId
					   ,@c_ComputerId
					   ,@Curr_MouseActivity
					   ,@Curr_KeyboardActivity)
		END

		SET @interval_StartDate = DATEADD(hour,1,@interval_StartDate)
		SET @Curr_StartDate = @interval_StartDate
	END
	
	--BEGIN update AggregateWorkItemIntervals
	UPDATE TOP (1) [dbo].[AggregateWorkItemIntervals] 
	   SET [EndDate] = @c_EndDate,
		   [UpdateDate] = GETUTCDATE()
	 WHERE EndDate = @c_StartDate --enddate should match
	   AND PhaseId = @c_PhaseId
	   AND WorkId = @c_WorkId
	   AND UserId = @c_UserId
	   AND GroupId = @c_GroupId
	   AND CompanyId = @c_CompanyId
	   AND ComputerId = @c_ComputerId
	   
	IF @@rowcount = 0
	BEGIN
		INSERT INTO [dbo].[AggregateWorkItemIntervals]
				   ([WorkId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[UserId]
				   ,[GroupId]
				   ,[CompanyId]
				   ,[PhaseId]
				   ,[ComputerId]
				   ,[CreateDate]
				   ,[UpdateDate])
			 VALUES
				   (@c_WorkId
				   ,@c_StartDate
				   ,@c_EndDate
				   ,@c_UserId
				   ,@c_GroupId
				   ,@c_CompanyId
				   ,@c_PhaseId
				   ,@c_ComputerId
				   ,GETUTCDATE()
				   ,GETUTCDATE())
	END	
	--END update AggregateWorkItemIntervals

	--BEGIN update AggregateIdleIntervals
	IF @c_MouseActivity = 0 AND @c_KeyboardActivity = 0
	BEGIN
		UPDATE TOP (1) [dbo].[AggregateIdleIntervals] 
		   SET [EndDate] = @c_EndDate,
			   [UpdateDate] = GETUTCDATE()
		 WHERE EndDate = @c_StartDate --enddate should match
		   AND PhaseId = @c_PhaseId
		   AND WorkId = @c_WorkId
		   AND UserId = @c_UserId
		   AND GroupId = @c_GroupId
		   AND CompanyId = @c_CompanyId
		   AND ComputerId = @c_ComputerId
	   
		IF @@rowcount = 0
		BEGIN
			INSERT INTO [dbo].[AggregateIdleIntervals]
					   ([WorkId]
					   ,[StartDate]
					   ,[EndDate]
					   ,[UserId]
					   ,[GroupId]
					   ,[CompanyId]
					   ,[PhaseId]
					   ,[ComputerId]
					   ,[CreateDate]
					   ,[UpdateDate])
				 VALUES
					   (@c_WorkId
					   ,@c_StartDate
					   ,@c_EndDate
					   ,@c_UserId
					   ,@c_GroupId
					   ,@c_CompanyId
					   ,@c_PhaseId
					   ,@c_ComputerId
					   ,GETUTCDATE()
					   ,GETUTCDATE())
		END
	END
	--END update AggregateIdleIntervals
END
CLOSE interval_cursor
DEALLOCATE interval_cursor

COMMIT TRAN	
	
	RETURN
' 
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateHourlyAggregateWorkItems] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateHourlyAggregateWorkItems]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[UpdateHourlyAggregateWorkItems]
AS
	SET NOCOUNT ON

SET XACT_ABORT ON
BEGIN TRAN
declare @StartId bigint, @EndId bigint, @MinStartDate datetime
SET @StartId = (SELECT ISNULL(MAX(LastAggregatedId),0) FROM dbo.AggregateLastWorkItem WITH (TABLOCKX, HOLDLOCK))

exec dbo.[UpdateHourlyAggregateWorkItemsFromId] @StartId, @EndId OUT, @MinStartDate OUT

IF @MinStartDate IS NOT NULL -- we have new aggregated intervals 
BEGIN
	exec dbo.[MergeAggregateWorkItemIntervals] @MinStartDate

	exec dbo.[MergeAggregateIdleIntervals] @MinStartDate
END

TRUNCATE TABLE dbo.AggregateLastWorkItem

INSERT INTO dbo.AggregateLastWorkItem VALUES (@EndId)

COMMIT TRAN	
	
	RETURN
' 
END
GO
/****** Object:  Table [dbo].[IvrUserWorks] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IvrUserWorks]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[IvrUserWorks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PhoneNumber] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TrunkId] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[UserId] [int] NOT NULL,
	[WorkId] [int] NOT NULL,
	[RuleId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_IvrUserWorks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON),
 CONSTRAINT [IX_IvrWorks_Unique] UNIQUE NONCLUSTERED 
(
	[PhoneNumber] ASC,
	[TrunkId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Trigger [IvrRules_Validation] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[IvrRules_Validation]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[IvrRules_Validation]
ON [dbo].[IvrRules]
AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	
	IF EXISTS(SELECT * FROM inserted WHERE StartTime<0 OR EndTime<0 OR IncrementInside<0 OR IncrementOutside<0)
	BEGIN
		RAISERROR(''Cannot insert negative values for StartTime, EndTime, IncrementInside, IncrementOutside'',16,1)
		ROLLBACK
		RETURN
	END
	IF EXISTS(SELECT * FROM inserted WHERE EndTime<StartTime)
	BEGIN
		RAISERROR(''StartTime should be less than or equal to EndTime'',16,1)
		ROLLBACK
		RETURN
	END
	IF EXISTS(SELECT * FROM inserted WHERE IncrementInsideMaxTime IS NOT NULL AND 
				(
				(IncrementInsideMaxTime<EndTime)
				OR
				(IncrementInsideMaxTime>EndTime+IncrementInside)
				))
	BEGIN
		RAISERROR(''IncrementInsideMaxTime should be between EndTime and EndTime+IncrementInside'',16,1)
		ROLLBACK
		RETURN
	END
	IF EXISTS(SELECT * FROM inserted WHERE LogoffMaxEndTime IS NOT NULL AND 
				(
				(IncrementInsideMaxTime IS NOT NULL AND LogoffMaxEndTime<IncrementInsideMaxTime)
				OR
				(IncrementInsideMaxTime IS NULL AND LogoffMaxEndTime<EndTime+IncrementInside)
				))
	BEGIN
		RAISERROR(''LogoffMaxEndTime should be greater than or equal to IncrementInsideMaxTime ?? EndTime+IncrementInside'',16,1)
		ROLLBACK
		RETURN
	END

END
'
GO
/****** Object:  Table [dbo].[ClientSettings] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientSettings]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClientSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Version] [timestamp] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[Menu] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[MenuUpdateInterval] [int] NULL,
	[CaptureWorkItemInterval] [int] NULL,
	[CaptureActiveWindowInterval] [int] NULL,
	[CaptureScreenShotInterval] [int] NULL,
	[TimeSyncThreshold] [int] NULL,
	[JpegQuality] [int] NULL,
	[JpegScalePct] [int] NULL,
	[WorkTimeStartInMins] [int] NULL,
	[WorkTimeEndInMins] [int] NULL,
	[AfterWorkTimeIdleInMins] [int] NULL,
	[MaxOfflineWorkItems] [int] NULL,
	[WorkDetectorRules] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CensorRules] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[DuringWorkTimeIdleInMins] [int] NULL,
	[DuringWorkTimeIdleManualInterval] [int] NULL,
	[MaxManualMeetingInterval] [int] NULL,
	[RuleRestrictions] [int] NULL,
	[IsMeetingTrackingEnabled] [bit] NULL,
	[IsMeetingSubjectMandatory] [bit] NULL,
	[BusyTimeThreshold] [int] NULL,
	[CoincidentalClientsEnabled] [bit] NULL,
	[IsManualMeetingStartsOnLock] [bit] NULL,
	[IsLotusNotesMeetingTrackingEnabled] [bit] NULL,
	[RuleMatchingInterval] [int] NULL,
	[MenuVersion] [binary](8) NOT NULL,
	[WorkDetectorRulesVersion] [binary](8) NOT NULL,
	[CensorRulesVersion] [binary](8) NOT NULL,
	[ClientSettingsVersion] [binary](8) NOT NULL,
 CONSTRAINT [PK_ClientSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ClientSettings]') AND name = N'IX_ClientSettings_UserId')
CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientSettings_UserId] ON [dbo].[ClientSettings] 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Trigger [CalendarExceptions_Validation] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[CalendarExceptions_Validation]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[CalendarExceptions_Validation]
ON [dbo].[CalendarExceptions]
AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	
	IF EXISTS(SELECT * FROM inserted WHERE Date <> CAST(FLOOR(CAST(Date as float)) as datetime))
	BEGIN
		RAISERROR(''Date should have no time part'',16,1)
		ROLLBACK
		RETURN
	END
END'
GO
/****** Object:  UserDefinedFunction [dbo].[GetFlattenedCalendarExceptions] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFlattenedCalendarExceptions]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE FUNCTION [dbo].[GetFlattenedCalendarExceptions] 
(	
	@calendarId int 
)
RETURNS @results TABLE (Date datetime, IsWorkDay bit)
AS
BEGIN

	INSERT INTO @results
	SELECT Date, IsWorkDay 
	FROM
	(
		SELECT ce.Date, ce.IsWorkDay, RANK() OVER (PARTITION BY ce.Date ORDER BY ic.Level) AS Rank
		FROM CalendarExceptions ce
		JOIN dbo.GetInheritedCalendarIds(@calendarId) ic ON ce.CalendarId = ic.Id
	) tmp
	WHERE
	Rank = 1
	
	RETURN
	/*
	Basically its the same as:
	SELECT ce.Date, ce.IsWorkDay, ic.Level
	FROM CalendarExceptions ce
	JOIN dbo.GetInheritedCalendarIds(@calendarId) ic ON ce.CalendarId = ic.Id
	JOIN
		(SELECT Date, MIN(Level) AS Level
		FROM CalendarExceptions ce
		JOIN dbo.GetInheritedCalendarIds(@calendarId) ic ON ce.CalendarId = ic.Id
		GROUP BY Date) filter ON filter.Date = ce.Date AND filter.Level = ic.Level
	*/
END
' 
END
GO
/****** Object:  Table [dbo].[DesktopWindows] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DesktopWindows]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DesktopWindows](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DesktopCaptureId] [bigint] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[ProcessNameId] [int] NOT NULL,
	[TitleId] [int] NOT NULL,
	[UrlId] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[X] [smallint] NOT NULL,
	[Y] [smallint] NOT NULL,
	[Width] [smallint] NOT NULL,
	[Height] [smallint] NOT NULL,
	[ClientArea] [int] NOT NULL,
	[VisibleClientArea] [int] NOT NULL,
 CONSTRAINT [PK_DesktopWindows] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DesktopWindows]') AND name = N'IX_DesktopWindows_CreateDate_Clust')
CREATE CLUSTERED INDEX [IX_DesktopWindows_CreateDate_Clust] ON [dbo].[DesktopWindows] 
(
	[CreateDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  Table [dbo].[Screens] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Screens]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Screens](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DesktopCaptureId] [bigint] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[X] [smallint] NOT NULL,
	[Y] [smallint] NOT NULL,
	[Width] [smallint] NOT NULL,
	[Height] [smallint] NOT NULL,
	[ScreenNumber] [tinyint] NOT NULL,
	[Extension] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_Screens] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Screens]') AND name = N'IX_Screens_CreateDate_Clust')
CREATE CLUSTERED INDEX [IX_Screens_CreateDate_Clust] ON [dbo].[Screens] 
(
	[CreateDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  View [dbo].[ScreenShots] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ScreenShots]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[ScreenShots]
AS
SELECT     dbo.Screens.Id, dbo.DesktopCaptures.WorkItemId, dbo.Screens.CreateDate, dbo.Screens.CreateDate AS ReceiveDate, dbo.Screens.ScreenNumber, 
                      dbo.Screens.Extension
FROM         dbo.DesktopCaptures INNER JOIN
                      dbo.Screens ON dbo.DesktopCaptures.Id = dbo.Screens.DesktopCaptureId
'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'ScreenShots', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "DesktopCaptures"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 91
               Right = 190
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Screens"
            Begin Extent = 
               Top = 6
               Left = 228
               Bottom = 243
               Right = 397
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 1380
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ScreenShots'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'ScreenShots', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ScreenShots'
GO
/****** Object:  Trigger [ClientSettings_Versions_U] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[ClientSettings_Versions_U]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[ClientSettings_Versions_U]
	ON [dbo].[ClientSettings]
	AFTER UPDATE
	AS
	BEGIN
		SET NOCOUNT ON
		DECLARE @nextValue BINARY(8)
		EXEC GetNextValueForSequence @nextValue OUTPUT
		IF ( UPDATE (Menu) ) 
		BEGIN
			UPDATE [ClientSettings] SET MenuVersion = @nextValue WHERE UserId IN (SELECT i.UserId FROM inserted i JOIN deleted d ON i.UserId = d.UserId WHERE i.Menu <> d.Menu OR (i.Menu IS NOT NULL AND d.Menu IS NULL) OR (i.Menu IS NULL AND d.Menu IS NOT NULL))
		END
		IF ( UPDATE (WorkDetectorRules) ) 
		BEGIN
			UPDATE [ClientSettings] SET WorkDetectorRulesVersion = @nextValue WHERE UserId IN (SELECT i.UserId FROM inserted i JOIN deleted d ON i.UserId = d.UserId WHERE i.WorkDetectorRules <> d.WorkDetectorRules OR (i.WorkDetectorRules IS NOT NULL AND d.WorkDetectorRules IS NULL) OR (i.WorkDetectorRules IS NULL AND d.WorkDetectorRules IS NOT NULL))
		END
		IF ( UPDATE (CensorRules) ) 
		BEGIN
			UPDATE [ClientSettings] SET CensorRulesVersion = @nextValue WHERE UserId IN (SELECT i.UserId FROM inserted i JOIN deleted d ON i.UserId = d.UserId WHERE i.CensorRules <> d.CensorRules OR (i.CensorRules IS NOT NULL AND d.CensorRules IS NULL) OR (i.CensorRules IS NULL AND d.CensorRules IS NOT NULL))
		END
		IF (UPDATE(MenuUpdateInterval) OR UPDATE(CaptureWorkItemInterval) OR UPDATE(CaptureActiveWindowInterval) OR UPDATE(CaptureScreenShotInterval)
			OR UPDATE(TimeSyncThreshold) OR UPDATE(JpegQuality) OR UPDATE(JpegScalePct) OR UPDATE(WorkTimeStartInMins)
			OR UPDATE(WorkTimeEndInMins) OR UPDATE(AfterWorkTimeIdleInMins) OR UPDATE(MaxOfflineWorkItems) OR UPDATE(DuringWorkTimeIdleInMins)
			OR UPDATE(DuringWorkTimeIdleManualInterval) OR UPDATE(MaxManualMeetingInterval) OR UPDATE(RuleRestrictions) OR UPDATE(IsMeetingTrackingEnabled)
			OR UPDATE(IsMeetingSubjectMandatory) OR UPDATE(BusyTimeThreshold) OR UPDATE(CoincidentalClientsEnabled) OR UPDATE(IsManualMeetingStartsOnLock)
			OR UPDATE(IsLotusNotesMeetingTrackingEnabled) OR UPDATE(RuleMatchingInterval))
		BEGIN
			UPDATE [ClientSettings] SET ClientSettingsVersion = @nextValue 
			WHERE UserId IN 
			(
				SELECT i.UserId FROM inserted i JOIN deleted d ON i.UserId = d.UserId 
				WHERE i.MenuUpdateInterval <> d.MenuUpdateInterval OR (i.MenuUpdateInterval IS NOT NULL AND d.MenuUpdateInterval IS NULL) OR (i.MenuUpdateInterval IS NULL AND d.MenuUpdateInterval IS NOT NULL)
				OR i.CaptureWorkItemInterval <> d.CaptureWorkItemInterval OR (i.CaptureWorkItemInterval IS NOT NULL AND d.CaptureWorkItemInterval IS NULL) OR (i.CaptureWorkItemInterval IS NULL AND d.CaptureWorkItemInterval IS NOT NULL)
				OR i.CaptureActiveWindowInterval <> d.CaptureActiveWindowInterval OR (i.CaptureActiveWindowInterval IS NOT NULL AND d.CaptureActiveWindowInterval IS NULL) OR (i.CaptureActiveWindowInterval IS NULL AND d.CaptureActiveWindowInterval IS NOT NULL)
				OR i.CaptureScreenShotInterval <> d.CaptureScreenShotInterval OR (i.CaptureScreenShotInterval IS NOT NULL AND d.CaptureScreenShotInterval IS NULL) OR (i.CaptureScreenShotInterval IS NULL AND d.CaptureScreenShotInterval IS NOT NULL)
				OR i.TimeSyncThreshold <> d.TimeSyncThreshold OR (i.TimeSyncThreshold IS NOT NULL AND d.TimeSyncThreshold IS NULL) OR (i.TimeSyncThreshold IS NULL AND d.TimeSyncThreshold IS NOT NULL)
				OR i.JpegQuality <> d.JpegQuality OR (i.JpegQuality IS NOT NULL AND d.JpegQuality IS NULL) OR (i.JpegQuality IS NULL AND d.JpegQuality IS NOT NULL)
				OR i.JpegScalePct <> d.JpegScalePct OR (i.JpegScalePct IS NOT NULL AND d.JpegScalePct IS NULL) OR (i.JpegScalePct IS NULL AND d.JpegScalePct IS NOT NULL)
				OR i.WorkTimeStartInMins <> d.WorkTimeStartInMins OR (i.WorkTimeStartInMins IS NOT NULL AND d.WorkTimeStartInMins IS NULL) OR (i.WorkTimeStartInMins IS NULL AND d.WorkTimeStartInMins IS NOT NULL)
				OR i.WorkTimeEndInMins <> d.WorkTimeEndInMins OR (i.WorkTimeEndInMins IS NOT NULL AND d.WorkTimeEndInMins IS NULL) OR (i.WorkTimeEndInMins IS NULL AND d.WorkTimeEndInMins IS NOT NULL)
				OR i.AfterWorkTimeIdleInMins <> d.AfterWorkTimeIdleInMins OR (i.AfterWorkTimeIdleInMins IS NOT NULL AND d.AfterWorkTimeIdleInMins IS NULL) OR (i.AfterWorkTimeIdleInMins IS NULL AND d.AfterWorkTimeIdleInMins IS NOT NULL)
				OR i.MaxOfflineWorkItems <> d.MaxOfflineWorkItems OR (i.MaxOfflineWorkItems IS NOT NULL AND d.MaxOfflineWorkItems IS NULL) OR (i.MaxOfflineWorkItems IS NULL AND d.MaxOfflineWorkItems IS NOT NULL)
				OR i.DuringWorkTimeIdleInMins <> d.DuringWorkTimeIdleInMins OR (i.DuringWorkTimeIdleInMins IS NOT NULL AND d.DuringWorkTimeIdleInMins IS NULL) OR (i.DuringWorkTimeIdleInMins IS NULL AND d.DuringWorkTimeIdleInMins IS NOT NULL)
				OR i.DuringWorkTimeIdleManualInterval <> d.DuringWorkTimeIdleManualInterval OR (i.DuringWorkTimeIdleManualInterval IS NOT NULL AND d.DuringWorkTimeIdleManualInterval IS NULL) OR (i.DuringWorkTimeIdleManualInterval IS NULL AND d.DuringWorkTimeIdleManualInterval IS NOT NULL)
				OR i.MaxManualMeetingInterval <> d.MaxManualMeetingInterval OR (i.MaxManualMeetingInterval IS NOT NULL AND d.MaxManualMeetingInterval IS NULL) OR (i.MaxManualMeetingInterval IS NULL AND d.MaxManualMeetingInterval IS NOT NULL)
				OR i.RuleRestrictions <> d.RuleRestrictions OR (i.RuleRestrictions IS NOT NULL AND d.RuleRestrictions IS NULL) OR (i.RuleRestrictions IS NULL AND d.RuleRestrictions IS NOT NULL)
				OR i.IsMeetingTrackingEnabled <> d.IsMeetingTrackingEnabled OR (i.IsMeetingTrackingEnabled IS NOT NULL AND d.IsMeetingTrackingEnabled IS NULL) OR (i.IsMeetingTrackingEnabled IS NULL AND d.IsMeetingTrackingEnabled IS NOT NULL)
				OR i.IsMeetingSubjectMandatory <> d.IsMeetingSubjectMandatory OR (i.IsMeetingSubjectMandatory IS NOT NULL AND d.IsMeetingSubjectMandatory IS NULL) OR (i.IsMeetingSubjectMandatory IS NULL AND d.IsMeetingSubjectMandatory IS NOT NULL)
				OR i.BusyTimeThreshold <> d.BusyTimeThreshold OR (i.BusyTimeThreshold IS NOT NULL AND d.BusyTimeThreshold IS NULL) OR (i.BusyTimeThreshold IS NULL AND d.BusyTimeThreshold IS NOT NULL)
				OR i.CoincidentalClientsEnabled <> d.CoincidentalClientsEnabled OR (i.CoincidentalClientsEnabled IS NOT NULL AND d.CoincidentalClientsEnabled IS NULL) OR (i.CoincidentalClientsEnabled IS NULL AND d.CoincidentalClientsEnabled IS NOT NULL)
				OR i.IsManualMeetingStartsOnLock <> d.IsManualMeetingStartsOnLock OR (i.IsManualMeetingStartsOnLock IS NOT NULL AND d.IsManualMeetingStartsOnLock IS NULL) OR (i.IsManualMeetingStartsOnLock IS NULL AND d.IsManualMeetingStartsOnLock IS NOT NULL)
				OR i.IsLotusNotesMeetingTrackingEnabled <> d.IsLotusNotesMeetingTrackingEnabled OR (i.IsLotusNotesMeetingTrackingEnabled IS NOT NULL AND d.IsLotusNotesMeetingTrackingEnabled IS NULL) OR (i.IsLotusNotesMeetingTrackingEnabled IS NULL AND d.IsLotusNotesMeetingTrackingEnabled IS NOT NULL)
				OR i.RuleMatchingInterval <> d.RuleMatchingInterval OR (i.RuleMatchingInterval IS NOT NULL AND d.RuleMatchingInterval IS NULL) OR (i.RuleMatchingInterval IS NULL AND d.RuleMatchingInterval IS NOT NULL)
			)
		END
	END
'
GO
/****** Object:  Trigger [ClientSettings_Versions_I] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[ClientSettings_Versions_I]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[ClientSettings_Versions_I]
	ON [dbo].[ClientSettings]
	AFTER INSERT
	AS
	BEGIN
		SET NOCOUNT ON
		DECLARE @nextValue BINARY(8)
		EXEC GetNextValueForSequence @nextValue OUTPUT
		UPDATE [dbo].[ClientSettings] SET MenuVersion=@nextValue, WorkDetectorRulesVersion=@nextValue, CensorRulesVersion=@nextValue, ClientSettingsVersion=@nextValue WHERE UserId IN (SELECT UserId FROM inserted)
	END
'
GO
/****** Object:  Table [dbo].[DesktopActiveWindows] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DesktopActiveWindows](
	[Id] [bigint] NOT NULL,
	[DesktopCaptureId] [bigint] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[ProcessNameId] [int] NOT NULL,
	[TitleId] [int] NOT NULL,
	[UrlId] [int] NULL,
 CONSTRAINT [PK_DesktopActiveWindows] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]') AND name = N'IX_DesktopActiveWindows_CreateDate_Clust')
CREATE CLUSTERED INDEX [IX_DesktopActiveWindows_CreateDate_Clust] ON [dbo].[DesktopActiveWindows] 
(
	[CreateDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO
/****** Object:  StoredProcedure [dbo].[GetActiveWindowsGrouppedForUser] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetActiveWindowsGrouppedForUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[GetActiveWindowsGrouppedForUser]
	(
	@userId int,
	@startDate datetime,
	@endDate datetime
	)
AS
	SET NOCOUNT ON
	
	IF @userId IS NULL OR @startDate IS NULL OR @endDate IS NULL
	BEGIN
		RAISERROR(''@userId, @startDate and @endDate cannot be NULL'', 16, 1)
		RETURN
	END

	IF NULLIF(object_id(''tempdb..#Deletions''), 0) IS NOT NULL DROP TABLE #Deletions
	
	SELECT 
		StartDate, EndDate 
	INTO 
		#Deletions
	FROM 
		ManualWorkItems m
	WHERE
		m.UserId = @userId
		AND @startDate < m.EndDate
		AND m.StartDate < @endDate
		AND m.ManualWorkItemTypeId IN (1, 3) -- DeleteInterval, DeleteComputerInterval

	SELECT
		p.ProcessName, t.Title, u.Url, g.[Count]
	FROM
	(
		SELECT
			a.ProcessNameId, a.TitleId, a.UrlId, COUNT(*) AS [Count]
		FROM
			DesktopActiveWindows a
		WHERE
			a.CreateDate >= @startDate 
			AND a.CreateDate < @endDate
			AND a.UserId = @userId
			AND NOT EXISTS (SELECT 1 FROM #Deletions d WHERE d.StartDate <= a.CreateDate AND a.CreateDate <= d.EndDate)
		GROUP BY 
			a.ProcessNameId, a.TitleId, a.UrlId
	)AS g
	JOIN
		ProcessNameLookup p ON p.Id = g.ProcessNameId
	JOIN
		TitleLookup t ON t.Id = g.TitleId
	LEFT JOIN
		UrlLookup u ON u.Id = g.UrlId

	RETURN
' 
END
GO
/****** Object:  Trigger [IvrUserWorks_Validation] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[IvrUserWorks_Validation]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[IvrUserWorks_Validation]
ON [dbo].[IvrUserWorks]
AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	--A PhoneNumber should identify one UserId
	IF EXISTS (	
		SELECT * 
		  FROM (
				SELECT DISTINCT PhoneNumber, UserId FROM IvrUserWorks
			   ) g 
	  GROUP BY g.PhoneNumber HAVING COUNT(*)>1
	)
	BEGIN
		RAISERROR(''A PhoneNumber can only be assigned to one user'',16,1)
		ROLLBACK
		RETURN
	END
END'
GO
/****** Object:  UserDefinedFunction [dbo].[IsWorkDay] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsWorkDay]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE FUNCTION [dbo].[IsWorkDay] 
(
	@calendarId int,
	@date datetime
)
RETURNS bit
WITH RETURNS NULL ON NULL INPUT
AS
BEGIN
	declare @realDate datetime
	SET @realDate = dbo.GetDatePart(@date)
	
	DECLARE @result bit
	SET @result = (
		SELECT IsWorkDay FROM dbo.GetFlattenedCalendarExceptions(@calendarId) ce
		WHERE ce.Date = @realDate
	)
	
	IF (@result IS NULL)
	BEGIN
		SET @result = 
		CASE ((DATEPART(dw, @realDate) + @@DATEFIRST - 1) % 7)
			WHEN 1 THEN (SELECT IsMondayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 2 THEN (SELECT IsTuesdayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 3 THEN (SELECT IsWednesdayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 4 THEN (SELECT IsThursdayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 5 THEN (SELECT IsFridayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 6 THEN (SELECT IsSaturdayWorkDay FROM Calendars WHERE Id = @calendarId)
			WHEN 0 THEN (SELECT IsSundayWorkDay FROM Calendars WHERE Id = @calendarId)
			ELSE NULL
		END
	END

	RETURN @result

END
' 
END
GO
/****** Object:  StoredProcedure [dbo].[InsertDesktopWindow] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertDesktopWindow]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author: Zoltan Torok
-- =============================================
CREATE PROCEDURE [dbo].[InsertDesktopWindow]
	(
	@id bigint output,
	@desktopCaptureId bigint,
	@userId int,
	@createDate datetime output,
	@processNameId int output,
	@titleId int output,
	@urlId int output,
	@isActive bit,
	@x smallint,
	@y smallint,
	@width smallint,
	@height smallint,
	@clientArea int,
	@visibleClientArea int,
	@processName nvarchar(100),
	@title nvarchar(1000),
	@url nvarchar(1000)
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @processNameId IS NULL --if specified we won''t check if it''s correct
		exec GetIdForProcessName @processName, @processNameId output

	IF @titleId IS NULL --if specified we won''t check if it''s correct
		exec GetIdForTitle @title, @titleId output

	IF @urlId IS NULL AND @url IS NOT NULL --if specified we won''t check if it''s correct
		exec GetIdForUrl @url, @urlId output

	SET NOCOUNT OFF

	BEGIN TRAN

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
           (@desktopCaptureId
           ,@userId
           ,@createDate
           ,@processNameId
           ,@titleId
           ,@urlId
           ,@isActive
           ,@x
           ,@y
           ,@width
           ,@height
           ,@clientArea
           ,@visibleClientArea)

	SET NOCOUNT ON
	SET @id = CAST(SCOPE_IDENTITY() as bigint)
	SET @createDate = (
		SELECT [CreateDate]
		  FROM [dbo].[DesktopWindows]
		 WHERE [Id] = SCOPE_IDENTITY()
	)

	IF @isActive = 1
	BEGIN
		INSERT INTO [dbo].[DesktopActiveWindows]
			   ([Id]
			   ,[DesktopCaptureId]
			   ,[UserId]
			   ,[CreateDate]
			   ,[ProcessNameId]
			   ,[TitleId]
			   ,[UrlId])
		 VALUES
			   (@id
			   ,@desktopCaptureId
			   ,@userId
			   ,@createDate
			   ,@processNameId
			   ,@titleId
			   ,@urlId)
	END

	COMMIT TRAN
	RETURN
' 
END
GO
/****** Object:  View [dbo].[ActiveWindows] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[ActiveWindows]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[ActiveWindows]
AS
SELECT     dbo.DesktopActiveWindows.Id, dbo.DesktopCaptures.WorkItemId, dbo.DesktopActiveWindows.CreateDate, 
                      dbo.DesktopActiveWindows.CreateDate AS ReceiveDate, dbo.ProcessNameLookup.ProcessName, dbo.TitleLookup.Title, dbo.UrlLookup.Url
FROM         dbo.DesktopActiveWindows INNER JOIN
                      dbo.DesktopCaptures ON dbo.DesktopCaptures.Id = dbo.DesktopActiveWindows.DesktopCaptureId INNER JOIN
                      dbo.ProcessNameLookup ON dbo.DesktopActiveWindows.ProcessNameId = dbo.ProcessNameLookup.Id INNER JOIN
                      dbo.TitleLookup ON dbo.DesktopActiveWindows.TitleId = dbo.TitleLookup.Id LEFT OUTER JOIN
                      dbo.UrlLookup ON dbo.DesktopActiveWindows.UrlId = dbo.UrlLookup.Id
'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'ActiveWindows', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[26] 2[15] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "DesktopCaptures"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 91
               Right = 190
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "DesktopActiveWindows"
            Begin Extent = 
               Top = 3
               Left = 257
               Bottom = 170
               Right = 426
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ProcessNameLookup"
            Begin Extent = 
               Top = 168
               Left = 547
               Bottom = 268
               Right = 699
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "TitleLookup"
            Begin Extent = 
               Top = 47
               Left = 662
               Bottom = 147
               Right = 814
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "UrlLookup"
            Begin Extent = 
               Top = 196
               Left = 334
               Bottom = 296
               Right = 486
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 2595
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
 ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ActiveWindows'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPane2' , N'SCHEMA',N'dbo', N'VIEW',N'ActiveWindows', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'        Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ActiveWindows'
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'ActiveWindows', NULL,NULL))
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ActiveWindows'
GO
/****** Object:  Default [DF_AggregateIdleIntervals_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateIdleIntervals_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateIdleIntervals]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateIdleIntervals_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateIdleIntervals] ADD  CONSTRAINT [DF_AggregateIdleIntervals_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_AggregateIdleIntervals_UpdateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateIdleIntervals_UpdateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateIdleIntervals]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateIdleIntervals_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateIdleIntervals] ADD  CONSTRAINT [DF_AggregateIdleIntervals_UpdateDate]  DEFAULT (getutcdate()) FOR [UpdateDate]
END


End
GO
/****** Object:  Default [DF_AggregateWorkItemIntervals_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateWorkItemIntervals_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateWorkItemIntervals]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateWorkItemIntervals_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateWorkItemIntervals] ADD  CONSTRAINT [DF_AggregateWorkItemIntervals_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_AggregateWorkItemIntervals_UpdateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateWorkItemIntervals_UpdateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateWorkItemIntervals]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateWorkItemIntervals_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateWorkItemIntervals] ADD  CONSTRAINT [DF_AggregateWorkItemIntervals_UpdateDate]  DEFAULT (getutcdate()) FOR [UpdateDate]
END


End
GO
/****** Object:  Default [DF_AggregateWorkItems_UpdateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateWorkItems_UpdateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateWorkItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateWorkItems_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateWorkItems] ADD  CONSTRAINT [DF_AggregateWorkItems_UpdateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_AggregateWorkItems_UpdateDate_1] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_AggregateWorkItems_UpdateDate_1]') AND parent_object_id = OBJECT_ID(N'[dbo].[AggregateWorkItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AggregateWorkItems_UpdateDate_1]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[AggregateWorkItems] ADD  CONSTRAINT [DF_AggregateWorkItems_UpdateDate_1]  DEFAULT (getutcdate()) FOR [UpdateDate]
END


End
GO
/****** Object:  Default [DF_CalendarExceptions_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_CalendarExceptions_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[CalendarExceptions]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_CalendarExceptions_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[CalendarExceptions] ADD  CONSTRAINT [DF_CalendarExceptions_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_Calendar_CreateData] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_Calendar_CreateData]') AND parent_object_id = OBJECT_ID(N'[dbo].[Calendars]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Calendar_CreateData]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Calendars] ADD  CONSTRAINT [DF_Calendar_CreateData]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerAddresses_FirstReceiveDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerAddresses_FirstReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerAddresses]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerAddresses_FirstReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerAddresses] ADD  CONSTRAINT [DF_ClientComputerAddresses_FirstReceiveDate]  DEFAULT (getutcdate()) FOR [FirstReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerAddresses_LastReceiveDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerAddresses_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerAddresses]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerAddresses_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerAddresses] ADD  CONSTRAINT [DF_ClientComputerAddresses_LastReceiveDate]  DEFAULT (getutcdate()) FOR [LastReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerErrors_FirstReceiveDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerErrors_FirstReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerErrors_FirstReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerErrors] ADD  CONSTRAINT [DF_ClientComputerErrors_FirstReceiveDate]  DEFAULT (getutcdate()) FOR [FirstReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerErrors_LastReceiveDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerErrors_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerErrors]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerErrors_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerErrors] ADD  CONSTRAINT [DF_ClientComputerErrors_LastReceiveDate]  DEFAULT (getutcdate()) FOR [LastReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerVersions_ReceiveDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerVersions_ReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerVersions]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerVersions_ReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerVersions] ADD  CONSTRAINT [DF_ClientComputerVersions_ReceiveDate]  DEFAULT (getutcdate()) FOR [FirstReceiveDate]
END


End
GO
/****** Object:  Default [DF_ClientComputerVersions_LastReceiveDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientComputerVersions_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientComputerVersions]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientComputerVersions_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientComputerVersions] ADD  CONSTRAINT [DF_ClientComputerVersions_LastReceiveDate]  DEFAULT (getutcdate()) FOR [LastReceiveDate]
END


End
GO
/****** Object:  Default [DF__ClientNotifications_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientNotifications_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientNotifications_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientNotifications] ADD  CONSTRAINT [DF__ClientNotifications_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_ClientSettings_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ClientSettings_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ClientSettings_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] ADD  CONSTRAINT [DF_ClientSettings_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF__ClientSet__MenuV__2DE6D218] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientSet__MenuV__2DE6D218]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientSet__MenuV__2DE6D218]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] ADD  CONSTRAINT [DF__ClientSet__MenuV__2DE6D218]  DEFAULT ((0)) FOR [MenuVersion]
END


End
GO
/****** Object:  Default [DF__ClientSet__WorkD__2EDAF651] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientSet__WorkD__2EDAF651]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientSet__WorkD__2EDAF651]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] ADD  CONSTRAINT [DF__ClientSet__WorkD__2EDAF651]  DEFAULT ((0)) FOR [WorkDetectorRulesVersion]
END


End
GO
/****** Object:  Default [DF__ClientSet__Censo__2FCF1A8A] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientSet__Censo__2FCF1A8A]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientSet__Censo__2FCF1A8A]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] ADD  CONSTRAINT [DF__ClientSet__Censo__2FCF1A8A]  DEFAULT ((0)) FOR [CensorRulesVersion]
END


End
GO
/****** Object:  Default [DF__ClientSet__Clien__30C33EC3] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__ClientSet__Clien__30C33EC3]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientSettings]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__ClientSet__Clien__30C33EC3]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ClientSettings] ADD  CONSTRAINT [DF__ClientSet__Clien__30C33EC3]  DEFAULT ((0)) FOR [ClientSettingsVersion]
END


End
GO
/****** Object:  Default [DF_DeadLetterItems_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_DeadLetterItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[DeadLetterItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_DeadLetterItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[DeadLetterItems] ADD  CONSTRAINT [DF_DeadLetterItems_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrCustomRules_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrCustomRules_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrCustomRules]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrCustomRules_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrCustomRules] ADD  CONSTRAINT [DF_IvrCustomRules_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrLocations_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrLocations_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrLocations]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrLocations_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrLocations] ADD  CONSTRAINT [DF_IvrLocations_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrRules_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrRules_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrRules]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrRules_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrRules] ADD  CONSTRAINT [DF_IvrRules_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrRules_InstantNotificationEmail] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrRules_InstantNotificationEmail]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrRules]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrRules_InstantNotificationEmail]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrRules] ADD  CONSTRAINT [DF_IvrRules_InstantNotificationEmail]  DEFAULT ((0)) FOR [InstantNotificationEmail]
END


End
GO
/****** Object:  Default [DF_IvrUserWorks_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrUserWorks_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrUserWorks]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrUserWorks_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrUserWorks] ADD  CONSTRAINT [DF_IvrUserWorks_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrWorkItems_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrWorkItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrWorkItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrWorkItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrWorkItems] ADD  CONSTRAINT [DF_IvrWorkItems_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_IvrWorkItems_InstantNotificationEmail] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_IvrWorkItems_InstantNotificationEmail]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrWorkItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_IvrWorkItems_InstantNotificationEmail]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[IvrWorkItems] ADD  CONSTRAINT [DF_IvrWorkItems_InstantNotificationEmail]  DEFAULT ((0)) FOR [InstantNotificationEmail]
END


End
GO
/****** Object:  Default [DF_ManualWorkItems_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ManualWorkItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ManualWorkItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ManualWorkItems] ADD  CONSTRAINT [DF_ManualWorkItems_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF__NotificationForms_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF__NotificationForms_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[NotificationForms]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__NotificationForms_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[NotificationForms] ADD  CONSTRAINT [DF__NotificationForms_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_ParallelWorkItems_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_ParallelWorkItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[ParallelWorkItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ParallelWorkItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ParallelWorkItems] ADD  CONSTRAINT [DF_ParallelWorkItems_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_Storages_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_Storages_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[Storages]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Storages_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Storages] ADD  CONSTRAINT [DF_Storages_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_UsageStats_IsAcked] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_IsAcked]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_IsAcked]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] ADD  CONSTRAINT [DF_UsageStats_IsAcked]  DEFAULT ((0)) FOR [IsAcked]
END


End
GO
/****** Object:  Default [DF_UsageStats_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] ADD  CONSTRAINT [DF_UsageStats_CreateDate]  DEFAULT (getutcdate()) FOR [CreateDate]
END


End
GO
/****** Object:  Default [DF_UsageStats_UpdateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_UpdateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_UpdateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] ADD  CONSTRAINT [DF_UsageStats_UpdateDate]  DEFAULT (getutcdate()) FOR [UpdateDate]
END


End
GO
/****** Object:  Default [DF_UsageStats_IvrWorkTime] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_IvrWorkTime]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_IvrWorkTime]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] ADD  CONSTRAINT [DF_UsageStats_IvrWorkTime]  DEFAULT ((0)) FOR [IvrWorkTime]
END


End
GO
/****** Object:  Default [DF_UsageStats_MobileWorkTime] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_MobileWorkTime]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_MobileWorkTime]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] ADD  CONSTRAINT [DF_UsageStats_MobileWorkTime]  DEFAULT ((0)) FOR [MobileWorkTime]
END


End
GO
/****** Object:  Default [DF_UsageStats_ManuallyAddedWorkTime] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_UsageStats_ManuallyAddedWorkTime]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_UsageStats_ManuallyAddedWorkTime]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[UsageStats] ADD  CONSTRAINT [DF_UsageStats_ManuallyAddedWorkTime]  DEFAULT ((0)) FOR [ManuallyAddedWorkTime]
END


End
GO
/****** Object:  Default [DF_VoiceRecordings_ReceiveDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_VoiceRecordings_ReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[VoiceRecordings]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_VoiceRecordings_ReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[VoiceRecordings] ADD  CONSTRAINT [DF_VoiceRecordings_ReceiveDate]  DEFAULT (getutcdate()) FOR [FirstReceiveDate]
END


End
GO
/****** Object:  Default [DF_VoiceRecordings_LastReceiveDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_VoiceRecordings_LastReceiveDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[VoiceRecordings]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_VoiceRecordings_LastReceiveDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[VoiceRecordings] ADD  CONSTRAINT [DF_VoiceRecordings_LastReceiveDate]  DEFAULT (getutcdate()) FOR [LastReceiveDate]
END


End
GO
/****** Object:  Default [DF_WorkItems_CreateDate] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_WorkItems_CreateDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[WorkItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_WorkItems_CreateDate]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[WorkItems] ADD  CONSTRAINT [DF_WorkItems_CreateDate]  DEFAULT (getutcdate()) FOR [ReceiveDate]
END


End
GO
/****** Object:  Default [DF_WorkItems_IsRemoteDesktop] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_WorkItems_IsRemoteDesktop]') AND parent_object_id = OBJECT_ID(N'[dbo].[WorkItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_WorkItems_IsRemoteDesktop]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[WorkItems] ADD  CONSTRAINT [DF_WorkItems_IsRemoteDesktop]  DEFAULT ((0)) FOR [IsRemoteDesktop]
END


End
GO
/****** Object:  Default [DF_WorkItems_IsVirtualMachine] ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_WorkItems_IsVirtualMachine]') AND parent_object_id = OBJECT_ID(N'[dbo].[WorkItems]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_WorkItems_IsVirtualMachine]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[WorkItems] ADD  CONSTRAINT [DF_WorkItems_IsVirtualMachine]  DEFAULT ((0)) FOR [IsVirtualMachine]
END


End
GO
/****** Object:  Check [CK_UsageStats_LocalDate_DateOnly] ******/
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_UsageStats_LocalDate_DateOnly]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
ALTER TABLE [dbo].[UsageStats]  WITH CHECK ADD  CONSTRAINT [CK_UsageStats_LocalDate_DateOnly] CHECK  (([LocalDate]=CONVERT([datetime],floor(CONVERT([float],[LocalDate],(0))),(0))))
GO
IF  EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_UsageStats_LocalDate_DateOnly]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
ALTER TABLE [dbo].[UsageStats] CHECK CONSTRAINT [CK_UsageStats_LocalDate_DateOnly]
GO
/****** Object:  Check [CK_UsageStats_StartDate_Less_Than_EndDate] ******/
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_UsageStats_StartDate_Less_Than_EndDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
ALTER TABLE [dbo].[UsageStats]  WITH CHECK ADD  CONSTRAINT [CK_UsageStats_StartDate_Less_Than_EndDate] CHECK  (([StartDate]<[EndDate]))
GO
IF  EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[dbo].[CK_UsageStats_StartDate_Less_Than_EndDate]') AND parent_object_id = OBJECT_ID(N'[dbo].[UsageStats]'))
ALTER TABLE [dbo].[UsageStats] CHECK CONSTRAINT [CK_UsageStats_StartDate_Less_Than_EndDate]
GO
/****** Object:  ForeignKey [FK_CalendarExceptions_Calendar] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CalendarExceptions_Calendar]') AND parent_object_id = OBJECT_ID(N'[dbo].[CalendarExceptions]'))
ALTER TABLE [dbo].[CalendarExceptions]  WITH CHECK ADD  CONSTRAINT [FK_CalendarExceptions_Calendar] FOREIGN KEY([CalendarId])
REFERENCES [dbo].[Calendars] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CalendarExceptions_Calendar]') AND parent_object_id = OBJECT_ID(N'[dbo].[CalendarExceptions]'))
ALTER TABLE [dbo].[CalendarExceptions] CHECK CONSTRAINT [FK_CalendarExceptions_Calendar]
GO
/****** Object:  ForeignKey [FK_Calendars_Calendars] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Calendars_Calendars]') AND parent_object_id = OBJECT_ID(N'[dbo].[Calendars]'))
ALTER TABLE [dbo].[Calendars]  WITH CHECK ADD  CONSTRAINT [FK_Calendars_Calendars] FOREIGN KEY([InheritedFrom])
REFERENCES [dbo].[Calendars] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Calendars_Calendars]') AND parent_object_id = OBJECT_ID(N'[dbo].[Calendars]'))
ALTER TABLE [dbo].[Calendars] CHECK CONSTRAINT [FK_Calendars_Calendars]
GO
/****** Object:  ForeignKey [FK_ClientNotifications_NotificationForms] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClientNotifications_NotificationForms]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
ALTER TABLE [dbo].[ClientNotifications]  WITH CHECK ADD  CONSTRAINT [FK_ClientNotifications_NotificationForms] FOREIGN KEY([FormId])
REFERENCES [dbo].[NotificationForms] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClientNotifications_NotificationForms]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClientNotifications]'))
ALTER TABLE [dbo].[ClientNotifications] CHECK CONSTRAINT [FK_ClientNotifications_NotificationForms]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_DesktopCaptures] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_DesktopCaptures]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows]  WITH CHECK ADD  CONSTRAINT [FK_DesktopActiveWindows_DesktopCaptures] FOREIGN KEY([DesktopCaptureId])
REFERENCES [dbo].[DesktopCaptures] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_DesktopCaptures]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] CHECK CONSTRAINT [FK_DesktopActiveWindows_DesktopCaptures]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_DesktopWindows] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_DesktopWindows]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows]  WITH CHECK ADD  CONSTRAINT [FK_DesktopActiveWindows_DesktopWindows] FOREIGN KEY([Id])
REFERENCES [dbo].[DesktopWindows] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_DesktopWindows]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] CHECK CONSTRAINT [FK_DesktopActiveWindows_DesktopWindows]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_ProcessNameLookup] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_ProcessNameLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows]  WITH CHECK ADD  CONSTRAINT [FK_DesktopActiveWindows_ProcessNameLookup] FOREIGN KEY([ProcessNameId])
REFERENCES [dbo].[ProcessNameLookup] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_ProcessNameLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] CHECK CONSTRAINT [FK_DesktopActiveWindows_ProcessNameLookup]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_TitleLookup] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_TitleLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows]  WITH CHECK ADD  CONSTRAINT [FK_DesktopActiveWindows_TitleLookup] FOREIGN KEY([TitleId])
REFERENCES [dbo].[TitleLookup] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_TitleLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] CHECK CONSTRAINT [FK_DesktopActiveWindows_TitleLookup]
GO
/****** Object:  ForeignKey [FK_DesktopActiveWindows_UrlLookup] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_UrlLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows]  WITH CHECK ADD  CONSTRAINT [FK_DesktopActiveWindows_UrlLookup] FOREIGN KEY([UrlId])
REFERENCES [dbo].[UrlLookup] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopActiveWindows_UrlLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopActiveWindows]'))
ALTER TABLE [dbo].[DesktopActiveWindows] CHECK CONSTRAINT [FK_DesktopActiveWindows_UrlLookup]
GO
/****** Object:  ForeignKey [FK_DesktopCaptures_WorkItems] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopCaptures_WorkItems]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopCaptures]'))
ALTER TABLE [dbo].[DesktopCaptures]  WITH CHECK ADD  CONSTRAINT [FK_DesktopCaptures_WorkItems] FOREIGN KEY([WorkItemId])
REFERENCES [dbo].[WorkItems] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopCaptures_WorkItems]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopCaptures]'))
ALTER TABLE [dbo].[DesktopCaptures] CHECK CONSTRAINT [FK_DesktopCaptures_WorkItems]
GO
/****** Object:  ForeignKey [FK_DesktopWindows_DesktopCaptures] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_DesktopCaptures]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows]  WITH CHECK ADD  CONSTRAINT [FK_DesktopWindows_DesktopCaptures] FOREIGN KEY([DesktopCaptureId])
REFERENCES [dbo].[DesktopCaptures] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_DesktopCaptures]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows] CHECK CONSTRAINT [FK_DesktopWindows_DesktopCaptures]
GO
/****** Object:  ForeignKey [FK_DesktopWindows_ProcessNameLookup] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_ProcessNameLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows]  WITH CHECK ADD  CONSTRAINT [FK_DesktopWindows_ProcessNameLookup] FOREIGN KEY([ProcessNameId])
REFERENCES [dbo].[ProcessNameLookup] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_ProcessNameLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows] CHECK CONSTRAINT [FK_DesktopWindows_ProcessNameLookup]
GO
/****** Object:  ForeignKey [FK_DesktopWindows_TitleLookup] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_TitleLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows]  WITH CHECK ADD  CONSTRAINT [FK_DesktopWindows_TitleLookup] FOREIGN KEY([TitleId])
REFERENCES [dbo].[TitleLookup] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_TitleLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows] CHECK CONSTRAINT [FK_DesktopWindows_TitleLookup]
GO
/****** Object:  ForeignKey [FK_DesktopWindows_UrlLookup] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_UrlLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows]  WITH CHECK ADD  CONSTRAINT [FK_DesktopWindows_UrlLookup] FOREIGN KEY([UrlId])
REFERENCES [dbo].[UrlLookup] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DesktopWindows_UrlLookup]') AND parent_object_id = OBJECT_ID(N'[dbo].[DesktopWindows]'))
ALTER TABLE [dbo].[DesktopWindows] CHECK CONSTRAINT [FK_DesktopWindows_UrlLookup]
GO
/****** Object:  ForeignKey [FK_IvrLocations_IvrWorkItems] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IvrLocations_IvrWorkItems]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrLocations]'))
ALTER TABLE [dbo].[IvrLocations]  WITH CHECK ADD  CONSTRAINT [FK_IvrLocations_IvrWorkItems] FOREIGN KEY([IvrWorkItemId])
REFERENCES [dbo].[IvrWorkItems] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IvrLocations_IvrWorkItems]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrLocations]'))
ALTER TABLE [dbo].[IvrLocations] CHECK CONSTRAINT [FK_IvrLocations_IvrWorkItems]
GO
/****** Object:  ForeignKey [FK_IvrRules_IvrCustomRules] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IvrRules_IvrCustomRules]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrRules]'))
ALTER TABLE [dbo].[IvrRules]  WITH CHECK ADD  CONSTRAINT [FK_IvrRules_IvrCustomRules] FOREIGN KEY([CustomRuleId])
REFERENCES [dbo].[IvrCustomRules] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IvrRules_IvrCustomRules]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrRules]'))
ALTER TABLE [dbo].[IvrRules] CHECK CONSTRAINT [FK_IvrRules_IvrCustomRules]
GO
/****** Object:  ForeignKey [FK_IvrUserWorks_IvrRules] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IvrUserWorks_IvrRules]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrUserWorks]'))
ALTER TABLE [dbo].[IvrUserWorks]  WITH CHECK ADD  CONSTRAINT [FK_IvrUserWorks_IvrRules] FOREIGN KEY([RuleId])
REFERENCES [dbo].[IvrRules] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_IvrUserWorks_IvrRules]') AND parent_object_id = OBJECT_ID(N'[dbo].[IvrUserWorks]'))
ALTER TABLE [dbo].[IvrUserWorks] CHECK CONSTRAINT [FK_IvrUserWorks_IvrRules]
GO
/****** Object:  ForeignKey [FK_ManualWorkItems_ManualWorkItemSource] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ManualWorkItems_ManualWorkItemSource]') AND parent_object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]'))
ALTER TABLE [dbo].[ManualWorkItems]  WITH CHECK ADD  CONSTRAINT [FK_ManualWorkItems_ManualWorkItemSource] FOREIGN KEY([SourceId])
REFERENCES [dbo].[ManualWorkItemSource] ([SourceId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ManualWorkItems_ManualWorkItemSource]') AND parent_object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]'))
ALTER TABLE [dbo].[ManualWorkItems] CHECK CONSTRAINT [FK_ManualWorkItems_ManualWorkItemSource]
GO
/****** Object:  ForeignKey [FK_ManualWorkItems_ManualWorkItemTypes] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ManualWorkItems_ManualWorkItemTypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]'))
ALTER TABLE [dbo].[ManualWorkItems]  WITH CHECK ADD  CONSTRAINT [FK_ManualWorkItems_ManualWorkItemTypes] FOREIGN KEY([ManualWorkItemTypeId])
REFERENCES [dbo].[ManualWorkItemTypes] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ManualWorkItems_ManualWorkItemTypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[ManualWorkItems]'))
ALTER TABLE [dbo].[ManualWorkItems] CHECK CONSTRAINT [FK_ManualWorkItems_ManualWorkItemTypes]
GO
/****** Object:  ForeignKey [FK_ParallelWorkItems_ParallelWorkItemTypes] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ParallelWorkItems_ParallelWorkItemTypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[ParallelWorkItems]'))
ALTER TABLE [dbo].[ParallelWorkItems]  WITH CHECK ADD  CONSTRAINT [FK_ParallelWorkItems_ParallelWorkItemTypes] FOREIGN KEY([ParallelWorkItemTypeId])
REFERENCES [dbo].[ParallelWorkItemTypes] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ParallelWorkItems_ParallelWorkItemTypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[ParallelWorkItems]'))
ALTER TABLE [dbo].[ParallelWorkItems] CHECK CONSTRAINT [FK_ParallelWorkItems_ParallelWorkItemTypes]
GO
/****** Object:  ForeignKey [FK_Screens_DesktopCaptures] ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Screens_DesktopCaptures]') AND parent_object_id = OBJECT_ID(N'[dbo].[Screens]'))
ALTER TABLE [dbo].[Screens]  WITH CHECK ADD  CONSTRAINT [FK_Screens_DesktopCaptures] FOREIGN KEY([DesktopCaptureId])
REFERENCES [dbo].[DesktopCaptures] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Screens_DesktopCaptures]') AND parent_object_id = OBJECT_ID(N'[dbo].[Screens]'))
ALTER TABLE [dbo].[Screens] CHECK CONSTRAINT [FK_Screens_DesktopCaptures]
GO
