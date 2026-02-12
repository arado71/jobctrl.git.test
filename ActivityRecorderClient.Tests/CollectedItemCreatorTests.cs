using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Rules.Collector;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class CollectedItemCreatorTests : DisabledThreadAsserts
	{
		private static readonly DateTime now = new DateTime(2015, 05, 06, 15, 00, 00);

		private readonly CollectedItemCreator creator;
		private readonly List<CollectedItem> itemsCreated = new List<CollectedItem>();

		public CollectedItemCreatorTests()
		{
			//Arrange
			creator = new CollectedItemCreator(10, TimeSpan.FromSeconds(10));
			creator.ItemCreated += (s, e) => itemsCreated.Add(e.Value);
		}

		[Fact]
		public void CreateSimple()
		{
			//Act
			creator.UpdateCapturedValues(now, new Dictionary<string, string>() { { "A", "B" } });

			//Assert
			Assert.Equal(1, itemsCreated.Count);
			Assert.Equal(1, itemsCreated[0].CapturedValues.Count);
			Assert.Equal("A", itemsCreated[0].CapturedValues.Single().Key);
			Assert.Equal("B", itemsCreated[0].CapturedValues.Single().Value);
		}

		[Fact]
		public void CreateAndUpdateSimple()
		{
			//Arrange
			creator.UpdateCapturedValues(now, new Dictionary<string, string>() { { "A", "B" } });
			itemsCreated.Clear();

			//Act
			creator.UpdateCapturedValues(now.AddSeconds(1), new Dictionary<string, string>() { { "A", "C" } });

			//Assert
			Assert.Equal(1, itemsCreated.Count);
			Assert.Equal(1, itemsCreated[0].CapturedValues.Count);
			Assert.Equal("A", itemsCreated[0].CapturedValues.Single().Key);
			Assert.Equal("C", itemsCreated[0].CapturedValues.Single().Value);
		}

		[Fact]
		public void CreateRemove()
		{
			// Arrange
			creator.UpdateCapturedValues(now, new Dictionary<string, string>() { { "A", "B" } });
			itemsCreated.Clear();

			// Act
			creator.UpdateCapturedValues(now.AddSeconds(1), new Dictionary<string, string>());

			// Assert
			Assert.Equal(1, itemsCreated.Count);
		}

		[Fact]
		public void CreateResetRemove()
		{
			// Arrange
			creator.UpdateCapturedValues(now.AddSeconds(-1), new Dictionary<string, string>() {{"A", "B"}});
			itemsCreated.Clear();

			// Act
			creator.UpdateCapturedValues(now.Add(CollectedItemCreator.resetInterval), new Dictionary<string, string>());

			// Assert
			Assert.Equal(1, itemsCreated.Count);
		}

		[Fact]
		public void CreateNullResetRemove()
		{
			// Arrange
			creator.UpdateCapturedValues(now.AddSeconds(-1), new Dictionary<string, string>() { { "A", null } });
			itemsCreated.Clear();

			// Act
			creator.UpdateCapturedValues(now.Add(CollectedItemCreator.resetInterval), new Dictionary<string, string>());

			// Assert
			Assert.Equal(0, itemsCreated.Count);
		}

		[Fact]
		public void CreateAndCreateOther()
		{
			//Arrange
			creator.UpdateCapturedValues(now, new Dictionary<string, string>() { { "A", "B" } });
			itemsCreated.Clear();

			//Act
			creator.UpdateCapturedValues(now.AddSeconds(1), new Dictionary<string, string>() { { "A2", "B2" } });

			//Assert
			Assert.Equal(1, itemsCreated.Count);
			Assert.Equal(2, itemsCreated[0].CapturedValues.Count);
			Assert.Equal("A", itemsCreated[0].CapturedValues.Where(n => n.Key == "A").Single().Key);
			Assert.Equal(null, itemsCreated[0].CapturedValues.Where(n => n.Key == "A").Single().Value);
			Assert.Equal("A2", itemsCreated[0].CapturedValues.Where(n => n.Key != "A").Single().Key);
			Assert.Equal("B2", itemsCreated[0].CapturedValues.Where(n => n.Key != "A").Single().Value);
		}

		[Fact]
		public void ExceedThrottleLimit()
		{
			//Act
			for (int i = 0; i < 11; i++)
			{
				creator.UpdateCapturedValues(now.AddMilliseconds(i), new Dictionary<string, string>() { { "A", i.ToString() } });
			}

			//Assert
			Assert.Equal(11, itemsCreated.Count);
			Assert.Equal(1, itemsCreated.Last().CapturedValues.Count);
			Assert.Equal("A", itemsCreated.Last().CapturedValues.Single().Key);
			Assert.Equal(CollectedItemCreator.MaskedValue.LimitExceededValue, itemsCreated.Last().CapturedValues.Single().Value);

			Assert.Equal("A", itemsCreated.Skip(9).First().CapturedValues.Single().Key);
			Assert.Equal("9", itemsCreated.Skip(9).First().CapturedValues.Single().Value);
		}

		[Fact]
		public void CreateAndCreateOtherWithUpdate()
		{
			//Act
			creator.UpdateCapturedValues(now, new Dictionary<string, string>() { { "A", null } });
			creator.UpdateCapturedValues(now.AddSeconds(1), new Dictionary<string, string>() { { "A2", "B2" } });
			creator.UpdateCapturedValues(now.AddSeconds(2), new Dictionary<string, string>() { { "A", "B" }, { "A2", "B2" } });
			creator.UpdateCapturedValues(now.AddSeconds(3), new Dictionary<string, string>() { { "A", "C" }, { "A2", "C2" } });
			creator.UpdateCapturedValues(now.AddSeconds(4), new Dictionary<string, string>());

			//Assert
			Assert.Equal(5, itemsCreated.Count);

			var itemIdx = 0;
			Assert.Equal(1, itemsCreated[itemIdx].CapturedValues.Count);
			Assert.Equal("A", itemsCreated[itemIdx].CapturedValues.Single().Key);
			Assert.Equal(null, itemsCreated[itemIdx].CapturedValues.Single().Value);

			itemIdx = 1;
			Assert.Equal(1, itemsCreated[itemIdx].CapturedValues.Count);
			Assert.Equal("A2", itemsCreated[itemIdx].CapturedValues.Single().Key);
			Assert.Equal("B2", itemsCreated[itemIdx].CapturedValues.Single().Value);

			itemIdx = 2;
			Assert.Equal(1, itemsCreated[itemIdx].CapturedValues.Count);
			Assert.Equal("A", itemsCreated[itemIdx].CapturedValues.Single().Key);
			Assert.Equal("B", itemsCreated[itemIdx].CapturedValues.Single().Value);

			itemIdx = 3;
			Assert.Equal(2, itemsCreated[itemIdx].CapturedValues.Count);
			Assert.Equal("A", itemsCreated[itemIdx].CapturedValues.Where(n => n.Key == "A").Single().Key);
			Assert.Equal("C", itemsCreated[itemIdx].CapturedValues.Where(n => n.Key == "A").Single().Value);
			Assert.Equal("A2", itemsCreated[itemIdx].CapturedValues.Where(n => n.Key != "A").Single().Key);
			Assert.Equal("C2", itemsCreated[itemIdx].CapturedValues.Where(n => n.Key != "A").Single().Value);

			itemIdx = 4;
			Assert.Equal(2, itemsCreated[itemIdx].CapturedValues.Count);
			Assert.Equal("A", itemsCreated[itemIdx].CapturedValues.Where(n => n.Key == "A").Single().Key);
			Assert.Equal(null, itemsCreated[itemIdx].CapturedValues.Where(n => n.Key == "A").Single().Value);
			Assert.Equal("A2", itemsCreated[itemIdx].CapturedValues.Where(n => n.Key != "A").Single().Key);
			Assert.Equal(null, itemsCreated[itemIdx].CapturedValues.Where(n => n.Key != "A").Single().Value);
		}

		[Fact]
		public void CreateSimpleTruncatedValue()
		{
			//Act
			creator.UpdateCapturedValues(now, new Dictionary<string, string>() { { "A", new string('B', CollectedItemCreator.MaxValueLength + 1) } });

			//Assert
			Assert.Equal(1, itemsCreated.Count);
			Assert.Equal(1, itemsCreated[0].CapturedValues.Count);
			Assert.Equal("A", itemsCreated[0].CapturedValues.Single().Key);
			Assert.Equal(new string('B', CollectedItemCreator.MaxValueLength), itemsCreated[0].CapturedValues.Single().Value);
		}
	}
}
