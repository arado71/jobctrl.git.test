CREATE TABLE [dbo].[ParallelWorkItemTypes] (
    [Id]          SMALLINT        NOT NULL,
    [Name]        NVARCHAR (1000) NOT NULL,
    [Description] NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_ParallelWorkItemTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

