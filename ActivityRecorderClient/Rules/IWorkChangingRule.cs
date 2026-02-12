using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules
{
	public interface IWorkChangingRule : ITemplateRule, IFormattedRule
	{
		WorkChangingRuleType RuleType { get; }
		int RelatedId { get; set; }
		bool IsPermanent { get; }
		bool IsTemplate { get; }
		bool IsLearning { get; }
		bool IsEnabledInNonWorkStatus { get; }
		WorkDetectorRule OriginalRule { get; }
	}
}
