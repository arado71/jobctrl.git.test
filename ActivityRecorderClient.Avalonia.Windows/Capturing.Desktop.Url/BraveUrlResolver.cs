using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public class BraveUrlResolver: ChromiumUrlResolverBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public BraveUrlResolver() : base(log)
		{
		}

		public override string ProcessName => "brave.exe";

		public override Browser Browser => Browser.Brave;
	}
}
