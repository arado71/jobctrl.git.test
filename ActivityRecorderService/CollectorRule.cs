using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "CollectorRule", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CollectorRule
	{
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

		[DataMember(Name = "ServerId", Order = 25, IsRequired = false, EmitDefaultValue = false)]
		public int ServerId { get; set; }

		[DataMember(Name = "ExtensionRulesByIdByKey", Order = 26, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, Dictionary<string, string>> ExtensionRulesByIdByKey { get; set; }

		[DataMember(Name = "WindowScope", Order = 27, IsRequired = false, EmitDefaultValue = false)]
		public WindowScopeType WindowScope { get; set; }

		[DataMember(Name = "FormattedNamedGroups", Order = 32, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, string> FormattedNamedGroups { get; set; }

		[DataMember(Name = "Children", Order = 33, IsRequired = false, EmitDefaultValue = false)]
		public List<WindowRule> Children { get; set; }

		[DataMember(Name = "CapturedKeys", Order = 34, IsRequired = true, EmitDefaultValue = false)]
		public List<string> CapturedKeys { get; set; } //name of the capturing groups

		[DataMember(Name = "ExtensionRuleParametersById", Order = 35, IsRequired = false, EmitDefaultValue = false)]
		public Dictionary<string, List<ExtensionRuleParameter>> ExtensionRuleParametersById { get; set; }
	}

	[DataContract(Name = "CollectorRules", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CollectorRules
	{
		[DataMember(Name = "Rules", Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public List<CollectorRule> Rules { get; set; }
	}
}
