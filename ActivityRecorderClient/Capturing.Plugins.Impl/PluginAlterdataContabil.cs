using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginAlterdataContabil : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "Alterdata.Contabil";
		private const string Key1Text = "AberturaLançamentosTemporarios";
		private const string Key2Text = "AlterDataContabil";
		private const string Key3Text = "ConsultaDeMovimentosLançamentos";
		private const string Key4Text = "LançamentoTemporárioCadastramento";
		private const string Key5Text = "NovoLançamentoTemporário";
		private const string Key6Text = "ImportaçãoDeLançamentoTemporários";
		private const string Key7Text = "LocalizarLancamentos";
		private const string Key8Text = "BalanceteDinâmico";
		private const string Key9Text = "ConsultaSaldo";
		private const string Key10Text = "ImpressaoDeBalancete";

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
			yield return Key1Text;
			yield return Key2Text;
			yield return Key3Text;
			yield return Key4Text;
			yield return Key5Text;
			yield return Key6Text;
			yield return Key7Text;
			yield return Key8Text;
			yield return Key9Text;
			yield return Key10Text;
		}

		private KeyValuePair<string, string>? CaptureKey1(AutomationElement mainWindow)
		{
			try
			{
				if (
					!string.Equals(AutomationHelper.GetName(mainWindow), "Abertura de Lançamentos Temporários",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key1 - Invalid title");
					return null;
				}

				var element = mainWindow.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key1 - Failed to get second child of main window");
					return null;
				}

				element = element.GetFirstChildByName("Empresa", ControlType.Pane);
				if (element == null)
				{
					log.Verbose("Key1 - Failed to get 'Empresa' control");
					return null;
				}

				element = element.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key1 - Failed to get value element");
					return null;
				}

				return new KeyValuePair<string, string>(Key1Text, AutomationHelper.GetName(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key1 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey2(AutomationElement mainWindow)
		{
			try
			{
				if (!AutomationHelper.GetName(mainWindow).StartsWith("Alterdata Contábil", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key2 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TcxPageControl");
				if (element == null)
				{
					log.Verbose("Key2 - Failed to get TcxPageControl");
					return null;
				}

				element = element.GetFirstChildByClassName("TcxTabSheet");
				if (element == null)
				{
					log.Verbose("Key2 - Failed to get TcxTabSheet");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key2 - Failed to get first child");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key2 - Failed to get first child 2");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key2 - Failed to get first child 3");
					return null;
				}

				element = element.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key2 - Failed to get second child");
					return null;
				}

				return new KeyValuePair<string, string>(Key2Text, AutomationHelper.GetValue(element));
			}
			catch(Exception ex)
			{
				log.Verbose("Key2 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey3(AutomationElement mainWindow)
		{
			try
			{
				if (
					!string.Equals(AutomationHelper.GetName(mainWindow), "Consulta de Movimentos / Lançamentos",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key3 - Invalid title");
					return null;
				}

				var element = mainWindow.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key3 - Failed to get second child");
					return null;
				}

				element = element.GetChildByIndex(5);
				if (element == null)
				{
					log.Verbose("Key3 - Failed to get sixth child");
					return null;
				}

				return new KeyValuePair<string, string>(Key3Text, AutomationHelper.GetValue(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key3 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey4(AutomationElement mainWindow)
		{
			try
			{
				if (!AutomationHelper.GetName(mainWindow).StartsWith("Alterdata Contábil", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key4 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key4 - Invalid first child");
					return null;
				}

				if (
					!string.Equals(AutomationHelper.GetProperty(element, AutomationElement.ClassNameProperty), "TcxPageControl",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key4 - Invalid first child class");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key4 - Invalid first child 2");
					return null;
				}

				if (
					!string.Equals(AutomationHelper.GetProperty(element, AutomationElement.ClassNameProperty), "TcxTabSheet",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key4 - Invalid first child class 2");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key4 - Invalid first child 3");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key4 - Invalid first child 4");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key4 - Invalid first child 5");
					return null;
				}

				return new KeyValuePair<string, string>(Key4Text, AutomationHelper.GetValue(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key4 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey5(AutomationElement mainWindow)
		{
			try
			{
				if (
					!string.Equals(AutomationHelper.GetName(mainWindow), "Novo Lançamento Temporário",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key5 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key5 - First child not found");
					return null;
				}

				element = element.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key5 - Second child not found");
					return null;
				}

				return new KeyValuePair<string, string>(Key5Text, AutomationHelper.GetName(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key5 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey6(AutomationElement mainWindow)
		{
			try
			{
				if (
					!string.Equals(AutomationHelper.GetName(mainWindow), "Importaçăo de Lançamentos Temporários",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key6 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key6 - First child not found");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetName(element), "pgeEmpresa", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key6 - Invalid first child name");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key6 - First child not found 2");
					return null;
				}

				element = element.GetChildByIndex(3);
				if (element == null)
				{
					log.Verbose("Key6 - Fourth child not found");
					return null;
				}

				return new KeyValuePair<string, string>(Key6Text, AutomationHelper.GetName(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key6 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey7(AutomationElement mainWindow)
		{
			try
			{
				if (
					!string.Equals(AutomationHelper.GetName(mainWindow), "Localizar Lançamentos", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key7 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TcxButtonEdit");
				if (element == null)
				{
					log.Verbose("Key7 - Unable to find TcxButtonEdit");
					return null;
				}

				return new KeyValuePair<string, string>(Key7Text, AutomationHelper.GetName(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key7 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey8(AutomationElement mainWindow)
		{
			try
			{
				if (!string.Equals(AutomationHelper.GetName(mainWindow), "Balancete dinâmico", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key8 - Invalid title");
					return null;
				}

				var element = mainWindow.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key8 - Failed to get child by index");
					return null;
				}

				element = element.GetFirstChildByName("Empresa");
				if (element == null)
				{
					log.Verbose("Key8 - Failed to get empresa control");
					return null;
				}

				element = element.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key8 - Failed to get target control");
					return null;
				}

				return new KeyValuePair<string, string>(Key8Text, AutomationHelper.GetValue(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key8 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey9(AutomationElement mainWindow)
		{
			try
			{
				if (!string.Equals(AutomationHelper.GetName(mainWindow), "Consulta de Saldo", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key9 - Invalid title");
					return null;
				}

				var element = mainWindow.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key9 - Second child not found");
					return null;
				}

				element = element.GetFirstChildByName("Empresa");
				if (element == null)
				{
					log.Verbose("Key9 - Empresa control not found");
					return null;
				}

				element = element.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key9 - Target control not found");
					return null;
				}

				return new KeyValuePair<string, string>(Key9Text, AutomationHelper.GetValue(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key9 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey10(AutomationElement mainWindow)
		{
			try
			{
				if (
					!string.Equals(AutomationHelper.GetName(mainWindow), "Impressăo do Balancete", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key10 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByName("pgeSelecao");
				if (element == null)
				{
					log.Verbose("Key10 - pgeSelecao not found");
					return null;
				}

				element = element.GetFirstChildByClassName("TSelectPadrao");
				if (element == null)
				{
					log.Verbose("Key10 - TSelectPadrao not found");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key10 - Third child not found");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key10 - Target control not found");
					return null;
				}

				return new KeyValuePair<string, string>(Key10Text, AutomationHelper.GetName(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key10 failed", ex);
				return null;
			}
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (string.Equals(processName, "altpack_wcont_balancete_dinamico.exe", StringComparison.OrdinalIgnoreCase))
			{
				var altElement = AutomationElement.FromHandle(hWnd);
				if (altElement == null) yield break;
				var res = CaptureKey8(altElement);
				if (res != null) yield return res.Value;
			}

			if (!string.Equals(processName, "wcont.exe", StringComparison.OrdinalIgnoreCase)) yield break;

			var element = AutomationElement.FromHandle(hWnd);
			if (element == null)
			{
				log.Verbose("Failed to get automation element from handle");
				yield break;
			}

			var result = CaptureKey1(element);
			if (result != null) yield return result.Value;

			result = CaptureKey2(element);
			if (result != null) yield return result.Value;

			result = CaptureKey3(element);
			if (result != null) yield return result.Value;

			result = CaptureKey4(element);
			if (result != null) yield return result.Value;

			result = CaptureKey5(element);
			if (result != null) yield return result.Value;

			result = CaptureKey6(element);
			if (result != null) yield return result.Value;

			result = CaptureKey7(element);
			if (result != null) yield return result.Value;

			result = CaptureKey9(element);
			if (result != null) yield return result.Value;

			result = CaptureKey10(element);
			if (result != null) yield return result.Value;

		}
	}
}
