using SkiaSharp;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class Snippet
	{
		private SKBitmap image;

		[IgnoreDataMember]
		public SKBitmap Image
		{
			get => image;
			set
			{
				image = value;
				using var stream = new MemoryStream();
				using var skImage = SKImage.FromBitmap(image);
				using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);
				data.SaveTo(stream);
				ImageData = stream.ToArray();
			}
		}

		[IgnoreDataMember]
		public string ImageFileName { get; set; }
	}
}
