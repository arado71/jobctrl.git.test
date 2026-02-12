using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Automation;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginAlterdataPessoal : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "Alterdata.Pessoal";
		private const string Key1Text = "CadastroDeFuncionário";
		private const string Key2Text = "CadastroDeDependentes";
		private const string Key3Text = "MovimentoIndividual";
		private const string Key4Text = "CadastroDeEmpresa";
		private const string Key5Text = "DepartamentosCentroDeCusto";
		private const string Key6Text = "DepartamentosCentroDeCustoEdição";
		private const string Key7Text = "SelecaoDeEmpresas";
		private const string Key8Text = "RelatorioDeEmpresas";
		private const string Key9Text = "Afastamento";
		private const string Key10Text = "ImpressaoEGeraçãoEmDisco";
		private const string Key11Text = "Processos";
		private const string Key12Text = "Processos2";

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
			yield return Key11Text;
			yield return Key12Text;
		}

		private KeyValuePair<string, string>? CaptureKey1(AutomationElement mainWindow)
		{
			try
			{
				if (
					!string.Equals(AutomationHelper.GetName(mainWindow), "Cadastro de Funcionários",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key1 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TSelecaoEmpresa");
				if (element == null)
				{
					log.Verbose("Key1 - Failed to get TSelecaoEmpresa");
					return null;
				}

				element = element.GetChildByIndex(3);
				if (element == null)
				{
					log.Verbose("Key1 - Failed to get target element");
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
				if (!AutomationHelper.GetName(mainWindow).StartsWith("Dependentes", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key2 - Invalid title");
					return null;
				}

				var element = mainWindow.GetChildByIndex(5);
				if (element == null)
				{
					log.Verbose("Key2 - Failed to get sixth child");
					return null;
				}

				return new KeyValuePair<string, string>(Key2Text, AutomationHelper.GetName(element));
			}
			catch (Exception ex)
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "Movimento Individual",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key3 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TSelecaoEmpresa");
				if (element == null)
				{
					log.Verbose("Key3 - Failed to get TSelecaoEmpresa");
					return null;
				}

				element = element.GetChildByIndex(3);
				if (element == null)
				{
					log.Verbose("Key3 - Failed to get fourth child");
					return null;
				}

				return new KeyValuePair<string, string>(Key3Text, AutomationHelper.GetName(element));
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
				if (!string.Equals(AutomationHelper.GetName(mainWindow), "Empresas (Ediçăo)", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key4 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TcxPageControl");
				if (element == null)
				{
					log.Verbose("Key4 - Failed to get TcxPageControl");
					return null;
				}

				element = element.GetFirstChildByName("Geral");
				if (element == null)
				{
					log.Verbose("Key4 - Failed to get Geral");
					return null;
				}

				element = element.GetChildByIndex(3);
				if (element == null)
				{
					log.Verbose("Key4 - Invalid fourth child");
					return null;
				}

				element = element.GetChildByIndex(3);
				if (element == null)
				{
					log.Verbose("Key4 - Invalid fourth child 2");
					return null;
				}

				return new KeyValuePair<string, string>(Key4Text, AutomationHelper.GetName(element));
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "Departamentos / Centro de Custo",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key5 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TcxTextEdit");
				if (element == null)
				{
					log.Verbose("Key5 - TcxTextEdit child not found");
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "Departamentos / Centro de Custo (Ediчуo)", StringComparison.OrdinalIgnoreCase))
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

				if (!string.Equals(AutomationHelper.GetProperty(element, AutomationElement.ClassNameProperty), "TcxPageControl"))
				{
					log.Verbose("Key6 - Invalid TcxPageControl child");
					return null;
				}

				element = element.GetFirstChildByName("Dados");
				if (element == null)
				{
					log.Verbose("Key6 - No Dados child found");
					return null;
				}

				element = element.GetChildByIndex(5);
				if (element == null)
				{
					log.Verbose("Key6 - No target control found");
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "Seleчуo de Empresas", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key7 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TSelectPadraoEmpresas");
				if (element == null)
				{
					log.Verbose("Key7 - Unable to find TSelectPadraoEmpresas");
					return null;
				}

				element = element.GetFirstChildByClassName("TSelectPadrao");
				if (element == null)
				{
					log.Verbose("Key7 - Unable to find TSelectPadrao");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key7 - Unable to find third child");
					return null;
				}

				element = element.GetChildByIndex(5);
				if (element == null)
				{
					log.Verbose("Key7 - Unable to find sixth child");
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
				if (!string.Equals(AutomationHelper.GetName(mainWindow), "Relatórios de Empresas", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key8 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key8 - Failed to get first child");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key8 - Failed to get first child 2");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetName(element), "Empresas"))
				{
					log.Verbose("Key8 - Invalid name");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key8 - Failed to get first child 3");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key8 - Failed to get third child");
					return null;
				}

				element = element.GetChildByIndex(8);
				if (element == null)
				{
					log.Verbose("Key8 - Failed to get target");
					return null;
				}

				return new KeyValuePair<string, string>(Key8Text, AutomationHelper.GetName(element));
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
				if (!string.Equals(AutomationHelper.GetName(mainWindow), "Afastamentos", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key9 - Invalid title");
					return null;
				}

				var element = mainWindow.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key9 - Third child not found");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetProperty(element, AutomationElement.ClassNameProperty), "TSelecaoEmpresa"))
				{
					log.Verbose("Key9 - Invalid control name");
					return null;
				}

				element = element.GetChildByIndex(3);
				if (element == null)
				{
					log.Verbose("Key9 - First child not found 2");
					return null;
				}

				return new KeyValuePair<string, string>(Key9Text, AutomationHelper.GetName(element));
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "Impressăo e geraçăo em disco da GFIP / GRRF", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key10 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TcxPageControl");
				if (element == null)
				{
					log.Verbose("Key10 - TcxPageControl not found");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key10 - First child not found");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key10 - First child not found 2");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetProperty(element, AutomationElement.ClassNameProperty), "TSelectPadrao"))
				{
					log.Verbose("Key10 - Wrong classname");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key10 - Third child not found");
					return null;
				}

				element = element.GetChildByIndex(8);
				if (element == null)
				{
					log.Verbose("Key10 - Target not found");
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

		private KeyValuePair<string, string>? CaptureKey11(AutomationElement mainWindow)
		{
			try
			{
				if (!string.Equals(AutomationHelper.GetName(mainWindow), "Processos", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key11 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TNotebook");
				if (element == null)
				{
					log.Verbose("Key11 - TNotebook child not found");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key11 - First child not found");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetName(element), "SelEmpDptoFunc"))
				{
					log.Verbose("Key11 - Invalid name");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key11 - First child not found 2");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key11 - First child not found 3");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key11 - Third child not found");
					return null;
				}

				element = element.GetChildByIndex(8);
				if (element == null)
				{
					log.Verbose("Key11 - Target element not found");
					return null;
				}

				return new KeyValuePair<string, string>(Key9Text, AutomationHelper.GetName(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key11 failed", ex);
				return null;
			}
		}

		private KeyValuePair<string, string>? CaptureKey12(AutomationElement mainWindow)
		{
			try
			{
				if (
					!string.Equals(AutomationHelper.GetName(mainWindow), "Processos", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key12 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TNotebook");
				if (element == null)
				{
					log.Verbose("Key12 - TNotebook not found");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key12 - First child not found");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetName(element), "SelEmpFunc"))
				{
					log.Verbose("Key12 - Child name invalid");
					return null;
				}

				element = element.GetChildByIndex(4);
				if (element == null)
				{
					log.Verbose("Key10 - Target child not found");
					return null;
				}

				return new KeyValuePair<string, string>(Key10Text, AutomationHelper.GetName(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key12 failed", ex);
				return null;
			}
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (!string.Equals(processName, "WDP.EXE", StringComparison.OrdinalIgnoreCase)) yield break;

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

			result = CaptureKey8(element);
			if (result != null) yield return result.Value;

			result = CaptureKey9(element);
			if (result != null) yield return result.Value;

			result = CaptureKey10(element);
			if (result != null) yield return result.Value;

			result = CaptureKey11(element);
			if (result != null) yield return result.Value;

			result = CaptureKey12(element);
			if (result != null) yield return result.Value;
		}
	}
}
