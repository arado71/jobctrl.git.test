using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	public struct AppVersionInfo
	{
		public string Name;
		public string Version;

		public AppVersionInfo(string name, string ver)
		{
			Name = name;
			Version = ver;
		}
	}
}
