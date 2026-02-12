CREATE TABLE [dbo].[ManualWorkItemTypes] (
    [Id]               SMALLINT        NOT NULL,
    [Name]             NVARCHAR (1000) NOT NULL,
    [IsWorkIdRequired] BIT             NOT NULL,
    [Description]      NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_ManualWorkItemTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE TRIGGER [dbo].[ManualWorkItemTypes_Validation]
ON [dbo].[ManualWorkItemTypes]
FOR UPDATE --INSERT check is not needed
AS
BEGIN
	SET NOCOUNT ON;
	--WorkId cannot be NULL when IsWorkIdRequired is true
	IF EXISTS (	
		SELECT 1 FROM inserted i
		JOIN ManualWorkItems m ON i.Id = m.ManualWorkItemTypeId
		WHERE 
			m.WorkId IS NULL
			AND i.IsWorkIdRequired = 1
	)
	BEGIN
		RAISERROR('WorkId cannot be NULL when IsWorkIdRequired is true',16,1)
		ROLLBACK
		RETURN
	END
END
