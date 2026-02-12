using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using log4net;

namespace Tct.ActivityRecorderClient.View
{
	/// <summary>
	/// Based on undocumented private interfaces ITrayNotify and INotificationCB
	/// described on http://thread0.me/2014/11/workaround-windows-tray-area-item-preference/
	/// These interfaces may be changed without any announcement
	/// </summary>
	public static class TrayStateChanger
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public static List<NOTIFYITEM> GetTrayItems()
		{
			try
			{
				var instance = new TrayNotify();
				try
				{
					if (useLegacyInterface())
					{
						return getTrayItemsWin7(instance);
					}
					else
					{
						return getTrayItems(instance);
					}
				}
				finally
				{
					Marshal.ReleaseComObject(instance);
				}
			}
			catch (Exception e)
			{
				log.Warn("GetTrayItems failed: " + e.Message);
				return null;
			}
		}

		public static void SetTrayItemPref(string exePath, bool isShow)
		{
			try
			{
				var instance = new TrayNotify();
				try
				{
					if (useLegacyInterface())
					{
						setTrayItemWin7(instance, exePath.ToLower(), isShow);
					}
					else
					{
						setTrayItem(instance, exePath.ToLower(), isShow);
					}
				}
				finally
				{
					Marshal.ReleaseComObject(instance);
				}
			}
			catch (Exception e)
			{
				log.Warn("SetTrayItemPref failed: " + e.Message);
			}
		}

		private static void setTrayItem(TrayNotify instance, string exePath, bool isShow)
		{
			var notifier = (ITrayNotify) instance;
			var callback = new NotificationCb();
			var handle = default(ulong);

			notifier.RegisterCallback(callback, out handle);
			notifier.UnregisterCallback(handle);
			NOTIFYITEM found = callback.items.FirstOrDefault(i => i.exe_name.ToLower() == exePath);
			if (found.hwnd != (IntPtr)0)
			{
				var pref = isShow
					? NOTIFYITEM_PREFERENCE.PREFERENCE_SHOW_ALWAYS
					: NOTIFYITEM_PREFERENCE.PREFERENCE_SHOW_WHEN_ACTIVE;
				if (pref == found.preference) return;
				found.preference = pref;
				notifier.SetPreference(found);
			}
		}

		private static void setTrayItemWin7(TrayNotify instance, string exePath, bool isShow)
		{
			var notifier = (ITrayNotifyWin7)instance;
			var callback = new NotificationCb();

			notifier.RegisterCallback(callback);
			notifier.RegisterCallback(null);
			NOTIFYITEM found = callback.items.FirstOrDefault(i => i.exe_name.ToLower() == exePath);
			if (found.hwnd != (IntPtr)0)
			{
				var pref = isShow
					? NOTIFYITEM_PREFERENCE.PREFERENCE_SHOW_ALWAYS
					: NOTIFYITEM_PREFERENCE.PREFERENCE_SHOW_WHEN_ACTIVE;
				if (pref == found.preference) return;
				found.preference = pref;
				notifier.SetPreference(found);
			}
		}

		static List<NOTIFYITEM> getTrayItems(TrayNotify instance)
		{
			var notifier = (ITrayNotify)instance;
			var callback = new NotificationCb();
			var handle = default(ulong);

			notifier.RegisterCallback(callback, out handle);
			notifier.UnregisterCallback(handle);
			return callback.items;
		}

		static List<NOTIFYITEM> getTrayItemsWin7(TrayNotify instance)
		{
			var notifier = (ITrayNotifyWin7)instance;
			var callback = new NotificationCb();

			notifier.RegisterCallback(callback);
			notifier.RegisterCallback(null);
			return callback.items;
		}

		class NotificationCb : INotificationCb
		{
			public readonly List<NOTIFYITEM> items = new List<NOTIFYITEM>();

			public void Notify([In] uint nEvent, [In] ref NOTIFYITEM notifyItem)
			{
				items.Add(notifyItem);
			}
		}

		static bool useLegacyInterface()
		{
			var ver = Environment.OSVersion.Version;
			if (ver.Major < 6) return true;
			if (ver.Major > 6) return false;

			// Windows 6.2 and higher use new interface
			return ver.Minor <= 1;
		}
	}

	// The known values for NOTIFYITEM's dwPreference member.
	public enum NOTIFYITEM_PREFERENCE
	{
		// In Windows UI: "Only show notifications."
		PREFERENCE_SHOW_WHEN_ACTIVE = 0,
		// In Windows UI: "Hide icon and notifications."
		PREFERENCE_SHOW_NEVER = 1,
		// In Windows UI: "Show icon and notifications."
		PREFERENCE_SHOW_ALWAYS = 2
	};

	// NOTIFYITEM describes an entry in Explorer's registry of status icons.
	// Explorer keeps entries around for a process even after it exits.
	public struct NOTIFYITEM
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		public string exe_name;    // The file name of the creating executable.

		[MarshalAs(UnmanagedType.LPWStr)]
		public string tip;         // The last hover-text value associated with this status
		// item.

		public IntPtr icon;       // The icon associated with this status item.
		public IntPtr hwnd;       // The HWND associated with the status item.
		public NOTIFYITEM_PREFERENCE preference;  // Determines the behavior of the icon with respect to
		// the taskbar
		public uint id;           // The ID specified by the application.  (hWnd, uID) is
		// unique.
		public Guid guid;         // The GUID specified by the application, alternative to
		// uID.
	};

	[ComImport]
	[Guid("D782CCBA-AFB0-43F1-94DB-FDA3779EACCB")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface INotificationCb
	{
		void Notify([In]uint nEvent, [In] ref NOTIFYITEM notifyItem);
	}

	[ComImport]
	[Guid("FB852B2C-6BAD-4605-9551-F15F87830935")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface ITrayNotifyWin7
	{
		void RegisterCallback([MarshalAs(UnmanagedType.Interface)]INotificationCb callback);
		void SetPreference([In] ref NOTIFYITEM notifyItem);
		void EnableAutoTray([In] bool enabled);
	}

	[ComImport]
	[Guid("D133CE13-3537-48BA-93A7-AFCD5D2053B4")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface ITrayNotify
	{
		void RegisterCallback([MarshalAs(UnmanagedType.Interface)]INotificationCb callback, [Out] out ulong handle);
		void UnregisterCallback([In] ref ulong handle);
		void SetPreference([In] ref NOTIFYITEM notifyItem);
		void EnableAutoTray([In] bool enabled);
		void DoAction([In] bool enabled);
	}

	[ComImport, Guid("25DEAD04-1EAC-4911-9E3A-AD0A4AB560FD")]
	class TrayNotify { }
}
