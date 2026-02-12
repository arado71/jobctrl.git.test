using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using Ocr.Engine;
using Ocr.Helper;
using Ocr.Model;
using Ocr.Recognition;
using TcT.OcrSnippets;

namespace OcrConfig
{
	public class OcrLearnMetric
	{
		private OcrLearnMetric()
		{
			Results = new List<KeyValuePair<string, string>>();
		}

		private CustomRecognition Configuration { get; set; }
		public List<KeyValuePair<string, string>> Results { get; private set; }
		public double EvaluationTime { get; private set; }
		public double TrainTime { get; private set; }
		public string LearningString { get; private set; }

		public static OcrLearnMetric Create(DenseVector v, OcrConfiguration config, string trainingText,
			Dictionary<Bitmap, string> cases, Font f)
		{
			var timer = Stopwatch.StartNew();
			var transConfig = new TransformConfiguration(v)
			{
				BrightnessCorrection = (float) v[6],
				ContrastCorrection = (float) v[7],
				TresholdChannel = (ImageHelper.DesaturateMode) (int) v[8],
				TresholdLimit = v[9] > 50 ? (byte?) v[9] : null,
				Scale = v[10],
				InterpolationMode = ImageHelper.ToInterpolationMode(v[11]),
				Language = config.Language
			};
			var trainedEngine = TesseractEngineEx.Train(trainingText, f, transConfig);
			if (trainedEngine == null) return null;
			var res = new OcrLearnMetric
			{
				Configuration = new CustomRecognition(config, v),
				TrainTime = timer.Elapsed.TotalMilliseconds,
				LearningString = trainingText
			};
			timer.Restart();
			foreach (var c in cases)
				res.Results.Add(new KeyValuePair<string, string>(c.Value,
					RecognitionService.Recognize(c.Key, trainedEngine, res.Configuration)));

			res.EvaluationTime = timer.Elapsed.TotalMilliseconds;
			return res;
		}

		public static double Evaluate(OcrLearnMetric learnMetric)
		{
			if (learnMetric == null) return double.MaxValue;
			return learnMetric.Results.Sum(x => Math.Sqrt(RecognitionService.EvaluateResult(x.Value, x.Key)));
		}
	}
}