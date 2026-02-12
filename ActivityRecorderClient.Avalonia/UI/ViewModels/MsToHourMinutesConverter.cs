using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public class MsToHourMinutesConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			TimeSpan? timeSpan = null;
			if (value is long msLong)
			{
				timeSpan = TimeSpan.FromMilliseconds(msLong);
			}

			if (value is IConvertible conv)
			{
				timeSpan = TimeSpan.FromMilliseconds(conv.ToDouble(culture));
			}


			if (timeSpan != null)
			{
				var ts = timeSpan.Value;
				if (ts < TimeSpan.Zero)
				{
					ts = new TimeSpan(-ts.Ticks);
				}
				return $"{ts.Hours + ts.Days * 24}:{ts.Minutes:00}";
			}


			return "";
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
