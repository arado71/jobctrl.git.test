using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.InternetExplorer
{
	//this class can only be used from an STA thread
	public class IEController
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IntPtr handle;
		private readonly IWebBrowser2 browser;
		private IHTMLWindow2 window;
		private static readonly Type typeIWebBrowser2 = Type.GetTypeFromCLSID(typeof(IWebBrowser2).GUID);
		private static readonly Type typeIHTMLWindow2 = Type.GetTypeFromCLSID(typeof(IHTMLWindow2).GUID);
		private static readonly Type typeIHTMLDocument2 = Type.GetTypeFromCLSID(typeof(IHTMLDocument2).GUID);

		private const string getDomElementPropertyFunctionName = "__jc__getDomElementProperty";
		private const string getDomElementPropertyFunctionImpl =
			"function " + getDomElementPropertyFunctionName + @"(selector, propertyName) {
					try 
					{ 
						if (!document.querySelector) return null;
						var element = document.querySelector(selector);
						if (element) return element[propertyName];
						else return null;
					} 
					catch (ex) { return null; }
				}";

		private const string getDomElementPropertyWithEvalFunctionName = "__jc__getDomElementPropertyWithEval";
		private const string getDomElementPropertyWithEvalFunctionImpl =
			"function " + getDomElementPropertyWithEvalFunctionName + @"(evalString) {
					try { return eval(evalString); } 
					catch (ex) { return null; }
				}";

		private const string jsCodeToInject = getDomElementPropertyFunctionImpl + "\n\n" + getDomElementPropertyWithEvalFunctionImpl;

		public IEController(IntPtr hWnd)
		{
			DebugEx.EnsureSta();
			handle = hWnd;
			var doc2 = IEDOMFromhWnd(hWnd);
			if (doc2 == null) throw new InvalidOperationException("IEDOMFromhWnd failed");
			window =
				//doc2.parentWindow;
				//doc2.GetType() //good for more tabs
				//typeIHTMLDocument2 //good for more tabs
				//typeof(IHTMLDocument2) //not good for more tabs: hresult 0x800A01B6
			typeIHTMLDocument2.InvokeMember("parentWindow", BindingFlags.GetProperty, null, doc2, null) as IHTMLWindow2;
			browser = RetrieveIWebBrowser2FromIHtmlWindw2Instance(window);
			if (browser == null) throw new InvalidOperationException("RetrieveIWebBrowser2FromIHtmlWindw2Instance failed");
			IsValid = WinApi.IsWindow(handle);
			Debug.Assert(IsValid);
		}

		public bool IsBusy
		{
			get
			{
				DebugEx.EnsureSta();
				try
				{
					return IsValid //&& browser.Busy;
						//typeof(IWebBrowser2) //is faster than typeIWebBrowser2 but i'm not comfortable with it.... because typeof(XY) method doesn't work flawlessly for parentWindow
						&& (bool)typeIWebBrowser2.InvokeMember("Busy", BindingFlags.GetProperty, null, browser, null);
				}
				catch (COMException ex)
				{
					//handle if tab or ie is closed
					if (IsTabClosed(ex))
					{
						IsValid = false;
					}
					else
					{
						if (GetIsValidWithRefresh()) log.DebugAndFail("Valid but in error", ex); //not water-tight but good enough
					}
					return false;
				}
				catch (TargetInvocationException ex)
				{
					if (GetIsValidWithRefresh()) log.DebugAndFail("Valid but in error", ex); //not water-tight but good enough
					return false;
				}
			}
		}

		private string Url
		{
			get
			{
				try
				{
					return IsValid ? (string)typeIWebBrowser2.InvokeMember("LocationURL", BindingFlags.GetProperty, null, browser, null) : null;
				}
				catch //todo
				{
					return null;
				}
			}
		}

		public bool IsValid { get; private set; }

		public bool GetIsValidWithRefresh()
		{
			IsValid = WinApi.IsWindow(handle);
			return IsValid;
		}

		private static bool IsTabClosed(COMException ex)
		{
			return (ex.ErrorCode == -2147023179 //0x800706B5 //interface unknown
			   || ex.ErrorCode == -2147023174); //0x800706BA //rpc server unavailable
		}

		public bool TryGetDomElementProperty(DomSettings settings, out string value)
		{
			DebugEx.EnsureSta();
			var url = Url;
			value = url != null && settings.UrlRegex.IsMatch(url) ? GetDomElementProperty(settings, true) : null;
			return value != null;
		}

		private string GetDomElementProperty(DomSettings settings, bool retry)
		{
			try
			{
				if (!IsValid) return null;

				switch (settings.Type)
				{
					case DomSettings.CaptureType.Selector:
						return InvokeScript(getDomElementPropertyFunctionName, new object[] { settings.Selector, settings.PropertyName }) as string;
					case DomSettings.CaptureType.Eval:
						return InvokeScript(getDomElementPropertyWithEvalFunctionName, new object[] { settings.EvalString }) as string;
					default:
						return null;
				}
			}
			catch (Exception e)
			{
				var comException = e as COMException;
				if (comException != null)
				{
					if (IsTabClosed(comException))
					{
						IsValid = false;
					}
				}

				if (IsValid)
				{
					InjectScript(jsCodeToInject, "JScript");
					if (!IsKnownGetDomElementPropertyError(e))
					{
						log.DebugAndFail("Error while calling GetDomElementProperty", e);
					}
					if (retry)
					{
						return GetDomElementProperty(settings, false);
					}
				}
			}

			return null;
		}

		private static bool IsKnownGetDomElementPropertyError(Exception ex)
		{
			var comEx = ex as COMException;
			var targEx = ex as TargetInvocationException;
			if (comEx != null)
			{
				return (comEx.ErrorCode == -2147352570); //0x80020006 (DISP_E_UNKNOWNNAME)	//On page refresh/navigation we get this error, because our injected javascript code become unreachable.
			}
			if (targEx != null)
			{
				return
					(ex.InnerException is NotSupportedException) //HRESULT: 0x800A01B6 //This is a transient error.
					|| (ex.InnerException is COMException && ((COMException)ex.InnerException).ErrorCode == -2147352319) //HRESULT: 0x80020101 //This error occures while IE re-post confirmation popup is open.
					|| (ex.InnerException is UnauthorizedAccessException) //This error occurs after re-posting a form. (Because we injected our script when re-post confirmation popup was open.)
					;
			}
			return false;
		}

		private object InvokeScript(string name, object[] args)
		{
			if (window == null) return null;
			return typeIHTMLWindow2.InvokeMember(name, BindingFlags.InvokeMethod, null, window, args);
		}

		private void InjectScript(string script, string scriptLanguage)
		{
			try
			{
				if (!IsValid) return;
				var document = typeIWebBrowser2.InvokeMember("Document", BindingFlags.GetProperty, null, browser, null);
				window = typeIHTMLDocument2.InvokeMember("parentWindow", BindingFlags.GetProperty, null, document, null) as IHTMLWindow2;
				try
				{
					typeIHTMLWindow2.InvokeMember("execScript", BindingFlags.InvokeMethod, null, window, new object[] { script, scriptLanguage });
				}
				catch (COMException ex)
				{
					if ((uint)ex.ErrorCode == 0x80020006) //DISP_E_UNKNOWNNAME //http://msdn.microsoft.com/en-us/library/ie/ms536420%28v=vs.85%29.aspx //execScript is no longer supported. Starting with Internet Explorer 11, use eval.
					{
						typeIHTMLWindow2.InvokeMember("eval", BindingFlags.InvokeMethod, null, window, new object[] { script });
					}
					else
					{
						throw;
					}
				}
			}
			catch (Exception e)
			{
				var comException = e as COMException;
				if (comException != null)
				{
					if (IsTabClosed(comException))
					{
						IsValid = false;
					}
				}

				if (IsValid && !(e.InnerException is UnauthorizedAccessException)) // 0x80070005 (E_ACCESSDENIED) //Window is invalid because of a navigation to a new domain.
				{
					log.DebugAndFail("Error while calling InjectScript", e);
				}
			}
		}

		private static IHTMLDocument2 IEDOMFromhWnd(IntPtr hWnd)
		{
			// Register the message
			var lMsg = WinApi.RegisterWindowMessage(WinApi.WM_HTML_GETOBJECT);
			if (lMsg == 0) throw new Exception("Unable to register WM_HTML_GETOBJECT message", new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()));
			// Get the object
			var lRes = 0;
			WinApi.SendMessageTimeout(hWnd, lMsg, 0, 0, WinApi.SMTO_ABORTIFHUNG, 1000, ref lRes);
			if (lRes == 0) throw new Exception("Unable to get HTML object", new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()));

			// Get the object from lRes
			IHTMLDocument2 ieDOMFromhWnd = null;
			var IID_IHTMLDocument2 = typeof(IHTMLDocument).GUID;
			var hr = ObjectFromLresult(lRes, ref IID_IHTMLDocument2, 0, ref ieDOMFromhWnd);
			if (hr != 0) throw new COMException("ObjectFromLresult failed", hr);
			return ieDOMFromhWnd;
		}

		private static IWebBrowser2 RetrieveIWebBrowser2FromIHtmlWindw2Instance(IHTMLWindow2 ihtmlWindow2)
		{
			var serviceProvider = ihtmlWindow2 as IServiceProvider;
			if (serviceProvider == null) return null;

			object objIServiceProvider;
			var guidIServiceProvider = typeof(IServiceProvider).GUID;
			var guidTopLevelBrowser = SID_STopLevelBrowser;
			serviceProvider.QueryService(ref guidTopLevelBrowser, ref guidIServiceProvider, out objIServiceProvider);

			serviceProvider = objIServiceProvider as IServiceProvider;
			if (serviceProvider == null) return null;

			object objIWebBrowser;
			var guidIWebBrowser = typeof(IWebBrowser2).GUID;
			var guidWebBrowserApp = typeof(IWebBrowserApp).GUID;
			serviceProvider.QueryService(ref guidWebBrowserApp, ref guidIWebBrowser, out objIWebBrowser);
			var webBrowser = objIWebBrowser as IWebBrowser2;

			return webBrowser;
		}
		public static readonly Guid SID_STopLevelBrowser = new Guid(0x4C96BE40, 0x915C, 0x11CF, 0x99, 0xD3, 0x00, 0xAA, 0x00, 0x4A, 0xE8, 0x37);

		

		[ComImport, Guid("6d5140c1-7436-11ce-8034-00aa006009fa"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IServiceProvider
		{
			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			uint QueryService(
				ref Guid guidService,
				ref Guid riid,
				[MarshalAs(UnmanagedType.Interface)]out object ppvObject);
		}

		[Guid("D30C1661-CDAF-11D0-8A3E-00C04FC9E26E")]
		[TypeLibType(4176)]
		[ComImport]
		public interface IWebBrowser2 : IWebBrowserApp
		{
			[DispId(212)]
			bool Busy
			{
				[DispId(212), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				get;
			}
		}

		[TypeLibType(4160)]
		[Guid("332C4425-26CB-11D0-B483-00C04FD90119")]
		[ComImport]
		public interface IHTMLDocument2 //: IHTMLDocument
		{
			[DispId(1034)]
			IHTMLWindow2 parentWindow
			{
				[DispId(1034), MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				get;
			}
		}

		[Guid("626FC520-A41E-11CF-A731-00A0C9082637")]
		[TypeLibType(4160)]
		[ComImport]
		public interface IHTMLDocument
		{
		}

		[Guid("332C4427-26CB-11D0-B483-00C04FD90119")]
		[TypeLibType(4160)]
		[ComImport]
		public interface IHTMLWindow2 //: IHTMLFramesCollection2
		{
		}

		//[Guid("332C4426-26CB-11D0-B483-00C04FD90119")]
		//[TypeLibType(4160)]
		//[ComImport]
		//public interface IHTMLFramesCollection2
		//{
		//}

		[Guid("0002DF05-0000-0000-C000-000000000046")]
		[TypeLibType(4176)]
		[ComImport]
		public interface IWebBrowserApp //: IWebBrowser
		{
		}

		//[TypeLibType(4176)]
		//[Guid("EAB22AC1-30C1-11CF-A7EB-0000C05BAE0B")]
		//[ComImport]
		//public interface IWebBrowser
		//{
		//}
		[DllImport("oleacc", EntryPoint = "ObjectFromLresult", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern Int32 ObjectFromLresult(Int32 lResult, ref Guid riid, Int32 wParam, ref IEController.IHTMLDocument2 ppvObject);
	}

}
