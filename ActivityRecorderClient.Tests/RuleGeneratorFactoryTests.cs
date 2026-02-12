using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Rules.Generation;
using Tct.ActivityRecorderClient.Serialization;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class RuleGeneratorFactoryTests
	{
		#region Titles
		public static readonly string[] WordTitles = new[] {
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
				//word 2013 titles
				"{0} [Írásvédett] [kompatibilis üzemmód] - Word",
				"{0} [Írásvédett] [kompatibilis üzemmód] - Word (A termékaktiválás nem sikerült)",
				"{0} [Read-Only] [Compatibility Mode] - Word",
				"{0} [Read-Only] [Compatibility Mode] - Word (Product Activation Failed)",
				"{0} (Védett nézet) (Utoljára a felhasználó mentette) - Word",
				"{0} (Védett nézet) (Utoljára a felhasználó mentette) - Word (A termékaktiválás nem sikerült)",
				"{0} (Védett nézet) - Word",
				"{0} (Védett nézet) - Word (A termékaktiválás nem sikerült)",
				"{0} (Protected View) - Word",
				"{0} (Protected View) - Word (Product Activation Failed)",
				//"{0} - Dokumentum [kompatibilis üzemmód] - Word", //we don't support this atm.
			};

		public static readonly string[] ExcelTitles = new[] {
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
				//excel 2013 titles
				"{0} - Excel",
				"{0} [Olvasásra]  [kompatibilis üzemmód] - Excel", //this might not exist at all...
				"{0}  [Olvasásra]  [kompatibilis üzemmód] - Excel",
				"{0}  [Repaired]  [Compatibility Mode] - Excel",
				"{0}  [kompatibilis üzemmód] - Excel",
				"{0}  [kompatibilis üzemmód] - Excel (A termékaktiválás nem sikerült)",
				"{0}  [Read-Only] - Excel",
				"{0}  [Read-Only] - Excel (Product Activation Failed)",
		};

		public static readonly string[] PowerPointTitles = new[] {
				"{0} - Microsoft PowerPoint",
				"{0}:2 - Microsoft PowerPoint",
				"{0} [Kompatibilis mód] - Microsoft PowerPoint (A termékaktiválás nem sikerült)",
				"{0} [Protected View] - Microsoft PowerPoint (Product Activation Failed)",
				"{0} [Read-Only] - Microsoft PowerPoint",
				"{0} [Írásvédett] - Microsoft PowerPoint",
				"{0} [Írásvédett] [Kompatibilis mód] - Microsoft PowerPoint",
				"{0} [Read-Only] [Compatibility Mode] - Microsoft PowerPoint",
				"{0} [Írásvédett] - PowerPoint",
				"{0} [Protected View] - PowerPoint",
				"Microsoft PowerPoint - [{0}]",
				"Microsoft PowerPoint - [{0}:2]",
				"Microsoft PowerPoint - [{0} [Írásvédett]]",
				"Microsoft PowerPoint - [{0} [Compatibility Mode]:2]",
				"Microsoft PowerPoint - [{0} [Read-Only] [Compatibility Mode]]",
				"Microsoft PowerPoint - [{0}  [Read-Only] [Compatibility Mode]]",
				"Microsoft PowerPoint - [{0} [Írásvédett] [Kompatibilitási mód]]",
				"Microsoft PowerPoint - [{0} [Utoljára a felhasználó mentette]]",
				"Microsoft PowerPoint - [{0} [Autosaved]]",
				"Microsoft PowerPoint (Próba) - [{0} [Kompatibilitási mód]]",
				"Microsoft PowerPoint kereskedelmi célokra nem használható - [{0} [Kompatibilitási mód]]",
				"PowerPoint előadói nézet – [{0}]",
				"PowerPoint előadói nézet – [{0} [Írásvédett] [Kompatibilitási mód]]",
				"PowerPoint Presenter View – [{0} [Read-Only] [Compatibility Mode]]",
				"PowerPoint Slide Show – [{0} [Read-Only] [Compatibility Mode]]",
				"PowerPoint-vetítés - [{0} [Írásvédett] [Kompatibilis mód]]",
				"PowerPoint-vetítés - [{0} [Írásvédett] [Kompatibilis mód]] - Microsoft PowerPoint",
				"Microsoft Office PowerPoint - [{0}]",
				"Microsoft Office PowerPoint - [{0} [Írásvédett] [Kompatibilitási mód]]",
				//"PowerPoint Slide Show – [c:\\valami\\dir\\{0} [Read-Only] [Compatibility Mode]]", //we don't support these atm.
				//"PowerPoint-vetítés - [c:\\valami\\dir\\{0} [Írásvédett] [Kompatibilis mód]]",
				//"Microsoft PowerPoint - [PowerPoint Slide Show - [{0}]]",
				//"Microsoft PowerPoint - [PowerPoint Slide Show - [{0} [Compatibility Mode]]]",
				//powerpoint 2013 titles
				"{0} - PowerPoint",
				"{0} - PowerPoint (A termékaktiválás nem sikerült)",
				"{0} - PowerPoint (Product Activation Failed)",
				"{0} [Kompatibilis üzemmód] - PowerPoint",
				"{0} [Read-Only] - PowerPoint",
				"{0} [Írásvédett] [Kompatibilis üzemmód] - PowerPoint",
				"{0} [Írásvédett] [Kompatibilis üzemmód] - PowerPoint (A termékaktiválás nem sikerült)",
				"{0} [Read-Only] [Compatibility Mode] - PowerPoint",
				"{0} [Read-Only] [Compatibility Mode] - PowerPoint (Product Activation Failed)",
		};

		public static readonly string[] IgnoredOfficeTitles = new[] {
			"Megnyitás - Microsoft Word",
			"Opening - Microsoft Word (Product Activation Failed)",
			"Megnyitás - Microsoft Excel",
			"Opening - Microsoft Excel",
			"Megnyitás - Microsoft PowerPoint",
			"Megnyitás - Microsoft PowerPoint (A termékaktiválás nem sikerült)",
			"Megnyitás - PowerPoint",
			"Opening - Microsoft PowerPoint",
			"Opening - Microsoft PowerPoint (Product Activation Failed)",
			//office 2013
			"Megnyitás - Excel",
			"Opening - Excel",
			"Megnyitás - PowerPoint",
			"Opening - PowerPoint",
			"Megnyitás - Word",
			"Opening - Word",
			};
		#endregion

		#region Default Generators
		private const string excelAsPrefixBeginPattern = @"Microsoft Excel(?<optBrac>\s\([\p{L}\s-]+\))?\s-\s";
		private const string excelAsPrefixEndPattern = @"(?<optNum>\s?\(\d{1,2}\)|\[\d{1,2}\])?(?<optExt>\.\p{L}{1,4})?(?:[:]\d+)?(?<optBrac>\s\s?\[[\p{L}\s-]+\])*$";
		private const string excelAsSuffixEndPattern = @"(?<optNum>\s?\(\d{1,2}\)|\[\d{1,2}\])?(?<optExt>\.\p{L}{1,4})?(?:[:]\d+)?(?<optBrac>\s\s?\[[\p{L}\s-]+\])*(?<optBrac>\s\([\p{L}\s-]+\))?\s-\sExcel(?<optBrac>\s\([\p{L}\s-]+\))?$";

		private const string powerPointAsPrefixBeginPattern = @"(?<optMs>Microsoft\s)?(?<optOf>Office\s)?PowerPoint(?<optText>.*?)\s[-\u2013]\s\[";
		private const string powerPointAsPrefixEndPattern = @"(?<optNum>\s?\(\d{1,2}\)|\[\d{1,2}\])?(?<optExt>\.\p{L}{1,4})?(?<optBrac>\s\s?\[[\p{L}\s-]+\])*(?:[:]\d+)?\](?<optMsE>\s-\sMicrosoft PowerPoint)?$";
		private const string powerPointAsSuffixEndPattern = @"(?<optNum>\s?\(\d{1,2}\)|\[\d{1,2}\])?(?<optExt>\.\p{L}{1,4})?((?<optPar>\s\([\p{L}\s-]+\))?(?<optBrac>\s\[[\p{L}\s-]+\])?)*(?:[:]\d+)?\s-\s(?<optMs>Microsoft\s)?PowerPoint(?<optParEnd>\s\([\p{L}\s-]+\))?$";

		private static readonly List<RuleGeneratorData> currentGenerators = new List<RuleGeneratorData>()
				{
					RuleGeneratorFactory.GetDataFromCreateParams(new IgnoreRuleGeneratorCreateParams(){
						IgnoreCase = true, 
						ProcessNamePattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^(winword|excel|powerpnt)[.]exe$", },
						TitlePattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  @"^(Opening|Megnyit\u00E1s)\s-\s(?:Microsoft\s)?(Word|Excel|PowerPoint)(?:\s\([\p{L}\s-]+\))?$" },
						UrlPattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^.*$", },
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams(){ 
						IgnoreCase = true, 
						ProcessNameParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^excel[.]exe$" },},
						TitleParams = new [] { 
							new ReplaceGroupParameter() { MatchingPattern =  "^(?(?=("+excelAsPrefixBeginPattern+"))("+excelAsPrefixBeginPattern},
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern =  excelAsPrefixEndPattern+ ")|("},
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern =  excelAsSuffixEndPattern+"))" },
						},
						UrlParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams(){ 
						IgnoreCase = true, 
						ProcessNameParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^winword[.]exe$" },},
						TitleParams = new [] { 
							new ReplaceGroupParameter() { MatchingPattern = "^" },
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern = @"(?<optNum>\s?\(\d{1,2}\)|\[\d{1,2}\])?(?<optExt>\.\p{L}{1,4})?((?<optPar>\s\([\p{L}\s-]+\))?(?<optBrac>\s\[[\p{L}\s-]+\])?)*(?:[:]\d+)?\s-(?<optMs>\sMicrosoft)?\sWord(?<optParEnd>\s\([\p{L}\s-]+\))?$" },
						},
						UrlParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams(){ 
						IgnoreCase = true, 
						ProcessNameParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^powerpnt[.]exe$" },},
						TitleParams = new [] { 
							new ReplaceGroupParameter() { MatchingPattern =  "^(?(?=("+powerPointAsPrefixBeginPattern+"))("+powerPointAsPrefixBeginPattern},
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern =  powerPointAsPrefixEndPattern+ ")|("},
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern =  powerPointAsSuffixEndPattern+"))" },
						},
						UrlParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new IgnoreRuleGeneratorCreateParams(){
						IgnoreCase = true, 
						ProcessNamePattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^(winword|excel|powerpnt)[.]exe$", },
						TitlePattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^.*$" },
						UrlPattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^.*$", },
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new SimpleRuleGeneratorCreateParams(){ IgnoreCase = true}),
				};
		#endregion

		#region ISYS-ON Generators
		private static readonly List<RuleGeneratorData> isysonSpecialGeneratos = new List<RuleGeneratorData>() //until first '-' MEW, first five chars for QAD, ignore versions (V2,V3) in word, excel
				{
					RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams(){ 
						IgnoreCase = true, 
						ProcessNameParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^putty[.]exe$" },},
						TitleParams = new [] { new ReplaceGroupParameter() { MatchingPattern =  "^.*$"},},
						UrlParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams(){ 
						IgnoreCase = true,
						ProcessNameParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^MEW32[.]exe$" },},
						TitleParams = new [] { 
							new ReplaceGroupParameter() { MatchingPattern =  @"^(?<first>[^-]+-)", ReplaceGroupName = "first" },
							new ReplaceGroupParameter() { MatchingPattern = ".*$"},
						},
						UrlParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams(){ 
						IgnoreCase = true,
						ProcessNameParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^QAD[.](Applications|Client)[.]exe$" },},
						TitleParams = new [] { 
							new ReplaceGroupParameter() { MatchingPattern =  @"^(?<first>.{5})", ReplaceGroupName = "first" },
							new ReplaceGroupParameter() { MatchingPattern = ".*$"},
						},
						UrlParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new IgnoreRuleGeneratorCreateParams(){
						IgnoreCase = true, 
						ProcessNamePattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^(winword|excel|powerpnt)[.]exe$", },
						TitlePattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  @"^(Opening|Megnyit\u00E1s)\s-\s(?:Microsoft\s)?(Word|Excel|PowerPoint)(?:\s\([\p{L}\s-]+\))?$" },
						UrlPattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^.*$", },
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams(){ 
						IgnoreCase = true, 
						ProcessNameParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^excel[.]exe$" },},
						TitleParams = new [] { 
							new ReplaceGroupParameter() { MatchingPattern =  "^(?(?=("+excelAsPrefixBeginPattern+"))("+excelAsPrefixBeginPattern},
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern =  @"(?<optVer>\s?V\d{1,})?" + excelAsPrefixEndPattern+ ")|("},
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern =  @"(?<optVer>\s?V\d{1,})?" + excelAsSuffixEndPattern+"))" },
						},
						UrlParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams(){ 
						IgnoreCase = true, 
						ProcessNameParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^winword[.]exe$" },},
						TitleParams = new [] { 
							new ReplaceGroupParameter() { MatchingPattern = "^" },
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern = @"(?<optVer>\s?V\d{1,})?(?<optNum>\s?\(\d{1,2}\)|\[\d{1,2}\])?(?<optExt>\.\p{L}{1,4})?((?<optPar>\s\([\p{L}\s-]+\))?(?<optBrac>\s\[[\p{L}\s-]+\])?)*(?:[:]\d+)?\s-(?<optMs>\sMicrosoft)?\sWord(?<optParEnd>\s\([\p{L}\s-]+\))?$" },
						},
						UrlParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams(){ 
						IgnoreCase = true, 
						ProcessNameParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^powerpnt[.]exe$" },},
						TitleParams = new [] { 
							new ReplaceGroupParameter() { MatchingPattern =  "^(?(?=("+powerPointAsPrefixBeginPattern+"))("+powerPointAsPrefixBeginPattern},
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern =  @"(?<optVer>\s?V\d{1,})?" + powerPointAsPrefixEndPattern+ ")|("},
							new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
							new ReplaceGroupParameter() { MatchingPattern =  @"(?<optVer>\s?V\d{1,})?" + powerPointAsSuffixEndPattern+"))" },
						},
						UrlParams = new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new IgnoreRuleGeneratorCreateParams(){
						IgnoreCase = true, 
						ProcessNamePattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^(winword|excel|powerpnt|MEW32)[.]exe$", },
						TitlePattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^.*$" },
						UrlPattern = new IgnoreRuleMatchParameter(){ MatchingPattern =  "^.*$", },
					}),
					RuleGeneratorFactory.GetDataFromCreateParams(new SimpleRuleGeneratorCreateParams(){ IgnoreCase = true}),
				};
		#endregion

		public static string GetCurrentRuleGeneratorData()
		{
			string dataStr;
			using (var stream = new MemoryStream())
			{
				XmlPersistenceManager<List<RuleGeneratorData>>.WriteToStream(stream, currentGenerators);
				dataStr = Encoding.UTF8.GetString(stream.ToArray());
			}
			return dataStr;
		}

		[Fact]
		public void PrintCurrentRuleGeneratorData()
		{
			Console.WriteLine(GetCurrentRuleGeneratorData());
		}

		[Fact]
		public void HelperForServerIsTheSame() //i.e. RuleGeneratorFactoryHelper.CurrentGenerators == GetCurrentRuleGeneratorData()
		{
			List<RuleGeneratorData> o1, o2;
			string s1, s2;
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(GetCurrentRuleGeneratorData())))
			{
				o1 = XmlPersistenceManager<List<RuleGeneratorData>>.ReadFromStream(stream);
			}
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(RuleGeneratorFactoryHelper.CurrentGenerators)))
			{
				o2 = XmlPersistenceManager<List<RuleGeneratorData>>.ReadFromStream(stream);
			}
			using (var stream = new MemoryStream())
			{
				XmlPersistenceManager<List<RuleGeneratorData>>.WriteToStream(stream, o1);
				s1 = Encoding.UTF8.GetString(stream.ToArray());
			}
			using (var stream = new MemoryStream())
			{
				XmlPersistenceManager<List<RuleGeneratorData>>.WriteToStream(stream, o2);
				s2 = Encoding.UTF8.GetString(stream.ToArray());
			}

			Assert.Equal(s1, s2);
		}

		[Fact]
		public void GetDataFromCtorDataNotNullSimple()
		{
			var data = RuleGeneratorFactory.GetDataFromCreateParams(new SimpleRuleGeneratorCreateParams() { IgnoreCase = true });
			Assert.NotNull(data);
		}

		[Fact]
		public void GetDataFromCtorDataNotNullIgnore()
		{
			var data = RuleGeneratorFactory.GetDataFromCreateParams(new IgnoreRuleGeneratorCreateParams()
			{
				IgnoreCase = true,
				ProcessNamePattern = new IgnoreRuleMatchParameter() { MatchingPattern = ".*" },
				TitlePattern = new IgnoreRuleMatchParameter() { MatchingPattern = ".*" },
				UrlPattern = new IgnoreRuleMatchParameter() { MatchingPattern = ".*" },
			});
			Assert.NotNull(data);
		}

		[Fact]
		public void GetDataFromCtorDataNotNullReplace()
		{
			var data = RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams()
			{
				IgnoreCase = true,
				ProcessNameParams = new[] { new ReplaceGroupParameter() { MatchingPattern = "(?<a>.*)", ReplaceGroupName = "a" } },
				TitleParams = new[] { new ReplaceGroupParameter() { MatchingPattern = ".*" } },
				UrlParams = new[] { new ReplaceGroupParameter() { MatchingPattern = ".*" } },
			});
			Assert.NotNull(data);
		}

		[Fact]
		public void CreateGeneratorFromDataSimple()
		{
			var data = new RuleGeneratorData() { Name = "SimpleRuleGenerator", Parameters = "{\"IgnoreCase\":true}", };
			var gen = RuleGeneratorFactory.CreateGeneratorFromData(data);
			Assert.NotNull(gen);
			Assert.True(gen is SimpleRuleGenerator);
			var rule = gen.GetRuleFromWindow(new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" });
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(false, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("b", rule.TitleRule);
			Assert.Equal("c", rule.UrlRule);
		}

		[Fact]
		public void CreateGeneratorFromDataIgnore()
		{
			var data = RuleGeneratorFactory.GetDataFromCreateParams(new IgnoreRuleGeneratorCreateParams()
			{
				IgnoreCase = true,
				ProcessNamePattern = new IgnoreRuleMatchParameter() { MatchingPattern = "^A$" },
				TitlePattern = new IgnoreRuleMatchParameter() { MatchingPattern = ".*" },
				UrlPattern = new IgnoreRuleMatchParameter() { MatchingPattern = ".*" },
			});
			Assert.NotNull(data);
			var gen = RuleGeneratorFactory.CreateGeneratorFromData(data);
			Assert.True(gen is IgnoreRuleGenerator);
			var rule = gen.GetRuleFromWindow(new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" });
			Assert.Equal(false, rule.IsEnabled);
		}

		[Fact]
		public void CreateGeneratorFromDataIgnoreDiffCase()
		{
			var data = RuleGeneratorFactory.GetDataFromCreateParams(new IgnoreRuleGeneratorCreateParams()
			{
				IgnoreCase = false,
				ProcessNamePattern = new IgnoreRuleMatchParameter() { MatchingPattern = "^A$" },
				TitlePattern = new IgnoreRuleMatchParameter() { MatchingPattern = ".*" },
				UrlPattern = new IgnoreRuleMatchParameter() { MatchingPattern = ".*" },
			});
			Assert.NotNull(data);
			var gen = RuleGeneratorFactory.CreateGeneratorFromData(data);
			Assert.True(gen is IgnoreRuleGenerator);
			var rule = gen.GetRuleFromWindow(new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" });
			Assert.Null(rule);
		}


		[Fact]
		public void CreateGeneratorFromDataIgnoreNegateMatch()
		{
			var data = RuleGeneratorFactory.GetDataFromCreateParams(new IgnoreRuleGeneratorCreateParams()
			{
				IgnoreCase = false,
				ProcessNamePattern = new IgnoreRuleMatchParameter() { MatchingPattern = "^aaaaaaaa$", NegateMatch = true },
				TitlePattern = new IgnoreRuleMatchParameter() { MatchingPattern = ".*" },
				UrlPattern = new IgnoreRuleMatchParameter() { MatchingPattern = ".*" },
			});
			Assert.NotNull(data);
			var gen = RuleGeneratorFactory.CreateGeneratorFromData(data);
			Assert.True(gen is IgnoreRuleGenerator);
			var rule = gen.GetRuleFromWindow(new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" });
			Assert.Equal(false, rule.IsEnabled);
		}

		[Fact]
		public void CreateGeneratorFromDataReplace()
		{
			var data = RuleGeneratorFactory.GetDataFromCreateParams(new ReplaceGroupRuleGeneratorCreateParams()
			{
				IgnoreCase = true,
				ProcessNameParams = new[] { new ReplaceGroupParameter() { MatchingPattern = "(?<a>.*)", ReplaceGroupName = "a" } },
				TitleParams = new[] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" } },
				UrlParams = new[] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" } },
			});
			Assert.NotNull(data);
			var gen = RuleGeneratorFactory.CreateGeneratorFromData(data);
			Assert.True(gen is ReplaceGroupRuleGenerator);
			var rule = gen.GetRuleFromWindow(new DesktopWindow() { ProcessName = "aaaa", Title = "bbbb", Url = "cccc" });
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(true, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("aaaa", rule.ProcessRule);
			Assert.Equal("^.*$", rule.TitleRule);
			Assert.Equal("^.*$", rule.UrlRule);
		}

		[Fact]
		public void TestCurrentGeneratorsWord()
		{
			var match1 = new DesktopWindow() { IsActive = true, ProcessName = "winword.exe", Title = "Távközlési adó miatti változások Prémium Üzleti Micro ügyfelek esetén (Read-Only) [Compatibility Mode] - Microsoft Word" };
			var match2 = new DesktopWindow() { IsActive = true, ProcessName = "winword.exe", Title = "Távközlési adó miatti változások Prémium Üzleti Micro ügyfelek esetén - Microsoft Word" };
			var ruleMatcher = GetRuleMatcherFromWindow(match1);
			Assert.True(ruleMatcher.IsMatch(match1));
			Assert.True(ruleMatcher.IsMatch(match2));
		}

		[Fact]
		public void TestCurrentGeneratorsExcel()
		{

			var match1 = new DesktopWindow() { IsActive = true, ProcessName = "excel.exe", Title = "Microsoft Excel - Költségvetés_GOP111_Konzorciumvezető (Janos Walter's conflicted copy 2012-06-20)_FINAL  [kompatibilis üzemmód]" };
			var match2 = new DesktopWindow() { IsActive = true, ProcessName = "excel.exe", Title = "Microsoft Excel - Költségvetés_GOP111_Konzorciumvezető (Janos Walter's conflicted copy 2012-06-20)_FINAL" };
			var ruleMatcher = GetRuleMatcherFromWindow(match1);
			Assert.True(ruleMatcher.IsMatch(match1));
			Assert.True(ruleMatcher.IsMatch(match2));
		}

		[Fact]
		public void TestCurrentGeneratorsExcelInvalidTitle()
		{
			var match1 = new DesktopWindow() { IsActive = true, ProcessName = "excel.exe", Title = "Open  [kompatibilis üzemmód]" };
			var ruleMatcher = GetRuleMatcherFromWindow(match1);
			Assert.False(ruleMatcher.Rule.IsEnabled);
		}

		[Fact]
		public void TestCurrentGeneratorsWordInvalidProcess()
		{
			var match1 = new DesktopWindow() { IsActive = true, ProcessName = "winwor2.exe", Title = "Távközlési adó miatti változások Prémium Üzleti Micro ügyfelek esetén (Read-Only) [Compatibility Mode] - Microsoft Word" };
			var match2 = new DesktopWindow() { IsActive = true, ProcessName = "winwor2.exe", Title = "Távközlési adó miatti változások Prémium Üzleti Micro ügyfelek esetén - Microsoft Word" };
			var ruleMatcher = GetRuleMatcherFromWindow(match1);
			Assert.True(ruleMatcher.IsMatch(match1));
			Assert.False(ruleMatcher.IsMatch(match2));
		}

		[Fact]
		public void TestCurrentGeneratorsWordAllTitles()
		{
			var rightDoc = "sdafdsagfasdgf. asdfads fsd gs (2012-09-03).docx[1]";
			var wrongDoc = "dafdsagfasdgf. asdfads fsd gs (2012-09-03).docx[1]";

			foreach (var ruleTitle in WordTitles)
			{
				var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "winword.exe", Title = string.Format(ruleTitle, rightDoc), });
				foreach (var title in WordTitles)
				{
					var rightTitle = string.Format(title, rightDoc);
					var wrongTitle = string.Format(title, wrongDoc);
					Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "winword.exe", Title = rightTitle }), "Failed to match " + rightTitle + " with " + matcher);
					Assert.False(matcher.IsMatch(new DesktopWindow() { ProcessName = "winword.exe", Title = wrongTitle }), "Matched '" + wrongTitle + "' but it shouldn't with " + matcher);
				}
			}
		}

		[Fact]
		public void TestCurrentGeneratorExcelAllTitles()
		{
			var rightDoc = "sdafdsagfasdgf. asdfads fsd gs (2012-09-03).xlsx[1]";
			var wrongDoc = "dafdsagfasdgf. asdfads fsd gs (2012-09-03).xlsx[1]";

			foreach (var ruleTitle in ExcelTitles)
			{
				var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "excel.exe", Title = string.Format(ruleTitle, rightDoc), });
				foreach (var title in ExcelTitles)
				{
					var rightTitle = string.Format(title, rightDoc);
					var wrongTitle = string.Format(title, wrongDoc);
					Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "excel.exe", Title = rightTitle }), "Failed to match " + rightTitle + " with " + matcher);
					Assert.False(matcher.IsMatch(new DesktopWindow() { ProcessName = "excel.exe", Title = wrongTitle }), "Matched '" + wrongTitle + "' but it shouldn't with " + matcher);
				}
			}
		}

		[Fact]
		public void TestCurrentGeneratorPowerPntAllTitles()
		{
			var rightDoc = "sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[1].pptx";
			var wrongDoc = "dafdsagfasdgf. asdfads fsd gs (2012-09-03)[1].pptx";

			foreach (var ruleTitle in PowerPointTitles)
			{
				var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "powerpnt.exe", Title = string.Format(ruleTitle, rightDoc), });
				foreach (var title in PowerPointTitles)
				{
					var rightTitle = string.Format(title, rightDoc);
					var wrongTitle = string.Format(title, wrongDoc);
					Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "powerpnt.exe", Title = rightTitle }), "Failed to match " + rightTitle + " with " + matcher);
					Assert.False(matcher.IsMatch(new DesktopWindow() { ProcessName = "powerpnt.exe", Title = wrongTitle }), "Matched '" + wrongTitle + "' but it shouldn't with " + matcher);
				}
			}
		}

		[Fact]
		public void TestCurrentGeneratorIgnoredTitles()
		{
			foreach (var process in new[] { "winword.exe", "excel.exe", "powerpnt.exe" }) //we don't care which
			{
				foreach (var ruleTitle in IgnoredOfficeTitles)
				{
					var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = process, Title = ruleTitle, });
					Assert.False(matcher.IsMatch(new DesktopWindow() { ProcessName = process, Title = ruleTitle }), "Matched '" + ruleTitle + "' but it shouldn't with " + matcher);
				}
			}
		}

		[Fact]
		public void OneSampleXml()
		{
			var t0 = "TBS_jut (2).xlsx";
			var m = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "excel.exe", Title = t0 });
			Assert.False(m.Rule.IsEnabled);

			var t1 = "Microsoft Excel - TBS_jut (2).xlsx  [Védett nézet]";
			var t2 = "Microsoft Excel - TBS_jut (2).xlsx  [Olvasásra]";

			var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "excel.exe", Title = t1 });
			Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "excel.exe", Title = t2 }));
		}

		[Fact]
		public void TestCurrentGeneratorsWordAllTitlesExtAndVerIgnored()
		{
			var sameFiles = new[] { 
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03).docx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03).doc",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (1).docx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (1).doc",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (1)",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)(2).docx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)(2).doc",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)(2)",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[3].docx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[3].doc",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[3]",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (11).odt",
			};

			foreach (var ruleTitle in WordTitles)
			{
				var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "winword.exe", Title = string.Format(ruleTitle, sameFiles[0]), });
				foreach (var title in WordTitles)
				{
					foreach (var sameFile in sameFiles)
					{
						Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "winword.exe", Title = string.Format(title, sameFile) }), "Failed to match " + string.Format(title, sameFile) + " with " + matcher + " " + ruleTitle);
					}
				}
			}
		}

		[Fact]
		public void TestCurrentGeneratorsExcelAllTitlesExtAndVerIgnored()
		{
			var sameFiles = new[] { 
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03).xlsx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03).xls",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (1).xlsx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (1).xls",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (1)",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)(2).xlsx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)(2).xls",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)(2)",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[3].xlsx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[3].xls",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[3]",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (11).odt",
			};

			foreach (var ruleTitle in ExcelTitles)
			{
				var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "excel.exe", Title = string.Format(ruleTitle, sameFiles[0]), });
				foreach (var title in ExcelTitles)
				{
					foreach (var sameFile in sameFiles)
					{
						Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "excel.exe", Title = string.Format(title, sameFile) }), "Failed to match " + string.Format(title, sameFile) + " with " + matcher + " " + ruleTitle);
					}
				}
			}
		}

		[Fact]
		public void TestCurrentGeneratorsPowerPntAllTitlesExtAndVerIgnored()
		{
			var sameFiles = new[] { 
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03).pptx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03).ppt",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (1).pptx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (1).ppt",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (1)",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)(2).pptx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)(2).ppt",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)(2)",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[3].pptx",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[3].ppt",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03)[3]",
				"sdafdsagfasdgf. asdfads fsd gs (2012-09-03) (11).odt",
			};

			foreach (var ruleTitle in PowerPointTitles)
			{
				var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "powerpnt.exe", Title = string.Format(ruleTitle, sameFiles[0]), });
				foreach (var title in PowerPointTitles)
				{
					foreach (var sameFile in sameFiles)
					{
						Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "powerpnt.exe", Title = string.Format(title, sameFile) }), "Failed to match " + string.Format(title, sameFile) + " with " + matcher + " " + ruleTitle);
					}
				}
			}
		}

		[Fact]
		public void ISysOnPuttyTitleDoesntMatter()
		{
			var t1 = "12345";
			var t2 = "asdf";

			var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "putty.exe", Title = t1 }, isysonSpecialGeneratos);
			Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "putty.exe", Title = t2 }));
		}

		[Fact]
		public void ISysOnQadMathOnlyFirstFiveChars()
		{
			var t1 = "12345asdsfsdf";
			var t2 = "12345gfhjfghjd";

			var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "QAD.Client.exe", Title = t1 }, isysonSpecialGeneratos);
			Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "QAD.Client.exe", Title = t2 }));
		}

		[Fact]
		public void ISysOnExcelVersionIgnored()
		{
			var t1 = "Microsoft Excel - TBS_jut.xlsx  [Védett nézet]";
			var t2 = "Microsoft Excel - TBS_jut V2.xlsx  [Olvasásra]";

			var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "excel.exe", Title = t1 }, isysonSpecialGeneratos);
			Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "excel.exe", Title = t2 }));
		}

		[Fact]
		public void ISysOnWordVersionIgnored()
		{
			var t1 = "xysdfsdfdsf.doc [Írásvédett] [kompatibilis mód] - Microsoft Word";
			var t2 = "xysdfsdfdsf V2 (Minta) - Microsoft Word (Product Activation Failed)";

			var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "winword.exe", Title = t1 }, isysonSpecialGeneratos);
			Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "winword.exe", Title = t2 }));
		}

		[Fact]
		public void ISysOnMewMatchUntilFirstDash()
		{
			var t1 = "qwe - [qwewqe]";
			var t2 = "qwe - [234234324]";

			var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "MEW32.exe", Title = t1 }, isysonSpecialGeneratos);
			Assert.True(matcher.IsMatch(new DesktopWindow() { ProcessName = "MEW32.exe", Title = t2 }));
		}

		[Fact]
		public void ISysOnMewMatchUntilFirstDashFalse()
		{
			var t1 = "qwe - [qwewqe]";
			var t2 = "qke - [qwewqe]";

			var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "MEW32.exe", Title = t1 }, isysonSpecialGeneratos);
			Assert.False(matcher.IsMatch(new DesktopWindow() { ProcessName = "MEW32.exe", Title = t2 }));
		}

		[Fact]
		public void ISysOnMewMatchUntilFirstDashFlase2()
		{
			var t1 = "qwe";
			var t2 = "qwe324";

			var matcher = GetRuleMatcherFromWindow(new DesktopWindow() { ProcessName = "MEW32.exe", Title = t1 }, isysonSpecialGeneratos);
			Assert.False(matcher.IsMatch(new DesktopWindow() { ProcessName = "MEW32.exe", Title = t2 }));
		}

		private RuleMatcher<IRule> GetRuleMatcherFromWindow(DesktopWindow window)
		{
			return GetRuleMatcherFromWindow(window, currentGenerators);
		}

		private RuleMatcher<IRule> GetRuleMatcherFromWindow(DesktopWindow window, IEnumerable<RuleGeneratorData> generators)
		{
			return generators
				.Select(n => RuleGeneratorFactory.CreateGeneratorFromData(n).GetRuleFromWindow(window))
				.Where(n => n != null)
				.Select(n => new RuleMatcher<IRule>(n))
				.FirstOrDefault();
		}
	}
}
