using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.DeployService
{
	public class SmtpConfiguration
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public bool Ssl { get; set; }
		public string Address { get; set; }
		public string User { get; set; }
		public string Password { get; set; }
	}
}
