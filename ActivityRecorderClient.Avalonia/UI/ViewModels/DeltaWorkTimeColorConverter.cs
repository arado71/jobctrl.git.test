using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public class DeltaWorkTimeColorConverter : IValueConverter
	{
		const string DeltaRed = "#D00909";
		const string DeltaGreen = "#25A389";

		static readonly SolidColorBrush RedBrush = new SolidColorBrush(Color.Parse(DeltaRed));
		static readonly SolidColorBrush GreenBrush = new SolidColorBrush(Color.Parse(DeltaGreen));

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is IConvertible c)
			{
				var ms = c.ToDouble(culture);
				return ms < 0
					? RedBrush
					: GreenBrush;
			}

			return Brushes.Black;
		}

		public object? ConvertBack(object? value, Type t, object? p, CultureInfo c)
			=> throw new NotSupportedException();
	}
}
