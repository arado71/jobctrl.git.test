CREATE TABLE [dbo].[IvrLocations] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [IvrWorkItemId]   INT            NOT NULL,
    [CreateDate]      DATETIME       CONSTRAINT [DF_IvrLocations_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [Ctn]             VARCHAR (50)   NULL,
    [State]           VARCHAR (50)   NULL,
    [X]               INT            NULL,
    [Y]               INT            NULL,
    [Radius]          INT            NULL,
    [SubscriberState] INT            NULL,
    [Mcc]             INT            NULL,
    [Mnc]             INT            NULL,
    [Lac]             INT            NULL,
    [Cellid]          INT            NULL,
    [AgeOfLocation]   INT            NULL,
    [Date]            DATETIME       NULL,
    [Cust]            VARCHAR (50)   NULL,
    [Msid]            VARCHAR (50)   NULL,
    [ReplyText]       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_IvrLocations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_IvrLocations_IvrWorkItems] FOREIGN KEY ([IvrWorkItemId]) REFERENCES [dbo].[IvrWorkItems] ([Id])
);

