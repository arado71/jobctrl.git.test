using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Collector;

namespace Tct.Tests.ActivityRecorderService
{
	public abstract class DbTestsBase
	{
		protected static readonly TestDb testDb = new TestDb();

		static DbTestsBase()
		{
			testDb.InitializeDatabase();
			Tct.ActivityRecorderService.Properties.Settings.Default["recorderConnectionString"] = testDb.ConnectionString;
			Tct.ActivityRecorderService.Properties.Settings.Default["_jobcontrolConnectionString"] = testDb.ConnectionString;
		}

		protected DbTestsBase()
		{
			testDb.PurgeDatabase();
			ActivityRecorderDataClassesDataContext.LookupIdCache.Clear();
			CollectedItemDbHelper.LookupIdCache.Clear();
		}
	}
}
