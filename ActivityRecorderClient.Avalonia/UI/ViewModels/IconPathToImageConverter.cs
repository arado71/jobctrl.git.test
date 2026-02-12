using ActivityRecorderClientAV;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public class IconPathToImageConverter : IValueConverter, IMultiValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return ProcessConversion(value as string ?? parameter as string, value as ThemeVariant);
		}

		public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
		{
			var path = values.Count > 0 ? values[0] as string : null;
			var theme = values.Count > 1 ? values[1] as ThemeVariant : null;

			return ProcessConversion(path, theme);
		}

		private object? ProcessConversion(string? path, ThemeVariant? theme)
		{
			if (string.IsNullOrEmpty(path)) return null;

			if (!path.Contains("://"))
			{
				path = AppResourcesAV.BaseIconPath + path;
			}
			theme ??= App.Current?.ActualThemeVariant;
			var isDark = theme == ThemeVariant.Dark;

			if (isDark)
			{
				path = InsertDarkSuffix(path);
			}

			try
			{
				return new SvgImage
				{
					Source = SvgSource.Load(path)
				};
			}
			catch
			{
				return null; // Designer-safe
			}

		}

		private static string InsertDarkSuffix(string path)
		{
			var ext = Path.GetExtension(path);
			return path.Replace(ext, $"_dark{ext}");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotSupportedException();
	}

	public class Icon : MarkupExtension
	{
		[Content]
		public IBinding PathBinding { get; set; }

		public Icon() { }

		public Icon(IBinding pathBinding)
		{
			PathBinding = pathBinding;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (PathBinding == null) return null;

			var multiBinding = new MultiBinding
			{
				Converter = new IconPathToImageConverter()
			};

			// 1. The path from your VM or Source
			multiBinding.Bindings.Add(PathBinding);

			// 2. The theme trigger
			multiBinding.Bindings.Add(new Binding("ActualThemeVariant")
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.Self)
			});

			return multiBinding;
		}
	}
}
