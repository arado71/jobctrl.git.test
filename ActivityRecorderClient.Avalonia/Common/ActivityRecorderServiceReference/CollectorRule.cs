using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Rules;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	partial class CollectorRule : IFormattedRule, IPluginRule
	{
		//this is a copy/paste from WorkDetectorRule ;/
		public IEnumerable<KeyValuePair<CaptureExtensionKey, string>> ExtensionRules
		{
			get
			{
				if (ExtensionRulesByIdByKey == null) return null;
				return GetExtensionRulesNoNull();
			}
			set
			{
				if (value == null)
				{
					ExtensionRulesByIdByKey = null;
					return;
				}
				var result = new Dictionary<string, Dictionary<string, string>>();
				foreach (var kvpExtRule in value)
				{
					Dictionary<string, string> currVal;
					if (!result.TryGetValue(kvpExtRule.Key.Id, out currVal))
					{
						currVal = new Dictionary<string, string>();
						result.Add(kvpExtRule.Key.Id, currVal);
					}
					currVal[kvpExtRule.Key.Key] = kvpExtRule.Value;
				}
				ExtensionRulesByIdByKey = result;
			}
		}

		private IEnumerable<KeyValuePair<CaptureExtensionKey, string>> GetExtensionRulesNoNull()
		{
			foreach (var extensionRulesByKey in ExtensionRulesByIdByKey)
			{
				if (extensionRulesByKey.Value == null) continue;
				foreach (var keyValueRule in extensionRulesByKey.Value)
				{
					yield return new KeyValuePair<CaptureExtensionKey, string>(new CaptureExtensionKey(extensionRulesByKey.Key, keyValueRule.Key), keyValueRule.Value);
				}
			}
		}

		IEnumerable<IRule> IRule.Children { get { return Children == null ? null : Children.OfType<IRule>(); } }

		public override string ToString()
		{
			return "CollectorRule"
				+ (IsEnabled ? "" : " DISABLED")
				+ " n:" + Name
				+ " keys: " + (CapturedKeys == null ? "NULL" : string.Join(", ", CapturedKeys.ToArray()))
				+ " title:" + TitleRule
				+ " proc:" + ProcessRule
				+ " url:" + UrlRule
				+ " form: " + (FormattedNamedGroups == null ? "NULL" : "(" + string.Join(",", FormattedNamedGroups.Select(x => x.Key + ":" + x.Value).ToArray()) + ")")
				+ GetExtensionRulesToString()
				+ (WindowScope == WindowScopeType.Active ? "" : "(" + WindowScope + ")")
				+ (IsRegex ? " Regex" : "")
				+ (IgnoreCase ? "" : " CaseSensitive")
				+ (ServerId == 0 ? "" : " sid:" + ServerId)
				;
		}

		private string GetExtensionRulesToString()
		{
			if (ExtensionRulesByIdByKey == null || ExtensionRulesByIdByKey.Count == 0) return "";
			var sb = new StringBuilder();
			sb.Append(" ex:");
			foreach (var extensionRulesByKey in ExtensionRulesByIdByKey)
			{
				if (extensionRulesByKey.Value == null) continue;
				sb.Append(" ").Append(extensionRulesByKey.Key).Append(" (");
				var first = true;
				foreach (var keyValueRule in extensionRulesByKey.Value)
				{
					if (!first) sb.Append(", ");
					first = false;
					sb.Append(keyValueRule.Key).Append(":").Append(keyValueRule.Value);
				}
				sb.Append(")");
			}
			return sb.ToString();
		}

	}
}
