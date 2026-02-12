using System;
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
using MonitoringClient.ActivityMonitoringServiceReference;

namespace MonitoringClient.Converters
{
	public class StatusToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is OnlineStatus)
			{
				if ((OnlineStatus)value == OnlineStatus.Offline)
				{
					return new SolidColorBrush(Colors.Black);
				}
				else if (((OnlineStatus)value & OnlineStatus.OnlineComputer) != 0)
				{
					return new SolidColorBrush(Colors.Transparent);
				}
			}
			return new SolidColorBrush(Colors.White);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
