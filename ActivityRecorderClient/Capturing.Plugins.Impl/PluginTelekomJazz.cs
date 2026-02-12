using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginTelekomJazz : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "Telekom.Jazz";
		private const string KeyPhone = "Phone";
		private const string KeyTarif = "Tarif";

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
			yield return KeyPhone;
			yield return KeyTarif;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (string.Equals("cms.exe", processName, StringComparison.OrdinalIgnoreCase))
			{
				return Extract(hWnd);
			}

			return null; //wrong process name
		}

		private KeyValuePair<string, string>? GetPhone(IntPtr hWnd)
		{
			var accountsElement = WinApi.FindWindowEx(hWnd, IntPtr.Zero, "Centura:AccFrame", "Folyószámlák");
			if (accountsElement == IntPtr.Zero)
			{
				log.Verbose("AccountsElement not found");
				return null;
			}

			var element = AutomationElement.FromHandle(accountsElement);
			var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
			if (children.Count < 46)
			{
				log.Verbose("Not enough elements in AccountsElement");
				return null;
			}

			element = children[45];
			if (!string.Equals(AutomationHelper.GetName(element), "Szám :", StringComparison.Ordinal))
			{
				log.Verbose("Phone element name not as expected");
				return null;
			}

			return new KeyValuePair<string, string>(KeyPhone, AutomationHelper.GetValue(element));
		}

		private KeyValuePair<string, string>? GetTarif(IntPtr hWnd)
		{
			var contractsElement = WinApi.FindWindowEx(hWnd, IntPtr.Zero, "Centura:Form", "frm3Contract: Szerződés (Módosít )");
			if (contractsElement == IntPtr.Zero)
			{
				log.Verbose("ContractsElement not found");
				return null;
			}

			var element = AutomationElement.FromHandle(contractsElement);
			var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
			if (children.Count < 10)
			{
				log.Verbose("Not enough elements in ContractsElement");
				return null;
			}

			element = children[9];
			if (!string.Equals(AutomationHelper.GetName(element), "Díjcsomag :", StringComparison.Ordinal))
			{
				log.Verbose("Tarif element name not as expected");
				return null;
			}

			return new KeyValuePair<string, string>(KeyTarif, AutomationHelper.GetValue(element));
		}

		private IEnumerable<KeyValuePair<string, string>> Extract(IntPtr hWnd)
		{
			var workArea = WinApi.FindWindowEx(hWnd, IntPtr.Zero, "MDIClient", "Munkaterület");
			if (workArea == IntPtr.Zero)
			{
				log.Verbose("Workarea not found");
				yield break;
			}

			var result = GetPhone(workArea);
			if (result != null) yield return result.Value;

			result = GetTarif(workArea);
			if (result != null) yield return result.Value;
		}
	}
}
