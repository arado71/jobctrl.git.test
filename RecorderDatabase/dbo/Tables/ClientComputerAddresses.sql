CREATE TABLE [dbo].[ClientComputerAddresses] (
    [Id]               INT          IDENTITY (1, 1) NOT NULL,
    [UserId]           INT          NOT NULL,
    [ComputerId]       INT          NOT NULL,
    [Address]          VARCHAR (39) NULL,
    [IsCurrent]        BIT          NOT NULL,
    [FirstReceiveDate] DATETIME     CONSTRAINT [DF_ClientComputerAddresses_FirstReceiveDate] DEFAULT (getutcdate()) NOT NULL,
    [LastReceiveDate]  DATETIME     CONSTRAINT [DF_ClientComputerAddresses_LastReceiveDate] DEFAULT (getutcdate()) NOT NULL,
	[ClientComputerLocalAddressId] [int] NULL
    CONSTRAINT [PK_ClientComputerAddresses] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ClientComputerAddresses_UserId_ComputerId_IsCurrent]
    ON [dbo].[ClientComputerAddresses]([UserId] ASC, [ComputerId] ASC, [IsCurrent] ASC);

