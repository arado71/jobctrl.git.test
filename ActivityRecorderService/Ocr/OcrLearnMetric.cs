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

namespace Tct.ActivityRecorderService.Ocr
{
	public class OcrLearnMetric
	{
		public class ResultElement
		{
			public Image Image { get; private set; }
			public Guid Guid { get; private set; }
			public string Expected { get; private set; }
			public string Result { get; private set; }

			public ResultElement(Image key, Guid guid, string expected, string result)
			{
				Image = key;
				Guid = guid;
				Expected = expected;
				Result = result;
			}
		}
		private OcrLearnMetric()
		{
			Results = new List<ResultElement>();
		}

		private CustomRecognition Configuration { get; set; }
		public List<ResultElement> Results { get; private set; }
		public double EvaluationTime { get; private set; }
		public double TrainTime { get; private set; }
		public string LearningString { get; private set; }

		public static OcrLearnMetric Create(DenseVector v, OcrConfiguration config, string trainingText,
			Dictionary<Bitmap, Tuple<Guid, string>> cases, Font f)
		{
			var timer = Stopwatch.StartNew();
			var transConfig = new TransformConfiguration(v)
			{
				Brightness = (float)v[6],
					BrightnessCorrection = (float)v[6],
				Contrast = (float)v[7],
					ContrastCorrection = (float)v[7],
				TresholdMode = (ImageHelper.DesaturateMode)((int)v[8]),
				TresholdChannel = (ImageHelper.DesaturateMode)((int)v[8]),
				TresholdLimit = (byte)v[3],
				//TresholdLimit = v[9] > 50 ? (byte?)v[9] : null,
				Scale = v[10],
				InterpolationMode = ImageHelper.ToInterpolationMode(v[11]),
				Language = config.Language ?? "eng"
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
				res.Results.Add(new ResultElement(
					c.Key,
					c.Value.Item1,
					c.Value.Item2,
					RecognitionService.Recognize(c.Key, trainedEngine, res.Configuration)));

			res.EvaluationTime = timer.Elapsed.TotalMilliseconds;
			return res;
		}

		public static double Evaluate(OcrLearnMetric learnMetric)
		{
			if (learnMetric == null) return double.MaxValue;
			return learnMetric.Results.Sum(x => Math.Sqrt(RecognitionService.EvaluateResult(x.Result, x.Expected)));
		}
	}
}