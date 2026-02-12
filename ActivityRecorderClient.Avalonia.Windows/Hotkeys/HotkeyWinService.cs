using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;

namespace Tct.ActivityRecorderClient.Hotkeys
{
	//TODO: Thread-safety consideration.
	public class HotkeyWinService : IHotkeyService, IMessageFilter
	{
        public static HotkeyWinService Instance { get
			{
				if (_hotkeyWinService == null) _hotkeyWinService = new HotkeyWinService();
				return _hotkeyWinService;
			}
		}

		private static HotkeyWinService _hotkeyWinService;

        #region Interop

        private const uint WM_HOTKEY = 0x312;

		private const uint MOD_ALT = 0x1;
		private const uint MOD_CONTROL = 0x2;
		private const uint MOD_SHIFT = 0x4;
		private const uint MOD_WIN = 0x8;

		private const uint ERROR_HOTKEY_ALREADY_REGISTERED = 1409;

		#endregion

		private static int currentID;
		private const int maximumID = 0xBFFF;

		private readonly Dictionary<int, Hotkey> registeredHotkeys = new Dictionary<int, Hotkey>();

		public bool CanRegister(Hotkey hotkey)
		{
			try
			{
				Register(hotkey);
				Unregister(hotkey);
				return true;
			}
			catch (Win32Exception)
			{
				return false;
			}
			catch (NotSupportedException)
			{
				return false;
			}
		}

		public bool IsRegistered(Hotkey hotkey)
		{
			return registeredHotkeys.Count(kv => kv.Value.Equals(hotkey)) > 0;
		}

		public void Register(Hotkey hotkey)
		{
			// Check that we have not registered this hotkey
			if (registeredHotkeys.Count(kv => kv.Value.Equals(hotkey)) > 0) throw new NotSupportedException("You cannot register a hotkey that is already registered.");
			// We can't register an empty hotkey
			if ((Keys)hotkey.KeyCode == Keys.None) throw new NotSupportedException("You cannot register an empty hotkey.");

			// Get an ID for the hotkey and increase current ID
			int id = currentID;
			currentID = currentID + 1 % maximumID;
			Debug.Assert(registeredHotkeys.ContainsKey(id) == false);

			// Translate modifier keys into unmanaged version
			uint modifiers = (hotkey.Alt ? MOD_ALT : 0) | (hotkey.Control ? MOD_CONTROL : 0) | (hotkey.Shift ? MOD_SHIFT : 0) | (hotkey.Windows ? MOD_WIN : 0);

			// Register the hotkey
			if (WinApi.RegisterHotKey(IntPtr.Zero, id, modifiers, (Keys)hotkey.KeyCode) == 0)
			{
				throw new Win32Exception();
			}

			// Save register state of hotkey
			registeredHotkeys.Add(id, hotkey);
		}

		public void Unregister(Hotkey hotkey)
		{
			Unregister(hotkey, true);
		}

		private void Unregister(Hotkey hotkey, bool throwOnError)
		{
			// Check that we have registered this hotkey
			if (registeredHotkeys.Count(kv => kv.Value.Equals(hotkey)) == 0)
			{
				if (throwOnError) throw new NotSupportedException("You cannot unregister a hotkey that is not registered");
				return;
			}
			Debug.Assert(registeredHotkeys.Count(kv => kv.Value.Equals(hotkey)) == 1);

			int id = registeredHotkeys.First(kv => kv.Value.Equals(hotkey)).Key;

			// Unegister the hotkey
			if (WinApi.UnregisterHotKey(IntPtr.Zero, id) == 0)
			{
				if (throwOnError) throw new Win32Exception();
			}

			// Clear register state of hotkey
			registeredHotkeys.Remove(id);
		}

		public event EventHandler<SingleValueEventArgs<Hotkey>> HotkeyPressed;

		public void Dispose()
		{
			if (registeredHotkeys.Count == 0) return;
			registeredHotkeys.Values.ToList().ForEach(hk => Unregister(hk, false));
		}

		public bool PreFilterMessage(ref Message message)
		{
			return message.Msg == WM_HOTKEY && OnHotkeyPressed(message.WParam.ToInt32());
		}

		private bool OnHotkeyPressed(int id)
		{
			Hotkey hk = null;
			registeredHotkeys.TryGetValue(id, out hk);
			if (hk == null) return false;

			var del = HotkeyPressed;
			if (del != null) del(this, new SingleValueEventArgs<Hotkey>(hk));
			return true;
		}
	}
}
