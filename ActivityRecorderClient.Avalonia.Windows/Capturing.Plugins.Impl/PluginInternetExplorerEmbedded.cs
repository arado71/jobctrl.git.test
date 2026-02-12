using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	//this class can only be used from an STA thread
	public class PluginInternetExplorerEmbedded : PluginInternetExplorerBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string PluginId = "JobCTRL.IE.Embedded";
		private const string ParamProcessName = "ProcessName";

		private HashSet<string> processNamesToCheck;

		public PluginInternetExplorerEmbedded()
			: base(log)
		{
			Id = PluginId;
		}

		public override IEnumerable<string> GetParameterNames()
		{
			return base.GetParameterNames().Concat(new[] { ParamProcessName });
		}

		public override void SetParameter(string name, string value)
		{
			base.SetParameter(name, value);
			if (string.Equals(name, ParamProcessName, StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrEmpty(value))
				{
					processNamesToCheck = null;
				}
				else
				{
					processNamesToCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					foreach (var file in value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
					{
						processNamesToCheck.Add(file);
					}
				}
			}
		}

		protected override ChildWindowInfo GetExplorerServerWindow(IntPtr hWnd, int processId, string processName)
		{
			if (processNamesToCheck != null
				&& !processNamesToCheck.Contains(processName))
			{
				return null; //wrong process name
			}
			return EnumChildWindowsHelper.GetFirstChildWindowInfo(hWnd, n => n.ClassName == "Internet Explorer_Server");
		}
	}
}
