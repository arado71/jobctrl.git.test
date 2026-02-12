using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Rules;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
    public partial class WorkDetectorRule : IEquatable<WorkDetectorRule>, IPluginRule
	{
		public static readonly string GroupNameWorkKey = "workkey";
		public static readonly string GroupNameProjectKey = "projectkey";
		public static readonly string GroupNameWorkName = "workname";
		public static readonly string GroupNameProjectName = "projectname";
		public static readonly string GroupNameDescription = "desc";

		private DateTime? CreateDateField; //use this name so changing this to a datamember won't cause issues
		[DataMember(IsRequired = false)]
		public DateTime? CreateDate
		{
			get { return CreateDateField; }
			set { UpdateField(ref CreateDateField, value, "CreateDate"); }
		}

		private DateTime? UpdateDateField;
		[DataMember(IsRequired = false)]
		public DateTime? UpdateDate
		{
			get { return UpdateDateField ?? CreateDateField; }
			set { UpdateField(ref UpdateDateField, value, "UpdateDate"); }
		}

		private bool CreatedFromLearningRuleField;
		[DataMember(IsRequired = false)]
		public bool CreatedFromLearningRule
		{
			get { return CreatedFromLearningRuleField; }
			set { UpdateField(ref CreatedFromLearningRuleField, value, "CreatedFromLearningRule"); }
		}

		private DateTime? ValidUntilDateField;
		[DataMember(IsRequired = false)]
		public DateTime? ValidUntilDate
		{
			get { return ValidUntilDateField; }
			set { UpdateField(ref ValidUntilDateField, value, "ValidUntilDate"); }
		}

		public IEnumerable<KeyValuePair<CaptureExtensionKey, string>> ExtensionRules
		{
			get
			{
				if (ExtensionRulesByIdByKey == null) return null;
				return GetExtensionRulesNoNull();
			}
			set
			{
				if (value == null)
				{
					ExtensionRulesByIdByKey = null;
					return;
				}
				var result = new Dictionary<string, Dictionary<string, string>>();
				foreach (var kvpExtRule in value)
				{
					Dictionary<string, string> currVal;
					if (!result.TryGetValue(kvpExtRule.Key.Id, out currVal))
					{
						currVal = new Dictionary<string, string>();
						result.Add(kvpExtRule.Key.Id, currVal);
					}
					currVal[kvpExtRule.Key.Key] = kvpExtRule.Value;
				}
				ExtensionRulesByIdByKey = result;
			}
		}

		private IEnumerable<KeyValuePair<CaptureExtensionKey, string>> GetExtensionRulesNoNull()
		{
			foreach (var extensionRulesByKey in ExtensionRulesByIdByKey)
			{
				if (extensionRulesByKey.Value == null) continue;
				foreach (var keyValueRule in extensionRulesByKey.Value)
				{
					yield return new KeyValuePair<CaptureExtensionKey, string>(new CaptureExtensionKey(extensionRulesByKey.Key, keyValueRule.Key), keyValueRule.Value);
				}
			}
		}

		protected void UpdateField<T>(ref T field, T value, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return;
			field = value;
			RaisePropertyChanged(propertyName);
		}

		public WorkDetectorRule()
		{
			CreateDate = DateTime.Now;
		}

		public override string ToString()
		{
			return "WorkDetectorRule "
				+ RuleType
				+ (IsEnabled ? "" : " DISABLED")
				+ " rid:" + RelatedId
				+ " n:" + Name
				+ " title:" + TitleRule
				+ " proc:" + ProcessRule
				+ " url:" + UrlRule
				+ " form: " + (FormattedNamedGroups == null ? "NULL" : "(" + string.Join(",", FormattedNamedGroups.Select(x => x.Key + ":" + x.Value).ToArray()) + ")")
				+ GetExtensionRulesToString()
				+ (WindowScope == WindowScopeType.Active ? "" : "(" + WindowScope + ")")
				+ (ValidUntilDate == null ? "" : " VD:" + ValidUntilDate)
				+ (IsPermanent ? " Permanent" : "")
				+ (IsRegex ? " Regex" : "")
				+ (IgnoreCase ? "" : " CaseSensitive")
				+ ((AdditionalActions == null || AdditionalActions.Count == 0) ? "" : " A:" + string.Join(",", AdditionalActions.ToArray()))
				+ (WorkSelector == null ? "" : " Stor:" + WorkSelector)
				+ (string.IsNullOrEmpty(KeySuffix) ? "" : " suf:" + KeySuffix)
				+ (IsEnabledInNonWorkStatus ? " NonWorkEnabled" : "")
				+ (ServerId == 0 ? "" : " sid:" + ServerId)
				+ (CreatedFromLearningRule ? " L" : "")
				;
		}

		public string ToDetailedString()
		{
			return ToString() + GetExtensionParametersRulesToString();
		}

		private string GetExtensionRulesToString()
		{
			if (ExtensionRulesByIdByKey == null || ExtensionRulesByIdByKey.Count == 0) return "";
			var sb = new StringBuilder();
			sb.Append(" ex:");
			foreach (var extensionRulesByKey in ExtensionRulesByIdByKey)
			{
				if (extensionRulesByKey.Value == null) continue;
				sb.Append(" ").Append(extensionRulesByKey.Key).Append(" (");
				var first = true;
				foreach (var keyValueRule in extensionRulesByKey.Value)
				{
					if (!first) sb.Append(", ");
					first = false;
					sb.Append(keyValueRule.Key).Append(":").Append(keyValueRule.Value);
				}
				sb.Append(")");
			}
			return sb.ToString();
		}

		private string GetExtensionParametersRulesToString()
		{
			if (ExtensionRuleParametersById == null || ExtensionRuleParametersById.Count == 0) return "";
			var sb = new StringBuilder();
			sb.Append(" exPar:");
			foreach (var extensionRuleParamsByKey in ExtensionRuleParametersById)
			{
				if (extensionRuleParamsByKey.Value == null) continue;
				sb.Append(" ").Append(extensionRuleParamsByKey.Key).Append(" {");
				var first = true;
				foreach (var ruleParam in extensionRuleParamsByKey.Value)
				{
					if (!first) sb.Append(", ");
					first = false;
					sb.Append(ruleParam.Name).Append(":").Append(ruleParam.Value);
				}
				sb.Append("}");
			}
			return sb.ToString();
		}

        private static readonly DataContractJsonSerializerSettings JsonSerializerSettings = new()
        {
            MaxItemsInObjectGraph = int.MaxValue,
            UseSimpleDictionaryFormat = true,
            IgnoreExtensionDataObject = false
        };

		public string ToSerializedString()
		{
            var ser = new DataContractJsonSerializer(this.GetType(), JsonSerializerSettings); //extension data is lost, but that is ok atm.

			using (var stream = new MemoryStream())
			{
				ser.WriteObject(stream, this);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		public static WorkDetectorRule FromSerializedString(string ruleStr)
		{
			var bytes = Encoding.UTF8.GetBytes(ruleStr);
			var ser = new DataContractJsonSerializer(typeof(WorkDetectorRule), JsonSerializerSettings);
			using (var stream = new MemoryStream(bytes, false))
			{
				return (WorkDetectorRule)ser.ReadObject(stream);
			}
		}

		public RuleMatcherFormatter<IWorkChangingRule>[] ValidateAndGetMatchers(ClientMenuLookup menuLookup)
		{
			var changingRules = WorkChangingRuleFactory.CreateFrom(this, menuLookup)
				.Select(n => new RuleMatcherFormatter<IWorkChangingRule>(n))
				.ToArray();
			if (this.RuleType == WorkDetectorRuleType.TempStartOrAssignWork)
			{
				if (changingRules.Length > 1) throw new ArgumentOutOfRangeException();
				foreach (var changingRule in changingRules) //there should be at most one rule atm. (no rule if disabled)
				{
					if (!changingRule.GetGroupNames().Contains(GroupNameWorkKey) && !changingRule.GetFormatterKeys().Contains(GroupNameWorkKey))
					{
						throw new Exception(string.Format("{0} missing", GroupNameWorkKey)); //since we cannot edit this on the gui we don't have to localize it
					}
				}
			}
			else if (this.RuleType == WorkDetectorRuleType.TempStartOrAssignProject)
			{
				if (changingRules.Length > 1) throw new ArgumentOutOfRangeException();
				foreach (var changingRule in changingRules) //there should be at most one rule atm. (no rule if disabled)
				{
					if (!changingRule.GetGroupNames().Contains(GroupNameProjectKey) && !changingRule.GetFormatterKeys().Contains(GroupNameProjectKey))
					{
						throw new Exception(string.Format("{0} missing", GroupNameProjectKey)); //since we cannot edit this on the gui we don't have to localize it
					}
				}
			}
			else if (this.RuleType == WorkDetectorRuleType.TempStartOrAssignProjectAndWork)
			{
				if (changingRules.Length > 1) throw new ArgumentOutOfRangeException();
				foreach (var changingRule in changingRules) //there should be at most one rule atm. (no rule if disabled)
				{
					if (!changingRule.GetGroupNames().Contains(GroupNameWorkKey) && !changingRule.GetFormatterKeys().Contains(GroupNameWorkKey))
					{
						throw new Exception(string.Format("{0} missing", GroupNameWorkKey)); //since we cannot edit this on the gui we don't have to localize it
					}
				}
			}
			return changingRules;
		}

		public WorkDetectorRule Clone()
		{
			var clone = WorkDetectorRule.FromSerializedString(this.ToSerializedString());
			clone.CreateDateField = this.CreateDateField;
			clone.UpdateDateField = this.UpdateDateField;
			clone.CreatedFromLearningRuleField = this.CreatedFromLearningRuleField;
			clone.ValidUntilDateField = this.ValidUntilDateField;		
			return clone;
		}

		public bool Equals(WorkDetectorRule other)
		{
			if (other == null) return false;
			return CreateDate == other.CreateDate
				   && UpdateDate == other.UpdateDate
				   && CreatedFromLearningRule == other.CreatedFromLearningRule
				   && ValidUntilDate == other.ValidUntilDate
				   && ToSerializedString() == other.ToSerializedString();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as WorkDetectorRule);
		}

		public override int GetHashCode()
		{
			var result = 17;
			result = 31 * result + (CreateDate == null ? 0 : CreateDate.GetHashCode());
			result = 31 * result + (UpdateDate == null ? 0 : UpdateDate.GetHashCode());
			result = 31 * result + CreatedFromLearningRule.GetHashCode();
			result = 31 * result + (ValidUntilDate == null ? 0 : ValidUntilDate.GetHashCode());
			result = 31 * result + ToSerializedString().GetHashCode();
			return result;
		}
	}
}
