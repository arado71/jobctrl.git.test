using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Model.Mobile;

namespace Reporter.Model.Mobile
{
	public class MobileUserPhoneBook //todo we don't handle number changes atm.
	{
		private readonly Dictionary<string, MobileUser> usersByPhoneNumber = new Dictionary<string, MobileUser>();
		private readonly Dictionary<int, MobileUser> usersById = new Dictionary<int, MobileUser>();

		public MobileUserPhoneBook(IMobileDbContext context)
		{
			var pBooks = context.GetUsersFromPhoneBooks();
			foreach (var mobileUser in pBooks)
			{
				if (mobileUser.PhoneNumber == null) continue;
				usersByPhoneNumber[mobileUser.PhoneNumber] = mobileUser;
			}

			var ivr = context.GetUsersFromIvr();
			foreach (var mobileUser in ivr)
			{
				if (mobileUser.UserId == null || mobileUser.PhoneNumber == null) continue;
				if (mobileUser.PhoneNumber.StartsWith("00")) mobileUser.PhoneNumber = "+" + mobileUser.PhoneNumber.Substring(2);
				usersByPhoneNumber[mobileUser.PhoneNumber] = mobileUser;
			}

			var users = context.GetUsers();
			foreach (var user in users)
			{
				usersById[user.UserId.Value] = user; //no phonenumber here
			}
		}

		public MobileUser GetUserForPhoneNumber(string phoneNumber)
		{
			MobileUser result;
			usersByPhoneNumber.TryGetValue(phoneNumber, out result);
			return result;
		}

		public MobileUser GetUserForId(int userId)
		{
			MobileUser result;
			usersById.TryGetValue(userId, out result);
			return result;
		}
	}
}
