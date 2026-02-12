using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;

namespace Tct.ActivityRecorderClient.Rules.Generation
{
	internal class GeneratedRule : IRule
	{
		public string Name { get; set; }
		public bool IsEnabled { get; set; }
		public bool IsEnabledInNonWorkStatus { get; set; }
		public bool IsRegex { get; set; }
		public bool IgnoreCase { get; set; }
		public string TitleRule { get; set; }
		public string ProcessRule { get; set; }
		public string UrlRule { get; set; }
		IEnumerable<KeyValuePair<CaptureExtensionKey, string>> IRule.ExtensionRules { get { return ExtensionRules; } }
		public Dictionary<CaptureExtensionKey, string> ExtensionRules { get; set; }
		public WindowScopeType WindowScope { get; set; }
		IEnumerable<IRule> IRule.Children { get { return Children == null ? null : Children.OfType<IRule>(); } }
		public List<WindowRule> Children { get; set; }

		public static readonly GeneratedRule Empty = new GeneratedRule() { IsEnabled = false };

		public static Dictionary<CaptureExtensionKey, string> GetSimpleExtensionRules(DesktopWindow aw, IRule matchingRule, bool isRegex)
		{
			if (matchingRule == null || matchingRule.ExtensionRules == null) return null;
			var result = new Dictionary<CaptureExtensionKey, string>();
			foreach (var explicitRule in matchingRule.ExtensionRules) //we only create ExtensionRule for the generated rule if it was specified for the learning rule
			{
				string capturedValue;
				if (aw.CaptureExtensions == null || !aw.CaptureExtensions.TryGetValue(explicitRule.Key, out capturedValue))
				{
					capturedValue = ""; //we also create a CaptureExtension even if it was not captured (but it is in the rule)
				}
				if (capturedValue == null) capturedValue = ""; //null value cannot be handled so treated as empty
				Debug.Assert(!result.ContainsKey(explicitRule.Key));
				result[explicitRule.Key] = isRegex ? "^" + Regex.Escape(capturedValue) + "$" : capturedValue;
			}
			return result.Count == 0 ? null : result;
		}

		public static List<WindowRule> GetChildrenRules(IRule matchingRule) //we copy children rules as they are (rules are not replaced by captured values)
		{
			if (matchingRule == null || matchingRule.Children == null) return null;
			var result = matchingRule.Children.Select(n => WindowRule.CreateFromIRule(n)).ToList();
			return result.Count > 0 ? result : null;
		}
	}
}
