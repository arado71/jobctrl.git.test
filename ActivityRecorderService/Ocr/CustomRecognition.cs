using System.Drawing;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using Ocr.Model;
using Ocr.Recognition;

namespace Tct.ActivityRecorderService.Ocr
{
	public class CustomRecognition : RecognitionConfiguration
	{
		private readonly OcrConfiguration config;

		public CustomRecognition(OcrConfiguration config, TransformConfiguration transformConfig)
		{
			this.config = config;
			Configuration = transformConfig;
		}

		public CustomRecognition(OcrConfiguration config, DenseVector vector)
			: this(config, new TransformConfiguration(vector))
		{
#if DEBUG
			var rootAppender = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository())
				.Root.Appenders.OfType<log4net.Appender.FileAppender>()
				.FirstOrDefault();
			DebugPath = rootAppender != null ? System.IO.Path.GetDirectoryName(rootAppender.File) : string.Empty; 
#endif
		}

		public override global::System.Drawing.Rectangle? GetAreaOfInterest(global::System.Drawing.Bitmap image)
		{
			var left = config.HorizontalAlign != HorizontalAlign.Right
				? config.Area.X
				: config.Area.X + (image.Width - config.Size.Width);
			var top = config.VerticalAlign != VerticalAlign.Bottom
				? config.Area.Y
				: config.Area.Y + (image.Height - config.Size.Height);
			var width = config.HorizontalAlign != HorizontalAlign.Stretch
				? config.Area.Width
				: config.Area.Width + (image.Width - config.Size.Width);
			var height = config.VerticalAlign != VerticalAlign.Stretch
				? config.Area.Height
				: config.Area.Height + (image.Height - config.Size.Height);
			if (width < 0 || height < 0) return null;
			return new Rectangle(left, top, width, height);
		}
	}
}