/****** Object:  Table [dbo].[ClientComputerInfo]    Script Date: 05/22/2014 20:28:05 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerInfo]') AND type in (N'U'))
DROP TABLE [dbo].[ClientComputerInfo]
GO
/****** Object:  Table [dbo].[ClientComputerInfo]    Script Date: 05/22/2014 20:28:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClientComputerInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ClientComputerInfo](
	[UserId] [int] NOT NULL,
	[ComputerId] [int] NOT NULL,
	[OSMajor] [int] NOT NULL,
	[OSMinor] [int] NOT NULL,
	[OSBuild] [int] NOT NULL,
	[OSRevision] [int] NOT NULL,
	[IsNet4Available] [bit] NOT NULL,
	[IsNet45Available] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ClientComputerInfo] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[ComputerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
