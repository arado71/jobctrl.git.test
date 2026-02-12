using log4net;
using ObjCRuntime;
using SkiaSharp;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Url;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.SystemEvents;

namespace Tct.ActivityRecorderClient.Capturing.Desktop
{
	public partial class DesktopCaptureMacService : DesktopCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly nint kCGWindowOwnerName = Marshal.ReadIntPtr(Dlfcn.dlsym(LibraryMac.CoreGraphics.Handle, "kCGWindowOwnerName"));
		private static readonly nint kCGWindowName = Marshal.ReadIntPtr(Dlfcn.dlsym(LibraryMac.CoreGraphics.Handle, "kCGWindowName"));
		private static readonly nint kCGWindowOwnerPID = Marshal.ReadIntPtr(Dlfcn.dlsym(LibraryMac.CoreGraphics.Handle, "kCGWindowOwnerPID"));
		private static readonly nint kCGWindowBounds = Marshal.ReadIntPtr(Dlfcn.dlsym(LibraryMac.CoreGraphics.Handle, "kCGWindowBounds"));
		private static readonly nint kCGWindowLayer = Marshal.ReadIntPtr(Dlfcn.dlsym(LibraryMac.CoreGraphics.Handle, "kCGWindowLayer"));

		private const int kCGNullWindowID = 0;
		private const string defaultValue = "N\\A";
		private readonly List<IUrlResolver> urlResolvers = new List<IUrlResolver>() { new SafariUrlResolver(), new ChromeUrlResolver(), };

		public DesktopCaptureMacService(ISystemEventsService eventsService, IPluginCaptureService pluginCaptureSvc)
			: base(eventsService, pluginCaptureSvc)
		{
		}

		protected override List<Screen> GetAllScreens()
		{
			using (var pool = new NSAutoreleasePool())
			{
				var screens = NSScreen.Screens;
				var result = new List<Screen>(screens.Length);
				var screenNum = 0;
				foreach (var screen in screens)
				{
					var bounds = screen.Frame;
					Rectangle boundsRect = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
					var scr = new Screen()
					{
						CreateDate = DateTime.UtcNow,
						Bounds = boundsRect,
						ScreenNumber = (byte)(screenNum++ % 256),
					};
					result.Add(scr);
				}
				// Transform coordinates
				for (var i = 1; i < result.Count; i++)
				{
					var curr = result[i].Bounds;
					var yOffset = curr.Y - result[0].Bounds.Height;
					result[i].Bounds = new Rectangle(curr.X, 0 - yOffset - curr.Height, curr.Width, curr.Height);
				}
				return result;
			}
		}

		protected override SKBitmap GetScreenShot(Rectangle screenBounds)
		{
			var rect = new CGRect(screenBounds.X, screenBounds.Y, screenBounds.Width, screenBounds.Height);
			IntPtr cgImage = CGWindowListCreateImage(rect, CGWindowListOption.OnScreenOnly, 0, CGWindowImageOption.Default);
			if (cgImage == IntPtr.Zero)
				throw new Exception("Failed to capture screen image");

			try
			{
				IntPtr provider = CGImageGetDataProvider(cgImage);
				IntPtr cfData = CGDataProviderCopyData(provider); // CFDataRef
				if (cfData == IntPtr.Zero)
					throw new Exception("Failed to copy image data");

				nint width = CGImageGetWidth(cgImage);
				nint height = CGImageGetHeight(cgImage);
				int rowBytes = (int)CGImageGetBytesPerRow(cgImage);

				var ptr = CFDataGetBytePtr(cfData);
				var info = new SKImageInfo((int)width, (int)height, SKColorType.Bgra8888, SKAlphaType.Premul);

				var bitmap = new SKBitmap();
				bool ok = bitmap.InstallPixels(
					info,
					ptr,
					rowBytes,
					(addr, ctx) => CFRelease((nint)ctx), // release when Skia is done
					cfData                           // ctx passed to release callback
				);

				if (!ok)
				{
					CFRelease(cfData);
					throw new Exception("Failed to install pixels");
				}

				return bitmap;
			}
			finally
			{
				CGImageRelease(cgImage);
			}
		}

