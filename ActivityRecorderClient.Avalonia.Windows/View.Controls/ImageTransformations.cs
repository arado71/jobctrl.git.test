using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.View.Controls
{
	class ImageTransformations
	{
		public static Bitmap DisableImage(Image image)
		{
			var disabledImage = new Bitmap(image.Width, image.Height);
			using (Graphics graphics = Graphics.FromImage(disabledImage))
			{
				using (ImageAttributes imageAttributes = new ImageAttributes())
				{
					ColorMatrix colorMatrix = new ColorMatrix(
						 new float[][]
						 {
							 new float[] {.3f, .3f, .3f, 0, 0},
							 new float[] {.59f, .59f, .59f, 0, 0},
							 new float[] {.11f, .11f, .11f, 0, 0},
							 new float[] {0, 0, 0, 1, 0},
							 new float[] {0, 0, 0, 0, 1}
						 });
					imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
					Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
					graphics.DrawImage(image, rect, rect.X, rect.Y, rect.Width, rect.Height, GraphicsUnit.Pixel, imageAttributes);
				}
			}
			return disabledImage;
		}
	}
}
