/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.DeadLetterItems
	(
	Id int IDENTITY(1,1) NOT NULL,
	WorkId int NULL,
	StartDate datetime NOT NULL,
	EndDate datetime NOT NULL,
	UserId int NOT NULL,
	CreateDate datetime NOT NULL,
	ItemType nvarchar(200) NULL,
	ErrorText nvarchar(3000) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.DeadLetterItems ADD CONSTRAINT
	DF_DeadLetterItems_CreateDate DEFAULT (getutcdate()) FOR CreateDate
GO
ALTER TABLE dbo.DeadLetterItems ADD CONSTRAINT
	PK_DeadLetterItems PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.DeadLetterItems SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
