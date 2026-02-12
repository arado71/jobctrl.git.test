using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Model.Email
{
	public class EmailUser : IEmailUser, IEqualityComparer<EmailUser>
	{
		public int? Id { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public override string ToString()
		{
			return LastName == null
				? FirstName == null
					? (Email ?? "Unknown (" + Id + ")")
					: FirstName
				: FirstName == null
					? LastName
					: LastName + " " + FirstName;
		}

		public bool Equals(EmailUser x, EmailUser y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
			return x.Email == y.Email && x.Id == y.Id;
		}

		public int GetHashCode(EmailUser obj)
		{
			var hashCode = 13;
			hashCode = 7*hashCode + obj.Id.GetHashCode();
			hashCode = 7*hashCode + obj.Email.GetHashCode();
			return hashCode;
		}
	}
}
