using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Model.Email
{
	public interface IEmailUser
	{
		int? Id { get; set; }
		string Email { get; set; }
		string FirstName { get; set; }
		string LastName { get; set; }
	}
}
