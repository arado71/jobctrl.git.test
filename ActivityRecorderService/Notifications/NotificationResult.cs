using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService.Notifications
{
	[DataContract(Name = "NotificationResult", Namespace = "http://jobctrl.com/Notifications")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class NotificationResult
	{
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public int UserId { get; set; }

		[DataMember]
		public DateTime ShowDate { get; set; }

		[DataMember]
		public DateTime ConfirmDate { get; set; }

		[DataMember]
		public string Result { get; set; }

		public override string ToString()
		{
			return "uid: " + UserId + " id: " + Id + " conf: " + ConfirmDate + " res: " + Result;
		}
	}
}
