using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Collector;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class WorkItemTests : DbTestsBase
	{
		private DateTime now = new DateTime(2011, 09, 22, 17, 00, 00);

		[Fact]
		public void InsertUnicodeValues()
		{
			//Arrange
			const string processName = "bela\u3000exe";
			const string title = "BELA\u3000bela";
			const string url = "about\u3000blank";
			CollectedItemDbHelper.Insert(new CollectedItem()
			{
				UserId = 1,
				CreateDate = now.AddHours(1),
				CapturedValues =
					new Dictionary<string, string> { { "ProcessName", processName }, { "Title", title }, { "Url", url } },
			});

			//Act
			var inserted = StatsDbHelper.GetCollectedItemsForUser(1, now, now.AddHours(2)).ToDictionary(i => i.Key, i => i.Value);

			//Assert
			Assert.Equal(processName, inserted["ProcessName"]);
			Assert.Equal(title, inserted["Title"]);
			Assert.Equal(url, inserted["Url"]);
		}

		[Fact]
		public void LinqBinaryTests()
		{
			byte[] data = null;
			System.Data.Linq.Binary bin = data;
			Assert.Equal(new System.Data.Linq.Binary(null), bin);
			Assert.Equal(0, new System.Data.Linq.Binary(null).Length);
		}

		[Fact]
		public void LinqBinaryConvetTests()
		{
			var e = 3L;
			Assert.Equal(e, e.ToBinary().ToLong());
			//Assert.True(BitConverter.IsLittleEndian);
		}

	}
}
