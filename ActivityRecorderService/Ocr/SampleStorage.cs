using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Drawing;
using System.Linq;
using System.Drawing.Imaging;

namespace Tct.ActivityRecorderService.Ocr
{
	public class SampleStorage : IDisposable
	{
		private class SampleItem
		{
			private readonly SnippetExt snippet;
			public Guid Guid => snippet.Guid;
			public int UserId => snippet.UserId;
			public int CompanyId => snippet.CompanyId;
			public string ExpectedValue => snippet.Content;
			public string ProcessName => snippet.ProcessName;
			public int Quality => snippet.Quality;
			public int Error { get; private set; }
			public string LastGuess { get; private set; }
			public SampleItem(SnippetExt snippet)
			{
				this.snippet = snippet;
			}
			public void AddError(string guess)
			{
				++Error;
				LastGuess = guess;
			}
		}
		private readonly Dictionary<Bitmap, SampleItem> samples = new Dictionary<Bitmap, SampleItem>();

		public IEnumerable<KeyValuePair<byte[], Tuple<string, string, int, string, int>>> GetProblematicItems
		{
			get
			{
				using (var stream = new System.IO.MemoryStream())
					foreach (var img in samples.Where(e => e.Value.Error > 0))
					{
						stream.SetLength(0);
						img.Key.Save(stream, ImageFormat.Png);
						yield return new KeyValuePair<byte[], Tuple<string, string, int, string, int>>(stream.ToArray(),
							new Tuple<string, string, int, string, int>(img.Value.ExpectedValue, img.Value.LastGuess, img.Value.UserId, img.Value.ProcessName, GetNewQuality(img.Value.Quality, true)));
					}
			}
		}
		public bool TrySet(Image image, SnippetExt snippet)
		{
			try
			{
				Bitmap clone = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
				using (Graphics gr = Graphics.FromImage(clone))
				{
					gr.DrawImage(image, new Rectangle(0, 0, clone.Width, clone.Height));
					if (samples.ContainsKey(clone))
						samples[clone] = new SampleItem(snippet);
					else
						samples.Add(clone, new SampleItem(snippet));
				}
			}
			catch (ArgumentException) { return false; }
			return true;
		}

		public Dictionary<Bitmap, Tuple<Guid, string>> GetSamples()
		{
			Dictionary<Bitmap, Tuple<Guid, string>> ret = new Dictionary<Bitmap, Tuple<Guid, string>>();
			foreach (var e in samples) ret.Add(e.Key, new Tuple<Guid, string>(e.Value.Guid, e.Value.ExpectedValue));
			return ret;
		}

		public IEnumerable<Guid> GetSampleIds()
		{
			foreach (var e in samples)
				yield return e.Value.Guid;

		}
		public int ErrorCounter { get { return samples.Sum(e => e.Value.Error); } }
		public void AddError(Guid guid, string guess)
		{
			var s = samples.FirstOrDefault(e => e.Value.Guid == guid);
			if (samples[s.Key] != null)
				samples[s.Key].AddError(guess);
		}

		public int CompanyErrors(int company)
		{
			return samples.Where(e => e.Value.CompanyId == company).Sum(e => e.Value.Error);
		}

		public void RegisterProcessResult()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.SetXactAbortOn();
				var processDate = DateTime.Now;
				foreach (var sample in samples)
				{
					var rec = context.Snippets.SingleOrDefault(e => e.Guid == sample.Value.Guid);
					if (rec == null) continue;
					rec.Quality = GetNewQuality(rec.Quality, sample.Value.Error > 0);
					if (rec.ProcessedAt == null)
					{
						rec.ProcessedAt = processDate;
					}
						
				}
				context.SubmitChanges();
			}
		}

		public List<Guid> RestoreQuality()
		{
			var restored = new List<Guid>();
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.SetXactAbortOn();
				foreach (var sample in samples)
				{
					var rec = context.Snippets.SingleOrDefault(e => e.Guid == sample.Value.Guid);
					if (rec == null) continue;
					if (sample.Value.Error == 0)
					{
						rec.Quality = 5;
						restored.Add(rec.Guid);
					}
				}
				context.SubmitChanges();
			}
			return restored;
		}

		private int GetNewQuality(int oldQuality, bool decrease)
		{
			return decrease ? Math.Max(oldQuality - 1, 0) : Math.Min(oldQuality + 1, 10);
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
