using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract(Name = "CannedCloseReasons", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CannedCloseReasons
	{
		[DataMember]
		public List<string> DefaultReasons { get; set; }

		[DataMember(Order = 1)]
		public bool IsReadonly { get; set; }

		[DataMember(Order = 2)]
		public List<CloseReasonNode> TreeRoot { get; set; }

		//todo nicer interface?
		//[DataMember(Order = 2)]
		//public List<CannedReason> CloseReasons { get; set; }
	}

	[DataContract(Name = "CloseReasonNode", Namespace = "http://jobctrl.com/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class CloseReasonNode
	{
		[DataMember]
		public int NodeId { get; set; }

		[DataMember]
		public string ReasonPart { get; set; }

		[DataMember]
		public List<CloseReasonNode> Children { get; set; }
	}

	//[DataContract(Name = "CannedReason", Namespace = "http://jobctrl.com/")]
	//public class CannedReason
	//{
	//    [DataMember]
	//    public string Title { get; set; }
	//    [DataMember]
	//    public string Reason { get; set; }
	//}
}
