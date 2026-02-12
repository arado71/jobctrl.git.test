using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Rules;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class RuleTemplateTests
	{
		private static ClientMenuLookup GetSimpleMenu()
		{
			return new ClientMenuLookup()
			{
				ClientMenu = new ClientMenu()
				{
					Works = new List<WorkData>()
					{
						new WorkData() { Id = 1, Name = "Munka 1" },
						new WorkData() { Id = 2, Name = "Munka 2" },
					},
				}
			};
		}

		private static ClientMenuLookup GetDetailedMenu()
		{
			return new ClientMenuLookup()
			{
				ClientMenu = new ClientMenu()
				{
					Works = new List<WorkData>()
						{
							new WorkData() { ProjectId = 4, Name = "Proj$.e4", Children = new List<WorkData> ()
							{
								new WorkData() { Id = 1, Name = "Munka 1", TaxId = "ta" },
								new WorkData() { Id = 2, Name = "Munka 2", TaxId = "ta" },
							}},
							new WorkData() { ProjectId = 5, Name = "Proj$.e5", Children = new List<WorkData> ()
							{
								new WorkData() { Id = 11, Name = "Munka 11", TaxId = "ta" },
								new WorkData() { Id = 21, Name = "Munka 21", TaxId = "ta" },
							}},
							new WorkData() {ProjectId = 6, Name = "Project1", Children = new List<WorkData>()
							{
								new WorkData() { Id = 31, Name = "Task1", TemplateRegex = "(alma)|(szilva)"},
								new WorkData() { Id = 32, Name = "Task1", TemplateRegex = "(kifli)|(zsemle)"},
							}},
						},
				}
			};
		}

		[Fact]
		public void WorkTemplate()
		{
			var rules = WorkChangingRuleFactory.CreateFrom(new WorkDetectorRule()
				{
					IsEnabled = true,
					ProcessRule = "$WorkName$.exe",
					TitleRule = "*",
					RuleType = WorkDetectorRuleType.TempStartWorkTemplate,
				}, GetSimpleMenu()).ToList();
			Assert.Equal(2, rules.Count);
			Assert.Equal("Munka 1.exe", rules[0].ProcessRule);
			Assert.Equal("Munka 2.exe", rules[1].ProcessRule);
			Assert.True(rules.All(n => n.TitleRule == "*"));
		}

		[Fact]
		public void WorkTemplatePlugin()
		{
			var rule = new WorkDetectorRule()
			{
				IsEnabled = true,
				IsRegex = true,
				ProcessRule = "(word|excel|powerpnt)",
				TitleRule = ".*",
				RuleType = WorkDetectorRuleType.TempStartWorkTemplate,
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>() { { "JobCTRL.Office", new Dictionary<string, string> { { "DocumentPath", @"^Z:\\Óbuda-Újlak Zrt\\1.Aktív projektek\\$WorkName$" } } } },
			};
			var rules = WorkChangingRuleFactory.CreateFrom(rule, GetSimpleMenu()).ToList();
			Assert.Equal(2, rules.Count);
			Assert.Equal(new CaptureExtensionKey("JobCTRL.Office", "DocumentPath"), rules[0].ExtensionRules.First().Key);
			Assert.Equal(@"^Z:\\Óbuda-Újlak Zrt\\1.Aktív projektek\\" + Regex.Escape("Munka 1"), rules[0].ExtensionRules.First().Value);
			Assert.Equal(new CaptureExtensionKey("JobCTRL.Office", "DocumentPath"), rules[1].ExtensionRules.First().Key);
			Assert.Equal(@"^Z:\\Óbuda-Újlak Zrt\\1.Aktív projektek\\Munka\ 2", rules[1].ExtensionRules.First().Value);
			Assert.True(rules.All(n => n.TitleRule == ".*"));
		}

		[Fact]
		public void WorkTemplateChild()
		{
			var rules = WorkChangingRuleFactory.CreateFrom(new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "P.exe",
				TitleRule = "*",
				RuleType = WorkDetectorRuleType.TempStartWorkTemplate,
				Children = new List<WindowRule>()
				{
					new WindowRule() { ProcessRule = "$WorkName$.exe", TitleRule = "*", IsEnabled = true, },
				},
			}, GetSimpleMenu()).ToList();
			Assert.Equal(2, rules.Count);
			Assert.Equal("Munka 1.exe", rules[0].Children.First().ProcessRule);
			Assert.Equal("Munka 2.exe", rules[1].Children.First().ProcessRule);
			Assert.True(rules.All(n => n.ProcessRule == "P.exe"));
			Assert.True(rules.All(n => n.TitleRule == "*"));
			Assert.True(rules.All(n => n.Children.First().TitleRule == "*"));
		}

		[Fact]
		public void WorkTemplateChildDisabled()
		{
			var rules = WorkChangingRuleFactory.CreateFrom(new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "P.exe",
				TitleRule = "*",
				RuleType = WorkDetectorRuleType.TempStartWorkTemplate,
				Children = new List<WindowRule>()
				{
					new WindowRule() { ProcessRule = "$WorkName$.exe", TitleRule = "*", IsEnabled = false, },
				},
			}, GetSimpleMenu()).ToList();
			Assert.Equal(0, rules.Count);
		}

		[Fact]
		public void ProjectTemplate()
		{
			var rules = WorkChangingRuleFactory.CreateFrom(new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "valami.exe",
				TitleRule = "$ProjectName$",
				IsRegex = true,
				WorkSelector = new WorkSelector() { IsRegex = true, Rule = "Munka 1.*_ta", TemplateText = "$WorkName$_$TaxId$" },
				RuleType = WorkDetectorRuleType.TempStartProjectTemplate,
			}, GetDetailedMenu()).ToList();
			Assert.Equal(2, rules.Count);
			Assert.Equal(Regex.Escape("Proj$.e4"), rules[0].TitleRule);
			Assert.Equal(Regex.Escape("Proj$.e5"), rules[1].TitleRule);
			Assert.Equal(1, rules[0].RelatedId);
			Assert.Equal(11, rules[1].RelatedId);
			Assert.True(rules.All(n => n.ProcessRule == "valami.exe"));
		}

		[Fact]
		public void ProjectTemplateChild()
		{
			var rules = WorkChangingRuleFactory.CreateFrom(new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "valami.exe",
				TitleRule = "XYZ",
				IsRegex = true,
				WorkSelector = new WorkSelector() { IsRegex = true, Rule = "Munka 1.*_ta", TemplateText = "$WorkName$_$TaxId$" },
				RuleType = WorkDetectorRuleType.TempStartProjectTemplate,
				Children = new List<WindowRule>()
				{
					new WindowRule() { ProcessRule = "masik.exe", TitleRule = "$ProjectName$", IsRegex = true, IsEnabled= true, }
				},
			}, GetDetailedMenu()).ToList();
			Assert.Equal(2, rules.Count);
			Assert.Equal(Regex.Escape("Proj$.e4"), rules[0].Children.First().TitleRule);
			Assert.Equal(Regex.Escape("Proj$.e5"), rules[1].Children.First().TitleRule);
			Assert.Equal(true, rules[0].Children.First().IsRegex);
			Assert.Equal(true, rules[1].Children.First().IsRegex);
			Assert.Equal(1, rules[0].RelatedId);
			Assert.Equal(11, rules[1].RelatedId);
			Assert.True(rules.All(n => n.Children.First().ProcessRule == "masik.exe"));
			Assert.True(rules.All(n => n.ProcessRule == "valami.exe"));
			Assert.True(rules.All(n => n.TitleRule == "XYZ"));
		}

		[Fact]
		public void ProjectTemplateChildNonRegex()
		{
			var rules = WorkChangingRuleFactory.CreateFrom(new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "valami.exe",
				TitleRule = "XYZ",
				IsRegex = false,
				WorkSelector = new WorkSelector() { IsRegex = false, Rule = "Munka 1*_ta", TemplateText = "$WorkName$_$TaxId$" },
				RuleType = WorkDetectorRuleType.TempStartProjectTemplate,
				Children = new List<WindowRule>()
				{
					new WindowRule() { ProcessRule = "masik.exe", TitleRule = "$ProjectName$", IsRegex = false, IsEnabled= true, }
				},
			}, GetDetailedMenu()).ToList();
			Assert.Equal(2, rules.Count);
			Assert.Equal("Proj$.e4", rules[0].Children.First().TitleRule);
			Assert.Equal("Proj$.e5", rules[1].Children.First().TitleRule);
			Assert.Equal(false, rules[0].Children.First().IsRegex);
			Assert.Equal(false, rules[1].Children.First().IsRegex);
			Assert.Equal(1, rules[0].RelatedId);
			Assert.Equal(11, rules[1].RelatedId);
			Assert.True(rules.All(n => n.Children.First().ProcessRule == "masik.exe"));
			Assert.True(rules.All(n => n.ProcessRule == "valami.exe"));
			Assert.True(rules.All(n => n.TitleRule == "XYZ"));
		}

		[Fact]
		public void ProjectTemplateChildDisabled()
		{
			var rules = WorkChangingRuleFactory.CreateFrom(new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "valami.exe",
				TitleRule = "XYZ",
				IsRegex = true,
				WorkSelector = new WorkSelector() { IsRegex = true, Rule = "Munka 1.*_ta", TemplateText = "$WorkName$_$TaxId$" },
				RuleType = WorkDetectorRuleType.TempStartProjectTemplate,
				Children = new List<WindowRule>()
				{
					new WindowRule() { ProcessRule = "masik.exe", TitleRule = "$ProjectName$", IsRegex = true, IsEnabled= false, }
				},
			}, GetDetailedMenu()).ToList();
			Assert.Equal(0, rules.Count);
		}

		[Fact]
		public void CloneWindowRuleInWorkChangingRuleFactory()
		{
			//Arrange
			WorkDetectorRule wdr;
			var rules = WorkChangingRuleFactory.CreateFrom(wdr = new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "valami.exe",
				TitleRule = "XYZ",
				IsRegex = true,
				WorkSelector = new WorkSelector() { IsRegex = true, Rule = "Munka 1.*_ta", TemplateText = "$WorkName$_$TaxId$" },
				RuleType = WorkDetectorRuleType.TempStartProjectTemplate,
				Children = new List<WindowRule>()
				{
					new WindowRule() {
						ProcessRule = "masik.exe", TitleRule = "$ProjectName$", IsRegex = true, IsEnabled= true, 
						ExtensionRulesByIdByKey = new Dictionary<string,Dictionary<string,string>>() { {"JobCTRL.Test", new	Dictionary<string,string>() {{"Key", "Rule"},}}, }
					}
				},
			}, GetDetailedMenu()).ToList();
			Assert.Equal(2, rules.Count);

			//Act
			wdr.Children[0].ProcessRule = "masik2.exe";
			wdr.Children[0].ExtensionRulesByIdByKey["JobCTRL.Test"]["Key"] = "Rule2";

			//Assert
			Assert.Equal(2, rules.Count);
			Assert.True(ReferenceEquals(wdr, rules[0].OriginalRule));
			Assert.True(ReferenceEquals(wdr, rules[1].OriginalRule));
			Assert.False(ReferenceEquals(wdr.Children[0].ExtensionRulesByIdByKey, ((WindowRule)rules[0].Children.First()).ExtensionRulesByIdByKey));
			Assert.False(ReferenceEquals(wdr.Children[0].ExtensionRulesByIdByKey, ((WindowRule)rules[1].Children.First()).ExtensionRulesByIdByKey));
			Assert.Equal("Rule", ((WindowRule)rules[0].Children.First()).ExtensionRulesByIdByKey["JobCTRL.Test"]["Key"]);
			Assert.Equal("Rule", ((WindowRule)rules[1].Children.First()).ExtensionRulesByIdByKey["JobCTRL.Test"]["Key"]);
			Assert.True(rules.All(n => n.Children.First().ProcessRule == "masik.exe"));
			Assert.True(rules.All(n => n.Children.First().ExtensionRules.Where(m => m.Key.Equals(new CaptureExtensionKey("JobCTRL.Test", "Key"))).Single().Value == "Rule"));
		}

		[Fact]
		public void CloneWindowRuleInWorkChangingRuleFactoryNotTemplate()
		{
			//Arrange
			WorkDetectorRule wdr;
			var rules = WorkChangingRuleFactory.CreateFrom(wdr = new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "valami.exe",
				TitleRule = "XYZ",
				IsRegex = true,
				WorkSelector = new WorkSelector() { IsRegex = true, Rule = "Munka 1.*_ta", TemplateText = "$WorkName$_$TaxId$" },
				RuleType = WorkDetectorRuleType.TempStartWork,
				Children = new List<WindowRule>()
				{
					new WindowRule() {
						ProcessRule = "masik.exe", TitleRule = "ProjectName", IsRegex = true, IsEnabled= true, 
						ExtensionRulesByIdByKey = new Dictionary<string,Dictionary<string,string>>() { {"JobCTRL.Test", new	Dictionary<string,string>() {{"Key", "Rule"},}}, }
					}
				},
			}, GetDetailedMenu()).ToList();
			Assert.Equal(1, rules.Count);

			//Act
			wdr.Children[0].ProcessRule = "masik2.exe";
			wdr.Children[0].ExtensionRulesByIdByKey["JobCTRL.Test"]["Key"] = "Rule2";

			//Assert
			Assert.Equal(1, rules.Count);
			Assert.True(ReferenceEquals(wdr, rules[0].OriginalRule));
			Assert.False(ReferenceEquals(wdr.Children[0].ExtensionRulesByIdByKey, ((WindowRule)rules[0].Children.First()).ExtensionRulesByIdByKey));
			Assert.Equal("Rule", ((WindowRule)rules[0].Children.First()).ExtensionRulesByIdByKey["JobCTRL.Test"]["Key"]);
			Assert.True(rules.All(n => n.Children.First().ProcessRule == "masik.exe"));
			Assert.True(rules.All(n => n.Children.First().ExtensionRules.Where(m => m.Key.Equals(new CaptureExtensionKey("JobCTRL.Test", "Key"))).Single().Value == "Rule"));
		}

		[Fact]
		public void CloneWindowRule()
		{
			//Arrange
			var orig = new WindowRule()
			{
				ProcessRule = "masik.exe",
				TitleRule = "$ProjectName$",
				IsRegex = true,
				IsEnabled = true,
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>() { { "JobCTRL.Test", new Dictionary<string, string>() { { "Key", "Rule" }, } }, }
			};

			//Act
			var clone = orig.Clone();
			orig.ProcessRule = "masik2.exe";
			orig.ExtensionRulesByIdByKey["JobCTRL.Test"]["Key"] = "Rule2";

			//Assert
			Assert.True(clone.ProcessRule == "masik.exe");
			Assert.True(clone.ExtensionRules.Where(m => m.Key.Equals(new CaptureExtensionKey("JobCTRL.Test", "Key"))).Single().Value == "Rule");
		}

		[Fact]
		public void TemplateRegexTest()
		{
			var dw1 = new DesktopWindow()
			{
				ProcessName = "valami.exe",
				Title = "Itt terem a szilva!",
				IsActive = true,
			};
			var desktopCapture1 = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw1 }, };
			var dw2 = new DesktopWindow()
			{
				ProcessName = "valami.exe",
				Title = "Itt van egy zsemle szilva!",
				IsActive = true,
			};
			var desktopCapture2 = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw2 }, };
			var dw3 = new DesktopWindow()
			{
				ProcessName = "valami.exe",
				Title = "Itt meg egy kifli meg egy alma!",
				IsActive = true,
			};
			var desktopCapture3 = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw3 }, };
			var rules = WorkChangingRuleFactory.CreateFrom(new WorkDetectorRule()
			{
				IsEnabled = true,
				IsRegex = true,
				ProcessRule = "valami.exe",
				TitleRule = "$TemplateRegex$",
				RuleType = WorkDetectorRuleType.TempStartWorkTemplate,
			}, GetDetailedMenu()).ToList();
			Assert.Equal(2, rules.Count);
			Assert.Equal("valami.exe", rules[0].ProcessRule);
			Assert.Equal("valami.exe", rules[1].ProcessRule);
			DesktopWindow matchedWindow;
			Assert.True(new RuleMatcher<WorkChangingRule>((WorkChangingRule)rules[0]).IsMatch(desktopCapture1, out matchedWindow));
			Assert.True(new RuleMatcher<WorkChangingRule>((WorkChangingRule)rules[1]).IsMatch(desktopCapture2, out matchedWindow));
			Assert.True(new RuleMatcher<WorkChangingRule>((WorkChangingRule)rules[0]).IsMatch(desktopCapture3, out matchedWindow));
			Assert.True(new RuleMatcher<WorkChangingRule>((WorkChangingRule)rules[1]).IsMatch(desktopCapture3, out matchedWindow));
		}
	}
}
