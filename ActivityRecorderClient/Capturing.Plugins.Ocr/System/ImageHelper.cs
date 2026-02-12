using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	public static class ImageHelper
	{
		public static InterpolationMode ToInterpolationMode(double val)
		{
			switch ((int)val)
			{
				case 0:
					return InterpolationMode.Bicubic;
				case 1:
					return InterpolationMode.Bilinear;
				case 2:
					return InterpolationMode.HighQualityBicubic;
				case 3:
					return InterpolationMode.HighQualityBilinear;
				case 4:
					return InterpolationMode.NearestNeighbor;
				default:
					Debug.Fail("Unknown value");
					return InterpolationMode.Default;
			}
		}

		public enum DesaturateMode
		{
			Luma = 9,
			Luminance = 8,
			Saturation = 3,
			Average = 7,
			Lightness = 4,
			MaxDecompose = 5,
			MinDecompose = 6,
			RedChannel = 2,
			GreenChannel = 1,
			BlueChannel = 0
		}

		public static byte GetBitsPerPixel(PixelFormat f)
		{
			switch (f)
			{
				case PixelFormat.Format16bppArgb1555:
					return 16;
				case PixelFormat.Format16bppGrayScale:
					return 16;
				case PixelFormat.Format16bppRgb555:
					return 16;
				case PixelFormat.Format16bppRgb565:
					return 16;
				case PixelFormat.Format1bppIndexed:
					return 1;
				case PixelFormat.Format24bppRgb:
					return 24;
				case PixelFormat.Format32bppArgb:
					return 32;
				case PixelFormat.Format32bppPArgb:
					return 32;
				case PixelFormat.Format32bppRgb:
					return 32;
				case PixelFormat.Format48bppRgb:
					return 48;
				case PixelFormat.Format4bppIndexed:
					return 4;
				case PixelFormat.Format64bppArgb:
					return 64;
				case PixelFormat.Format64bppPArgb:
					return 64;
				case PixelFormat.Format8bppIndexed:
					return 8;
				default:
					Debug.Fail("Unkown image format");
					return 8;
			}
		}
		private static unsafe byte Desaturate(byte* color, DesaturateMode channel)
		{
			switch (channel)
			{
				case DesaturateMode.RedChannel:
				case DesaturateMode.GreenChannel:
				case DesaturateMode.BlueChannel:
					return color[(int)channel];
				case DesaturateMode.MinDecompose:
					return Math.Min(color[0], Math.Min(color[1], color[2]));
				case DesaturateMode.MaxDecompose:
					return Math.Max(color[0], Math.Max(color[1], color[2]));
				case DesaturateMode.Average:
					return (byte)((color[0] + color[1] + color[2]) / 3);
				case DesaturateMode.Saturation:
					var min = Math.Min(color[0], Math.Min(color[1], color[2]));
					var max = Math.Max(color[0], Math.Max(color[1], color[2]));
					var delta = max - min;
					return max != 0 ? (byte)(255 - delta * 255 / max) : (byte)0;
				case DesaturateMode.Lightness:
					var mind = Math.Min(color[0], Math.Min(color[1], color[2]));
					var maxd = Math.Max(color[0], Math.Max(color[1], color[2]));
					return (byte)((maxd + mind) / 2);
				case DesaturateMode.Luminance:
					return (byte)(0.3 * color[2] + 0.59 * color[1] + 0.11 * color[0]);
				case DesaturateMode.Luma:
					return (byte)(0.2126 * color[2] + 0.7152 * color[1] + 0.0722 * color[0]);
			}

			Debug.Fail("Unknown desaturation mode");
			return 0;
		}

		public static unsafe void Treshold(this Bitmap image, byte limit, DesaturateMode channel = DesaturateMode.RedChannel)
		{
			var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
			var bpp = GetBitsPerPixel(image.PixelFormat);
			byte* scan0 = (byte*)data.Scan0.ToPointer();
			for (int i = 0; i < image.Height; i++)
			{
				for (int j = 0; j < image.Width; j++)
				{
					byte* colPtr = scan0 + i * data.Stride + j * bpp / 8;
					colPtr[0] = colPtr[1] = colPtr[2] = Desaturate(colPtr, channel) < limit ? (byte)0 : (byte)255;
				}
			}

			image.UnlockBits(data);
		}
		private static ImageAttributes GetBrightnessContrastAttributes(float brightness, float contrast)
		{
			var ptsArray = new[]
			{
				new [] {contrast, 0, 0, 0, 0}, 
				new [] {0, contrast, 0, 0, 0}, 
				new [] {0, 0, contrast, 0, 0}, 
				new [] {0, 0, 0, 1.0f, 0}, 
				new [] {brightness - 1.0f, brightness - 1.0f, brightness - 1.0f, 0, 1}, 
			};
			var imgAttrib = new ImageAttributes();
			imgAttrib.ClearColorMatrix();
			imgAttrib.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			imgAttrib.SetGamma(1.0f, ColorAdjustType.Bitmap);
			return imgAttrib;
		}
		public static Bitmap cropAtRect(this Bitmap b, Rectangle r)
		{
			Bitmap nb = new Bitmap(r.Width, r.Height);
			Graphics g = Graphics.FromImage(nb);
			g.DrawImage(b, -r.X, -r.Y);
			return nb;
		}
		public static Bitmap ScaledCopy(this Bitmap source, Rectangle partToScale, double scale, float brightness = 1.0f, float contrast = 1.0f, InterpolationMode interpolation = InterpolationMode.HighQualityBicubic)
		{

			var width = (int)(partToScale.Width * scale);
			var height = (int)(partToScale.Height * scale);
			var result = new Bitmap(width, height);
			using (Graphics g = Graphics.FromImage(result))
			{
				g.InterpolationMode = interpolation;
				g.DrawImage(source, new Rectangle(0, 0, width, height), partToScale.X, partToScale.Y, partToScale.Width, partToScale.Height, GraphicsUnit.Pixel, GetBrightnessContrastAttributes(brightness, contrast));
			}

			return result;
		}
	}
}
