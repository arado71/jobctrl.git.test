CREATE TABLE [dbo].[NotificationForms] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (200)  NULL,
    [CompanyId]   INT             NOT NULL,
    [Data]        NVARCHAR (MAX)  NULL,
    [Description] NVARCHAR (2000) NULL,
    [WorkId]      INT             NULL,
    [CreatedBy]   INT             NULL,
    [CreateDate]  DATETIME        CONSTRAINT [DF__NotificationForms_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [DeleteDate]  DATETIME        NULL,
    CONSTRAINT [PK__NotificationForms_Id_Clust] PRIMARY KEY CLUSTERED ([Id] ASC)
);

