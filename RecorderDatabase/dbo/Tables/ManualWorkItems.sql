CREATE TABLE [dbo].[ManualWorkItems] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [ManualWorkItemTypeId] SMALLINT       NOT NULL,
    [WorkId]               INT            NULL,
    [StartDate]            DATETIME       NOT NULL,
    [EndDate]              DATETIME       NOT NULL,
    [UserId]               INT            NOT NULL,
    [GroupId]              INT            NOT NULL,
    [CompanyId]            INT            NOT NULL,
    [Comment]              NVARCHAR (MAX) NULL,
    [CreateDate]           DATETIME       CONSTRAINT [DF_ManualWorkItems_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [CreatedBy]            INT            NULL,
    [SourceId]             TINYINT        NULL,
    CONSTRAINT [PK_ManualWorkItems] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ManualWorkItems_ManualWorkItemSource] FOREIGN KEY ([SourceId]) REFERENCES [dbo].[ManualWorkItemSource] ([SourceId]),
    CONSTRAINT [FK_ManualWorkItems_ManualWorkItemTypes] FOREIGN KEY ([ManualWorkItemTypeId]) REFERENCES [dbo].[ManualWorkItemTypes] ([Id])
);












GO
CREATE CLUSTERED INDEX [IX_ManualWorkItems_StartDateClust]
    ON [dbo].[ManualWorkItems]([StartDate] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ManualWorkItems_DatesForOR]
    ON [dbo].[ManualWorkItems]([EndDate] ASC, [StartDate] ASC);


GO
CREATE TRIGGER [dbo].[ManualWorkItems_Validation]
ON [dbo].[ManualWorkItems]
FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	--WorkId cannot be NULL when IsWorkIdRequired is true
	IF EXISTS (	
		SELECT 1 FROM inserted i
		JOIN ManualWorkItemTypes t ON t.Id = i.ManualWorkItemTypeId
		WHERE 
			i.WorkId IS NULL
			AND t.IsWorkIdRequired = 1
	)
	BEGIN
		RAISERROR('WorkId cannot be NULL when IsWorkIdRequired is true',16,1)
		ROLLBACK
		RETURN
	END
	
	--EndDate should be greater than or equal to StartDate
	IF EXISTS (	
		SELECT 1 FROM inserted i
		WHERE
			i.EndDate < i.StartDate
	)
	BEGIN
		RAISERROR('EndDate should be greater than or equal to StartDate',16,1)
		ROLLBACK
		RETURN
	END
	
	--WorkId should be NULL when IsWorkIdRequired is false (as we don't handle deletion by WorkId atm.)
	IF EXISTS (	
		SELECT 1 FROM inserted i
		JOIN ManualWorkItemTypes t ON t.Id = i.ManualWorkItemTypeId
		WHERE 
			i.WorkId IS NOT NULL
			AND t.IsWorkIdRequired = 0
	)
	BEGIN
		RAISERROR('WorkId should be NULL when IsWorkIdRequired is false',16,1)
		ROLLBACK
		RETURN
	END
END

GO

CREATE TRIGGER [dbo].[ManualWorkItems_DailyAggrInvalidation_U]
    ON [dbo].[ManualWorkItems]
    FOR UPDATE
    AS
    BEGIN
        SET NoCount ON
		SET XACT_ABORT ON

		IF UPDATE(UserId) AND EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON i.Id = d.Id WHERE i.UserId <> d.UserId)
		BEGIN
			RAISERROR('Cannot update UserId column',16,1)
			ROLLBACK
			RETURN
		END

		declare @rowcnt int
		SET @rowcnt = (SELECT COUNT(*)
						 FROM inserted i
						 JOIN deleted d ON i.Id = d.Id)

		IF @rowcnt = 1 -- possible fast path
		BEGIN
			declare @startDay date, @endDay date, @userId int

			SELECT @startDay = [dbo].[GetDatePart](CASE WHEN i.StartDate<d.StartDate THEN i.StartDate ELSE d.StartDate END),
				   @endDay = [dbo].[GetDatePart](CASE WHEN i.EndDate>d.EndDate THEN i.EndDate ELSE d.EndDate END),
				   @userId = i.[UserId]
			  FROM inserted i
			  JOIN deleted d ON i.Id = d.Id

			IF @startDay = @endDay --fast path
			BEGIN
				MERGE [dbo].[AggregateDailyWorkTimes] WITH (HOLDLOCK) AS a
				USING (
						SELECT @userId AS [UserId], @startDay AS [Day]
						) AS i
					ON  i.[UserId] = a.[UserId] AND i.[Day] = a.[Day]
					WHEN NOT MATCHED THEN
						INSERT ([UserId], [Day]) VALUES (i.[UserId], i.[Day])
					WHEN MATCHED THEN
						UPDATE SET [IsValid] = 0;
				RETURN
			END
		END

		declare @userDays TABLE (
			[UserId] int NOT NULL,
			[Day] date NOT NULL,
			PRIMARY KEY ([UserId], [Day])
		)

   INSERT INTO @userDays
		SELECT DISTINCT i.[UserId],
			   r.[Day]
		  FROM inserted i
		  JOIN deleted d ON i.Id = d.Id
   CROSS APPLY [dbo].[GetDayRange](CASE WHEN i.StartDate<d.StartDate THEN i.StartDate ELSE d.StartDate END, CASE WHEN i.EndDate>d.EndDate THEN i.EndDate ELSE d.EndDate END) r

		MERGE [dbo].[AggregateDailyWorkTimes] WITH (HOLDLOCK) AS a
		USING (
				SELECT [UserId], [Day]
				  FROM @userDays
			  ) AS i
		   ON  i.[UserId] = a.[UserId] AND i.[Day] = a.[Day]
		 WHEN NOT MATCHED THEN
				INSERT ([UserId], [Day]) VALUES (i.[UserId], i.[Day])
		 WHEN MATCHED THEN
				UPDATE SET [IsValid] = 0;
    END
