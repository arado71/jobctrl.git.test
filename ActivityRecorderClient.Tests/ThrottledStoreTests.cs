using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Rules.Collector;
using Tct.ActivityRecorderClient.View;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class ThrottledStoreTests
	{
		private static readonly DateTime now = new DateTime(2015, 05, 06, 11, 00, 00);

		[Fact]
		public void CanSetWithinLimit()
		{
			//Arrange
			var store = new ThrottledStore<string>(10, TimeSpan.FromSeconds(10));

			//Act
			var vals = new List<ThrottledValue<string>>();
			for (int i = 0; i < 10; i++)
			{
				vals.Add(store.Set(i.ToString(), now.AddSeconds(i)));
			}

			//Assert
			Assert.True(vals.All(n => !n.IsThrottleLimitExceeded));
			Assert.True(vals.Select(n => n.Value).SequenceEqual(Enumerable.Range(0, 10).Select(n => n.ToString())));
		}

		[Fact]
		public void CanSetWithinLimitNewWindow()
		{
			//Arrange
			var store = new ThrottledStore<string>(10, TimeSpan.FromSeconds(10));

			//Act
			var vals = new List<ThrottledValue<string>>();
			for (int i = 0; i < 99; i++)
			{
				vals.Add(store.Set(i.ToString(), now.AddSeconds(i)));
			}

			//Assert
			Assert.True(vals.All(n => !n.IsThrottleLimitExceeded));
			Assert.True(vals.Select(n => n.Value).SequenceEqual(Enumerable.Range(0, 99).Select(n => n.ToString())));
		}

		[Fact]
		public void CanSetWithinLimitNewWindowSameValue()
		{
			//Arrange
			var store = new ThrottledStore<string>(10, TimeSpan.FromSeconds(10));

			//Act
			var vals = new List<ThrottledValue<string>>();
			for (int i = 0; i < 99; i++)
			{
				vals.Add(store.Set(i.ToString(), now.AddSeconds(i)));
				vals.Add(store.Set(i.ToString(), now.AddSeconds(i).AddMilliseconds(1)));
			}

			//Assert
			Assert.True(vals.All(n => !n.IsThrottleLimitExceeded));
			Assert.True(vals.Select(n => n.Value).SequenceEqual(Enumerable.Range(0, 99 * 2).Select(n => (n / 2).ToString())));
		}

		[Fact]
		public void CanSetButThrottled()
		{
			//Arrange
			var store = new ThrottledStore<string>(10, TimeSpan.FromSeconds(10));

			//Act
			var vals = new List<ThrottledValue<string>>();
			for (int i = 0; i < 11; i++)
			{
				vals.Add(store.Set(i.ToString(), now.AddMilliseconds(i)));
			}

			//Assert
			Assert.Equal(11, vals.Count());
			Assert.True(vals.Last().IsThrottleLimitExceeded);
			Assert.Equal("10", vals.Last().Value);

			Assert.True(vals.Take(10).All(n => !n.IsThrottleLimitExceeded));
			Assert.True(vals.Take(10).Select(n => n.Value).SequenceEqual(Enumerable.Range(0, 10).Select(n => n.ToString())));
		}

		[Fact]
		public void AfterThrottledValueIsChangedForGet()
		{
			var store = new ThrottledStore<string>(10, TimeSpan.FromSeconds(10));

			var vals = new List<ThrottledValue<string>>();
			for (int i = 0; i < 11; i++)
			{
				vals.Add(store.Set(i.ToString(), now.AddMilliseconds(i)));
			}

			Assert.Equal(11, vals.Count());
			Assert.True(vals.Last().IsThrottleLimitExceeded);
			Assert.Equal("10", vals.Last().Value);

			var tval = store.Get(now.AddSeconds(10).AddMilliseconds(-1));
			Assert.True(tval.IsThrottleLimitExceeded);
			Assert.Equal("10", tval.Value);

			tval = store.Get(now.AddSeconds(10));
			Assert.False(tval.IsThrottleLimitExceeded);
			Assert.Equal("10", tval.Value);
		}

		[Fact]
		public void AfterThrottledValueIsChangedForSet()
		{
			var store = new ThrottledStore<string>(10, TimeSpan.FromSeconds(10));

			var vals = new List<ThrottledValue<string>>();
			for (int i = 0; i < 11; i++)
			{
				vals.Add(store.Set(i.ToString(), now.AddMilliseconds(i)));
			}

			Assert.Equal(11, vals.Count());
			Assert.True(vals.Last().IsThrottleLimitExceeded);
			Assert.Equal("10", vals.Last().Value);

			var tval = store.Set("11", now.AddSeconds(5));
			Assert.True(tval.IsThrottleLimitExceeded);
			Assert.Equal("11", tval.Value);

			tval = store.Get(now.AddSeconds(10).AddMilliseconds(-1));
			Assert.True(tval.IsThrottleLimitExceeded);
			Assert.Equal("11", tval.Value);

			tval = store.Get(now.AddSeconds(10));
			Assert.False(tval.IsThrottleLimitExceeded);
			Assert.Equal("11", tval.Value);
		}

		[Fact]
		public void AfterThrottledValueIsChangedForGetAndThatIsASet()
		{
			var store = new ThrottledStore<string>(10, TimeSpan.FromSeconds(10));

			var vals = new List<ThrottledValue<string>>();
			for (int i = 0; i < 11; i++)
			{
				vals.Add(store.Set(i.ToString(), now.AddMilliseconds(i)));
			}

			Assert.Equal(11, vals.Count());
			Assert.True(vals.Last().IsThrottleLimitExceeded);
			Assert.Equal("10", vals.Last().Value);

			var tval = store.Get(now.AddSeconds(10).AddMilliseconds(-1));
			Assert.True(tval.IsThrottleLimitExceeded);
			Assert.Equal("10", tval.Value);

			tval = store.Get(now.AddSeconds(10));
			Assert.False(tval.IsThrottleLimitExceeded);
			Assert.Equal("10", tval.Value);

			vals.Clear();
			for (int i = 0; i < 10; i++)
			{
				vals.Add(store.Set(i.ToString(), now.AddSeconds(10).AddMilliseconds(1 + i)));
			}
			Assert.True(vals.Take(9).All(n => !n.IsThrottleLimitExceeded));
			Assert.True(vals.Last().IsThrottleLimitExceeded);
		}

		[Fact]
		public void InvalidTimeIsIgnored()
		{
			//Arrange
			var store = new ThrottledStore<string>(10, TimeSpan.FromSeconds(10));
			store.Set("1", now);

			//Act
			var tval = store.Set("2", now.AddSeconds(-1));

			//Assert
			Assert.False(tval.IsThrottleLimitExceeded);
			Assert.Equal("1", tval.Value);
		}

		[Fact]
		public void CanSetWithinLimitNewWindowWithInsensitiveComparer()
		{
			//Arrange
			var store = new ThrottledStore<string>(10, TimeSpan.FromSeconds(10), StringComparer.OrdinalIgnoreCase);

			//Act
			var vals = new List<ThrottledValue<string>>();
			for (int i = 0; i < 24; i++)
			{
				var tval1 = store.Set(((char)('A' + i)).ToString(), now.AddSeconds(i));
				vals.Add(tval1);
				var tval2 = store.Set(((char)('a' + i)).ToString(), now.AddSeconds(i).AddMilliseconds(1));
				vals.Add(tval2);

				Assert.Equal(tval1.Value, tval2.Value);
			}

			//Assert
			Assert.True(vals.All(n => !n.IsThrottleLimitExceeded));
		}
	}
}
