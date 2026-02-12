using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Common;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome
{
	public abstract class ChromiumProxyBase
	{
		private readonly ILog log;

		protected ChromiumProxyBase(ILog log)
		{
			this.log = log;
		}

		protected abstract string ExecScriptImpl(string request);

		public Dictionary<string, string> Capture(IntPtr hWnd, int processId, List<DomSettings> domCaptureSettings)
		{
			try
			{
				if (!IsApplicable(hWnd, processId)) return null;
				var url = ExecScript(ExtensionCommand.GetActiveTabUrl); //todo get from Chrome url resolver? from capture context to save cpu?
				return CaptureApplicableSettings(url, domCaptureSettings);
			}
			catch (Exception ex)
			{
				log.Debug("Capture failed", ex);
				return null;
			}
		}

		//http://stackoverflow.com/questions/18447621/how-to-get-window-handle-int-from-chrome-extension
		//http://stackoverflow.com/questions/11472708/get-window-handle-of-chrome-tab-from-within-extension
		private bool IsApplicable(IntPtr hWnd, int processId)
		{
			string activeFirefoxHwnd = null; //ExecScript(client, "gethandle"); //I think native window handle can't be retrieved from chrome extension.
			if (activeFirefoxHwnd == null)
			{
				var title = ExecScript(ExtensionCommand.GetActiveTabTitle);
				if (String.IsNullOrEmpty(title)) return false;
				var win = EnumChildWindowsHelper.GetFirstChildWindowInfo(IntPtr.Zero, x =>
				{
					WinApi.GetWindowThreadProcessId(x.Handle, out int childProcessId);
					return childProcessId == processId && x.ClassName == "Chrome_WidgetWin_1" && x.Caption.Contains(title);
				});
				return win != null && win.Handle == hWnd;
			}
			else
			{
				var currentWindowHwnd = "0x" + hWnd.ToInt32().ToString("x2");
				return string.Equals(activeFirefoxHwnd, currentWindowHwnd, StringComparison.OrdinalIgnoreCase); //we only get data from one window
			}
		}

		private Dictionary<string, string> CaptureApplicableSettings(string url, List<DomSettings> domCaptureSettings)
		{
			var result = new Dictionary<string, string>(domCaptureSettings.Count);
			foreach (var captureSetting in domCaptureSettings)
			{
				string value;
				if (TryGetDomElementProperty(url, captureSetting, out value))
				{
					result[captureSetting.Key] = value;
				}
			}
			return result;
		}

		private bool TryGetDomElementProperty(string url, DomSettings settings, out string value)
		{
			try
			{
				value = url != null && settings.UrlRegex.IsMatch(url) ? GetDomElementProperty(settings) : null;
				return value != null;
			}
			catch (DomCaptureException e)
			{
				log.Verbose("TryGetDomElementProperty failed. (" + settings + ")", e);
				value = null;
				return false;
			}
		}

		private string GetDomElementProperty(DomSettings settings)
		{
			switch (settings.Type)
			{
				case DomSettings.CaptureType.Selector:
					return ExecScript(settings.Selector, settings.PropertyName);
				case DomSettings.CaptureType.Eval:
					return ExecScript(settings.EvalString);
				case DomSettings.CaptureType.EveryTabEval:
					return ExecScript(settings.EveryTab.UrlPattern, settings.EveryTab.TitlePattern, settings.EveryTab.EvalString);
				default:
					return null;
			}
		}

		private string ExecScript(ExtensionCommand command)
		{
			if (ConfigManager.CheckDiagnosticOperationMode(DiagnosticOperationMode.DisableDomCapture))
				return null;
			return ExecScript(new ChromeRequest() { ExtensionCommand = command.ToString() });
		}

		private string ExecScript(string command)
		{
			return ExecScript(new ChromeRequest() { EvalString = command });
		}
		private string ExecScript(string selector, string propertyName)
		{
			return ExecScript(new ChromeRequest() { Selector = selector, PropertyName = propertyName });
		}

		private string ExecScript(string urlPattern, string titlePattern, string evalString)
		{
			return ExecScript(new ChromeRequest
			{
				EveryTab = new EveryTab
				{
					EvalString = evalString,
					TitlePattern = titlePattern,
					UrlPattern = urlPattern
				}
			});
		}

		private string ExecScript(ChromeRequest command)
		{
			var request = JsonHelper.SerializeData(command);
			string received;

			try
			{
				received = ExecScriptImpl(request);
			}
			catch (EndpointNotFoundException e)
			{
				log.Verbose("ExecScript failed.", e);
				return null;
			}

			ChromeRespone response;
			JsonHelper.DeserializeData(received, out response);
			if (response == null) return null;
			if (response.Error != null) throw new DomCaptureException(response.Error);
			return response.Result;
		}

		[DataContract]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		private class ChromeRequest
		{
			[DataMember(Name = "selector")]
			public string Selector { get; set; }

			[DataMember(Name = "propertyName")]
			public string PropertyName { get; set; }

			[DataMember(Name = "evalString")]
			public string EvalString { get; set; }

			[DataMember(Name = "extensionCommand")]
			public string ExtensionCommand { get; set; }

			[DataMember(Name = "everyTab")]
			public EveryTab EveryTab
			{
				get;
				set;
			}
		}

		[DataContract]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		private class EveryTab
		{
			[DataMember(Name = "url")]
			public string UrlPattern
			{
				get;
				set;
			}

			[DataMember(Name = "title")]
			public string TitlePattern
			{
				get;
				set;
			}

			[DataMember(Name = "eval")]
			public string EvalString
			{
				get;
				set;
			}
		}

		// ReSharper disable ClassNeverInstantiated.Local
		[DataContract]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		private class ChromeRespone
		{
			[DataMember(Name = "error")]
			public string Error { get; set; }

			[DataMember(Name = "result")]
			public string Result { get; set; }
		}
		// ReSharper restore ClassNeverInstantiated.Local

		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		internal enum ExtensionCommand
		{
			GetActiveTabTitle,
			GetActiveTabUrl,
		}
	}
}
