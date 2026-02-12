using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public class StringToBrushConverter : IValueConverter
	{
		public static readonly StringToBrushConverter Instance = new();

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is null)
			{
				return AvaloniaProperty.UnsetValue;
			}

			if (value is Brush b)
			{
				return b;
			}

			if (value is string s && !string.IsNullOrWhiteSpace(s))
			{
				try
				{
					return Brush.Parse(s);
				}
				catch
				{
					return AvaloniaProperty.UnsetValue;
				}
			}

			return AvaloniaProperty.UnsetValue;
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is Brush b)
				return b.ToString() ?? "";

			return AvaloniaProperty.UnsetValue;
		}
	}
}
