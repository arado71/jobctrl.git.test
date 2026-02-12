using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	
	public class CursorHelper
	{
		public static Bitmap AsBitmap(Control c)
		{
			var bm = new Bitmap(c.Width, c.Height);
			c.DrawToBitmap(bm, new Rectangle(0, 0, c.Width, c.Height));
			return bm;
		}

		private static Bitmap BitmapFromCursor(Cursor cur)
		{
			var ii = new WinApi.IconInfo();
			WinApi.GetIconInfo(cur.Handle, ref ii);

			try
			{
				using (Bitmap bmp = Image.FromHbitmap(ii.hbmColor))
				{
					WinApi.DeleteObject(ii.hbmColor);
					WinApi.DeleteObject(ii.hbmMask);

					BitmapData bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
					using (
							var dstBitmap = new Bitmap(bmData.Width, bmData.Height, bmData.Stride, PixelFormat.Format32bppArgb, bmData.Scan0))
						//todo Check
					{
						bmp.UnlockBits(bmData);
						return new Bitmap(dstBitmap);
					}
				}
			}
			catch (ExternalException ex)
			{
				log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Warn("BitmapFromCursor failed", ex);
				return new Bitmap(32,32);
			}
		}

		public static Cursor CreateCursor(Bitmap bm, int xHotspot, int yHotspot)
		{
			IntPtr ptr = bm.GetHicon();
			var tmp = new WinApi.IconInfo();
			WinApi.GetIconInfo(ptr, ref tmp);
			if (tmp.hbmColor != IntPtr.Zero) WinApi.DeleteObject(tmp.hbmColor);
			tmp.hbmColor = GetBlendedHBitmap(bm);
			tmp.xHotspot = xHotspot;
			tmp.yHotspot = yHotspot;
			tmp.fIcon = false;
			IntPtr cursorPtr = WinApi.CreateIconIndirect(ref tmp);

			if (tmp.hbmColor != IntPtr.Zero) WinApi.DeleteObject(tmp.hbmColor);
			if (tmp.hbmMask != IntPtr.Zero) WinApi.DeleteObject(tmp.hbmMask);
			if (ptr != IntPtr.Zero) WinApi.DestroyIcon(ptr);

			return new Cursor(cursorPtr);
		}

		public static Cursor CreateCursor(Bitmap baseImage, Cursor c, Point position, float imageTransparency)
		{
			using (Bitmap cursorImage = BitmapFromCursor(c))
			{
				using (
					var newCursorImage = new Bitmap(baseImage.Width + cursorImage.Width*2, baseImage.Height + cursorImage.Height*2))
				{
					using (Graphics g = Graphics.FromImage((newCursorImage)))
					{
						var matrix = new ColorMatrix { Matrix33 = imageTransparency };
						using (var attributes = new ImageAttributes())
						{
							attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
							g.FillRectangle(Brushes.Transparent, new Rectangle(new Point(0, 0), newCursorImage.Size));
							g.DrawImage(baseImage, new Rectangle(cursorImage.Width, cursorImage.Height, baseImage.Width, baseImage.Height),
								0, 0, baseImage.Width, baseImage.Height, GraphicsUnit.Pixel, attributes);
							g.DrawImage(cursorImage,
								new Rectangle(position.X - c.HotSpot.X + cursorImage.Width, position.Y - c.HotSpot.Y + cursorImage.Height,
									cursorImage.Width, cursorImage.Height));
						}
					}

					return CreateCursor(newCursorImage, position.X + cursorImage.Width, position.Y + cursorImage.Height);
				}
			}
		}

		public static IntPtr GetBlendedHBitmap(Bitmap bitmap)
		{
			var bitmapInfo = new WinApi.BitmapInfoHeader
			{
				biSize = 40,
				biBitCount = 32,
				biPlanes = 1,
				biWidth = bitmap.Width,
				biHeight = -bitmap.Height
			};

			IntPtr pixelData;
			IntPtr hBitmap = WinApi.CreateDIBSection(
				IntPtr.Zero, ref bitmapInfo, 0, out pixelData, IntPtr.Zero, 0);

			var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			BitmapData bitmapData = bitmap.LockBits(
				bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			WinApi.RtlMoveMemory(
				pixelData, bitmapData.Scan0, bitmap.Height*bitmapData.Stride);

			bitmap.UnlockBits(bitmapData);
			return hBitmap;
		}

		
	}
}