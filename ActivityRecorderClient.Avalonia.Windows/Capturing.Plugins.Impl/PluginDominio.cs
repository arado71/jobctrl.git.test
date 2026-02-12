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
	public class PluginDominio : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "JobCTRL.Dominio";
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
			if (!string.Equals(processName, "contabil.exe", StringComparison.OrdinalIgnoreCase)) return null;

			var element = AutomationElement.FromHandle(hWnd);
			if (element == null) return null;

			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) return null;

			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) return null;

			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) return null;

			var paneTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane);
			element = element.FindFirst(TreeScope.Children, paneTypeCond);
			if (element == null) return null;

			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) return null;

			return new Dictionary<string, string>(1)
			{
				{KeyText, AutomationHelper.GetName(element) }
			};
		}
	}
}
