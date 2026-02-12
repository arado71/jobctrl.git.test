using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Persistence;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class RuleGeneratorDbTests : DbTestsBase
	{
		[Fact]
		public void RuleGeneratorFactoryTestsAndDbAreSame()
		{
			List<RuleGeneratorData> testRules, dbRules;
			var testData = RuleGeneratorFactoryHelper.CurrentGenerators;
			string dbData;
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(testData)))
			{
				XmlPersistenceHelper.ReadFromStream(stream, out testRules);
			}
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				string version = null;
				dbData = context.GetLearningRuleGenerators(1, null, ref version).Single().LearningRuleGenerators;
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(dbData)))
				{
					XmlPersistenceHelper.ReadFromStream(stream, out dbRules);
				}
			}

			Assert.Equal(testRules.Count, dbRules.Count);
			for (int i = 0; i < testRules.Count; i++)
			{
				Assert.Equal(testRules[i].Name, dbRules[i].Name);
				Assert.Equal(testRules[i].Parameters, dbRules[i].Parameters);
			}

			using (var testStream = new MemoryStream())
			using (var dbStream = new MemoryStream())
			{
				XmlPersistenceHelper.WriteToStream(testStream, testRules);
				XmlPersistenceHelper.WriteToStream(dbStream, dbRules);

				var test = testStream.ToArray();
				var db = dbStream.ToArray();
				Assert.True(test.Length == db.Length, "Size is different" + Environment.NewLine + "test: " + Environment.NewLine + testData
					+ Environment.NewLine + "db: " + Environment.NewLine + dbData);
				for (int i = 0; i < test.Length; i++)
				{
					Assert.True(test[i] == db[i], "Difference at pos " + i + Environment.NewLine + "test: " + Environment.NewLine + testData
						+ Environment.NewLine + "db: " + Environment.NewLine + dbData);
				}
			}
		}
	}
}
