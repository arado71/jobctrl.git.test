using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JobCTRL.Plugins;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginKeszUreq : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const string PluginId = "Kesz.Ureq";
		private const string KeyRaiUtal = "RaiUtal";

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
			yield return KeyRaiUtal;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			if (string.Equals("UREQ.EXE", processName, StringComparison.OrdinalIgnoreCase))
			{
				return GetUreq(hWnd);
			}

			return null; //wrong process name
		}

		private Dictionary<string, string> GetUreq(IntPtr hWnd)
		{
			var handle = WinApi.FindWindowEx(hWnd, IntPtr.Zero, "MDIClient", null);
			if (handle == IntPtr.Zero) return null;
			handle = WinApi.FindWindowEx(handle, IntPtr.Zero, "TFMain225", "Forint átutalás");
			if (handle == IntPtr.Zero) return null;

			handle = WinApi.FindWindowEx(handle, IntPtr.Zero, "TPageControl", null);
			if (handle == IntPtr.Zero) return null;
			handle = WinApi.FindWindowEx(handle, IntPtr.Zero, "TTabSheet", "Tételek");
			if (handle == IntPtr.Zero) return null;

			var handleChild = WinApi.FindWindowEx(handle, IntPtr.Zero, "TPanel", null);
			if (handleChild == IntPtr.Zero) return null;
			handle = WinApi.FindWindowEx(handle, handleChild, "TPanel", null);
			if (handle == IntPtr.Zero) return null;

			handleChild = WinApi.FindWindowEx(handle, IntPtr.Zero, "TPanel", null);
			if (handleChild == IntPtr.Zero) return null;
			handle = WinApi.FindWindowEx(handle, handleChild, "TPanel", null);
			if (handle == IntPtr.Zero) return null;

			handle = WinApi.FindWindowEx(handle, IntPtr.Zero, "TGroupBox", null);
			if (handle == IntPtr.Zero) return null;
			handle = WinApi.FindWindowEx(handle, IntPtr.Zero, "TComboBox", null);
			if (handle == IntPtr.Zero) return null;

			var title = WindowTextHelper.GetWindowTextMsg(handle);
			return new Dictionary<string, string>(1)
			{
				{KeyRaiUtal, title}
			};
		}
	}
}
