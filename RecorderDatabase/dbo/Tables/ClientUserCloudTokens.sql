CREATE TABLE [dbo].[ClientUserCloudTokens]
(
	[Id] INT Identity(1,1) NOT NULL, 
    [UserId] INT NOT NULL, 
    [AuthToken] NVARCHAR(4000) NOT NULL, 
    [SyncToken] NVARCHAR(4000) NULL, 
	[ServiceType] SMALLINT NULL, 
    [LastUpdateTime] DATETIME NULL, 
    [LastCheckTime] DATETIME NULL,
    CONSTRAINT [PK_ClientUserCloudTokens] PRIMARY KEY CLUSTERED ([Id] ASC)
)

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientUserCloudTokens_UserId] ON [dbo].[ClientUserCloudTokens] ([UserId] ASC)
