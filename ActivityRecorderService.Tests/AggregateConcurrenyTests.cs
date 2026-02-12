using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class AggregateConcurrenyTests : IDisposable
	{
		private static readonly TestDb testDb = new TestDb();

		private static readonly string connectionString = testDb.ConnectionString;

		private const string dropTable = "IF NULLIF(object_id('TestSrcTable'), 0) IS NOT NULL DROP TABLE TestSrcTable";
		private const string createTable = "CREATE TABLE TestSrcTable([Id] [int] IDENTITY(1,1) NOT NULL, [Value] [int] NOT NULL)";
		private const string insertIntoTable = "INSERT INTO TestSrcTable (Value) VALUES (@value)";

		private const string dropAggrTable = "IF NULLIF(object_id('TestAggrTable'), 0) IS NOT NULL DROP TABLE TestAggrTable";
		private const string createAggrTable = "CREATE TABLE TestAggrTable([Id] [int] NOT NULL, [Count] [int] NOT NULL)";
		private const string checkAggrTable = "SELECT [Id], [Count] FROM dbo.TestAggrTable";

		private const string dropAggrIdTable = "IF NULLIF(object_id('TestAggrIdTable'), 0) IS NOT NULL DROP TABLE TestAggrIdTable";
		private const string createAggrIdTable = "CREATE TABLE TestAggrIdTable([Id] [int] NOT NULL)";

		private const string nameAggrSproc = "[dbo].[AggregateTest]";
		private const string dropAggrSproc = @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AggregateTest]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AggregateTest]";
		private const string createAggrSproc = @"
CREATE PROCEDURE [dbo].[AggregateTest]
	--(
	--@StartId int,
	--@EndId int OUTPUT
	--)
AS
SET NOCOUNT ON
SET XACT_ABORT ON

declare @EndId int,
		@StartId int,
		@MaxEndId int

BEGIN TRAN
SET @MaxEndId = (SELECT MAX(Id) FROM [dbo].[TestSrcTable] WITH (TABLOCK)) --  Without (TABLOCK) ConcurrencyTest2 would fail
SET @StartId = (SELECT ISNULL(MAX(Id),0) FROM dbo.TestAggrIdTable WITH (TABLOCKX, HOLDLOCK)) 
SET @EndId = @StartId

declare
@c_Id int,
@c_Value int

DECLARE interval_cursor CURSOR FAST_FORWARD FOR 
SELECT [Id]
      ,[Value]
  FROM [dbo].[TestSrcTable]
 WHERE [Id] > @StartId 
   AND [Id] <= @MaxEndId --ConcurrencyTest would fail if I comment this out and there is no (TABLOCKX) for this statement


OPEN interval_cursor

WHILE 1=1
BEGIN
	FETCH NEXT FROM interval_cursor INTO 
		@c_Id,
		@c_Value

	IF @@FETCH_STATUS <> 0
		BREAK

	IF (@EndId<@c_Id) SET @EndId = @c_Id

	UPDATE [dbo].[TestAggrTable]
	   SET [Count] = [Count] + 1
	 WHERE [Id] = @c_Value

	IF @@rowcount = 0
	BEGIN
		INSERT INTO [dbo].[TestAggrTable] ([Id], [Count]) VALUES (@c_Value, 1)
	END

END

CLOSE interval_cursor
DEALLOCATE interval_cursor

TRUNCATE TABLE dbo.TestAggrIdTable
INSERT INTO dbo.TestAggrIdTable VALUES (@EndId)

--WAITFOR DELAY '00:00:01'

COMMIT TRAN	
	
