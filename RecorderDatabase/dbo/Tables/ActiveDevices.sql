CREATE TABLE [dbo].[ActiveDevices] (
    [Id]        INT      IDENTITY (1, 1) NOT NULL,
    [UserId]    INT      NOT NULL,
    [DeviceId]  BIGINT   NULL,
    [FirstSeen] DATETIME NOT NULL,
    [LastSeen]  DATETIME NOT NULL,
    CONSTRAINT [PK_ActiveDevices] PRIMARY KEY CLUSTERED ([Id] ASC)
);

