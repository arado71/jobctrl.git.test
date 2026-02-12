using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Rules.Generation;

namespace Tct.Tests.ActivityRecorderClient
{
	public static class RuleGeneratorTestsExtensions
	{
		public static IRule GetRuleFromWindow(this IRuleGenerator gen, DesktopWindow desktopWindow) //so we don't have to refactor many unit tests
		{
			return gen.GetRuleFromWindow(desktopWindow, new GeneratedRule()
			{
				IsEnabled = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = "*",
				TitleRule = "*",
				UrlRule = "*",
			});
		}
	}
}
