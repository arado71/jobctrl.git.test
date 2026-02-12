using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules.Generation
{
	public class IgnoreRuleGenerator : IRuleGenerator
	{
		private readonly bool ignoreCase;
		private readonly Regex processNameRegex;
		private readonly Regex titleRegex;
		private readonly Regex urlRegex;
		private readonly bool processNameNegateMatch; //we need these for performance reasons
		private readonly bool titleNegateMatch;
		private readonly bool urlNegateMatch;

		public IgnoreRuleGenerator(bool ignoreCase, IgnoreRuleMatchParameter processNameParam, IgnoreRuleMatchParameter titleParam, IgnoreRuleMatchParameter urlParam)
		{
			if (processNameParam == null || titleParam == null || urlParam == null) throw new ArgumentNullException();
			this.ignoreCase = ignoreCase;
			processNameRegex = new Regex(processNameParam.MatchingPattern, GetRegexOptions());
			titleRegex = new Regex(titleParam.MatchingPattern, GetRegexOptions());
			urlRegex = new Regex(urlParam.MatchingPattern, GetRegexOptions());
			processNameNegateMatch = processNameParam.NegateMatch;
			titleNegateMatch = titleParam.NegateMatch;
			urlNegateMatch = urlParam.NegateMatch;
		}

		private RegexOptions GetRegexOptions()
		{
			return RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
		}

		public IRule GetRuleFromWindow(DesktopWindow aw, IRule matchingRule)
		{
			return aw != null
				&& (processNameRegex.IsMatch(aw.ProcessName ?? "") ^ processNameNegateMatch)
				&& (titleRegex.IsMatch(aw.Title ?? "") ^ titleNegateMatch)
				&& (urlRegex.IsMatch(aw.Url ?? "") ^ urlNegateMatch)
					? GeneratedRule.Empty
					: null;
		}
	}
}
