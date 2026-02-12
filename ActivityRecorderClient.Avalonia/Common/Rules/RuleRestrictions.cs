using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Rules
{
	// todo We should handle when rule creation or override is changed....
	/// <summary>
	/// Restrictions on rules.
	/// </summary>
	/// <remarks>
	/// These restrictions are not water-tight and can be 'hacked' easily atm.
	/// </remarks>
	[Flags]
	public enum RuleRestrictions
	{
		None = 0,
		CannotOverrideRules = 1 << 0,
		CannotCreateOrModifyRules = 1 << 1,
		CanModifyRuleTitle = 1 << 2, //applies only when CannotCreateOrModifyRules
		//CanModifyRuleUrl = 1 << X, //applies only when CannotCreateOrModifyRules
		//CanModifyRuleIsPermanent = 1 << X, //applies only when CannotCreateOrModifyRules
		//CanModifyRulePlugins = 1 << X, //applies only when CannotCreateOrModifyRules
		//CanModifyRuleWindowScope = 1 << X, //applies only when CannotCreateOrModifyRules
		//CanMoveRules = 1 << X, //applies only when CannotCreateOrModifyRules
		CannotUseAnyOkValueForLearningRule = 1 << 3,
		CanUseOkDefault = 1 << 4, //applies only when CannotUseAnyOkValueForLearningRule
		CanUseOkUntilWindowClosed = 1 << 5, //applies only when CannotUseAnyOkValueForLearningRule
		CanUseOkForOneHour = 1 << 6, //applies only when CannotUseAnyOkValueForLearningRule
		CanUseOkForOneDay = 1 << 7, //applies only when CannotUseAnyOkValueForLearningRule
	}
}
