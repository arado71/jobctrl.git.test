using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginCef : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "JobCTRL.Cef"; //Cordilheira Escrita Fiscal
		private const string KeyText = "Text";

		public string Id
		{
			get { return PluginId; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public void SetParameter(string name, string value)
		{
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyText;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!string.Equals(processName, "cef.exe", StringComparison.OrdinalIgnoreCase)) return null;

			var element = AutomationElement.FromHandle(hWnd);
			if (element == null) return null;

			var statusTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.StatusBar);
			element = element.FindFirst(TreeScope.Children, statusTypeCond);
			if (element == null) return null;

			var children1 = element.FindAll(TreeScope.Children, Condition.TrueCondition); //AutomationId: StatusBar.Pane4
			element = children1.Count < 5 ? null : children1[4];
			if (element == null) return null;

			return new Dictionary<string, string>(1)
			{
				{KeyText, AutomationHelper.GetValue(element) }
			};
		}
	}
}
