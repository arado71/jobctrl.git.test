USE [JobControl]
GO
/****** Object:  StoredProcedure [dbo].[Client_GetUserSettings]    Script Date: 2015.04.20. 13:27:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[Client_GetUserSettings]
	@userId int
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	SELECT ~CanModifyWorkTimeWithoutApproval AS [IsModificationApprovalNeeded], CASE WHEN ManualWorkItemEditAgeLimit = 0 THEN 24*60 ELSE ManualWorkItemEditAgeLimit END AS [ModificationAgeLimitInHours]
	FROM UserEffectiveSettings
	WHERE [UserId] = @UserId

RETURN 0
