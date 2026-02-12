using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Processing
{
	public class UserLookup
	{
		private readonly Dictionary<int, string> userLookup;

		public UserLookup(Dictionary<int, string> userLookup)
		{
			this.userLookup = userLookup;
		}

		public string GetName(int userId)
		{
			string userName;
			if (!userLookup.TryGetValue(userId, out userName))
			{
				return userId.ToString();
			}

			return userName;
		}
	}
}
