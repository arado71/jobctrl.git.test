CREATE TABLE [dbo].[IvrUserWorks] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [PhoneNumber] VARCHAR (50) NOT NULL,
    [TrunkId]     VARCHAR (50) NOT NULL,
    [UserId]      INT          NOT NULL,
    [WorkId]      INT          NOT NULL,
    [RuleId]      INT          NOT NULL,
    [CreateDate]  DATETIME     CONSTRAINT [DF_IvrUserWorks_CreateDate] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_IvrUserWorks] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_IvrUserWorks_IvrRules] FOREIGN KEY ([RuleId]) REFERENCES [dbo].[IvrRules] ([Id]),
    CONSTRAINT [IX_IvrWorks_Unique] UNIQUE NONCLUSTERED ([PhoneNumber] ASC, [TrunkId] ASC)
);


GO
CREATE TRIGGER [dbo].[IvrUserWorks_Validation]
ON [dbo].[IvrUserWorks]
AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	--A PhoneNumber should identify one UserId
	IF EXISTS (	
		SELECT * 
		  FROM (
				SELECT DISTINCT PhoneNumber, UserId FROM IvrUserWorks
			   ) g 
	  GROUP BY g.PhoneNumber HAVING COUNT(*)>1
	)
	BEGIN
		RAISERROR('A PhoneNumber can only be assigned to one user',16,1)
		ROLLBACK
		RETURN
	END
END