using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginKeszJD : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "Kesz.JD";
		private const string KeyCompany = "Company";
		private const string KeyBatch = "Batch";
		private const string KeyDocType = "DocType";

        private readonly CachedDictionary<IntPtr, AutomationElement[]> elementCache = new CachedDictionary<IntPtr, AutomationElement[]>(TimeSpan.FromSeconds(60), true);

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
			yield return KeyBatch;
			yield return KeyDocType;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (string.Equals("oexplore.exe", processName, StringComparison.OrdinalIgnoreCase))
			{
				return GetOexplore(hWnd);
			}

			return null; //wrong process name
		}

		private Dictionary<string, string> GetOexplore(IntPtr hWnd) //Therefore DMS
		{
			//starndard mdi client - PluginMdiClient / JobCTRL.MDI
			var mdiClient = EnumChildWindowsHelper.GetFirstChildWindowInfo(hWnd, child => child.ClassName.IndexOf("MDIClient", StringComparison.OrdinalIgnoreCase) > -1);
			if (mdiClient == null) { log.Verbose("Cannot find mdiClient"); return null; }
			var topChild = WinApi.GetWindow(mdiClient.Handle, WinApi.GetWindowCmd.GW_CHILD); //I'm not sure how to get the active window so use top window atm.
			if (topChild == IntPtr.Zero) { log.Verbose("Cannot find topChild"); return null; }
			var title = WindowTextHelper.GetWindowText(topChild);
		    AutomationElement[] elements;
		    if (elementCache.TryGetValue(topChild, out elements))
            {
                try
                {
                    return new Dictionary<string, string>(3)
                    {
                        { KeyCompany, AutomationHelper.GetName(elements[0], true) },
                        { KeyBatch, AutomationHelper.GetName(elements[1], true) },
                        { KeyDocType, AutomationHelper.GetName(elements[2], true) },
                    };
                }
                catch (ElementNotAvailableException)
                {
                    elementCache.Remove(topChild);
                }
            }
		    AutomationElement element;
		    try
		    {
		        element = AutomationElement.FromHandle(topChild);
            }
            catch (ElementNotAvailableException) { log.Verbose("Cannot get AutomationElement from topChild"); return null; }
		    if (element == null) { log.Verbose("Cannot get AutomationElement from topChild"); return null; }

            AutomationElement comp = null, batch = null, docType = null;

			if (title.IndexOf("Vegyes feladás ÁFÁ-val", StringComparison.OrdinalIgnoreCase) > -1)
			{
				var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);

				comp = children.Count < 3 ? null : children[2];
				batch = children.Count < 17 ? null : children[16];
				docType = children.Count < 1 ? null : children[0];
			}
			else if (title.IndexOf("Számla felvitel - fizetési információ", StringComparison.OrdinalIgnoreCase) > -1)
			{
				var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);

				var mainTitle = WindowTextHelper.GetWindowText(hWnd);
				if (mainTitle.IndexOf("Szállítói számlák felvitele", StringComparison.OrdinalIgnoreCase) > -1)
				{
					comp = children.Count < 1 ? null : children[0];
					batch = children.Count < 23 ? null : children[20];
					docType = children.Count < 15 ? null : children[14];
				}
				else
				{
					comp = children.Count < 18 ? null : children[17]; //0 ??
					batch = children.Count < 23 ? null : children[22]; //20 ??
					docType = children.Count < 17 ? null : children[16]; //14 ??
				}
			}
			else if (title.IndexOf("Számla igazolás", StringComparison.OrdinalIgnoreCase) > -1)
			{
				var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);

				comp = children.Count < 3 ? null : children[2];
				batch = children.Count < 18 ? null : children[17];
				docType = children.Count < 2 ? null : children[1];
			}
			else if (title.IndexOf("Standard számlabevitel", StringComparison.OrdinalIgnoreCase) > -1)
			{
				var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);

				//var comp = children.Count < 2 ? null : children[1];
				comp = children.Count < 19 ? null : children[18]; //we have two...
				batch = children.Count < 12 ? null : children[11];
				docType = children.Count < 18 ? null : children[17];
			}
			else if (title.IndexOf("Befizetés rögzítés", StringComparison.OrdinalIgnoreCase) > -1)
			{
				var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);

				comp = children.Count < 1 ? null : children[0];
				batch = children.Count < 13 ? null : children[12];
				docType = null; //N/A
			}

		    elementCache.Add(topChild, new[] { comp, batch, docType });

			return new Dictionary<string, string>(3)
			{
				{KeyCompany, AutomationHelper.GetName(comp)},
				{KeyBatch, AutomationHelper.GetName(batch)},
				{KeyDocType, AutomationHelper.GetName(docType)},
			};
		}
	}
}
