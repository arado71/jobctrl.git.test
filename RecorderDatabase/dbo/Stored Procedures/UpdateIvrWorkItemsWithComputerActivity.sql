CREATE PROCEDURE [dbo].[UpdateIvrWorkItemsWithComputerActivity]
AS
	SET NOCOUNT ON

declare @result table (
	IvrWorkItemId int NOT NULL,
	StartDate datetime NOT NULL,
	OldEndDate datetime,
	NewEndDate datetime NOT NULL,
	UserId int NOT NULL,
	WorkId int NOT NULL,
	InstantNotificationEmail bit NOT NULL
	)
	
declare
@c_Id int,
@c_WorkId int,
@c_StartDate datetime,
@c_EndDate datetime,
@c_MaxEndDate datetime,
@c_UserId int,
@c_GroupId int,
@c_CompanyId int,
@c_PhoneNumber varchar(50),
@c_TrunkId varchar(50),
@c_AutoEndOnComputerActivity bit,
@c_InstantNotificationEmail bit

--TODO LogoffMaxEndDate in not handled

DECLARE ivrworkitem_cursor CURSOR FORWARD_ONLY FOR 	
SELECT [Id]
      ,[WorkId]
      ,[StartDate]
      ,[EndDate]
      ,[MaxEndDate]
      ,[UserId]
      ,[GroupId]
      ,[CompanyId]
      ,[PhoneNumber]
      ,[TrunkId]
      ,[AutoEndOnComputerActivity]
      ,[InstantNotificationEmail]
  FROM [dbo].[IvrWorkItems]
  WHERE
		[AutoEndOnComputerActivity] = 1
		--AND ([EndDate] IS NULL OR [StartDate] <> [EndDate]) -- if duration is 0 then no update needed
		AND (DATEDIFF(hour, [StartDate], [MaxEndDate]) < 200) --protect WorkItems from big queries
	
	
OPEN ivrworkitem_cursor

WHILE 1=1
BEGIN
	FETCH NEXT FROM ivrworkitem_cursor INTO 
		@c_Id,
		@c_WorkId,
		@c_StartDate,
		@c_EndDate,
		@c_MaxEndDate,
		@c_UserId,
		@c_GroupId,
		@c_CompanyId,
		@c_PhoneNumber,
		@c_TrunkId,
		@c_AutoEndOnComputerActivity,
		@c_InstantNotificationEmail

	IF @@FETCH_STATUS <> 0
		BREAK
		
	declare @computerStartDate datetime
	SET @computerStartDate = (	
								SELECT
									MIN(StartDate) 
								FROM 
									[dbo].[WorkItems]
								WHERE
									UserId = @c_UserId
									AND StartDate >= DATEADD(minute, 5, @c_StartDate)
									AND StartDate < ISNULL(@c_EndDate, @c_MaxEndDate)
							 )

	IF (@computerStartDate IS NOT NULL)
	BEGIN
		UPDATE [dbo].[IvrWorkItems]
		   SET [EndDate] = @computerStartDate
		 WHERE
			   [Id] = @c_Id
			   AND ((@c_EndDate IS NULL AND [EndDate] IS NULL) OR [EndDate] = @c_EndDate)
			   
		IF @@ROWCOUNT<>0
		BEGIN
			INSERT INTO @result (IvrWorkItemId,	StartDate, OldEndDate, NewEndDate, UserId, WorkId, InstantNotificationEmail) VALUES
			(@c_Id, @c_StartDate, @c_EndDate, @computerStartDate, @c_UserId, @c_WorkId, @c_InstantNotificationEmail)
		END
	END
END
CLOSE ivrworkitem_cursor
DEALLOCATE ivrworkitem_cursor	

SELECT * FROM @result
	
	RETURN
