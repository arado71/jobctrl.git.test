using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules
{
	public interface IRuleManagementService : IDisposable
	{
		void DisplayWorkDetectorRulesEditingGui(bool hotKeyPressed);
		void DisplayLearnRuleFromCaptureGui(IWorkChangingRule matchingRule, DesktopCapture desktopCapture, DesktopWindow matchedWindow);
		void DisplayWorkDetectorRuleDeletingGui();
		void SetLearningRuleGenerators(IEnumerable<RuleGeneratorData> learningRuleGenerators);
		bool ShouldSkipLoadingUserRule(WorkDetectorRule rule);
	}
}
