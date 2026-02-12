using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Menu;

namespace Tct.ActivityRecorderClient.Rules
{
	/// <summary>
	/// We have to convert WorkDetectorRules into WorkChangingRules,
	/// because for one templated WorkDetectorRule we can have many WorkChangingRules.
	/// But we need both of them, because we show WorkDetectorRules on the GUI but use WorkChangingRules for matching.
	/// </summary>
	/// <remarks>
	/// If we have no placeholders in a templated rule then we won't use that rule.
	/// If there is an unknown placeholder in a templated rule (for a given project/work) then we won't switch to the given project/work.
	/// We cannot decide in advance (for all rules) which work will be seleted as TempStartCategory depends on current work
	/// </remarks>
	public static class WorkChangingRuleFactory
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static IEnumerable<IWorkChangingRule> CreateFrom(IEnumerable<WorkDetectorRule> workDetectorRules, ClientMenuLookup menuLookup)
		{
			return workDetectorRules.SelectMany(n => CreateFrom(n, menuLookup));
		}

		public static IEnumerable<IWorkChangingRule> CreateFrom(WorkDetectorRule workDetectorRule, ClientMenuLookup menuLookup)
		{
			if (workDetectorRule == null || !workDetectorRule.IsEnabled) yield break; //don't create WorkChangingRules for disabled WorkDetectorRules
			switch (workDetectorRule.RuleType)
			{
				case WorkDetectorRuleType.TempStartWork:
				case WorkDetectorRuleType.TempStopWork:
				case WorkDetectorRuleType.TempStartCategory:
				case WorkDetectorRuleType.DoNothing:
				case WorkDetectorRuleType.EndTempEffect:
				case WorkDetectorRuleType.CreateNewRuleAndEndTempEffect:
				case WorkDetectorRuleType.CreateNewRuleAndTempStartWork:
				case WorkDetectorRuleType.TempStartOrAssignWork:
				case WorkDetectorRuleType.TempStartOrAssignProject:
				case WorkDetectorRuleType.TempStartOrAssignProjectAndWork:
					yield return CreateFromWithCommonPopertiesSet(workDetectorRule); //we don't search for placeholders here, it's a one to one relation
					break;
				case WorkDetectorRuleType.TempStartProjectTemplate:
					var dynProjs = menuLookup.ProjectDataById
						.OrderByDescending(n => n.Value.WorkData.Priority ?? -1)
						.ThenByDescending(n => n.Value.WorkData.EndDate ?? DateTime.MinValue)
						.ThenBy(n => n.Value.WorkData.Name)
						.Select(n =>
						{
							var workId = GetWorkIdFromSelector(n.Value.WorkData.ProjectId.Value, menuLookup, workDetectorRule.WorkSelector);
							return workId.HasValue ? new ProjectTemplateData(n.Value.WorkData, workId.Value) : null;
						})
						.Where(n => n != null)
						.Cast<TemplateData>(); //no 4.0 atm.
					foreach (var rule in CreateFromTemplateRule(workDetectorRule, dynProjs))
					{
						yield return rule;
					}
					break;
				case WorkDetectorRuleType.TempStartWorkTemplate:
					var dynWorks = menuLookup.WorkDataById
						.OrderByDescending(n => n.Value.WorkData.Priority ?? -1)
						.ThenByDescending(n => n.Value.WorkData.EndDate ?? DateTime.MinValue)
						.ThenBy(n => n.Value.WorkData.Name)
						.Select(n => new WorkTemplateData(n.Value.WorkData, menuLookup))
						.Cast<TemplateData>(); //no 4.0 atm.
					foreach (var rule in CreateFromTemplateRule(workDetectorRule, dynWorks))
					{
						yield return rule;
					}
					break;
				default:
					Debug.Fail("Invalid rule type");
					yield break;
			}
		}

