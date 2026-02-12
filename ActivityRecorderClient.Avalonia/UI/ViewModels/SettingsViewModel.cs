using ActivityRecorderClient.Avalonia.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public partial class SettingsViewModel : ViewModelBase
	{
		[ObservableProperty]
		public HotkeysSettingsViewModel hotkeysSettings = new ();
	}
}
