using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using log4net;
using SuperWebSocket;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;
using Tct.ActivityRecorderClient.Common;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Edge
{
	public class EdgeProxy
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    public Dictionary<string, string> Capture(IntPtr hWnd, List<DomSettings> domCaptureSettings)
		{
			try
			{
				if (!IsApplicable(hWnd)) return null;
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
		private static bool IsApplicable(IntPtr hWnd)
		{
            string activeFirefoxHwnd = null; //ExecScript(client, "gethandle"); //I think native window handle can't be retrieved from chrome extension.
            if (activeFirefoxHwnd == null)
            {
                string url;
                //UrlHelper.TryGetUrlFromWindow(hWnd, Browser.Edge, out url);
                //if (String.IsNullOrEmpty(url)) return false;
	            if (!IsWin10WindowEdge(hWnd)) return false;
                var title = ExecScript(ExtensionCommand.GetActiveTabTitle);
                if (String.IsNullOrEmpty(title)) return false;
                //var win = EnumChildWindowsHelper.GetFirstChildWindowInfo(IntPtr.Zero, x => x.ClassName == "Chrome_WidgetWin_1" && x.Caption.Contains(title) && x.Caption.Contains("- Edge"));
                //return win != null && win.Handle == hWnd;
                return true;
            }
            else
			{
				var currentWindowHwnd = "0x" + hWnd.ToInt32().ToString("x2");
				return string.Equals(activeFirefoxHwnd, currentWindowHwnd, StringComparison.OrdinalIgnoreCase); //we only get data from one window
			}
		}
        
		private static Dictionary<string, string> CaptureApplicableSettings(string url, List<DomSettings> domCaptureSettings)
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

		private static bool TryGetDomElementProperty(string url, DomSettings settings, out string value)
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

		private static string GetDomElementProperty(DomSettings settings)
		{
			switch (settings.Type)
			{
				case DomSettings.CaptureType.Selector:
					return ExecScript(settings.Selector, settings.PropertyName);
				case DomSettings.CaptureType.Eval:
					return ExecScript(settings.EvalString);
				default:
					return null;
			}
		}

		internal static string ExecScript(ExtensionCommand command)
		{
			if (ConfigManager.CheckDiagnosticOperationMode(DiagnosticOperationMode.DisableDomCapture))
				return null;
			return ExecScript(new ChromeRequest() { ExtensionCommand = command.ToString() });
		}

		private static string ExecScript(string command)
		{
			return ExecScript(new ChromeRequest() { EvalString = command });
		}
		private static string ExecScript(string selector, string propertyName)
		{
			return ExecScript(new ChromeRequest() { Selector = selector, PropertyName = propertyName });
		}

		private static string ExecScript(ChromeRequest command)
		{
			var request = JsonHelper.SerializeData(command);
			string received;

			try
			{
			    received = EdgeCaptureClientWrapper.Send(request);
			}
			catch (Exception e)
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

	
		private static string GetClassName(IntPtr hWnd)
		{
			var classNameStringBuilder = new StringBuilder(256);
			var length = WinApi.GetClassName(hWnd, classNameStringBuilder, classNameStringBuilder.Capacity);
			return length == 0 ? null : classNameStringBuilder.ToString();
		}

		private static string GetCaptionOfWindow(IntPtr hwnd)
		{
			string caption = "";
			StringBuilder windowText = null;
			try
			{
				int max_length = WinApi.GetWindowTextLength(hwnd);
				windowText = new StringBuilder("", max_length + 5);
				WinApi.GetWindowText(hwnd, windowText, max_length + 2);

				if (!String.IsNullOrEmpty(windowText.ToString()) && !String.IsNullOrWhiteSpace(windowText.ToString()))
					caption = windowText.ToString();
			}
			catch (Exception ex)
			{
				caption = ex.Message;
			}
			finally
			{
				windowText = null;
			}
			return caption;
		} 
		private static bool IsWin10WindowEdge(IntPtr hWnd)
		{
			var foundChildren = false;
			uint processId;
			WinApi.GetWindowThreadProcessId(hWnd, out processId);

			WinApi.EnumChildWindows(hWnd, (childHWnd, lparam) =>
			{
				uint childProcessId;
				WinApi.GetWindowThreadProcessId(childHWnd, out childProcessId);
				if (processId != childProcessId)
				{
					var childClassName = GetClassName(childHWnd);
					var childCaption = GetCaptionOfWindow(childHWnd);
					if (childClassName.StartsWith("Spartan") || childClassName.StartsWith("Internet Explorer") || childCaption.StartsWith("Microsoft Edge"))
					{
						foundChildren = true;
						return true;
					}
					
				}
				return true;
			}, IntPtr.Zero);
			return foundChildren;
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
