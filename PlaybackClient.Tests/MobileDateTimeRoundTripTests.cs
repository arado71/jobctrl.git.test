using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PlaybackClient.Tests
{
	public class MobileDateTimeRoundTripTests
	{
		private static readonly DateTime now = new DateTime(2013, 10, 04, 11, 35, 00, DateTimeKind.Unspecified);
		private static readonly DateTime nowLocal = new DateTime(2013, 10, 04, 11, 35, 00, DateTimeKind.Local);
		private static readonly DateTime nowUtc = new DateTime(2013, 10, 04, 11, 35, 00, DateTimeKind.Utc);

		[Fact]
		public void WorkItemStart()
		{
			var item = new MobileServiceReference.WorkItem();
			item.StartDateTyped = now;
			Assert.Equal(now, item.StartDateTyped);

			item.StartDateTyped = nowLocal;
			Assert.Equal(nowLocal, item.StartDateTyped);

			item.StartDateTyped = nowUtc;
			Assert.Equal(nowUtc, item.StartDateTyped);
		}

		[Fact]
		public void WorkItemEnd()
		{
			var item = new MobileServiceReference.WorkItem();
			item.EndDateTyped = now;
			Assert.Equal(now, item.EndDateTyped);

			item.EndDateTyped = nowLocal;
			Assert.Equal(nowLocal, item.EndDateTyped);

			item.EndDateTyped = nowUtc;
			Assert.Equal(nowUtc, item.EndDateTyped);
		}

		[Fact]
		public void LocationDate()
		{
			var item = new MobileServiceReference.LocationInfo();
			item.DateTyped = now;
			Assert.Equal(now, item.DateTyped);

			item.DateTyped = nowLocal;
			Assert.Equal(nowLocal, item.DateTyped);

			item.DateTyped = nowUtc;
			Assert.Equal(nowUtc, item.DateTyped);
		}

	}
}
