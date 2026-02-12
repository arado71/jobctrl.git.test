CREATE TABLE [dbo].[DeadLetterItems] (
    [Id]         INT             IDENTITY (1, 1) NOT NULL,
    [WorkId]     INT             NULL,
    [StartDate]  DATETIME        NOT NULL,
    [EndDate]    DATETIME        NOT NULL,
    [UserId]     INT             NOT NULL,
    [CreateDate] DATETIME        CONSTRAINT [DF_DeadLetterItems_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [ItemType]   NVARCHAR (200)  NULL,
    [ErrorText]  NVARCHAR (3000) NULL,
    CONSTRAINT [PK_DeadLetterItems] PRIMARY KEY CLUSTERED ([Id] ASC)
);

