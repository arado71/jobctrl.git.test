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
ALTER TABLE dbo.VoiceRecordings
	DROP CONSTRAINT DF_VoiceRecordings_ReceiveDate
GO
CREATE TABLE dbo.Tmp_VoiceRecordings
	(
	Id int NOT NULL IDENTITY (1, 1),
	ClientId uniqueidentifier NOT NULL,
	UserId int NOT NULL,
	WorkId int NULL,
	StartDate datetime NOT NULL,
	EndDate datetime NULL,
	Duration int NOT NULL,
	Codec int NOT NULL,
	Name nvarchar(200) NULL,
	Extension varchar(10) NULL,
	FirstReceiveDate datetime NOT NULL,
	LastReceiveDate datetime NOT NULL,
	ChunkCount int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_VoiceRecordings SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_VoiceRecordings ADD CONSTRAINT
	DF_VoiceRecordings_ClientId DEFAULT newid() FOR ClientId
GO
ALTER TABLE dbo.Tmp_VoiceRecordings ADD CONSTRAINT
	DF_VoiceRecordings_ReceiveDate DEFAULT (getutcdate()) FOR FirstReceiveDate
GO
ALTER TABLE dbo.Tmp_VoiceRecordings ADD CONSTRAINT
	DF_VoiceRecordings_LastReceiveDate DEFAULT (getutcdate()) FOR LastReceiveDate
GO
ALTER TABLE dbo.Tmp_VoiceRecordings ADD CONSTRAINT
	DF_VoiceRecordings_ChunkCount DEFAULT 0 FOR ChunkCount
GO
SET IDENTITY_INSERT dbo.Tmp_VoiceRecordings ON
GO
IF EXISTS(SELECT * FROM dbo.VoiceRecordings)
	 EXEC('INSERT INTO dbo.Tmp_VoiceRecordings (Id, UserId, WorkId, StartDate, Duration, Codec, Name, Extension, FirstReceiveDate)
		SELECT Id, UserId, WorkId, CreateDate, Length, Codec, Name, Extension, ReceiveDate FROM dbo.VoiceRecordings WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_VoiceRecordings OFF
GO
DROP TABLE dbo.VoiceRecordings
GO
EXECUTE sp_rename N'dbo.Tmp_VoiceRecordings', N'VoiceRecordings', 'OBJECT' 
GO
ALTER TABLE dbo.VoiceRecordings ADD CONSTRAINT
	PK_VoiceRecordings PRIMARY KEY NONCLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX IX_VoiceRecordings_UserId_StartDate_Clust ON dbo.VoiceRecordings
	(
	UserId,
	StartDate
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_VoiceRecordings_ClientId ON dbo.VoiceRecordings
	(
	ClientId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT


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
ALTER TABLE dbo.VoiceRecordings
	DROP CONSTRAINT DF_VoiceRecordings_ClientId
GO
ALTER TABLE dbo.VoiceRecordings
	DROP CONSTRAINT DF_VoiceRecordings_ChunkCount
GO
ALTER TABLE dbo.VoiceRecordings SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

BEGIN TRANSACTION
GO
EXECUTE sp_rename N'dbo.VoiceRecordings.ChunkCount', N'Tmp_Offset', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.VoiceRecordings.Tmp_Offset', N'Offset', 'COLUMN' 
GO
ALTER TABLE dbo.VoiceRecordings SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

BEGIN TRANSACTION
GO
ALTER TABLE dbo.VoiceRecordings ADD
	DeleteDate datetime NULL
GO
ALTER TABLE dbo.VoiceRecordings SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


