CREATE TABLE [dbo].[CollectedKeyLookup] (
    [Id]       INT             IDENTITY (1, 1) NOT NULL,
    [HashCode] INT             NOT NULL,
    [Key]      NVARCHAR (4000) NOT NULL,
    CONSTRAINT [PK_CollectedKeyLookup] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_CollectedKeyLookup_HashCode]
    ON [dbo].[CollectedKeyLookup]([HashCode] ASC);

