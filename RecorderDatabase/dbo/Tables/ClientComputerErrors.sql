CREATE TABLE [dbo].[ClientComputerErrors] (
    [Id]               INT              IDENTITY (1, 1) NOT NULL,
    [ClientId]         UNIQUEIDENTIFIER NOT NULL,
    [UserId]           INT              NOT NULL,
    [ComputerId]       INT              NOT NULL,
    [Major]            INT              NOT NULL,
    [Minor]            INT              NOT NULL,
    [Build]            INT              NOT NULL,
    [Revision]         INT              NOT NULL,
    [Description]      NVARCHAR (4000)  NULL,
    [HasAttachment]    BIT              NOT NULL,
    [FirstReceiveDate] DATETIME         CONSTRAINT [DF_ClientComputerErrors_FirstReceiveDate] DEFAULT (getutcdate()) NOT NULL,
    [LastReceiveDate]  DATETIME         CONSTRAINT [DF_ClientComputerErrors_LastReceiveDate] DEFAULT (getutcdate()) NOT NULL,
    [Offset]           INT              NOT NULL,
    [IsCompleted]      BIT              NOT NULL,
    [IsCancelled]      BIT              NOT NULL,
    [EnabledFeatures]  NVARCHAR(2000)   NULL, 
    CONSTRAINT [PK_ClientComputerErrors] PRIMARY KEY CLUSTERED ([Id] ASC)
);






















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientComputerErrors_ClientId]
    ON [dbo].[ClientComputerErrors]([ClientId] ASC);

