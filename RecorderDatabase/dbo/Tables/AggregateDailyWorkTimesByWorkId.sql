CREATE TABLE [dbo].[AggregateDailyWorkTimesByWorkId] (
    [UserId]        INT  NOT NULL,
    [Day]           DATE NOT NULL,
    [WorkId]        INT  NOT NULL,
    [TotalWorkTime] INT  NOT NULL,
    CONSTRAINT [PK_AggregateDailyWorkTimesByWorkId] PRIMARY KEY CLUSTERED ([UserId] ASC, [Day] ASC, [WorkId] ASC)
);





