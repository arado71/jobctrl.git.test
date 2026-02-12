using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ProjectSync
{
	[Serializable]
	public class ProjectCredentials
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}
}
