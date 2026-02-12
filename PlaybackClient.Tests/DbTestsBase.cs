using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.Tests.ActivityRecorderService;

namespace PlaybackClient.Tests
{
	public abstract class DbTestsBase
	{
		protected static readonly TestDb testDb = new TestDb();

		static DbTestsBase()
		{
			testDb.InitializeDatabase();
			Tct.ActivityRecorderService.Properties.Settings.Default["recorderConnectionString"] = testDb.ConnectionString;
			Tct.ActivityRecorderService.Properties.Settings.Default["_jobcontrolConnectionString"] = testDb.ConnectionString;
			PlaybackClient.Properties.Settings.Default["mobileConnectionString"] = testDb.ConnectionString;
		}

		protected DbTestsBase()
		{
			testDb.PurgeDatabase();
		}
	}
}