RETURN
";
		static AggregateConcurrenyTests()
		{
			testDb.InitializeDatabase();
		}

		public AggregateConcurrenyTests()
		{
			CreateSchemas();
		}

		private static void CreateSchemas()
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();
				foreach (var sqlcmd in new[] { dropTable, createTable, dropAggrTable, createAggrTable, dropAggrIdTable, createAggrIdTable, dropAggrSproc, createAggrSproc })
				{
					using (var sqlCommand = new SqlCommand(sqlcmd, sqlConnection))
					{
						sqlCommand.ExecuteNonQuery();
					}
				}
			}
		}

		private static void DropSchemas()
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();
				foreach (var sqlcmd in new[] { dropTable, dropAggrTable, dropAggrIdTable, dropAggrSproc })
				{
					using (var sqlCommand = new SqlCommand(sqlcmd, sqlConnection))
					{
						sqlCommand.ExecuteNonQuery();
					}
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			try
			{
				DropSchemas();
			}
			catch { }
		}

		#endregion

		[Fact]
		public void CanCreateDb()
		{
		}

		[Fact]
		public void AggreageteEmpty()
		{
			AggregateData();
		}

		[Fact]
		public void CanAggregateThreeSimpleData()
		{
			//Arrange
			InsertData(3, 2);

			//Act
			AggregateData();

			//Assert
			var result = GetAggregateResults().Single();
			Assert.Equal(2, result.Id);
			Assert.Equal(3, result.Count);
		}

		[Fact]
		public void CanAggregateTwoDifferentData()
		{
			//Arrange
			InsertData(1, 4);
			InsertData(3, 5);
			InsertData(1, 4);

			//Act
			AggregateData();

			//Assert
			var result = GetAggregateResults().Where(n => n.Id == 4).Single();
			Assert.Equal(2, result.Count);
			result = GetAggregateResults().Where(n => n.Id == 5).Single();
			Assert.Equal(3, result.Count);
		}

		[Fact]
		public void ConcurrencyTest()
		{
			const int iter = 300;
			const int numInsBefore = 100;
			const int numInsAfter = 100;
			Parallel.For(0, iter, i =>
								{
									InsertData(numInsBefore, 1);
									AggregateData();
									InsertData(numInsAfter, 1);
								});
			AggregateData();

			//Assert
			var result = GetAggregateResults().Single();
			Assert.Equal(1, result.Id);
			Assert.Equal(iter * (numInsBefore + numInsAfter), result.Count);
		}

		[Fact(Skip = "Takes 16 mins")]
		public void ConcurrencyTest2()
		{
			const int iter = 30000;
			const int numInsBefore = 1;
			const int numInsAfter = 1;
			var tasks = new Task[iter];
			for (int i = 0; i < iter; i++)
			{
				tasks[i] = Task.Factory.StartNew(() =>
					{
						InsertData(numInsBefore, 1);
						AggregateData();
						InsertData(numInsAfter, 1);
					}
				);
			}
			Task.WaitAll(tasks);
			AggregateData();

			//Assert
			var result = GetAggregateResults().Single();
			Assert.Equal(1, result.Id);
			Assert.Equal(iter * (numInsBefore + numInsAfter), result.Count);
		}

		[Fact(Skip = "Takes 1 min")] //1min
		public void ConcurrencyTest3()
		{
			const int iterOuter = 20;
			const int iterInner = 60;
			const int numInsBefore = 100;
			const int numInsAfter = 100;
			var signals = new ManualResetEvent[iterInner];
			for (int i = 0; i < iterInner; i++)
			{
				signals[i] = new ManualResetEvent(false);
			}
			Parallel.For(0, iterOuter, i =>
			{
				for (int j = 0; j < iterInner; j++)
				{
					InsertData(numInsBefore, 1);
					var currentCheckPoint = signals[j];
					var shouldAggregate = false;
					lock (currentCheckPoint)
					{
						var gotSignal = currentCheckPoint.WaitOne(0);
						if (!gotSignal)
						{
							shouldAggregate = true;
							currentCheckPoint.Set();
						}
					}
					if (shouldAggregate)
					{
						AggregateData();
					}
					InsertData(numInsAfter, 1);
				}
			});
			AggregateData();

			//Assert
			var result = GetAggregateResults().Single();
			Assert.Equal(1, result.Id);
			Assert.Equal(iterOuter * iterInner * (numInsBefore + numInsAfter), result.Count);
		}

		[Fact(Skip = "Takes 1 min")]
		public void ConcurrencyTest4()
		{
			const int iterOuter = 60;
			const int iterInner = 20;
			const int numInsBefore = 100;
			const int numInsAfter = 100;
			var signals = new ManualResetEvent[iterInner];
			for (int i = 0; i < iterInner; i++)
			{
				signals[i] = new ManualResetEvent(false);
			}
			var tasks = new Task[iterOuter];
			for (int i = 0; i < iterOuter; i++)
			{
				tasks[i] = Task.Factory.StartNew(() =>
					{
						for (int j = 0; j < iterInner; j++)
						{
							InsertData(numInsBefore, 1);
							var currentCheckPoint = signals[j];
							var shouldAggregate = false;
							lock (currentCheckPoint)
							{
								var gotSignal = currentCheckPoint.WaitOne(0);
								if (!gotSignal)
								{
									shouldAggregate = true;
									currentCheckPoint.Set();
								}
							}
							if (shouldAggregate)
							{
								AggregateData();
							}
							InsertData(numInsAfter, 1);
						}
					});
			}
			Task.WaitAll(tasks);
			AggregateData();

			//Assert
			var result = GetAggregateResults().Single();
			Assert.Equal(1, result.Id);
			Assert.Equal(iterOuter * iterInner * (numInsBefore + numInsAfter), result.Count);
		}

		[Fact(Skip = "Takes 5 mins")]
		public void ConcurrencyTest5()
		{
			const int iterOuter = 6;
			const int iterInner = 1000;
			const int numInsBefore = 1;
			const int numInsAfter = 1;
			var tasks = new Task[iterOuter];
			for (int i = 0; i < iterOuter; i++)
			{
				tasks[i] = Task.Factory.StartNew(() =>
				{
					for (int j = 0; j < iterInner; j++)
					{
						InsertData(numInsBefore, 1);
						AggregateData();
						InsertData(numInsAfter, 1);
					}
				});
			}
			Task.WaitAll(tasks);
			AggregateData();

			//Assert
			var result = GetAggregateResults().Single();
			Assert.Equal(1, result.Id);
			Assert.Equal(iterOuter * iterInner * (numInsBefore + numInsAfter), result.Count);
		}

		private static void InsertData(int numInserts, int value)
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();
				//using (var tran = sqlConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
				{
					int times = numInserts;
					while (--times >= 0)
					{
						using (var sqlCommand = new SqlCommand(insertIntoTable, sqlConnection))
						{
							//sqlCommand.Transaction = tran;
							sqlCommand.Parameters.Add(new SqlParameter("value", System.Data.SqlDbType.Int) { Value = value });
							sqlCommand.ExecuteNonQuery();
						}
					}
					//tran.Commit();
				}
			}
		}

		private static void AggregateData()
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(nameAggrSproc, sqlConnection))
				{
					sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
					sqlCommand.ExecuteNonQuery();
				}
			}
		}

		private static IEnumerable<AggrResult> GetAggregateResults()
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(checkAggrTable, sqlConnection))
				{
					using (var reader = sqlCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							yield return new AggrResult() { Id = reader.GetSqlInt32(0).Value, Count = reader.GetSqlInt32(1).Value };
						}
					}
				}
			}
		}

		private class AggrResult
		{
			public int Id { get; set; }
			public int Count { get; set; }
		}
	}
}
