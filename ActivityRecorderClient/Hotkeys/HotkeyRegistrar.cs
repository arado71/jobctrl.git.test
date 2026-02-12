using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Serialization;
using log4net;

namespace Tct.ActivityRecorderClient.Hotkeys
{
	public class HotkeyRegistrar : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static string HotkeySettingsPath { get { return "HotkeySettings-" + ConfigManager.UserId; } }

		private readonly object thisLock = new object();
		private List<HotkeySetting> hotkeySettings = new List<HotkeySetting>();
		private readonly IHotkeyService hotkeyService;

		public HotkeyRegistrar(IHotkeyService hotkeyService)
		{
			this.hotkeyService = hotkeyService;
			hotkeyService.HotkeyPressed += OnHotkeyPressed;
		}

		public void Dispose()
		{
			hotkeyService.HotkeyPressed -= OnHotkeyPressed;
			UnregisterHotkeys(hotkeySettings);
			hotkeyService.Dispose();
		}

		public void LoadSettings()
		{
			lock (thisLock)
			{
				if (!IsolatedStorageSerializationHelper.Exists(HotkeySettingsPath)) return;

				List<HotkeySetting> loadedHotkeySettings;
				if (IsolatedStorageSerializationHelper.Load(HotkeySettingsPath, out loadedHotkeySettings) && loadedHotkeySettings != null)
				{
					SetHotkeys(loadedHotkeySettings, false);
				}
			}
		}

		public bool SaveSettings()
		{
			lock (thisLock)
			{
				return IsolatedStorageSerializationHelper.Save(HotkeySettingsPath, hotkeySettings);
			}
		}

		public List<HotkeySetting> GetHotkeys()
		{
			lock (thisLock)
			{
				var result = new List<HotkeySetting>(hotkeySettings.Count);
				result.AddRange(hotkeySettings.Select(hks => hks.Clone()));
				return result;
			}
		}

		public void SetHotkeys(List<HotkeySetting> _hotkeySettings)
		{
			SetHotkeys(_hotkeySettings, true);
		}

		private void SetHotkeys(List<HotkeySetting> _hotkeySettings, bool save)
		{
			lock (thisLock)
			{
				log.Info("Loading " + _hotkeySettings.Count + " hotkey setting" + (_hotkeySettings.Count == 1 ? "" : "s"));

				UnregisterHotkeys(this.hotkeySettings);
				this.hotkeySettings = _hotkeySettings;
				RegisterHotkeys(_hotkeySettings);
				if (save) SaveSettings();
			}
		}

		private void RegisterHotkeys(List<HotkeySetting> _hotkeySettings)
		{
			foreach (var hks in _hotkeySettings)
			{
				try
				{
					var hk = new Hotkey() { KeyCode = hks.KeyCode, Control = hks.Control, Shift = hks.Shift, Alt = hks.Alt, Windows = hks.Windows };
					hotkeyService.Register(hk);
					log.InfoFormat("Hotkey setting registered: ({0})", hks.ToString());
				}
				catch (Exception e)
				{
					log.Error(String.Format("Hotkey registration failure! ({0})", hks.ToString()), e);
				}
			}
		}

		private void UnregisterHotkeys(List<HotkeySetting> _hotkeySettings)
		{
			foreach (var hks in _hotkeySettings)
			{
				try
				{
					var hk = new Hotkey() { KeyCode = hks.KeyCode, Control = hks.Control, Shift = hks.Shift, Alt = hks.Alt, Windows = hks.Windows };
					hotkeyService.Unregister(hk);
				}
				catch (Exception e)
				{
					log.Error(String.Format("Hotkey unregistration failure! ({0})", hks.ToString()), e);
				}
			}
		}

		public event EventHandler<SingleValueEventArgs<HotkeySetting>> HotkeyPressed;

		private void OnHotkeyPressed(object sender, SingleValueEventArgs<Hotkey> e)
		{
			lock (thisLock)
			{
				var hk = e.Value;
				var hks = hotkeySettings.FirstOrDefault(h => h.KeyCode == hk.KeyCode && h.Shift == hk.Shift && h.Control == hk.Control && h.Alt == hk.Alt && h.Windows == hk.Windows);
				var del = HotkeyPressed;
				if (del != null && hks != null) del(this, new SingleValueEventArgs<HotkeySetting>(hks));
			}
		}
	}
}
