using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "WorkDetectorRule", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class WorkDetectorRule
	{
		[DataMember(Name = "RuleType", Order = 0, IsRequired = true, EmitDefaultValue = true)]
		public WorkDetectorRuleType RuleType { get; set; }
		[DataMember(Name = "RelatedId", Order = 1, IsRequired = true, EmitDefaultValue = true)]
		public int RelatedId { get; set; }

		//could be put into a base class but that would break serialization for WorkDetectorRule
		[DataMember(Name = "Name", Order = 2, IsRequired = true, EmitDefaultValue = true)]
		public string Name { get; set; }
		[DataMember(Name = "IsEnabled", Order = 3, IsRequired = false, EmitDefaultValue = false)]
		public bool IsEnabled { get; set; }
		[DataMember(Name = "IsRegex", Order = 4, IsRequired = false, EmitDefaultValue = false)]
		public bool IsRegex { get; set; }
		[DataMember(Name = "IgnoreCase", Order = 5, IsRequired = false, EmitDefaultValue = false)]
		public bool IgnoreCase { get; set; }
		[DataMember(Name = "TitleRule", Order = 6, IsRequired = true, EmitDefaultValue = true)]
		public string TitleRule { get; set; }
		[DataMember(Name = "ProcessRule", Order = 7, IsRequired = true, EmitDefaultValue = true)]
		public string ProcessRule { get; set; }
		[DataMember(Name = "UrlRule", Order = 8, IsRequired = false, EmitDefaultValue = false)]
		public string UrlRule { get; set; }

		[DataMember(Name = "IsPermanent", Order = 22, IsRequired = false, EmitDefaultValue = false)]
		public bool IsPermanent { get; set; }
		[DataMember(Name = "WorkSelector", Order = 23, IsRequired = false, EmitDefaultValue = false)]
		public WorkSelector WorkSelector { get; set; }
		[DataMember(Name = "KeySuffix", Order = 24, IsRequired = false, EmitDefaultValue = false)]
		public string KeySuffix { get; set; }
		[DataMember(Name = "ServerId", Order = 25, IsRequired = false, EmitDefaultValue = false)]
		public int ServerId { get; set; }

		[DataMember(Name = "ExtensionRulesByIdByKey", Order = 26, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, Dictionary<string, string>> ExtensionRulesByIdByKey { get; set; }

		[DataMember(Name = "WindowScope", Order = 27, IsRequired = false, EmitDefaultValue = false)]
		public WindowScopeType WindowScope { get; set; }

		[DataMember(Name = "IsEnabledInNonWorkStatus", Order = 28, IsRequired = false, EmitDefaultValue = false)]
		public bool IsEnabledInNonWorkStatus { get; set; }
		[DataMember(Name = "IsEnabledInProjectIds", Order = 29, IsRequired = false, EmitDefaultValue = false)]
		public List<int> IsEnabledInProjectIds { get; set; }

		[DataMember(Name = "ExtensionRuleParametersById", Order = 30, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, List<ExtensionRuleParameter>> ExtensionRuleParametersById { get; set; }

		[DataMember(Name = "AdditionalActions", Order = 31, IsRequired = false, EmitDefaultValue = false)]
		public List<string> AdditionalActions { get; set; }

		[DataMember(Name = "FormattedNamedGroups", Order = 32, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, string> FormattedNamedGroups { get; set; }

		[DataMember(Name = "Children", Order = 33, IsRequired = false, EmitDefaultValue = false)]
		public List<WindowRule> Children { get; set; }

		[DataMember(Name = "IsDefault", Order = 36, IsRequired = false, EmitDefaultValue = false)]
		public bool IsDefault { get; set; }
	}

	[DataContract(Name = "WindowRule", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class WindowRule
	{
		[DataMember(Name = "Name", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public string Name { get; set; }
		[DataMember(Name = "IsEnabled", Order = 3, IsRequired = false, EmitDefaultValue = false)]
		public bool IsEnabled { get; set; }
		[DataMember(Name = "IsRegex", Order = 4, IsRequired = false, EmitDefaultValue = false)]
		public bool IsRegex { get; set; }
		[DataMember(Name = "IgnoreCase", Order = 5, IsRequired = false, EmitDefaultValue = false)]
		public bool IgnoreCase { get; set; }
		[DataMember(Name = "TitleRule", Order = 6, IsRequired = true, EmitDefaultValue = true)]
		public string TitleRule { get; set; }
		[DataMember(Name = "ProcessRule", Order = 7, IsRequired = true, EmitDefaultValue = true)]
		public string ProcessRule { get; set; }
		[DataMember(Name = "UrlRule", Order = 8, IsRequired = false, EmitDefaultValue = false)]
		public string UrlRule { get; set; }
		[DataMember(Name = "ExtensionRulesByIdByKey", Order = 26, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, Dictionary<string, string>> ExtensionRulesByIdByKey { get; set; }

		[DataMember(Name = "WindowScope", Order = 27, IsRequired = false, EmitDefaultValue = false)]
		public WindowScopeType WindowScope { get; set; }

		//[DataMember(Name = "ExtensionRuleParametersById", Order = 30, IsRequired = false, EmitDefaultValue = false)]
		//public Dictionary<string, List<ExtensionRuleParameter>> ExtensionRuleParametersById { get; set; }
		//[DataMember(Name = "FormattedNamedGroups", Order = 32, IsRequired = false, EmitDefaultValue = false)]
		//public Dictionary<string, string> FormattedNamedGroups { get; set; }
		//[DataMember(Name = "Children", Order = 33, IsRequired = false, EmitDefaultValue = false)]
		//public List<WindowRule> Children { get; set; }
	}

	[DataContract(Name = "ExtensionRuleParameter", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ExtensionRuleParameter
	{
		[DataMember(Name = "Name", Order = 1, IsRequired = true, EmitDefaultValue = false)]
		public string Name { get; set; }
		[DataMember(Name = "Value", Order = 2, IsRequired = true, EmitDefaultValue = false)]
		public string Value { get; set; }
	}

	[DataContract(Name = "WorkSelector", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class WorkSelector
	{
		[DataMember(Name = "Name", Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public string Name { get; set; }
		//[DataMember(Name = "IsEnabled", Order = 3, IsRequired = false, EmitDefaultValue = false)]
		//public bool IsEnabled { get; set; }
		[DataMember(Name = "IsRegex", Order = 4, IsRequired = false, EmitDefaultValue = false)]
		public bool IsRegex { get; set; }
		[DataMember(Name = "IgnoreCase", Order = 5, IsRequired = false, EmitDefaultValue = false)]
		public bool IgnoreCase { get; set; }
		[DataMember(Name = "Rule", Order = 6, IsRequired = true, EmitDefaultValue = true)]
		public string Rule { get; set; }
		[DataMember(Name = "TemplateText", Order = 7, IsRequired = true, EmitDefaultValue = true)]
		public string TemplateText { get; set; }
	}

	[DataContract(Name = "WorkDetectorRuleType", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum WorkDetectorRuleType
	{
		[EnumMember]
		TempStartWork = 0,
		[EnumMember]
		TempStopWork,
		[EnumMember]
		TempStartCategory,
		[EnumMember]
		DoNothing,
		[EnumMember]
		TempStartProjectTemplate,
		[EnumMember]
		TempStartWorkTemplate,
		[EnumMember]
		EndTempEffect,
		[EnumMember]
		CreateNewRuleAndEndTempEffect,
		[EnumMember]
		CreateNewRuleAndTempStartWork,
		[EnumMember]
		TempStartOrAssignWork,
		[EnumMember]
		TempStartOrAssignProject,
		[EnumMember]
		TempStartOrAssignProjectAndWork,
	}

	[DataContract(Name = "WindowScopeType", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum WindowScopeType
	{
		[EnumMember]
		Active = 0,
		[EnumMember]
		VisibleOrActive,
		[EnumMember]
		Any,
	}
}
