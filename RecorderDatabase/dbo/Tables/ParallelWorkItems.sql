CREATE TABLE [dbo].[ParallelWorkItems] (
    [Id]                     INT      IDENTITY (1, 1) NOT NULL,
    [ParallelWorkItemTypeId] SMALLINT NOT NULL,
    [WorkId]                 INT      NOT NULL,
    [StartDate]              DATETIME NOT NULL,
    [EndDate]                DATETIME NOT NULL,
    [UserId]                 INT      NOT NULL,
    [CreateDate]             DATETIME CONSTRAINT [DF_ParallelWorkItems_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_ParallelWorkItems] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ParallelWorkItems_ParallelWorkItemTypes] FOREIGN KEY ([ParallelWorkItemTypeId]) REFERENCES [dbo].[ParallelWorkItemTypes] ([Id])
);


GO
CREATE CLUSTERED INDEX [IX_ParallelWorkItems_UserId_StartDate_Clust]
    ON [dbo].[ParallelWorkItems]([UserId] ASC, [StartDate] ASC);

