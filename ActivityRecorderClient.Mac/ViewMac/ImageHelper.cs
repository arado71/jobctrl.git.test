using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public static class ImageHelper
	{
		public static NSImage GetScaledImage(string imageName, SizeF size)
		{
			var scaledImage = new NSImage(size);
			using (var iconImage = NSImage.ImageNamed(imageName))
			{
				scaledImage.LockFocus();
				try
				{
					iconImage.DrawInRect(new RectangleF(PointF.Empty, scaledImage.Size), new RectangleF(PointF.Empty, iconImage.Size), NSCompositingOperation.SourceOver, 1);
				}
				finally
				{
					scaledImage.UnlockFocus();
				}
			}
			return scaledImage;
		}

		public static NSImage ConvertToNSImage(Image img)
		{
			using (var stream = new MemoryStream())
			{
				img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				stream.Position = 0;
				return NSImage.FromStream(stream);
			}
		}

		public static NSImage GetProgressBarImage(float? barValue, string barText = null)
		{
			if (!barValue.HasValue)
			{
				return new NSImage(new SizeF(0, 0));
			}
			return GetProgressBarImage(barValue.Value, barText);
		}

		public static NSImage GetProgressBarImage(float barValue, string barText = null)
		{
			var foreColor = barValue > 1 ? Color.Red : Color.Blue;
			var text = barText ?? barValue.ToString("0.0%");
			return ImageHelper.GetProgressBarImage(foreColor, Color.White, Color.White, Color.Black, new Size(60, 15), barValue, text, new Font("Segoe UI", 8.0f, FontStyle.Regular));
		}

		private const TextFormatFlags format = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;

		private static NSImage GetProgressBarImage(Color barForeColor, Color barBackColor, Color fontForeColor, Color fontBackColor, Size barSize, float barValue, string barText, Font barFont)
		{
			var rect = new Rectangle(Point.Empty, barSize);
			if (rect.Width <= 0 || rect.Height <= 0)
				throw new ArgumentException("Invalid size");
			var rectLeft = new Rectangle(rect.X, rect.Y, (int)(rect.Width * Math.Min(1, Math.Max(0, barValue))), rect.Height);
			var rectRight = new Rectangle(rectLeft.Width, rect.Y, rect.Width - rectLeft.Width, rect.Height);

			using (Image result = new Bitmap(rect.Width, rect.Height))
			using (var gResult = Graphics.FromImage(result))
			{
				if (rectLeft.Width > 0)
				{
					gResult.Clear(barForeColor);
					TextRenderer.DrawText(gResult, barText, barFont, rect, fontForeColor, barForeColor, format);
				}

				if (rectRight.Width > 0)
				{
					using (var bitmapRight = new Bitmap(rect.Width, rect.Height))
					using (var gRight = Graphics.FromImage(bitmapRight))
					{
						gRight.Clear(barBackColor);
						TextRenderer.DrawText(gRight, barText, barFont, rect, fontBackColor, barBackColor, format);

						using (var bmpR = bitmapRight.Clone(rectRight, bitmapRight.PixelFormat))
						{
							gResult.DrawImage(bmpR, rectRight);
						}
					}
				}

				if (rect.Width > 1 && rect.Height > 1)
				{
					gResult.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
				}
				return ConvertToNSImage(result);
			}
		}
	}
}

