using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public class IsDeletedCardStyleConverter : IValueConverter
	{
		public static readonly IsDeletedCardStyleConverter Instance = new();

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is CardStyle cardStyle)
			{
				if (parameter != null && bool.TryParse(parameter.ToString(), out bool negate) && negate)
				{
					return cardStyle != CardStyle.Deleted;
				}
				return cardStyle == CardStyle.Deleted;
			}
			return false;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
