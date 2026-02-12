using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class ReportAddressTests : DbTestsBase
	{
		string localsV1 = "1.0.0.0,2.0.0.0,3.0.0.0";
		string localsV2 = "1.0.0.1,2.0.0.1,3.0.0.1";

		[Fact]
		public void InsertFirstAddress()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", localsV1);
				var res = context.GetClientComputerAddresses().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);
				Assert.NotNull(res.ClientComputerLocalAddressId);
				context.CheckLocals(res.ClientComputerLocalAddressId, localsV1);
			}
		}

		[Fact]
		public void UpdateSameAddress()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", localsV1);
				var res = context.GetClientComputerAddresses().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);
				Assert.NotNull(res.ClientComputerLocalAddressId);
				context.CheckLocals(res.ClientComputerLocalAddressId, localsV1);
			}
		}

		[Fact]
		public void UpdateSameAddressDifferentLocals()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", localsV2);
				var res = context.GetClientComputerAddresses().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);
				Assert.NotNull(res.ClientComputerLocalAddressId);
				context.CheckLocals(res.ClientComputerLocalAddressId, localsV2);
			}
		}

		[Fact]
		public void UpdateAddress()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", localsV1);
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.2", localsV2);
				var resList = context.GetClientComputerAddresses().ToList();

				var res = resList.Where(n => n.IsCurrent).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.2", res.Address);
				Assert.Equal(true, res.IsCurrent);

				res = resList.Where(n => !n.IsCurrent).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(false, res.IsCurrent);
			}
		}
		[Fact]
		public void UpdateLocalsThenBack()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", localsV1);
				var res = context.GetClientComputerAddresses().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);
				Assert.NotNull(res.ClientComputerLocalAddressId);
				var rel = context.GetClientComputerLocalAddresses(res.ClientComputerLocalAddressId.Value).Single();
				Assert.Equal(rel.AddressList, localsV1);
				var relId = rel.Id;

				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", localsV2);
				res = context.GetClientComputerAddresses().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);

				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", localsV1);
				res = context.GetClientComputerAddresses().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);
				Assert.NotNull(res.ClientComputerLocalAddressId);
				rel = context.GetClientComputerLocalAddresses(res.ClientComputerLocalAddressId.Value).Single();
				Assert.Equal(true, rel.Id == relId);
			}
		}

		[Fact]
		public void InsertDifferentCompSameUserAddresss()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", localsV2);
				context.ReportClientComputerAddress(13, 456456452, "127.0.0.1", localsV2);
				var res = context.GetClientComputerAddresses().Where(n => n.ComputerId == 123123213).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);
				context.CheckLocals(res.ClientComputerLocalAddressId, localsV2);

				res = context.GetClientComputerAddresses().Where(n => n.ComputerId == 456456452).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(456456452, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);
				context.CheckLocals(res.ClientComputerLocalAddressId, localsV2);
			}
		}

		[Fact]
		public void InsertDifferentUserSameCompAddresss()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", localsV1);
				context.ReportClientComputerAddress(3453, 123123213, "127.0.0.1", localsV2);
				var res = context.GetClientComputerAddresses().Where(n => n.UserId == 13).Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);

				res = context.GetClientComputerAddresses().Where(n => n.UserId == 3453).Single();
				Assert.Equal(3453, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);
			}
		}

		[Fact]
		public void UpdateAddressToNewerThenBackToOriginal()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", "1.0.0.1,2.0.0.1,3.0.0.1");
				context.ReportClientComputerAddress(13, 123123213, "127.0.100.1", "1.0.0.1,2.0.0.1,3.0.0.1");
				context.ReportClientComputerAddress(13, 123123213, "127.0.0.1", "1.0.0.1,2.0.0.1,3.0.0.1");
				var res = context.GetClientComputerAddresses().OrderBy(n => n.Id).First();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(false, res.IsCurrent);

				res = context.GetClientComputerAddresses().OrderBy(n => n.Id).ElementAt(1);
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.100.1", res.Address);
				Assert.Equal(false, res.IsCurrent);

				res = context.GetClientComputerAddresses().OrderBy(n => n.Id).Last();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.0.1", res.Address);
				Assert.Equal(true, res.IsCurrent);
			}
		}

		[Fact]
		public void UpdateAddressToNewerThenBackToOriginalWithNull()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, null, null);
				context.ReportClientComputerAddress(13, 123123213, "127.0.100.1", null);
				context.ReportClientComputerAddress(13, 123123213, null, null);
				var res = context.GetClientComputerAddresses().OrderBy(n => n.Id).First();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(null, res.Address);
				Assert.Equal(false, res.IsCurrent);

				res = context.GetClientComputerAddresses().OrderBy(n => n.Id).ElementAt(1);
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal("127.0.100.1", res.Address);
				Assert.Equal(false, res.IsCurrent);

				res = context.GetClientComputerAddresses().OrderBy(n => n.Id).Last();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(null, res.Address);
				Assert.Equal(true, res.IsCurrent);
			}
		}

		[Fact]
		public void UpdateSameAddressWithNull()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.ReportClientComputerAddress(13, 123123213, null, null);
				context.ReportClientComputerAddress(13, 123123213, null, null);
				var res = context.GetClientComputerAddresses().Single();
				Assert.Equal(13, res.UserId);
				Assert.Equal(123123213, res.ComputerId);
				Assert.Equal(null, res.Address);
				Assert.Equal(true, res.IsCurrent);
			}
		}

		[Fact(Skip = "this will fail atm. if you've got enough horse power")]
		public void NoDeadlock()
		{
			//--SQL
			//declare @a varchar(39)
			//WAITFOR TIME '12:48'
			//WHILE 1=1
			//BEGIN
			//    --SET @a = (SELECT CAST(NEWID() AS VARCHAR(39)))
			//    SET @a = (SELECT SUBSTRING(CAST(NEWID() AS VARCHAR(39)),0,2))
			//    exec [dbo].[ReportClientComputerAddress] 13,35345,@a
			//END
			System.Threading.ThreadPool.SetMinThreads(100, 100);
			var task = Task.Factory.StartNew(() =>
				{
					for (int i = 0; i < 400; i++)
					{
						Task.Factory.StartNew(() =>
							{
								using (var context = new ActivityRecorderDataClassesDataContext())
								{
									//context.ReportClientComputerAddress(13, 34, GetRandomInt().ToString());
									//minimize linq overhead
									context.ExecuteCommand("exec [dbo].[ReportClientComputerAddress] 13,35345,'" + GetRandomInt() + "'");
								}
							}, TaskCreationOptions.AttachedToParent);
					}
				});

			task.Wait();
		}

		private static readonly RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
		private static int GetRandomInt()
		{
			byte[] bytes = new byte[4];
			rnd.GetBytes(bytes);
			return BitConverter.ToInt32(bytes, 0);
		}
	}

	internal class ClientComputerAddress
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public int ComputerId { get; set; }
		public string Address { get; set; }
		public bool IsCurrent { get; set; }
		public DateTime FirstReceiveDate { get; set; }
		public DateTime LastReceiveDate { get; set; }
		public int? ClientComputerLocalAddressId { get; set; }
	}

	internal class ClientComputerLocalAddresses
	{
		public int Id { get; set; }
		public string AddressList { get; set; }
	}

	internal static class ClientComputerAddressHelper
	{
		internal static IEnumerable<ClientComputerAddress> GetClientComputerAddresses(this ActivityRecorderDataClassesDataContext context)
		{
			return context.ExecuteQuery<ClientComputerAddress>("SELECT * FROM [dbo].[ClientComputerAddresses]");
		}
		internal static IEnumerable<ClientComputerLocalAddresses> GetClientComputerLocalAddresses(this ActivityRecorderDataClassesDataContext context, int cId)
		{
			return context.ExecuteQuery<ClientComputerLocalAddresses>("SELECT * FROM [dbo].[ClientComputerLocalAddresses] WHERE Id=" + cId);
	}
}
	internal static class Extension
	{
		public static void CheckLocals(this ActivityRecorderDataClassesDataContext context, int? clId, string locals)
		{
			Assert.NotNull(clId);
			var rel = context.GetClientComputerLocalAddresses(clId.Value).Single();
			Assert.Equal(rel.AddressList, locals);
		}
	}
}
