using log4net;
using System.Runtime.InteropServices;
using Tct.ActivityRecorderClient.Forms;

namespace Tct.ActivityRecorderClient.Hotkeys
{
	public class HotkeyMacService : IHotkeyService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static HotkeyMacService Instance
		{
			get
			{
				if (_hotkeyMacService == null) _hotkeyMacService = new HotkeyMacService();
				return _hotkeyMacService;
			}
		}

		private static HotkeyMacService? _hotkeyMacService;

		private readonly HashSet<Hotkey> registeredHotkeys = new();

		private NSObject evtMonitorGlobal;
		private NSObject evtMonitorLocal;

		private HotkeyMacService()
		{
			if (AXIsProcessTrusted())
			{
				log.Info("Registering hotkey service with accessibility enabled");
			}
			else
			{
				// TODO: mac, do something about it
				log.Warn("Cannot register hotkey service with accessibility disabled");
			}
			evtMonitorGlobal = NSEvent.AddGlobalMonitorForEventsMatchingMask(NSEventMask.KeyDown, HookEvent);
			evtMonitorLocal = NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyDown, LocalHookEvent);
		}

		private NSEvent LocalHookEvent(NSEvent evt)
		{
			HookEvent(evt);
			return evt;
		}

		private void HookEvent(NSEvent evt)
		{
			var hotkey = MacKeyMapper.FromNSEvent(evt);
			if (hotkey.KeyCode != Keys.None && registeredHotkeys.Contains(hotkey))
			{
				OnHotkeyPressed(hotkey);
			}
		}

		public bool CanRegister(Hotkey hotkey)
		{
			return true;
		}

		public bool IsRegistered(Hotkey hotkey)
		{
			return registeredHotkeys.Contains(hotkey);
		}

		public void Register(Hotkey hotkey)
		{
			// Check that we have not registered this hotkey
			if (IsRegistered(hotkey)) throw new NotSupportedException("You cannot register a hotkey that is already registered.");
			// We can't register an empty hotkey
			if (hotkey.KeyCode == Keys.None) throw new NotSupportedException("You cannot register an empty hotkey.");

			registeredHotkeys.Add(hotkey);
		}

		public void Unregister(Hotkey hotkey)
		{
			Unregister(hotkey, true);
		}

		private void Unregister(Hotkey hotkey, bool throwOnError)
		{
			// Check that we have registered this hotkey
			if (!IsRegistered(hotkey))
			{
				if (throwOnError) throw new NotSupportedException("You cannot unregister a hotkey that is not registered");
				return;
			}
			registeredHotkeys.Remove(hotkey);
		}

		public event EventHandler<SingleValueEventArgs<Hotkey>>? HotkeyPressed;

		private bool isDisposed;
		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			NSEvent.RemoveMonitor(evtMonitorGlobal);
			evtMonitorGlobal.Dispose();
			NSEvent.RemoveMonitor(evtMonitorLocal);
			evtMonitorLocal.Dispose();
		}

		private bool OnHotkeyPressed(Hotkey hotkey)
		{
			if (!IsRegistered(hotkey)) return false;
			HotkeyPressed?.Invoke(this, new SingleValueEventArgs<Hotkey>(hotkey));
			return true;
		}


		[DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static extern bool AXIsProcessTrusted();
	}
}
