using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraSyncTool.Jira.Model.Jc
{
	public class User : IId, IEquatable<User>
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public bool IsFromServer { get { return Id != -1; } }
		public string ExtId { get; set; }

		public bool Equals(User other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(other, this)) return true;
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as User);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return FirstName + " " + LastName + " (" + Id + ") [" + Email + "]";
		}
	}
}
