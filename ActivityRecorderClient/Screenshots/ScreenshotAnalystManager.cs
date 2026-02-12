using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.UserActivity;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Screenshots
{
	public static class ScreenshotAnalystManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int checkInterval = 1000;
		private static int lastCheck;
		private static bool isStopped;
		private static bool isPreviewMode, isPreviewModeRunning;
		private static string ScreenshotAnalyzerStorePath => "ScreenshotAnalyzer-" + ConfigManager.UserId;
		private static readonly ScreenshotAnalyzerStore Store;
		private static List<Bitmap> otherPreviewImages;
		private static readonly object lockObject = new object();
		private static Action previewCallback;
		private static int? lastLastActivity;
		private static int startActivity;
		private static bool hasActivity;
		private static readonly long duringWorkTimeIdleTicks = ConfigManager.DuringWorkTimeIdleInMins * 60000;

		public static bool IsScreenshotAnalyzerEnabled { get; set; }

		public static bool? IsPictureDetected { get; private set; }

		public static ScreenshotAnalyzerConfigs Configs { get; } = ConfigManager.LocalSettingsForUser.ScreenshotAnalyzerConfigs.DeepClone();

		public static List<Bitmap> PreviewImages;

		public static long ProductiveTime { get => Store.ProductiveTime; private set => Store.ProductiveTime = value; }

		public static long NonProductiveTime { get => Store.NonProductiveTime; private set => Store.NonProductiveTime = value; }

		public static void ProcessImagesIfNecessary(IEnumerable<Bitmap> images)
		{
			lock(lockObject)
			{
				CheckActivity();
				if (!IsScreenshotAnalyzerEnabled || isPreviewModeRunning || Environment.TickCount - lastCheck < checkInterval)  return;
				lastCheck = Environment.TickCount;
				bool isPictureDetected = false;
				if (isPreviewMode && otherPreviewImages == null)
					otherPreviewImages = images.Select(i => new Bitmap(i.Width, i.Height, PixelFormat.Format32bppArgb)).ToList();

				int pos = 0;
				foreach (var image in images)
				{
					var bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb); // Lock the bitmap data
					try
					{
						var backColors = new int[100];
						var countColors = DetectDominantColors(bmpData.Scan0, bmpData.Stride / 4, bmpData.Width, bmpData.Height, backColors);
						var result = new Rectangle1[100];
						var count = DetectEmbeds(bmpData.Scan0, bmpData.Stride / 4, bmpData.Width, bmpData.Height, backColors, countColors, !isPreviewMode, result);
						isPictureDetected |= count > 0;
						if (isPreviewMode)
						{
							var replaceColors = backColors.Take(countColors).ToDictionary(c => c, c => Color.LightGray.ToArgb());
							var bmpBounds = new Rectangle(0, 0, bmpData.Width, bmpData.Height);
							var resData = otherPreviewImages[pos].LockBits(bmpBounds, ImageLockMode.WriteOnly, otherPreviewImages[pos].PixelFormat);

							try
							{
								ReplaceColors(bmpData.Scan0, resData.Scan0, bmpData.Stride / 4, bmpData.Width, bmpData.Height, replaceColors.Select(r => r.Key).Concat(replaceColors.Select(r => r.Value)).ToArray(), replaceColors.Count);
							}
							finally
							{
								otherPreviewImages[pos].UnlockBits(resData);
							}

							using (var g = Graphics.FromImage(otherPreviewImages[pos]))
								foreach (var rectangle in result.Take(count))
								{
									using (var pen = new Pen(Color.Lime, 3))
									{
										g.DrawRectangle(pen, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
									}
								}

							pos++;
						}
					}
					finally
					{
						image.UnlockBits(bmpData); // Unlock the bitmap
					}
				}

				IsPictureDetected = hasActivity ? isPictureDetected : (bool?)null;

				if (Store.LastChange.Date != DateTime.Today)
				{
					var nonProductiveTime = (int)((ScreenshotAnalystManager.NonProductiveTime + 30000L) / 60000L);
					var productiveTime = (int)((ScreenshotAnalystManager.ProductiveTime + 30000L) / 60000L);
					//flush last daily counters
					CaptureCoordinator.Instance.UpdateCollectedItem(Store.LastChange, new Dictionary<string, string>{ { "ProductiveTime", productiveTime.ToString() }, { "NonProductiveTime", nonProductiveTime.ToString() }});
					ProductiveTime = 0L;
					NonProductiveTime = 0L;
					Store.LastChange = DateTime.Now;
				}

				if (hasActivity && !isStopped)
				{
					var now = Environment.TickCount;
					var elapsed = now - startActivity;
					startActivity = now;
					if (isPictureDetected)
						NonProductiveTime += elapsed;
					else
						ProductiveTime += elapsed;
					Store.LastChange = DateTime.Now;
				}

				var tmp = PreviewImages;
				PreviewImages = otherPreviewImages;
				otherPreviewImages = tmp;
				if (isPreviewMode)
				{
					isPreviewModeRunning = true;
					previewCallback?.Invoke();
				}
			}
		}

		private static void CheckActivity()
		{
			var lastActivity = UserActivityWinService.Instance.GetLastActivity();
			if (lastLastActivity != lastActivity)
			{
				lastLastActivity = lastActivity;
				if (hasActivity) return;
				startActivity = Environment.TickCount;
				hasActivity = true;
				return;
			}

			if (Environment.TickCount - lastActivity < duringWorkTimeIdleTicks) return;
			hasActivity = false;
		}

		public static void StartPreview(Action callback)
		{
			lock (lockObject)
			{
				if (isPreviewMode) return;
				isPreviewMode = true;
				previewCallback = callback;
				lastCheck = 0;
			}
		}

		public static void StopPreview()
		{
			lock (lockObject)
			{
				isPreviewMode = isPreviewModeRunning = false;
				lastCheck = 0;
			}
		}

		static ScreenshotAnalystManager()
		{
			try
			{
				if (IsolatedStorageSerializationHelper.Exists(ScreenshotAnalyzerStorePath) && IsolatedStorageSerializationHelper.Load(ScreenshotAnalyzerStorePath, out Store))
				{
					if (Store.ProductiveTime > TimeSpan.FromDays(1).TotalMilliseconds || Store.NonProductiveTime > TimeSpan.FromDays(1).TotalMilliseconds)
					{
						Store.ProductiveTime /= TimeSpan.TicksPerMillisecond;
						Store.NonProductiveTime /= TimeSpan.TicksPerMillisecond;
					}
				}
				else
				{
					Store = new ScreenshotAnalyzerStore();
				}

				UpdateConfigs();
			}
			catch (Exception ex)
			{
				log.Error("static init failed", ex);
			}
		}

		public static void Start()
		{
			if (!isStopped || !IsScreenshotAnalyzerEnabled) return;
			log.Debug("Started");
			isStopped = false;
			startActivity = Environment.TickCount;
		}

		public static void Stop()
		{
			if (isStopped || !IsScreenshotAnalyzerEnabled) return;
			log.Debug("Stopped");
			isStopped = true;
			if (Store != null) IsolatedStorageSerializationHelper.Save(ScreenshotAnalyzerStorePath, Store);
		}

		public static void UpdateConfigs()
		{
			ConfigManager.LocalSettingsForUser.ScreenshotAnalyzerConfigs = Configs.DeepClone();
			SetConfigs(Configs);
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct Rectangle1
		{
			public int Left, Top, Width, Height;
		}

		[DllImport("ImageTools.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int DetectEmbeds(IntPtr addr, int stride, int width, int height, int[] colors, int colorsLen, bool stopAtFirst, [Out] Rectangle1[] result);

		[DllImport("ImageTools.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int DetectDominantColors(IntPtr scan0, int stride, int width, int height, int[] result);

		[DllImport("ImageTools.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ReplaceColors(IntPtr srcScan0, IntPtr resScan0, int stride, int width, int height, int[] replaceColors, int count);

		[DllImport("ImageTools.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void SetConfigs(ScreenshotAnalyzerConfigs configs);
	}

	[Serializable]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	[StructLayout(LayoutKind.Sequential)]
	public class ScreenshotAnalyzerConfigs
	{
		public static ScreenshotAnalyzerConfigs Default = new ScreenshotAnalyzerConfigs
		{
			BackgroundColorPercent = 2,
			SimilarColorDistanceP2 = 200,
			FillAreaRatio = 60,
			SizeMinPixels = 100,
			StepPixels = 20,
			DetColorStepPixels = 20,
			AspectRatioLimit = 5,
			IndividualColorsLimit = 50,
		};
		public int BackgroundColorPercent;
		public int SimilarColorDistanceP2;
		public int FillAreaRatio;
		public int SizeMinPixels;
		public int StepPixels;
		public int DetColorStepPixels;
		public int AspectRatioLimit;
		public int IndividualColorsLimit;

		public override string ToString()
		{
			return $"{nameof(BackgroundColorPercent)}: {BackgroundColorPercent}, {nameof(SimilarColorDistanceP2)}: {SimilarColorDistanceP2}, {nameof(FillAreaRatio)}: {FillAreaRatio}, {nameof(SizeMinPixels)}: {SizeMinPixels}, {nameof(StepPixels)}: {StepPixels}, {nameof(DetColorStepPixels)}: {DetColorStepPixels}, {nameof(AspectRatioLimit)}: {AspectRatioLimit}, {nameof(IndividualColorsLimit)}: {IndividualColorsLimit}";
		}
	}

	[Serializable]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ScreenshotAnalyzerStore
	{
		public long ProductiveTime { get; set; }
		public long NonProductiveTime { get; set; }
		public DateTime LastChange { get; set; }
	}

}
