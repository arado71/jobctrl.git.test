using System;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonitoringClient.Converters
{
	public class TimeSpanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is TimeSpan)
			{
				var timeSpan = ((TimeSpan)value);
				var prefix = "";
				if (timeSpan < TimeSpan.Zero)
				{
					prefix = "-";
					timeSpan = -timeSpan;
				}
				if ("HH:mm:ss".Equals(parameter))
				{
					return prefix + string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Days * 24 + timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
				}
				else
				{
					return prefix + string.Format("{0:D2}:{1:D2}", timeSpan.Days * 24 + timeSpan.Hours, timeSpan.Minutes);
				}
			}
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string strValue = value as string;
			//yagni
			return DependencyProperty.UnsetValue;
		}
	}
}