		public static int? GetWorkIdFromSelector(int projectId, ClientMenuLookup menuLookup, WorkSelector workSelector)
		{
			if (workSelector == null) return null;
			var worksInProject = menuLookup
				.GetWorksForProjectId(projectId)
				.OrderByDescending(n => n.Priority ?? -1)
				.ThenBy(n => n.EndDate ?? DateTime.MaxValue)
				.ThenBy(n => n.Name)
				.Select(n => new WorkTemplateData(n, menuLookup));
			try
			{
				var workRule = RuleMatcher.GetRegexForRule(workSelector.Rule, workSelector.IsRegex, workSelector.IgnoreCase); //there are no placeholders in Rule
				foreach (var workDynamicData in worksInProject)
				{
					string selectorText;
					var validTemplate = PlaceholderHelper.TryReplacePlaceholders(workSelector.TemplateText, workDynamicData.GetTryReplaceFunc(false), out selectorText); //placeholders are in TemplateText
					if (!validTemplate)
					{
						log.Debug("Invalid TemplateText: " + workSelector.TemplateText + " for workId: " + workDynamicData.WorkId); //until all works have the same placeholders it's ok to do a WARN otherwise only DEBUG.
						continue;
					}
					if (workRule.IsMatch(selectorText))
					{
						return workDynamicData.WorkId; //todo warn/debug on more than one match?
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("Cannot get workId from projectId " + projectId, ex);
			}
			log.Debug("Cannot select any work for projectId " + projectId + " and selector " + workSelector);
			return null;
		}

		//14000 rules means +70MB memory and 800ms lag for validation atm. regexes are heavy... so be careful
		private static IEnumerable<IWorkChangingRule> CreateFromTemplateRule(WorkDetectorRule workDetectorRule, IEnumerable<TemplateData> dynamicData)
		{
			Debug.Assert(workDetectorRule != null);
			Debug.Assert(workDetectorRule.RuleType == WorkDetectorRuleType.TempStartWorkTemplate
				|| workDetectorRule.RuleType == WorkDetectorRuleType.TempStartProjectTemplate);
			foreach (var dyn in dynamicData)
			{
				var rule = CreateFromWithCommonPopertiesSet(workDetectorRule);
				rule.RelatedId = dyn.WorkId;
				int allOk, allNok;
				ReplacePlaceholdersForWindowRules(rule, dyn, out allOk, out allNok);
				if (allOk > 0 && allNok == 0) //we only care about rules where all placeholders can be replaced and at least one is replaced
				{
					yield return rule;
				}
			}
		}

		private static void ReplacePlaceholdersForWindowRules(ITemplateRule rule, TemplateData dyn, out int allOk, out int allNok)
		{
			allOk = 0; allNok = 0;
			int thisOk, thisNok;
			rule.UrlRule = PlaceholderHelper.ReplacePlaceholdersWithTryReplaceFunc(rule.UrlRule, dyn.GetTryReplaceFunc(rule.IsRegex), out thisOk, out thisNok);
			allOk += thisOk; allNok += thisNok;
			rule.ProcessRule = PlaceholderHelper.ReplacePlaceholdersWithTryReplaceFunc(rule.ProcessRule, dyn.GetTryReplaceFunc(rule.IsRegex), out thisOk, out thisNok);
			allOk += thisOk; allNok += thisNok;
			rule.TitleRule = PlaceholderHelper.ReplacePlaceholdersWithTryReplaceFunc(rule.TitleRule, dyn.GetTryReplaceFunc(rule.IsRegex), out thisOk, out thisNok);
			allOk += thisOk; allNok += thisNok;
			if (rule.ExtensionRules != null)
			{
				var replacedExtRules = new Dictionary<CaptureExtensionKey, string>();
				foreach (var kvpExtensionRule in rule.ExtensionRules)
				{
					replacedExtRules[kvpExtensionRule.Key] = PlaceholderHelper.ReplacePlaceholdersWithTryReplaceFunc(kvpExtensionRule.Value, dyn.GetTryReplaceFunc(rule.IsRegex), out thisOk, out thisNok);
					allOk += thisOk; allNok += thisNok;
				}
				rule.ExtensionRules = replacedExtRules;
			}
			if (rule.Children != null)
			{
				Debug.Assert(rule.Children.All(n => n is ITemplateRule), "Not all Children are ITemplateRule"); //this is true atm. otherwise we should search for IRules.Children if it is an ITemplateRule 
				foreach (var child in rule.Children.OfType<ITemplateRule>().Where(n => n.IsEnabled))
				{
					ReplacePlaceholdersForWindowRules(child, dyn, out thisOk, out thisNok);
					allOk += thisOk; allNok += thisNok;
				}
			}
		}

		private static WorkChangingRule CreateFromWithCommonPopertiesSet(WorkDetectorRule workDetectorRule)
		{
			return new WorkChangingRule(workDetectorRule);
		}


		public abstract class TemplateData
		{
			private readonly Dictionary<string, string> keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			public int WorkId { get; protected set; }

			protected void AddPlaceholderValue(string key, string value)
			{
				if (string.IsNullOrEmpty(value)) return; //don't add empty values
				Debug.Assert(PlaceholderHelper.IsKeyValid(key));
				keys.Add(key, value);
			}

			public PlaceholderHelper.TryReplaceFunc GetTryReplaceFunc(bool isRegex)
			{
				return isRegex ? (PlaceholderHelper.TryReplaceFunc)TryGetPlaceholderValueEscaped : TryGetPlaceholderValue;
			}

			public bool TryGetPlaceholderValue(string key, out string value)
			{
				return keys.TryGetValue(key, out value);
			}

			public bool TryGetPlaceholderValueEscaped(string key, out string value)
			{
				string valTemp;
				var res = TryGetPlaceholderValue(key, out valTemp);
				value = valTemp == null ? null : key == WorkTemplateData.TemplateRegexKey ? valTemp : Regex.Escape(valTemp);
				return res;
			}
		}

		public class ProjectTemplateData : TemplateData
		{
			public static readonly string ProjectIdKey = "ProjectId";
			public static readonly string ProjectNameKey = "ProjectName";
			public static readonly string TaxIdKey = "TaxId";
			public static readonly string ExtIdKey = "ExtId";

			public ProjectTemplateData(WorkData workData, int workId)
			{
				Debug.Assert(workData != null && workData.ProjectId.HasValue);
				WorkId = workId;
				AddPlaceholderValue(ProjectIdKey, workData.ProjectId.Value.ToString());
				AddPlaceholderValue(ProjectNameKey, workData.Name);
				AddPlaceholderValue(TaxIdKey, workData.TaxId);
				AddPlaceholderValue(ExtIdKey, workData.ExtId.ToString());
			}
		}

		public class WorkTemplateData : TemplateData
		{
			public static readonly string WorkIdKey = "WorkId";
			public static readonly string WorkNameKey = "WorkName";
			public static readonly string CategoryIdKey = "CategoryId";
			public static readonly string CategoryNameKey = "CategoryName";
			public static readonly string TaxIdKey = "TaxId";
			public static readonly string ExtIdKey = "ExtId";
			public static readonly string TemplateRegexKey = "TemplateRegex";

			public WorkTemplateData(WorkData workData, ClientMenuLookup menuLookup)
			{
				Debug.Assert(workData != null && workData.Id.HasValue);
				WorkId = workData.Id.Value;
				AddPlaceholderValue(WorkIdKey, workData.Id.Value.ToString());
				AddPlaceholderValue(WorkNameKey, workData.Name);
				AddPlaceholderValue(CategoryIdKey, workData.CategoryId.ToString());
				AddPlaceholderValue(TaxIdKey, workData.TaxId);
				AddPlaceholderValue(ExtIdKey, workData.ExtId.ToString());
				AddPlaceholderValue(TemplateRegexKey, workData.TemplateRegex);
				CategoryData categoryData;
				if (workData.CategoryId.HasValue && menuLookup.AllCategoriesById.TryGetValue(workData.CategoryId.Value, out categoryData))
				{
					AddPlaceholderValue(CategoryNameKey, categoryData.Name);
				}
			}
		}
	}
}
