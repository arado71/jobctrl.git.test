using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules.Generation
{
	public class SimpleRuleGenerator : IRuleGenerator
	{
		private readonly bool ignoreCase;

		public SimpleRuleGenerator(bool ignoreCase)
		{
			this.ignoreCase = ignoreCase;
		}

		public IRule GetRuleFromWindow(DesktopWindow aw, IRule matchingRule)
		{
			if (aw == null) return null;
			return new GeneratedRule()
			{
				ProcessRule = aw.ProcessName,
				TitleRule = aw.Title,
				UrlRule = aw.Url,
				IgnoreCase = ignoreCase,
				IsEnabled = true,
				IsRegex = false,
				ExtensionRules = GeneratedRule.GetSimpleExtensionRules(aw, matchingRule, false),
				WindowScope = matchingRule == null ? WindowScopeType.Active : matchingRule.WindowScope,
				Children = GeneratedRule.GetChildrenRules(matchingRule),
			};
		}
	}
}
