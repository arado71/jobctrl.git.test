using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Firefox
{
	public class FirefoxException : Exception
	{
		public FirefoxException(string message) : base(message)
		{
		}
	}
}
