CREATE TABLE [dbo].[Snippets]
(
    [Guid] UNIQUEIDENTIFIER NOT NULL, 
    [ImageData] VARBINARY(MAX) NOT NULL, 
    [Content] NVARCHAR(500) NULL, 
    [UserId] INT NOT NULL, 
    [CreatedAt] DATETIME NOT NULL, 
    [ProcessedAt] DATETIME NULL, 
    [RuleId] INT NOT NULL,
	[IsBadData] bit NOT NULL CONSTRAINT DF_Snippets_IsBadData DEFAULT 0,
	ProcessName nvarchar(100) NOT NULL, 
    [Quality] INT NOT NULL CONSTRAINT DF_Snippets_Quality DEFAULT 5, 
    CONSTRAINT [PK_Snippets] PRIMARY KEY NONCLUSTERED ([Guid])
)

GO


ALTER TABLE Snippets
 ADD CONSTRAINT DF_Snippets_CreatedAt 
 DEFAULT GETUTCDATE() for [CreatedAt]


GO

CREATE CLUSTERED INDEX [IX_Snippets_ProcessedAt_RuleId] ON [dbo].[Snippets] ([ProcessedAt], [RuleId])
