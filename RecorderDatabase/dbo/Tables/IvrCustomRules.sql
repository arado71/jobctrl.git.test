CREATE TABLE [dbo].[IvrCustomRules] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Method]      NVARCHAR (50)  NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Settings]    NVARCHAR (MAX) NULL,
    [CreateDate]  DATETIME       CONSTRAINT [DF_IvrCustomRules_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_IvrCustomRules] PRIMARY KEY CLUSTERED ([Id] ASC)
);

