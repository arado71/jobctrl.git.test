/****** Object:  Table [dbo].[ActiveDevices]    Script Date: 05/28/2013 17:59:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ActiveDevices](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[DeviceId] [bigint] NULL,
	[FirstSeen] [datetime] NOT NULL,
	[LastSeen] [datetime] NOT NULL,
 CONSTRAINT [PK_ActiveDevices] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ClientComputerKicks]
  ALTER COLUMN [ComputerId] [bigint]
GO

ALTER TABLE [dbo].[ClientSettings]
  ADD [CoincidentalClientsEnabled] [bit] NULL
GO

/****** Object:  StoredProcedure [dbo].[ClientComputerKickSend]    Script Date: 05/28/2013 21:29:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author: Zoltan Torok
-- =============================================
ALTER PROCEDURE [dbo].[ClientComputerKickSend]
	(
	@id int,
	@userId int,
	@deviceId bigint,
	@sendDate datetime
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @id IS NULL OR @userId IS NULL OR @deviceId IS NULL OR @sendDate IS NULL
	BEGIN
		RAISERROR('@userId, @deviceId and @sendDate cannot be NULL', 16, 1)
		RETURN
	END

UPDATE [dbo].[ClientComputerKicks]
   SET [SendDate] = @sendDate
 WHERE [Id] = @id
   AND [UserId] = @userId
   AND [ComputerId] = @deviceId
   AND [SendDate] IS NULL

	RETURN @@ROWCOUNT

GO

/****** Object:  StoredProcedure [dbo].[ClientComputerKickConfirm]    Script Date: 05/28/2013 21:30:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author: Zoltan Torok
-- =============================================
ALTER PROCEDURE [dbo].[ClientComputerKickConfirm]
	(
	@id int,
	@userId int,
	@deviceId bigint,
	@confirmDate datetime,
	@result int
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	IF @id IS NULL OR @userId IS NULL OR @deviceId IS NULL OR @confirmDate IS NULL OR @result IS NULL
	BEGIN
		RAISERROR('@id, @userId, @deviceId, @confirmDate and @result cannot be NULL', 16, 1)
		RETURN
	END

UPDATE [dbo].[ClientComputerKicks]
   SET [ConfirmDate] = @confirmDate
      ,[Result] = @result
 WHERE [Id] = @id
   AND [UserId] = @userId
   AND [ComputerId] = @deviceId
   AND [ConfirmDate] IS NULL
   AND [Result] IS NULL

	RETURN @@ROWCOUNT

GO

