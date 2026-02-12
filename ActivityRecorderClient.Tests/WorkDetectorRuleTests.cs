using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class WorkDetectorRuleTests
	{
		private readonly List<WorkDetectorRule> sampleRules = new List<WorkDetectorRule>()
			{
				new WorkDetectorRule(), //empty rule
				new	WorkDetectorRule(){ 
					RuleType = WorkDetectorRuleType.TempStartWork,
					RelatedId = 23,
					IgnoreCase = true,
					IsEnabled = true,
					IsRegex = true,
					Name = "<script>\";&amp;&quot;",
					ProcessRule = ".*",
					TitleRule = ".*",
				},
				new	WorkDetectorRule(){ 
					Name = "Two  spaces, and now   Three",
				},
				new	WorkDetectorRule(){ 
					Name = "{}!@#$%^&*()_-=+",
				},
				new WorkDetectorRule() { RuleType= WorkDetectorRuleType.TempStopWork },
				new WorkDetectorRule() { RuleType= WorkDetectorRuleType.TempStartCategory },
				new WorkDetectorRule() { RuleType= WorkDetectorRuleType.DoNothing },
			};

		private readonly string sampleRuleOneXml = ""
			+ "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine
			+ "<WorkDetectorRule xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://jobctrl.com/\">" + Environment.NewLine
			+ "  <RuleType>TempStartWork</RuleType>" + Environment.NewLine
			+ "  <RelatedId>23</RelatedId>" + Environment.NewLine
			+ "  <Name>&lt;script&gt;\";&amp;amp;&amp;quot;</Name>" + Environment.NewLine
			+ "  <IsEnabled>true</IsEnabled>" + Environment.NewLine
			+ "  <IsRegex>true</IsRegex>" + Environment.NewLine
			+ "  <IgnoreCase>true</IgnoreCase>" + Environment.NewLine
			+ "  <TitleRule>.*</TitleRule>" + Environment.NewLine
			+ "  <ProcessRule>.*</ProcessRule>" + Environment.NewLine
			+ "</WorkDetectorRule>";

		private readonly string unknownFieldXml = ""
			+ "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine
			+ "<WorkDetectorRule xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://jobctrl.com/\">" + Environment.NewLine
			+ "  <RuleType>TempStartWork</RuleType>" + Environment.NewLine
			+ "  <RelatedId>23</RelatedId>" + Environment.NewLine
			+ "  <Name>&lt;script&gt;\";&amp;amp;&amp;quot;</Name>" + Environment.NewLine
			+ "  <IsEnabled>true</IsEnabled>" + Environment.NewLine
			+ "  <IsRegex>true</IsRegex>" + Environment.NewLine
			+ "  <IgnoreCase>true</IgnoreCase>" + Environment.NewLine
			+ "  <TitleRule>.*</TitleRule>" + Environment.NewLine
			+ "  <ProcessRule>.*</ProcessRule>" + Environment.NewLine
			+ "  <Doba>l33t</Doba>" + Environment.NewLine
			+ "</WorkDetectorRule>";

		private readonly string sampleRuleOneJson = @"{""RuleType"":0,""RelatedId"":23,""Name"":""<script>\"";&amp;&quot;"",""IsEnabled"":true,""IsRegex"":true,""IgnoreCase"":true,""TitleRule"":"".*"",""ProcessRule"":"".*""}";
		private readonly string unknownJson = @"{""RuleType"":0,""RelatedId"":23,""Name"":""<script>\"";&amp;&quot;"",""IsEnabled"":true,""IsRegex"":true,""IgnoreCase"":true,""TitleRule"":"".*"",""ProcessRule"":"".*"",""Doba"":""l33t""}";

		public static void AssertSame(WorkDetectorRule first, WorkDetectorRule second)
		{
			Assert.Equal(first.RuleType, second.RuleType);
			Assert.Equal(first.RelatedId, second.RelatedId);
			Assert.Equal(first.IgnoreCase, second.IgnoreCase);
			Assert.Equal(first.IsEnabled, second.IsEnabled);
			Assert.Equal(first.IsRegex, second.IsRegex);
			Assert.Equal(first.Name, second.Name);
			Assert.Equal(first.ProcessRule, second.ProcessRule);
			Assert.Equal(first.TitleRule, second.TitleRule);
			Assert.Equal(first.UrlRule, second.UrlRule);
		}

		[Fact]
		public void CanSerializeAndDeserializeSampleRules()
		{
			foreach (var rule in sampleRules)
			{
				var serialized = rule.ToSerializedString();
				//Console.WriteLine(serialized);
				var deserialized = WorkDetectorRule.FromSerializedString(serialized);
				AssertSame(rule, deserialized);
			}
		}

		[Fact]
		public void XmlSerialize()
		{
			using (var stream = new MemoryStream())
			{
				XmlSerializationHelper.WriteToStream(stream, sampleRules[1]);
				var contents = Encoding.UTF8.GetString(stream.ToArray());
				//Console.WriteLine(contents);
			}
		}

		[Fact]
		public void XmlDeserialize()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleRuleOneXml)))
			{
				WorkDetectorRule deserialized;
				XmlSerializationHelper.ReadFromStream(stream, out deserialized);
				AssertSame(deserialized, sampleRules[1]);
			}
		}

		[Fact]
		public void CanSerializeRuleWithUnkownField()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(unknownFieldXml)))
			{
				WorkDetectorRule deserialized;
				XmlSerializationHelper.ReadFromStream(stream, out deserialized);
				var serialized = deserialized.ToSerializedString();
				Console.Write(serialized);
				var deserializedAgain = WorkDetectorRule.FromSerializedString(serialized);
				AssertSame(deserialized, deserializedAgain);
			}
		}

		[Fact]
		public void CanCloneRuleWithUnkownField()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(unknownFieldXml)))
			{
				WorkDetectorRule deserialized;
				XmlSerializationHelper.ReadFromStream(stream, out deserialized);
				var clone = deserialized.Clone();
				AssertSame(deserialized, clone);

				using (var stream2 = new MemoryStream())
				{
					XmlSerializationHelper.WriteToStream(stream2, clone);
					var contents = Encoding.UTF8.GetString(stream2.ToArray());
					Assert.Contains("doba", unknownFieldXml, StringComparison.OrdinalIgnoreCase);
					Assert.DoesNotContain("doba", contents, StringComparison.OrdinalIgnoreCase); //extension data is lost, but that is ok atm.
				}
			}
		}

		[Fact]
		public void JsonDeserialize()
		{
			var deserialized = WorkDetectorRule.FromSerializedString(sampleRuleOneJson);
			AssertSame(deserialized, sampleRules[1]);
			Assert.Equal(sampleRuleOneJson, sampleRules[1].ToSerializedString());
		}

		[Fact]
		public void CanDeserializeRuleWithUnkownField()
		{
			var deserialized = WorkDetectorRule.FromSerializedString(unknownJson);
			AssertSame(deserialized, sampleRules[1]);
			Assert.Contains("doba", unknownJson, StringComparison.OrdinalIgnoreCase);
			Assert.DoesNotContain("doba", deserialized.ToSerializedString(), StringComparison.OrdinalIgnoreCase);
		}
	}
}
