using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Model.Mobile
{
	public class MobileUser
	{
		public int? UserId { get; set; }
		public string PhoneNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public MobileUser Clone()
		{
			return new MobileUser() { UserId = UserId, PhoneNumber = PhoneNumber, FirstName = FirstName, LastName = LastName };
		}

		public override string ToString()
		{
			return LastName == null
				? FirstName == null
					? (PhoneNumber ?? "Unknown (" + UserId + ")")
					: FirstName
				: FirstName == null
					? LastName
					: LastName + " " + FirstName;
		}
	}
}
