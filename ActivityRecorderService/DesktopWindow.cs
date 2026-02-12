using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService
{
	[global::System.Runtime.Serialization.DataContractAttribute()]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class DesktopWindow
	{
		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 1)]
		public System.DateTime CreateDate { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 2)]
		public bool IsActive { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 3)]
		public short X { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 4)]
		public short Y { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 5)]
		public short Width { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 6)]
		public short Height { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 7)]
		public int ClientArea { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 8)]
		public int VisibleClientArea { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 20)]
		public string ProcessName { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 21)]
		public string Title { get; set; }

		[global::System.Runtime.Serialization.DataMemberAttribute(Order = 22)]
		public string Url { get; set; }
	}


}
