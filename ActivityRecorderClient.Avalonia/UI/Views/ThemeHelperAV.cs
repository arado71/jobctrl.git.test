using Avalonia.Controls;
using System;
using System.Diagnostics;
using ApplicationAV = Avalonia.Application;
//using System.Collections.Generic; // no longer needed as it looks
using ControlAV = Avalonia.Controls.Control;

namespace ActivityRecorderClientAV
{
	public static class ThemeHelperAV
	{
		public static void RegisterThemeChangeHandler(ControlAV control)
		{
			control.ActualThemeVariantChanged += (sender, e) =>
			{
				Debug.WriteLine(control.ToString() + " Theme Changed");
				ApplyThemeChanges(control);
			};
		}

		private static void ApplyThemeChanges(ControlAV control)
		{
			ApplyThemeBackground(control);
		}

		private static void ApplyThemeBackground(ControlAV control)
		{
			Debug.WriteLine(control.ToString() + " ApplyThemeBackground()");

			var currentTheme = ApplicationAV.Current?.ActualThemeVariant;

			if (currentTheme == Avalonia.Styling.ThemeVariant.Light)
			{
				control.Classes.Add("light");
				control.Classes.Remove("dark");
			}
			else
			{
				control.Classes.Add("dark");
				control.Classes.Remove("light");
			}
		}

    }
}