using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginFocus : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "JobCTRL.Focus";
		private const string ParamProcess = "ProcessName";
		private const string KeyValue = "Value";
		private const string KeyName = "Name";
		private const string KeyId = "Id";
		private const string KeySelection = "Selection";
		private const string KeyClassName = "ClassName";
		private const string KeyAutomationId = "AutomationId";
		private const string KeyHelpText = "HelpText";
		private const string KeyText = "Text";
		private const string KeyControlType = "HelpText";

		private HashSet<string> ProcessNamesToCheck { get; set; }

		public string Id { get { return PluginId; } }
		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamProcess;
		}

		public void SetParameter(string name, string value)
		{
			if (string.Equals(name, ParamProcess, StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrEmpty(value))
				{
					ProcessNamesToCheck = null;
				}
				else
				{
					ProcessNamesToCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					foreach (var processName in value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
					{
						ProcessNamesToCheck.Add(processName);
					}
				}
			}
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyValue;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			string name = "", value = "", className = "", selection = "", 
				automationId = "", controlType = "", helpText = "", runtimeId = "", text = "";
			//var sw = Stopwatch.StartNew();
			if (ProcessNamesToCheck != null)
			{
				if (!ProcessNamesToCheck.Contains(processName)) yield break;
			}
			try
			{
				if (processId == WinApi.GetCurrentProcessId()) yield break;
				var focusedElement = AutomationElement.FocusedElement;
				if (focusedElement == null) yield break;
				if (focusedElement.Current.ProcessId != processId) yield break;
				Debug.Assert(focusedElement.Current.ProcessId != WinApi.GetCurrentProcessId());


				if (!focusedElement.Current.IsPassword)
				{
					value = AutomationHelper.GetValue(focusedElement).Ellipse(40);
					text = AutomationHelper.GetText(focusedElement).Ellipse(40);
					selection = AutomationHelper.GetSelection(focusedElement).Ellipse(40);
				}

				name = focusedElement.Current.Name.Ellipse(40);
				className = focusedElement.Current.ClassName.Ellipse(40);
				automationId = focusedElement.Current.AutomationId.Ellipse(40);
				controlType = AutomationHelper.GetControlType(focusedElement).Ellipse(40);
				helpText = AutomationHelper.GetHelpText(focusedElement).Ellipse(40);
				runtimeId = string.Join(" ",
					focusedElement.GetRuntimeId().Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray()).Ellipse(40);
			}
			catch(Exception ex)
			{
				log.Verbose("Unexpected exception in Capture", ex);
			}

			yield return new KeyValuePair<string, string>(KeyValue, value);
			yield return new KeyValuePair<string, string>(KeyName, name);
			yield return new KeyValuePair<string, string>(KeyClassName, className);
			yield return new KeyValuePair<string, string>(KeySelection, selection);
			yield return new KeyValuePair<string, string>(KeyAutomationId, automationId);
			yield return new KeyValuePair<string, string>(KeyControlType, controlType);
			yield return new KeyValuePair<string, string>(KeyHelpText, helpText);
			yield return new KeyValuePair<string, string>(KeyText, text);
			yield return new KeyValuePair<string, string>(KeyId, runtimeId);
			//log.VerboseFormat("Extracted v: {0} n: {1} c: {2} s: {3} aid: {4} ct: {5} h: {6} t: {7} id: {8} in {9} ms",
			//	value, name, className, selection, automationId, controlType, helpText, text, runtimeId, sw.Elapsed.TotalMilliseconds);
		}
	}
}
