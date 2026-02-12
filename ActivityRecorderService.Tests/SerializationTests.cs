using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Persistence;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class SerializationTests
	{
		private readonly TimeZoneInfo localTimeZone = TimeZoneInfo.FromSerializedString("Central Europe Standard Time;60;(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague;Central Europe Standard Time;Central Europe Daylight Time;[01:01:0001;12:31:9999;60;[0;02:00:00;3;5;0;];[0;03:00:00;10;5;0;];];");

		[DataContract]
		//[KnownType(typeof(System.TimeZoneInfo))]
		//[KnownType(typeof(System.TimeZoneInfo.AdjustmentRule))]
		[KnownType(typeof(System.TimeZoneInfo.AdjustmentRule[]))]
		[KnownType(typeof(System.TimeZoneInfo.TransitionTime))]
		[KnownType(typeof(System.DayOfWeek))]
		private class E
		{
			[DataMember]
			public TimeZoneInfo TZInfo;
		}

		//http://social.msdn.microsoft.com/forums/en-US/wcf/thread/f164f185-ae18-4775-a2ff-a814813d262d
		[Fact]
		public void CanSerializeTimeZoneInfo()
		{
			using (var stream = new MemoryStream())
			{
				XmlPersistenceHelper.WriteToStream(stream, new E() { TZInfo = localTimeZone });
				var w = stream.ToArray();
				var s = Encoding.UTF8.GetString(w);
				Assert.True(s.Contains(localTimeZone.Id));

				//Console.WriteLine(s);
			}
		}

		[Fact]
		public void CanSerializeRule()
		{
			var rule = new WorkDetectorRule() { Name = "Testname", ProcessRule = "devenv.*", TitleRule = ";\\<\">", RuleType = WorkDetectorRuleType.TempStartWork, RelatedId = 23 };
			var ruleStr = ToJson(rule);

			//Console.WriteLine(ruleStr);
			//Console.WriteLine(HttpUtility.HtmlEncode(ruleStr));
		}

		public static string ToJson(object obj)
		{
			DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
			using (var stream = new MemoryStream())
			{
				ser.WriteObject(stream, obj);
				string result = Encoding.UTF8.GetString(stream.ToArray());
				return result;
			}
		}

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
		}

		[Fact]
		public void XmlSerialize()
		{
			using (var stream = new MemoryStream())
			{
				XmlPersistenceHelper.WriteToStream(stream, sampleRules[1]);
				var contents = Encoding.UTF8.GetString(stream.ToArray());
				Console.WriteLine(contents);
				//Assert.Same(sampleRuleOneXml, contents);
			}
		}

		[Fact]
		public void XmlDeserialize()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleRuleOneXml)))
			{
				WorkDetectorRule deserialized;
				XmlPersistenceHelper.ReadFromStream(stream, out deserialized);
				AssertSame(deserialized, sampleRules[1]);
			}
		}
	}
}
