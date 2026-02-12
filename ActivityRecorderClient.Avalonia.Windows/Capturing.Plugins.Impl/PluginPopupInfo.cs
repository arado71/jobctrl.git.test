using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JobCTRL.Plugins;
using log4net;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginPopupInfo : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const string PluginId = "JobCTRL.Popup";
		public const string KeyIsPopup = "IsPopup";
		public const string KeyIsOwned = "IsOwned";
		public const string KeyPopupInfo = "PopupInfo";

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
			yield return KeyIsPopup;
			yield return KeyIsOwned;
			yield return KeyPopupInfo;
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			try
			{
				var style = (long)GetWindowLong(hWnd, GWL_STYLE);
				var isPopup = (style & WS_POPUP) == WS_POPUP && ((style & WS_MINIMIZEBOX) == 0 || (style & WS_MAXIMIZEBOX) == 0);
				var ownerHwnd = WinApi.GetWindow(hWnd, GW_OWNER);
				var isOwned = ownerHwnd != IntPtr.Zero;

				var res = new Dictionary<string, string>(3)
				{
					{ KeyIsPopup, isPopup.ToString() },
					{ KeyIsOwned, isOwned.ToString() },
				};

				if (isPopup || isOwned)
				{
					var popupInfo = (isPopup ? KeyIsPopup : "");
					popupInfo += (string.IsNullOrEmpty(popupInfo) ? "" : " ") + (isOwned ? KeyIsOwned : "");
					res.Add(KeyPopupInfo, popupInfo);
				}

				return res;
			}
			catch (Exception e)
			{
				log.Verbose("Capture failed", e);
				return null;
			}
		}

		#region Native methods

		private const int GWL_STYLE = -16;
		private const uint GW_OWNER = 4;
		private const uint WS_POPUP = 0x80000000;
		private const uint WS_MINIMIZEBOX = 0x00020000;
		private const uint WS_MAXIMIZEBOX = 0x00010000;

		//http://stackoverflow.com/questions/3343724/how-do-i-pinvoke-to-getwindowlongptr-and-setwindowlongptr-on-32-bit-platforms
		private static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size == 4)
			{
				return WinApi.GetWindowLong32(hWnd, nIndex);
			}
			return WinApi.GetWindowLongPtr64(hWnd, nIndex);
		}
		#endregion
	}
}
