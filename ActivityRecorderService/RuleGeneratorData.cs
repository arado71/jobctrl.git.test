using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "RuleGeneratorData", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class RuleGeneratorData
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Parameters { get; set; }
	}
}
