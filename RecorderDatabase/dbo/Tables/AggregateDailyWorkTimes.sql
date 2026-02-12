CREATE TABLE [dbo].[AggregateDailyWorkTimes] (
    [UserId]           INT        NOT NULL,
    [Day]              DATE       NOT NULL,
    [TotalWorkTime]    AS         ((((([ComputerWorkTime]+[IvrWorkTime])+[MobileWorkTime])+[ManualWorkTime])+[HolidayTime])+[SickLeaveTime]),
    [NetWorkTime]      INT        DEFAULT ((0)) NOT NULL,
    [ComputerWorkTime] INT        DEFAULT ((0)) NOT NULL,
    [IvrWorkTime]      INT        DEFAULT ((0)) NOT NULL,
    [MobileWorkTime]   INT        DEFAULT ((0)) NOT NULL,
    [ManualWorkTime]   INT        DEFAULT ((0)) NOT NULL,
    [HolidayTime]      INT        DEFAULT ((0)) NOT NULL,
    [SickLeaveTime]    INT        DEFAULT ((0)) NOT NULL,
    [IsValid]          BIT        DEFAULT ((0)) NOT NULL,
    [Version]          ROWVERSION NOT NULL,
    CONSTRAINT [PK_AggregateDailyWorkTimes] PRIMARY KEY CLUSTERED ([UserId] ASC, [Day] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_AggregateDailyWorkTimes_Day_IsValid_Filtered]
    ON [dbo].[AggregateDailyWorkTimes]([Day] ASC) WHERE ([IsValid]=(0));

