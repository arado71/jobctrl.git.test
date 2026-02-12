using Tct.JcMon.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.Java.Accessibility;

namespace JavaMon.Plugin
{
	enum PluginValueType	
	{
		Name,
		Role,
		Description,
		Text,
		ComboValue,
		Table
	}

	class PluginController
	{
		public static string Compile(AccessibleItem accItem, PluginValueType pvt, string parameters = null)
		{
			string res = "myKey:*//";
			List<string> indexList = new List<string>();
			while (accItem.Parent != null)
			{
				indexList.Add($"[index={accItem.IndexInParent}]/");
				accItem = accItem.Parent;
			}
			indexList.Reverse();
		 	res += string.Join("", indexList);
			if (parameters != null)
			{
				res += $"/[{pvt.ToString()}({parameters})]";
			}
			else
			{
				res += $"/[{pvt.ToString()}]";
			}
			return res;
		}

		public static List<Func<IntPtr, CaptureResult>> Decompile(string param)
		{
			string[] splitted = param.Split(';');
			List<Func<IntPtr, CaptureResult>> result = new List<Func<IntPtr, CaptureResult>>();
			foreach (var query in splitted)
			{
				string[] splittedQuery = query.Split(new[] { "//" }, StringSplitOptions.None);
				string name = splittedQuery[0].Substring(0, splittedQuery[0].IndexOf(':'));
				string[] elementQueries = splittedQuery[1].Split('/');
				List<int> indexes = new List<int>();
				foreach (var elementQuery in elementQueries)
				{
					var index = int.Parse(elementQuery.Substring(7, elementQuery.Length - 8));
					indexes.Add(index);
				}

				string capturableValueTypeString = splittedQuery[2].Substring(1, splittedQuery[2].Length - 2);
				PluginValueType pvt;
				string pluginValueTypeString;
				string parameters = null;
				if (capturableValueTypeString.Contains("("))
				{
					int indexOfParenthesis = capturableValueTypeString.IndexOf("(", StringComparison.Ordinal);
					pluginValueTypeString = capturableValueTypeString.Substring(0, indexOfParenthesis);
					parameters = capturableValueTypeString.Substring(indexOfParenthesis + 1, capturableValueTypeString.IndexOf(")", StringComparison.Ordinal) - (indexOfParenthesis + 1));
				}
				else
				{
					pluginValueTypeString = capturableValueTypeString;
				}
				pvt = (PluginValueType)Enum.Parse(typeof(PluginValueType),
					pluginValueTypeString);

				result.Add(getCaptureFuncFromIndexList(name, indexes, pvt, parameters));
			}
			return result;
		}

		private static Func<IntPtr, CaptureResult> getCaptureFuncFromIndexList(string captureName, List<int> indexes, PluginValueType pvt, string parameters)
		{
			return hwnd =>
			{
				Stopwatch sw = Stopwatch.StartNew();
				CaptureResult result = new CaptureResult();
				result.Name = captureName;
				if (!JabApiController.Instance.IsJavaWindow(hwnd))
					return result;
				AccessibleWrapper aw = JabApiController.Instance.GetContextFromHwnd(hwnd);
				foreach (var index in indexes)
				{
					aw = JabApiController.Instance.GetChildElementAt(aw, index);
				}
				switch (pvt)
				{
					case PluginValueType.Description:
						result.Value = aw.Description;
						break;
					case PluginValueType.Name: 
						result.Value = aw.Name;
						break;
					case PluginValueType.Role:
						result.Value = aw.Role;
						break;
					case PluginValueType.Text:
						result.Value = JabApiController.Instance.GetTextElementFromAccessibleWrapper(aw);
						break;
					case PluginValueType.ComboValue:
						result.Value = JabApiController.Instance.GetComboValueFromAccessibleWrapper(aw);
						break;
					case PluginValueType.Table:
						int colNumber = int.Parse(parameters);
						result.Value = JabApiController.Instance.GetTableValueFromAccessibleWrapper(aw, colNumber);
						break;
				}
				result.ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds;
				return result;
			};
		}
	}
}
