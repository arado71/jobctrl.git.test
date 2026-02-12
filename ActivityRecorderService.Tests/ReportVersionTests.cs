using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.ClientComputerData;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class ReportVersionTests : DbTestsBase
	{
		private const string defaultAppName = "JobCTRL";
		private const string anotherAppName = "TestCTRL";

		[Fact]
		public void InsertFirstVersion()
		{
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ReportClientComputerVersion(13, 123123213, 1, 2, 3, 4, defaultAppName);
				var res = context.GetClientComputerVersions().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);
			}
		}

		[Fact]
		public void UpdateSameVersion()
		{
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ReportClientComputerVersion(13, 123123213, 1, 2, 3, 4, defaultAppName);
				context.ReportClientComputerVersion(13, 123123213, 1, 2, 3, 4, defaultAppName);
				var res = context.GetClientComputerVersions().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);
			}
		}

		[Fact]
		public void UpdateVersion()
		{
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ReportClientComputerVersion(13, 123123213, 1, 1, 1, 1, defaultAppName);
				context.ReportClientComputerVersion(13, 123123213, 1, 2, 3, 4, defaultAppName);
				var resList = context.GetClientComputerVersions().ToList();

				var res = resList.Where(n => n.IsCurrent).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);

				res = resList.Where(n => !n.IsCurrent).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(1, res.Minor);
				Assert.Equal(1, res.Build);
				Assert.Equal(1, res.Revision);
				Assert.Equal(false, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);
			}
		}

		[Fact]
		public void InsertDifferentCompSameUserVersions()
		{
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ReportClientComputerVersion(13, 123123213, 1, 2, 3, 4, defaultAppName);
				context.ReportClientComputerVersion(13, 456456452, 1, 2, 3, 4, defaultAppName);
				var res = context.GetClientComputerVersions().Where(n => n.ComputerId == 123123213).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);

				res = context.GetClientComputerVersions().Where(n => n.ComputerId == 456456452).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(456456452, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);
			}
		}


		[Fact]
		public void InsertDifferentUserSameCompVersions()
		{
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ReportClientComputerVersion(13, 123123213, 1, 2, 3, 4, defaultAppName);
				context.ReportClientComputerVersion(3453, 123123213, 1, 2, 3, 4, defaultAppName);
				var res = context.GetClientComputerVersions().Where(n => n.UserId == 13).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);

				res = context.GetClientComputerVersions().Where(n => n.UserId == 3453).Single();
				Assert.Equal(3453, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);
			}
		}

		[Fact]
		public void UpdateVersionToNewerThenBackToOriginal()
		{
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ReportClientComputerVersion(13, 123123213, 1, 2, 3, 4, defaultAppName);
				context.ReportClientComputerVersion(13, 123123213, 2, 3, 4, 5, defaultAppName);
				context.ReportClientComputerVersion(13, 123123213, 1, 2, 3, 4, defaultAppName);
				var res = context.GetClientComputerVersions().OrderBy(n => n.Id).First();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(false, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);

				res = context.GetClientComputerVersions().OrderBy(n => n.Id).ElementAt(1);
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(2, res.Major);
				Assert.Equal(3, res.Minor);
				Assert.Equal(4, res.Build);
				Assert.Equal(5, res.Revision);
				Assert.Equal(false, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);

				res = context.GetClientComputerVersions().OrderBy(n => n.Id).Last();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);
			}
		}

		[Fact]
		public void SameVersionWithNewServerAndDefaultApplication()
		{
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ReportClientComputerVersion(13, 123123213, 1, 1, 1, 1, null);
				context.ReportClientComputerVersion(13, 123123213, 1, 1, 1, 1, defaultAppName);
				var res = context.GetClientComputerVersions().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(1, res.Minor);
				Assert.Equal(1, res.Build);
				Assert.Equal(1, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(null, res.Application);
			}
		}

		[Fact]
		public void SameVersionWithNewServerAndAnotherApplication()
		{
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ReportClientComputerVersion(13, 123123213, 1, 1, 1, 1, null);
				context.ReportClientComputerVersion(13, 123123213, 1, 1, 1, 1, anotherAppName);
				var res = context.GetClientComputerVersions().Where(n => n.Application == null).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(1, res.Minor);
				Assert.Equal(1, res.Build);
				Assert.Equal(1, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(null, res.Application);

				res = context.GetClientComputerVersions().Where(n => n.Application == anotherAppName).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(1, res.Minor);
				Assert.Equal(1, res.Build);
				Assert.Equal(1, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(anotherAppName, res.Application);
			}
		}

		[Fact]
		public void UpdateVersionWithNewServerAndDefaultApplication()
		{
			using (var context = new ClientComputerInfoDataClassesDataContext())
			{
				context.ReportClientComputerVersion(13, 123123213, 1, 1, 1, 1, null);
				context.ReportClientComputerVersion(13, 123123213, 1, 2, 3, 4, defaultAppName);
				var res = context.GetClientComputerVersions().Where(n => n.Application == null).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(1, res.Minor);
				Assert.Equal(1, res.Build);
				Assert.Equal(1, res.Revision);
				Assert.Equal(false, res.IsCurrent);
				Assert.Equal(null, res.Application);

				res = context.GetClientComputerVersions().Where(n => n.Application == defaultAppName).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(1, res.Major);
				Assert.Equal(2, res.Minor);
				Assert.Equal(3, res.Build);
				Assert.Equal(4, res.Revision);
				Assert.Equal(true, res.IsCurrent);
				Assert.Equal(defaultAppName, res.Application);
			}
		}


	}

	internal class ClientComputerVersion
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public int ComputerId { get; set; }
		public int Major { get; set; }
		public int Minor { get; set; }
		public int Build { get; set; }
		public int Revision { get; set; }
		public bool IsCurrent { get; set; }
		public string Application { get; set; }
	}

	internal static class ClientComputerVersionsHelper
	{
		internal static IEnumerable<ClientComputerVersion> GetClientComputerVersions(this ClientComputerInfoDataClassesDataContext context)
		{
			return context.ExecuteQuery<ClientComputerVersion>("SELECT * FROM [dbo].[ClientComputerVersions]");
		}
	}
}
