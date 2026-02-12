using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.Rules.Collector
{
	//todo solve timestamp problem when creating dynamic works
	/// <summary>
	/// Thread-safe class for collecting data via collector rules from desktop captures.
	/// </summary>
	public class CollectorRuleDetector
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object thisLock = new object();
		private readonly List<RuleMatcherFormatter<CollectorRule>> rules = new List<RuleMatcherFormatter<CollectorRule>>();

		public bool IsEnabled { get { lock (thisLock) { return rules.Count != 0; } } }

		public void SetCollectorRules(CollectorRules collectorRules)
		{
			var newRules = collectorRules == null || collectorRules.Rules == null
				? new RuleMatcherFormatter<CollectorRule>[0]
				: collectorRules.Rules.Select(n => new RuleMatcherFormatter<CollectorRule>(n)).ToArray();

			log.Debug("Loading " + newRules.Length + " server rule" + (newRules.Length == 1 ? "" : "s"));
			lock (thisLock)
			{
				rules.Clear();
				foreach (var rule in newRules)
				{
					rules.Add(rule);
					log.Debug("Loading server rule " + rule.Rule);
				}
			}
		}

		public CollectorRuleDetectorResult DetectCaptures(DesktopCapture desktopCapture)
		{
			DebugEx.EnsureBgThread();
			return new CollectorRuleDetectorResult(DetectCapturesImpl(desktopCapture));
		}

		private static readonly Dictionary<string, string> emptyCapture = new Dictionary<string, string>(0);
		private Dictionary<string, string> DetectCapturesImpl(DesktopCapture desktopCapture)
		{
			DebugEx.EnsureBgThread();
			RuleMatcherFormatter<CollectorRule>[] rulesCopy;
			lock (thisLock)
			{
				rulesCopy = rules.ToArray(); //it is kinda immutable
			}

			Dictionary<string, string> result = null;
			foreach (var rule in rulesCopy)
			{
				var dict = rule.GetFormatted(desktopCapture, rule.Rule.CapturedKeys.ToArray());
				if (result == null)
				{
					result = dict;
				}
				else if (dict != null)
				{
					foreach (var kvp in dict)
					{
						result[kvp.Key] = kvp.Value;
					}
				}
			}
			return result ?? emptyCapture;
		}
	}
}
