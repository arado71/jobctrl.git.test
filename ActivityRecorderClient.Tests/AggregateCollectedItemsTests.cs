using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class AggregateCollectedItemsTests
	{
		private static readonly DateTime now = new DateTime(2015, 06, 05, 12, 00, 00);

		private static AggregateCollectedItems GetEmpty()
		{
			return new AggregateCollectedItems()
			{
				KeyLookup = new Dictionary<int, string>(),
				ValueLookup = new Dictionary<int, string>(),
				Items = new List<CollectedItemIdOnly>(),
			};
		}

		[Fact]
		public void AddOneItemOneCapturedValue()
		{
			//Arrange
			var aggr = GetEmpty();

			//Act
			aggr.Add(new CollectedItem()
			{
				CreateDate = now,
				CapturedValues = new Dictionary<string, string>() { 
					{"A", "B"},
				}
			});

			//Assert
			Assert.Equal(1, aggr.Items.Count);
			Assert.Equal(1, aggr.KeyLookup.Count);
			Assert.Equal(1, aggr.ValueLookup.Count);
			Assert.Equal(1, aggr.Items[0].CapturedValues.Count);
			Assert.Equal(now, aggr.Items[0].CreateDate);
			Assert.Equal("A", aggr.KeyLookup[aggr.Items[0].CapturedValues.First().Key]);
			Assert.Equal("B", aggr.ValueLookup[aggr.Items[0].CapturedValues.First().Value.Value]);
		}

		[Fact]
		public void AddOneItemTwoCapturedValues()
		{
			//Arrange
			var aggr = GetEmpty();

			//Act
			aggr.Add(new CollectedItem()
			{
				CreateDate = now,
				CapturedValues = new Dictionary<string, string>() { 
					{"A", "B"},
					{"A2", "B2"},
				}
			});

			//Assert
			Assert.Equal(1, aggr.Items.Count);
			Assert.Equal(2, aggr.KeyLookup.Count);
			Assert.Equal(2, aggr.ValueLookup.Count);
			Assert.Equal(2, aggr.Items[0].CapturedValues.Count);
			Assert.Equal(now, aggr.Items[0].CreateDate);
			Assert.True(new[] { "A", "A2" }.SequenceEqual(aggr.Items[0].CapturedValues.Select(n => aggr.KeyLookup[n.Key]).OrderBy(n => n)));
			Assert.True(new[] { "B", "B2" }.SequenceEqual(aggr.Items[0].CapturedValues.Select(n => aggr.ValueLookup[n.Value.Value]).OrderBy(n => n)));
		}

		[Fact]
		public void AddSeveralItemOneCapturedValue()
		{
			//Arrange
			var aggr = GetEmpty();

			//Act
			aggr.Add(new CollectedItem()
			{
				CreateDate = now,
				CapturedValues = new Dictionary<string, string>() { 
					{"A", "B"},
				}
			});
			aggr.Add(new CollectedItem()
			{
				CreateDate = now.AddMinutes(1),
				CapturedValues = new Dictionary<string, string>() { 
					{"A", ""},
				}
			});
			aggr.Add(new CollectedItem()
			{
				CreateDate = now.AddMinutes(2),
				CapturedValues = new Dictionary<string, string>() { 
					{"A", "B"},
				}
			});

			//Assert
			Assert.Equal(3, aggr.Items.Count);
			Assert.Equal(1, aggr.KeyLookup.Count);
			Assert.Equal(2, aggr.ValueLookup.Count);
			Assert.True(new[] { 1, 1, 1 }.SequenceEqual(aggr.Items.Select(n => n.CapturedValues.Count)));
			Assert.True(new[] { now, now.AddMinutes(1), now.AddMinutes(2) }.SequenceEqual(aggr.Items.Select(n => n.CreateDate)));
			Assert.True(new[] { "A", "A", "A" }.SequenceEqual(aggr.Items.SelectMany(m => m.CapturedValues.Select(n => aggr.KeyLookup[n.Key]))));
			Assert.True(new[] { "B", "", "B" }.SequenceEqual(aggr.Items.SelectMany(m => m.CapturedValues.Select(n => aggr.ValueLookup[n.Value.Value]))));
		}

		[Fact]
		public void AddSeveralItemOneCapturedValueCaseInsensitive()
		{
			//Arrange
			var aggr = GetEmpty();

			//Act
			aggr.Add(new CollectedItem()
			{
				CreateDate = now,
				CapturedValues = new Dictionary<string, string>() { 
					{"A", "B"},
				}
			});
			aggr.Add(new CollectedItem()
			{
				CreateDate = now.AddMinutes(1),
				CapturedValues = new Dictionary<string, string>() { 
					{"a", ""},
				}
			});
			aggr.Add(new CollectedItem()
			{
				CreateDate = now.AddMinutes(2),
				CapturedValues = new Dictionary<string, string>() { 
					{"a", "b"},
				}
			});

			//Assert
			Assert.Equal(3, aggr.Items.Count);
			Assert.Equal(1, aggr.KeyLookup.Count);
			Assert.Equal(2, aggr.ValueLookup.Count);
			Assert.True(new[] { 1, 1, 1 }.SequenceEqual(aggr.Items.Select(n => n.CapturedValues.Count)));
			Assert.True(new[] { now, now.AddMinutes(1), now.AddMinutes(2) }.SequenceEqual(aggr.Items.Select(n => n.CreateDate)));
			Assert.True(new[] { "A", "A", "A" }.SequenceEqual(aggr.Items.SelectMany(m => m.CapturedValues.Select(n => aggr.KeyLookup[n.Key]))));
			Assert.True(new[] { "B", "", "B" }.SequenceEqual(aggr.Items.SelectMany(m => m.CapturedValues.Select(n => aggr.ValueLookup[n.Value.Value]))));
		}

		[Fact]
		public void AddSeveralItemSeveralCapturedValuesCaseInsensitive()
		{
			//Arrange
			var aggr = GetEmpty();

			//Act
			aggr.Add(new CollectedItem()
			{
				CreateDate = now,
				CapturedValues = new Dictionary<string, string>() { 
					{"A", "B"},
					{"A2", "B2"},
				}
			});
			aggr.Add(new CollectedItem()
			{
				CreateDate = now.AddMinutes(1),
				CapturedValues = new Dictionary<string, string>() { 
					{"A2", null},
				}
			});
			aggr.Add(new CollectedItem()
			{
				CreateDate = now.AddMinutes(2),
				CapturedValues = new Dictionary<string, string>() { 
					{"a", "C"},
					{"A2", "b2"},
				}
			});

			//Assert
			Assert.Equal(3, aggr.Items.Count);
			Assert.Equal(2, aggr.KeyLookup.Count);
			Assert.Equal(3, aggr.ValueLookup.Count);
			Assert.True(new[] { 2, 1, 2 }.SequenceEqual(aggr.Items.Select(n => n.CapturedValues.Count)));
			Assert.True(new[] { now, now.AddMinutes(1), now.AddMinutes(2) }.SequenceEqual(aggr.Items.Select(n => n.CreateDate)));
			Assert.True(new[] { "A", "A2", "A2", "A", "A2" }.SequenceEqual(aggr.Items.SelectMany(m => m.CapturedValues.Select(n => aggr.KeyLookup[n.Key]).OrderBy(n => n))));
			Assert.True(new[] { "B", "B2", null, "C", "B2" }.SequenceEqual(aggr.Items.SelectMany(m => m.CapturedValues.OrderBy(n => aggr.KeyLookup[n.Key]).Select(n => n.Value.HasValue ? aggr.ValueLookup[n.Value.Value] : null))));
		}
	}
}
