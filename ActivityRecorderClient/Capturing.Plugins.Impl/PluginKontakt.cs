using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginKontakt : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "JobCTRL.Kontakt";
		private const string KeyRoleName = "RoleName";
		private const string KeyIvk = "Ivk";
		private const string KeyOthers = "Others";
		private const string ParamProcess = "ProcessName";

		private Regex processRegex = new Regex("KONTAKT2017[.]Client[.]exe", RegexOptions.IgnoreCase);

		public string Id
		{
			get { return PluginId; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamProcess;
		}

		public void SetParameter(string name, string value)
		{
			if (string.Equals(name, ParamProcess, StringComparison.OrdinalIgnoreCase))
			{
				processRegex = new Regex(value, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyRoleName;
			yield return KeyIvk;
			yield return KeyOthers;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!processRegex.IsMatch(processName)) return null;

			var mdiClient = EnumChildWindowsHelper.GetFirstChildWindowInfo(hWnd, child => child.ClassName.IndexOf("MDIClient", StringComparison.OrdinalIgnoreCase) > -1);
			if (mdiClient == null) { log.Verbose("Cannot find mdiClient"); return null; }
			var topChild = WinApi.GetWindow(mdiClient.Handle, WinApi.GetWindowCmd.GW_CHILD); //I'm not sure how to get the active window so use top window atm.
			if (topChild == IntPtr.Zero) { log.Verbose("Cannot find topChild"); return null; }
			var element = AutomationElement.FromHandle(topChild);
			if (element == null) { log.Verbose("topChild is null"); return null; }
			var roleName = WindowTextHelper.GetWindowText(topChild);
			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) { log.Verbose("splitContainerControl1 not found"); return null; }
			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) { log.Verbose("panel1 not found"); return null; }
			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) { log.Verbose("tab not found"); return null; }
			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) { log.Verbose("folder pane not found"); return null; }
			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) { log.Verbose("noname pane not found"); return null; }
			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) { log.Verbose("noname pane2 not found"); return null; }
			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) { log.Verbose("xtraScrollableControl not found"); return null; }
			element = element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "headerPanelControl"));
			if (element == null) { log.Verbose("headerPanelControl not found"); return null; }
			var controls = element.FindAll(TreeScope.Children,
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
			if (controls == null || controls.Count == 0) { log.Verbose("text controls not found"); return null; }
			var first = AutomationHelper.GetName(controls[0]);
			if (first.StartsWith("IVK: ")) first = first.Substring(5);
			String second = null;
			if (controls.Count > 1)
				second = AutomationHelper.GetName(controls[1]);
			return new Dictionary<string, string>
			{
				{KeyRoleName, roleName},
				{KeyIvk, first},
				{KeyOthers, second}
			};
		}


	}
}
