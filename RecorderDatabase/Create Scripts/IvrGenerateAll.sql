/****** Object:  Table [dbo].[Jc_IOs]    Script Date: 05/06/2010 15:48:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Jc_IOs]') AND type in (N'U'))
DROP TABLE [dbo].[Jc_IOs]
GO
/****** Object:  Default [DF_Jc_IOs_bProcessed]    Script Date: 05/06/2010 15:48:30 ******/
IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_Jc_IOs_bProcessed]') AND parent_object_id = OBJECT_ID(N'[dbo].[Jc_IOs]'))
Begin
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Jc_IOs_bProcessed]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Jc_IOs] DROP CONSTRAINT [DF_Jc_IOs_bProcessed]
END


End
GO
/****** Object:  Table [dbo].[Jc_IOs]    Script Date: 05/06/2010 15:48:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Jc_IOs]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Jc_IOs](
	[nId] [int] IDENTITY(1,1) NOT NULL,
	[cTrunkId] [varchar](50) COLLATE SQL_Hungarian_CP1250_CI_AS NOT NULL,
	[cCtn] [varchar](50) COLLATE SQL_Hungarian_CP1250_CI_AS NOT NULL,
	[dCrDate] [datetime] NOT NULL,
	[bStatus] [tinyint] NOT NULL,
	[bProcessed] [tinyint] NOT NULL,
 CONSTRAINT [PK_Jc_IOs] PRIMARY KEY CLUSTERED 
(
	[nId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Default [DF_Jc_IOs_bProcessed]    Script Date: 05/06/2010 15:48:30 ******/
IF Not EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_Jc_IOs_bProcessed]') AND parent_object_id = OBJECT_ID(N'[dbo].[Jc_IOs]'))
Begin
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Jc_IOs_bProcessed]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Jc_IOs] ADD  CONSTRAINT [DF_Jc_IOs_bProcessed]  DEFAULT ((0)) FOR [bProcessed]
END


End
GO
