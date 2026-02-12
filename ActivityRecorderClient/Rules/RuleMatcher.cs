using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;

namespace Tct.ActivityRecorderClient.Rules
{
	/// <summary>
	/// Immutable (kinda) class for matching WorkDetectorRules and CensorRules
	/// </summary>
	public class RuleMatcher<T> where T : class, IRule
	{
		public T Rule { get; private set; }

		private static readonly string[] emptyStrArr = new string[0];
		private static readonly Dictionary<CaptureExtensionKey, string[]> emptyExtGroupNames = new Dictionary<CaptureExtensionKey, string[]>(0); //should be read-only
		private readonly RegexOrContains titleRuleRegex;
		private readonly RegexOrContains processRuleRegex;
		private readonly RegexOrContains urlRuleRegex;
		private readonly bool isEnabled;
		private readonly string[] titleGroupNames;
		private readonly string[] processGroupNames;
		private readonly string[] urlGroupNames;
		private readonly Dictionary<CaptureExtensionKey, CachedRegex> extensionRulesRegex;
		private readonly Dictionary<CaptureExtensionKey, string[]> extensionGroupNames;
		private readonly WindowScopeType windowScope;
		private readonly RuleMatcher<IRule>[] children;

		public RuleMatcher(T rule)
		{
			if (rule == null) throw new ArgumentNullException("rule");
			Rule = rule; //we don't track changes on the rule... (YAGNI)
			isEnabled = rule.IsEnabled;
			if (!isEnabled) return;
			windowScope = rule.WindowScope;
			titleRuleRegex = RuleMatcher.GetRegexForRule(rule.TitleRule, rule.IsRegex, rule.IgnoreCase);
			processRuleRegex = RuleMatcher.GetRegexForRule(rule.ProcessRule, rule.IsRegex, rule.IgnoreCase);
			//url rule is different from the other two, because not all process has url. (Also it was added later so it might be null for old rules)
			urlRuleRegex = rule.UrlRule == null ? null : RuleMatcher.GetRegexForRule(rule.UrlRule, rule.IsRegex, rule.IgnoreCase);
			titleGroupNames = titleRuleRegex.GetGroupNames();
			processGroupNames = processRuleRegex.GetGroupNames();
			urlGroupNames = urlRuleRegex == null ? emptyStrArr : urlRuleRegex.GetGroupNames();
			if (rule.ExtensionRules == null)
			{
				extensionGroupNames = emptyExtGroupNames;
			}
			else
			{
				extensionRulesRegex = rule.ExtensionRules.ToDictionary(n => n.Key, n => new CachedRegex { Regex = RuleMatcher.GetRegexForRule(n.Value, rule.IsRegex, rule.IgnoreCase)});
				extensionGroupNames = extensionRulesRegex.ToDictionary(n => n.Key, n => n.Value.Regex.GetGroupNames());
			}
			var ruleChildren = rule.Children;
			if (ruleChildren != null)
			{
				children = ruleChildren.Where(n => n.IsEnabled).Select(n => new RuleMatcher<IRule>(n)).ToArray();
				if (children.Length == 0) children = null;
			}
		}

		public bool IsMatch(DesktopCapture desktopCapture, out DesktopWindow matchedWindow)
		{
			matchedWindow = GetMatchedWindows(desktopCapture).FirstOrDefault();
			return matchedWindow != null;
		}

		public bool IsMatch(DesktopCapture desktopCapture)
		{
			DesktopWindow _;
			return IsMatch(desktopCapture, out _);
		}

		private IEnumerable<DesktopWindow> GetMatchedWindows(DesktopCapture desktopCapture)
		{
			return desktopCapture.GetDesktopWindowsNotNull().Where(n => IsMatch(n, desktopCapture));
		}

		private bool IsWindowInScope(DesktopWindow desktopWindow)
		{
			Debug.Assert(desktopWindow != null);
			return desktopWindow.IsActive
				|| windowScope == WindowScopeType.VisibleOrActive && desktopWindow.VisibleClientArea > 0
				|| windowScope == WindowScopeType.Any;
		}

		public bool IsMatch(DesktopWindow desktopWindow, DesktopCapture desktopCapture)
		{
			if (desktopWindow == null) return false;
			return (isEnabled
				&& IsWindowInScope(desktopWindow)
				&& titleRuleRegex.IsMatch(desktopWindow.Title ?? "")
				&& processRuleRegex.IsMatch(desktopWindow.ProcessName ?? "")
				&& (urlRuleRegex == null || urlRuleRegex.IsMatch(desktopWindow.Url ?? ""))
				&& IsMatchForExtensions(desktopWindow)
				&& (children == null || children.All(n => n.IsMatch(desktopCapture)))
				);
		}

		private bool IsMatchForExtensions(DesktopWindow desktopWindow)
		{
			return extensionRulesRegex == null
				|| extensionRulesRegex.All(n =>
											{
												string extensionValue;
												if (desktopWindow.CaptureExtensions == null
													|| !desktopWindow.CaptureExtensions.TryGetValue(n.Key, out extensionValue))
												{
													extensionValue = null;
												}
												return n.Value.IsMatch(extensionValue ?? "");
											});
		}

		public string[] GetGroupNames()
		{
			return titleGroupNames
				.Concat(processGroupNames)
				.Concat(urlGroupNames)
				.Concat(extensionGroupNames.Values.SelectMany(n => n))
				.Concat(children == null ? new string[0] : children.SelectMany(n => n.GetGroupNames()))
				.Distinct()
				.ToArray(); //we should only return a copy
		}

