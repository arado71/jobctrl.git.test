USE [JobControl]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Client_GetMobileActivityInfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Client_GetMobileActivityInfo]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Client_GetMobileActivityInfo]
	@startDate datetime,
	@endDate datetime
AS
BEGIN
	SET NOCOUNT ON;

SELECT [UserId]
      ,[Imei]
      ,[StartDate]
      ,[EndDate]
      ,([Activity] * DATEDIFF(second, [StartDate], [EndDate]) + 29) / 30 AS [Activity]
  FROM [dbo].[MobileClientHelmetActivities]
 WHERE [StartDate] < @endDate
   AND [EndDate] > @startDate

END
GO
