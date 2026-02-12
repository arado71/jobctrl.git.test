CREATE TABLE [dbo].[ClientUserCloudEventDates]
(
	[Id] INT NOT NULL IDENTITY, 
    [UserId] INT NOT NULL, 
    [EventId] VARCHAR(4000) NOT NULL, 
	[EventIdHash] INT NOT NULL, 
    [StartTime] DATETIME NOT NULL,
    CONSTRAINT [PK_ClientUserCloudEventDates] PRIMARY KEY CLUSTERED ([Id] ASC)
)

GO

CREATE NONCLUSTERED INDEX [IX_ClientUserCloudEventDates_UserId_EventIdHash] ON [dbo].[ClientUserCloudEventDates] ([UserId] ASC, [EventIdHash] ASC)
