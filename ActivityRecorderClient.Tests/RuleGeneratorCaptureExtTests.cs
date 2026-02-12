using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Rules.Generation;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class RuleGeneratorCaptureExtTests
	{
		[Fact]
		public void SimpleGeneratorWithoutCaptureExt()
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c", IsActive = true };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(window));
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(false, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("b", rule.TitleRule);
			Assert.Equal("c", rule.UrlRule);
			Assert.Null(rule.ExtensionRules);
		}

		[Fact]
		public void SimpleGeneratorWithoutCaptureExtAnyWindow()
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				WindowScope = WindowScopeType.Any,
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(window));
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(false, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("b", rule.TitleRule);
			Assert.Equal("c", rule.UrlRule);
			Assert.Null(rule.ExtensionRules);
		}

		[Fact]
		public void SimpleGeneratorWithoutCaptureExtNoMatch()
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.False(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window));
			Assert.False(new RuleMatcher<IRule>(rule).IsMatch(window));
		}

		[Fact]
		public void SimpleGeneratorCaptureExtAdded()
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			window.SetCaptureExtension(new CaptureExtensionKey("Id", "Key"), "Value");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
				{
					{new CaptureExtensionKey("Id", "Key"), "*"},
				},
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(window));
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(false, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("b", rule.TitleRule);
			Assert.Equal("c", rule.UrlRule);
			Assert.NotNull(rule.ExtensionRules);
			Assert.Equal("Id", rule.ExtensionRules.Single().Key.Id);
			Assert.Equal("Key", rule.ExtensionRules.Single().Key.Key);
			Assert.Equal("Value", rule.ExtensionRules.Single().Value);
		}

		[Fact]
		public void SimpleGeneratorCaptureExtAddedEscape()
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			window.SetCaptureExtension(new CaptureExtensionKey("Id", "Key"), "Value*.?[");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
				{
					{new CaptureExtensionKey("Id", "Key"), "*"},
				},
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(window));
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(false, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("b", rule.TitleRule);
			Assert.Equal("c", rule.UrlRule);
			Assert.NotNull(rule.ExtensionRules);
			Assert.Equal("Id", rule.ExtensionRules.Single().Key.Id);
			Assert.Equal("Key", rule.ExtensionRules.Single().Key.Key);
			Assert.Equal("Value*.?[", rule.ExtensionRules.Single().Value);
		}


		[Fact]
		public void SimpleGeneratorCaptureExtAddedOnlyIfSpecified()
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			window.SetCaptureExtension(new CaptureExtensionKey("Id", "Key2"), "Value2");
			window.SetCaptureExtension(new CaptureExtensionKey("Id", "Key"), "Value");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
				{
					{new CaptureExtensionKey("Id", "Key"), "*"},
				},
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(window));
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(false, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("b", rule.TitleRule);
			Assert.Equal("c", rule.UrlRule);
			Assert.NotNull(rule.ExtensionRules);
			Assert.Equal("Id", rule.ExtensionRules.Single().Key.Id);
			Assert.Equal("Key", rule.ExtensionRules.Single().Key.Key);
			Assert.Equal("Value", rule.ExtensionRules.Single().Value);
		}

		[Fact]
		public void SimpleGeneratorCaptureExtNull()
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			window.SetCaptureExtension(new CaptureExtensionKey("Id", "Key"), null);
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
				{
					{new CaptureExtensionKey("Id", "Key"), "*"},
				},
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(window));
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(false, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("b", rule.TitleRule);
			Assert.Equal("c", rule.UrlRule);
			Assert.NotNull(rule.ExtensionRules);
			Assert.Equal("Id", rule.ExtensionRules.Single().Key.Id);
			Assert.Equal("Key", rule.ExtensionRules.Single().Key.Key);
			Assert.Equal("", rule.ExtensionRules.Single().Value);
		}

		[Fact]
		public void SimpleGeneratorCaptureExtMissingCaptureAddedAsEmpty() //not really sure if this is right... but probably it is
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			var window2 = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			window2.SetCaptureExtension(new CaptureExtensionKey("Id", "Key"), "Value");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
				{
					{new CaptureExtensionKey("Id", "Key"), "*"},
				},
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(window));
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window2));
			Assert.False(new RuleMatcher<IRule>(rule).IsMatch(window2)); //since we only match for empty string this should fail
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(false, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("b", rule.TitleRule);
			Assert.Equal("c", rule.UrlRule);
			Assert.NotNull(rule.ExtensionRules);
			Assert.Equal("Id", rule.ExtensionRules.Single().Key.Id);
			Assert.Equal("Key", rule.ExtensionRules.Single().Key.Key);
			Assert.Equal("", rule.ExtensionRules.Single().Value);
		}


		[Fact]
		public void ReplaceGeneratorCaptureExtAddedEscape()
		{
			//Arrange
			var gen = new ReplaceGroupRuleGenerator(true, new[] { new ReplaceGroupParameter() { MatchingPattern = "^(?<a>.*)$", ReplaceGroupName = "a" } }, new[] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" } }, new[] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" } });
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			window.SetCaptureExtension(new CaptureExtensionKey("Id", "Key"), "Value*.?[");
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
				{
					{new CaptureExtensionKey("Id", "Key"), "*"},
				},
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(window));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(window));
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(true, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("^.*$", rule.TitleRule);
			Assert.Equal("^.*$", rule.UrlRule);
			Assert.NotNull(rule.ExtensionRules);
			Assert.Equal("Id", rule.ExtensionRules.Single().Key.Id);
			Assert.Equal("Key", rule.ExtensionRules.Single().Key.Key);
			Assert.Equal("^Value\\*\\.\\?\\[$", rule.ExtensionRules.Single().Value);
		}

		[Fact]
		public void ReplaceGeneratorCaptureExtAddedEscapeWithChild()
		{
			//Arrange
			var gen = new ReplaceGroupRuleGenerator(true, new[] { new ReplaceGroupParameter() { MatchingPattern = "^(?<a>.*)$", ReplaceGroupName = "a" } }, new[] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" } }, new[] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" } });
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			window.SetCaptureExtension(new CaptureExtensionKey("Id", "Key"), "Value*.?[");
			var dw2 = new DesktopWindow() { ProcessName = "d", Title = "e", Url = "f" };
			dw2.SetCaptureExtension(new CaptureExtensionKey("Id2", "Key2"), "Value*.?[2");
			var dc = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { window, dw2 } };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
				{
					{new CaptureExtensionKey("Id", "Key"), "*"},
				},
				Children = new List<WindowRule>()
				{
					new WindowRule() { IsEnabled = true, ProcessRule = "d", TitleRule = ".*", UrlRule = "f", IsRegex = true, WindowScope = WindowScopeType.Any,
						ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
						{
							{new CaptureExtensionKey("Id2", "Key2"), "V*"},
						},
					}
				}
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(dc));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(dc));
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(true, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("^.*$", rule.TitleRule);
			Assert.Equal("^.*$", rule.UrlRule);
			Assert.NotNull(rule.ExtensionRules);
			Assert.Equal("Id", rule.ExtensionRules.Single().Key.Id);
			Assert.Equal("Key", rule.ExtensionRules.Single().Key.Key);
			Assert.Equal("^Value\\*\\.\\?\\[$", rule.ExtensionRules.Single().Value);
			Assert.NotNull(rule.Children);
			Assert.Equal(1, rule.Children.Count());
			Assert.Equal("d", rule.Children.First().ProcessRule);
			Assert.Equal(".*", rule.Children.First().TitleRule); //children rules are only copied
			Assert.Equal("f", rule.Children.First().UrlRule);
			Assert.Equal(1, rule.Children.First().ExtensionRules.Count());
			Assert.Equal("Id2", rule.Children.First().ExtensionRules.First().Key.Id);
			Assert.Equal("Key2", rule.Children.First().ExtensionRules.First().Key.Key);
			Assert.Equal("V*", rule.Children.First().ExtensionRules.First().Value);
		}

		[Fact]
		public void SimpleGeneratorCaptureExtWithChild()
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			window.SetCaptureExtension(new CaptureExtensionKey("Id", "Key"), "Value*.?[");
			var dw2 = new DesktopWindow() { ProcessName = "d", Title = "e", Url = "f" };
			dw2.SetCaptureExtension(new CaptureExtensionKey("Id2", "Key2"), "Value*.?[2");
			var dc = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { window, dw2 } };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
				{
					{new CaptureExtensionKey("Id", "Key"), "*"},
				},
				Children = new List<WindowRule>()
				{
					new WindowRule() { IsEnabled = true, ProcessRule = "d", TitleRule = ".*", UrlRule = "f", IsRegex = true, WindowScope = WindowScopeType.Any,
						ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
						{
							{new CaptureExtensionKey("Id2", "Key2"), "V*"},
						},
					}
				}
			});

			//Act
			var rule = gen.GetRuleFromWindow(window, matchRule);

			//Assert
			Assert.True(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(dc));
			Assert.True(new RuleMatcher<IRule>(rule).IsMatch(dc));
			Assert.Equal(true, rule.IgnoreCase);
			Assert.Equal(false, rule.IsRegex);
			Assert.Equal(true, rule.IsEnabled);
			Assert.Equal("a", rule.ProcessRule);
			Assert.Equal("b", rule.TitleRule);
			Assert.Equal("c", rule.UrlRule);
			Assert.NotNull(rule.ExtensionRules);
			Assert.Equal("Id", rule.ExtensionRules.Single().Key.Id);
			Assert.Equal("Key", rule.ExtensionRules.Single().Key.Key);
			Assert.Equal("Value*.?[", rule.ExtensionRules.Single().Value);
			Assert.NotNull(rule.Children);
			Assert.Equal(1, rule.Children.Count());
			Assert.Equal("d", rule.Children.First().ProcessRule);
			Assert.Equal(".*", rule.Children.First().TitleRule); //children rules are only copied
			Assert.Equal("f", rule.Children.First().UrlRule);
			Assert.Equal(1, rule.Children.First().ExtensionRules.Count());
			Assert.Equal("Id2", rule.Children.First().ExtensionRules.First().Key.Id);
			Assert.Equal("Key2", rule.Children.First().ExtensionRules.First().Key.Key);
			Assert.Equal("V*", rule.Children.First().ExtensionRules.First().Value);
		}

		[Fact]
		public void SimpleGeneratorCaptureExtWithChildNoMatch()
		{
			//Arrange
			var gen = new SimpleRuleGenerator(true);
			var window = new DesktopWindow() { ProcessName = "a", Title = "b", Url = "c" };
			window.SetCaptureExtension(new CaptureExtensionKey("Id", "Key"), "Value*.?[");
			var dw2 = new DesktopWindow() { ProcessName = "d", Title = "e", Url = "f" };
			dw2.SetCaptureExtension(new CaptureExtensionKey("Id2", "Key2"), "Value*.?[2");
			var dc = new DesktopCapture() { DesktopWindows = new List<DesktopWindow>() { window, dw2 } };
			var matchRule = new WorkChangingRule(new WorkDetectorRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "a",
				TitleRule = "b",
				UrlRule = "c",
				IsRegex = false,
				ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
				{
					{new CaptureExtensionKey("Id", "Key"), "*"},
				},
				Children = new List<WindowRule>()
				{
					new WindowRule() { IsEnabled = true, ProcessRule = "d", TitleRule = "f",/*<- This won't match*/ UrlRule = "f", IsRegex = true, WindowScope = WindowScopeType.Any,
						ExtensionRules = new Dictionary<CaptureExtensionKey, string>()
						{
							{new CaptureExtensionKey("Id2", "Key2"), "V*"},
						},
					}
				}
			});

			//Assert
			Assert.False(new RuleMatcher<WorkChangingRule>(matchRule).IsMatch(dc));
		}
	}
}
