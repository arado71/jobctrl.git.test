using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public class DragonUrlResolver: ChromiumUrlResolverBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public DragonUrlResolver() : base(log)
		{
		}

		public override string ProcessName => "dragon.exe";

		public override Browser Browser => Browser.Dragon;
	}
}
