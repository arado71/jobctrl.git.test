using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace JcMon2
{
	public static class BitmapHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		public static string BitmapToBase64(Bitmap bitmap)
		{
			using (var ms = new MemoryStream())
			{
				bitmap.Save(ms, ImageFormat.Png);
				return Convert.ToBase64String(ms.ToArray());
			}
		}

		public static Bitmap Base64ToBitmap(string bitmap)
		{
			try
			{
				var data = Convert.FromBase64String(bitmap);
				using (var ms = new MemoryStream(data))
				{
					return (Bitmap)(Bitmap.FromStream(ms));
				}
			}
			catch (Exception)
			{
				log.Error("Failed to decode image");
			}

			return null;
		}
	}
}
