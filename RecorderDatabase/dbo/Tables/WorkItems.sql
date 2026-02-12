CREATE TABLE [dbo].[WorkItems] (
    [Id]               BIGINT           IDENTITY (1, 1) NOT NULL,
    [WorkId]           INT              NOT NULL,
    [PhaseId]          UNIQUEIDENTIFIER NOT NULL,
    [StartDate]        DATETIME         NOT NULL,
    [EndDate]          DATETIME         NOT NULL,
    [ReceiveDate]      DATETIME         CONSTRAINT [DF_WorkItems_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [UserId]           INT              NOT NULL,
    [GroupId]          INT              NOT NULL,
    [CompanyId]        INT              NOT NULL,
    [ComputerId]       INT              NOT NULL,
    [MouseActivity]    INT              NOT NULL,
    [KeyboardActivity] INT              NOT NULL,
    [IsRemoteDesktop]  BIT              CONSTRAINT [DF_WorkItems_IsRemoteDesktop] DEFAULT ((0)) NOT NULL,
    [IsVirtualMachine] BIT              CONSTRAINT [DF_WorkItems_IsVirtualMachine] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_WorkItems] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [IX_WorkItems_Unique] UNIQUE NONCLUSTERED ([StartDate] ASC, [PhaseId] ASC)
);


GO
CREATE CLUSTERED INDEX [IX_WorkItems_StartDateClust]
    ON [dbo].[WorkItems]([StartDate] ASC);

