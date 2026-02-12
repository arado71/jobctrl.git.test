using JobCTRL.Plugins;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	public class PluginOcr : ICaptureExtension, IDisposable
	{
		private class checkedLanguage
		{
			public string Config { get; set; }
			public bool Checked { get; set; }
		}
		private static readonly ILog log = LogManager.GetLogger(typeof(PluginOcr));
		private static readonly string currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		private const string ParamCapture = "Capture";
		private const string ParamMode = "Mode";
		private const string ParamLanguage = "Language";
		private static readonly TimeSpan expiration = new TimeSpan(0, 1, 0);
		private Regex titleMatcher;
		private Regex processNameMatcher;
		private OcrConfig configuration;
		private readonly Dictionary<IntPtr, RecognitionCacheElement> cache = new Dictionary<IntPtr, RecognitionCacheElement>();
		private List<KeyValuePair<string, checkedLanguage>> captures = new List<KeyValuePair<string, checkedLanguage>>();
		private string language = "eng";
		private PluginOcrModeEnum pluginOcrMode = PluginOcrModeEnum.Offline;
		private static List<DeviceInfo> monitors = new Monitors();
		public const string PluginId = "JobCTRL.Ocr";
		private readonly int ruleId;

#if OcrPlugin
		private Tesseract.TesseractEngine engine;
		private readonly object recognizeLock = new object();
		public PluginOcr(PluginStartInfoDetails details)
		{
			log.DebugFormat("Ocr plugin tessdata's location {0}", currentPath);
			if (details != null && details.Rule != null)
				ruleId = details.Rule.ServerId;
		}
		public void Dispose()
		{
			using (engine)
			{
			}

			while (cache.Count > 0)
			{
				RemoveCache(cache.Keys.First());
			}
		}
#else
		public void Dispose() { }
#endif
		public string Id
		{
			get { return PluginId; }
		}

		public IEnumerable<string> GetParameterNames()
		{
			yield return ParamCapture;
			yield return ParamLanguage;
		}
		// called for each incoming paramter
		public void SetParameter(string name, string value)
		{
			if (string.Equals(ParamLanguage, name, StringComparison.OrdinalIgnoreCase))
				language = setLanguage(value);
			else
				if (string.Equals(ParamCapture, name, StringComparison.OrdinalIgnoreCase))
					try
					{
						captures = Compile(value);
						log.Info("OCR configuration compiled");
					}
					catch (Exception e)
					{
						log.Warn("Invalid OCR configuration", e);
						configuration = null;
					}
				else if (string.Equals(ParamMode, name, StringComparison.OrdinalIgnoreCase))
					if (Enum.TryParse(value, true, out pluginOcrMode))
						if (pluginOcrMode == PluginOcrModeEnum.Learning)
							ContributionController.Instance.Start(pluginOcrMode);
		}
		private string setLanguage(string value)
		{
			if (checkLanguageFileExists(value)) return value;
			if (checkLanguageFileExists(ConfigManager.OCRLanguage)) return ConfigManager.OCRLanguage;
			if (checkLanguageFileExists(language)) return language;
			throw new FileNotFoundException(string.Format("OCR Configuration error: neither {0} nor the default (eng) language file does not exists", value));
		}
		private bool checkLanguageFileExists(string name)
		{
			return File.Exists(Path.Combine(currentPath, "tessdata", name + ".traineddata"));
		}
		public IEnumerable<string> GetCapturableKeys()
		{
			return captures.Select(x => x.Key);
		}
		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
#if !OcrPlugin
			yield break;
#endif
			ContributionController.Instance.PopupContributionIfNeeded();
			if (captures == null || captures.Count == 0) yield break;
			if (pluginOcrMode == PluginOcrModeEnum.Offline) yield break;
			foreach (var capture in captures)
			{
				configuration = OcrConfig.FromJson(capture.Value.Config);

				titleMatcher = new Regex(configuration.TitleRegex);
				processNameMatcher = new Regex(configuration.ProcessNameRegex);
				if (configuration == null || titleMatcher == null || processNameMatcher == null) continue;
				var title = GetWindowText(hWnd);
				if (!titleMatcher.IsMatch(title) || !processNameMatcher.IsMatch(processName)) continue;

				string languageCharset = configuration.Language + (int) configuration.CharSet;
				string captureSpecificLanguage;
				if (!capture.Value.Checked)
				{
					captureSpecificLanguage = setLanguage(languageCharset);
					capture.Value.Checked = true;
				}
				else
					captureSpecificLanguage = languageCharset;

				var wRect = EnumWindowsHelper.GetWindowRect(hWnd);
				Stopwatch sw = Stopwatch.StartNew();
				var mHandle = EnumWindowsHelper.MonitorFromPoint(wRect.Location);
				var appMonitor = GetAppMonitorByHandle(mHandle);
				EnumWindowsHelper.WindowInfo appWindow = EnumWindowsHelper.GetWindowInfo(hWnd);
				if (appWindow == null || !appWindow.IsActive) yield break;
				using (var windowImage = GetImageOfInterest(appWindow.Handle, appWindow.ClientRect, configuration.GetAreaOfInterest(), appMonitor))
				{
					if (windowImage == null) continue;
					if (pluginOcrMode == PluginOcrModeEnum.InProgress)
						yield return
							new KeyValuePair<string, string>(capture.Key, GetCachedRecognition(hWnd, windowImage, captureSpecificLanguage));
					else
					{
						if (ruleId != 0)
						{
							string guess = GetCachedRecognition(hWnd, windowImage, captureSpecificLanguage);
							if (configuration.UserContribution)
							{
								ContributionController.Instance.PersistImage(windowImage, ruleId, processName, guess);
								
							}
							else
							{
								ContributionController.Instance.PersistImageReadyToUpload(windowImage, ruleId, processName);
							}
							log.DebugFormat("OCR capture in learning mode: {0} ruleID: {1} TresholdLimit: {2}, UserContribution: {3}", guess, ruleId, configuration.TresholdLimit, configuration.UserContribution);
						}	
					}
					
				}
				log.DebugFormat("OCR capturing '{0}' took {1} ms", capture.Key, sw.ElapsedMilliseconds);
			}
		}

		private DeviceInfo GetAppMonitorByHandle(IntPtr handle)
		{
			var appMonitor = monitors.FirstOrDefault(e => e.Handle == handle);
			if (appMonitor == null)
			{
				monitors = new Monitors();
				appMonitor = monitors.FirstOrDefault(e => e.Handle == handle);
			}
			return appMonitor;
		}

		private static string GetWindowText(IntPtr hWnd)
		{
			int windowTextLength = WinApi.GetWindowTextLength(hWnd) + 1;
			StringBuilder activeWindowText = new StringBuilder(windowTextLength);

			WinApi.GetWindowText(hWnd, activeWindowText, windowTextLength);
			return activeWindowText.ToString();
		}

		private string GetCachedRecognition(IntPtr hWnd, Bitmap image, string lang)
		{
			ClearExpired();

			if (cache.ContainsKey(hWnd))
			{
				var element = cache[hWnd];
				if (!element.ImageEquals(image))
				{
					RemoveCache(hWnd);
				}
			}

			if (!cache.ContainsKey(hWnd))
			{
				cache.Add(hWnd, new RecognitionCacheElement(new Bitmap(image), Recognize(image, lang)));
			}
			else
			{
				cache[hWnd].LastValidDate = DateTime.UtcNow;
			}
#if OCRDEBUG
			log.Debug("Recognized value is " + cache[hWnd].Text);
#endif
			return cache[hWnd].Text;
		}

		private void ClearExpired()
		{
			var keysToRemove = new List<IntPtr>();
			var deadLine = DateTime.UtcNow - expiration;
			foreach (var element in cache)
			{
				if (element.Value.LastValidDate < deadLine)
				{
					keysToRemove.Add(element.Key);
				}
			}

			foreach (var key in keysToRemove)
			{
				RemoveCache(key);
			}
		}

		private void RemoveCache(IntPtr key)
		{
			Debug.Assert(cache.ContainsKey(key));
			cache[key].Dispose();
			cache.Remove(key);
		}

		private string Recognize(Bitmap image, string lang)
		{
			using (var ocrImage = configuration.Transform(image, new Rectangle(Point.Empty, image.Size)))
			{
				if (ocrImage == null) return null;
				var sw = Stopwatch.StartNew();
				var recognized = RecognizeString(ocrImage, lang);
				log.DebugFormat("Ocr recognized \"{0}\" in {1} ms for proc {2}", recognized, sw.Elapsed.TotalMilliseconds, configuration.ProcessNameRegex);
				return recognized;
			}
		}

		private string RecognizeString(Bitmap image, string lang)
		{
#if OcrPlugin
			using (var p = Tesseract.PixConverter.ToPix(image))
			{
				lock (recognizeLock)
				{
					if (engine == null)
						engine = new Tesseract.TesseractEngine(Path.Combine(currentPath, "tessdata"), lang, Tesseract.EngineMode.Default);
					using (var page = engine.Process(p, Tesseract.PageSegMode.Auto))
					{
						return page.GetText().Trim('\n', '\r', ' ', '\t');
					}
				}
			}
#else
			return null;
#endif
		}
		private Bitmap GetImageOfInterest(IntPtr appWindowHandle, Rectangle appWindowWindowRect, Rectangle selectionRectangle, DeviceInfo selectionMonitor)
		{
			Rectangle appWindowRectScaled = EnumWindowsHelper.CorrectCoordinates(appWindowHandle, appWindowWindowRect);
			var X = (int)(appWindowRectScaled.Left * selectionMonitor.HScale);
			var Y = (int)(appWindowRectScaled.Top * selectionMonitor.VScale);
			var scaledRect = new Rectangle(
				X, Y,
				(int)(appWindowRectScaled.Right * selectionMonitor.HScale) - X,
				(int)(appWindowRectScaled.Bottom * selectionMonitor.VScale) - Y);
			// app window full capture
			var bmp = new Bitmap(scaledRect.Width, scaledRect.Height, PixelFormat.Format24bppRgb);
			Graphics graphics = Graphics.FromImage(bmp);
			graphics.CopyFromScreen(scaledRect.Left, scaledRect.Top, 0, 0, new Size(scaledRect.Width, scaledRect.Height), CopyPixelOperation.SourceCopy);
			// scaled rect over the capture
			var area = new Rectangle(
				(int)(selectionRectangle.Left * selectionMonitor.HScale) /*+ (int)(appWindowRectScaled.Left * selectionMonitor.HScale) + selectionMonitor.Screen.Bounds.Left*/,
				(int)(selectionRectangle.Top * selectionMonitor.HScale) /*+ (int)(appWindowRectScaled.Top * selectionMonitor.VScale)*/,
				(int)(selectionRectangle.Width * selectionMonitor.HScale),
				(int)(selectionRectangle.Height * selectionMonitor.VScale));
			// sliced part of capture
			var image = new Bitmap(area.Width, area.Height);
			using (Graphics gr = Graphics.FromImage(image))
			{
				gr.DrawImage(bmp,
					new Rectangle(0, 0, image.Width, image.Height),
					area,
					GraphicsUnit.Pixel);
			}
			//Imager.Showw(image, "XX");
			return image;
		}
		private static List<KeyValuePair<string, checkedLanguage>> Compile(string value)
		{
			List<KeyValuePair<string, checkedLanguage>> result = new List<KeyValuePair<string, checkedLanguage>>();
			if (string.IsNullOrEmpty(value)) throw new ArgumentException("Script cannot be empty");
			if (!value.EndsWith(";")) value += ";";
			var regex = new Regex(@"((?<key>\w+)=(?<value>([^;]*)));");
			foreach (Match match in regex.Matches(value))
			{
				if (match.Groups["key"].Success && match.Groups["value"].Success)
					result.Add(new KeyValuePair<string, checkedLanguage>(
						match.Groups["key"].Value,
						new checkedLanguage { Config = match.Groups["value"].Value }
						)
					);
				else
					log.Error("Invalid configuration. Either `key` or `value` doesn't match", new ArgumentException(value));
			}
			return result;
		}

		private sealed class RecognitionCacheElement : IDisposable
		{
			private Bitmap Image { get; set; }
			public string Text { get; private set; }
			public DateTime LastValidDate { get; set; }

			public RecognitionCacheElement(Bitmap image, string text)
			{
				Image = image;
				LastValidDate = DateTime.UtcNow;
				Text = text;
			}

			public void Dispose()
			{
				Image.Dispose();
			}

			public bool ImageEquals(Bitmap other)
			{
				return ImageStorageCleaner.ImageEquals(Image, other);
			}

			
		}
	}

}
