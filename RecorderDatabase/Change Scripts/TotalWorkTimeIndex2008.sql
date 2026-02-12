--This is needed  to speed up GetTotalWorkTimeByWorkIdForUser sproc
--more specifically this will speed up GetWorkTimeByWorkIdForUser which is used several times when there are some comp corrections

CREATE NONCLUSTERED INDEX [IX_AggregateWorkItemIntervals_UserId_StartDate_EndDate_I_WorkdId]
ON [dbo].[AggregateWorkItemIntervals] ([UserId],[StartDate],[EndDate])
INCLUDE ([WorkId])