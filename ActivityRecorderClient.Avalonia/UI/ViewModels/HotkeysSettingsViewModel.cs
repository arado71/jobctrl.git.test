using ActivityRecorderClient.Avalonia.ViewModels;
using ActivityRecorderClientAV;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Hotkeys;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class HotkeysSettingsViewModel : ViewModelBase
	{
		public static IReadOnlyList<EnumOption<HotkeyActionType>> FilteredActions { get; } = new List<EnumOption<HotkeyActionType>>
		{
			new (HotkeyActionType.StartManualMeeting,"Offline worktime"),
		};
		public static IReadOnlyList<EnumOption<Keys>> FilteredKeys { get; } = new List<EnumOption<Keys>>
		{
			new (Keys.F1),
			new (Keys.F2),
			new (Keys.F3),
			new (Keys.F4),
			new (Keys.F5),
			new (Keys.F6),
			new (Keys.F7),
			new (Keys.F8),
			new (Keys.F9),
			new (Keys.F10),
			new (Keys.F11),
			new (Keys.F12),
		};


		public ObservableCollection<HotkeyEntryViewModel> Hotkeys { get; } = new();

		public HotkeysSettingsViewModel()
		{
			CancelChanges(); // Load hotkeys
		}

		[RelayCommand]
		public void AddHotkey()
		{
			var entry = new HotkeyEntryViewModel(RemoveHotkeyInternal);
			Hotkeys.Add(entry);
		}

		[RelayCommand]
		public void SaveHotkeysCommand()
		{
			var hotkeys = new List<HotkeySetting>();
			foreach (var hotkey in Hotkeys)
			{
				hotkeys.Add(MapToDto(hotkey));
			}
			App.MainWindow.HotkeyRegistrar?.SetHotkeys(hotkeys);
		}

		[RelayCommand]
		public void CancelChanges()
		{
			var hotkeys = App.MainWindow?.HotkeyRegistrar?.GetHotkeys();
			Hotkeys.Clear();
			foreach (var hotkey in hotkeys ?? [])
			{
				Hotkeys.Add(MapToViewModel(hotkey));
			}
		}

		private HotkeyEntryViewModel MapToViewModel(HotkeySetting hotkey)
		{
			return new HotkeyEntryViewModel(RemoveHotkeyInternal)
			{
				Action = FilteredActions.First(n => n.Value == hotkey.ActionType),
				Alt = hotkey.Alt,
				Ctrl = hotkey.Control,
				Key = FilteredKeys.First(n => n.Value == hotkey.KeyCode),
				Shift = hotkey.Shift,
				Win = hotkey.Windows
			};
		}

		private HotkeySetting MapToDto(HotkeyEntryViewModel hotkey)
		{
			return new HotkeySetting()
			{
				ActionType = hotkey.Action.Value,
				Alt = hotkey.Alt,
				Control = hotkey.Ctrl,
				Shift = hotkey.Shift,
				Windows = hotkey.Win,
				KeyCode = hotkey.Key.Value,
			};
		}

		private void RemoveHotkeyInternal(HotkeyEntryViewModel entry)
		{
			if (Hotkeys.Contains(entry))
				Hotkeys.Remove(entry);
		}
	}

	public class HotkeysSettingsDesignViewModel : HotkeysSettingsViewModel
	{
		public HotkeysSettingsDesignViewModel()
		{
			Hotkeys.Add(new HotkeyEntryViewModel { Key = new(Keys.F1), Shift = true, Ctrl = false, Alt = false, Win = false, Action = new(HotkeyActionType.StartManualMeeting), Task = "T1" });
			Hotkeys.Add(new HotkeyEntryViewModel { Key = new(Keys.F2), Shift = false, Ctrl = true, Alt = false, Win = false, Action = new(HotkeyActionType.StartManualMeeting), Task = "T2" });
			Hotkeys.Add(new HotkeyEntryViewModel { Key = new(Keys.F3), Shift = false, Ctrl = false, Alt = true, Win = false, Action = new(HotkeyActionType.StartManualMeeting), Task = "T3" });
		}
	}
}
