CREATE TABLE [dbo].[ClientNotifications] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [UserId]      INT           NOT NULL,
    [FormId]      INT           NOT NULL,
    [CreatedBy]   INT           NULL,
    [CreateDate]  DATETIME      CONSTRAINT [DF__ClientNotifications_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [SendDate]    DATETIME      NULL,
    [ReceiveDate] DATETIME      NULL,
    [ShowDate]    DATETIME      NULL,
    [ConfirmDate] DATETIME      NULL,
    [Result]      NVARCHAR (50) NULL,
    [DeviceId]    BIGINT        NULL,
    CONSTRAINT [PK__ClientNotifications_Id_Clust] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClientNotifications_NotificationForms] FOREIGN KEY ([FormId]) REFERENCES [dbo].[NotificationForms] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_ClientNotifications_UserId_ReceiveDate_Filtered]
    ON [dbo].[ClientNotifications]([UserId] ASC, [ReceiveDate] ASC) WHERE ([ReceiveDate] IS NULL);