GO

CREATE TRIGGER [dbo].[ManualWorkItems_DailyAggrInvalidation_I]
    ON [dbo].[ManualWorkItems]
    FOR INSERT
    AS
    BEGIN
        SET NoCount ON
		SET XACT_ABORT ON

		declare @rowcnt int
		SET @rowcnt = (SELECT COUNT(*) FROM inserted)

		IF @rowcnt = 1 -- possible fast path
		BEGIN
			declare @startDay date, @endDay date, @userId int

			SELECT @startDay = [dbo].[GetDatePart](StartDate),
				   @endDay = [dbo].[GetDatePart](EndDate),
				   @userId = UserId
			  FROM inserted

			IF @startDay = @endDay --fast path
			BEGIN
				MERGE [dbo].[AggregateDailyWorkTimes] WITH (HOLDLOCK) AS a
				USING (
						SELECT @userId AS [UserId], @startDay AS [Day]
						) AS i
					ON  i.[UserId] = a.[UserId] AND i.[Day] = a.[Day]
					WHEN NOT MATCHED THEN
						INSERT ([UserId], [Day]) VALUES (i.[UserId], i.[Day])
					WHEN MATCHED THEN
						UPDATE SET [IsValid] = 0;
				RETURN
			END
		END

		declare @userDays TABLE (
			[UserId] int NOT NULL,
			[Day] date NOT NULL,
			PRIMARY KEY ([UserId], [Day])
		)

   INSERT INTO @userDays
		SELECT DISTINCT i.[UserId],
			   r.[Day]
		  FROM inserted i
   CROSS APPLY [dbo].[GetDayRange](i.[StartDate], i.[EndDate]) r

		MERGE [dbo].[AggregateDailyWorkTimes] WITH (HOLDLOCK) AS a
		USING (
				SELECT [UserId], [Day]
				  FROM @userDays
			  ) AS i
		   ON  i.[UserId] = a.[UserId] AND i.[Day] = a.[Day]
		 WHEN NOT MATCHED THEN
				INSERT ([UserId], [Day]) VALUES (i.[UserId], i.[Day])
		 WHEN MATCHED THEN
				UPDATE SET [IsValid] = 0;
    END
GO

CREATE TRIGGER [dbo].[ManualWorkItems_DailyAggrInvalidation_D]
    ON [dbo].[ManualWorkItems]
    FOR DELETE
    AS
    BEGIN
        SET NoCount ON
		SET XACT_ABORT ON

		declare @rowcnt int
		SET @rowcnt = (SELECT COUNT(*) FROM deleted)

		IF @rowcnt = 1 -- possible fast path
		BEGIN
			declare @startDay date, @endDay date, @userId int

			SELECT @startDay = [dbo].[GetDatePart](StartDate),
				   @endDay = [dbo].[GetDatePart](EndDate),
				   @userId = UserId
			  FROM deleted

			IF @startDay = @endDay --fast path
			BEGIN
				MERGE [dbo].[AggregateDailyWorkTimes] WITH (HOLDLOCK) AS a
				USING (
						SELECT @userId AS [UserId], @startDay AS [Day]
						) AS i
					ON  i.[UserId] = a.[UserId] AND i.[Day] = a.[Day]
					WHEN NOT MATCHED THEN
						INSERT ([UserId], [Day]) VALUES (i.[UserId], i.[Day])
					WHEN MATCHED THEN
						UPDATE SET [IsValid] = 0;
				RETURN
			END
		END

		declare @userDays TABLE (
			[UserId] int NOT NULL,
			[Day] date NOT NULL,
			PRIMARY KEY ([UserId], [Day])
		)

   INSERT INTO @userDays
		SELECT DISTINCT i.[UserId],
			   r.[Day]
		  FROM deleted i
   CROSS APPLY [dbo].[GetDayRange](i.[StartDate], i.[EndDate]) r

		MERGE [dbo].[AggregateDailyWorkTimes] WITH (HOLDLOCK) AS a
		USING (
				SELECT [UserId], [Day]
				  FROM @userDays
			  ) AS i
		   ON  i.[UserId] = a.[UserId] AND i.[Day] = a.[Day]
		 WHEN NOT MATCHED THEN
				INSERT ([UserId], [Day]) VALUES (i.[UserId], i.[Day])
		 WHEN MATCHED THEN
				UPDATE SET [IsValid] = 0;
    END