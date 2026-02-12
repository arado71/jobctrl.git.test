using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules
{
	/// <summary>
	/// Comparer for WorkDetectorRules because learning rules should always be after all other rules
	/// </summary>
	public class WorkDetectorRuleTypeComparer : Comparer<WorkDetectorRule>
	{
		public override int Compare(WorkDetectorRule x, WorkDetectorRule y)
		{
			return GetRuleValue(x).CompareTo(GetRuleValue(y));
		}

		private static int GetRuleValue(WorkDetectorRule rule)
		{
			return (rule.IsDefault ? 2 : 0) + (RuleManagementService.IsLearning(rule.RuleType) ? 1 : 0);
		}
	}
}
