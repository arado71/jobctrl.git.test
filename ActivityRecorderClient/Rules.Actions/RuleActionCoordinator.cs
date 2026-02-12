using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderService;

namespace Tct.ActivityRecorderClient.Rules.Actions
{
	/// <summary>
	/// Class for executing matching rule's action at the end of rule processing.
	/// </summary>
	public class RuleActionCoordinator : IRuleActionCoordinator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly VoxCtrlController voxCtrlController = new VoxCtrlController();

		public bool ContainsRuleAction(IWorkChangingRule rule)
		{
			return rule != null
				&& rule.OriginalRule != null
				&& rule.OriginalRule.AdditionalActions != null
				&& rule.OriginalRule.AdditionalActions.Count > 0;
		}

		public IDisposable GetExecuter(IWorkChangingRule rule, AssignData assignData) //called on UI thread (Dispose also)
		{
			if (!ContainsRuleAction(rule)) return null;
			return new Executer(() =>
			{
				foreach (var ruleAction in rule.OriginalRule.AdditionalActions)
				{
					ExecuteAction(ruleAction, rule, assignData);
				}
			});
		}

		private void ExecuteAction(string ruleAction, IWorkChangingRule rule, AssignData assignData)
		{
			switch (ruleAction)
			{
				case WorkDetectorRuleActions.VoxCtrlStartRecording:
					voxCtrlController.StartRecording(assignData);
					break;
				case WorkDetectorRuleActions.VoxCtrlSetName:
					voxCtrlController.SetName(assignData);
					break;
				case WorkDetectorRuleActions.VoxCtrlStopRecording:
					voxCtrlController.StopRecording();
					break;
				default:
					log.Warn("Invalid rule action " + ruleAction);
					break;
			}
		}

		private sealed class Executer : IDisposable
		{
			private readonly Action disposeAction;

			public Executer(Action disposeAction)
			{
				if (disposeAction == null) throw new ArgumentNullException("disposeAction");
				this.disposeAction = disposeAction;
			}

			public void Dispose()
			{
				disposeAction();
			}
		}
	}
}
