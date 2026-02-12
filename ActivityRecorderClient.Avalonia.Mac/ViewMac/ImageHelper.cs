using AppKit;
using Avalonia.Media;
using CoreGraphics;
using SkiaSharp;
using SkiaTextRenderer;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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
					// TODO: mac
					iconImage.Draw(new CGRect(CGPoint.Empty, new CGSize(size.Width, size.Height)), new CGRect(CGPoint.Empty, new CGSize(iconImage.Size.Width, iconImage.Size.Height)), NSCompositingOperation.SourceOver, 1);
					//iconImage.DrawInRect(new RectangleF(PointF.Empty, scaledImage.Size), new RectangleF(PointF.Empty, iconImage.Size), NSCompositingOperation.SourceOver, 1);
				}
				finally
				{
					scaledImage.UnlockFocus();
				}
			}
			return scaledImage;
		}

		public static NSImage ConvertToNSImage(SKImage img)
		{
			using (var stream = new MemoryStream())
			using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
			{
				data.SaveTo(stream);
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
			var foreColor = barValue > 1 ? SKColors.Red : SKColors.Blue;
			var text = barText ?? barValue.ToString("0.0%");
			var barFont = new SkiaTextRenderer.Font(SKTypeface.Default, 8.0f, SkiaTextRenderer.FontStyle.Regular);
			//var barFont = new Font("Segoe UI", 8.0f, System.Drawing.FontStyle.Regular);
			return ImageHelper.GetProgressBarImage(foreColor, SKColors.White, SKColors.White, SKColors.Black, new Size(60, 15), barValue, text, barFont);
		}

		private const TextFormatFlags format = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;

		private static NSImage GetProgressBarImage(SKColor barForeColor, SKColor barBackColor, SKColor fontForeColor, SKColor fontBackColor, Size barSize, float barValue, string barText, SkiaTextRenderer.Font barFont)
		{
			var rect = new Rectangle(Point.Empty, barSize);
			if (rect.Width <= 0 || rect.Height <= 0)
				throw new ArgumentException("Invalid size");
			var rectLeft = new Rectangle(rect.X, rect.Y, (int)(rect.Width * Math.Min(1, Math.Max(0, barValue))), rect.Height);
			var rectRight = new Rectangle(rectLeft.Width, rect.Y, rect.Width - rectLeft.Width, rect.Height);

			var skRect = new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

			var imageInfo = new SKImageInfo(barSize.Width, barSize.Height);
			using var surface = SKSurface.Create(imageInfo);
			var canvas = surface.Canvas;

			if (rectLeft.Width > 0)
			{
				var leftRect = new SKRect(0, 0, rectLeft.Width, barSize.Height);
				using var paint = new SKPaint { Color = barForeColor };
				canvas.DrawRect(leftRect, paint);

				TextRendererSk.DrawText(canvas, barText, barFont, skRect, fontForeColor, format);
			}

			if (rectRight.Width > 0)
			{
				using var paint = new SKPaint { Color = barBackColor };

				using var surfaceRight = SKSurface.Create(imageInfo);
				var canvasRight = surfaceRight.Canvas;
				canvasRight.Clear(barBackColor);
				TextRendererSk.DrawText(canvasRight, barText, barFont, skRect, fontBackColor, format);
				surfaceRight.Flush();
				using var imageRight = surfaceRight.Snapshot(new SKRectI(rectRight.Left, rectRight.Top, rectRight.Right, rectRight.Bottom));
				canvas.DrawImage(imageRight, rectRight.Left, rectRight.Top);
			}

			if (rect.Width > 1 && rect.Height > 1)
			{
				using var borderPaint = new SKPaint() { Color = SKColors.Black, StrokeWidth = 1 };
				canvas.DrawRect(new SKRect(rect.X, rect.Y, rect.Width - 1, rect.Height - 1), borderPaint);
			}

			canvas.Flush();
			var result = surface.Snapshot();
			return ConvertToNSImage(result);
		}
	}
}

