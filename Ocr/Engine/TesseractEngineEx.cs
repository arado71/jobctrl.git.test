using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using Ocr.Recognition;
using Tesseract;

namespace Ocr.Engine
{
	public partial class TesseractEngineEx : OcrEngine
	{
		private const PageSegMode PageSegmentationMode = PageSegMode.SingleLine;
		private readonly Tesseract.TesseractEngine engine;
		private readonly object lockObject = new object();
		private readonly string lang;

		public TesseractEngineEx(string language)
		{
			lang = language;
			engine = new Tesseract.TesseractEngine(@".\tessdata", language, EngineMode.Default);
		}

		private TesseractEngineEx(string tessFile, string language)
		{
			lang = language;
			engine = new Tesseract.TesseractEngine(Path.GetDirectoryName(tessFile), Path.GetFileNameWithoutExtension(tessFile), EngineMode.Default);
		}

		public static OcrEngine FromFile(string filename)
		{
			var lang = Path.GetFileNameWithoutExtension(filename);
			File.Copy(filename, Path.Combine(@".\tessdata", lang + ".traineddata"), true);
			return new TesseractEngineEx(lang);
		}

		public override string Name
		{
			get { return "Tesseract " + lang; }
		}

		public override string Language
		{
			get
			{
				return lang;
			}
		}

		public override IEnumerable<Recognition> RecognizeAreas(Bitmap bitmap)
		{
			var p = PixConverter.ToPix(bitmap);
			lock (lockObject)
			{
				using (var page = engine.Process(p, PageSegmentationMode))
				using (var it = page.GetIterator())
				{
					it.Begin();
					do
					{
						Rect r;
						if (it.TryGetBoundingBox(PageIteratorLevel.Word, out r))
						{
							yield return
								new Recognition()
								{
									Area = new Rectangle(r.X1, r.Y1, r.X2 - r.X1, r.Y2 - r.Y1),
									Text = it.GetText(PageIteratorLevel.Word)
								};
						}
					} while (it.Next(PageIteratorLevel.Word));
				}
			}
		}

		public override string RecognizeString(Bitmap image)
		{
			var p = PixConverter.ToPix(image);
			lock (lockObject)
			{
				using (var page = engine.Process(p, PageSegmentationMode))
				{
					return page.GetText().Trim('\n', '\r', ' ', '\t');
				}
			}
		}

		private static readonly TesseractTrainer trainer = new TesseractTrainer(ConfigurationManager.AppSettings["tesseractPath"], "eng");
		public static TesseractEngineEx Train(string text, Font font, TransformConfiguration config, string name = "train")
		{
			var tessFile = trainer.GetTrainedData(text, font, name, config);
			return tessFile != null ? new TesseractEngineEx(tessFile, name) : null;
		}

		public static bool CreateTrainData(string text, Font font, TransformConfiguration config, string targetFile)
		{
			var tessFile = trainer.GetTrainedData(text, font, Path.GetFileNameWithoutExtension(targetFile), config);
			if (tessFile != null)
			{
				try
				{
					if (File.Exists(targetFile)) File.Delete(targetFile);
					File.Move(tessFile, targetFile);
					return true;
				}
				catch
				{
				}
			}

			return false;
		}

		public override void Dispose()
		{
			engine.Dispose();
		}
	}
}
