using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.Kicks;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class KickTests : DbTestsBase
	{
		private static readonly DateTime now = new DateTime(2011, 10, 19, 12, 00, 00);

		[Fact]
		public void CreateKick()
		{
			var newKick = KickDbHelper.CreateKick(13, 123123, "valami", TimeSpan.FromSeconds(2), 1);
			Assert.Equal(13, newKick.UserId);
			Assert.Equal(123123, newKick.ComputerId);
			Assert.Equal("valami", newKick.Reason);
			Assert.Equal(1, newKick.CreatedBy);
			Assert.Equal(TimeSpan.FromSeconds(2), newKick.ExpirationDate - newKick.CreateDate);
			Assert.Equal(new KickResult?(), newKick.Result);
			Assert.Equal(null, newKick.ConfirmDate);
			Assert.Equal(null, newKick.SendDate);
			using (var context = new KickDataClassesDataContext())
			{
				var dbKick = context.ClientComputerKicks.Single();
				Assert.Equal(13, dbKick.UserId);
				Assert.Equal(123123, dbKick.ComputerId);
				Assert.Equal("valami", dbKick.Reason);
				Assert.Equal(1, dbKick.CreatedBy);
				Assert.Equal(TimeSpan.FromSeconds(2), dbKick.ExpirationDate - dbKick.CreateDate);
				Assert.Equal(new KickResult?(), dbKick.Result);
				Assert.Equal(null, dbKick.ConfirmDate);
				Assert.Equal(null, dbKick.SendDate);

				Assert.Equal(dbKick.Id, newKick.Id);
			}
		}

		[Fact]
		public void ConfirmKick()
		{
			var newKick = KickDbHelper.CreateKick(13, 123123, "valami", TimeSpan.FromSeconds(2), 1);
			int numRows = KickDbHelper.ConfirmKick(newKick.Id, 13, 123123, now, KickResult.Ok);
			Assert.Equal(1, numRows);

			using (var context = new KickDataClassesDataContext())
			{
				var dbKick = context.ClientComputerKicks.Single();
				Assert.Equal(13, dbKick.UserId);
				Assert.Equal(123123, dbKick.ComputerId);
				Assert.Equal("valami", dbKick.Reason);
				Assert.Equal(1, dbKick.CreatedBy);
				Assert.Equal(TimeSpan.FromSeconds(2), dbKick.ExpirationDate - dbKick.CreateDate);
				Assert.Equal(KickResult.Ok, dbKick.Result);
				Assert.Equal(now, dbKick.ConfirmDate);
				Assert.Equal(null, dbKick.SendDate);

				Assert.Equal(dbKick.Id, newKick.Id);
			}
		}

		[Fact]
		public void ConfirmKickCannotOverwrite()
		{
			var newKick = KickDbHelper.CreateKick(13, 123123, "valami", TimeSpan.FromSeconds(2), 1);
			int numRows = KickDbHelper.ConfirmKick(newKick.Id, 13, 123123, now, KickResult.Ok);
			Assert.Equal(1, numRows);
			numRows = KickDbHelper.ConfirmKick(newKick.Id, 13, 123123, now.AddDays(1), KickResult.AlreadyOffline);
			Assert.Equal(0, numRows);

			using (var context = new KickDataClassesDataContext())
			{
				var dbKick = context.ClientComputerKicks.Single();
				Assert.Equal(13, dbKick.UserId);
				Assert.Equal(123123, dbKick.ComputerId);
				Assert.Equal("valami", dbKick.Reason);
				Assert.Equal(1, dbKick.CreatedBy);
				Assert.Equal(TimeSpan.FromSeconds(2), dbKick.ExpirationDate - dbKick.CreateDate);
				Assert.Equal(KickResult.Ok, dbKick.Result);
				Assert.Equal(now, dbKick.ConfirmDate);
				Assert.Equal(null, dbKick.SendDate);

				Assert.Equal(dbKick.Id, newKick.Id);
			}
		}

		[Fact]
		public void SendKick()
		{
			var newKick = KickDbHelper.CreateKick(13, 123123, "valami", TimeSpan.FromSeconds(2), 1);
			int numRows = KickDbHelper.SendKick(newKick.Id, 13, 123123, now);
			Assert.Equal(1, numRows);

			using (var context = new KickDataClassesDataContext())
			{
				var dbKick = context.ClientComputerKicks.Single();
				Assert.Equal(13, dbKick.UserId);
				Assert.Equal(123123, dbKick.ComputerId);
				Assert.Equal("valami", dbKick.Reason);
				Assert.Equal(1, dbKick.CreatedBy);
				Assert.Equal(TimeSpan.FromSeconds(2), dbKick.ExpirationDate - dbKick.CreateDate);
				Assert.Equal(new KickResult?(), dbKick.Result);
				Assert.Equal(null, dbKick.ConfirmDate);
				Assert.Equal(now, dbKick.SendDate);

				Assert.Equal(dbKick.Id, newKick.Id);
			}
		}

		[Fact]
		public void SendKickCannotOverwrite()
		{
			var newKick = KickDbHelper.CreateKick(13, 123123, "valami", TimeSpan.FromSeconds(2), 1);
			int numRows = KickDbHelper.SendKick(newKick.Id, 13, 123123, now);
			Assert.Equal(1, numRows);
			numRows = KickDbHelper.SendKick(newKick.Id, 13, 123123, now.AddDays(1));
			Assert.Equal(0, numRows);

			using (var context = new KickDataClassesDataContext())
			{
				var dbKick = context.ClientComputerKicks.Single();
				Assert.Equal(13, dbKick.UserId);
				Assert.Equal(123123, dbKick.ComputerId);
				Assert.Equal("valami", dbKick.Reason);
				Assert.Equal(1, dbKick.CreatedBy);
				Assert.Equal(TimeSpan.FromSeconds(2), dbKick.ExpirationDate - dbKick.CreateDate);
				Assert.Equal(new KickResult?(), dbKick.Result);
				Assert.Equal(null, dbKick.ConfirmDate);
				Assert.Equal(now, dbKick.SendDate);

				Assert.Equal(dbKick.Id, newKick.Id);
			}
		}


	}
}
