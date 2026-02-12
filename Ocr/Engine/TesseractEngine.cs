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
		private static readonly string currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		public static readonly string TesseractAPIPath = ConfigurationManager.AppSettings["tesseractPath"] ?? Path.Combine(currentPath, "tessapi");

		public static string TessDataPath
		{
			get { return Path.Combine(TesseractAPIPath, "tessdata"); }
		}

		private static readonly TesseractTrainer trainer = new TesseractTrainer(TesseractAPIPath, "eng");

		private readonly TesseractEngine engine;
		private readonly string lang;
		private readonly object lockObject = new object();

		public TesseractEngineEx(string language)
		{
			lang = language;
			engine = new TesseractEngine(TessDataPath, language, EngineMode.Default);
		}

		private TesseractEngineEx(string tessFile, string language)
		{
			lang = language;
			engine = new TesseractEngine(Path.GetDirectoryName(tessFile), Path.GetFileNameWithoutExtension(tessFile),
				EngineMode.Default);
		}

		public override string Name
		{
			get { return "Tesseract " + lang; }
		}

		public override string Language
		{
			get { return lang; }
		}

		public static OcrEngine FromFile(string filename)
		{
			var lang = Path.GetFileNameWithoutExtension(filename);
			File.Copy(filename, Path.Combine(TessDataPath, lang + ".traineddata"), true);
			return new TesseractEngineEx(lang);
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
							yield return
								new Recognition
								{
									Area = new Rectangle(r.X1, r.Y1, r.X2 - r.X1, r.Y2 - r.Y1),
									Text = it.GetText(PageIteratorLevel.Word)
								};
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

		public static TesseractEngineEx Train(string text, Font font, TransformConfiguration config)
		{
			var tessFile = trainer.GetTrainedData(text, font, config);
			return tessFile != null 
				? new TesseractEngineEx(tessFile, config.Language) 
				: null;
		}

		public static bool CreateTrainData(string text, Font font, TransformConfiguration config, string filePath)
		{
			var targetFile = filePath;
			config.Language = Path.GetFileNameWithoutExtension(config.Language);
			var tessFile = trainer.GetTrainedData(text, font, config);
			
			if (tessFile != null)
			{
				if (File.Exists(targetFile)) File.Delete(targetFile);
				File.Copy(tessFile, targetFile);
				return true;
			}
			return false;
		}

		public static bool CreateCombinedTrainData(string text, List<Font> fonts, TransformConfiguration config, string filePath)
		{
			var combinedTrainer = new TesseractTrainerExtended(TesseractAPIPath, "eng");
			var targetFile = filePath;
			config.Language = Path.GetFileNameWithoutExtension(config.Language);
			var tessFile = combinedTrainer.GetTrainedData(text, fonts, config);

			if (tessFile != null)
			{
				if (File.Exists(targetFile)) File.Delete(targetFile);
				File.Copy(tessFile, targetFile);
				return true;
			}
			return false;
		}

		public override void Dispose()
		{
			engine.Dispose();
		}
	}
}