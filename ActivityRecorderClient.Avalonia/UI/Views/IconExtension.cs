using ActivityRecorderClientAV;
using Avalonia.Markup.Xaml;
using Avalonia.Svg.Skia;
using System;

namespace Tct.ActivityRecorderClient.Avalonia.UI.Views
{
	public class IconExtension : MarkupExtension
	{
		public string Name { get; set; } = "";

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return SvgSource.Load(AppResourcesAV.BaseIconPath + Name);
		}
	}
}
