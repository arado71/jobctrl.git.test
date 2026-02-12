CREATE TABLE [dbo].[CollectedValueLookup] (
    [Id]       INT             IDENTITY (1, 1) NOT NULL,
    [HashCode] INT             NOT NULL,
    [Value]    NVARCHAR (4000) NOT NULL,
    CONSTRAINT [PK_CollectedValueLookup] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_CollectedValueLookup_HashCode]
    ON [dbo].[CollectedValueLookup]([HashCode] ASC);

