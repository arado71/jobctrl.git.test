using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using log4net;
using Ocr.Learning;
using MathNet.Numerics.LinearAlgebra.Double;
using Ocr.Optimization;
using Ocr.Recognition;
using System.Text.RegularExpressions;
using Ocr.Engine;
using Ocr.Helper;
using Ocr.Model;
using Tesseract;
using System.IO;

namespace Tct.ActivityRecorderService
{
	public class SnippetExt : Snippet
	{
		public int CompanyId { get; set; }
	}
}

namespace Tct.ActivityRecorderService.Ocr
{
	public class LearningManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LearningManager));
		private const string tesseractEnvVariableName = "TESSDATA_PREFIX";
		private readonly Dictionary<int, OcrConfig> successMetric = new Dictionary<int, OcrConfig>();
		private const int MAX_LEARNING_ITEMS = 50;
		private const int MAX_CHECKING_ITEMS = 100;
		private const int MAX_GOOD_ITEMS = 120;
		private readonly bool isTessapiAvailable;
		private RecognitionProcessor contentProcessor;

		public LearningManager()
			: base(log)
		{
#if DEBUG
			ManagerCallbackInterval = 60 * 1000;
#else
			ManagerCallbackInterval = ConfigManager.OcrLearningManagerInterval;
#endif
			log.DebugFormat("Learning callback interval = {0}ms", ManagerCallbackInterval);

			if (!Directory.Exists(TesseractEngineEx.TesseractAPIPath))
			{
				log.DebugFormat("Tessapi path: {0} is not available so ocr learning is disabled.", TesseractEngineEx.TesseractAPIPath);
				return;
			}

			if (Environment.GetEnvironmentVariable(tesseractEnvVariableName) == null)
			{
				log.DebugFormat("Environment variable {0} not found", tesseractEnvVariableName);
				Environment.SetEnvironmentVariable(tesseractEnvVariableName, TesseractEngineEx.TesseractAPIPath, EnvironmentVariableTarget.Process);
				log.DebugFormat("Environment variable {0} added with value: {1}", tesseractEnvVariableName, TesseractEngineEx.TesseractAPIPath);
			}
			isTessapiAvailable = true;
		}

		protected override void ManagerCallbackImpl()
		{
			if (!isTessapiAvailable) { return; }
			int itemsProcessed = 0;
			int errorsResulted;
			using (OcrStatsHelper statsHelper = new OcrStatsHelper())
			{
				// not processed are the new ones
				var newSnippets = GetSnippets(context => from e in context.Snippets
													  where e.Content != "" &&
															e.IsBadData != true &&
															e.ProcessedAt == null
													  select e).ToList();
				if (!newSnippets.Any())
				{
					var snippets = GetRandomLowQualitySnippet();
					if (snippets.Count > 0)
					{
						RecognizeSnippets(snippets, statsHelper, GetReferenceMetric, out itemsProcessed, out errorsResulted);
						var restored = statsHelper.SampleStorage.RestoreQuality();
						log.DebugFormat("Quality of {0} snippet(s) restored", restored.Count);
						restored.ForEach(rs => log.DebugFormat("Restored snippet: {0}", rs));
					}
					return;
				}

				var rules = new OcrRules(newSnippets);

				foreach (var ruleId in rules.RuleIds)
				{
					bool isLearningSuccessful = false;
					log.Debug("RuleId: " + ruleId);
					OcrEngineStatsHelper.ClearStats();
					var ruleParam = rules.Parameters.Where((r) => r.RuleId == ruleId).FirstOrDefault();
					log.Debug("ProcessName: " + ruleParam.ProcessName);
					log.Debug("CompanyId: " + ruleParam.CompanyId);
					var snippetsForRule = rules.GetItems(e => e.RuleId == ruleId);

					OcrSnippetValidator validator = new OcrSnippetValidator(ruleParam.CharSet, snippetsForRule);
					if (validator.HasInvalidContent())
					{
						log.WarnFormat("CharSet from config ({0}) is not compatible with the contents of the snippets for rule {1}!", ruleParam.CharSet, ruleParam.RuleId);
						validator.MarkBadData();

						var updatedSnippets = GetSnippets(context => from e in context.Snippets
																 where e.Content != "" &&
																	   e.IsBadData != true &&
																	   e.ProcessedAt == null &&
																	   e.RuleId == ruleId
																 select e).ToList();
						if (!updatedSnippets.Any()) continue;
						var updatedRules = new OcrRules(updatedSnippets);
						snippetsForRule = updatedRules.GetItems(e => e.RuleId == ruleId);
					}

					statsHelper.AddStat(StatsTypeEnum.CompanyId, ruleParam.CompanyId);
					statsHelper.AddStat(StatsTypeEnum.NewSnippetsCount, snippetsForRule.Count);
					log.DebugFormat("New Snippets: {0}", snippetsForRule.Count);
					RecognizeSnippets(snippetsForRule, statsHelper, GetReferenceMetric, out itemsProcessed, out errorsResulted);
					if (itemsProcessed == snippetsForRule.Count && errorsResulted == 0)
					{
						statsHelper.SampleStorage.RegisterProcessResult();
						isLearningSuccessful = true;
						statsHelper.AddStat(StatsTypeEnum.Iterations, 0);
						statsHelper.AddStat(StatsTypeEnum.ElapsedMinutes, 0);
					}
					else
					{
						var processStarted = DateTime.Now;
						for (int iterations = 1; iterations <= ConfigManager.OcrLearningIterationsMaxCount; iterations++)
						{
							statsHelper.AddStat(StatsTypeEnum.ElapsedMinutes, Convert.ToInt32(Math.Floor((DateTime.Now - processStarted).TotalMinutes)));
							if ((DateTime.Now - processStarted).TotalMinutes > ConfigManager.OcrLearningTimeoutInMinutes)
							{
								//time limit reached
								log.Debug("OCR learning of the current rule stopped because of timeout. RuleId: " + ruleId);
								break;
							}
							log.Debug("OCR learning of the current rule iteration #" + iterations + ". RuleId: " + ruleId);
							statsHelper.AddStat(StatsTypeEnum.Iterations, iterations);
							var oldSnippets = CreateBalancedSnippetCollection(ruleId, processStarted);
							List<SnippetExt> learningSnippets;
							List<SnippetExt> checkingSnippets;
							if (oldSnippets.Count >= MAX_CHECKING_ITEMS + MAX_LEARNING_ITEMS)
							{
								learningSnippets = oldSnippets.Take(MAX_LEARNING_ITEMS).ToList();
								checkingSnippets = oldSnippets.Skip(MAX_LEARNING_ITEMS).Take(MAX_CHECKING_ITEMS).ToList();
							}
							else
							{
								int learningCount = Convert.ToInt32(Math.Ceiling(oldSnippets.Count / 2.0));
								learningSnippets = oldSnippets.Take(learningCount).ToList();
								checkingSnippets = oldSnippets.Skip(learningCount).Take(MAX_CHECKING_ITEMS).ToList();
							}
							learningSnippets.AddRange(snippetsForRule);
							log.DebugFormat("OCR learning groups: learning: {0}, checking: {1}", learningSnippets.Count, checkingSnippets.Count);

							LearningSnippets(rules, learningSnippets, ruleParam, statsHelper, out errorsResulted);
							if (errorsResulted > 0) continue;

							RecognizeSnippets(checkingSnippets, statsHelper, GetConfigForRule, out itemsProcessed, out errorsResulted);
							if (errorsResulted == 0)
							{
								rules.UpdateConfigInDB(ruleId, GetConfigForRule(ruleId));
								log.DebugFormat("OCR learning of the current rule finished. RuleId: {0} Iterations: {1} Time: {2} mins", ruleId, iterations, (DateTime.Now - processStarted).TotalMinutes);
								isLearningSuccessful = true;
								statsHelper.AddStat(StatsTypeEnum.ElapsedMinutes, Convert.ToInt32(Math.Floor((DateTime.Now - processStarted).TotalMinutes)));
								break;
							}
						}
					}
					log.DebugFormat("OCR processing finished for rule {0}", ruleId);
					log.DebugFormat("Training stats: {0} timeouts from {1}, max length: {2}, average length: {3}", OcrEngineStatsHelper.GetTimeouts(), OcrEngineStatsHelper.GetTotal(), OcrEngineStatsHelper.GetMax(), OcrEngineStatsHelper.GetAverage());
					if (TimelySendingEmail)
					{
						var emailHelper = new OcrEmailHelper();
						emailHelper.SendEmail(statsHelper, ruleId, isLearningSuccessful);
					}
				}

				log.Debug("OCR process just finished");
			}
		}

		private OcrConfig GetConfigForRule(int ruleId)
		{
			OcrConfig config;
			if (!successMetric.TryGetValue(ruleId, out config))
			{
				config = GetReferenceMetric(ruleId);
			}
			return config;
		}

		OcrConfig GetReferenceMetric(int ruleId)
		{
			var refSnippet = GetReferenceSnippet(ruleId);
			if (refSnippet == null) return null;
			return GetReferenceConfig(refSnippet);
		}

		private void RecognizeSnippets(List<SnippetExt> snippets, OcrStatsHelper statsHelper, Func<int, OcrConfig> getRefMetric, out int itemsProcessed, out int errors)
		{
			if (snippets.Count == 0)
			{
				itemsProcessed = errors = 0;
				log.Debug("OCR Recognize: no snippets");
				return;
			}
			log.Debug("OCR Recognize Snippets");
			statsHelper.ResetStorage();
			itemsProcessed = errors = 0;

			foreach (var item in snippets)
			{
				log.Debug("itemGuid=" + item.Guid);
				var configuration = getRefMetric(item.RuleId);
				if (configuration == null) continue;
				contentProcessor = new RecognitionProcessor(configuration.ContentRegex, configuration.IgnoreCase);
				using (TesseractEngine engine =
					new TesseractEngine(TesseractEngineEx.TesseractAPIPath, configuration.Language,
						EngineMode.Default))
				{
					using (var image = ImageHelper.ConvertByteArrayToImage(item.ImageData))
					{
						if (!statsHelper.SampleStorage.TrySet(image, item)) continue; // ezt ki kell majd igazítani, így semmi értelme
						using (Bitmap clone = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb))
						{
							using (Graphics gr = Graphics.FromImage(clone))
							{
								gr.DrawImage(image, new Rectangle(0, 0, clone.Width, clone.Height));
								string testReadString = RecognitionService.Recognize(clone, new TesseractEngineEx(configuration.Language));
								using (var bitmap = configuration.Transform(clone, new Rectangle(Point.Empty, image.Size)))
								{
									using (var p = PixConverter.ToPix(bitmap))
									using (var page = engine.Process(p, PageSegMode.Auto))
									{
										var r = page.GetText().Trim('\n', '\r', ' ', '\t');
										log.DebugFormat("OCR learning page.GetText().Trim(): {0} expected: {1} alternative method: {2}", r, item.Content, testReadString);
										if (!contentProcessor.Process(r).Equals(contentProcessor.Process(item.Content), StringComparison.Ordinal))
											statsHelper.SampleStorage.AddError(item.Guid, r);
										++itemsProcessed;
									}
								}
							}
						}
					}
				}
			}

			errors = statsHelper.SampleStorage.ErrorCounter;
		}

		private OcrConfig GetReferenceConfig(Snippet refSnippet)
		{
			log.Debug("OCR getting reference config");
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				OcrConfig config;
				string ruleParameterString = context.GetPluginParameter(refSnippet.RuleId); // key=OcrConfig;...
				if (ruleParameterString == null)
				{
					log.ErrorFormat("Rule paramter cannot be empty. RuleId is " + refSnippet.RuleId);
					throw new ArgumentException();
				}

				if (!ruleParameterString.EndsWith(";")) ruleParameterString += ";";
				var regex = new Regex(@"((?<key>\w+)=(?<value>([^;]*)));");
				foreach (Match match in regex.Matches(ruleParameterString))
					if (match.Groups["key"].Success && match.Groups["value"].Success)
					{
						JsonHelper.DeserializeData(match.Groups["value"].Value, out config);
						return config;
					}
				return null;
			}
		}
		private bool TimelySendingEmail
		{
			get
			{
#if DEBUG
				return true;
#else
				//in deveploment stage it's good to know about everything
				//return (DateTime.Now - lastOCRRun).Days > 0;
				return true;
#endif
			}
		}

		private void LearningSnippets(OcrRules rules, List<SnippetExt> snippets, OcrRuleParameter ruleParam, OcrStatsHelper statsHelper, out int errors)
		{
			statsHelper.ResetStorage();
			errors = 0;
			log.Debug("OCR Learning Snippets");

			SnippetExt item = snippets.FirstOrDefault();
			if (item == null)
			{
				log.Debug("item null");
				return;
			}

			foreach (var snippet in snippets)
				statsHelper.SampleStorage.TrySet(ImageHelper.ConvertByteArrayToImage(snippet.ImageData), snippet);

			// setup parameters
			if (ruleParam == null)
			{
				log.Debug("ruleParam null");
				return;
			}

			var config = new OcrConfiguration
			{
				Language = ruleParam.Language,
				HorizontalAlign = HorizontalAlign.Stretch,
				VerticalAlign = VerticalAlign.Stretch
			};
			contentProcessor = new RecognitionProcessor(ruleParam.OcrConfig2DataAccordingToRuleKey.ContentRegex, ruleParam.OcrConfig2DataAccordingToRuleKey.IgnoreCase);
			var samples = statsHelper.SampleStorage.GetSamples(); //itt több szabály esetén felgyűlnek a snippetek, nem?
			var learningText = LearningHelper.GetLearningText(ruleParam.CharSet);
			var channelMax = Enum.GetValues(typeof(ImageHelper.DesaturateMode)).Cast<int>().Max() + 1;
			var fontFamilyMax = Enum.GetValues(typeof(LearningHelper.OcrFontFamily)).Cast<int>().Max() + 1;
			var lowerLimits = new DenseVector(new[] { 0.7, 0.7, 2.0, 0.0, 0.0, 0.0, 0.7, 0.7, 0.0, 0.0, 1.0, 0.0, 8.0, 0.0 });
			var upperLimits = new DenseVector(new[]
				{ 1.3, 1.3, 4.0, 256.0, channelMax, 5.0, 1.3, 1.3, channelMax, 256.0, 4.0, 5.0, 48.0, fontFamilyMax });

			// prepare for learning
			DenseVector denseVector = new DenseVector(ruleParam.OcrConfigurationDataAccordingToRuleKey.EngineParameters);

			var vector = denseVector;
			Func<DenseVector, OcrLearnMetric> p = (x) => OcrLearnMetric.Create(x,
				config,
				learningText,
				samples,
				LearningHelper.GetFont((LearningHelper.OcrFontFamily)(int)x[13], (float)x[12]));

			var swarm = ParticleSwarmFactory.Minimize(
				(x) => p(x),
				OcrLearnMetric.Evaluate,
				lowerLimits,
				upperLimits);

			Stopwatch sw = Stopwatch.StartNew();
			while (true)
			{
				swarm.Iterate();
				log.Debug("TESZT Learning iterations: " + swarm.Iterations);
				errors = (int)Evaluate(swarm);
				log.Debug("OCR errors: " + errors);
				if (errors == 0)
				{
					var swarmState = swarm.GetState();
					denseVector = swarmState.BestPosition;
					AddToSuccessMetric(item.RuleId, rules.UpdateConfig(item.RuleId, ruleParam.CompanyId, ruleParam.ProcessName, denseVector));
					break;
				}

				if (swarm.Iterations > ConfigManager.OcrLearningIterationsMaxCount ||
					sw.Elapsed.TotalMinutes > ConfigManager.OcrLearningTimeoutInMinutes)
				{
					Evaluate(swarm, true, statsHelper.SampleStorage);
					break;
				}
			}

			statsHelper.SampleStorage.RegisterProcessResult();
		}

		private void AddToSuccessMetric(int ruleId, OcrConfig config)
		{
			if (successMetric.ContainsKey(ruleId))
			{
				successMetric[ruleId] = config;
			} 
			else
			{
				successMetric.Add(ruleId, config);
			}
		}

		private double Evaluate(ParticleSwarm<OcrLearnMetric> swarm, bool keepResult = false, SampleStorage storage = null)
		{
			var errorSum = 0.0;
			var best = swarm.BestPosition;
			if (best == null || swarm.Best == null) return 0d;
			foreach (var result in swarm.Best.Results)
			{
				var expected = contentProcessor.Process(result.Expected);
				var evaluated = contentProcessor.Process(result.Result);
				var error = RecognitionService.EvaluateResult(evaluated, expected);
				if (keepResult && storage != null && error > 0)
				{
					storage.AddError(result.Guid, evaluated);
				}
				errorSum += error;
			}
			return errorSum;
		}
		private static Snippet GetReferenceSnippet(int ruleId)
		{
			log.Debug("OCR getting reference snippet");
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var snippets = from e in context.Snippets
							   where e.IsBadData != true &&
									 e.ProcessedAt != null &&
									 e.RuleId == ruleId
							   select e;
				var snippet = snippets.FirstOrDefault();
				if (snippet == null) return null;
				return new SnippetExt
				{
					Guid = snippet.Guid,
					RuleId = snippet.RuleId,
					UserId = snippet.UserId,
					Content = snippet.Content,
					ImageData = snippet.ImageData,
					ProcessName = snippet.ProcessName,
					ProcessedAt = snippet.ProcessedAt
				};
			}
		}
		private IEnumerable<SnippetExt> GetSnippets(Func<ActivityRecorderDataClassesDataContext, IQueryable<Snippet>> query)
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			using (var jcContext = new JobControlDataClassesDataContext())
			{
				foreach (var snippet in query(context))
				{
					var companyId = jcContext.GetUserStatInfoById(snippet.UserId).CompanyId;
					yield return new SnippetExt
					{
						Guid = snippet.Guid,
						RuleId = snippet.RuleId,
						UserId = snippet.UserId,
						CompanyId = companyId,
						Content = snippet.Content,
						ImageData = snippet.ImageData,
						ProcessName = snippet.ProcessName,
						Quality = snippet.Quality
					};
				}
			}
		}

		private IList<SnippetExt> CreateBalancedSnippetCollection(int ruleId, DateTime processStarted)
		{
			Random rnd = new Random();
			var goodSnippets = GetSnippets(context => from e in context.Snippets
													  where e.Content != "" &&
															e.IsBadData != true &&
															e.ProcessedAt < processStarted &&
															e.RuleId == ruleId &&
															e.Quality > 5
													  select e).ToList();
			log.DebugFormat("Good snippets: {0}", goodSnippets.Count);
			goodSnippets = goodSnippets.OrderBy(x => rnd.Next()).ToList();
			var normalSnippets = GetSnippets(context => from e in context.Snippets
														where e.Content != "" &&
															  e.IsBadData != true &&
															  e.ProcessedAt < processStarted &&
															  e.RuleId == ruleId &&
															  e.Quality > 0 &&
															  e.Quality < 6
														select e).ToList();
			log.DebugFormat("Normal Snippets: {0}", normalSnippets.Count);
			normalSnippets = normalSnippets.OrderBy(x => rnd.Next()).ToList();

			var snippetCollection = goodSnippets.Take(MAX_GOOD_ITEMS).ToList();
			snippetCollection.AddRange(normalSnippets.Take(MAX_CHECKING_ITEMS + MAX_LEARNING_ITEMS - snippetCollection.Count));
			return snippetCollection.OrderBy(x => rnd.Next()).ToList();
		}

		private List<SnippetExt> GetRandomLowQualitySnippet()
		{
			Random rnd = new Random();
			var lowQualitySnippets = GetSnippets(context => from e in context.Snippets
															where e.Content != "" &&
																  e.IsBadData != true &&
																  e.Quality == 0
															select e).ToList();
			log.DebugFormat("Found {0} snippet(s) with 0 quality", lowQualitySnippets.Count);
			return lowQualitySnippets.OrderBy(x => rnd.Next()).Take(10).ToList();
		}
	}
}

