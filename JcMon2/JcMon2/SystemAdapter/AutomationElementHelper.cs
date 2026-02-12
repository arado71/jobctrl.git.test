using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;
using log4net;

namespace JcMon2.SystemAdapter
{
	public static class AutomationElementHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static bool TryGetValueFromHandle(IntPtr hWnd, ILog logOvr, out string value)
		{
			try
			{
				var element = AutomationElement.FromHandle(hWnd);
				return TryGetValueFromElement(element, logOvr, out value);
			}
			catch (Exception ex)
			{
				(logOvr ?? log).Debug("Cannot get AutomationElement from handle", ex);
				Debug.Fail(ex.Message);
			}
			value = null;
			return false;
		}

		public static bool TryGetValueFromElement(AutomationElement element, ILog logOvr, out string value)
		{
			try
			{
				object valuePattern;
				if (element != null
					&& element.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern)
					&& valuePattern != null)
				{
					value = ((ValuePattern)valuePattern).Current.Value;
					return true;
				}
			}
			catch (Exception ex)
			{
				(logOvr ?? log).Debug("Cannot get value from AutomationElement", ex);
				Debug.Fail(ex.Message);
			}
			value = null;
			return false;
		}

		public static bool TryGetSelectionFromElement(AutomationElement element, ILog logOvr, out string value)
		{
			try
			{
				object pattern;
				TextPattern textPattern;
				if (element != null
					&& element.TryGetCurrentPattern(TextPattern.Pattern, out pattern)
					&& (textPattern = pattern as TextPattern) != null
					&& textPattern.SupportedTextSelection != SupportedTextSelection.None)
				{
					var range = textPattern.GetSelection();
					if (range != null)
					{
						value = string.Join("", range.Select(x => x.GetText(-1)).ToArray());
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				(logOvr ?? log).Debug("Cannot get value from AutomationElement", ex);
				Debug.Fail(ex.Message);
			}
			value = null;
			return false;
		}

		public static bool TryGetTextFromElement(AutomationElement element, ILog logOvr, out string value)
		{
			try
			{
				object pattern;
				TextPattern textPattern;
				if (element != null
					&& element.TryGetCurrentPattern(TextPattern.Pattern, out pattern)
					&& (textPattern = pattern as TextPattern) != null)
				{
					var range = textPattern.DocumentRange;
					if (range != null)
					{
						value = range.GetText(-1);
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				(logOvr ?? log).Debug("Cannot get value from AutomationElement", ex);
				Debug.Fail(ex.Message);
			}
			value = null;
			return false;
		}

		public static string GetProperty(AutomationElement element, AutomationProperty property, bool canThrow = false)
		{
			try
			{
				return element == null
					? ""
					: ((element.GetCurrentPropertyValue(property, true) as string) ?? "");
			}
			catch (Exception)
			{
				if (canThrow)
					throw;
				return "";
			}
		}

		public static string GetText(AutomationElement element)
		{
			string result;
			TryGetTextFromElement(element, null, out result);
			return result ?? "";
		}

		public static string GetValue(AutomationElement element)
		{
			string result;
			TryGetValueFromElement(element, null, out result);
			return result ?? "";
		}

		public static string GetSelection(AutomationElement element)
		{
			string result;
			TryGetSelectionFromElement(element, null, out result);
			return result ?? "";
		}

		public static string GetName(AutomationElement element, bool canThrow = false)
		{
			return GetProperty(element, AutomationElement.NameProperty, canThrow);
		}

		public static string GetHelpText(AutomationElement element, bool canThrow = false)
		{
			return GetProperty(element, AutomationElement.HelpTextProperty, canThrow);
		}

		public static string GetControlType(AutomationElement element, bool canThrow = false)
		{
			return GetProperty(element, AutomationElement.ControlTypeProperty, canThrow);
		}
	}
}
