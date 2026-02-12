CREATE TABLE [dbo].[CollectedItems] (
    [UserId]     INT      NOT NULL,
    [CreateDate] DATETIME NOT NULL,
    [ComputerId] INT      NOT NULL,
    [KeyId]      INT      NOT NULL,
    [ValueId]    INT      NULL,
    CONSTRAINT [FK_CollectedItems_CollectedKeyLookup] FOREIGN KEY ([KeyId]) REFERENCES [dbo].[CollectedKeyLookup] ([Id]),
    CONSTRAINT [FK_CollectedItems_CollectedValueLookup] FOREIGN KEY ([ValueId]) REFERENCES [dbo].[CollectedValueLookup] ([Id])
);

GO

CREATE CLUSTERED INDEX [IX_CollectedItems_UserId_CreateDate] ON [dbo].[CollectedItems]
(
	[UserId] ASC,
	[CreateDate] ASC
) ON [PRIMARY]
GO

