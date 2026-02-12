using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;

namespace Tct.ActivityRecorderClient.Rules
{
	public class WorkChangingRule : IWorkChangingRule
	{
		public string Name { get { return OriginalRule.Name; } }
		public bool IsEnabled { get { return OriginalRule.IsEnabled; } }
		public bool IsRegex { get { return OriginalRule.IsRegex; } }
		public bool IgnoreCase { get { return OriginalRule.IgnoreCase; } }
		public bool IsEnabledInNonWorkStatus { get { return OriginalRule.IsEnabledInNonWorkStatus; } }
		public Dictionary<string, string> FormattedNamedGroups { get { return OriginalRule.FormattedNamedGroups; } }
		public string TitleRule { get; set; }
		public string ProcessRule { get; set; }
		public string UrlRule { get; set; }
		public Dictionary<CaptureExtensionKey, string> ExtensionRules { get; set; }
		IEnumerable<KeyValuePair<CaptureExtensionKey, string>> IRule.ExtensionRules { get { return ExtensionRules; } }

		IEnumerable<KeyValuePair<CaptureExtensionKey, string>> ITemplateRule.ExtensionRules
		{
			get { return ExtensionRules; }
			set
			{
				ExtensionRules = value == null
					? null
					: value is Dictionary<CaptureExtensionKey, string>
						? (Dictionary<CaptureExtensionKey, string>)value
						: value.ToDictionaryAllowDuplicates(n => n.Key, n => n.Value);
			}
		}

		public WindowScopeType WindowScope { get { return OriginalRule.WindowScope; } }
		IEnumerable<IRule> IRule.Children { get { return Children == null ? null : Children.OfType<IRule>(); } }
		public List<WindowRule> Children { get; set; }

		public bool IsPermanent { get { return OriginalRule.IsPermanent; } }
		public bool IsTemplate { get { return GetIsTemplate(OriginalRule.RuleType); } }
		public bool IsLearning { get { return GetIsLearning(OriginalRule.RuleType); } }
		public WorkChangingRuleType RuleType { get { return GetCompatibleType(OriginalRule.RuleType); } }
		public WorkDetectorRule OriginalRule { get; private set; }
		public int RelatedId { get; set; }

		public WorkChangingRule(WorkDetectorRule originalRule)
		{
			if (originalRule == null) throw new ArgumentNullException("originalRule");
			OriginalRule = originalRule;
			RelatedId = OriginalRule.RelatedId;
			//we copy these rules seperately so template rules can change them without touching the OriginalRule (otherwise we should clone the OriginalRule before creating a template rule)
			TitleRule = OriginalRule.TitleRule;
			ProcessRule = OriginalRule.ProcessRule;
			UrlRule = OriginalRule.UrlRule;
			if (OriginalRule.ExtensionRules != null)
			{
				var extensionRules = new Dictionary<CaptureExtensionKey, string>();
				foreach (var kvpExtensionRule in OriginalRule.ExtensionRules)
				{
					extensionRules[kvpExtensionRule.Key] = kvpExtensionRule.Value;
				}
				ExtensionRules = extensionRules;
			}
			if (OriginalRule.Children != null && OriginalRule.Children.Count != 0)
			{
				Children = new List<WindowRule>(OriginalRule.Children.Count);
				foreach (var child in OriginalRule.Children)
				{
					Children.Add(child.Clone());
				}
			}
		}

		private static bool GetIsLearning(WorkDetectorRuleType workDetectorRuleType)
		{
			switch (workDetectorRuleType)
			{
				case WorkDetectorRuleType.CreateNewRuleAndEndTempEffect:
				case WorkDetectorRuleType.CreateNewRuleAndTempStartWork:
					return true;
				default:
					return false;
			}
		}

		private static bool GetIsTemplate(WorkDetectorRuleType workDetectorRuleType)
		{
			switch (workDetectorRuleType)
			{
				case WorkDetectorRuleType.TempStartProjectTemplate:
				case WorkDetectorRuleType.TempStartWorkTemplate:
					return true;
				default:
					return false;
			}
		}

		private static WorkChangingRuleType GetCompatibleType(WorkDetectorRuleType workDetectorRuleType)
		{
			switch (workDetectorRuleType)
			{
				case WorkDetectorRuleType.TempStartWork:
				case WorkDetectorRuleType.TempStartProjectTemplate: //converted to a simple StartWork
				case WorkDetectorRuleType.TempStartWorkTemplate: //converted to a simple StartWork
				case WorkDetectorRuleType.CreateNewRuleAndTempStartWork:
					return WorkChangingRuleType.StartWork;
				case WorkDetectorRuleType.TempStopWork:
					return WorkChangingRuleType.StopWork;
				case WorkDetectorRuleType.TempStartCategory:
					return WorkChangingRuleType.StartCategory;
				case WorkDetectorRuleType.DoNothing:
					return WorkChangingRuleType.DoNothing; //might not be marshaled to the gui so learning rules shouldn't use it
				case WorkDetectorRuleType.CreateNewRuleAndEndTempEffect: //i don't think end effect is the proper thing to do anymore...
				case WorkDetectorRuleType.EndTempEffect:
					return WorkChangingRuleType.EndEffect;
				case WorkDetectorRuleType.TempStartOrAssignWork:
					return WorkChangingRuleType.StartOrAssignWork;
				case WorkDetectorRuleType.TempStartOrAssignProject:
					return WorkChangingRuleType.StartOrAssignProject;
				case WorkDetectorRuleType.TempStartOrAssignProjectAndWork:
					return WorkChangingRuleType.StartOrAssignProjectAndWork;
				default:
					Debug.Fail("Invalid rule type");
					throw new ArgumentOutOfRangeException("workDetectorRuleType");
			}
		}

		public override string ToString()
		{
			return Name + " (" + RuleType + ": " + RelatedId + ") " + OriginalRule;
		}
	}

	public enum WorkChangingRuleType
	{
		StartWork = 0,     //WorkId, IsPermanent, IsTemplate, IsLearning
		StopWork,
		DoNothing,
		StartCategory,     //CategoryId, IsPermanent, IsTemplate
		EndEffect,         //IsLearning
		StartOrAssignWork, //IsPermanent, KeySuffix
		StartOrAssignProject, //IsPermanent, KeySuffix, WorkSelector
		StartOrAssignProjectAndWork, //IsPermanent, KeySuffix
	}
}
