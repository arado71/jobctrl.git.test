CREATE TABLE [dbo].[ScreenShots](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
 	[ComputerId] INT NOT NULL, 
	[CreateDate] [datetime] NOT NULL,
	[ReceiveDate] [datetime] NOT NULL,
	[X] [smallint] NOT NULL,
	[Y] [smallint] NOT NULL,
	[Width] [smallint] NOT NULL,
	[Height] [smallint] NOT NULL,
	[ScreenNumber] [tinyint] NOT NULL,
	[Extension] [varchar](10) NULL,
    CONSTRAINT [PK_ScreenShots] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
GO

CREATE NONCLUSTERED INDEX [IX_Screenshots_UserId_CreateDate]
    ON [dbo].[ScreenShots]([UserId] ASC, [CreateDate] ASC);
