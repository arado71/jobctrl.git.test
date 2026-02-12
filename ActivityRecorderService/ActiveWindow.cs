using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	[global::System.Runtime.Serialization.DataContractAttribute()]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public partial class ActiveWindow
	{
		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 1)]
		public System.DateTime CreateDate { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 2)]
		public string ProcessName { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 3)]
		public string Title { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 4)]
		public string Url { get; set; }
	}
}
