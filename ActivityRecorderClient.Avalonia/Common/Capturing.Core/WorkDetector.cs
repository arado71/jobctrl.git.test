using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Thread-safe work detector class
	/// </summary>
	public class WorkDetector
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string WorkDetectorSettingsPath { get { return "WorkDetectorUserSettings-" + ConfigManager.UserId; } }
		private const int assignWorkKeyLength = 50;
		private const int assignProjectKeyLength = 200;
		private const int assignProjectAndWorkKeyLength = 200;
		private static readonly string[] dynamicKeys = { WorkDetectorRule.GroupNameWorkName, WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameProjectName, WorkDetectorRule.GroupNameProjectKey, WorkDetectorRule.GroupNameDescription };

		private readonly WorkDetectorSettings settings = new WorkDetectorSettings();
		private readonly object thisLock = new object();
		private readonly List<RuleMatcherFormatter<IWorkChangingRule>> currentUserRules = new List<RuleMatcherFormatter<IWorkChangingRule>>();
		private readonly List<RuleMatcherFormatter<IWorkChangingRule>> currentServerRules = new List<RuleMatcherFormatter<IWorkChangingRule>>();
		private readonly List<RuleMatcherFormatter<IWorkChangingRule>> currentRules = new List<RuleMatcherFormatter<IWorkChangingRule>>();
		private readonly ClientMenuLookup menuLookup = new ClientMenuLookup();

		public event EventHandler<SingleValueEventArgs<int[]>> WorkIdsMissing;

		public Keys? HotKey
		{
			get
			{
				lock (thisLock)
				{
					return settings.HotKey;
				}
			}
			set
			{
				lock (thisLock)
				{
					settings.HotKey = value;
				}
			}
		}

		/// <summary>
		/// Detects the first matching rule, matched window, assign data, and validity deadline.
		/// </summary>
		/// <param name="desktopCapture">Captured desktop state to detect.</param>
		/// <param name="isWorking">The recent work state</param>
		/// <param name="matchedWindow">The first <see cref="DesktopWindow"/> matched by the first rule.</param>
		/// <param name="assignData">Dynamic task key if first matching rule is a dynamic rule, else null.</param>
		/// <param name="validUntil"></param>
		/// <param name="matchingRuleIsEnabledInNonWorkStatus"></param>
		/// <returns>The first matching <see cref="IWorkChangingRule"/> rule.</returns>
		public IWorkChangingRule DetectWork(DesktopCapture desktopCapture, bool isWorking, out DesktopWindow matchedWindow, out AssignData assignData, out DateTime? validUntil, out bool matchingRuleIsEnabledInNonWorkStatus)
		{
			if (desktopCapture == null) throw new ArgumentNullException("desktopCapture");
			IEnumerable<RuleMatcherFormatter<IWorkChangingRule>> rulesCopy;
			lock (thisLock)
			{
				rulesCopy = currentRules.ToArray(); //it is kinda immutable
			}
			DesktopWindow matchedWin = null;
			var first = rulesCopy
				.Where(n => !IsRuleOutdated(n.Rule.OriginalRule))
				.Where(n => n.IsMatch(desktopCapture, out matchedWin))
				.FirstOrDefault();
			matchedWindow = matchedWin;
			if (!isWorking) { 
				var firstEnabledInNonWork = rulesCopy
					.Where(n => !IsRuleOutdated(n.Rule.OriginalRule))
					.Where(n => n.IsMatch(desktopCapture, out matchedWin))
					.FirstOrDefault(n => n.Rule.IsEnabledInNonWorkStatus);
				matchingRuleIsEnabledInNonWorkStatus = first != null && firstEnabledInNonWork != null && RuleTypesCauseSameWorkState(first.Rule.RuleType, firstEnabledInNonWork.Rule.RuleType);
			}
			else matchingRuleIsEnabledInNonWorkStatus = false;
			Debug.Assert(first == null && matchedWindow == null || first != null && matchedWindow != null);
			//since we only have a ValidUntilDate (and no ValidFromDate) so we can be sure that only the selected rule's validity can change the value of the detection
			//[i.e. if a rule earlier in the chain becomes invalid it doesn't matter]
			validUntil = first == null ? null : first.Rule.OriginalRule.ValidUntilDate;
			//race can occur here as the menu could be changed while we do the matching so the event will be raised with the old menu
			//if we want to release the lock while doing the matching we have to check if the state has changed since we started
			//Since we are using CaptureCoordinator to prevent this we won't need this check anymore
			assignData = first != null ? GetAssignData(desktopCapture, first) : null; //the race would appear here too, but thx to CaptureCoordinator we don't care
			return first != null ? first.Rule : null;
		}

		private bool RuleTypesCauseSameWorkState(WorkChangingRuleType type1, WorkChangingRuleType type2)
		{
			switch (type1)
			{
				case WorkChangingRuleType.DoNothing:
					return type2 == WorkChangingRuleType.DoNothing;
				case WorkChangingRuleType.StartCategory:
				case WorkChangingRuleType.StartOrAssignProject:
				case WorkChangingRuleType.StartOrAssignProjectAndWork:
				case WorkChangingRuleType.StartOrAssignWork:
				case WorkChangingRuleType.StartWork:
					return type2 != WorkChangingRuleType.StopWork;
				case WorkChangingRuleType.StopWork:
					return type2 == WorkChangingRuleType.StopWork;
				default:
					return type2 == type1;
			}
		}

		private AssignData GetAssignData(DesktopCapture desktopCapture, RuleMatcherFormatter<IWorkChangingRule> matcher)
		{
			AssignData assignData = null;
			switch (matcher.Rule.RuleType)
			{
				case WorkChangingRuleType.StartOrAssignWork:
					assignData = GetAssignWorkData(desktopCapture, matcher);
					break;
				case WorkChangingRuleType.StartOrAssignProject:
					assignData = GetAssignProjectData(desktopCapture, matcher);
					break;
				case WorkChangingRuleType.StartOrAssignProjectAndWork:
					assignData = GetAssignProjectAndWorkData(desktopCapture, matcher);
					break;
			}
			SetCommonData(desktopCapture, matcher, ref assignData);
			return assignData;
		}

		private static bool TryGetAndCombineKeys(Dictionary<string, string> dict, string groupName, string suffix, int maxLen, out string key)
		{
			return dict.TryGetValue(groupName, out key) && TryCombineKeys(ref key, suffix, maxLen);
		}

		internal static bool TryCombineKeys(ref string key, string suffix, int maxLen)
		{
			if (key.IsNullOrWhiteSpace()) return false;
			suffix = suffix ?? ""; //don't trim suffix (key can still end with whitesapce if suffix ends with whitespace)
			key = key.Trim();
			if (maxLen - suffix.Length <= 0) return false;
			key = key.Truncate(maxLen - suffix.Length) + suffix;
			return true;
		}

		internal AssignData GetAssignWorkData(DesktopCapture desktopCapture, RuleMatcherFormatter<IWorkChangingRule> matcher)
		{
			Debug.Assert(matcher.Rule.RuleType == WorkChangingRuleType.StartOrAssignWork);
			//for StartOrAssignWork we do the matching once again, but now we get additional info this time (not ideal, but ok atm.)
			var dict = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameWorkName,
				WorkDetectorRule.GroupNameDescription);
			Debug.Assert(dict != null); //since this is a rematch it should not be null
			string key;
			//concat keys starts with GroupNameWorkKey? (not sure if we need that)
			if (!TryGetAndCombineKeys(dict, WorkDetectorRule.GroupNameWorkKey, matcher.Rule.OriginalRule.KeySuffix, assignWorkKeyLength, out key))
			{
				return null;
			}
			string name;
			if (!dict.TryGetValue(WorkDetectorRule.GroupNameWorkName, out name))
			{
				name = null;
			}
			string description;
			if (!dict.TryGetValue(WorkDetectorRule.GroupNameDescription, out description))
			{
				description = null;
			}
			var result = new AssignData(new AssignWorkData() { WorkKey = key, ServerRuleId = matcher.Rule.OriginalRule.ServerId, WorkName = name, Description = description });
			lock (thisLock)
			{
				WorkDataWithParentNames work;
				if (menuLookup.WorkDataExtMapping.TryGetValue(key, out work))
				{
					result.Work.WorkId = work.WorkData.Id;
				}
				else if (menuLookup.IgnoredWorkExtMapping.Contains(key))
				{
					return null;
				}
			}
			return result;
		}

		private AssignData GetAssignProjectData(DesktopCapture desktopCapture, RuleMatcherFormatter<IWorkChangingRule> matcher)
		{
			Debug.Assert(matcher.Rule.RuleType == WorkChangingRuleType.StartOrAssignProject);
			//for StartOrAssignProject we do the matching once again, but now we get additional info this time (not ideal, but ok atm.)
			var dict = matcher.GetFormatted(desktopCapture, WorkDetectorRule.GroupNameProjectKey, WorkDetectorRule.GroupNameProjectName, WorkDetectorRule.GroupNameDescription);
			Debug.Assert(dict != null); //since this is a rematch it should not be null
			string key;
			if (!TryGetAndCombineKeys(dict, WorkDetectorRule.GroupNameProjectKey, matcher.Rule.OriginalRule.KeySuffix, assignProjectKeyLength, out key))
			{
				return null;
			}
			string name;
			if (!dict.TryGetValue(WorkDetectorRule.GroupNameProjectName, out name))
			{
				name = null;
			}
			string description;
			if (!dict.TryGetValue(WorkDetectorRule.GroupNameDescription, out description))
			{
				description = null;
			}
			var result = new AssignData(new AssignProjectData() { ProjectKey = key, ServerRuleId = matcher.Rule.OriginalRule.ServerId, ProjectName = name, Description = description });
			lock (thisLock)
			{
				WorkDataWithParentNames project;
				if (menuLookup.ProjectExtMapping.TryGetValue(key, out project))
				{
					int projectId = project.WorkData.ProjectId.Value;
					result.Project.ProjectId = projectId; //indicate that we have have a matching mapping (even if the WorkSelector or easy path cannot find a work)
					if (matcher.Rule.OriginalRule.WorkSelector == null) //select the first work as an easy path
					{
						if (project.WorkData.Children != null)
						{
							var work = project.WorkData.Children.Where(n => n.Id.HasValue).FirstOrDefault(); //we only care about the first level in this case (don't recurse)
							if (work != null)
							{
								result.Project.WorkId = work.Id;
							}
						}
					}
					else
					{
						result.Project.WorkId = WorkChangingRuleFactory.GetWorkIdFromSelector(projectId, menuLookup, matcher.Rule.OriginalRule.WorkSelector);
					}
				}
				else if (menuLookup.IgnoredProjectExtMapping.Contains(key))
				{
					return null;
				}
			}
			return result;
		}

		/// <summary>
		/// Creates composite type <see cref="AssignData"/>.
		/// </summary>
		/// <param name="desktopCapture"></param>
		/// <param name="matcher"></param>
		/// <returns></returns>
		internal AssignData GetAssignProjectAndWorkData(DesktopCapture desktopCapture, RuleMatcherFormatter<IWorkChangingRule> matcher)
		{
			Debug.Assert(matcher.Rule.RuleType == WorkChangingRuleType.StartOrAssignProjectAndWork);
			var projGroupNames = matcher.GetGroupNames().Concat(matcher.GetFormatterKeys()).Where(n => n.StartsWith(WorkDetectorRule.GroupNameProjectKey)).Distinct().OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray();
			var groups = new[] { WorkDetectorRule.GroupNameWorkKey, WorkDetectorRule.GroupNameWorkName, WorkDetectorRule.GroupNameDescription }.Concat(projGroupNames).ToArray();
			var dict = matcher.GetFormatted(desktopCapture, groups);
			Debug.Assert(dict != null); //since this is a rematch it should not be null
			string key;
			if (!TryGetAndCombineKeys(dict, WorkDetectorRule.GroupNameWorkKey, matcher.Rule.OriginalRule.KeySuffix, assignProjectAndWorkKeyLength, out key))
			{
				return null;
			}
			string name;
			if (!dict.TryGetValue(WorkDetectorRule.GroupNameWorkName, out name))
			{
				name = null;
			}
			string description;
			if (!dict.TryGetValue(WorkDetectorRule.GroupNameDescription, out description))
			{
				description = null;
			}
			var projectKeys = new List<string>(projGroupNames.Length);
			var keyNotFound = false; //keys at the end can be optional (but not in the middle)
			for (int i = 0; i < projGroupNames.Length; i++)
			{
				string currProjKey;
				if (!TryGetAndCombineKeys(dict, projGroupNames[i], matcher.Rule.OriginalRule.KeySuffix, assignProjectAndWorkKeyLength, out currProjKey))
				{
					keyNotFound = true;
				}
				else
				{
					if (keyNotFound) return null;
					projectKeys.Add(currProjKey);
				}
			}

			var result = new AssignData(new AssignCompositeData() { WorkKey = key, ServerRuleId = matcher.Rule.OriginalRule.ServerId, WorkName = name, ProjectKeys = projectKeys, Description = description });
			lock (thisLock)
			{
				bool ignored;
				var work = menuLookup.GetWorkForCompositeKey(key, projectKeys, out ignored);
				if (work != null)
				{
					result.Composite.WorkId = work.WorkData.Id;
				}
				else if (ignored)
				{
					return null;
				}
			}
			return result;
		}

		private void SetCommonData(DesktopCapture desktopCapture, RuleMatcherFormatter<IWorkChangingRule> matcher, ref AssignData assignData)
		{
			var fields = matcher.GetFormatterKeys().Except(dynamicKeys).ToArray();
			if (fields.Length == 0) return;
			var dict = matcher.GetFormatted(desktopCapture, fields);
			if (assignData == null) assignData = new AssignData(new AssignCommonData(dict));
			else assignData.SetAssignData(new AssignCommonData(dict));
		}

		public void UpdateMenu(ClientMenu clientMenu)
		{
			log.Info("Updating rules due to menu change...");
			lock (thisLock)
			{
				menuLookup.ClientMenu = clientMenu;
				//refresh rules
				SetServerRules(GetServerRules());
				SetUserRules(GetUserRules());
			}
			log.Info("Updated rules");
		}

		public void LoadSettings(ClientMenu clientMenu, Func<WorkDetectorRule, bool> removeUserRulesFilter)
		{
			lock (thisLock)
			{
				menuLookup.ClientMenu = clientMenu;
			}
			if (!IsolatedStorageSerializationHelper.Exists(WorkDetectorSettingsPath)) return;
			WorkDetectorSettings settingsLoaded;
			if (IsolatedStorageSerializationHelper.Load(WorkDetectorSettingsPath, out settingsLoaded) && settingsLoaded != null)
			{
				RemoveMatchingRules(settingsLoaded.UserRules, removeUserRulesFilter, "filtered user");
				lock (thisLock)
				{
					SetServerRules(settingsLoaded.ServerRules);
					SetUserRules(settingsLoaded.UserRules);
					settings.HotKey = settingsLoaded.HotKey;
				}
			}
		}

		public bool SaveSettings()
		{
			WorkDetectorSettings settingsToSave;
			lock (thisLock)
			{
				settingsToSave = settings.Clone();
			}
			return IsolatedStorageSerializationHelper.Save(WorkDetectorSettingsPath, settingsToSave);
		}

		public List<WorkDetectorRule> GetServerRules()
		{
			lock (thisLock)
			{
				var result = new List<WorkDetectorRule>(settings.ServerRules.Count);
				foreach (var rule in settings.ServerRules)
				{
					result.Add(rule.Clone());
				}
				return result;
			}
		}

		//we can only return a copy of the rules (because one can only set rules via SetXXXRules)
		public List<WorkDetectorRule> GetUserRules()
		{
			lock (thisLock)
			{
				var result = new List<WorkDetectorRule>(settings.UserRules.Count);
				foreach (var rule in settings.UserRules)
				{
					result.Add(rule.Clone());
				}
				return result;
			}
		}

		public void SetServerRules(List<WorkDetectorRule> newRules)
		{
			if (newRules == null) return;
			//not used at server rules//RemoveMatchingRules(newRules, IsRuleOutdated, "outdated server");
			log.Info("Loading " + newRules.Count + " server rule" + (newRules.Count == 1 ? "" : "s"));
			int[] unassignedWorkIds;
			lock (thisLock)
			{
				settings.ServerRules.Clear();
				currentServerRules.Clear();
				foreach (var newRule in newRules.OrderBy(n => n, new WorkDetectorRuleTypeComparer()).Select(n => n.Clone())) //order so learning rules are at the end, clone before adding
				{
					settings.ServerRules.Add(newRule);
					currentServerRules.AddRange(GetWorkChangingRules("server", newRule, menuLookup));
				}
				currentRules.Clear();
				currentRules.AddRange(currentServerRules.Concat(currentUserRules).OrderBy(n => n.Rule.OriginalRule, new WorkDetectorRuleTypeComparer()));
				unassignedWorkIds = currentServerRules
					.Where(n => n.Rule.RuleType == WorkChangingRuleType.StartWork)
					//obvious that this is from server so don't need .Where(n => MenuCoordinator.IsWorkIdFromServer(n.Rule.RelatedId))
					.Where(n => !menuLookup.WorkDataById.ContainsKey(n.Rule.RelatedId))
					.Select(n => n.Rule.RelatedId)
					.ToArray();
			}
			if (unassignedWorkIds.Length == 0) return;
			log.Info("Found missing workIds: " + string.Join(",", unassignedWorkIds.Select(n => n.ToString()).ToArray()));
			OnWorkIdsMissing(unassignedWorkIds);
		}

		public void SetUserRules(List<WorkDetectorRule> newRules)
		{
			if (newRules == null) return;
			RemoveMatchingRules(newRules, IsRuleOutdated, "outdated user");
			log.Info("Loading " + newRules.Count + " user rule" + (newRules.Count == 1 ? "" : "s"));
			lock (thisLock)
			{
				settings.UserRules.Clear();
				currentUserRules.Clear();
				foreach (var newRule in newRules.OrderBy(n => n, new WorkDetectorRuleTypeComparer()).Select(n => n.Clone())) //order so learning rules are at the end, clone before adding
				{
					if (newRule.UrlRule == null) newRule.UrlRule = newRule.IsRegex ? ".*" : "*"; //set default value and avoid null so the rules grid will display the proper value
					settings.UserRules.Add(newRule);
					//Filter for applicable rules ? So invalid rules won't trigger error popups ? (don't know if we want that...)
					currentUserRules.AddRange(GetWorkChangingRules("user", newRule, menuLookup));
				}
				currentRules.Clear();
				currentRules.AddRange(currentServerRules.Concat(currentUserRules).OrderBy(n => n.Rule.OriginalRule, new WorkDetectorRuleTypeComparer()));
			}
		}

		public void AddUserRule(WorkDetectorRule rule)
		{
			if (rule == null) return;
			log.Info("Adding user rule " + rule);
			lock (thisLock)
			{
				var newRules = GetUserRules();
				newRules.Add(rule);
				SetUserRules(newRules);
			}
		}

		public void RemoveUserRules(Func<WorkDetectorRule, bool> predicate)
		{
			if (predicate == null) return;
			lock (thisLock)
			{
				var newRules = GetUserRules();
				RemoveMatchingRules(newRules, predicate, "user");
				SetUserRules(newRules);
			}
			SaveSettings();
		}

		public static bool IsRuleOutdated(WorkDetectorRule rule)
		{
			Debug.Assert(rule != null);
			return rule.ValidUntilDate.HasValue && rule.ValidUntilDate.Value < DateTime.UtcNow;
		}

		public void ClearLearningRuleTimers()
		{
			lock (thisLock)
			{
				foreach (var rule in currentUserRules.Where(r => r.Rule?.OriginalRule?.CreatedFromLearningRule ?? false))
				{
					rule.Rule.OriginalRule.ValidUntilDate = DateTime.MinValue;
				}
			}
			log.Debug("Learning rule timers cleared");
		}

		//RemoveAll with logging
		private static void RemoveMatchingRules(List<WorkDetectorRule> rules, Func<WorkDetectorRule, bool> predicate, string type)
		{
			Debug.Assert(rules != null && predicate != null);
			for (int i = 0; i < rules.Count; i++)
			{
				if (!predicate(rules[i])) continue;
				log.Info("Removing " + type + " rule " + rules[i]);
				rules.RemoveAt(i--);
			}
		}

		private static readonly RuleMatcherFormatter<IWorkChangingRule>[] emptyWorkChangingRules = Enumerable.Empty<RuleMatcherFormatter<IWorkChangingRule>>().ToArray();

		private static RuleMatcherFormatter<IWorkChangingRule>[] GetWorkChangingRules(string ruleType, WorkDetectorRule newRule, ClientMenuLookup menuLookup)
		{
			try
			{
				var newChangingRules = newRule.ValidateAndGetMatchers(menuLookup);
				if (newChangingRules.Length == 0) //until all works/projects have the same placeholders it's ok to do a WARN otherwise only DEBUG.
				{
					if (!newRule.IsEnabled)
					{
						log.Info("Skipped to load " + ruleType + " rule " + newRule.ToDetailedString());
					}
					else
					{
						log.Warn("Unable to load " + ruleType + " rule " + newRule.ToDetailedString());
					}
				}
				else
				{
					log.Info("Loading " + ruleType + " rule (" + newChangingRules.Length + ") " + newRule.ToDetailedString());
					return newChangingRules;
				}
			}
			catch (Exception ex)
			{
				log.Error("Invalid " + ruleType + " rule " + newRule.ToDetailedString(), ex);
			}
			return emptyWorkChangingRules;
		}

		private void OnWorkIdsMissing(int[] unassignedWorkIds)
		{
			var del = WorkIdsMissing;
			if (del != null) del(this, SingleValueEventArgs.Create(unassignedWorkIds));
		}

		[DataContract]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public partial class WorkDetectorSettings : ICloneable
		{
			[DataMember]
			public List<WorkDetectorRule> UserRules { get; private set; } = new List<WorkDetectorRule>();
			[DataMember]
			public List<WorkDetectorRule> ServerRules { get; private set; } = new List<WorkDetectorRule>();
			[DataMember]
			public Keys? HotKey { get; set; }

			public WorkDetectorSettings()
			{
			}

			#region ICloneable Members

			public WorkDetectorSettings Clone()
			{
				var result = new WorkDetectorSettings() { HotKey = HotKey };
				result.UserRules.AddRange(UserRules);
				result.ServerRules.AddRange(ServerRules);
				return result;
			}

			object ICloneable.Clone()
			{
				return Clone();
			}

			#endregion
		}
	}
}
