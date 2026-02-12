using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	//this class can only be used from an STA thread
	public class PluginInternetExplorer : PluginInternetExplorerBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string PluginId = "JobCTRL.IE";

		public PluginInternetExplorer()
			: base(log)
		{
			Id = PluginId;
		}

		protected override ChildWindowInfo GetExplorerServerWindow(IntPtr hWnd, int processId, string processName)
		{
			if (!string.Equals(processName, "iexplore.exe", StringComparison.OrdinalIgnoreCase)) return null;
			return InternetExplorerUrlResolver.GetAutomationWindow(hWnd);
		}
	}
}
