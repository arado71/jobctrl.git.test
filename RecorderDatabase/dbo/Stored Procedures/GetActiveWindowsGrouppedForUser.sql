CREATE PROCEDURE [dbo].[GetActiveWindowsGrouppedForUser]
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

	IF NULLIF(object_id('tempdb..#Deletions'), 0) IS NOT NULL DROP TABLE #Deletions
	
	SELECT 
		StartDate, EndDate 
	INTO 
		#Deletions
	FROM 
		ManualWorkItems m
	WHERE
		m.UserId = @userId
		AND @startDate < m.EndDate
		AND m.StartDate < @endDate
		AND m.ManualWorkItemTypeId IN (1, 3) -- DeleteInterval, DeleteComputerInterval

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
			AND NOT EXISTS (SELECT 1 FROM #Deletions d WHERE d.StartDate <= a.CreateDate AND a.CreateDate <= d.EndDate)
		GROUP BY 
			a.ProcessNameId, a.TitleId, a.UrlId
	)AS g
	JOIN
		ProcessNameLookup p ON p.Id = g.ProcessNameId
	JOIN
		TitleLookup t ON t.Id = g.TitleId
	LEFT JOIN
		UrlLookup u ON u.Id = g.UrlId

	RETURN
