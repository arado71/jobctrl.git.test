using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public class MsToTimeSpanConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is long msLong)
			{
				return TimeSpan.FromMilliseconds(msLong);
			}

			if (value is int msInt)
			{
				return TimeSpan.FromMilliseconds(msInt);
			}

			if (value is double msDouble)
			{
				return TimeSpan.FromMilliseconds(msDouble);
			}

			if (value is IConvertible conv)
			{
				return TimeSpan.FromMilliseconds(conv.ToDouble(culture));
			}

			return TimeSpan.Zero;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
