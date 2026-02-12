using Avalonia.Controls;
using Avalonia.Input;
using System;
using Tct.ActivityRecorderClient.Avalonia.UI.ViewModels;


namespace ActivityRecorderClientAV
{
	public partial class PreferencesFormAV : Window
	{
		public PreferencesFormAV()
		{
			InitializeComponent();

			if (OperatingSystem.IsMacOS())
			{
				lblAlt.Text = "Opt";
				lblWin.Text = "Cmd";
			}

			DataContext = new SettingsViewModel();
		}

		private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
		{
			// Check if the left mouse button is pressed
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
			{
				BeginMoveDrag(e); // Initiates the drag operation
			}
		}


	}
}