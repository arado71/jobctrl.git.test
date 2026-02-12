-- =============================================
-- Author: Zoltan Torok
-- =============================================
ALTER PROCEDURE dbo.GetActiveWindowsGrouppedForUser
	(
	@userId int,
	@startDate datetime,
	@endDate datetime
	)
AS
	SET NOCOUNT ON
	
	IF @userId IS NULL OR @startDate IS NULL OR @endDate IS NULL
	BEGIN
		RAISERROR('@userId, @startDate and @endDate cannot be NULL', 16, 1)
		RETURN
	END

	SELECT
		p.ProcessName, t.Title, u.Url, g.[Count]
	FROM
	(
		SELECT
			a.ProcessNameId, a.TitleId, a.UrlId, COUNT(*) AS [Count]
		FROM
			DesktopActiveWindows a
		WHERE
			a.CreateDate >= @startDate 
			AND a.CreateDate < @endDate
			AND a.UserId = @userId
		GROUP BY 
			a.ProcessNameId, a.TitleId, a.UrlId
	)AS g
	JOIN
		ProcessNameLookup p WITH (FORCESEEK) ON p.Id = g.ProcessNameId
	JOIN
		TitleLookup t WITH (FORCESEEK) ON t.Id = g.TitleId
	LEFT JOIN
		UrlLookup u WITH (FORCESEEK)  ON u.Id = g.UrlId

	RETURN
