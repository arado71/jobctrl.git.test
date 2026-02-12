using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "CensorRule", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CensorRule
	{
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

		[DataMember(Name = "RuleType", Order = 20, IsRequired = true, EmitDefaultValue = true)]
		public CensorRuleType RuleType { get; set; }

		//not supported atm. but might be later
		//[DataMember(Name = "ExtensionRulesByIdByKey", Order = 26, IsRequired = false, EmitDefaultValue = false)]
		//public Dictionary<string, Dictionary<string, string>> ExtensionRulesByIdByKey { get; set; }
	}

	[Flags]
	[DataContract(Name = "CensorRuleType", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum CensorRuleType
	{
		[EnumMember]
		None = 0,
		[EnumMember]
		HideTitle = 1 << 0,
		[EnumMember]
		HideScreenShot = 1 << 1,
		[EnumMember]
		HideUrl = 1 << 2,
		[EnumMember]
		HideWindow = 1 << 3,
	}
}
