using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public static class AutomationScriptHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// Example: Chrome URL capture
		// url:[class="Chrome_WidgetWin_1"]//[name="Google Chrome"]/[index=1]/[index=1]/[index=1]/[index=3]/[index=1]//![value];

		// {{Script}}: {{{NameAndQuery}};}*{{NameAndQuery}}
		// {{NameAndQuery}}: {{Name}}:{{Query}}
		// {{Query}}: {{WindowQuery}}//{{ElementQuery}}//{{StringSelector}}
		// {{WindowQuery}}: {{{WindowSelectionLevel}}/}*{{WindowSelectionLevel}}
		// {{ElementQuery}}: {{{ElementSelectionLevel}}/}*{{ElementSelectionLevel}}
		// {{StringSelector}}: {{Flag}}[{{ElementAttribute}}]
		// {{SelectionLevel}}: {{Flag}}{{AttributeCondition}}*
		// {{AttributeCondition}}: [{{AttributeName}}{{Operator}}{{Value}}]
		// {{WindowAttributeName}}: index|title|class
		// {{ElementAttributeName}}: index|name|class|text|value
		// {{Name}}: Anything except :
		// {{Operator}}: =|^=|~=|!=
		// {{Value}}: {{numeric}}|"{{string}}"
		// {{Flag}}: *|!|{{Empty}}
		// {{Numeric}}: 0-9
		// {{string}}: anything except "

		private const string WindowQueryRegex = @"^.*?//";
		private const string ElementQueryRegex = @"^.*?//";
		private const string FlagRegex = @"^(\*|\!)";
		private const string WindowAttributeConditionRegex = @"^\s*\[\s*(((title|class)\s*(\=|\~\=|\^\=|\!\=)\s*""(.*?)""\s*)|(index\s*\=\s*(\d+)))\s*\]";
		private const string ElementAttributeConditionRegex = @"^\s*\[\s*(((name|class|value|text)\s*(\=|\~\=|\^\=|\!\=)\s*""(.*?)""\s*)|(index\s*\=\s*(\d+)))\s*\]";
		private const string LevelRegex = @"^\s*(\*|\!)*\s*(\[(.*?("".*?"")?.*?)\])*?\s*(//|$|/)";
		private const string NameAndQueryRegex = @"^[^;""]*("".*?"")*[^;""]*(?=;|$)";
		private const string CharactersBeforeColonRegex = "^\\s*(.*?)\\:";

		public static List<AutomationCapture> Compile(string script)
		{
			var remaining = script;
			var result = new List<AutomationCapture>();
			do
			{
				if (string.IsNullOrEmpty(remaining)) break;
				string[] matchingGroups;
				var nameAndQueryRemaining = Parse(ref remaining, NameAndQueryRegex);
				if (nameAndQueryRemaining == null)
				{
					throw new ArgumentException("Invalid query in " + remaining);
				}

				if (!TryParse(ref nameAndQueryRemaining, CharactersBeforeColonRegex, out matchingGroups)) break;
				Debug.Assert(matchingGroups.Length >= 2);
				var queryName = matchingGroups[1];
				result.Add(CompileQuery(queryName, nameAndQueryRemaining));
				if (!TryParse(ref remaining, ";")) break;

			} while (true);
			if (remaining.Length != 0) throw new ArgumentException("Unable to process " + remaining);

			return result;
		}

		private static AutomationCapture CompileQuery(string queryName, string query)
		{
			string windowQuery, elementQuery, stringSelector;
			SplitQuery(query, out windowQuery, out elementQuery, out stringSelector);
			var result = new AutomationCapture(queryName);
			CompileWindowSelectorInto(windowQuery, result);
			CompileElementSelectorInto(elementQuery, result);
			CompileStringSelectorInto(stringSelector, result);
			return result;
		}

		private static void SplitQuery(string query, out string windowQuery, out string elementQuery, out string elementSelector)
		{
			var remaining = query;
			windowQuery = Parse(ref remaining, WindowQueryRegex);
			elementQuery = Parse(ref remaining, ElementQueryRegex);
			elementSelector = remaining;
		}

		private static void CompileWindowSelectorInto(string query, AutomationCapture target)
		{
			var remaining = query;
			string currentLevel;
			while ((currentLevel = Parse(ref remaining, LevelRegex)) != null)
			{
				var flags = Parse(ref currentLevel, FlagRegex);
				var windowLevels = ParseWindowLevel(currentLevel);
				if (!string.IsNullOrEmpty(flags))
				{
					switch (flags)
					{
						case "*":
							windowLevels = new List<SearchOperation>();
							break;
						default:
							throw new NotImplementedException();
					}
				}

				target.AddWindowSelector(GenerateWindowCondition(windowLevels));
			}

			if (!string.IsNullOrEmpty(remaining)) throw new ArgumentException("Invalid element inside " + remaining);
		}

		private static void CompileElementSelectorInto(string query, AutomationCapture target)
		{
			var remaining = query;
			string currentLevel;
			while ((currentLevel = Parse(ref remaining, LevelRegex)) != null)
			{
				var isCached = false;
				var flags = Parse(ref currentLevel, FlagRegex);
				var elementLevels = ParseElementLevel(currentLevel);
				if (elementLevels.Count == 0 && string.IsNullOrEmpty(flags)) break;
				if (!string.IsNullOrEmpty(flags))
				{
					switch (flags)
					{
						case "*":
							elementLevels = new List<SearchOperation>();
							break;
						case "!":
							isCached = true;
							break;
						default:
							throw new NotImplementedException();
					}
				}

				target.AddElementSelector(GenerateElementChildSelector(elementLevels), isCached);
			}

			if (!string.IsNullOrEmpty(remaining)) throw new ArgumentException("Invalid element inside " + remaining);
		}

		private static void CompileStringSelectorInto(string query, AutomationCapture target)
		{
			string[] selectionGroups;
			var remaining = query;
			if (TryParse(ref remaining, LevelRegex, out selectionGroups))
			{
				var cacheLastElement = !string.IsNullOrEmpty(selectionGroups[1]);
				var elementSelector = selectionGroups[3];
				target.AddElementSelector(CompileElementStringSelector(elementSelector), cacheLastElement);
			}
			else
			{
				throw new ArgumentException("Invalid selector in " + remaining);
			}
		}

		private static string Parse(ref string input, string regex)
		{
			string[] result;
			if (TryParse(ref input, regex, out result))
			{
				return result[0];
			}

			return null;
		}

		private static bool TryParse(ref string input, string regex)
		{
			string[] _;
			return TryParse(ref input, regex, out _);
		}

		private static bool TryParse(ref string input, string regex, out string[] match)
		{
			match = null;
			var regexMatch = Regex.Match(input, regex);
			if (regexMatch.Success && regexMatch.Length > 0)
			{
				match = regexMatch.Groups.Cast<Group>().Select(x => x.Value).ToArray();
				input = input.Substring(regexMatch.Index + regexMatch.Length);
				return true;
			}

			return false;
		}

		private static Func<IntPtr, string> CompileWindowStringSelector(string input)
		{
			if (string.Equals(input, "title", StringComparison.OrdinalIgnoreCase))
			{
				return WindowTextHelper.GetWindowText;
			}

			if (string.Equals(input, "class", StringComparison.OrdinalIgnoreCase))
			{
				return WindowTextHelper.GetClassName;
			}

			throw new ArgumentException("Unknown window attribute: " + input);
		}

		private static Func<AutomationElement, string> CompileElementStringSelector(string input)
		{
			if (string.Equals(input, "name", StringComparison.OrdinalIgnoreCase))
			{
				return x => AutomationHelper.GetName(x);
			}

			if (string.Equals(input, "class", StringComparison.OrdinalIgnoreCase))
			{
				return x => AutomationHelper.GetProperty(x, AutomationElement.ClassNameProperty);
			}

			if (string.Equals(input, "value", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationHelper.GetValue;
			}

			if (string.Equals(input, "text", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationHelper.GetText;
			}
			if (string.Equals(input, "radioname", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationHelper.GetRadioName;
			}
			if (string.Equals(input, "radiovalue", StringComparison.OrdinalIgnoreCase))
			{
				return AutomationHelper.GetRadioValue;
			}
			throw new ArgumentException("Unknown element attribute: " + input);
		}

		private static Func<string, string, bool> CompileStringOperator(string input)
		{
			if (string.Equals(input, "="))
			{
				return string.Equals;
			}

			if (string.Equals(input, "~="))
			{
				return (x, y) => x.IndexOf(y, StringComparison.Ordinal) >= 0;
			}

			if (string.Equals(input, "!="))
			{
				return (x, y) => !string.Equals(x, y);
			}

			if (string.Equals(input, "^="))
			{
				return (x, y) => x.StartsWith(y);
			}

			throw new ArgumentException("Unknown string operator: " + input);
		}

		private static Func<IntPtr, int, bool> GenerateWindowCondition(SearchOperation op)
		{
			if (string.Equals(op.Name, "index", StringComparison.OrdinalIgnoreCase))
			{
				var targetIndex = int.Parse(op.Parameter);
				return (x, y) => y == targetIndex;
			}

			var stringOperator = CompileStringOperator(op.Operator);
			var stringSelector = CompileWindowStringSelector(op.Name);
			return (x, y) => stringOperator(stringSelector(x), op.Parameter);
		}

		private static Func<AutomationElement, int, bool> GenerateElementCondition(SearchOperation op)
		{
			if (string.Equals(op.Name, "index", StringComparison.OrdinalIgnoreCase))
			{
				var targetIndex = int.Parse(op.Parameter);
				return (x, y) => y == targetIndex;
			}

			var stringOperator = CompileStringOperator(op.Operator);
			var stringSelector = CompileElementStringSelector(op.Name);
			return (x, y) => stringOperator(stringSelector(x), op.Parameter);
		}

		private static Func<IntPtr, int, bool> GenerateWindowCondition(IEnumerable<SearchOperation> operations)
		{
			Func<IntPtr, int, bool> result = (x, y) => true;

			foreach (var op in operations)
			{
				var winop = GenerateWindowCondition(op);
				var oldResult = result;
				result = (x, y) => oldResult(x, y) && winop(x, y);
			}

			return result;
		}

		private static Func<AutomationElement, int, bool> GenerateElementCondition(IEnumerable<SearchOperation> operations)
		{
			Func<AutomationElement, int, bool> result = (x, y) => true;

			foreach (var op in operations)
			{
				var winop = GenerateElementCondition(op);
				var oldResult = result;
				result = (x, y) => oldResult(x, y) && winop(x, y);
			}

			return result;
		}

		private static Func<AutomationElement, AutomationElement> GenerateElementChildSelector(List<SearchOperation> operations)
		{
			if (operations.Count == 1 && string.Equals(operations[0].Name, "name", StringComparison.OrdinalIgnoreCase) && operations[0].Operator == "=")
			{
				return
					x =>
						x.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, operations[0].Parameter));
			}

			if (operations.Count == 1 && string.Equals(operations[0].Name, "class", StringComparison.OrdinalIgnoreCase) && operations[0].Operator == "=")
			{
				return
					x =>
						x.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, operations[0].Parameter));
			}

			var ops = GenerateElementCondition(operations);
			return x =>
			{
				var i = 0;
				foreach (var child in x.FindAll(TreeScope.Children, Condition.TrueCondition).Cast<AutomationElement>())
				{
					if (ops(child, i++)) return child;
				}

				return null;
			};
		}

		private static List<SearchOperation> ParseElementLevel(string input)
		{
			var result = new List<SearchOperation>();
			string[] res;
			while (TryParse(ref input, ElementAttributeConditionRegex, out res))
			{
				if (!string.IsNullOrEmpty(res[7]))
				{
					result.Add(new SearchOperation { Name = "index", Operator = "=", Parameter = res[7] });
				}
				else
				{
					result.Add(new SearchOperation { Name = res[3], Operator = res[4], Parameter = res[5] });
				}
			}

			if (!string.IsNullOrEmpty(input.TrimStart(' ', '/'))) throw new ArgumentException("Invalid element query in " + input);

			return result;
		}

		private static List<SearchOperation> ParseWindowLevel(string input)
		{
			var result = new List<SearchOperation>();
			string[] res;
			while (TryParse(ref input, WindowAttributeConditionRegex, out res))
			{
				if (!string.IsNullOrEmpty(res[7]))
				{
					result.Add(new SearchOperation { Name = "index", Operator = "=", Parameter = res[7] });
				}
				else
				{
					result.Add(new SearchOperation { Name = res[3], Operator = res[4], Parameter = res[5] });
				}
			}

			if (!string.IsNullOrEmpty(input.TrimStart(' ', '/'))) throw new ArgumentException("Invalid window query in " + input);

			return result;
		}

		private class SearchOperation
		{
			public string Name { get; set; }
			public string Operator { get; set; }
			public string Parameter { get; set; }
		}
	}
}
