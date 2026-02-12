CREATE TYPE [dbo].[WorkTimesById] AS TABLE (
    [WorkId]   INT NOT NULL,
    [WorkTime] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([WorkId] ASC));

