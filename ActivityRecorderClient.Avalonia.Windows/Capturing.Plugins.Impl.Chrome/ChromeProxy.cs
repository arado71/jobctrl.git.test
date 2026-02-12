using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using log4net;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Common;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome
{
	public class ChromeProxy : ChromiumProxyBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ChromeProxy() : base(log)
		{
		}

		protected override string ExecScriptImpl(string request)
		{
			return ChromeCaptureClientWrapper.Execute(c => c.SendCommand(request));
		}
	}
}
