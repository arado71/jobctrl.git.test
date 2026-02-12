using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Rules;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class WindowRule : ITemplateRule
	{
		IEnumerable<IRule> IRule.Children { get { return null; } } //no need for this atm.

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

		public static WindowRule CreateFromIRule(IRule rule) //children's childen are ignored atm. (because there cannot be any)
		{
			if (rule == null) return null;
			Debug.Assert(rule.Children == null || !rule.Children.Any(), "Child exists, but ignored");
			return new WindowRule()
			{
				IsEnabled = rule.IsEnabled,
				IgnoreCase = rule.IgnoreCase,
				IsRegex = rule.IsRegex,
				Name = rule.Name,
				ProcessRule = rule.ProcessRule,
				TitleRule = rule.TitleRule,
				UrlRule = rule.UrlRule,
				WindowScope = rule.WindowScope,
				ExtensionRules = rule.ExtensionRules,
			};
		}

		public override string ToString()
		{
			return ProcessRule + " " + TitleRule + " " + UrlRule;
		}

		public WindowRule Clone()
		{
			return CreateFromIRule(this);
		}
	}
}
