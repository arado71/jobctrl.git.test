CREATE TABLE [dbo].[IvrWorkItems] (
    [Id]                        INT          IDENTITY (1, 1) NOT NULL,
    [WorkId]                    INT          NOT NULL,
    [StartDate]                 DATETIME     NOT NULL,
    [EndDate]                   DATETIME     NULL,
    [MaxEndDate]                DATETIME     NOT NULL,
    [UserId]                    INT          NOT NULL,
    [GroupId]                   INT          NOT NULL,
    [CompanyId]                 INT          NOT NULL,
    [PhoneNumber]               VARCHAR (50) NOT NULL,
    [TrunkId]                   VARCHAR (50) NOT NULL,
    [AutoEndOnComputerActivity] BIT          NOT NULL,
    [IvrLastCheckDate]          DATETIME     NOT NULL,
    [CreateDate]                DATETIME     CONSTRAINT [DF_IvrWorkItems_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [LogoffMaxEndDate]          DATETIME     NULL,
    [InstantNotificationEmail]  BIT          CONSTRAINT [DF_IvrWorkItems_InstantNotificationEmail] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_IvrWorkItems] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO

CREATE TRIGGER [dbo].[IvrWorkItems_DailyAggrInvalidation_U]
    ON [dbo].[IvrWorkItems]
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
				   @endDay = [dbo].[GetDatePart](CASE WHEN [dbo].[GetIvrEndDateNotNull](i.EndDate,i.IvrLastCheckDate,i.MaxEndDate)>[dbo].[GetIvrEndDateNotNull](d.EndDate,d.IvrLastCheckDate,d.MaxEndDate) THEN [dbo].[GetIvrEndDateNotNull](i.EndDate,i.IvrLastCheckDate,i.MaxEndDate) ELSE [dbo].[GetIvrEndDateNotNull](d.EndDate,d.IvrLastCheckDate,d.MaxEndDate) END),
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
   CROSS APPLY [dbo].[GetDayRange](CASE WHEN i.StartDate<d.StartDate THEN i.StartDate ELSE d.StartDate END, CASE WHEN [dbo].[GetIvrEndDateNotNull](i.EndDate,i.IvrLastCheckDate,i.MaxEndDate)>[dbo].[GetIvrEndDateNotNull](d.EndDate,d.IvrLastCheckDate,d.MaxEndDate) THEN [dbo].[GetIvrEndDateNotNull](i.EndDate,i.IvrLastCheckDate,i.MaxEndDate) ELSE [dbo].[GetIvrEndDateNotNull](d.EndDate,d.IvrLastCheckDate,d.MaxEndDate) END) r

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

CREATE TRIGGER [dbo].[IvrWorkItems_DailyAggrInvalidation_I]
    ON [dbo].[IvrWorkItems]
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
				   @endDay = [dbo].[GetDatePart]([dbo].[GetIvrEndDateNotNull](EndDate,IvrLastCheckDate,MaxEndDate)),
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
   CROSS APPLY [dbo].[GetDayRange](i.[StartDate], [dbo].[GetIvrEndDateNotNull](i.EndDate,i.IvrLastCheckDate,i.MaxEndDate)) r

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
CREATE TRIGGER [dbo].[IvrWorkItems_DailyAggrInvalidation_D]
    ON [dbo].[IvrWorkItems]
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
				   @endDay = [dbo].[GetDatePart]([dbo].[GetIvrEndDateNotNull](EndDate,IvrLastCheckDate,MaxEndDate)),
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
   CROSS APPLY [dbo].[GetDayRange](i.[StartDate], [dbo].[GetIvrEndDateNotNull](i.EndDate,i.IvrLastCheckDate,i.MaxEndDate)) r

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