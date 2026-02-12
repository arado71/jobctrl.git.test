using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.CustomReporting
{
	public interface IWebApi
	{
		Dictionary<int, string> GetUserNames(int[] userIds);
	}
}
