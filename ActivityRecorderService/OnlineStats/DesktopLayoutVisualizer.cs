using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Tct.ActivityRecorderService.OnlineStats
{
	public static class DesktopLayoutVisualizer
	{
		//we need thread-safe brushes (and cannot use Brushes due to custom colors) [Brushes and Pens also use Thread Local Storages]
		//probably its not wise to 'pollute' our threadpool threads with brushes and pens
		private static readonly ThreadLocal<Dictionary<Color, Brush>> localBrushDict = new ThreadLocal<Dictionary<Color, Brush>>(() => new Dictionary<Color, Brush>());

		public static List<ScreenShot> GetScreenShotsFromCapture(DesktopCapture desktopCapture, float scale = 1)
		{
			return desktopCapture.Screens
				.Where(n => n.Width > 0 && n.Height > 0) //we need valid screens (we have some invalid ones in some unit tests)
				.Select(screen => GetScreenShotForScreen(screen, desktopCapture.DesktopWindows, scale))
				.ToList();
		}

		private static ScreenShot GetScreenShotForScreen(Screen screen, List<DesktopWindow> desktopWindows, float scale)
		{
			var screenRect = new Rectangle(0, 0, (int)(screen.Width * scale), (int)(screen.Height * scale));
			var x = screen.X;
			var y = screen.Y;

			using (var screenCanvas = new Bitmap(screenRect.Width, screenRect.Height))
			using (var graphics = Graphics.FromImage(screenCanvas))
			{
				graphics.Clear(Color.White);

				if (desktopWindows != null)
					for (int i = desktopWindows.Count - 1; i >= 0; i--)
					{
						var window = desktopWindows[i];
						var windowRect = new Rectangle((int) ((window.X - x)*scale), (int) ((window.Y - y)*scale),
							(int) (window.Width*scale), (int) (window.Height*scale));
						if (!screenRect.IntersectsWith(windowRect)) continue;
						AddWindow(graphics, windowRect, window.IsActive, window.ProcessName, window.Title, window.Url);
					}

				using (var stream = new MemoryStream())
				{
					screenCanvas.Save(stream, ImageFormat.Png);
					return new ScreenShot()
					{
						Id = screen.Id,
						Extension = "png",
						CreateDate = screen.CreateDate,
						ScreenNumber = screen.ScreenNumber,
						ReceiveDate = DateTime.UtcNow,
						Data = stream.ToArray(),
					};
				}
			}
		}

		private const TextFormatFlags format = TextFormatFlags.Top | TextFormatFlags.Left | TextFormatFlags.WordBreak;
		private static readonly Font[] fonts = new Font[] { 
			new Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Pixel),
			new Font("Arial", 15, FontStyle.Bold, GraphicsUnit.Pixel),
			new Font("Arial", 20, FontStyle.Bold, GraphicsUnit.Pixel),
			new Font("Arial", 30, FontStyle.Bold, GraphicsUnit.Pixel),
			new Font("Arial", 40, FontStyle.Bold, GraphicsUnit.Pixel),
			new Font("Arial", 50, FontStyle.Bold, GraphicsUnit.Pixel),
		};

		private static Graphics AddWindow(Graphics graphics, Rectangle windowRect, bool isActive, string process, string title, string url)
		{
			if (windowRect.Width < 10 || windowRect.Height < 10) return graphics; //ignore small windows
			graphics.FillRectangle(GetBrushForProcessName(process), windowRect);
			graphics.DrawRectangle(isActive ? Pens.Red : Pens.Gray, windowRect.X, windowRect.Y, windowRect.Width - 1, windowRect.Height - 1);
			graphics.DrawRectangle(isActive ? Pens.Red : Pens.Gray, windowRect.X + 1, windowRect.Y + 1, windowRect.Width - 3, windowRect.Height - 3);
			var str = process + " - " + title + (string.IsNullOrEmpty(url) ? "" : Environment.NewLine + url);
			var winSize = new Size(windowRect.Width, windowRect.Height);
			var font = GetFontThatFits(str, winSize);
			TextRenderer.DrawText(graphics, str, font, windowRect, Color.Black, Color.Transparent, format);
			return graphics;
		}

		private static Font GetFontThatFits(string str, Size maxSize)
		{
			for (int i = fonts.Length - 1; i >= 1; i--)
			{
				var currFont = fonts[i];
				var size = TextRenderer.MeasureText(str, currFont, maxSize, format);
				if ((size.Width <= maxSize.Width && size.Height <= maxSize.Height)) return currFont;
			}
			return fonts[0];
		}

		private static readonly Color[] colors = new[] {
			Color.Lavender, Color.LavenderBlush, Color.PeachPuff, Color.LemonChiffon, Color.MistyRose, Color.Honeydew, Color.AliceBlue, Color.WhiteSmoke, Color.AntiqueWhite, Color.LightCyan, //Light
			Color.SkyBlue, Color.LimeGreen, Color.MediumOrchid, Color.LightCoral, Color.SteelBlue, Color.YellowGreen, Color.Turquoise, Color.HotPink, Color.Khaki, Color.Tan, Color.DarkSeaGreen, Color.CornflowerBlue, Color.Plum, Color.CadetBlue, Color.PeachPuff, Color.LightSalmon, //Pastel
			Color.SeaGreen, Color.DarkCyan, Color.MediumSeaGreen, Color.DarkSeaGreen, //Some from SeaGreen
			};
		private static Brush GetBrushForProcessName(string process)
		{
			var color = colors[Math.Abs((process.GetHashCode() + process.Length) % colors.Length)];
			Brush localBrush;
			if (!localBrushDict.Value.TryGetValue(color, out localBrush))
			{
				localBrush = new SolidBrush(color);
				localBrushDict.Value.Add(color, localBrush);
			}
			return localBrush;
		}

		//http://blogs.msdn.com/b/alexgor/archive/2009/10/06/setting-chart-series-colors.aspx
		//private static readonly Color[] _colorsDefault = new Color[] { Color.Green, Color.Blue, Color.Purple, Color.Lime, Color.Fuchsia, Color.Teal, Color.Yellow, Color.Gray, Color.Aqua, Color.Navy, Color.Maroon, Color.Red, Color.Olive, Color.Silver, Color.Tomato, Color.Moccasin };
		//private static readonly Color[] _colorsPastel = new Color[] { Color.SkyBlue, Color.LimeGreen, Color.MediumOrchid, Color.LightCoral, Color.SteelBlue, Color.YellowGreen, Color.Turquoise, Color.HotPink, Color.Khaki, Color.Tan, Color.DarkSeaGreen, Color.CornflowerBlue, Color.Plum, Color.CadetBlue, Color.PeachPuff, Color.LightSalmon };
		//private static readonly Color[] _colorsEarth = new Color[] { Color.FromArgb(255, 128, 0), Color.DarkGoldenrod, Color.FromArgb(192, 64, 0), Color.OliveDrab, Color.Peru, Color.FromArgb(192, 192, 0), Color.ForestGreen, Color.Chocolate, Color.Olive, Color.LightSeaGreen, Color.SandyBrown, Color.FromArgb(0, 192, 0), Color.DarkSeaGreen, Color.Firebrick, Color.SaddleBrown, Color.FromArgb(192, 0, 0) };
		//private static readonly Color[] _colorsSemiTransparent = new Color[] { Color.FromArgb(150, 255, 0, 0), Color.FromArgb(150, 0, 255, 0), Color.FromArgb(150, 0, 0, 255), Color.FromArgb(150, 255, 255, 0), Color.FromArgb(150, 0, 255, 255), Color.FromArgb(150, 255, 0, 255), Color.FromArgb(150, 170, 120, 20), Color.FromArgb(80, 255, 0, 0), Color.FromArgb(80, 0, 255, 0), Color.FromArgb(80, 0, 0, 255), Color.FromArgb(80, 255, 255, 0), Color.FromArgb(80, 0, 255, 255), Color.FromArgb(80, 255, 0, 255), Color.FromArgb(80, 170, 120, 20), Color.FromArgb(150, 100, 120, 50), Color.FromArgb(150, 40, 90, 150) };
		//private static readonly Color[] _colorsLight = new Color[] { Color.Lavender, Color.LavenderBlush, Color.PeachPuff, Color.LemonChiffon, Color.MistyRose, Color.Honeydew, Color.AliceBlue, Color.WhiteSmoke, Color.AntiqueWhite, Color.LightCyan };
		//private static readonly Color[] _colorsExcel = new Color[] { Color.FromArgb(153, 153, 255), Color.FromArgb(153, 51, 102), Color.FromArgb(255, 255, 204), Color.FromArgb(204, 255, 255), Color.FromArgb(102, 0, 102), Color.FromArgb(255, 128, 128), Color.FromArgb(0, 102, 204), Color.FromArgb(204, 204, 255), Color.FromArgb(0, 0, 128), Color.FromArgb(255, 0, 255), Color.FromArgb(255, 255, 0), Color.FromArgb(0, 255, 255), Color.FromArgb(128, 0, 128), Color.FromArgb(128, 0, 0), Color.FromArgb(0, 128, 128), Color.FromArgb(0, 0, 255) };
		//private static readonly Color[] _colorsBerry = new Color[] { Color.BlueViolet, Color.MediumOrchid, Color.RoyalBlue, Color.MediumVioletRed, Color.Blue, Color.BlueViolet, Color.Orchid, Color.MediumSlateBlue, Color.FromArgb(192, 0, 192), Color.MediumBlue, Color.Purple };
		//private static readonly Color[] _colorsChocolate = new Color[] { Color.Sienna, Color.Chocolate, Color.DarkRed, Color.Peru, Color.Brown, Color.SandyBrown, Color.SaddleBrown, Color.FromArgb(192, 64, 0), Color.Firebrick, Color.FromArgb(182, 92, 58) };
		//private static readonly Color[] _colorsFire = new Color[] { Color.Gold, Color.Red, Color.DeepPink, Color.Crimson, Color.DarkOrange, Color.Magenta, Color.Yellow, Color.OrangeRed, Color.MediumVioletRed, Color.FromArgb(221, 226, 33) };
		//private static readonly Color[] _colorsSeaGreen = new Color[] { Color.SeaGreen, Color.MediumAquamarine, Color.SteelBlue, Color.DarkCyan, Color.CadetBlue, Color.MediumSeaGreen, Color.MediumTurquoise, Color.LightSteelBlue, Color.DarkSeaGreen, Color.SkyBlue };
		//private static readonly Color[] _colorsBrightPastel = new Color[] { Color.FromArgb(65, 140, 240), Color.FromArgb(252, 180, 65), Color.FromArgb(224, 64, 10), Color.FromArgb(5, 100, 146), Color.FromArgb(191, 191, 191), Color.FromArgb(26, 59, 105), Color.FromArgb(255, 227, 130), Color.FromArgb(18, 156, 221), Color.FromArgb(202, 107, 75), Color.FromArgb(0, 92, 219), Color.FromArgb(243, 210, 136), Color.FromArgb(80, 99, 129), Color.FromArgb(241, 185, 168), Color.FromArgb(224, 131, 10), Color.FromArgb(120, 147, 190) };
	}
}
