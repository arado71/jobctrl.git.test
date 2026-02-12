CREATE TABLE [dbo].[Calendars] (
	[Id]                                INT            IDENTITY (1, 1) NOT NULL,
	[Name]                              NVARCHAR (500) NULL,
	[CreateDate]                        DATETIME       CONSTRAINT [DF_Calendar_CreateData] DEFAULT (getutcdate()) NOT NULL,
	[IsMondayWorkDay]                   BIT            NOT NULL,
	[IsTuesdayWorkDay]                  BIT            NOT NULL,
	[IsWednesdayWorkDay]                BIT            NOT NULL,
	[IsThursdayWorkDay]                 BIT            NOT NULL,
	[IsFridayWorkDay]                   BIT            NOT NULL,
	[IsSaturdayWorkDay]                 BIT            NOT NULL,
	[IsSundayWorkDay]                   BIT            NOT NULL,
	[InheritedFrom]                     INT            NULL,
	[TargetWorkTimeInMinutesMonday]     SMALLINT       NULL, 
	[TargetWorkTimeInMinutesTuesday]    SMALLINT       NULL, 
	[TargetWorkTimeInMinutesWednesday]  SMALLINT       NULL, 
	[TargetWorkTimeInMinutesThursday]   SMALLINT       NULL, 
	[TargetWorkTimeInMinutesFriday]     SMALLINT       NULL, 
	[TargetWorkTimeInMinutesSaturday]   SMALLINT       NULL, 
	[TargetWorkTimeInMinutesSunday]     SMALLINT       NULL, 
	[CoreTimeEndMonday]                 SMALLINT       NULL, 
	[CoreTimeEndTuesday]                SMALLINT       NULL, 
	[CoreTimeEndWednesday]              SMALLINT       NULL, 
	[CoreTimeEndThursday]               SMALLINT       NULL, 
	[CoreTimeEndFriday]                 SMALLINT       NULL, 
	[CoreTimeEndSaturday]               SMALLINT       NULL, 
	[CoreTimeEndSunday]                 SMALLINT       NULL, 
	CONSTRAINT [PK_Calendar] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_Calendars_Calendars] FOREIGN KEY ([InheritedFrom]) REFERENCES [dbo].[Calendars] ([Id])
);


GO
CREATE TRIGGER [dbo].[Calendars_Validation]
ON [dbo].[Calendars]
AFTER INSERT, UPDATE --INSERT is needed because we can insert a row referencing itself
AS
BEGIN
	SET NOCOUNT ON;
	
	--probably we should lock table [dbo].[Calendars]
	declare
	@curr_id int,
	@ci_Id bigint,
	@ci_InheritedFrom int

	DECLARE inserted_cursor CURSOR FAST_FORWARD FOR 
	SELECT [Id]
		  ,[InheritedFrom]
	  FROM inserted
	  
	OPEN inserted_cursor

	WHILE 1=1
	BEGIN
		FETCH NEXT FROM inserted_cursor INTO 
			@ci_Id,
			@ci_InheritedFrom

		IF @@FETCH_STATUS <> 0
			BREAK

		SET @curr_id = @ci_InheritedFrom
		WHILE @curr_id IS NOT NULL
		BEGIN
			IF (@curr_id = @ci_Id)
			BEGIN
				RAISERROR('Cannot create circular reference',16,1)
				ROLLBACK
				RETURN
			END
			SET @curr_id = (SELECT [InheritedFrom] FROM [dbo].[Calendars] WHERE [Id] = @curr_id)
		END
		
	END
	
	CLOSE inserted_cursor
	DEALLOCATE inserted_cursor

END
