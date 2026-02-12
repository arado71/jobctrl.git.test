using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using log4net;
using Ocr.Engine;
using Ocr.Helper;

namespace Ocr.Recognition
{
	public static class RecognitionService
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(RecognitionService));

		public static Bitmap Transform(this Bitmap inputImage, Rectangle areaOfInterest, TransformConfiguration config,
			bool fullClientArea = false, string debugPath = null)
		{
			var area = fullClientArea ? areaOfInterest : new Rectangle(0, 0, areaOfInterest.Width, areaOfInterest.Height);
			var swPart = Stopwatch.StartNew();
			var ocrImage = inputImage.ScaledCopy(area, config.Scale, config.BrightnessCorrection, config.ContrastCorrection,
				config.InterpolationMode);
			if (!string.IsNullOrEmpty(debugPath))
				ocrImage.Save(Path.Combine(debugPath, "recog_transform.png"));
			var swScale = swPart.Elapsed.TotalMilliseconds;

			swPart = Stopwatch.StartNew();
			if (config.TresholdLimit.HasValue)
			{
				//ocrImage.Treshold(config.TresholdLimit.Value, config.TresholdChannel);
				//log.DebugFormat("OCR T 1:{0} 2:{1}", config.TresholdLimit.Value, config.TresholdChannel);
			}

			if (!string.IsNullOrEmpty(debugPath))
				ocrImage.Save(Path.Combine(debugPath, "recog_treshold.png"));
			/*
			var swGs = swPart.Elapsed.TotalMilliseconds;
			log.DebugFormat(" Transformation: {0} ms", swGs + swScale);
			log.DebugFormat("  GrayScale: {0} ms", swGs);
			log.DebugFormat("  Scale: {0} ms", swScale);
			*/
			return ocrImage;
		}

		public static string Recognize(Bitmap processedImage, OcrEngine engine)
		{
			var swPart = Stopwatch.StartNew();
			try
			{
				var r = engine.RecognizeString(processedImage);
				// string.Join(" ", engine.RecognizeAreas(processedImage).Select(x => x.Text)).TrimEnd('\n', '\r', ' ', '\t');
				var swRecog = swPart.Elapsed.TotalMilliseconds;
				//log.DebugFormat(" Recognition: {0} ms", swRecog);
				return r;
			}
			catch (Exception ex)
			{
				log.DebugFormat(" Recognition fail: {0} ms", swPart.Elapsed.TotalMilliseconds);
				log.ErrorFormat("Exception: {0}", ex.Message);
			}

			return null;
		}

		// https://en.wikipedia.org/wiki/Levenshtein_distance
		private static int GetLevenshteinDistance(string expected, string actual)
		{
			var dp = new int[actual.Length + 1, expected.Length + 1];
			for (var i = 0; i <= actual.Length; i++) dp[i, 0] = i;
			for (var i = 0; i <= expected.Length; i++) dp[0, i] = i;

			for (var i = 1; i <= actual.Length; i++)
				for (var j = 1; j <= expected.Length; j++)
				{
					var mod = actual[i - 1] == expected[j - 1] ? 0 : 1;
					dp[i, j] = new[] { dp[i - 1, j] + 1, dp[i, j - 1] + 1, dp[i - 1, j - 1] + mod }.Min();
				}
			log.DebugFormat("actual:{0} expected:{1}", actual, expected);
			return dp[actual.Length, expected.Length];
		}

		public static double EvaluateResult(string result, string expected)
		{
			if (result == null) result = "";
			if (expected == null) expected = "";
			if (result.Length == 0 && expected.Length == 0) return 0.0;
			return GetLevenshteinDistance(expected, result);
		}

		public static string Recognize(Bitmap image, OcrEngine engine, RecognitionConfiguration config,
			string imageName = "image")
		{
			if (config == null) throw new ArgumentException("config");

			var sw = Stopwatch.StartNew();
			Rectangle? area;
			if (string.IsNullOrEmpty(config.DebugPath))
				area = config.GetAreaOfInterest(image);
			else
				using (var debugImage = (Bitmap)image.Clone())
				{
					area = config.GetAreaOfInterest(debugImage);
					debugImage.Save(Path.Combine(config.DebugPath, string.Format("raw_{0}.png", imageName)));
				}

			if (!area.HasValue)
			{
				log.WarnFormat("Image {0} has no area of interest", imageName);
				return null;
			}
			using (var ocrImage = image.Transform(area.Value, config.Configuration, true, debugPath:config.DebugPath))
			{
				if (ocrImage == null)
				{
					log.WarnFormat("Image {0} couldn't be recognized", imageName);
					return null;
				}

				if (!string.IsNullOrEmpty(config.DebugPath))
					ocrImage.Save(Path.Combine(config.DebugPath, "recog_" + imageName + ".png"));
				var r = Recognize(ocrImage, engine);
				//log.DebugFormat("Processed in {0} ms", sw.Elapsed.TotalMilliseconds);
				return r;
			}
		}
		public static string Recognize(string rawFileName, OcrEngine engine, RecognitionConfiguration config)
		{
			if (config == null) throw new ArgumentException("config");
			if (rawFileName == null) throw new ArgumentException("rawFileName");

			var filename = Path.GetFileNameWithoutExtension(rawFileName);
			log.DebugFormat("Processing {0}...", filename);
			using (var b = (Bitmap)(Bitmap.FromFile(rawFileName)))
			{
				return Recognize(b, engine, config, filename);
			}
		}
	}
}