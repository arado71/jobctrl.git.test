using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class TestDb
	{
		private static bool databaseInitialized = false;
		private static readonly string defaultOriginalConnStr = Environment.GetEnvironmentVariable("JC_TEST_CONNECTIONSTRING") ?? "Data Source=.;Initial Catalog=recorder_test;Integrated Security=True;Pooling=False";
		private static readonly string[] initializeOnceCommands = new string[] { @"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'PurgeDatabase' AND ROUTINE_SCHEMA = 'dbo' AND ROUTINE_TYPE = 'PROCEDURE')
 EXEC ('DROP PROCEDURE [dbo].[PurgeDatabase]')
", @"
CREATE PROCEDURE [dbo].[PurgeDatabase] AS
BEGIN

	-- disable all constraints
	EXEC sp_msforeachtable ""ALTER TABLE ? NOCHECK CONSTRAINT all"";
	-- disable all triggers
	EXEC sp_msforeachtable ""ALTER TABLE ? DISABLE Trigger all"";

	-- delete data in all tables
	EXEC sp_MSForEachTable ""SET QUOTED_IDENTIFIER ON; DELETE FROM ?"";

	-- enable all triggers
	EXEC sp_msforeachtable ""ALTER TABLE ? ENABLE Trigger all"";
	-- enable all constraints
	exec sp_msforeachtable ""ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"";
END
"};

		public string ConnectionString
		{
			get
			{
				return connectionString;
			}
		}

		private readonly string connectionString;

		public TestDb()
			: this(defaultOriginalConnStr)
		{
		}

		private TestDb(string originalConnectionString)
		{
			if (originalConnectionString == null) throw new ArgumentNullException("originalConnectionString");
			connectionString = originalConnectionString;
		}

		public void InitializeDatabase()
		{
			if (!databaseInitialized)
			{
				databaseInitialized = true;
				StringBuilder generateSql = new StringBuilder();
				var assembly = Assembly.GetExecutingAssembly();
				using (var s = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().First(n => n.EndsWith(".IvrGenerateAll.sql"))))
				using (StreamReader reader = new StreamReader(s, Encoding.Default))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						generateSql.AppendLine(line);
					}
				}

				string[] sqlCommands =
						Regex.Split(generateSql.ToString(), @"^GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline)
							.Select(n => n.Trim())
							.Where(n => !string.IsNullOrEmpty(n))
							.ToArray();

				using (var conn = new SqlConnection(ConnectionString))
				{
					conn.Open();
					foreach (var command in initializeOnceCommands)
					{
						using (var cmd = new SqlCommand(command, conn))
						{
							cmd.ExecuteNonQuery();
						}
					}

					foreach (string sqlCommand in sqlCommands)
					{
						using (SqlCommand cmd = new SqlCommand(sqlCommand, conn))
						{
							cmd.ExecuteNonQuery();
						}
					}
				}
			}
		}

		public void PurgeDatabase()
		{
			using (var conn = new SqlConnection(ConnectionString))
			{
				conn.Open();
				using (var cmd = new SqlCommand("[dbo].[PurgeDatabase]", conn){CommandType = CommandType.StoredProcedure})
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

	}
}
