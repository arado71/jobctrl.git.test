CREATE TABLE [dbo].[VoiceRecordings] (
    [Id]               INT              IDENTITY (1, 1) NOT NULL,
    [ClientId]         UNIQUEIDENTIFIER NOT NULL,
    [UserId]           INT              NOT NULL,
    [WorkId]           INT              NULL,
    [StartDate]        DATETIME         NOT NULL,
    [EndDate]          DATETIME         NULL,
    [Duration]         INT              NOT NULL,
    [Codec]            INT              NOT NULL,
    [Name]             NVARCHAR (200)   NULL,
    [Extension]        VARCHAR (10)     NULL,
    [FirstReceiveDate] DATETIME         CONSTRAINT [DF_VoiceRecordings_ReceiveDate] DEFAULT (getutcdate()) NOT NULL,
    [LastReceiveDate]  DATETIME         CONSTRAINT [DF_VoiceRecordings_LastReceiveDate] DEFAULT (getutcdate()) NOT NULL,
    [Offset]           INT              NOT NULL,
    [DeleteDate]       DATETIME         NULL,
    CONSTRAINT [PK_VoiceRecordings] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE CLUSTERED INDEX [IX_VoiceRecordings_UserId_StartDate_Clust]
    ON [dbo].[VoiceRecordings]([UserId] ASC, [StartDate] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_VoiceRecordings_ClientId]
    ON [dbo].[VoiceRecordings]([ClientId] ASC);

