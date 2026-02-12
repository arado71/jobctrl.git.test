CREATE TABLE [dbo].[AggregateIdleIntervals] (
    [Id]         BIGINT           IDENTITY (1, 1) NOT NULL,
    [WorkId]     INT              NOT NULL,
    [StartDate]  DATETIME         NOT NULL,
    [EndDate]    DATETIME         NOT NULL,
    [UserId]     INT              NOT NULL,
    [GroupId]    INT              NOT NULL,
    [CompanyId]  INT              NOT NULL,
    [PhaseId]    UNIQUEIDENTIFIER NOT NULL,
    [ComputerId] INT              NOT NULL,
    [CreateDate] DATETIME         CONSTRAINT [DF_AggregateIdleIntervals_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [UpdateDate] DATETIME         CONSTRAINT [DF_AggregateIdleIntervals_UpdateDate] DEFAULT (getutcdate()) NOT NULL,
    [IsRemoteDesktop] BIT NULL, 
    [IsVirtualMachine] BIT NULL, 
    CONSTRAINT [PK_AggregateIdleIntervals] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE CLUSTERED INDEX [IX_AggregateIdleIntervals_StartDateClust]
    ON [dbo].[AggregateIdleIntervals]([StartDate] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AggregateIdleIntervals_EndDate_StartDate]
    ON [dbo].[AggregateIdleIntervals]([EndDate] ASC, [StartDate] ASC);

