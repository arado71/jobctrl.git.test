using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ActivityRecorderClientAV
{
	public static class SvgIconRegistryAV
	{
		private static readonly Dictionary<string, SvgIconAV> Icons = LoadIcons();

		private static Dictionary<string, SvgIconAV> LoadIcons()
		{
			var iconType = typeof(SvgIconAV);
			var subclasses = Assembly.GetExecutingAssembly()
									 .GetTypes()
									 .Where(t => t.IsSubclassOf(iconType) && !t.IsAbstract);

			var icons = new Dictionary<string, SvgIconAV>();
			foreach (var subclass in subclasses)
			{
				var instance = Activator.CreateInstance(subclass) as SvgIconAV;
				if (instance != null)
				{
					icons[subclass.Name] = instance; // Key is the class name (e.g., "SvgHomeIcon")
				}
			}

			return icons;
		}

		public static SvgIconAV? GetIcon(string iconName) =>
			Icons.TryGetValue(iconName, out var icon) ? icon : null;

		public static IEnumerable<string> GetAllIconNames() => Icons.Keys;

	}
}