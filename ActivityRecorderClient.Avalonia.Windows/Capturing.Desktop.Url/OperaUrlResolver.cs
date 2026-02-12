using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public class OperaUrlResolver: ChromiumUrlResolverBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public OperaUrlResolver() : base(log)
		{
		}

		public override string ProcessName => "opera.exe";

		public override Browser Browser => Browser.Opera;
	}
}
