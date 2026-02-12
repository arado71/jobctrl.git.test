CREATE TABLE [dbo].[EmailStats] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [UserId]       INT      NOT NULL,
    [LastSendDate] DATETIME NULL,
    CONSTRAINT [PK_EmailStats] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [IX_EmailStats_Unique] UNIQUE NONCLUSTERED ([UserId] ASC)
);

