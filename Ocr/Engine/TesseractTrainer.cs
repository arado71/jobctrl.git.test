using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using Ocr.Learning;
using Ocr.Recognition;
using TesseractFontTeacher;

namespace Ocr.Engine
{
	public partial class TesseractEngineEx
	{
		private class TesseractTrainer
		{
			protected static readonly ILog log = LogManager.GetLogger(typeof(TesseractTrainer));
			protected readonly string baseLanguage = "eng";
			protected readonly string tesseractAPIPath;

			public TesseractTrainer(string tesseractApiPath, string baseLanguage)
			{
				this.tesseractAPIPath = tesseractApiPath;
				this.baseLanguage = baseLanguage;
			}

			protected void GenerateLearningInput(string baseFileName, LearningSet toLearn, string workingPath)
			{
				toLearn.Image.Save(Path.Combine(workingPath, baseFileName + ".tif"));
				toLearn.Image.Save(Path.Combine(workingPath, baseFileName + ".png"));
				using (var newBox = new StreamWriter(Path.Combine(workingPath, baseFileName + ".box")))
				{
					foreach (var ch in toLearn.Characters)
						newBox.WriteLine("{0} {1} {2} {3} {4}", ch.Character, (int)ch.Position.X,
							toLearn.Image.Height - (int)Math.Ceiling(ch.Position.Y + ch.Position.Height),
							(int)Math.Ceiling(ch.Position.X + ch.Position.Width), (int)(toLearn.Image.Height - ch.Position.Y));
				}
			}

			protected int GenerateTrFile(string workingPath, string baseFileName)
			{
				return PromptHelper.Run(Path.Combine(tesseractAPIPath, "tesseract.exe"),
					baseFileName + ".tif " + baseFileName + " nobatch box.train",
					workingPath, 10000);
			}

			private int GenerateUnicharset(string workingPath, string baseFileName)
			{
				return PromptHelper.Run(Path.Combine(tesseractAPIPath, "unicharset_extractor.exe"),
					baseFileName + ".box",
					workingPath);
			}

			private void GenerateFontFile(string workingPath, string fontName)
			{
				using (var fontFile = new StreamWriter(Path.Combine(workingPath, "font_properties")))
				{
					fontFile.WriteLine("{0} 0 0 0 0 0", fontName);
				}
			}

			protected int GenerateShapeCluster(string workingPath, string language, params string[] trFiles)
			{
				return PromptHelper.Run(Path.Combine(tesseractAPIPath, "shapeclustering.exe"),
					string.Format("-F font_properties -U unicharset -O {0}.unicharset {1}", language, string.Join(" ", trFiles)),
					workingPath);
			}

			protected int MfTraining(string workingPath, string language, params string[] trFiles)
			{
				return PromptHelper.Run(Path.Combine(tesseractAPIPath, "mftraining.exe"),
					string.Format("-F font_properties -U unicharset -O {0}.unicharset {1}", language, string.Join(" ", trFiles)),
					workingPath);
			}

			protected int CnTraining(string workingPath, params string[] trFiles)
			{
				return PromptHelper.Run(Path.Combine(tesseractAPIPath, "cntraining.exe"),
					string.Join(" ", trFiles),
					workingPath);
			}

			private static void AddPrefix(string path, string name, string prefix)
			{
				File.Move(Path.Combine(path, name), Path.Combine(path, string.Format("{0}{1}", prefix, name)));
			}

			protected int CombineTessdata(string workingPath, string language)
			{
				AddPrefix(workingPath, "shapetable", language + ".");
				AddPrefix(workingPath, "normproto", language + ".");
				AddPrefix(workingPath, "inttemp", language + ".");
				AddPrefix(workingPath, "pffmtable", language + ".");
				return PromptHelper.Run(Path.Combine(tesseractAPIPath, "combine_tessdata.exe"),
					language + ".",
					workingPath);
			}

