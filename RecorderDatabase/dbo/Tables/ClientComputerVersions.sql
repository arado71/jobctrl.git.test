CREATE TABLE [dbo].[ClientComputerVersions] (
    [Id]               INT      IDENTITY (1, 1) NOT NULL,
    [UserId]           INT      NOT NULL,
    [ComputerId]       INT      NOT NULL,
    [Major]            INT      NOT NULL,
    [Minor]            INT      NOT NULL,
    [Build]            INT      NOT NULL,
    [Revision]         INT      NOT NULL,
    [IsCurrent]        BIT      NOT NULL,
    [FirstReceiveDate] DATETIME CONSTRAINT [DF_ClientComputerVersions_ReceiveDate] DEFAULT (getutcdate()) NOT NULL,
    [LastReceiveDate]  DATETIME CONSTRAINT [DF_ClientComputerVersions_LastReceiveDate] DEFAULT (getutcdate()) NOT NULL,
    [Application] NVARCHAR(50) NULL, 
    CONSTRAINT [PK_ClientComputerVersions] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ClientComputerVersions_UserId_ComputerId_Application_IsCurrent]
    ON [dbo].[ClientComputerVersions]([UserId] ASC, [ComputerId] ASC, [Application] ASC, [IsCurrent] ASC);

