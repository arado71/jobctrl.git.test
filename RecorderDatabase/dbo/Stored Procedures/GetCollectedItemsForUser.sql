CREATE PROCEDURE [dbo].[GetCollectedItemsForUser]
	@userId int,
	@startDate datetime,
	@endDate datetime
AS
	select c.userid, c.createdate, k.[Key], v.Value, c.ComputerId  from CollectedItems c
		join CollectedKeyLookup k on c.KeyId = k.Id
		join CollectedValueLookup v on c.ValueId = v.Id
	where c.userid = @userId 
		and createdate > @startDate and createdate < @endDate
	order by createdate;
RETURN 0
