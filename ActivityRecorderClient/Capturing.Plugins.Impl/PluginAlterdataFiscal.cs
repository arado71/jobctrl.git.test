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
	public class PluginAlterdataFiscal : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "Alterdata.Fiscal";
		private const string Key1Text = "ConsultaDeNotasFiscaisEProdutosModeloAtual";
		private const string Key2Text = "MovimentoDeEntrada";
		private const string Key3Text = "ConsultaDeNotasFiscaisEProdutosModeloPisCofins";
		private const string Key4Text = "ReduçãoZMapaResumo";
		private const string Key5Text = "MovimentoDeIssTomador";
		//private const string Key6Text = "MovimentoDeIssTomadorCadastramento";
		//private const string Key7Text = "MovimentaçãoDeItensDaNota";
		private const string Key8Text = "MovimentoDeIssPrestador";
		private const string Key9Text = "ImpressõesDeLivrosFiscais";
		private const string Key10Text = "SpedFiscal";
		private const string Key11Text = "GeraçãoDeLivroEmDisco";
		private const string Key12Text = "SpedPisCofins";

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
			//yield return Key6Text;
			//yield return Key7Text;
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "Consulta de Notas Fiscais e Produtos - Cenário: MODELO PIS COFINS",
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
				if (!AutomationHelper.GetName(mainWindow).StartsWith("Alterdata Fiscal", StringComparison.OrdinalIgnoreCase))
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

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key2 - Failed to get first child 4");
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "Consulta de Notas Fiscais e Produtos - Cenário: MODELO PIS COFINS",
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

				element = element.GetChildByIndex(3);
				if (element == null)
				{
					log.Verbose("Key3 - Failed to get fourth child");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetName(element), "Empresa", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key3 - Wrong element name");
					return null;
				}

				element = element.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key3 - Failed to get target element");
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
				if (!AutomationHelper.GetName(mainWindow).StartsWith("Reduчуo Z / Mapa Resumo", StringComparison.OrdinalIgnoreCase))
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

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key4 - Invalid first child 2");
					return null;
				}

				element = element.GetChildByIndex(1);
				if (element == null)
				{
					log.Verbose("Key4 - Invalid second child");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetName(element), "Dados Bcsicos", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key4 - Invalid control name");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key4 - Invalid target control");
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "Movimento de ISS Tomador",
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

				element = element.GetChildByIndex(3);
				if (element == null)
				{
					log.Verbose("Key5 - Fourth child not found");
					return null;
				}

				return new KeyValuePair<string, string>(Key5Text, AutomationHelper.GetValue(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key5 failed", ex);
				return null;
			}
		}

		/*private KeyValuePair<string, string>? CaptureKey6(AutomationElement mainWindow)
		{
			try
			{
				if (
					!string.Equals(AutomationHelper.GetName(mainWindow), "Movimento de ISS Tomador",
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
		}*/

		private KeyValuePair<string, string>? CaptureKey8(AutomationElement mainWindow)
		{
			try
			{
				if (!string.Equals(AutomationHelper.GetName(mainWindow), "Movimento de ISS", StringComparison.OrdinalIgnoreCase))
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

				element = element.GetChildByIndex(3);
				if (element == null)
				{
					log.Verbose("Key8 - Failed to get target control");
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
				if (!string.Equals(AutomationHelper.GetName(mainWindow), "Impressăo de Livros Fiscais", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key9 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key9 - First child not found");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetName(element), "tbsEmpresas", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key9 - Invalid control name");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key9 - First child not found 2");
					return null;
				}

				if (
					!string.Equals(AutomationHelper.GetProperty(element, AutomationElement.ClassNameProperty), "TSelectPadrao",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key9 - Invalid control name 2");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key9 - Third child not found");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key9 - Third child not found 2");
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "SPED Fiscal", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key10 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TTabSheet");
				if (element == null)
				{
					log.Verbose("Key10 - TTabSheet not found");
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
					log.Verbose("Key10 - Third child not found 2");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key10 - Target control not found");
					return null;
				}

				return new KeyValuePair<string, string>(Key10Text, AutomationHelper.GetValue(element));
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
				if (!string.Equals(AutomationHelper.GetName(mainWindow), "Geraусo de Livro em Disco / Sintegra", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key11 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key11 - First child not found");
					return null;
				}

				if (!string.Equals(AutomationHelper.GetName(element), "pgeEmpresa", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key11 - Invalid control name");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key11 - TSelectPadrao not found");
					return null;
				}

				if (
					!string.Equals(AutomationHelper.GetProperty(element, AutomationElement.ClassNameProperty), "TSelectPadrao",
						StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key11 - Invalid control name 2");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key11 - Third child not found");
					return null;
				}

				element = element.GetChildByIndex(2);
				if (element == null)
				{
					log.Verbose("Key11 - Third child not found 2");
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
					!string.Equals(AutomationHelper.GetName(mainWindow), "SPED PIS/COFINS", StringComparison.OrdinalIgnoreCase))
				{
					log.Verbose("Key12 - Invalid title");
					return null;
				}

				var element = mainWindow.GetFirstChildByClassName("TTabSheet");
				if (element == null)
				{
					log.Verbose("Key12 - TTabSheet not found");
					return null;
				}

				element = element.GetFirstChildByClassName("TSelectPadrao");
				if (element == null)
				{
					log.Verbose("Key12 - TSelectPadrao not found");
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
					log.Verbose("Key10 - Third child not found 2");
					return null;
				}

				element = element.GetFirstChild();
				if (element == null)
				{
					log.Verbose("Key10 - Target control not found");
					return null;
				}

				return new KeyValuePair<string, string>(Key10Text, AutomationHelper.GetValue(element));
			}
			catch (Exception ex)
			{
				log.Verbose("Key12 failed", ex);
				return null;
			}
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (string.Equals(processName, "altpack_wfiscal_proc_sped.exe", StringComparison.OrdinalIgnoreCase))
			{
				var altElement = AutomationElement.FromHandle(hWnd);
				if (altElement == null) yield break;
				var res = CaptureKey10(altElement);
				if (res != null) yield return res.Value;
			}

			if (string.Equals(processName, "altpack_wfiscal_proc_sped_pis_cofins.exe", StringComparison.OrdinalIgnoreCase))
			{
				var altElement = AutomationElement.FromHandle(hWnd);
				if (altElement == null) yield break;
				var res = CaptureKey12(altElement);
				if (res != null) yield return res.Value;
			}

			if (!string.Equals(processName, "wfiscal.exe", StringComparison.OrdinalIgnoreCase)) yield break;

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

			/*result = CaptureKey6(element)
			if (result != null) yield return result.Value;

			result = CaptureKey7(element);
			if (result != null) yield return result.Value;*/

			result = CaptureKey8(element);
			if (result != null) yield return result.Value;

			result = CaptureKey9(element);
			if (result != null) yield return result.Value;

			result = CaptureKey11(element);
			if (result != null) yield return result.Value;

		}
	}
}
