using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Rules.Generation;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class RuleGenerationTests
	{
		private static readonly string[] legacyWordTitles = new[] {
				"{0} - Microsoft Word",
				"{0} - Microsoft Word (A termékaktiválás nem sikerült)",
				"{0}:1 - Microsoft Word",
				"{0} [kompatibilis mód] - Microsoft Word (A termékaktiválás nem sikerült)",
				"{0} [Írásvédett] [kompatibilis mód] - Microsoft Word",
				"{0} [Compatibility Mode] - Microsoft Word",
				"{0} [Compatibility Mode]:2 - Microsoft Word",
				"{0} (Read-Only) [Compatibility Mode] - Microsoft Word",
				"{0} (Csak olvasható) - Microsoft Word",
				"{0} [Írásvédett] - Microsoft Word",
				"{0} (Csak olvasható) [kompatibilitási mód] - Microsoft Word",	
				"{0} (Védett nézet) - Microsoft Word",
				"{0} (Minta) - Microsoft Word",
				"{0} - Microsoft Word (Product Activation Failed)",
				"{0} (Protected View) - Microsoft Word",
				//"{0} - Dokumentum [kompatibilis mód] - Microsoft Word (A termékaktiválás nem sikerült)", //we don't support this atm.
				"{0} (Autosaved) [Compatibility Mode] - Microsoft Word",
				"{0} (Read-Only) (Last saved by user) [Compatibility Mode] - Microsoft Word",
				"{0} (Csak olvasható) (Utoljára a felhasználó mentette) - Microsoft Word",
				"{0} [kompatibilitási mód] (Minta) - Microsoft Word",
			};

		private static readonly string[] legacyExcelTitles = new[] {
				"Microsoft Excel - {0}",
				"Microsoft Excel - {0}:2  [kompatibilis üzemmód]",
				"Microsoft Excel - {0}:2",
				"Microsoft Excel (A termékaktiválás nem sikerült) - {0}",
				"Microsoft Excel (A termékaktiválás nem sikerült) - {0}  [Védett nézet]",
				"Microsoft Excel (A termékaktiválás nem sikerült) - {0}  [Olvasásra]",
				"Microsoft Excel (A termékaktiválás nem sikerült) - {0}  [kompatibilis üzemmód]",
				"Microsoft Excel (Product Activation Failed) - {0}",
				"Microsoft Excel (Product Activation Failed) - {0}  [Protected View]",
				"Microsoft Excel (Product Activation Failed) - {0}  [Compatibility Mode]",
				"Microsoft Excel - {0} [Last saved by user]  [Compatibility Mode]",
				"Microsoft Excel - {0} [Legutóbb mentő felhasználó]",
				"Microsoft Excel - {0}  [Protected View]",
				"Microsoft Excel - {0}  [Olvasásra]  [közös]  [kompatibilis üzemmód]",
				"Microsoft Excel - {0}  [Csoport]",
				"Microsoft Excel - {0}  [Shared]  [Compatibility Mode]",
				"Microsoft Excel - {0}  [Read-Only]  [Shared]  [Compatibility Mode]",
				"Microsoft Excel - {0}  [Read-Only]  [Compatibility Mode]",
				"Microsoft Excel - {0}  [Olvasásra]  [kompatibilis üzemmód]",
				"Microsoft Excel - {0}  [Védett nézet]",
				"Microsoft Excel - {0}  [Read-Only]",
				"Microsoft Excel - {0}  [Olvasásra]",
				"Microsoft Excel - {0}  [közös]",
				"Microsoft Excel - {0}  [Védett nézet]",
				"Microsoft Excel - {0}  [kompatibilis üzemmód]",
		};

		[Fact]
		public void WordMatchTitlesWithRegex()
		{
			var rightDoc = "sdafdsagfasdgf. asdfads fsd gs (2012-09-03).docx[1]";
			var wrongDoc = "asdafdsagfasdgf. asdfads fsd gs (2012-09-03).docx[1]";

			foreach (var ruleTitle in legacyWordTitles)
			{
				var matcher = GetWordRuleFromTite(string.Format(ruleTitle, rightDoc));
				foreach (var title in legacyWordTitles)
				{
					var rightTitle = string.Format(title, rightDoc);
					var wrongTitle = string.Format(title, wrongDoc);
					Assert.True(matcher.IsMatch(rightTitle), "Failed to match " + rightTitle + " with " + matcher);
					Assert.False(matcher.IsMatch(wrongTitle), "Matched '" + wrongTitle + "' but it shouldn't with " + matcher);
				}
			}
		}

		private const string wordPattern1 = @"^";
		private const string wordPattern2 = @"(?<title>.+?)";
		private const string wordPattern3 = @"((?<optPar>\s\([\p{L}\s-]+\))?(?<optBrac>\s\[[\p{L}\s-]+\])?)*(?:[:]\d+)?\s- Microsoft Word(?<optParEnd>\s\([\p{L}\s-]+\))?$";
		private Regex GetWordRuleFromTite(string title)
		{
			var rep = Regex.Replace(title, wordPattern1 + wordPattern2 + wordPattern3, WordTitleMatchEvaluator);
			return new Regex(rep);
		}

		private static string WordTitleMatchEvaluator(Match match)
		{
			if (!match.Success) return match.ToString();
			return wordPattern1 + Regex.Escape(match.Groups["title"].Value) + wordPattern3;
		}

		[Fact]
		public void ExcelMatchTitlesWithRegex()
		{
			var rightDoc = "sdafdsagfasdgf. asdfads fsd gs (2012-09-03).xlsx[1]";
			var wrongDoc = "asdafdsagfasdgf. asdfads fsd gs (2012-09-03).xlsx[1]";

			foreach (var ruleTitle in legacyExcelTitles)
			{
				var matcher = GetExcelRuleFromTite(string.Format(ruleTitle, rightDoc));
				foreach (var title in legacyExcelTitles)
				{
					var rightTitle = string.Format(title, rightDoc);
					var wrongTitle = string.Format(title, wrongDoc);
					Assert.True(matcher.IsMatch(rightTitle), "Failed to match " + rightTitle + " with " + matcher);
					Assert.False(matcher.IsMatch(wrongTitle), "Matched '" + wrongTitle + "' but it shouldn't with " + matcher);
				}
			}
		}

		private const string excelPattern1 = @"^Microsoft Excel(?<optBrac>\s\([\p{L}\s-]+\))?\s-\s";
		private const string excelPattern2 = @"(?<title>.+?)";
		private const string excelPattern3 = @"(?:[:]\d+)?(?<optBrac>\s\s?\[[\p{L}\s-]+\])*$";
		private Regex GetExcelRuleFromTite(string title)
		{
			var rep = Regex.Replace(title, excelPattern1 + excelPattern2 + excelPattern3, ExcelTitleMatchEvaluator);
			return new Regex(rep);
		}

		private static string ExcelTitleMatchEvaluator(Match match)
		{
			if (!match.Success) return match.ToString();
			return excelPattern1 + Regex.Escape(match.Groups["title"].Value) + excelPattern3;
		}

		[Fact]
		public void WordMatchTitlesWithRuleGenerator()
		{
			var processParams = new[] { new ReplaceGroupParameter() { MatchingPattern = "winword.exe" } };
			var urlParams = new[] { new ReplaceGroupParameter() { MatchingPattern = ".*" } };
			var titleParams = new[] { 
				new ReplaceGroupParameter() { MatchingPattern = wordPattern1 },
				new ReplaceGroupParameter() { MatchingPattern = wordPattern2, ReplaceGroupName = "title" },
				new ReplaceGroupParameter() { MatchingPattern = wordPattern3 },
			};
			var generator = new ReplaceGroupRuleGenerator(false, processParams, titleParams, urlParams);

			var rightDoc = "sdafdsagfasdgf. asdfads fsd gs (2012-09-03).docx[1]";
			var wrongDoc = "dafdsagfasdgf. asdfads fsd gs (2012-09-03).docx[1]";

			foreach (var ruleTitle in legacyWordTitles)
			{
				var pattern = generator.GetRuleFromWindow(new DesktopWindow() { ProcessName = "winword.exe", Title = string.Format(ruleTitle, rightDoc), });
				Assert.True(pattern.IsRegex);
				Assert.True(pattern.IsEnabled);
				var matcher = new Regex(pattern.TitleRule);
				foreach (var title in legacyWordTitles)
				{
					var rightTitle = string.Format(title, rightDoc);
					var wrongTitle = string.Format(title, wrongDoc);
					Assert.True(matcher.IsMatch(rightTitle), "Failed to match " + rightTitle + " with " + matcher);
					Assert.False(matcher.IsMatch(wrongTitle), "Matched '" + wrongTitle + "' but it shouldn't with " + matcher);
				}
			}
		}

		[Fact]
		public void ExcelMatchTitlesWithRuleGenerator()
		{
			var processParams = new[] { new ReplaceGroupParameter() { MatchingPattern = "excel.exe" } };
			var urlParams = new[] { new ReplaceGroupParameter() { MatchingPattern = ".*" } };
			var titleParams = new[] { 
				new ReplaceGroupParameter() { MatchingPattern = excelPattern1 },
				new ReplaceGroupParameter() { MatchingPattern = excelPattern2, ReplaceGroupName = "title" },
				new ReplaceGroupParameter() { MatchingPattern = excelPattern3 },
			};
			var generator = new ReplaceGroupRuleGenerator(false, processParams, titleParams, urlParams);

			var rightDoc = "sdafdsagfasdgf. asdfads fsd gs (2012-09-03).xlsx[1]";
			var wrongDoc = "dafdsagfasdgf. asdfads fsd gs (2012-09-03).xlsx[1]";

			foreach (var ruleTitle in legacyExcelTitles)
			{
				var pattern = generator.GetRuleFromWindow(new DesktopWindow() { ProcessName = "excel.exe", Title = string.Format(ruleTitle, rightDoc), });
				Assert.True(pattern.IsRegex);
				Assert.True(pattern.IsEnabled);
				var matcher = new Regex(pattern.TitleRule);
				foreach (var title in legacyExcelTitles)
				{
					var rightTitle = string.Format(title, rightDoc);
					var wrongTitle = string.Format(title, wrongDoc);
					Assert.True(matcher.IsMatch(rightTitle), "Failed to match " + rightTitle + " with " + matcher);
					Assert.False(matcher.IsMatch(wrongTitle), "Matched '" + wrongTitle + "' but it shouldn't with " + matcher);
				}
			}
		}

		[Fact]
		public void LookaroundTests()
		{
			var testArr1 = legacyWordTitles;
			var testArr2 = legacyWordTitles.Select(n => new string(Enumerable.Repeat('a', n.Length).ToArray()));
			var regex = new Regex(" - ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
			var regexLook = new Regex("^((?! - ).)*$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
			//regexLook is 10 to 100 times slower (it:1000 it:10000)... that is why we have to negate regex sometimes

			bool res;

			res = true;
			foreach (var str in testArr1)
			{
				res &= regex.IsMatch(str);
			}
			Assert.True(res);
			res = false;
			foreach (var str in testArr2)
			{
				res |= regex.IsMatch(str);
			}
			Assert.False(res);


			res = true;
			foreach (var str in testArr1)
			{
				res &= !regexLook.IsMatch(str);
			}
			Assert.True(res);
			res = false;
			foreach (var str in testArr2)
			{
				res |= !regexLook.IsMatch(str);
			}
			Assert.False(res);


			var sw = Stopwatch.StartNew();

			for (int i = 0; i < 1000; i++)
			{
				res = true;
				foreach (var str in testArr1)
				{
					res &= regex.IsMatch(str);
				}
				res = false;
				foreach (var str in testArr2)
				{
					res |= regex.IsMatch(str);
				}
			}

			Console.WriteLine(sw.Elapsed.TotalMilliseconds);


			sw = Stopwatch.StartNew();

			for (int i = 0; i < 1000; i++)
			{
				res = true;
				foreach (var str in testArr1)
				{
					res &= !regexLook.IsMatch(str);
				}
				res = false;
				foreach (var str in testArr2)
				{
					res |= !regexLook.IsMatch(str);
				}
			}

			Console.WriteLine(sw.Elapsed.TotalMilliseconds);
		}
	}
}
