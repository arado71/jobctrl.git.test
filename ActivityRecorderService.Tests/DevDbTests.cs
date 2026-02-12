using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.WorkTimeHistory;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class DevDbTests : IDisposable
	{
		private readonly string origConnStr;
		public DevDbTests()
		{
			origConnStr = Tct.ActivityRecorderService.Properties.Settings.Default._jobcontrolConnectionString;
			Tct.ActivityRecorderService.Properties.Settings.Default["_jobcontrolConnectionString"] = @"Data Source=172.22.1.121\JobCTRL_DEV;Initial Catalog=Jobcontrol;Persist Security Info=True;User ID=tctuser;Password=tct";
		}

		[Fact(Skip = "Needs DEV db data")]
		public void CanGetWorkNames()
		{
			var names = WorkTimeHistoryDbHelper.GetWorkNames(13, new List<int> { 1253, 16766 });
			Assert.NotNull(names);
			Assert.NotEmpty(names.Names);
			Assert.True(names.Names.Single(n => n.Id == 1253).Name.Contains("elefon"));
			Assert.True(names.Names.Any(p => p.ProjectId == names.Names.Single(n => n.Id == 16766).ParentId));
		}

		[Fact(Skip = "Needs DEV db data")]
		public void CanGetWorkTimeHistory()
		{
			var hist = WorkTimeHistoryDbHelper.GetWorkTimeHistory(13, DateTime.Parse("2013-03-02"), DateTime.Parse("2013-03-06"));
			Assert.NotNull(hist);
			Assert.NotEmpty(hist.ComputerIntervals);
			Assert.NotEmpty(hist.MobileIntervals);
			Assert.NotEmpty(hist.ManualIntervals);
		}

		[Fact(Skip = "Needs DEV db data")]
		public void CanGetUserSettings()
		{
			var sett = WorkTimeHistoryDbHelper.GetUserSettings(13);
			Assert.NotNull(sett);
		}

		[Fact(Skip = "Needs DEV db data")]
		public void CanGetWorksOrProjects()
		{
			var names = StatsDbHelper.GetWorksOrProjects(new List<int> { 1253, 16766 });
			Assert.NotNull(names);
			Assert.NotEmpty(names);
			Assert.True(names.Single(n => n.Id == 1253).Name.Contains("elefon"));
			Assert.True(names.Any(p => p.ParentId == names.Single(n => n.Id == 16766).ParentId));
		}

		[Fact(Skip = "Needs DEV db data")]
		public void CanGetALotOfWorksOrProjects()
		{
			var names = StatsDbHelper.GetWorksOrProjects(Enumerable.Range(1, 200000));
			Assert.NotNull(names);
			Assert.NotEmpty(names);
			Assert.True(names.Single(n => n.Id == 1253).Name.Contains("elefon"));
			Assert.True(names.Any(p => p.ParentId == names.Single(n => n.Id == 16766).ParentId));
		}

		public void Dispose()
		{
			Tct.ActivityRecorderService.Properties.Settings.Default["_jobcontrolConnectionString"] = origConnStr;
		}
	}
}
