using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.TODOs
{
	[DataContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class TodoListToken
	{
		[DataMember]
		public bool IsAcquired { get; set; }

		[DataMember]
		public string EditedByLastName { get; set; }

		[DataMember]
		public string EditedByFirstName { get; set; }

		public TodoListToken()
		{
			IsAcquired = false;
			EditedByFirstName = "";
			EditedByLastName = "";
		}

		public TodoListToken(bool isAcquired, string editedByLastName = "", string editedByFirstName = "")
		{
			IsAcquired = isAcquired;
			EditedByLastName = editedByLastName;
			EditedByFirstName = editedByFirstName;
		}
	}
}
