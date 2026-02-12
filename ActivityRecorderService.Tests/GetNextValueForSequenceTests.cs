using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class GetNextValueForSequenceTests : DbTestsBase
	{
		[Fact]
		public void GetVersion()
		{
			using (var conn = new SqlConnection(Tct.ActivityRecorderService.Properties.Settings.Default.recorderConnectionString))
			{
				var p = new DynamicParameters();
				p.Add("@nextValue", dbType: DbType.Binary, direction: ParameterDirection.Output, size: 8);
				conn.Query<int>("GetNextValueForSequence", p, commandType: CommandType.StoredProcedure);
				var nextVal = p.Get<byte[]>("@nextValue");
				//Console.WriteLine(new Binary(nextVal));
				Assert.NotNull(nextVal);
			}
		}

		[Fact]
		public void ConcurrencyTest()
		{
			var tasks = new List<Task>();
			for (int i = 0; i < 1000; i++)
			{
				tasks.Add(Task.Run(() => GetVersion()));
			}
			Task.WaitAll(tasks.ToArray());
		}

		[Fact(Skip = "Slow")]
		public void ConcurrencyUpdateClientSettingsTest()
		{
			using (var conn = new SqlConnection(Tct.ActivityRecorderService.Properties.Settings.Default.recorderConnectionString))
			{
				conn.ExecuteScalar("INSERT INTO ClientSettings (UserId) VALUES (10)");
			}
			var tasks = new List<Task>();
			var cts = new CancellationTokenSource();
			for (int i = 0; i < 20; i++)
			{
				tasks.Add(Task.Run(() => UpdateClientSettings(cts)));
			}
			Task.WaitAll(tasks.ToArray());
			Assert.True(cts.IsCancellationRequested);
		}

		public void UpdateClientSettings(CancellationTokenSource cts)
		{
			try
			{
				using (var conn = new SqlConnection(Tct.ActivityRecorderService.Properties.Settings.Default.recorderConnectionString))
				{
					for (int i = 0; i < 1000; i++)
					{
						if (cts.IsCancellationRequested) break;
						conn.ExecuteScalar("UPDATE ClientSettings SET Menu = '1' WHERE UserId = 10");
						if (cts.IsCancellationRequested) break;
						conn.ExecuteScalar("UPDATE ClientSettings SET Menu = '2' WHERE UserId = 10");
					}
				}
			}
			catch (SqlException ex)
			{
				Console.WriteLine(ex);
				cts.Cancel();
			}
		}
	}
}
