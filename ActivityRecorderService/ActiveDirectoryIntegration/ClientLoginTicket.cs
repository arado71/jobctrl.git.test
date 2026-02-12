using System;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderService.ActiveDirectoryIntegration
{
	[DataContract(Name = "ClientLoginTicket", Namespace = "http://jobctrl.com/Authentication")]
	public class ClientLoginTicket
	{
		[DataMember]
		public int UserId { get; set; }

		[DataMember]
		public string Ticket { get; set; }

		[DataMember]
		public DateTime ExpirationDate { get; set; }
	}
}