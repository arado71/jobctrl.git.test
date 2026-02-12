CREATE TABLE [dbo].[ClientComputerInfo] (
    [UserId]           INT      NOT NULL,
    [ComputerId]       INT      NOT NULL,
    [OSMajor]          INT      NOT NULL,
    [OSMinor]          INT      NOT NULL,
    [OSBuild]          INT      NOT NULL,
    [OSRevision]       INT      NOT NULL,
    [IsNet4Available]  BIT      NOT NULL,
    [IsNet45Available] BIT      NOT NULL,
    [CreateDate]       DATETIME NOT NULL,
    [HighestNetVersionAvailable]    INT     NULL,
    [MachineName]     NVARCHAR(100) NULL,
    [LocalUserName]   NVARCHAR(100) NULL,
    CONSTRAINT [PK_ClientComputerInfo] PRIMARY KEY CLUSTERED ([UserId] ASC, [ComputerId] ASC)
);









