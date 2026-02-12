using Avalonia.Media.Imaging;
using log4net;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.SystemEvents;

namespace Tct.ActivityRecorderClient.Capturing.Desktop
{
	public class DesktopCaptureWinService : DesktopCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const string defaultValue = "N\\A";

		private readonly CachedDictionary<Tuple<int, IntPtr>, string> processNameCache = new CachedDictionary<Tuple<int, IntPtr>, string>(TimeSpan.FromSeconds(60), true); //lets hope pids won't be reused in 60 secs
		private readonly CachedDictionary<IntPtr, string> urlCache = new CachedDictionary<IntPtr, string>(TimeSpan.FromSeconds(5), true);

		public DesktopCaptureWinService(ISystemEventsService eventsService, IPluginCaptureService pluginCaptureService)
			: base(eventsService, pluginCaptureService)
		{
		}

		protected override List<Screen> GetAllScreens()
		{
			var result = new List<Screen>();
			//have to collect bounds as fast as possible and then take the screenshots if necessary
			var allScreens = System.Windows.Forms.Screen.AllScreens;
			var screenNum = 0;
			for (int i = 0; i < allScreens.Length; i++)
			{
				//AllScreens might not be accurate according to: http://stackoverflow.com/questions/5020559/screen-allscreen-is-not-giving-the-correct-monitor-count
				//check for pseudo monitors (Bounds.Equals is not enough so we need to check for containment, one upper-left corner is probably enough)
				var isPseudo = false;
				for (int j = 0; j < i; j++)
				{
					if (allScreens[i].Bounds.Contains(allScreens[j].Bounds.Location))
					{
						isPseudo = true;
						break;
					}
				}
				if (isPseudo) continue;

				var screen = new Screen()
				{
					Bounds = allScreens[i].Bounds,
					CreateDate = DateTime.UtcNow,
					ScreenNumber = (byte)(screenNum++ % 256),
				};
				result.Add(screen);
			}
			return result;
		}

		protected override List<DesktopWindow> GetDesktopWindows()
		{
			return EnumWindowsHelper.GetWindowsInfo(IsLocked);
		}

		protected override void SetProcessNames(List<DesktopWindow> windowsInfo)
		{
			foreach (var window in windowsInfo)
			{
				if (!ShouldCaptureWindow(window)) continue;
				SetProcessName(window);
			}
		}

		protected override void SetUrls(List<DesktopWindow> windowsInfo)
		{
			foreach (var window in windowsInfo)
			{
				if (!ShouldCaptureWindow(window)) continue;
				SetUrl(window);
			}
		}

		protected void SetProcessName(DesktopWindow windowInfo)
		{
			var key = Tuple.Create(windowInfo.ProcessId, windowInfo.Handle);
			if (!processNameCache.TryGetValue(key, out string processName))
			{
				processName = ResolveProcessNameFromId(windowInfo.ProcessId, windowInfo.Handle);
				if (processName != "")
					processNameCache.Set(key, processName);
				else
				{  // if processName is empty it comes from uwp at first time
					processName = defaultValue;
					processNameCache.Set(key, processName, TimeSpan.FromSeconds(3));
				}
			}
			windowInfo.ProcessName = processName;
		}

		protected void SetUrl(DesktopWindow windowInfo)
		{
			//we might have to forge the result here for usability
			var currUrl = GetUrlFromBrowser(windowInfo.Handle, windowInfo.ProcessName);
			if (currUrl != null)
			{
				urlCache.Set(windowInfo.Handle, currUrl);
			}
			else
			{
				urlCache.TryGetValue(windowInfo.Handle, out currUrl);
			}
			windowInfo.Url = currUrl;
		}