		protected override List<DesktopWindow> GetDesktopWindows()
		{
			bool topDockRemoved = false;
			IntPtr infoArray = CGWindowListCopyWindowInfo(CGWindowListOption.OnScreenOnly, kCGNullWindowID);
			try
			{
				if (infoArray == IntPtr.Zero)
					throw new Exception("Unable to get desktop windows");

				var length = CFArrayGetCount(infoArray).ToInt32();
				var result = new List<DesktopWindow>(length);
				var activeFound = false;

				for (int i = 0; i < length; i++)
				{
					var dict = CFArrayGetValueAtIndex(infoArray, i);


					string? owner = CFStringToString(CFDictionaryGetValue(dict, kCGWindowOwnerName));
					string? title = CFStringToString(CFDictionaryGetValue(dict, kCGWindowName));
					int pid = CFNumberToInt(CFDictionaryGetValue(dict, kCGWindowOwnerPID)) ?? 0;
					int layer = CFNumberToInt(CFDictionaryGetValue(dict, kCGWindowLayer)) ?? -1;
					CGRect bounds = default;
					var boundsDict = CFDictionaryGetValue(dict, kCGWindowBounds);
					if (boundsDict != IntPtr.Zero)
						CGRectMakeWithDictionaryRepresentation(boundsDict, out bounds);
					Rectangle boundsRect = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);

					var currWindow = new DesktopWindow()
					{
						CreateDate = DateTime.UtcNow,
						ProcessName = owner,
						Title = title,
						ProcessId = pid,
						WindowRect = boundsRect,
						ClientRect = boundsRect,
					};

					//this might not be enough in multi-monitor environment
					if (!topDockRemoved && currWindow.ProcessName == "Dock" && currWindow.X == 0 && currWindow.Y == 0)
					{
						topDockRemoved = true; //don't add transparent full window size dock which would hide everything
						continue;
					}

					//ignore small windows like tooltips
					if (currWindow.Height < 50)
					{
						continue;
					}

					//ignore ux crap like axAuditService
					if (layer >= 1000 && currWindow.Width > 1000 && currWindow.Height > 1000)
					{
						continue;
					}

					//first window on layer 0 is the top most and should be the actice
					if (layer == 0 && !activeFound)
					{
						activeFound = true;
						currWindow.IsActive = true;
					}

					result.Add(currWindow);
				}
				return result;
			}
			finally
			{
				if (infoArray != IntPtr.Zero)
					CFRelease(infoArray);
			}

		}

		protected override void SetProcessNames(List<DesktopWindow> windowsInfo)
		{
			using (var pool = new NSAutoreleasePool())
			{
				Dictionary<int, NSRunningApplication> runningProcesses = null;
				foreach (var item in windowsInfo)
				{
					if (item.ProcessId == 0) //Unknown active process
					{
						item.ProcessName = "Idle";
					}
					else if (item.ProcessName == "") //we have to get the name for the process
					{
						item.ProcessName = GetProcessNameForPidNoThrow(item.ProcessId, ref runningProcesses);
					}
					else //we have a valid process name just append .app at the end
					{
						item.ProcessName += ".app";
					}
				}
				if (runningProcesses != null)
				{
					foreach (KeyValuePair<int, NSRunningApplication> item in runningProcesses)
					{
						item.Value.Dispose();
					}
				}
			}
		}

		private string GetProcessNameForPidNoThrow(int processId, ref Dictionary<int, NSRunningApplication> runningProcesses)
		{
			try
			{
				if (runningProcesses == null)
				{
					runningProcesses = NSWorkspace.SharedWorkspace.RunningApplications.ToDictionary(n => n.ProcessIdentifier);
				}
				NSRunningApplication app;
				if (runningProcesses.TryGetValue(processId, out app))
				{
					return Uri.UnescapeDataString(Path.GetFileName(app.ExecutableUrl.AbsoluteString)) + ".app"; //this name might be different from the one from kCGWindowOwnerName
				}
				//not all processes are listed there so fallback to .net api
				using (var proc = Process.GetProcessById(processId))
				{
					return proc.ProcessName + ".app";
				}
			}
			catch (Exception ex)
			{
				log.Error("Unexpected error in process name resolving", ex);
				return defaultValue;
			}
		}

		protected override void SetUrls(List<DesktopWindow> windowsInfo)
		{
			using (var pool = new NSAutoreleasePool())
			{
				foreach (var urlResolver in urlResolvers)
				{
					urlResolver.SetUrls(windowsInfo);
				}
			}
		}

		internal enum WindowSharing
		{
			None = 0,
			ReadOnly = 1,
			ReadWrite = 2,
		}

		[LibraryImport(LibraryMac.CoreGraphics.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial IntPtr CGWindowListCopyWindowInfo(CGWindowListOption opt, uint windowId);

		[LibraryImport(LibraryMac.CoreFoundation.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial void CFRelease(IntPtr handle);

		[LibraryImport(LibraryMac.CoreGraphics.Path, StringMarshalling= StringMarshalling.Utf8)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static partial bool CGRectMakeWithDictionaryRepresentation(IntPtr dictRef, out CGRect rect);

		[LibraryImport(LibraryMac.CoreGraphics.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial IntPtr CGWindowListCreateImage(CGRect screenBounds, CGWindowListOption windowOption, uint windowID, CGWindowImageOption imageOption);

		[LibraryImport(LibraryMac.CoreFoundation.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial nint CFArrayGetCount(IntPtr array);

		[LibraryImport(LibraryMac.CoreFoundation.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial IntPtr CFArrayGetValueAtIndex(IntPtr array, nint index);

		[LibraryImport(LibraryMac.CoreFoundation.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial IntPtr CFDictionaryGetValue(IntPtr dict, IntPtr key);

		[LibraryImport(LibraryMac.CoreFoundation.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial IntPtr CFStringGetCStringPtr(IntPtr str, int encoding);

		[LibraryImport(LibraryMac.CoreFoundation.Path, StringMarshalling= StringMarshalling.Utf8)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static partial bool CFStringGetCString(IntPtr theString, IntPtr buffer, long bufferSize, uint encoding);

		[LibraryImport(LibraryMac.CoreFoundation.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial IntPtr CFStringCreateWithCString(IntPtr alloc, byte[] cStr, uint encoding);

		const int uft8Encoding = 0x08000100;
		const int bufferSize = 4096;
		static IntPtr strBuffer = Marshal.AllocHGlobal(bufferSize);
		//no Marshal.FreeHGlobal(buffer); but that is ok
		static string? CFStringToString(IntPtr ptrStr)
		{
			if (ptrStr == IntPtr.Zero)
				return null;
			IntPtr cStr = CFStringGetCStringPtr(ptrStr, uft8Encoding);
			if (cStr != IntPtr.Zero)
				return Marshal.PtrToStringUTF8(cStr);

			var ok = CFStringGetCString(ptrStr, strBuffer, bufferSize, uft8Encoding);
			return ok ? Marshal.PtrToStringUTF8(strBuffer) : null;
		}

		static int? CFNumberToInt(IntPtr number)
		{
			if (number == IntPtr.Zero) return null;
			int value;
			if (CFNumberGetValue(number, 9 /* int32 */, out value)) return value;
			return null;
		}

		[LibraryImport(LibraryMac.CoreFoundation.Path, StringMarshalling= StringMarshalling.Utf8)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static partial bool CFNumberGetValue(IntPtr number, int type, out int value);

		[LibraryImport(LibraryMac.CoreGraphics.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial IntPtr CGDataProviderCopyData(IntPtr provider);

		[LibraryImport(LibraryMac.CoreGraphics.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial IntPtr CGImageGetDataProvider(IntPtr image);

		[LibraryImport(LibraryMac.CoreGraphics.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial void CGImageRelease(IntPtr image);

		[LibraryImport(LibraryMac.CoreGraphics.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial nint CGImageGetWidth(IntPtr image);

		[LibraryImport(LibraryMac.CoreGraphics.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial nint CGImageGetHeight(IntPtr image);

		[LibraryImport(LibraryMac.CoreGraphics.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial nint CGImageGetBytesPerRow(IntPtr image);

		[LibraryImport(LibraryMac.CoreFoundation.Path, StringMarshalling= StringMarshalling.Utf8)]
		internal static partial IntPtr CFDataGetBytePtr(IntPtr theData);

		[StructLayout(LayoutKind.Sequential)]
		internal struct CGRect
		{
			public double X;
			public double Y;
			public double Width;
			public double Height;

			public CGRect(double x, double y, double width, double height)
			{
				X = x; Y = y; Width = width; Height = height;
			}
		}
	}
}

