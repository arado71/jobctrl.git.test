SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ManualWorkItemSource](
	[SourceId] [tinyint] NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_ManualWorkItemSource] PRIMARY KEY CLUSTERED 
(
	[SourceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE dbo.ManualWorkItems
	ADD SourceId tinyint NULL
GO

ALTER TABLE dbo.ManualWorkItems
	ADD CONSTRAINT FK_ManualWorkItems_ManualWorkItemSource FOREIGN KEY ( SourceId ) REFERENCES ManualWorkItemSource (SourceId)
GO
