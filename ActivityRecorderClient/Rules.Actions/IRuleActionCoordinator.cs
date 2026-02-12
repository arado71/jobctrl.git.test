using System;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.Rules.Actions
{
	public interface IRuleActionCoordinator
	{
		bool ContainsRuleAction(IWorkChangingRule rule);

		IDisposable GetExecuter(IWorkChangingRule rule, AssignData assignData) //called on UI thread (Dispose also)
			;
	}
}