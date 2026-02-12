using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public sealed class DeltaMsConverter : IMultiValueConverter
	{
		public object? Convert(
			IList<object?> values,
			Type targetType,
			object? parameter,
			CultureInfo culture)
		{
			if (values.Count < 2)
				return 0L;

			if (values[0] is long a && values[1] is long b)
			{
				return a - b;
			}

			return 0L;
		}
	}
}
