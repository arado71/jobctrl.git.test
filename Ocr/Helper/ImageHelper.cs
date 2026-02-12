using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Ocr.Helper
{
	public static class ImageHelper
	{
		private static readonly object thisLock = new object();

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

		private static byte Desaturate(byte R, byte G, byte B, DesaturateMode channel)
		{
			switch (channel)
			{
				case DesaturateMode.RedChannel:
					return R;
				case DesaturateMode.GreenChannel:
					return G;
				case DesaturateMode.BlueChannel:
					return B;
				case DesaturateMode.MinDecompose:
					return Math.Min(B, Math.Min(G, R));
				case DesaturateMode.MaxDecompose:
					return Math.Max(B, Math.Max(G, R));
				case DesaturateMode.Average:
					return (byte)((B + G + R) / 3);
				case DesaturateMode.Saturation:
					var min = Math.Min(B, Math.Min(G, R));
					var max = Math.Max(B, Math.Max(G, R));
					var delta = max - min;
					return max != 0 ? (byte)(255 - delta * 255 / max) : (byte)0;
				case DesaturateMode.Lightness:
					var mind = Math.Min(B, Math.Min(G, R));
					var maxd = Math.Max(B, Math.Max(G, R));
					return (byte)((maxd + mind) / 2);
				case DesaturateMode.Luminance:
					return (byte)(0.3 * R + 0.59 * G + 0.11 * B);
				case DesaturateMode.Luma:
					return (byte)(0.2126 * R + 0.7152 * G + 0.0722 * B);
			}
			Debug.Fail("Unknown desaturation mode");
			return 0;
		}

		public static void Treshold(this Bitmap image, byte limit, DesaturateMode channel = DesaturateMode.RedChannel)
		{
			var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
			var bpp = GetBitsPerPixel(image.PixelFormat);
			var ptr = data.Scan0;
			int offset = data.Stride - image.Width * bpp / 8;
			for (int y = 0; y <= image.Height - 1; y++)
			{
				for (int x = 0; x <= image.Width - 1; x++)
				{
					var B = Marshal.ReadByte(ptr, 0);
					var G = Marshal.ReadByte(ptr, 1);
					var R = Marshal.ReadByte(ptr, 2);
					var value = Desaturate(R, G, B, channel) < limit ? (byte)0 : (byte)255;
					Marshal.WriteByte(ptr, 0, value);
					Marshal.WriteByte(ptr, 1, value);
					Marshal.WriteByte(ptr, 2, value);
					ptr = (IntPtr)(ptr.ToInt64() + bpp/8);
				}
				ptr = (IntPtr)(ptr.ToInt64() + offset);
			}
			image.UnlockBits(data);
		}
		private static ImageAttributes GetBrightnessContrastAttributes(float brightness, float contrast)
		{
			var ptsArray = new[]
			{
				new[] { contrast, 0, 0, 0, 0 },
				new[] { 0, contrast, 0, 0, 0 },
				new[] { 0, 0, contrast, 0, 0 },
				new[] { 0, 0, 0, 1.0f, 0 },
				new[] { brightness - 1.0f, brightness - 1.0f, brightness - 1.0f, 0, 1 }
			};
			var imgAttrib = new ImageAttributes();
			imgAttrib.ClearColorMatrix();
			imgAttrib.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			imgAttrib.SetGamma(1.0f, ColorAdjustType.Bitmap);
			return imgAttrib;
		}

		public static Bitmap ScaledCopy(this Bitmap sourceImage, double scale, float brightness = 1.0f, float contrast = 1.0f,
			InterpolationMode interpolation = InterpolationMode.HighQualityBicubic)
		{
			return ScaledCopy(sourceImage, (int)(sourceImage.Width * scale), (int)(sourceImage.Height * scale), brightness,
				contrast, interpolation);
		}

		public static Bitmap ScaledCopy(this Bitmap sourceImage, int newWidth, int newHeight, float brightness = 1.0f,
			float contrast = 1.0f, InterpolationMode interpolation = InterpolationMode.HighQualityBicubic)
		{
			var result = new Bitmap(newWidth, newHeight);
			using (var g = Graphics.FromImage(result))
			{
				g.InterpolationMode = interpolation;
				g.DrawImage(sourceImage, new Rectangle(0, 0, newWidth, newHeight), 0, 0, sourceImage.Width, sourceImage.Height,
					GraphicsUnit.Pixel, GetBrightnessContrastAttributes(brightness, contrast));
			}

			return result;
		}

		public static Bitmap ScaledCopy(this Bitmap source, Rectangle partToScale, double scale, float brightness = 1.0f,
			float contrast = 1.0f, InterpolationMode interpolation = InterpolationMode.HighQualityBicubic)
		{
			lock (thisLock)
			{
				var width = (int)(partToScale.Width * scale);
				var height = (int)(partToScale.Height * scale);
				var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
				using (var g = Graphics.FromImage(result))
				{
					g.InterpolationMode = interpolation;
					g.DrawImage(source, new Rectangle(0, 0, width, height), partToScale.X, partToScale.Y, partToScale.Width,
						partToScale.Height, GraphicsUnit.Pixel, GetBrightnessContrastAttributes(brightness, contrast));
				}
				return result;
			}
		}

		public static unsafe Rectangle? GetBounds(Bitmap image, int limit, DesaturateMode? channel = null)
		{
			var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
			var bpp = GetBitsPerPixel(image.PixelFormat);
			var scan0 = (byte*)data.Scan0.ToPointer();
			var firstRow = -1;
			var lastRow = -1;
			var firstCol = -1;
			var lastCol = -1;
			for (var i = 0; i < image.Height; i++)
			{
				var firstOccurence = -1;
				var lastOccurence = -1;
				for (var j = 0; j < image.Width; j++)
				{
					var colPtr = scan0 + i * data.Stride + j * bpp / 8;
					var color = channel != null ? colPtr[(int)channel] : Math.Min(colPtr[0], Math.Min(colPtr[1], colPtr[2]));
					if (color < limit)
					{
						lastOccurence = j;
						if (firstOccurence == -1)
							firstOccurence = j;
					}
				}
				if (firstOccurence != -1)
				{
					lastRow = i;
					if (firstRow == -1)
						firstRow = i;
					if (firstCol == -1 || firstCol > firstOccurence)
						firstCol = firstOccurence;

					if (lastCol == -1 || lastCol < lastOccurence)
						lastCol = lastOccurence;
				}
			}

			Debug.Assert(!((firstRow == -1) ^ (firstCol == -1)));
			image.UnlockBits(data);
			return firstRow != -1
				? (Rectangle?)new Rectangle(firstCol, firstRow, lastCol - firstCol + 1, lastRow - firstRow + 1)
				: null;
		}
		public static Image ConvertByteArrayToImage(byte[] imageData)
		{
			using (MemoryStream ms = new MemoryStream(imageData))
				return Image.FromStream(ms);
		}
	}
}