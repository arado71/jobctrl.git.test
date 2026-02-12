CREATE TABLE [dbo].[UsageStats] (
    [Id]                    INT      IDENTITY (1, 1) NOT NULL,
    [LocalDate]             DATETIME NOT NULL,
    [StartDate]             DATETIME NOT NULL,
    [EndDate]               DATETIME NOT NULL,
    [UserId]                INT      NOT NULL,
    [ComputerWorkTime]      INT      NOT NULL,
    [IsAcked]               BIT      CONSTRAINT [DF_UsageStats_IsAcked] DEFAULT ((0)) NOT NULL,
    [CreateDate]            DATETIME CONSTRAINT [DF_UsageStats_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [UpdateDate]            DATETIME CONSTRAINT [DF_UsageStats_UpdateDate] DEFAULT (getutcdate()) NOT NULL,
    [IvrWorkTime]           INT      CONSTRAINT [DF_UsageStats_IvrWorkTime] DEFAULT ((0)) NOT NULL,
    [MobileWorkTime]        INT      CONSTRAINT [DF_UsageStats_MobileWorkTime] DEFAULT ((0)) NOT NULL,
    [ManuallyAddedWorkTime] INT      CONSTRAINT [DF_UsageStats_ManuallyAddedWorkTime] DEFAULT ((0)) NOT NULL,
	[UsedBeaconClient]      bit      CONSTRAINT [DF_UsageStats_UsedBeaconClient] DEFAULT 0 NOT NULL,
	[UsedVoxCtrl]           bit      CONSTRAINT [DF_UsageStats_UsedVoxCtrl] DEFAULT 0 NOT NULL,
	[UsedMobile]            bit      CONSTRAINT [DF_UsageStats_UsedMobile] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_UsageStats] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_UsageStats_LocalDate_DateOnly] CHECK ([LocalDate]=CONVERT([datetime],floor(CONVERT([float],[LocalDate],(0))),(0))),
    CONSTRAINT [CK_UsageStats_StartDate_Less_Than_EndDate] CHECK ([StartDate]<[EndDate]),
    CONSTRAINT [IX_UsageStats_UserId_LocalDate_Unique] UNIQUE NONCLUSTERED ([UserId] ASC, [LocalDate] ASC)
);

