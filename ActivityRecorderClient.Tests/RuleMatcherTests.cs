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
	public class RuleMatcherTests
	{
		[Fact]
		public void ComplexRuleMatching()
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
						{"1_BarCode", "(?<workkey>.+)"},
						{"1_CheckSubject", ".*"},
						{"1_CheckSubjectState", ".*"},
				    }}
				},
			});

			DesktopWindow matchedWindow;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow));
		}

		[Fact]
		public void ComplexRuleMatchingIsCaseSensitive()
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
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812"); var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
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
						{"1_Barcode", "(?<workkey>.+)"}, // <------------------------------------------------------------------- c != C
						{"1_CheckSubject", ".*"},
						{"1_CheckSubjectState", ".*"},
				    }}
				},
			});

			DesktopWindow matchedWindow;
			Assert.False(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow));
		}

		[Fact]
		public void MatchWithTwoWindows()
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
				ProcessName = "Notepad.exe",
				Title = "Untitled - Notepad",
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw, dw2 }, };
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "Status"), "Idle");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubject"), "Kimenő levelek iktatása");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubjectState"), "Folyamatban");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
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
						{"1_BarCode", "(?<workkey>.+)"},
						{"1_CheckSubject", ".*"},
						{"1_CheckSubjectState", ".*"},
				    }}
				},
				Children = new List<WindowRule> { new WindowRule()
					{
						IgnoreCase = true,
						IsEnabled = true,
						WindowScope = WindowScopeType.Any,
						ProcessRule = "Notepad.exe",
						TitleRule = ".*",
						IsRegex = true,
					}}
			});

			DesktopWindow matchedWindow;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow));

			dw2.ProcessName = "notmatched.exe";
			Assert.False(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow));
		}

		[Fact]
		public void KeywordBaseRuleMatching()
		{
			var dw = new DesktopWindow()
			{
				ProcessName = "IEXPLORE.EXE",
				Title = "xFLOWer: Ügyek keresése - LÁZS ANITA - xFLOWer - Groupama Garancia Biztosító Zrt. - Windows Internet Explorer",
				Url = "http://szemelyikolcson.raiffeisen.hu/xflower/index.php?lang=hu&sid=evp78tsjkdudcf90tmheh9iqf0&page=33&tab=2590;2530&focus=1",
				IsActive = true,
			};
			var desktopCapture = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { dw }, };
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "Status"), "Idle");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubject"), "Kimenő levelek iktatása");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_CheckSubjectState"), "Folyamatban");
			dw.SetCaptureExtension(new CaptureExtensionKey("JobCTRL.IE", "1_BarCode"), "12345679812");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IgnoreCase = true,
				IsEnabled = true,
				WindowScope = WindowScopeType.Active,
				ProcessRule = "iexplore.exe",
				TitleRule = ".*",
				UrlRule = @"((?=.*aktivszamla\.raiffeisen\.hu).*|(?=.*https\://www\.facebook\.com/raiffeisenbankHU/).*|(?=.*lakashitel\.raiffeisen\.hu).*|(?=.*rafi).*|(?=.*Raiffeisen).*|(?=.*raiffeisen\.hu).*|(?=.*szamlavezetes\.raiffeisen\.hu).*|(?=.*szemelyikolcson\.raiffeisen\.hu).*)",
				IsRegex = true,
				ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
				{
					{"JobCTRL.IE", new Dictionary<string, string>() {
						{"1_BarCode", "(?<workkey>.+)"},
						{"1_CheckSubject", ".*"},
						{"1_CheckSubjectState", ".*"},
					}}
				},
			});

			DesktopWindow matchedWindow;
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(desktopCapture, out matchedWindow));
		}
	}
}
