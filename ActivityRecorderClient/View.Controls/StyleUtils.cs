using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public enum FontStyle
	{
		Light = 0,
		Regular = 1,
		Bold = 2
	}

	public static class StyleUtils
	{
		public static readonly Color Background = Color.FromArgb(255, 255, 255);
		public static readonly Color BackgroundInactive = Color.FromArgb(230, 230, 230);
		public static readonly Color BackgroundHighlight = Color.FromArgb(208, 235, 248);
		public static readonly Color Negative = Color.FromArgb(255, 0, 0);
		public static readonly Color Positive = Color.FromArgb(34, 163, 119);
		public static readonly Color Foreground = Color.FromArgb(77, 77, 77);
		public static readonly Color ForegroundLight = Color.FromArgb(179, 179, 179);
		public static readonly Color ForegroundDark = Color.FromArgb(0, 0, 0);
		public static readonly Color ForegroundHighlight = Color.FromArgb(17, 156, 223);
		public static readonly Color JcColor = Color.FromArgb(255, 102, 0);
		public static readonly Color Shadow = Color.FromArgb(143, 143, 143);

		public static readonly ILog log = LogManager.GetLogger(typeof(StyleUtils));

		private static readonly PrivateFontCollection fontCollection = new PrivateFontCollection();
		private static readonly FontFamily light;
		private static readonly FontFamily bold;
		private static readonly FontFamily regular;

		static StyleUtils()
		{
			AddFont("Tct.ActivityRecorderClient.Resources.Opensans_Light.TTF");
			AddFont("Tct.ActivityRecorderClient.Resources.Opensans_Regular.TTF");
			AddFont("Tct.ActivityRecorderClient.Resources.Opensans_Bold.TTF");
			if (fontCollection.Families.Length > 1)
			{
				light = fontCollection.Families[1];
				bold = fontCollection.Families[0];
				regular = fontCollection.Families[0];
				log.Info("Fonts loaded from resource");
			}
			else
			{
				light = GetFontFamily("Open Sans Light", "Segoe UI", "Verdana", "Arial");
				bold = GetFontFamily("Open Sans", "Segoe", "Verdana", "Arial");
				regular = GetFontFamily("Open Sans", "Segoe", "Verdana", "Arial");
				log.Info("Fonts loaded from system");
			}
		}

		public static Font GetFont(FontStyle style, float size)
		{
			switch (style)
			{
				case FontStyle.Bold:
					return new Font(bold, size, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
				case FontStyle.Light:
					return new Font(light, size, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
				default:
					return new Font(regular, size, System.Drawing.FontStyle.Regular, GraphicsUnit.Point);
			}
		}

		private static void AddFont(string resource)
		{
			using (Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
			{
				if (fontStream == null) return;
				IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);
				var fontdata = new byte[fontStream.Length];
				fontStream.Read(fontdata, 0, (int)fontStream.Length);
				Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);
				fontCollection.AddMemoryFont(data, (int)fontStream.Length);
				//Marshal.FreeCoTaskMem(data); //http://stackoverflow.com/questions/11173457/using-a-method-to-get-embedded-font-causes-protected-memory-error/11180350#11180350
				//we have some AccessViolationExceptions probably due to freeing this memory, so don't free it
			}
		}

		private static FontFamily GetFontFamily(params string[] families)
		{
			if (!families.Any())
			{
				return null;
			}

			try
			{
				return new FontFamily(families.First());
			}
			catch (ArgumentException)
			{
				return GetFontFamily(families.Skip(1).ToArray());
			}
		}

		public static Color GetColor(int id)
		{
			return ColorPalette[Math.Abs(id % ColorPalette.Length)];
		}

		public static readonly Color[] ColorPalette =
             {
	             Color.FromArgb(163, 59, 76),
	             Color.FromArgb(234, 85, 109),
	             Color.FromArgb(237, 110, 131),
	             Color.FromArgb(240, 136, 153),
	             Color.FromArgb(161, 79, 80),
	             Color.FromArgb(197, 96, 98),
	             Color.FromArgb(231, 113, 115),
	             Color.FromArgb(235, 134, 136),
	             Color.FromArgb(238, 156, 157),
	             Color.FromArgb(154, 104, 84),
	             Color.FromArgb(187, 127, 103),
	             Color.FromArgb(220, 149, 121),
	             Color.FromArgb(225, 165, 141),
	             Color.FromArgb(231, 181, 161),
	             Color.FromArgb(163, 108, 73),
	             Color.FromArgb(234, 155, 105),
	             Color.FromArgb(237, 170, 127),
	             Color.FromArgb(240, 185, 150),
	             Color.FromArgb(170, 109, 53),
	             Color.FromArgb(208, 133, 65),
	             Color.FromArgb(244, 156, 76),
	             Color.FromArgb(246, 171, 103),
	             Color.FromArgb(247, 186, 130),
	             Color.FromArgb(172, 119, 42),
	             Color.FromArgb(210, 145, 51),
	             Color.FromArgb(247, 170, 60),
	             Color.FromArgb(248, 183, 89),
	             Color.FromArgb(249, 196, 119),
	             Color.FromArgb(176, 134, 24),
	             Color.FromArgb(214, 163, 30),
	             Color.FromArgb(252, 192, 35),
	             Color.FromArgb(253, 211, 101),
	             Color.FromArgb(161, 141, 33),
	             Color.FromArgb(196, 172, 40),
	             Color.FromArgb(230, 202, 47),
	             Color.FromArgb(234, 210, 78),
	             Color.FromArgb(238, 218, 110),
	             Color.FromArgb(135, 147, 31),
	             Color.FromArgb(164, 179, 37),
	             Color.FromArgb(193, 210, 44),
	             Color.FromArgb(202, 217, 75),
	             Color.FromArgb(212, 224, 108),
	             Color.FromArgb(114, 137, 49),
	             Color.FromArgb(139, 167, 60),
	             Color.FromArgb(163, 196, 70),
	             Color.FromArgb(191, 214, 126),
	             Color.FromArgb(112, 163, 98),
	             Color.FromArgb(132, 192, 115),
	             Color.FromArgb(150, 201, 136),
	             Color.FromArgb(169, 211, 157),
	             Color.FromArgb(60, 124, 80),
	             Color.FromArgb(73, 151, 98),
	             Color.FromArgb(86, 178, 115),
	             Color.FromArgb(111, 189, 136),
	             Color.FromArgb(137, 201, 157),
	             Color.FromArgb(12, 110, 80),
	             Color.FromArgb(14, 134, 97),
	             Color.FromArgb(17, 158, 114),
	             Color.FromArgb(52, 172, 135),
	             Color.FromArgb(89, 187, 157),
	             Color.FromArgb(15, 113, 101),
	             Color.FromArgb(19, 138, 123),
	             Color.FromArgb(22, 162, 144),
	             Color.FromArgb(57, 176, 161),
	             Color.FromArgb(92, 190, 178),
	             Color.FromArgb(20, 118, 133),
	             Color.FromArgb(25, 144, 162),
	             Color.FromArgb(29, 169, 190),
	             Color.FromArgb(37, 110, 134),
	             Color.FromArgb(45, 134, 163),
	             Color.FromArgb(53, 158, 192),
	             Color.FromArgb(83, 172, 201),
	             Color.FromArgb(114, 187, 211),
	             Color.FromArgb(71, 108, 145),
	             Color.FromArgb(87, 132, 177),
	             Color.FromArgb(102, 155, 208),
	             Color.FromArgb(125, 170, 215),
	             Color.FromArgb(148, 185, 222),
	             Color.FromArgb(77, 101, 138),
	             Color.FromArgb(110, 145, 197),
	             Color.FromArgb(132, 161, 206),
	             Color.FromArgb(154, 178, 215),
	             Color.FromArgb(84, 90, 126),
	             Color.FromArgb(103, 110, 154),
	             Color.FromArgb(121, 129, 181),
	             Color.FromArgb(141, 148, 192),
	             Color.FromArgb(161, 167, 203),
	             Color.FromArgb(112, 103, 150),
	             Color.FromArgb(132, 121, 176),
	             Color.FromArgb(150, 141, 188),
	             Color.FromArgb(169, 161, 200),
	             Color.FromArgb(103, 77, 117),
	             Color.FromArgb(126, 94, 143),
	             Color.FromArgb(148, 110, 168),
	             Color.FromArgb(164, 132, 181),
	             Color.FromArgb(180, 154, 194),
	             Color.FromArgb(113, 75, 112),
	             Color.FromArgb(138, 91, 136),
	             Color.FromArgb(162, 107, 160),
	             Color.FromArgb(176, 129, 174),
	             Color.FromArgb(190, 152, 189),
	             Color.FromArgb(128, 71, 103),
	             Color.FromArgb(156, 87, 126),
	             Color.FromArgb(183, 102, 148),
	             Color.FromArgb(194, 125, 164),
	             Color.FromArgb(205, 148, 180),
	             Color.FromArgb(142, 66, 92),
	             Color.FromArgb(173, 81, 112),
	             Color.FromArgb(211, 119, 150),
	             Color.FromArgb(219, 143, 169),
             };

		public static readonly Color[] SpareColors =
             {
	             Color.FromArgb(199, 72, 93),
	             Color.FromArgb(199, 132, 89),
	             Color.FromArgb(252, 201, 68),
	             Color.FromArgb(177, 205, 98),
	             Color.FromArgb(92, 134, 80),
	             Color.FromArgb(63, 182, 200),
	             Color.FromArgb(97, 195, 210),
	             Color.FromArgb(94, 123, 168),
	             Color.FromArgb(92, 84, 123),
	             Color.FromArgb(203, 95, 132),
             };
	}
}