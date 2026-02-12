using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules.Generation
{
	public class ReplaceGroupRuleGenerator : IRuleGenerator
	{
		private readonly bool ignoreCase;
		private readonly Regex processNameRegex;
		private readonly Regex titleRegex;
		private readonly Regex urlRegex;
		private readonly ReplaceGroupParameter[] processNameParams;
		private readonly ReplaceGroupParameter[] titleParams;
		private readonly ReplaceGroupParameter[] urlParams;

		public ReplaceGroupRuleGenerator(bool ignoreCase, IEnumerable<ReplaceGroupParameter> processNameReplaceParameters
			, IEnumerable<ReplaceGroupParameter> titleReplaceParameters
			, IEnumerable<ReplaceGroupParameter> urlReplaceParameters)
		{
			if (processNameReplaceParameters == null || titleReplaceParameters == null || urlReplaceParameters == null) throw new ArgumentNullException();
			this.ignoreCase = ignoreCase;
			processNameParams = processNameReplaceParameters.Select(n => n.Clone()).ToArray(); //not immutable so make defensive copies
			titleParams = titleReplaceParameters.Select(n => n.Clone()).ToArray();
			urlParams = urlReplaceParameters.Select(n => n.Clone()).ToArray();

			processNameRegex = new Regex(string.Concat(processNameParams.Select(n => n.MatchingPattern).ToArray()), GetRegexOptions());
			titleRegex = new Regex(string.Concat(titleParams.Select(n => n.MatchingPattern).ToArray()), GetRegexOptions());
			urlRegex = new Regex(string.Concat(urlParams.Select(n => n.MatchingPattern).ToArray()), GetRegexOptions());
		}

		private static string GetPatternWithReplace(string input, Regex regex, IEnumerable<ReplaceGroupParameter> replaceParams)
		{
			return input == null
				? ".*"
				: regex.Replace(input, match => BuildRegexPattern(match, replaceParams));
		}

		private static string BuildRegexPattern(Match match, IEnumerable<ReplaceGroupParameter> replaceParams)
		{
			Debug.Assert(match.Success);
			if (!match.Success) return match.ToString();
			return string.Concat(replaceParams
				.Select(n => n.ReplaceGroupName == null
								? n.MatchingPattern
								: Regex.Escape(match.Groups[n.ReplaceGroupName].Value))
				.ToArray());
		}

		public IRule GetRuleFromWindow(DesktopWindow aw, IRule matchingRule)
		{
			if (aw == null
				|| !processNameRegex.IsMatch(aw.ProcessName ?? "")
				|| !titleRegex.IsMatch(aw.Title ?? "")
				|| !urlRegex.IsMatch(aw.Url ?? ""))
			{
				return null;
			}
			return new GeneratedRule()
			{
				ProcessRule = GetPatternWithReplace(aw.ProcessName, processNameRegex, processNameParams),
				TitleRule = GetPatternWithReplace(aw.Title, titleRegex, titleParams),
				UrlRule = GetPatternWithReplace(aw.Url, urlRegex, urlParams),
				IgnoreCase = ignoreCase,
				IsEnabled = true,
				IsRegex = true,
				ExtensionRules = GeneratedRule.GetSimpleExtensionRules(aw, matchingRule, true),
				WindowScope = matchingRule == null ? WindowScopeType.Active : matchingRule.WindowScope,
				Children = GeneratedRule.GetChildrenRules(matchingRule),
			};
		}

		private RegexOptions GetRegexOptions()
		{
			return RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
		}
	}
}
