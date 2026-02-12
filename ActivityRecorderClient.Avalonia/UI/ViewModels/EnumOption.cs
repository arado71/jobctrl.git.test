using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Avalonia.UI.ViewModels
{
	public class EnumOption<T> where T : struct
	{
		public T Value { get; }
		public string Label { get; }

		public EnumOption(T value, string label = null)
		{
			Value = value;
			Label = label ?? value.ToString() ?? "(unknown)";
		}
	}
}
