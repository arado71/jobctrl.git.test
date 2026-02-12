using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MonitoringClient.ActivityMonitoringServiceReference;

namespace MonitoringClient.Converters
{
	public class ByteArrayToImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			byte[] byteBlob;
			Binary data = value as Binary;
			if (data != null)
			{
				byteBlob = data.Bytes;
			}
			else
			{
				byteBlob = value as byte[];
			}
			if (byteBlob == null || byteBlob.Length == 0)
			{
				return GetFallbackImage();
			}
			MemoryStream ms = new MemoryStream(byteBlob);
			BitmapImage bmi = new BitmapImage();
			bmi.SetSource(ms);
			return bmi;
		}

		private static BitmapSource GetFallbackImage()
		{
			var rect = new Rectangle() { Width = 320, Height = 256, Fill = new SolidColorBrush(Color.FromArgb(255, 0, 0, 50)), };
			var bmp = new WriteableBitmap(rect, null);
			return bmp;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

}
