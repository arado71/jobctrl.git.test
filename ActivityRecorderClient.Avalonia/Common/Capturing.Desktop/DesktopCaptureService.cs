using Avalonia.Media.Imaging;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;
//using Tct.ActivityRecorderClient.Screenshots;
using Tct.ActivityRecorderClient.SystemEvents;
using SkiaSharp;
using System.Drawing;

namespace Tct.ActivityRecorderClient.Capturing.Desktop
{
	public abstract class DesktopCaptureService : IDesktopCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly CaptureExtensionKey[] urlPluginKeys = Platform.Factory.GetDefaultParallelPlugins();

		protected const int MaxProcessNameLength = 200;
		protected const int MaxTitleLength = 2000;
		protected const int MaxUrlLength = 2000;

		protected abstract List<Screen> GetAllScreens();
		protected abstract List<DesktopWindow> GetDesktopWindows();
		protected abstract void SetProcessNames(List<DesktopWindow> windowsInfo);
		protected abstract void SetUrls(List<DesktopWindow> windowsInfo);
		protected abstract SKBitmap GetScreenShot(Rectangle screenBounds);

		private volatile bool isLocked;
		protected bool IsLocked
		{
			get { return isLocked; }
			private set { isLocked = value; }
		}

		private readonly ISystemEventsService systemEventsService;
		private readonly IPluginCaptureService pluginCaptureService;
		private static readonly Dictionary<string, ClientDataCollectionSettings> dataCollectionSettingsDictionary;

		protected DesktopCaptureService(ISystemEventsService eventsService, IPluginCaptureService pluginCaptureSvc)
		{
			pluginCaptureService = pluginCaptureSvc;
			systemEventsService = eventsService;
			if (systemEventsService == null) return;
			systemEventsService.SessionSwitch += SystemEventsServiceSessionSwitch;
			PluginCompositionBase.SetPluginCaptureService(pluginCaptureSvc);
		}

		static DesktopCaptureService()
		{
			dataCollectionSettingsDictionary = new Dictionary<string, ClientDataCollectionSettings>(StringComparer.OrdinalIgnoreCase);
			initializeDataCollectionSettingsDictionary();
		}

		private static void initializeDataCollectionSettingsDictionary()
		{
			dataCollectionSettingsDictionary.Add("Url", ClientDataCollectionSettings.Url);

			dataCollectionSettingsDictionary.Add("Title", ClientDataCollectionSettings.WindowTitle);

			dataCollectionSettingsDictionary.Add("From", ClientDataCollectionSettings.EmailAddress);
			dataCollectionSettingsDictionary.Add("FromEmail", ClientDataCollectionSettings.EmailAddress);
			dataCollectionSettingsDictionary.Add("Recipients", ClientDataCollectionSettings.EmailAddress);
			dataCollectionSettingsDictionary.Add("RecipientsEmail", ClientDataCollectionSettings.EmailAddress);
			dataCollectionSettingsDictionary.Add("To", ClientDataCollectionSettings.EmailAddress);
			dataCollectionSettingsDictionary.Add("ToEmail", ClientDataCollectionSettings.EmailAddress);
			dataCollectionSettingsDictionary.Add("emailfrom", ClientDataCollectionSettings.EmailAddress);
			dataCollectionSettingsDictionary.Add("emailto", ClientDataCollectionSettings.EmailAddress);
			dataCollectionSettingsDictionary.Add("subject", ClientDataCollectionSettings.EmailSubject);

			dataCollectionSettingsDictionary.Add("DocumentFullName", ClientDataCollectionSettings.DocumentNameAndPath);
			dataCollectionSettingsDictionary.Add("DocumentFileName", ClientDataCollectionSettings.DocumentNameAndPath);
			dataCollectionSettingsDictionary.Add("DocumentPath", ClientDataCollectionSettings.DocumentNameAndPath);

			dataCollectionSettingsDictionary.Add("ProcessName", ClientDataCollectionSettings.ProcessName);
		}

		private void SystemEventsServiceSessionSwitch(object sender, Forms.SessionSwitchEventArgs e)
		{
			switch (e.Reason)
			{
				case Forms.SessionSwitchReason.SessionLock:
					IsLocked = true;
					break;
				case Forms.SessionSwitchReason.SessionUnlock:
					IsLocked = false;
					break;
				default:
					return;
			}
		}