			public string GetTrainedData(string text, Font font, TransformConfiguration config)
			{
				try
				{
					var path = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
					Directory.CreateDirectory(path);
					var fontName = font.Name.Replace(" ", "");
					var baseFileName = baseLanguage + "." + fontName + ".exp0";
					var toLearn = LearningSet.Create(font, text, TextRenderingHint.ClearTypeGridFit);
					using (var transformedLearnSet = new LearningSet(toLearn, config))
					{
						GenerateLearningInput(baseFileName, transformedLearnSet, path);
					}
					Stopwatch sw = Stopwatch.StartNew();
					int retVal = GenerateTrFile(path, baseFileName);
					sw.Stop();
					log.DebugFormat("Return value: {0}, time: {1} ms", retVal, sw.ElapsedMilliseconds);
					OcrEngineStatsHelper.Add(sw.Elapsed.TotalSeconds);
					if (retVal != 0)
					{
						OcrEngineStatsHelper.AddTimeout();
						throw new ArgumentException("GenerateTrFile " + path);
					}
					if (GenerateUnicharset(path, baseFileName) != 0) throw new ArgumentException("GenerateUnicharset " + path);
					GenerateFontFile(path, fontName);
					if (GenerateShapeCluster(path, config.Language, baseFileName + ".tr") != 0) throw new ArgumentException("GenerateShapeCluster " + path);
					if (MfTraining(path, config.Language, baseFileName + ".tr") != 0) throw new ArgumentException("MfTraining " + path);
					if (CnTraining(path, baseFileName + ".tr") != 0) throw new ArgumentException("CnTraining " + path);
					if (CombineTessdata(path, config.Language) != 0) throw new ArgumentException("CombineTessdata " + path);
					var targetFile = Path.GetFullPath(Path.Combine(TessDataPath, config.Language + ".traineddata"));
					File.Copy(Path.Combine(path, config.Language + ".traineddata"), targetFile, true);
					var done = false;
					while (!done)
						try
						{
							Directory.Delete(path, true);
							done = true;
						}
						catch
						{
							Thread.Sleep(10);
						}

					return Path.Combine(TessDataPath, config.Language + ".traineddata");
				}
				catch (ArgumentException e)
				{
					log.Error("Error while configure training", e);
				}
				catch (Exception e)
				{
					log.Error("Error while training", e);
				}

				return null;
			}
		}

		private class TesseractTrainerExtended : TesseractTrainer
		{
			public TesseractTrainerExtended(string tesseractApiPath, string baseLanguage) : base(tesseractApiPath, baseLanguage)
			{

			}

			public string GetTrainedData(string text, List<Font> fonts, TransformConfiguration config)
			{
				try
				{
					var path = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
					Directory.CreateDirectory(path);
					var baseFileNameList = new List<string>();
					foreach (var font in fonts)
					{
						var fontName = font.Name.Replace(" ", "");
						var baseFileName = baseLanguage + "." + fontName + ".exp0";
						var toLearn = LearningSet.Create(font, text, TextRenderingHint.ClearTypeGridFit);
						using (var transformedLearnSet = new LearningSet(toLearn, config))
						{
							GenerateLearningInput(baseFileName, transformedLearnSet, path);
						}
						Stopwatch sw = Stopwatch.StartNew();
						int retVal = GenerateTrFile(path, baseFileName);
						sw.Stop();
						log.DebugFormat("Return value: {0}, time: {1} ms", retVal, sw.ElapsedMilliseconds);
						OcrEngineStatsHelper.Add(sw.Elapsed.TotalSeconds);
						if (retVal != 0)
						{
							OcrEngineStatsHelper.AddTimeout();
							throw new ArgumentException("GenerateTrFile " + path);
						}
						baseFileNameList.Add(baseFileName);
					}

					if (GenerateUnicharset(path, baseFileNameList) != 0) throw new ArgumentException("GenerateUnicharset " + path);
					GenerateFontFile(path, fonts.Select(font => font.Name.Replace(" ", "")).ToList());
					if (GenerateShapeCluster(path, config.Language, baseFileNameList.Select(name => name + ".tr").ToArray()) != 0) throw new ArgumentException("GenerateShapeCluster " + path);
					if (MfTraining(path, config.Language, baseFileNameList.Select(name => name + ".tr").ToArray()) != 0) throw new ArgumentException("MfTraining " + path);
					if (CnTraining(path, baseFileNameList.Select(name => name + ".tr").ToArray()) != 0) throw new ArgumentException("CnTraining " + path);
					if (CombineTessdata(path, config.Language) != 0) throw new ArgumentException("CombineTessdata " + path);
					var targetFile = Path.GetFullPath(Path.Combine(TessDataPath, config.Language + ".traineddata"));
					File.Copy(Path.Combine(path, config.Language + ".traineddata"), targetFile, true);
					var done = false;
					while (!done)
						try
						{
							Directory.Delete(path, true);
							done = true;
						}
						catch
						{
							Thread.Sleep(10);
						}

					return Path.Combine(TessDataPath, config.Language + ".traineddata");
				}
				catch (ArgumentException e)
				{
					log.Error("Error while configure training", e);
				}
				catch (Exception e)
				{
					log.Error("Error while training", e);
				}

				return null;
			}

			private int GenerateUnicharset(string workingPath, List<string> baseFileNames)
			{
				var parameters = string.Join(" ", baseFileNames.Select(name => name + ".box"));
				return PromptHelper.Run(Path.Combine(tesseractAPIPath, "unicharset_extractor.exe"),
					parameters,
					workingPath);
			}

			private void GenerateFontFile(string workingPath, List<string> baseFileNames)
			{
				using (var fontFile = new StreamWriter(Path.Combine(workingPath, "font_properties")))
				{
					foreach (var fontName in baseFileNames)
					{
						fontFile.WriteLine("{0} 0 0 0 0 0", fontName);
					}
				}
			}
		}
	}
}