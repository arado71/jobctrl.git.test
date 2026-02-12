USE [JobControl]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Client_GetWorksOrProjects]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Client_GetWorksOrProjects]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Client_GetWorksOrProjects]
	@ids IntIdTableType READONLY
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON

	;WITH parents ([Id], [ParentId], [Type], [Name], [RecursionLevel])
	AS (
		SELECT t.[Id], t.[ParentId], t.[Type], t.[Name], 1
		FROM [dbo].[Tasks] t
		JOIN @ids w ON w.[Id] = t.[Id]

		UNION ALL
		
		SELECT t.[Id], t.[ParentId], t.[Type], t.[Name], p.[RecursionLevel] + 1
		FROM [dbo].[Tasks] t
		JOIN parents p ON p.[ParentId] = t.[Id]
	)
	SELECT DISTINCT [Id], CASE WHEN [Type] < 2 THEN CAST(1 as bit) ELSE CAST(0 as bit) END AS [IsProject], [ParentId], [Name]
	FROM parents

RETURN 0
GO