		public DesktopCapture GetDesktopCapture(bool takeScreenShot)
		{
			var windows = GetDesktopWindows();
			var screens = GetAllScreens();
			// TODO: mac
			if (takeScreenShot/* || ScreenshotAnalystManager.IsScreenshotAnalyzerEnabled*/)
			{
				foreach (var screen in screens)
				{
					screen.OriginalScreenImage = GetScreenShot(screen.Bounds);
				}
				//ScreenshotAnalystManager.ProcessImagesIfNecessary(screens.Select(s => s.OriginalScreenImage).ToArray());
				if (!takeScreenShot)
				{
					foreach (var screen in screens)
					{
						screen.OriginalScreenImage.Dispose();
						screen.OriginalScreenImage = null;
					}
				}
			}
			TruncateMaximizedWindowsToScreen(screens, windows);
			UpdateVisibilityInfo(screens, windows);
			var globalFormatVariables = new Dictionary<string, string>();
			//On Mac we cannot resolve urls for individual windows (atm.), we have to know all windows
			SetProcessNames(windows); //the order is crucial as we cannot resolve the Url before knowing the ProcessName (we could force this with interfaces but that would complicate and slow down things)
			if (!ConfigManager.AsyncPluginsEnabled)
			{
				SetUrls(windows); //we don't want resolve urls for invisible windows! (but we have to know them because of the mac...)
			}

			pluginCaptureService.SetCaptureExtensions(windows, ShouldCaptureWindow, globalFormatVariables);
			if (ConfigManager.AsyncPluginsEnabled)
			{
				SetUrlsFromInternalPlugins(windows);
			}

			windows.RemoveAll(n => !ShouldCaptureWindow(n)); //we don't want to upload invisible windows (one exception is the active window which includes the injected Idle/Locked window)
			TruncateLongStrings(windows); //sending too long strings would require config limit changes and we would throw them away on the server side anyway
			return new DesktopCapture() { Screens = screens, DesktopWindows = windows, GlobalVariables = globalFormatVariables };
		}

		private void SetUrlsFromInternalPlugins(List<DesktopWindow> windowInfos)
		{
			foreach (var desktopWindow in windowInfos)
			{
				SetUrlFromInternalPlugins(desktopWindow);
			}
		}

		private void SetUrlFromInternalPlugins(DesktopWindow desktopWindow)
		{
			foreach (var plugin in urlPluginKeys)
			{
				string url;
				if (desktopWindow.CaptureExtensions != null && desktopWindow.CaptureExtensions.TryGetValue(plugin, out url))
				{
					desktopWindow.Url = url;
					return;
				}
			}
		}

		protected bool ShouldCaptureWindow(DesktopWindow windowInfo)
		{
			return true; //Since we have WindowScopeType.Any we have to process all windows
		}

		public static int RemoveAllWindowsWhichAreNotSentToServer(DesktopCapture desktopCapture)
		{
			if (desktopCapture == null || desktopCapture.DesktopWindows == null) return 0;
			return desktopCapture.DesktopWindows.RemoveAll(n => n.VisibleClientArea == 0 && !n.IsActive);
		}

		private static void TruncateMaximizedWindowsToScreen(IList<Screen> screens, IList<DesktopWindow> windows)
		{
			foreach (var windowInfo in windows)
			{
				if (!windowInfo.IsMaximized) continue;
				var middleClient = new Point(windowInfo.ClientRect.X + windowInfo.ClientRect.Width / 2,
					windowInfo.ClientRect.Y + windowInfo.ClientRect.Height / 2);
				foreach (var screen in screens)
				{
					if (!screen.Bounds.Contains(middleClient)) continue;
					windowInfo.WindowRect = Rectangle.Intersect(windowInfo.WindowRect, screen.Bounds); //ClientRect might be outside from WindowRect now
					windowInfo.ClientRect = Rectangle.Intersect(windowInfo.ClientRect, windowInfo.WindowRect); //truncate ClientRect too
					break;
				}
			}
		}

		private static void TruncateLongStrings(List<DesktopWindow> windows)
		{
			foreach (var windowInfo in windows)
			{
				windowInfo.ProcessName = Truncate(windowInfo.ProcessName, MaxProcessNameLength);
				windowInfo.Title = Truncate(windowInfo.Title, MaxTitleLength);
				windowInfo.Url = Truncate(windowInfo.Url, MaxUrlLength);
			}
		}

		private static string Truncate(string value, int maxValue)
		{
			if (value == null || value.Length <= maxValue) return value;
			return value.Substring(0, maxValue);
		}

		public static void UpdateVisibilityInfo(IList<Screen> screens, IList<DesktopWindow> windows)
		{
			using var regionVisibleScreen = new SKRegion();

			foreach (var screen in screens)
			{
				var rect = screen.Bounds.ToRectI();
				regionVisibleScreen.Op(rect, SKRegionOperation.Union);
			}

			// Process windows in Z-order
			foreach (var windowInfo in windows)
			{
				var rectClient = windowInfo.ClientRect.ToRectI();
				var rectWindow = windowInfo.WindowRect.ToRectI();

				windowInfo.ClientArea = rectClient.Width * rectClient.Height;

				if (windowInfo.ClientArea > 0)
				{
					using var regionClient = new SKRegion(rectClient);

					// Intersect client region with visible screen
					regionClient.Op(regionVisibleScreen, SKRegionOperation.Intersect);
					windowInfo.VisibleClientArea = GetArea(regionClient);

					// Exclude window area from remaining visible screen
					regionVisibleScreen.Op(rectWindow, SKRegionOperation.Difference);
				}
			}
		}