		//when the browser is busy this will return null so one might want to handle it
		//(waiting for valid data might not be an option as it could take some time)
		public static string GetUrlFromBrowser(IntPtr hWnd, string processName)
		{
			string url;
			var browser = UrlHelper.GetBrowserFromProcessName(processName);
			if (ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableUrlCapture)) return "";
			if (browser != Browser.Unknown && UrlHelper.TryGetUrlFromWindow(hWnd, browser, out url))
			{
				return url ?? ""; //null means we cannot get url
			}
			return null;
		}

		//QueryFullProcessImageName and process.MainModule.ModuleName does not work for 64 bit processes if JC is in 32bit (that is why we need AnyCPU in Release atm.) [but QFPIN seems to work for me...]
		//http://social.msdn.microsoft.com/Forums/en/vcgeneral/thread/f652ba04-8819-43dd-b301-a1303ccf3de0
		//this will take some milliseconds so one might want to cache it
		public static string ResolveProcessNameFromId(int processId, IntPtr handle)
		{
			//avoid common exceptions
			switch (processId)
			{
				case -1:
					return "Locked"; //special id indicating that the screen is locked
				case 0:
					return "Idle";
				case 4:
					return "System"; //On XP or later
				default:
					string fileName;
					return (ProcessNameHelper.TryGetProcessName(processId, handle, out fileName)) ? fileName : defaultValue;
			}
		}

		//http://stackoverflow.com/questions/3072349/capture-screenshot-including-semitransparent-windows-in-net
		protected override SKBitmap GetScreenShot(Rectangle screenBounds)
		{
			IntPtr hDesk = IntPtr.Zero, hSrce = IntPtr.Zero, hDest = IntPtr.Zero, hBmp = IntPtr.Zero, hOldBmp = IntPtr.Zero;
			try
			{
				hDesk = WinApi.GetDesktopWindow();
				if (IsIntPtrZero(hDesk, "GetDesktopWindow")) return null;
				hSrce = WinApi.GetWindowDC(hDesk);
				if (IsIntPtrZero(hSrce, "GetWindowDC")) return null;
				hDest = WinApi.CreateCompatibleDC(hSrce);
				if (IsIntPtrZero(hDest, "CreateCompatibleDC")) return null;
				hBmp = WinApi.CreateCompatibleBitmap(hSrce, screenBounds.Width, screenBounds.Height);
				if (IsIntPtrZero(hBmp, "CreateCompatibleBitmap")) return null;
				hOldBmp = WinApi.SelectObject(hDest, hBmp);
				if (IsIntPtrZero(hOldBmp, "SelectObject")) return null;
				var res = WinApi.BitBlt(hDest, 0, 0, screenBounds.Width, screenBounds.Height, hSrce, screenBounds.X, screenBounds.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
				if (IsIntPtrZero(new IntPtr(res), "BitBlt")) return null;
				//http://msdn.microsoft.com/en-us/library/k061we7x.aspx
				//The FromHbitmap method makes a copy of the GDI bitmap; so you can release the incoming GDI bitmap using the GDI DeleteObject method immediately after creating the new Image.
				using var bitmap = Image.FromHbitmap(hBmp);
				// TODO: mac - this is slow, will deal with it later
				using var mem = new MemoryStream();
				bitmap.Save(mem, ImageFormat.Png);
				mem.Position = 0;
				return SKBitmap.Decode(mem.ToArray());

				/*
				//Negative Stride is not working...
				var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
				var skBitmap = new SKBitmap();
				var r = skBitmap.InstallPixels(
					new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Rgb888x, SKAlphaType.Opaque),
					data.Scan0,
					data.Stride);
				bitmap.UnlockBits(data);
				return skBitmap;
				*/
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in GetScreenShot", ex);
				return null;
			}
			finally
			{
				if (hDest != IntPtr.Zero && hOldBmp != IntPtr.Zero) WinApi.SelectObject(hDest, hOldBmp);
				if (hBmp != IntPtr.Zero) WinApi.DeleteObject(hBmp);
				if (hDest != IntPtr.Zero) WinApi.DeleteDC(hDest);
				if (hDesk != IntPtr.Zero && hSrce != IntPtr.Zero) WinApi.ReleaseDC(hDesk, hSrce);
			}
		}

		private static bool IsIntPtrZero(IntPtr ptr, string method)
		{
			if (ptr != IntPtr.Zero) return false;
			var errCode = Marshal.GetLastWin32Error(); //SetLastError = true is needed for this to work properly
			var msg = new System.ComponentModel.Win32Exception(errCode).Message;
			if (errCode == 5 && method == "BitBlt") //this appears only on XP, on Win7 we can make screenshots while locked (at least on my machines)
			{
				log.Debug("Cannot capture screenshot due to error in [" + method + "] (" + errCode + ") probably the screen is locked:" + msg);
			}
			else
			{
				log.Error("Cannot capture screenshot due to error in [" + method + "] (" + errCode + "):" + msg);
				Debug.Fail(msg);
			}
			return true;
		}



	}
}
