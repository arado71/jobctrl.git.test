using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginTelekomJazz2 : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "Telekom.Jazz2";
		private const string KeyPhone = "cms_phone_number";
		private const string KeyTarif = "cms_dijcsomag_inaktiv";
		private const string KeyPhoneReplace = "cms_phone_num_replace";
		private const string KeySimReplace = "cms_sim_replace";

		private Dictionary<IntPtr, AutomationElement> workspaceCache = new Dictionary<IntPtr, AutomationElement>(); 

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
		}

		private bool IsValid(AutomationElement element)
		{
			try
			{
				var processId = element.Current.ProcessId;
				return true;
			}
			catch (ElementNotAvailableException)
			{
				return false;
			}
		}

		private KeyValuePair<string, string>? CapturePhone(AutomationElement workspaceElement)
		{
			var accountNameCond = new PropertyCondition(AutomationElement.NameProperty, "Folyószámlák");
			var accountElement = workspaceElement.FindFirst(TreeScope.Children, accountNameCond);
			if (accountElement == null)
			{
				log.Verbose("Failed to get Account element");
				return null;
			}

			var phoneNameCond = new PropertyCondition(AutomationElement.NameProperty, "Szám :");
			var phoneTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
			var phoneCond = new AndCondition(phoneNameCond, phoneTypeCond);

			var phoneElement = accountElement.FindFirst(TreeScope.Children, phoneCond);
			if (phoneElement == null)
			{
				log.Verbose("Failed to get Phone element");
				return null;
			}

			return new KeyValuePair<string, string>(KeyPhone, AutomationHelper.GetValue(phoneElement));
		}

		private KeyValuePair<string, string>? CaptureTarif(AutomationElement workspaceElement)
		{
			var contractNameCond = new PropertyCondition(AutomationElement.NameProperty, "frm3Contract");
			var contractElement = workspaceElement.FindFirst(TreeScope.Children, contractNameCond);
			if (contractElement == null)
			{
				log.Verbose("Failed to get Contract element");
				return null;
			}

			var tarifNameCond = new PropertyCondition(AutomationElement.NameProperty, "Díjcsomag :");
			var tarifTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox);
			var tarifCond = new AndCondition(tarifNameCond, tarifTypeCond);
			var tarifElement = contractElement.FindFirst(TreeScope.Children, tarifCond);
			if (tarifElement == null)
			{
				log.Verbose("Failed to get Tarif element");
				return null;
			}

			if (tarifElement.Current.IsEnabled)
			{
				log.Verbose("Tarif element is activated");
				return null;
			}

			return new KeyValuePair<string, string>(KeyTarif, AutomationHelper.GetValue(tarifElement));
		}

		private IEnumerable<KeyValuePair<string, string>> CaptureContractService(AutomationElement workspaceElement)
		{
			var contractServiceNameCond = new PropertyCondition(AutomationElement.NameProperty,
				"frm3Contractedservice: Szolgáltatás paraméterek és eszközök ( Módosít )");
			var contractServiceElement = workspaceElement.FindFirst(TreeScope.Children, contractServiceNameCond);
			if (contractServiceElement == null)
			{
				log.Verbose("Failed to get ContractService");
				yield break;
			}

			var numberNameCond = new PropertyCondition(AutomationElement.NameProperty, "Szám");
			var numberTypeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
			var numberCond = new AndCondition(numberNameCond, numberTypeCond);
			var numberElement = contractServiceElement.FindAll(TreeScope.Children, numberCond);
			if (numberElement == null || numberElement.Count == 0)
			{
				log.Verbose("Failed to get NumberElement");
				yield break;
			}

			yield return new KeyValuePair<string, string>(KeyPhoneReplace, AutomationHelper.GetValue(numberElement[0]));

			if (numberElement.Count < 2)
			{
				log.Verbose("Not enough NumberElements");
				yield break;
			}

			yield return new KeyValuePair<string, string>(KeySimReplace, AutomationHelper.GetValue(numberElement[1]));
		}

		private AutomationElement GetWorkspaceElement(IntPtr hWnd)
		{
			var element = AutomationElement.FromHandle(hWnd);
			if (element == null)
			{
				log.Verbose("Failed to get AutomationElement");
				return null;
			}

			var workspaceNameCond = new PropertyCondition(AutomationElement.NameProperty, "Munkaterület");
			return element.FindFirst(TreeScope.Children, workspaceNameCond);
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!string.Equals(processName, "cms.exe", StringComparison.OrdinalIgnoreCase)) yield break;

			AutomationElement workspaceElement;
			if (workspaceCache.TryGetValue(hWnd, out workspaceElement))
			{
				if (!IsValid(workspaceElement))
				{
					workspaceCache.Remove(hWnd);
					workspaceElement = null;
				}
			}

			if (workspaceElement == null)
			{
				workspaceElement = GetWorkspaceElement(hWnd);
				workspaceCache[hWnd] = workspaceElement;
			}

			var capture = CapturePhone(workspaceElement);
			if (capture != null) yield return capture.Value;

			capture = CaptureTarif(workspaceElement);
			if (capture != null) yield return capture.Value;

			foreach (var result in CaptureContractService(workspaceElement))
			{
				yield return result;
			}
		}
	}
}
