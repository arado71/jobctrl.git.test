using System.Drawing;
using System.IO;
using System.Runtime.Serialization;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class Snippet
	{
		private Image image;
		[IgnoreDataMember]
		public Image Image {
			set
			{
				image = value;
				using (var stream = new MemoryStream())
				{
					image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
					ImageData = stream.ToArray();
				}
			}
			get { return image; }
		}
		[IgnoreDataMember]
		public string ImageFileName { get; set; }
	}
}
