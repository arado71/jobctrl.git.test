using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderClient
{
	static class Extensions
	{
		public static void ErrorAndFail(this ILog log, string message, Exception ex)
		{
			log.Error(message, ex);
			Debug.Fail(message + Environment.NewLine + ex);
		}

	}
}
