CREATE TABLE [dbo].[Storages] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [FirstId]     BIGINT         NOT NULL,
    [Algorithm]   NVARCHAR (50)  NOT NULL,
    [Data]        NVARCHAR (MAX) NULL,
    [Description] NVARCHAR (MAX) NULL,
    [CreateDate]  DATETIME       CONSTRAINT [DF_Storages_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Storages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [IX_Storages_FirstId_Unique] UNIQUE NONCLUSTERED ([FirstId] ASC)
);

