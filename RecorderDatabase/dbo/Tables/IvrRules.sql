CREATE TABLE [dbo].[IvrRules] (
    [Id]                        INT             IDENTITY (1, 1) NOT NULL,
    [StartTime]                 INT             NOT NULL,
    [EndTime]                   INT             NOT NULL,
    [IncrementInside]           INT             NOT NULL,
    [IncrementOutside]          INT             NOT NULL,
    [IncrementInsideMaxTime]    INT             NULL,
    [AutoEndOnComputerActivity] BIT             NOT NULL,
    [Name]                      NVARCHAR (1000) NULL,
    [CreateDate]                DATETIME        CONSTRAINT [DF_IvrRules_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    [LogoffMaxEndTime]          INT             NULL,
    [CustomRuleId]              INT             NULL,
    [InstantNotificationEmail]  BIT             CONSTRAINT [DF_IvrRules_InstantNotificationEmail] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_IvrRules] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_IvrRules_IvrCustomRules] FOREIGN KEY ([CustomRuleId]) REFERENCES [dbo].[IvrCustomRules] ([Id])
);


GO
CREATE TRIGGER [dbo].[IvrRules_Validation]
ON [dbo].[IvrRules]
AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	
	IF EXISTS(SELECT * FROM inserted WHERE StartTime<0 OR EndTime<0 OR IncrementInside<0 OR IncrementOutside<0)
	BEGIN
		RAISERROR('Cannot insert negative values for StartTime, EndTime, IncrementInside, IncrementOutside',16,1)
		ROLLBACK
		RETURN
	END
	IF EXISTS(SELECT * FROM inserted WHERE EndTime<StartTime)
	BEGIN
		RAISERROR('StartTime should be less than or equal to EndTime',16,1)
		ROLLBACK
		RETURN
	END
	IF EXISTS(SELECT * FROM inserted WHERE IncrementInsideMaxTime IS NOT NULL AND 
				(
				(IncrementInsideMaxTime<EndTime)
				OR
				(IncrementInsideMaxTime>EndTime+IncrementInside)
				))
	BEGIN
		RAISERROR('IncrementInsideMaxTime should be between EndTime and EndTime+IncrementInside',16,1)
		ROLLBACK
		RETURN
	END
	IF EXISTS(SELECT * FROM inserted WHERE LogoffMaxEndTime IS NOT NULL AND 
				(
				(IncrementInsideMaxTime IS NOT NULL AND LogoffMaxEndTime<IncrementInsideMaxTime)
				OR
				(IncrementInsideMaxTime IS NULL AND LogoffMaxEndTime<EndTime+IncrementInside)
				))
	BEGIN
		RAISERROR('LogoffMaxEndTime should be greater than or equal to IncrementInsideMaxTime ?? EndTime+IncrementInside',16,1)
		ROLLBACK
		RETURN
	END

END
