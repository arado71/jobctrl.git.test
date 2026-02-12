using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Communication;

namespace Reporter.CustomReporting
{
	public class WebApi : IWebApi
	{
		public Dictionary<int, string> GetUserNames(int[] userIds)
		{
			return CommunicationHelper.GetUserNames(userIds);
		}
	}
}
