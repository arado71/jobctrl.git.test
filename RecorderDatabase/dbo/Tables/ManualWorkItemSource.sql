CREATE TABLE [dbo].[ManualWorkItemSource] (
    [SourceId] TINYINT       NOT NULL,
    [Name]     NVARCHAR (50) NULL,
    CONSTRAINT [PK_ManualWorkItemSource] PRIMARY KEY CLUSTERED ([SourceId] ASC)
);

