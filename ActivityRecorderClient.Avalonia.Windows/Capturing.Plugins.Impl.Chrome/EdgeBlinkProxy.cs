using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome
{
	public class EdgeBlinkProxy : ChromiumProxyBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public EdgeBlinkProxy() : base(log)
		{
		}

		protected override string ExecScriptImpl(string request)
		{
			return EdgeBlinkCaptureClientWrapper.Execute(c => c.SendCommand(request));
		}
	}
}
