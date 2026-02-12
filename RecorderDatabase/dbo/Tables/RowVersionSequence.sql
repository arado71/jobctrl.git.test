CREATE TABLE [dbo].[RowVersionSequence] (
    [IsDummy] BIT        NOT NULL,
    [Version] ROWVERSION NOT NULL,
    PRIMARY KEY CLUSTERED ([IsDummy] ASC)
);

