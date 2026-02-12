using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tct.ActivityRecorderService
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class AuthData
	{
		[DataMember]
		public int Id { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember(Order = 1)]
		public string FirstName { get; set; }
		[DataMember(Order = 2)]
		public string LastName { get; set; }
		[DataMember]
		public string Email { get; set; }
		[DataMember]
		public UserAccessLevel AccessLevel { get; set; }
		[DataMember]
		public string TimeZoneData { get; set; }
	}
}
