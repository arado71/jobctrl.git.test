using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JobCTRL.Plugins;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginMdiClient : ICaptureExtension
	{
		private const string PluginId = "JobCTRL.MDI";
		private const string ParamMdiClientClass = "MdiClientClass";
		private const string ParamProcessName = "ProcessName";
		private const string KeyTitle = "Title";

		private string mdiClientClass = "mdiclient";
		private HashSet<string> processNamesToCheck;

		public string Id
		{
			get { return PluginId; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamProcessName;
			yield return ParamMdiClientClass;
		}

		public void SetParameter(string name, string value)
		{
			if (string.Equals(name, ParamMdiClientClass, StringComparison.OrdinalIgnoreCase))
			{
				if (!string.IsNullOrEmpty(value))
				{
					mdiClientClass = value;
				}
			}
			else if (string.Equals(name, ParamProcessName, StringComparison.OrdinalIgnoreCase))
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

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyTitle;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (processNamesToCheck != null
				&& !processNamesToCheck.Contains(processName))
			{
				return null; //wrong process name
			}
			//we might want to cache this
			var mdiClient = EnumChildWindowsHelper.GetFirstChildWindowInfo(hWnd, child => child.ClassName.IndexOf(mdiClientClass, StringComparison.OrdinalIgnoreCase) > -1);
			if (mdiClient == null) return null;
			var topChild = WinApi.GetWindow(mdiClient.Handle, WinApi.GetWindowCmd.GW_CHILD); //I'm not sure how to get the active window so use top window atm.
			if (topChild == IntPtr.Zero) return null;
			var title = WindowTextHelper.GetWindowText(topChild);
			return new Dictionary<string, string>(1)
			{
				{KeyTitle, title}
			};
		}
	}
}
