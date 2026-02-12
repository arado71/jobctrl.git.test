using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Rules;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class TemplateTests
	{
		[Fact]
		public void ParseComment()
		{
			Regex commentMatcher = new Regex(@"\(\?#(?<comment>.*?)\)");
			var search = ".(?#ThisIsAComment)*(?#ThisIsAnOtherComment)";
			foreach (Match match in commentMatcher.Matches(search))
			{
				Console.WriteLine(match.Groups["comment"].Value);
			}
			Assert.True(
				new[] { "ThisIsAComment", "ThisIsAnOtherComment" }
				.SequenceEqual(
				commentMatcher.Matches(search).OfType<Match>().Select(n => n.Groups["comment"].Value))
				);
		}

		[Fact]
		public void DollarEscape()
		{
			//So it's not wise to use $$PLACEHOLDER$$ format
			//but (?#PLACEHOLDER) and $PLACEHOLDER$ might be ok
			var dollar = "$";
			var r = new Regex("$$");
			var r2 = new Regex("\\$");
			var r3 = new Regex("[$]");
			Assert.True(r.IsMatch(dollar));
			Assert.True(r2.IsMatch(dollar));
			Assert.True(r3.IsMatch(dollar));
		}

		[Fact]
		public void PlaceholderSimpleReplace()
		{
			var p1 = PlaceholderHelper.GetPlaceholder("p1");
			var p2 = PlaceholderHelper.GetPlaceholder("p2");
			var str = "a" + p1 + "c" + p2 + "e";
			var res = PlaceholderHelper.ReplacePlaceholders(str, key =>
				{
					switch (key)
					{
						case "p1": return "b";
						case "p2": return "d";
						default: throw new ArgumentException();
					}
				});
			Assert.NotEqual(str, res);
			Assert.Equal("abcde", res);
		}

		[Fact]
		public void InvalidPlaceholderKeys()
		{
			var invalidKeys = new[] { "$", "", "\\", "Sp ace", "\n", "wer\\wer", "wer$wer", "wer\nwer", "(", "?", "#", ")", "[", "]" }; //etc...
			foreach (var invalidKey in invalidKeys)
			{
				var key = invalidKey;
				Assert.Throws<ArgumentException>(() => PlaceholderHelper.GetPlaceholder(key));
			}
		}

		[Fact]
		public void TryReplacePlaceholders()
		{
			var p1 = PlaceholderHelper.GetPlaceholder("p1");
			var p2 = PlaceholderHelper.GetPlaceholder("p2");
			var str = "a" + p1 + "c" + p2 + "e";
			var e = new Dictionary<string, string> { { "p1", "b" } };
			string res;
			var resOk = PlaceholderHelper.TryReplacePlaceholders(str, e.TryGetValue, out res);
			Assert.False(resOk);
			//not sure if we need these below...
			Assert.NotEqual(str, res);
			Assert.Equal("abc" + p2 + "e", res);

			e.Add("p2", "d");
			resOk = PlaceholderHelper.TryReplacePlaceholders(str, e.TryGetValue, out res);
			Assert.True(resOk);
			Assert.NotEqual(str, res);
			Assert.Equal("abcde", res);
		}

		[Fact]
		public void ReplacePlaceholdersWithTryFunc()
		{
			var p1 = PlaceholderHelper.GetPlaceholder("p1");
			var p2 = PlaceholderHelper.GetPlaceholder("p2");
			var str = "a" + p1 + "c" + p2 + "e";
			var e = new Dictionary<string, string> { { "p1", "b" } };
			int ok, nok;
			var res = PlaceholderHelper.ReplacePlaceholdersWithTryReplaceFunc(str, e.TryGetValue, out ok, out nok);
			Assert.Equal(1, ok);
			Assert.Equal(1, nok);
			//not sure if we need these below...
			Assert.NotEqual(str, res);
			Assert.Equal("abc" + p2 + "e", res);

			e.Add("p2", "d");
			res = PlaceholderHelper.ReplacePlaceholdersWithTryReplaceFunc(res, e.TryGetValue, out ok, out nok);
			Assert.Equal(1, ok);
			Assert.Equal(0, nok);
			Assert.NotEqual(str, res);
			Assert.Equal("abcde", res);

			res = PlaceholderHelper.ReplacePlaceholdersWithTryReplaceFunc(str, e.TryGetValue, out ok, out nok);
			Assert.Equal(2, ok);
			Assert.Equal(0, nok);
			Assert.NotEqual(str, res);
			Assert.Equal("abcde", res);
		}

		[Fact]
		public void NullTryReplacePlaceholdersFunc()
		{
			string res;
			var str = "input";
			var resOk = PlaceholderHelper.TryReplacePlaceholders(str, null, out res);
			Assert.False(resOk);
			Assert.Equal(str, res);
		}

		[Fact]
		public void NullTryReplacePlaceholdersFunc2()
		{
			var str = "input";
			int ok, nok;
			var res = PlaceholderHelper.ReplacePlaceholdersWithTryReplaceFunc(str, null, out ok, out nok);
			Assert.Equal(0, ok);
			Assert.Equal(0, nok);
			Assert.Equal(str, res);
		}

		[Fact]
		public void NullReplacePlaceholdersFunc()
		{
			Assert.Throws<ArgumentNullException>(() => PlaceholderHelper.ReplacePlaceholders("input", null));
		}

		[Fact]
		public void ProjectTemplateIsValid()
		{
			Assert.DoesNotThrow(() => new WorkChangingRuleFactory.ProjectTemplateData(new WorkData() { ProjectId = 1, Name = "xy" }, 2));
		}

		[Fact]
		public void WorkTemplateIsValid()
		{
			Assert.DoesNotThrow(() => new WorkChangingRuleFactory.WorkTemplateData(new WorkData() { Id = 1, Name = "xy" }, new ClientMenuLookup()));
		}
	}
}
