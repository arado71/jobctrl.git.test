using System;
using System.Diagnostics;
using System.Reflection;
//using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform; // IsSystemInDarkMode()
using Avalonia.Svg.Skia;
using ApplicationAV = Avalonia.Application;
using ImageAV = Avalonia.Controls.Image;
using ControlAV = Avalonia.Controls.Control;
using FontFamilyAV = Avalonia.Media.FontFamily;

namespace ActivityRecorderClientAV
{
	public static class AppResourcesAV
	{
		public static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
		public static readonly string BaseIconPath = $"avares://{AssemblyName}/Assets/";
		public static readonly string BaseFontPath = $"avares://{AssemblyName}/fonts/";

		static AppResourcesAV()
		{
			InitializeFonts();
		}

		private static void InitializeFonts()
		{
			var fontPath = $"{BaseFontPath}#Inter Tight";

			if (IsFontValid(fontPath))
			{
				ApplicationAV.Current!.Resources["InterTightFont"] = new FontFamilyAV(fontPath);
				Debug.WriteLine("Inter Tight validated");
			}
			else
			{
				ApplicationAV.Current!.Resources["InterTightFont"] = new FontFamilyAV("Inter");
				Debug.WriteLine("Fallback to Inter");	// That's the embedded font, if that fails too it should load the system font
			}
		}

		private static bool IsFontValid(string fontPath)
		{
			try
			{
				var typeface = new Avalonia.Media.Typeface(fontPath);
				var glyphTypeface = typeface.GlyphTypeface; // Force load
				return glyphTypeface != null;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Font Invalid: {ex.Message}");
				return false;
			}
		}

		public static bool IsLightTheme()
		{
			var currentTheme = ApplicationAV.Current?.ActualThemeVariant;
			return currentTheme == Avalonia.Styling.ThemeVariant.Light;
		}

		public static bool IsSystemInDarkMode()
		{
			var platformSettings = ApplicationAV.Current?.PlatformSettings;
			var colorValues = platformSettings?.GetColorValues();
			var themeVariant = colorValues?.ThemeVariant;
			
			//Debug.WriteLine("PlatformSettings: " + platformSettings);
			//Debug.WriteLine("ColorValues: " + colorValues);
			Debug.WriteLine("ThemeVariant: " + themeVariant);

			//if (themeVariant == PlatformThemeVariant.Light) AvaloniaApp.ThemeSetting = 1;
			//else AvaloniaApp.ThemeSetting = 2;

			//return AvaloniaApp.ThemeSetting == 2;
			return themeVariant == PlatformThemeVariant.Dark;
		}

		public static void SetIconSource(ControlAV control, string iconName, string fileName)
		{
			var iconPath = $"{BaseIconPath}{fileName}";
			SvgSource? picture = null; // Initialize to null

			try
			{
				picture = SvgSource.Load(iconPath);
			}
			catch // (Exception ex)
			{
				Debug.WriteLine("Failed to load svg icon file: " + iconPath);
				// + Handling
			}

			try
			{
				if (picture != null)
				{
					var iconElement = control.FindControl<ImageAV>(iconName);
					if (iconElement != null)
					{
						iconElement.Source = new SvgImage
						{
							Source = picture
						};
					}
					else
					{
						Debug.WriteLine("Icon Element not found in axaml source: " + iconName);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error in method SetIconSource(): {ex.Message}");
				// + Handling
			}
		}

		public static void SetIcon(ControlAV control, string svgiconname, string iconname, bool lightmode)
		{
			SvgIconAV? icon;

			try
            {
				icon = SvgIconRegistryAV.GetIcon(svgiconname);
				if (icon != null)
				{
					var iconFileName = icon.GetFileName(lightmode);
					SetIconSource(control, iconname, iconFileName);
				}
				else
				{
					Debug.WriteLine("Icon not found: " + iconname);
				}
			}
			catch (Exception ex)
            {
				Debug.WriteLine($"Failed to load svg icon resource file: {ex.Message}");
                // +Handling it...
			}

			
		}

		public static void SetSystemThemeIcon(ControlAV control, bool lightmode)
		{
			SvgIconAV? themeIcon;
			SvgIconAV? systemThemeIcon;

			try
            {
				themeIcon = SvgIconRegistryAV.GetIcon("SvgThemeIcon");
				systemThemeIcon = SvgIconRegistryAV.GetIcon("SvgSystemThemeIcon");
				if (themeIcon != null && systemThemeIcon != null)
				{
					var themeIconFileName = App.IsSystemTheme ? systemThemeIcon.GetFileName(lightmode) : themeIcon.GetFileName(lightmode);
					SetIconSource(control, "ThemeIcon", themeIconFileName);
				}
				else
				{
					string missingIcons = "";
					if (themeIcon == null) missingIcons += "SvgThemeIcon ";
					if (systemThemeIcon == null) missingIcons += "SvgSystemThemeIcon";
					Debug.WriteLine("Icon(s) not found: " + missingIcons.Trim());
				}
			}
			catch (Exception ex)
            {
				Debug.WriteLine($"Failed to load svg theme icon resource file: {ex.Message}");
                // +Handling it...
			}
		}

    }
}