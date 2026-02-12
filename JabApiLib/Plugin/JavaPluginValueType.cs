using System.Runtime.Serialization;

namespace Tct.Java.Plugin
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	[DataContract(Namespace = "http://jobctrl.com/java", Name = "JavaPluginValueType")]
	public enum JavaPluginValueType
	{
		[EnumMember]
		Name,
		[EnumMember]
		Role,
		[EnumMember]
		Description,
		[EnumMember]
		Text,
		[EnumMember]
		ComboValue,
		[EnumMember]
		Table
	}
}
