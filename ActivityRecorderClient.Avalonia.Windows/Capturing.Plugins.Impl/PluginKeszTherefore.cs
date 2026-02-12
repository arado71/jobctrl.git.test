using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginKeszTherefore : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "Kesz.Therefore";
		private const string KeyCompany = "Company";
		private const string searchStr = "Tagvállalat kódja:";

		private readonly CachedDictionary<string, AutomationElement> elementCache = new CachedDictionary<string, AutomationElement>(TimeSpan.FromSeconds(60), true);

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
			yield return KeyCompany;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (string.Equals("TheViewer.exe", processName, StringComparison.OrdinalIgnoreCase))
			{
				return GetTheViewer(hWnd);
			}

			return null; //wrong process name
		}

		private IEnumerable<KeyValuePair<string, string>> GetTheViewer(IntPtr hWnd)
		{
			var key = WindowTextHelper.GetWindowText(hWnd) + hWnd;
			AutomationElement found;
			if (elementCache.TryGetValue(key, out found))
			{
				if (found == null)
					return null;
				try
				{
					var text = AutomationHelper.GetName(found, true);
					if (text.StartsWith(searchStr, StringComparison.OrdinalIgnoreCase))
						return new Dictionary<string, string>(1)
						{
							{ KeyCompany, text.Substring(searchStr.Length).Trim() },
						};
				}
				catch (ElementNotAvailableException)
				{
					// do nothing
				}
				elementCache.Remove(key);
			}
			var tabTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tab);

			var element = AutomationElement.FromHandle(hWnd);
			if (element == null) { log.Verbose("Cannot get AutomationElement from hWnd"); return null; }

			elementCache.Add(key, null);
			
			element = element.FindAll(TreeScope.Children, tabTypeCond).OfType<AutomationElement>()
				.Where(n => string.Equals(AutomationHelper.GetName(n), "Dokumentum részletei", StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();
			if (element == null) { log.Verbose("Cannot find doc details"); return null; }

			element = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
			if (element == null) { log.Verbose("Cannot find tree"); return null; }

			var comp = element.FindAll(TreeScope.Children, Condition.TrueCondition).OfType<AutomationElement>()
				.Select(n => new {Name = AutomationHelper.GetName(n), Element = n})
				.Where(m => m.Name.StartsWith(searchStr, StringComparison.OrdinalIgnoreCase))
				.Select(m => new { Name = m.Name.Substring(searchStr.Length).Trim(), m.Element})
				.FirstOrDefault();

			if (comp == null) { log.Verbose("Cannot find company"); return null; }

			elementCache.Set(key, comp.Element);

			return new Dictionary<string, string>(1)
			{
				{KeyCompany, comp.Name},
			};
		}
	}
}
