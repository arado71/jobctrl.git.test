using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using JCAutomation.SystemAdapter;

namespace JCAutomation.Extraction
{
	[DataContract]
	public class ControlCapturePath
	{
		public string ProcessNameRegex { get; set; }
		public ChildSelectOperation[] WindowChildSelectors { get; set; }
		public ChildSelectOperation[] ControlChildSelectors { get; set; }
		public string ValueSelector { get; set; }

		public bool IsApplicable(string processName)
		{
			return Regex.IsMatch(processName, ProcessNameRegex);
		}

		public IntPtr? GetChildren(IntPtr mainWindow)
		{
			var currentWindow = mainWindow;
			foreach (var childSelector in WindowChildSelectors)
			{
				var result = childSelector.Evaluate(currentWindow);
				if (result == null) return null;
				currentWindow = result.Value;
			}

			return currentWindow;
		}

		public AutomationElement GetChildren(AutomationElement element)
		{
			var currentElement = element;
			foreach (var childSelector in ControlChildSelectors)
			{
				var result = childSelector.Evaluate(currentElement);
				if (result == null) return null;
				currentElement = result;
			}

			return currentElement;
		}

		public string GetValue(AutomationElement element)
		{
			if (string.Equals(ValueSelector, "Text", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationElementHelper.GetText(element);
			}

			if (string.Equals(ValueSelector, "Value", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationElementHelper.GetValue(element);
			}

			if (string.Equals(ValueSelector, "Name", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationElementHelper.GetName(element);
			}

			if (string.Equals(ValueSelector, "Selection", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationElementHelper.GetSelection(element);
			}

			if (string.Equals(ValueSelector, "HelpText", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationElementHelper.GetHelpText(element);
			}

			if (string.Equals(ValueSelector, "RadioName", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationElementHelper.GetRadioName(element);
			}
			if (string.Equals(ValueSelector, "RadioValue", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationElementHelper.GetRadioValue(element);
			}

			throw new NotImplementedException();
		}
	}
}
