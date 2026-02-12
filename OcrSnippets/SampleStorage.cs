using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Tct.ActivityRecorderService.Ocr
{
	public class SampleStorage : IDisposable
	{
		private readonly Dictionary<Bitmap, Tuple<int, string, int>> samples = new Dictionary<Bitmap, Tuple<int, string, int>>();

		public int Count
		{
			get { return samples.Count; }
		}

		public void Set(Image image, string expectedValue, int id)
		{
			Bitmap clone = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			using (Graphics gr = Graphics.FromImage(clone))
			{
				gr.DrawImage(image, new Rectangle(0, 0, clone.Width, clone.Height));
				if (samples.ContainsKey(clone))
					samples[clone] = new Tuple<int, string, int>(id, expectedValue, 0);
				else
					samples.Add(clone, new Tuple<int, string, int>(id, expectedValue, 0));
			}
		}

		public Dictionary<Bitmap, string> GetSamples()
		{
			Dictionary<Bitmap, string> ret = new Dictionary<Bitmap, string>();
			foreach (var e in samples) ret.Add(e.Key, e.Value.Item2);
			return ret;
		}

		public IEnumerable<long> GetSampleIds()
		{
			foreach (var e in samples)
				yield return e.Value.Item1;

		}
		public void Dispose()
		{
			foreach (var sample in samples)
				if (sample.Key != null)
					sample.Key.Dispose();
			GC.Collect();
		}
	}
}
