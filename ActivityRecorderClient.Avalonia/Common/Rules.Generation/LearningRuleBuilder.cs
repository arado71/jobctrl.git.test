using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules.Generation
{
	/// <summary>
	/// Thread-safe class for building WorkDetectorRules from DesktopCaptures
	/// </summary>
	public class LearningRuleBuilder
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly IRuleGenerator lastGenerator = new SimpleRuleGenerator(true);
		private readonly object thisLock = new object();
		private IRuleGenerator[] generators = new[] { lastGenerator, };

		public WorkDetectorRule GetLearingRule(IWorkChangingRule matchingRule, DesktopCapture desktopCapture, DesktopWindow matchedWindow, out string cancelKey, out Rectangle matchingRectangle)
		{
			Debug.Assert(matchedWindow != null); //cannot be null because the rule is macthed before if we get here
			IRuleGenerator[] generatorsCopy;
			lock (thisLock)
			{
				generatorsCopy = generators;
			}
			var rule = generatorsCopy.Select(n => n.GetRuleFromWindow(matchedWindow, matchingRule)).Where(n => n != null).FirstOrDefault(); //Generators are immutable / thread-safe
			if (rule == null)
			{
				cancelKey = null;
				matchingRectangle = Rectangle.Empty;
				return null;
			}
			//todo remove this old cancel thing...
			cancelKey = matchedWindow.ProcessName + "|" + matchedWindow.Title + Environment.NewLine + matchedWindow.Url;
			matchingRectangle = matchedWindow.WindowRect;
			if (desktopCapture.Screens != null && desktopCapture.Screens.Count != 0)
			{
				var visible = desktopCapture.Screens.Any(n => n.Bounds.IntersectsWith(matchedWindow.WindowRect));
				if (!visible) matchingRectangle = desktopCapture.Screens[0].Bounds; //if the window is not visible center it on the primary screen (not water-tight it is still possible that the window is visible but the learning rule popup won't be)
			}
			return new WorkDetectorRule()
			{
				CreatedFromLearningRule = true,
				ProcessRule = rule.ProcessRule,
				UrlRule = rule.UrlRule,
				TitleRule = rule.TitleRule,
				ExtensionRules = rule.ExtensionRules, //set generated extension rules
				RelatedId = matchingRule.RelatedId,
				IsPermanent = matchingRule.IsPermanent,
				IsEnabledInNonWorkStatus = matchingRule.IsEnabledInNonWorkStatus,
				IgnoreCase = rule.IgnoreCase, //overwrite matchingRule.IgnoreCase! we need do work even if the user don't want it...
				IsEnabled = rule.IsEnabled,
				IsRegex = rule.IsRegex,
				RuleType = WorkDetectorRuleType.TempStartWork //usually we want to start a work
			};
		}

		public void SetGenerators(IEnumerable<RuleGeneratorData> learningRuleGenerators)
		{
			var localLearningRuleGenerators = learningRuleGenerators.ToArray(); //don't enumerate twice
			var gens = localLearningRuleGenerators
				.Select(n => RuleGeneratorFactory.CreateGeneratorFromData(n))
				.Concat(new[] { lastGenerator }) //ensure we always have a match
				.ToArray();
			lock (thisLock)
			{
				generators = gens;
			}
			foreach (var learningRuleGenerator in localLearningRuleGenerators)
			{
				log.Info("Loaded generator " + learningRuleGenerator.Name + " p:" + learningRuleGenerator.Parameters);
			}
		}
	}
}
#region TestData
/*
			gens = new IRuleGenerator[] { 
				new ReplaceGroupRuleGenerator(true, 
					new [] { new ReplaceGroupParameter() { MatchingPattern = "^baretail[.]exe$" },},
					new [] { 
						new ReplaceGroupParameter() { MatchingPattern = "^" },
						new ReplaceGroupParameter() { MatchingPattern = "(?<file>.+?)", ReplaceGroupName = "file" },
						new ReplaceGroupParameter() { MatchingPattern = @" \(\d+,\d+\ \w+\) - BareTail$" },
					},
					 new [] { new ReplaceGroupParameter() { MatchingPattern = "^.*$" },}
				),
				new IgnoreRuleGenerator(true, 
					new IgnoreRuleMatchParameter() { MatchingPattern = "^baretail[.]exe$" },
					new IgnoreRuleMatchParameter() { MatchingPattern = "^.*$" },
					new IgnoreRuleMatchParameter() { MatchingPattern = "^.*$" }
				),
			}.Concat(gens).ToArray();
*/
#endregion