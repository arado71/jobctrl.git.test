CREATE TABLE [dbo].[ClientComputerKicks] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [UserId]         INT             NOT NULL,
    [ComputerId]     BIGINT          NOT NULL,
    [Reason]         NVARCHAR (1000) NULL,
    [CreatedBy]      INT             NOT NULL,
    [CreateDate]     DATETIME        NOT NULL,
    [ExpirationDate] DATETIME        NOT NULL,
    [SendDate]       DATETIME        NULL,
    [ConfirmDate]    DATETIME        NULL,
    [Result]         INT             NULL,
    CONSTRAINT [PK_ClientComputerKicks] PRIMARY KEY CLUSTERED ([Id] ASC)
);