		protected Dictionary<string, string> GetAllMatchingGroups(DesktopCapture desktopCapture)
		{
			var matchedWindow = GetMatchedWindows(desktopCapture).FirstOrDefault();
			return GetAllMatchingGroups(matchedWindow, desktopCapture);
		}

		private Dictionary<string, string> GetAllMatchingGroups(DesktopWindow desktopWindow, DesktopCapture desktopCapture)
		{
			if (desktopWindow == null) return null;
			var matchTitle = titleRuleRegex.Match(desktopWindow.Title ?? "");
			var matchProc = processRuleRegex.Match(desktopWindow.ProcessName ?? "");
			var matchUrl = urlRuleRegex == null ? null : urlRuleRegex.Match(desktopWindow.Url ?? "");
			var matchForExtensions = GetMatchForExtensions(desktopWindow).ToList();
			if (!matchTitle.Success
				|| !matchProc.Success
				|| (matchUrl != null && !matchUrl.Success)
				|| matchForExtensions.Any(n => !n.Value.Success))
			{
				return null; //rule is not matched
			}

			var res = desktopCapture.GlobalVariables ?? new Dictionary<string, string>();
			foreach (var groupName in titleGroupNames)
			{
				AddGroupIfMatched(res, groupName, matchTitle);
			}

			foreach (var groupName in processGroupNames)
			{
				AddGroupIfMatched(res, groupName, matchProc);
			}

			if (matchUrl != null)
			{
				foreach (var groupName in urlGroupNames)
				{
					AddGroupIfMatched(res, groupName, matchUrl);
				}
			}

			foreach (var matchForExtension in matchForExtensions)
			{
				foreach (var groupName in extensionGroupNames[matchForExtension.Key])
				{
					AddGroupIfMatched(res, groupName, matchForExtension.Value);
				}
			}

			if (children != null)
			{
				foreach (var child in children)
				{
					var groups = child.GetAllMatchingGroups(desktopCapture);
					if (groups == null) return null; //rule is not matched
					foreach (var kvp in groups)
					{
						if (!res.ContainsKey(kvp.Key)) res.Add(kvp.Key, kvp.Value);
					}
				}
			}

			return res;
		}

		private IEnumerable<KeyValuePair<CaptureExtensionKey, RegexOrContains.MatchEx>> GetMatchForExtensions(DesktopWindow desktopWindow)
		{
			if (extensionRulesRegex == null) return Enumerable.Empty<KeyValuePair<CaptureExtensionKey, RegexOrContains.MatchEx>>();
			return extensionRulesRegex.Select(n =>
			{
				string extensionValue;
				if (desktopWindow.CaptureExtensions == null
					|| !desktopWindow.CaptureExtensions.TryGetValue(n.Key, out extensionValue))
				{
					extensionValue = null;
				}
				return new KeyValuePair<CaptureExtensionKey, RegexOrContains.MatchEx>(n.Key, n.Value.Regex.Match(extensionValue ?? ""));
			});
		}

		private static bool AddGroupIfMatched(Dictionary<string, string> result, string groupName, RegexOrContains.MatchEx match)
		{
			if (result.ContainsKey(groupName)) return false;
			var group = match.Groups[groupName];
			if (!group.Success) return false;
			result.Add(groupName, group.Value);
			return true;
		}
	}

	public static class RuleMatcher
	{
		public static RegexOrContains GetRegexForRule(string rule, bool isRegex, bool ignoreCase)
		{
			return RegexOrContains.Create(rule, isRegex, ignoreCase);
		}

		public static bool SameRuleApplies(DesktopCapture first, DesktopCapture second)
		{
			if (first == null || first.DesktopWindows == null) return second == null || second.DesktopWindows == null;
			if (second == null || second.DesktopWindows == null) return false;
			if (first.DesktopWindows.Count != second.DesktopWindows.Count) return false;
			for (int i = 0; i < first.DesktopWindows.Count; i++)
			{
				if (!SameRuleApplies(first.DesktopWindows[i], second.DesktopWindows[i])) return false;
			}
			return true;
		}

		private static bool SameRuleApplies(DesktopWindow firstAw, DesktopWindow secondAw)
		{
			if (firstAw == null) return secondAw == null;
			if (secondAw == null) return false;
			return firstAw.IsActive == secondAw.IsActive
				&& firstAw.VisibleClientArea == secondAw.VisibleClientArea
				&& firstAw.Title == secondAw.Title
				&& firstAw.ProcessName == secondAw.ProcessName
				&& firstAw.Url == secondAw.Url
				&& (firstAw.CaptureExtensions == secondAw.CaptureExtensions
					|| (firstAw.CaptureExtensions != null
						&& secondAw.CaptureExtensions != null
						&& firstAw.CaptureExtensions.Count == secondAw.CaptureExtensions.Count
						&& firstAw.CaptureExtensions.All(n =>
						{
							string value;
							return secondAw.CaptureExtensions.TryGetValue(n.Key, out value) && value == n.Value;
						}))
					)
				;
		}
	}

	public class CachedRegex
	{
		private readonly CachedDictionary<string, bool> isMatchDict = new CachedDictionary<string, bool>(TimeSpan.FromHours(1), true);
		public RegexOrContains Regex { get; set; }
		public bool IsMatch(string value)
		{
			bool isMatch;
			if (isMatchDict.TryGetValue(value, out isMatch))
				return isMatch;
			isMatch = Regex.IsMatch(value);
			isMatchDict.Add(value, isMatch);
			return isMatch;
		}
	}
}
