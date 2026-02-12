CREATE TABLE [dbo].[Issues]
(
    [IssueCode] NVARCHAR(50) NOT NULL, 
	[CompanyId] INT NOT NULL,
    [Name] NVARCHAR(100) NULL, 
    [Company] NVARCHAR(50) NULL, 
    [State] INT NOT NULL, 
    [CreatedAt] DATETIME2 NOT NULL, 
    [CreatedBy] INT NOT NULL, 
    [ModifiedAt] DATETIME2 NOT NULL, 
    [ModifiedBy] INT NOT NULL,
	CONSTRAINT [Issues_PK] PRIMARY KEY([IssueCode], [CompanyId])
)
