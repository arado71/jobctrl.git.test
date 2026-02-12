using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using JCAutomation.SystemAdapter;

namespace JCAutomation.Extraction
{
	[DataContract]
	public class ChildSelectOperation
	{
		public string Operation { get; set; }
		public string Type { get; set; }
		public string Parameter { get; set; }

		public IntPtr? Evaluate(IntPtr window)
		{
			var children = WindowHelper.GetChildren(window);
			if (string.Equals(Operation, "getChildByPosition", StringComparison.OrdinalIgnoreCase))
			{
				int targetIndex;
				if (!int.TryParse(Parameter, out targetIndex)) return null;
				var i = 0;
				foreach (var child in children)
				{
					if (++i == targetIndex) return child;
				}

				return null;
			}

			var hwndToString = GetWindowStringSelector();
			var isMatch = GetStringIsMatch();
			foreach (var child in children)
			{
				var str = hwndToString(child);
				if (isMatch(str)) return child;
			}

			return null;
		}

		public AutomationElement Evaluate(AutomationElement element)
		{
			var children = AutomationHelper.GetChildren(element);
			if (string.Equals(Operation, "getChildByPosition", StringComparison.OrdinalIgnoreCase))
			{
				int targetIndex;
				if (!int.TryParse(Parameter, out targetIndex)) return null;
				var i = 0;
				foreach (var child in children)
				{
					if (++i == targetIndex) return child;
				}

				return null;
			}

			var elementToString = GetControlStringSelector();
			var isMatch = GetStringIsMatch();
			foreach (var child in children)
			{
				var str = elementToString(child);
				if (isMatch(str)) return child;
			}

			return null;
		}

		private Func<IntPtr, string> GetWindowStringSelector()
		{
			if (string.Equals(Operation, "getChildByTitle", StringComparison.OrdinalIgnoreCase))
			{
				return WindowHelper.GetWindowText;
			}

			if (string.Equals(Operation, "getChildByClassName", StringComparison.OrdinalIgnoreCase))
			{
				return WindowHelper.GetClassName;
			}

			throw new NotImplementedException();
		}

		private Func<AutomationElement, string> GetControlStringSelector()
		{
			if (string.Equals(Operation, "getChildByName", StringComparison.OrdinalIgnoreCase))
			{
				return x => AutomationElementHelper.GetName(x);
			}

			if (string.Equals(Operation, "getChildByControlType", StringComparison.OrdinalIgnoreCase))
			{
				return x => AutomationElementHelper.GetControlType(x);
			}

			if (string.Equals(Operation, "getChildByHelpText", StringComparison.OrdinalIgnoreCase))
			{
				return x => AutomationElementHelper.GetHelpText(x);
			}

			throw new NotImplementedException();
		}

		private Func<string, bool> GetStringIsMatch()
		{
			if (string.Equals(Type, "regex", StringComparison.OrdinalIgnoreCase))
			{
				return x => Regex.IsMatch(x, Parameter, RegexOptions.IgnoreCase);
			}

			if (string.Equals(Type, "contains", StringComparison.OrdinalIgnoreCase))
			{
				return x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(Parameter, x, CompareOptions.OrdinalIgnoreCase) >= 0;
			}

			if (string.Equals(Type, "equals", StringComparison.OrdinalIgnoreCase))
			{
				return x => string.Equals(x, Parameter, StringComparison.OrdinalIgnoreCase);
			}

			throw new NotImplementedException();
		}
	}
}
