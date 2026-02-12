using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Collector;
using Dapper;
using Tct.ActivityRecorderService.Persistence;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class CollectedItemNoDbTests
	{
		[Fact]
		public void GetTestString()
		{
			var newRules = new CollectorRules()
			{
				Rules = new List<CollectorRule>()
				{
					new CollectorRule()
					{
						IsEnabled = true,
						IsRegex = true,
						ProcessRule = "(?<ProcessName>.*)",
						TitleRule = "(?<Title>.*)",
						UrlRule = "^(?<Url>[^?#@]*)",
						ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
						{
							{ "JobCTRL.Mail", new Dictionary<string, string>() { 
								{ "FromEmail", "(?<MailFrom>.*)" },
								{ "RecipientsEmail", "(?<MailTo>.*)" },
								{ "Id", "(?<MailId>.*)" },
							}},
							{ "JobCTRL.Office", new Dictionary<string, string>() { 
								{ "DocumentPath", "(?<DocumentPath>.*)" },
								{ "DocumentFileName", "(?<DocumentFileName>.*)" },
							}},
						},
						CapturedKeys = new List<string>() { "ProcessName", "Title", "Url", "MailFrom" , "MailTo" , "MailId", "DocumentPath", "DocumentFileName" },
					}
				}
			};

			string rulesData;

			using (var stream = new MemoryStream())
			{
				XmlPersistenceHelper.WriteToStream(stream, newRules);
				rulesData = Encoding.UTF8.GetString(stream.ToArray());
			}

			Console.WriteLine(rulesData);
		}

	}

	public class CollectedItemTests : DbTestsBase
	{
		private static readonly DateTime now = new DateTime(2015, 05, 04, 12, 00, 00);
		private const int userId = 13;
		private const int computerId = 234;

		private static CollectedItem GetEmptyCollectedItem()
		{
			return new CollectedItem()
			{
				UserId = userId,
				ComputerId = computerId,
				CreateDate = now,
				CapturedValues = new Dictionary<string, string>(),
			};
		}

		private static AggregateCollectedItems GetEmptyAggrItem()
		{
			return new AggregateCollectedItems()
			{
				UserId = userId,
				ComputerId = computerId,
				KeyLookup = new Dictionary<int, string>(),
				ValueLookup = new Dictionary<int, string>(),
				Items = new List<CollectedItemIdOnly>()
				{
					new CollectedItemIdOnly()
					{
						CreateDate = now,
						CapturedValues = new Dictionary<int, int?>(),
					}
				},
			};
		}

		private static AggregateCollectedItems ConvertToAggr(CollectedItem item)
		{
			var res = GetEmptyAggrItem();
			AddCollectedItem(res, item);
			return res;
		}

		private static AggregateCollectedItems ConvertToAggr(IEnumerable<CollectedItem> items)
		{
			var res = GetEmptyAggrItem();
			foreach (var item in items)
			{
				AddCollectedItem(res, item);
			}
			return res;
		}

		private static void AddCollectedItem(AggregateCollectedItems target, CollectedItem item)
		{
			var res = new CollectedItemIdOnly()
			{
				CreateDate = item.CreateDate,
				CapturedValues = new Dictionary<int, int?>(item.CapturedValues.Count)
			};
			foreach (var capturedValue in item.CapturedValues)
			{
				var keyId = target.KeyLookup.Where(n => n.Value == capturedValue.Key).Select(n => new int?(n.Key)).FirstOrDefault();
				if (keyId == null)
				{
					keyId = target.KeyLookup.Select(n => n.Key).DefaultIfEmpty(0).Max() + 1;
					target.KeyLookup.Add(keyId.Value, capturedValue.Key);
				}

				var valueId = capturedValue.Value == null ? new int?() : target.ValueLookup.Where(n => n.Value == capturedValue.Value).Select(n => new int?(n.Key)).FirstOrDefault();
				if (valueId == null && capturedValue.Value != null)
				{
					valueId = target.ValueLookup.Select(n => n.Key).DefaultIfEmpty(0).Max() + 1;
					target.ValueLookup.Add(valueId.Value, capturedValue.Value);
				}

				res.CapturedValues.Add(keyId.Value, valueId);
			}
			target.Items.Add(res);
		}

		private static List<CollectedDbItem> GetDbData()
		{
			using (var conn = new SqlConnection(Tct.ActivityRecorderService.Properties.Settings.Default.recorderConnectionString))
			{
				conn.Open();
				return conn.Query<CollectedDbItem>("SELECT c.UserId, c.ComputerId, c.CreateDate, c.KeyId, c.ValueId, k.[Key], v.Value " +
											"FROM CollectedItems AS c " +
											"JOIN CollectedKeyLookup AS k ON k.Id = c.KeyId " +
											"LEFT JOIN CollectedValueLookup AS v ON v.Id = c.ValueId").ToList();
			}
		}

		#region Simple Inserts
		[Fact]
		public void CanAddOneItem()
		{
			//Arrange
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");
			var cache = new CollectedLookupIdCache(10, 10);

			//Act
			CollectedItemDbHelper.Insert(item, cache);

			//Assert
			var data = GetDbData().Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A", data.Key);
			Assert.Equal("B", data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B"), data.ValueId);
		}

		[Fact]
		public void CanAddOneItemWithNullValue()
		{
			//Arrange
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", null);
			var cache = new CollectedLookupIdCache(10, 10);

			//Act
			CollectedItemDbHelper.Insert(item, cache);

			//Assert
			var data = GetDbData().Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A", data.Key);
			Assert.Equal(null, data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(null, data.ValueId);
		}

		[Fact]
		public void CanAddOneItemWithMultipleCaptures()
		{
			//Arrange
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");
			item.CapturedValues.Add("A2", "B2");
			var cache = new CollectedLookupIdCache(10, 10);

			//Act
			CollectedItemDbHelper.Insert(item, cache);

			//Assert
			var allData = GetDbData();
			Assert.Equal(2, allData.Count);
			var data = allData.Where(n => n.Key == "A").Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A", data.Key);
			Assert.Equal("B", data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B"), data.ValueId);

			data = allData.Where(n => n.Key != "A").Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A2", data.Key);
			Assert.Equal("B2", data.Value);
			Assert.Equal(cache.GetIdForKey("A2"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B2"), data.ValueId);
		}

		[Fact]
		public void CanAddTwoItems()
		{
			//Arrange/Act
			var cache = new CollectedLookupIdCache(10, 10);
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");
			CollectedItemDbHelper.Insert(item, cache);

			item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A2", "B2");
			CollectedItemDbHelper.Insert(item, cache);

			//Assert
			var data = GetDbData().Where(n => n.Key == "A").Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A", data.Key);
			Assert.Equal("B", data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B"), data.ValueId);

			data = GetDbData().Where(n => n.Key != "A").Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A2", data.Key);
			Assert.Equal("B2", data.Value);
			Assert.Equal(cache.GetIdForKey("A2"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B2"), data.ValueId);
		}

		[Fact]
		public void KeysAndValuesAreCaseInsesitive()
		{
			//Arrange / Act
			var cache = new CollectedLookupIdCache(10, 10);
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");
			CollectedItemDbHelper.Insert(item, cache);
			Assert.Equal(cache.GetIdForKey("A"), cache.GetIdForKey("a"));
			Assert.Equal(cache.GetIdForValue("B"), cache.GetIdForValue("b"));

			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddHours(1);
			item.CapturedValues.Add("a", "b");
			CollectedItemDbHelper.Insert(item, cache);

			//Assert
			var allData = GetDbData();
			Assert.Equal(2, allData.Where(n => n.KeyId == cache.GetIdForKey("A")).Count());
			Assert.Equal(cache.GetIdForKey("A"), cache.GetIdForKey("a"));
			Assert.Equal(2, allData.Where(n => n.ValueId == cache.GetIdForValue("B")).Count());
			Assert.Equal(cache.GetIdForValue("B"), cache.GetIdForValue("b"));
		}

		[Fact]
		public void CanAddOneItemTooLongButTruncated()
		{
			//Arrange
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add(new string('A', CollectedItemDbHelper.MaxKeyLength + 1), new string('B', CollectedItemDbHelper.MaxValueLength + 1));
			var cache = new CollectedLookupIdCache(10, 10);

			//Act
			CollectedItemDbHelper.Insert(item, cache);

			//Assert
			var data = GetDbData().Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal(new string('A', CollectedItemDbHelper.MaxKeyLength), data.Key);
			Assert.Equal(new string('B', CollectedItemDbHelper.MaxValueLength), data.Value);
			Assert.Equal(cache.GetIdForKey(new string('A', CollectedItemDbHelper.MaxKeyLength)), data.KeyId);
			Assert.Equal(cache.GetIdForValue(new string('B', CollectedItemDbHelper.MaxValueLength)), data.ValueId);
		}
		#endregion


		#region Aggregate Insert
		[Fact]
		public void CanAddOneAggrItem()
		{
			//Arrange
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");
			var cache = new CollectedLookupIdCache(10, 10);
			var aggr = ConvertToAggr(item);

			//Act
			CollectedItemDbHelper.Insert(aggr, cache);

			//Assert
			var data = GetDbData().Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A", data.Key);
			Assert.Equal("B", data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B"), data.ValueId);
		}

		[Fact]
		public void CanAddOneAggrItemWithNullValue()
		{
			//Arrange
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", null);
			var cache = new CollectedLookupIdCache(10, 10);
			var aggr = ConvertToAggr(item);

			//Act
			CollectedItemDbHelper.Insert(aggr, cache);

			//Assert
			var data = GetDbData().Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A", data.Key);
			Assert.Equal(null, data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(null, data.ValueId);
		}

		[Fact]
		public void CanAddOneAggrItemWithMultipleCaptures()
		{
			//Arrange
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");
			item.CapturedValues.Add("A2", "B2");
			var cache = new CollectedLookupIdCache(10, 10);
			var aggr = ConvertToAggr(item);

			//Act
			CollectedItemDbHelper.Insert(aggr, cache);

			//Assert
			var allData = GetDbData();
			Assert.Equal(2, allData.Count);
			var data = allData.Where(n => n.Key == "A").Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A", data.Key);
			Assert.Equal("B", data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B"), data.ValueId);

			data = allData.Where(n => n.Key != "A").Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A2", data.Key);
			Assert.Equal("B2", data.Value);
			Assert.Equal(cache.GetIdForKey("A2"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B2"), data.ValueId);
		}

		[Fact]
		public void CanAddTwoItemsAggr()
		{
			//Arrange/Act
			var cache = new CollectedLookupIdCache(10, 10);
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");

			var item2 = GetEmptyCollectedItem();
			item2.CapturedValues.Add("A2", "B2");

			var aggr = ConvertToAggr(new[] { item, item2 });
			CollectedItemDbHelper.Insert(aggr, cache);

			//Assert
			var data = GetDbData().Where(n => n.Key == "A").Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A", data.Key);
			Assert.Equal("B", data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B"), data.ValueId);

			data = GetDbData().Where(n => n.Key != "A").Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal("A2", data.Key);
			Assert.Equal("B2", data.Value);
			Assert.Equal(cache.GetIdForKey("A2"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B2"), data.ValueId);
		}

		[Fact]
		public void KeysAndValuesAreCaseInsesitiveAggr()
		{
			//Arrange / Act
			var cache = new CollectedLookupIdCache(10, 10);
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");
			var aggr = ConvertToAggr(item);
			CollectedItemDbHelper.Insert(aggr, cache);
			Assert.Equal(cache.GetIdForKey("A"), cache.GetIdForKey("a"));
			Assert.Equal(cache.GetIdForValue("B"), cache.GetIdForValue("b"));

			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddHours(1);
			item.CapturedValues.Add("a", "b");
			aggr = ConvertToAggr(item);
			CollectedItemDbHelper.Insert(aggr, cache);

			//Assert
			var allData = GetDbData();
			Assert.Equal(2, allData.Where(n => n.KeyId == cache.GetIdForKey("A")).Count());
			Assert.Equal(cache.GetIdForKey("A"), cache.GetIdForKey("a"));
			Assert.Equal(2, allData.Where(n => n.ValueId == cache.GetIdForValue("B")).Count());
			Assert.Equal(cache.GetIdForValue("B"), cache.GetIdForValue("b"));
		}

		[Fact]
		public void CanAddOneAggrItemTooLongButTruncated()
		{
			//Arrange
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add(new string('A', CollectedItemDbHelper.MaxKeyLength + 1), new string('B', CollectedItemDbHelper.MaxValueLength + 1));
			var cache = new CollectedLookupIdCache(10, 10);
			var aggr = ConvertToAggr(item);

			//Act
			CollectedItemDbHelper.Insert(aggr, cache);

			//Assert
			var data = GetDbData().Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal(now, data.CreateDate);
			Assert.Equal(new string('A', CollectedItemDbHelper.MaxKeyLength), data.Key);
			Assert.Equal(new string('B', CollectedItemDbHelper.MaxValueLength), data.Value);
			Assert.Equal(cache.GetIdForKey(new string('A', CollectedItemDbHelper.MaxKeyLength)), data.KeyId);
			Assert.Equal(cache.GetIdForValue(new string('B', CollectedItemDbHelper.MaxValueLength)), data.ValueId);
		}

		[Fact]
		public void CanAddSeveralItemsAggr()
		{
			//Arrange
			var cache = new CollectedLookupIdCache(10, 10);
			var aggr = GetEmptyAggrItem();
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");
			AddCollectedItem(aggr, item);

			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(1);
			item.CapturedValues.Add("A2", "B2");
			AddCollectedItem(aggr, item);

			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(2);
			item.CapturedValues.Add("A", null);
			AddCollectedItem(aggr, item);

			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(3);
			item.CapturedValues.Add("A", "B");
			AddCollectedItem(aggr, item);

			//Act
			CollectedItemDbHelper.Insert(aggr, cache);

			//Assert
			Assert.Equal(2, aggr.KeyLookup.Count);
			Assert.Equal(2, aggr.ValueLookup.Count);

			var data = GetDbData().Where(n => n.CreateDate == now).Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal("A", data.Key);
			Assert.Equal("B", data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B"), data.ValueId);

			data = GetDbData().Where(n => n.CreateDate == now.AddMinutes(1)).Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal("A2", data.Key);
			Assert.Equal("B2", data.Value);
			Assert.Equal(cache.GetIdForKey("A2"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B2"), data.ValueId);

			data = GetDbData().Where(n => n.CreateDate == now.AddMinutes(2)).Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal("A", data.Key);
			Assert.Equal(null, data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(null, data.ValueId);

			data = GetDbData().Where(n => n.CreateDate == now.AddMinutes(3)).Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal("A", data.Key);
			Assert.Equal("B", data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B"), data.ValueId);

			Assert.Equal(4, GetDbData().Count());
		}
		#endregion

		[Fact]
		public void MaxLimitsMatch()
		{
			using (var conn = new SqlConnection(Tct.ActivityRecorderService.Properties.Settings.Default.recorderConnectionString))
			{
				conn.Open();
				var maxKeyLength =
					conn.Query<int>(
						"SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CollectedKeyLookup' AND COLUMN_NAME = 'Key'")
						.Single();
				var maxValueLength =
					conn.Query<int>(
						"SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CollectedValueLookup' AND COLUMN_NAME = 'Value'")
						.Single();
				Assert.Equal(CollectedItemDbHelper.MaxKeyLength, maxKeyLength);
				Assert.Equal(CollectedItemDbHelper.MaxValueLength, maxValueLength);
			}
		}

		[Fact]
		public void CanAddItemExistingInLookup()
		{
			var testValue = "Test Value #" + new Random().Next();
			var cache = new CollectedLookupIdCache(10, 10);
			var aggr = GetEmptyAggrItem();
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", testValue);
			AddCollectedItem(aggr, item);

			CollectedItemDbHelper.Insert(aggr, cache);

			var testData = GetDbData().Where(n => n.CreateDate == now).Single();
			var insertedValueId = testData.ValueId;
			Assert.NotNull(insertedValueId);
			Assert.Equal(cache.GetIdForValue(testValue), testData.ValueId);

			aggr = GetEmptyAggrItem();
			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(1);
			item.CapturedValues.Add("A2", testValue);
			AddCollectedItem(aggr, item);

			CollectedItemDbHelper.Insert(aggr, cache);

			testData = GetDbData().Where(n => n.CreateDate == now.AddMinutes(1)).Single();
			Assert.Equal(insertedValueId, testData.ValueId);
			Assert.Equal(cache.GetIdForValue(testValue), testData.ValueId);


		}

		[Fact]
		public void ComplexMultipleInsertion()
		{

			//Step 1: Some new items: nothing has id
			var cache = new CollectedLookupIdCache(10, 10);
			var aggr = GetEmptyAggrItem();
			var item = GetEmptyCollectedItem();
			item.CapturedValues.Add("A", "B");
			AddCollectedItem(aggr, item);

			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(1);
			item.CapturedValues.Add("A2", "B2");
			item.CapturedValues.Add("A3", "B3");
			AddCollectedItem(aggr, item);

			CollectedItemDbHelper.Insert(aggr, cache);

			Assert.Equal(3, GetDbData().Count());
			Assert.Equal(3, GetDbData().Select(n => n.KeyId).Distinct().Count());
			Assert.Equal(3, GetDbData().Select(n => n.ValueId).Distinct().Count());

			//Step 2: Some new items: part of it has id
			aggr = GetEmptyAggrItem();
			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(2);
			item.CapturedValues.Add("A2", "B2");
			item.CapturedValues.Add("A3", "B3");
			AddCollectedItem(aggr, item);

			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(3);
			item.CapturedValues.Add("A4", "B4");
			AddCollectedItem(aggr, item);

			CollectedItemDbHelper.Insert(aggr, cache);

			Assert.Equal(6, GetDbData().Count());
			Assert.Equal(4, GetDbData().Select(n => n.KeyId).Distinct().Count());
			Assert.Equal(4, GetDbData().Select(n => n.ValueId).Distinct().Count());

			//Step 3: Some new items: all of it has id
			aggr = GetEmptyAggrItem();
			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(4);
			item.CapturedValues.Add("A2", "B2");
			item.CapturedValues.Add("A3", "B6");
			AddCollectedItem(aggr, item);

			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(5);
			item.CapturedValues.Add("A4", "B4");
			AddCollectedItem(aggr, item);

			CollectedItemDbHelper.Insert(aggr, cache);

			Assert.Equal(9, GetDbData().Count());
			Assert.Equal(4, GetDbData().Select(n => n.KeyId).Distinct().Count());
			Assert.Equal(5, GetDbData().Select(n => n.ValueId).Distinct().Count());

			//Step 4: Some new items: part of it has id
			aggr = GetEmptyAggrItem();
			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(6);
			item.CapturedValues.Add("A2", "B2");
			item.CapturedValues.Add("A3", "B3");
			AddCollectedItem(aggr, item);

			item = GetEmptyCollectedItem();
			item.CreateDate = now.AddMinutes(7);
			item.CapturedValues.Add("A5", "B5");
			AddCollectedItem(aggr, item);

			CollectedItemDbHelper.Insert(aggr, cache);

			//Step 5: Check results
			//TODO
			var data = GetDbData().Where(n => n.CreateDate == now).Single();
			Assert.Equal(userId, data.UserId);
			Assert.Equal(computerId, data.ComputerId);
			Assert.Equal("A", data.Key);
			Assert.Equal("B", data.Value);
			Assert.Equal(cache.GetIdForKey("A"), data.KeyId);
			Assert.Equal(cache.GetIdForValue("B"), data.ValueId);

			Assert.Equal(12, GetDbData().Count());
			Assert.Equal(5, GetDbData().Select(n => n.KeyId).Distinct().Count());
			Assert.Equal(6, GetDbData().Select(n => n.ValueId).Distinct().Count());

		}

		// ReSharper disable ClassNeverInstantiated.Local, UnusedAutoPropertyAccessor.Local
		private class CollectedDbItem
		{
			public int UserId { get; set; }
			public int ComputerId { get; set; }
			public DateTime CreateDate { get; set; }
			public int KeyId { get; set; }
			public int? ValueId { get; set; }
			public string Key { get; set; }
			public string Value { get; set; }
		}
		// ReSharper restore ClassNeverInstantiated.Local, UnusedAutoPropertyAccessor.Local
	}
}
