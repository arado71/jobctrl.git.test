using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Model.Mobile
{
	public interface IMobileDbContext
	{
		List<MobileUser> GetUsersFromPhoneBooks();
		List<MobileUser> GetUsersFromIvr();
		List<MobileUser> GetUsers();
		List<MobilePhoneCall> GetMobilePhoneCalls(int[] userIds, DateTime startDate, DateTime endDate);
	}
}
