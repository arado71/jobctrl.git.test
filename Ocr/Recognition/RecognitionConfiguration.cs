using System.Drawing;

namespace Ocr.Recognition
{
	public abstract class RecognitionConfiguration
	{
		public TransformConfiguration Configuration { get; protected set; }
		public string DebugPath { get; set; }
		public abstract Rectangle? GetAreaOfInterest(Bitmap image);
	}
}