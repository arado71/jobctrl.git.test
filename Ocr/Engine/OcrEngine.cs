using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Ocr.Engine
{
	public struct Recognition
	{
		public Rectangle Area { get; set; }
		public string Text { get; set; }
	}

	public abstract class OcrEngine : IDisposable
	{
		public abstract string Name { get; }
		public abstract string Language { get; }

		public abstract void Dispose();
		public abstract IEnumerable<Recognition> RecognizeAreas(Bitmap image);

		public virtual string RecognizeString(Bitmap image)
		{
			return string.Join(" ", RecognizeAreas(image).Select(x => x.Text)).TrimEnd('\n', '\r', ' ', '\t');
		}
	}
}