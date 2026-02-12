using ActivityRecorderClient.Avalonia.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Hotkeys;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class HotkeyEntryViewModel : ViewModelBase
	{
		public IReadOnlyList<EnumOption<HotkeyActionType>> FilteredActions { get; set; } = HotkeysSettingsViewModel.FilteredActions;
		public IReadOnlyList<EnumOption<Keys>> FilteredKeys { get; set; } = HotkeysSettingsViewModel.FilteredKeys;

		public HotkeyEntryViewModel() : this(null) // design-time support
		{
		}

		public HotkeyEntryViewModel(Action<HotkeyEntryViewModel>? removeCallback = null)
		{
			RemoveSelfCommand = new RelayCommand(() =>
			{
				removeCallback?.Invoke(this);
			});
		}

		[ObservableProperty] private EnumOption<Keys> key = new (Keys.F1);
		[ObservableProperty] private bool shift;
		[ObservableProperty] private bool ctrl;
		[ObservableProperty] private bool alt;
		[ObservableProperty] private bool win;

		[ObservableProperty] private EnumOption<HotkeyActionType> action = new (HotkeyActionType.StartManualMeeting);
		[ObservableProperty] private string task = "";

		public IRelayCommand RemoveSelfCommand { get; }
	}
}
