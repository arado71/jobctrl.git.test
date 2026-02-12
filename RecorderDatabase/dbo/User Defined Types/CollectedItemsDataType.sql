CREATE TYPE [dbo].[CollectedItemsDataType] AS TABLE (
    [UserId]   INT,
    [CreateDate] datetime,
	[ComputerId] INT,
	[KeyId] INT,
	[ValueId] INT,
	[Key] nvarchar(4000),
    [Value] nvarchar(4000)
	);