		private static int GetArea(SKRegion region)
		{
			using var iterator = region.CreateRectIterator();
			int total = 0;

			while (iterator.Next(out SKRectI rect))
				total += rect.Width * rect.Height;

			return total;
		}

		public static void EncodeImages(DesktopCapture desktopCapture)
		{
			if (desktopCapture.Screens == null) return;
			foreach (var screen in desktopCapture.Screens)
			{
				if (screen.OriginalScreenImage == null) continue;
				try
				{
					using (var img = screen.OriginalScreenImage)
					{
						string extension;
						screen.OriginalScreenImage = null;
#if EncodeTransmissionScreen
						screen.ScreenShot = Screenshots.ScreenshotEncoderHelper.EncodeImage(img, out extension); //this is a png
						screen.Extension = extension;
						screen.EncodeBitmapId = Screenshots.ScreenshotEncoderHelper.GetNextId();
						screen.EncodeJpgQuality = ConfigManager.JpegQuality; //save the quality when screen is created not when sent...
#else
						screen.ScreenShot = EncodeImage(img, out extension);
						screen.Extension = extension;
#endif
					}
				}
				catch (Exception ex)
				{
					log.ErrorAndFail("Unexpected error in EncodeImages", ex);
				}
			}
		}

		protected static byte[] EncodeImage(SKBitmap source, out string extension)
		{
			var quality = ConfigManager.JpegQuality;
			var scale = ConfigManager.JpegScalePct / 100f;

			// Scale the image
			using var scaled = ScaleByPercent(source, scale);

			// Encode as JPEG with quality
			using var stream = new MemoryStream();
			using var image = SKImage.FromBitmap(scaled);
			var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);

			data.SaveTo(stream);
			extension = "jpg";
			return stream.ToArray();
		}

		public static SKBitmap ScaleByPercent(SKBitmap bitmap, float percent)
		{
			int newWidth = (int)(bitmap.Width * percent);
			int newHeight = (int)(bitmap.Height * percent);

			var scaled = new SKBitmap(newWidth, newHeight, bitmap.ColorType, bitmap.AlphaType);
			using var canvas = new SKCanvas(scaled);
			var destRect = new SKRect(0, 0, newWidth, newHeight);
			var srcRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
			var sampling = new SKSamplingOptions(SKFilterMode.Nearest);

			using var srcImage = SKImage.FromBitmap(bitmap);
			canvas.Clear(SKColors.Transparent);
			canvas.DrawImage(srcImage, destRect, sampling);
			canvas.Flush();

			return scaled;
		}

		public static void FilterCapture(DesktopCapture desktopCapture, ClientDataCollectionSettings? clientDataCollectionSettings)
		{
			if (clientDataCollectionSettings == null) return;
			foreach (var desktopWindow in desktopCapture.DesktopWindows)
			{
				if (!clientDataCollectionSettings.Value.HasFlag(ClientDataCollectionSettings.WindowTitle))
				{
					desktopWindow.Title = null;
				}
				if (!clientDataCollectionSettings.Value.HasFlag(ClientDataCollectionSettings.Url))
				{
					desktopWindow.Url = null;
				}
				if (!clientDataCollectionSettings.Value.HasFlag(ClientDataCollectionSettings.ProcessName))
				{
					desktopWindow.ProcessName = null;
				}
				if (desktopWindow.CaptureExtensions == null) continue;
				List<CaptureExtensionKey> removableKeys = new List<CaptureExtensionKey>();
				foreach (var captureExtensionKey in desktopWindow.CaptureExtensions.Keys)
				{
					ClientDataCollectionSettings setting;
					if (dataCollectionSettingsDictionary.TryGetValue(captureExtensionKey.Key, out setting))
					{
						if (!clientDataCollectionSettings.Value.HasFlag(setting))
						{
							removableKeys.Add(captureExtensionKey);

						}
					}
				}
				foreach (var removableKey in removableKeys)
				{
					desktopWindow.CaptureExtensions.Remove(removableKey);
				}
			}
		}

		private int isDisposed;
		public void Dispose()
		{
			if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
			if (systemEventsService == null) return;
			systemEventsService.SessionSwitch -= SystemEventsServiceSessionSwitch;
			//systemEventsService.Dispose(); //we don't own this
		}
	}
}
