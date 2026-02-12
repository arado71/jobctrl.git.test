using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Rules;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class RuleMatcherFormatterTests
	{
		[Fact]
		public void ComplexRuleMatchingAndFormatting()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "IEXPLORE.EXE",
				Title = "xFLOWer: Ügyek keresése - LÁZS ANITA - xFLOWer - Groupama Garancia Biztosító Zrt. - Windows Internet Explorer",
				Url = "http://xflowerw3.garancia.hu/xflower/index.php?lang=hu&sid=evp78tsjkdudcf90tmheh9iqf0&page=33&tab=2590;2530&focus=1",
				IsActive = true,
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw }, };
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "Status"), "Idle");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubject"), "Kimenő levelek iktatása");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubjectState"), "Folyamatban");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				RuleType = WorkDetectorRuleType.TempStartOrAssignProjectAndWork,
				IgnoreCase = true,
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "iexplore.exe",
				TitleRule = ".*",
				UrlRule = "^https?://xflowerw3(.garancia.hu)?/.+page=33.+$",
				IsRegex = true,
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
				{
				    {"JobCTRL.IE", new Dictionary<string, string>() {
						{"1_BarCode", "(?<tag2>.+)"},
						{"1_CheckSubject", "(?<tag1>.+)"},
						{"1_CheckSubjectState", "(?<desc>.+)"},
				    }}
				},
				FormattedNamedGroups = new Dictionary<string, string>()
				{
					{"workkey", "{tag1} - {tag2}"}
				}
			});

			DesktopWindow matchedWindow;
			var matcher = new RuleMatcherFormatter<IWorkChangingRule>(matchRule);
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));
			var res = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameDescription);
			Assert.True(res.Count == 2);
			Assert.Equal("Kimenő levelek iktatása - 12345679812", res[WorkDetectorRule.GroupNameWorkKey]);
			Assert.Equal("Folyamatban", res[WorkDetectorRule.GroupNameDescription]);
			Assert.DoesNotThrow(() => matcher.Rule.OriginalRule.ValidateAndGetMatchers(new Tct.ActivityRecorderClient.Menu.ClientMenuLookup()));

			//WorkDetector
			var assignData = new Tct.ActivityRecorderClient.Capturing.Core.WorkDetector().GetAssignProjectAndWorkData(desktopCapture, matcher);
			Assert.NotNull(assignData);
			Assert.Equal("Kimenő levelek iktatása - 12345679812", assignData.Composite.WorkKey);
			Assert.Equal(0, assignData.Composite.ProjectKeys.Count);
			Assert.Equal("Folyamatban", assignData.Composite.Description);
		}

		[Fact]
		public void InvalidRuleMatchingAndFormatting()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "IEXPLORE.EXE",
				Title = "xFLOWer: Ügyek keresése - LÁZS ANITA - xFLOWer - Groupama Garancia Biztosító Zrt. - Windows Internet Explorer",
				Url = "http://xflowerw3.garancia.hu/xflower/index.php?lang=hu&sid=evp78tsjkdudcf90tmheh9iqf0&page=33&tab=2590;2530&focus=1",
				IsActive = true,
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw }, };
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "Status"), "Idle");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubject"), "Kimenő levelek iktatása");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubjectState"), "Folyamatban");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				RuleType = WorkDetectorRuleType.TempStartOrAssignProjectAndWork,
				IgnoreCase = true,
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "iexplore.exe",
				TitleRule = ".*",
				UrlRule = "^https?://xflowerw3(.garancia.hu)?/.+page=33.+$",
				IsRegex = true,
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
				{
				    {"JobCTRL.IE", new Dictionary<string, string>() {
						{"1_BarCode", "(?<tag2>.+)"},
						{"1_CheckSubject", "(?<tag1>.+)"},
						{"1_CheckSubjectState", "(?<desc>.+)"},
				    }}
				},
				FormattedNamedGroups = new Dictionary<string, string>()
				{
					{"workkey", "{tag1} - {tag2"}
				}
			});

			DesktopWindow matchedWindow;
			var matcher = new RuleMatcherFormatter<IWorkChangingRule>(matchRule);
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));
			var res = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameDescription);
			Assert.True(res.Count == 1);
			Assert.Equal("Folyamatban", res[WorkDetectorRule.GroupNameDescription]);
			Assert.Throws<Exception>(() => matcher.Rule.OriginalRule.ValidateAndGetMatchers(new Tct.ActivityRecorderClient.Menu.ClientMenuLookup()));

			//WorkDetector
			var assignData = new Tct.ActivityRecorderClient.Capturing.Core.WorkDetector().GetAssignProjectAndWorkData(desktopCapture, matcher);
			Assert.Null(assignData);
		}

		[Fact]
		public void MoreComplexRuleMatchingAndFormatting()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "IEXPLORE.EXE",
				Title = "xFLOWer: Ügyek keresése - LÁZS ANITA - xFLOWer - Groupama Garancia Biztosító Zrt. - Windows Internet Explorer",
				Url = "http://xflowerw3.garancia.hu/xflower/index.php?lang=hu&sid=evp78tsjkdudcf90tmheh9iqf0&page=33&tab=2590;2530&focus=1",
				IsActive = true,
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw }, };
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "Status"), "Idle");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubject"), "Kimenő levelek iktatása");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubjectState"), "Folyamatban");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				RuleType = WorkDetectorRuleType.TempStartOrAssignWork,
				IgnoreCase = true,
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "(?<empty>.*)iexplore.exe",
				TitleRule = ".*",
				UrlRule = "^https?://xflowerw3(.garancia.hu)?/.+page=33.+$",
				IsRegex = true,
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
				{
				    {"JobCTRL.IE", new Dictionary<string, string>() {
						{"1_BarCode", "(?<tag2>.+)"},
						{"1_CheckSubject", "(?<tag1>.+)"},
						{"1_CheckSubjectState", "(?<desc>.+)"},
				    }}
				},
				FormattedNamedGroups = new Dictionary<string, string>()
				{
					{"workkey", "{empty}{tag1} - {tag2}"},
					{"workname", " {tag2} "},
				}
			});

			DesktopWindow matchedWindow;
			var matcher = new RuleMatcherFormatter<IWorkChangingRule>(matchRule);
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));
			var res = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameWorkName, WorkDetectorRule.GroupNameDescription);
			Assert.True(res.Count == 3);
			Assert.Equal("Kimenő levelek iktatása - 12345679812", res[WorkDetectorRule.GroupNameWorkKey]);
			Assert.Equal(" 12345679812 ", res[WorkDetectorRule.GroupNameWorkName]);
			Assert.Equal("Folyamatban", res[WorkDetectorRule.GroupNameDescription]);
			Assert.DoesNotThrow(() => matcher.Rule.OriginalRule.ValidateAndGetMatchers(new Tct.ActivityRecorderClient.Menu.ClientMenuLookup()));

			//WorkDetector
			var assignData = new Tct.ActivityRecorderClient.Capturing.Core.WorkDetector().GetAssignWorkData(desktopCapture, matcher);
			Assert.NotNull(assignData);
			Assert.Equal("Kimenő levelek iktatása - 12345679812", assignData.Work.WorkKey);
			Assert.Equal(" 12345679812 ", assignData.Work.WorkName);
			Assert.Equal("Folyamatban", assignData.Work.Description);
		}

		[Fact]
		public void UnmatchedGroupHandling()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "test.EXE",
				Title = "x",
				Url = "fallback",
				IsActive = true,
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw }, };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IgnoreCase = true,
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = @"((?<exe>.+)(?=\.exe))|((?<dll>.+)(?=\.dll))",
				TitleRule = ".*",
				UrlRule = "(?<dll>.*)",
				IsRegex = true,
				FormattedNamedGroups = new Dictionary<string, string>()
				{
					{"workkey", "{exe} - {dll}"},
				}
			});

			DesktopWindow matchedWindow;
			var matcher = new RuleMatcherFormatter<WorkChangingRule>(matchRule);
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));
			var res = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey);
			Assert.True(res.Count == 1);
			Assert.Equal("test - fallback", res[WorkDetectorRule.GroupNameWorkKey]);
		}

		[Fact]
		public void UnmatchedGroupHandlingReversed()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "fallback",
				Title = "x",
				Url = "test.EXE",
				IsActive = true,
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw }, };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IgnoreCase = true,
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "(?<dll>.*)",
				TitleRule = ".*",
				UrlRule = @"((?<exe>.+)(?=\.exe))|((?<dll>.+)(?=\.dll))",
				IsRegex = true,
				FormattedNamedGroups = new Dictionary<string, string>()
				{
					{"workkey", "{exe} - {dll}"},
				}
			});

			DesktopWindow matchedWindow;
			var matcher = new RuleMatcherFormatter<WorkChangingRule>(matchRule);
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));
			var res = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey);
			Assert.True(res.Count == 1);
			Assert.Equal("test - fallback", res[WorkDetectorRule.GroupNameWorkKey]);
		}

		[Fact]
		public void ComplexRuleMatchingAndFormattingWithProjectKeys()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "IEXPLORE.EXE",
				Title = "xFLOWer: Ügyek keresése - LÁZS ANITA - xFLOWer - Groupama Garancia Biztosító Zrt. - Windows Internet Explorer",
				Url = "http://xflowerw3.garancia.hu/xflower/index.php?lang=hu&sid=evp78tsjkdudcf90tmheh9iqf0&page=33&tab=2590;2530&focus=1",
				IsActive = true,
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw }, };
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "Status"), "Idle");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubject"), "Kimenő levelek iktatása");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubjectState"), "Folyamatban");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				RuleType = WorkDetectorRuleType.TempStartOrAssignProjectAndWork,
				IgnoreCase = true,
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "iexplore.exe",
				TitleRule = ".*",
				UrlRule = "^https?://xflowerw3(.garancia.hu)?/.+page=33.+$",
				IsRegex = true,
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
				{
				    {"JobCTRL.IE", new Dictionary<string, string>() {
						{"1_BarCode", "(?<tag2>.+)"},
						{"1_CheckSubject", "(?<tag1>.+)"},
						{"1_CheckSubjectState", "(?<desc>.+)"},
				    }}
				},
				FormattedNamedGroups = new Dictionary<string, string>()
				{
					{"workkey", "{tag1} - {tag2}"},
					{"projectkey1", "{tag1} - {desc}"},
					{"projectkey2", "{tag2} - {tag1}"},
				}
			});

			DesktopWindow matchedWindow;
			var matcher = new RuleMatcherFormatter<IWorkChangingRule>(matchRule);
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));
			var res = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameDescription, WorkDetectorRule.GroupNameProjectKey + "1", WorkDetectorRule.GroupNameProjectKey + "2");
			Assert.True(res.Count == 4);
			Assert.Equal("Kimenő levelek iktatása - 12345679812", res[WorkDetectorRule.GroupNameWorkKey]);
			Assert.Equal("Kimenő levelek iktatása - Folyamatban", res[WorkDetectorRule.GroupNameProjectKey + "1"]);
			Assert.Equal("12345679812 - Kimenő levelek iktatása", res[WorkDetectorRule.GroupNameProjectKey + "2"]);
			Assert.Equal("Folyamatban", res[WorkDetectorRule.GroupNameDescription]);
			Assert.DoesNotThrow(() => matcher.Rule.OriginalRule.ValidateAndGetMatchers(new Tct.ActivityRecorderClient.Menu.ClientMenuLookup()));

			//WorkDetector
			var assignData = new Tct.ActivityRecorderClient.Capturing.Core.WorkDetector().GetAssignProjectAndWorkData(desktopCapture, matcher);
			Assert.NotNull(assignData);
			Assert.Equal("Kimenő levelek iktatása - 12345679812", assignData.Composite.WorkKey);
			Assert.Equal(2, assignData.Composite.ProjectKeys.Count);
			Assert.Equal("Kimenő levelek iktatása - Folyamatban", assignData.Composite.ProjectKeys[0]);
			Assert.Equal("12345679812 - Kimenő levelek iktatása", assignData.Composite.ProjectKeys[1]);
			Assert.Equal("Folyamatban", assignData.Composite.Description);
		}

		[Fact]
		public void ComplexRuleMatchingAndFormattingWithTwoWindows()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "IEXPLORE.EXE",
				Title = "xFLOWer: Ügyek keresése - LÁZS ANITA - xFLOWer - Groupama Garancia Biztosító Zrt. - Windows Internet Explorer",
				Url = "http://xflowerw3.garancia.hu/xflower/index.php?lang=hu&sid=evp78tsjkdudcf90tmheh9iqf0&page=33&tab=2590;2530&focus=1",
				IsActive = true,
			};
			var dw2 = new DesktopWindow()
			{
				ProcessName = "notepad.exe",
				Title = "Untitled - Notepad",
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw, dw2 }, };
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "Status"), "Idle");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubject"), "Kimenő levelek iktatása");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubjectState"), "Folyamatban");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				RuleType = WorkDetectorRuleType.TempStartOrAssignProjectAndWork,
				IgnoreCase = true,
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "iexplore.exe",
				TitleRule = ".*",
				UrlRule = "^https?://xflowerw3(.garancia.hu)?/.+page=33.+$",
				IsRegex = true,
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
				{
				    {"JobCTRL.IE", new Dictionary<string, string>() {
						{"1_BarCode", "(?<tag2>.+)"},
						{"1_CheckSubject", "(?<tag1>.+)"},
						{"1_CheckSubjectState", "(?<desc>.+)"},
				    }}
				},
				FormattedNamedGroups = new Dictionary<string, string>()
				{
					{"workkey", "{tag1} - {tag2} - {tag3}"}
				},
				Children = new List<WindowRule>()
				{
					new WindowRule() { ProcessRule = "(?<tag3>notepad.exe)", TitleRule = ".*", IsRegex = true, WindowScope = WindowScopeType.Any, IsEnabled = true }
				}
			});

			DesktopWindow matchedWindow;
			var matcher = new RuleMatcherFormatter<IWorkChangingRule>(matchRule);
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));
			var res = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameDescription);
			Assert.True(res.Count == 2);
			Assert.Equal("Kimenő levelek iktatása - 12345679812 - notepad.exe", res[WorkDetectorRule.GroupNameWorkKey]);
			Assert.Equal("Folyamatban", res[WorkDetectorRule.GroupNameDescription]);
			Assert.DoesNotThrow(() => matcher.Rule.OriginalRule.ValidateAndGetMatchers(new Tct.ActivityRecorderClient.Menu.ClientMenuLookup()));

			//WorkDetector
			var assignData = new Tct.ActivityRecorderClient.Capturing.Core.WorkDetector().GetAssignProjectAndWorkData(desktopCapture, matcher);
			Assert.NotNull(assignData);
			Assert.Equal("Kimenő levelek iktatása - 12345679812 - notepad.exe", assignData.Composite.WorkKey);
			Assert.Equal(0, assignData.Composite.ProjectKeys.Count);
			Assert.Equal("Folyamatban", assignData.Composite.Description);

			//disable child
			matchRule.OriginalRule.Children[0].IsEnabled = false;
			matcher = new RuleMatcherFormatter<IWorkChangingRule>(new WorkChangingRule(matchRule.OriginalRule));
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));

			assignData = new Tct.ActivityRecorderClient.Capturing.Core.WorkDetector().GetAssignProjectAndWorkData(desktopCapture, matcher);
			Assert.NotNull(assignData);
			Assert.Equal("Kimenő levelek iktatása - 12345679812 -", assignData.Composite.WorkKey);
			Assert.Equal(0, assignData.Composite.ProjectKeys.Count);
			Assert.Equal("Folyamatban", assignData.Composite.Description);
		}

		[Fact]
		public void ComplexRuleMatchingAndFormattingWithTwoWindowsReversed()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "IEXPLORE.EXE",
				Title = "xFLOWer: Ügyek keresése - LÁZS ANITA - xFLOWer - Groupama Garancia Biztosító Zrt. - Windows Internet Explorer",
				Url = "http://xflowerw3.garancia.hu/xflower/index.php?lang=hu&sid=evp78tsjkdudcf90tmheh9iqf0&page=33&tab=2590;2530&focus=1",
				IsActive = true,
			};
			var dw2 = new DesktopWindow()
			{
				ProcessName = "notepad.exe",
				Title = "Untitled - Notepad",
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw, dw2 }, };
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "Status"), "Idle");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubject"), "Kimenő levelek iktatása");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubjectState"), "Folyamatban");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				RuleType = WorkDetectorRuleType.TempStartOrAssignProjectAndWork,
				ProcessRule = "(?<tag3>notepad.exe)",
				TitleRule = ".*",
				IsRegex = true,
				WindowScope = WindowScopeType.Any,
				IsEnabled = true,
				IgnoreCase = true,
				FormattedNamedGroups = new Dictionary<string, string>()
				{
					{"workkey", "{tag1} - {tag2} - {tag3}"}
				},
				Children = new List<WindowRule>()
				{
					new WindowRule() { 
						IsEnabled = true,
						IgnoreCase = true,
						WindowScope = WindowScopeType.Active,
						ProcessRule = "iexplore.exe",
						TitleRule = ".*",
						UrlRule = "^https?://xflowerw3(.garancia.hu)?/.+page=33.+$",
						IsRegex = true,
						ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
						{
							{"JobCTRL.IE", new Dictionary<string, string>() {
								{"1_BarCode", "(?<tag2>.+)"},
								{"1_CheckSubject", "(?<tag1>.+)"},
								{"1_CheckSubjectState", "(?<desc>.+)"},
							}}
						},
					}
				}
			});

			DesktopWindow matchedWindow;
			var matcher = new RuleMatcherFormatter<IWorkChangingRule>(matchRule);
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));
			var res = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameDescription);
			Assert.True(res.Count == 2);
			Assert.Equal("Kimenő levelek iktatása - 12345679812 - notepad.exe", res[WorkDetectorRule.GroupNameWorkKey]);
			Assert.Equal("Folyamatban", res[WorkDetectorRule.GroupNameDescription]);
			Assert.DoesNotThrow(() => matcher.Rule.OriginalRule.ValidateAndGetMatchers(new Tct.ActivityRecorderClient.Menu.ClientMenuLookup()));

			//WorkDetector
			var assignData = new Tct.ActivityRecorderClient.Capturing.Core.WorkDetector().GetAssignProjectAndWorkData(desktopCapture, matcher);
			Assert.NotNull(assignData);
			Assert.Equal("Kimenő levelek iktatása - 12345679812 - notepad.exe", assignData.Composite.WorkKey);
			Assert.Equal(0, assignData.Composite.ProjectKeys.Count);
			Assert.Equal("Folyamatban", assignData.Composite.Description);

			//make it fail
			matchRule.OriginalRule.Children[0].IgnoreCase = false;
			matcher = new RuleMatcherFormatter<IWorkChangingRule>(new WorkChangingRule(matchRule.OriginalRule));
			Assert.False(matcher.IsMatch(desktopCapture, out matchedWindow));
		}

		[Fact]
		public void ValidationWorkKeyWithTwoWindows()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "IEXPLORE.EXE",
				Title = "xFLOWer: Ügyek keresése - LÁZS ANITA - xFLOWer - Groupama Garancia Biztosító Zrt. - Windows Internet Explorer",
				Url = "http://xflowerw3.garancia.hu/xflower/index.php?lang=hu&sid=evp78tsjkdudcf90tmheh9iqf0&page=33&tab=2590;2530&focus=1",
				IsActive = true,
			};
			var dw2 = new DesktopWindow()
			{
				ProcessName = "notepad.exe",
				Title = "Untitled - Notepad",
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw, dw2 }, };
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "Status"), "Idle");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubject"), "Kimenő levelek iktatása");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubjectState"), "Folyamatban");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				RuleType = WorkDetectorRuleType.TempStartOrAssignProjectAndWork,
				IgnoreCase = true,
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "iexplore.exe",
				TitleRule = ".*",
				UrlRule = "^https?://xflowerw3(.garancia.hu)?/.+page=33.+$",
				IsRegex = true,
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
				{
				    {"JobCTRL.IE", new Dictionary<string, string>() {
						{"1_BarCode", "(?<tag2>.+)"},
						{"1_CheckSubject", "(?<tag1>.+)"},
						{"1_CheckSubjectState", "(?<desc>.+)"},
				    }}
				},
				FormattedNamedGroups = new Dictionary<string, string>()
				{
					{"projectkey1", "{tag1} - {tag2}"}
				},
				Children = new List<WindowRule>()
				{
					new WindowRule() { ProcessRule = "(?<workkey>notepad.exe)", TitleRule = ".*", IsRegex = true, WindowScope = WindowScopeType.Any, IsEnabled = true }
				}
			});

			DesktopWindow matchedWindow;
			var matcher = new RuleMatcherFormatter<IWorkChangingRule>(matchRule);
			Assert.True(matcher.IsMatch(desktopCapture, out matchedWindow));
			var res = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameDescription, "projectkey1");
			Assert.True(res.Count == 3);
			Assert.Equal("notepad.exe", res[WorkDetectorRule.GroupNameWorkKey]);
			Assert.Equal("Folyamatban", res[WorkDetectorRule.GroupNameDescription]);
			Assert.Equal("Kimenő levelek iktatása - 12345679812", res["projectkey1"]);
			Assert.DoesNotThrow(() => matcher.Rule.OriginalRule.ValidateAndGetMatchers(new Tct.ActivityRecorderClient.Menu.ClientMenuLookup()));

			//WorkDetector
			var assignData = new Tct.ActivityRecorderClient.Capturing.Core.WorkDetector().GetAssignProjectAndWorkData(desktopCapture, matcher);
			Assert.NotNull(assignData);
			Assert.Equal("notepad.exe", assignData.Composite.WorkKey);
			Assert.Equal(1, assignData.Composite.ProjectKeys.Count);
			Assert.Equal("Kimenő levelek iktatása - 12345679812", assignData.Composite.ProjectKeys[0]);
			Assert.Equal("Folyamatban", assignData.Composite.Description);

			//disable child
			matchRule.OriginalRule.Children[0].IsEnabled = false;
			matcher = new RuleMatcherFormatter<IWorkChangingRule>(new WorkChangingRule(matchRule.OriginalRule));
			Assert.Throws<Exception>(() => matcher.Rule.OriginalRule.ValidateAndGetMatchers(new Tct.ActivityRecorderClient.Menu.ClientMenuLookup()));
		}
	}
}
