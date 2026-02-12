CREATE TABLE [dbo].[CalendarExceptions] (
    [Id]                      INT            IDENTITY (1, 1) NOT NULL,
    [Name]                    NVARCHAR (500) NULL,
    [CreateDate]              DATETIME       CONSTRAINT [DF_CalendarExceptions_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [CalendarId]              INT            NOT NULL,
    [Date]                    DATETIME       NOT NULL,
    [IsWorkDay]               BIT            NOT NULL,
    [TargetWorkTimeInMinutes] SMALLINT       NULL, 
    [CoreTimeEnd]             SMALLINT       NULL, 
    CONSTRAINT [PK_CalendarExceptions] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CalendarExceptions_Calendar] FOREIGN KEY ([CalendarId]) REFERENCES [dbo].[Calendars] ([Id]),
    CONSTRAINT [IX_CalendarExceptions_Unique] UNIQUE NONCLUSTERED ([Date] ASC, [CalendarId] ASC)
);


GO
CREATE CLUSTERED INDEX [IX_CalendarExceptions_Date_Clust]
    ON [dbo].[CalendarExceptions]([Date] ASC);


GO
CREATE TRIGGER [dbo].[CalendarExceptions_Validation]
ON [dbo].[CalendarExceptions]
AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	
	IF EXISTS(SELECT * FROM inserted WHERE Date <> CAST(FLOOR(CAST(Date as float)) as datetime))
	BEGIN
		RAISERROR('Date should have no time part',16,1)
		ROLLBACK
		RETURN
	END
END