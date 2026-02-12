using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Url;
using Tct.ActivityRecorderClient.SystemEvents;
using System.IO;
using System.Diagnostics;

namespace Tct.ActivityRecorderClient.Capturing.Desktop
{
	public class DesktopCaptureMacService : DesktopCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ConstructorInfo cgImgCtor = typeof(CGImage).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new [] {
				typeof(IntPtr) ,
				typeof(bool)
			}, null);
		private static readonly NSString kCGWindowOwnerPID = Dlfcn.GetStringConstant(LibraryMac.CoreGraphics.Handle, "kCGWindowOwnerPID");
		private static readonly NSString kCGWindowBounds = Dlfcn.GetStringConstant(LibraryMac.CoreGraphics.Handle, "kCGWindowBounds");
		private static readonly NSString kCGWindowName = Dlfcn.GetStringConstant(LibraryMac.CoreGraphics.Handle, "kCGWindowName");
		private static readonly NSString kCGWindowOwnerName = Dlfcn.GetStringConstant(LibraryMac.CoreGraphics.Handle, "kCGWindowOwnerName");
		//private static readonly NSString kCGWindowSharingState = Dlfcn.GetStringConstant(LibraryMac.CoreGraphics.Handle, "kCGWindowSharingState");
		private static readonly NSString kNSApplicationProcessIdentifier = new NSString("NSApplicationProcessIdentifier");
		private const int kCGNullWindowID = 0;
		private const string defaultValue = "N\\A";
		private readonly List<IUrlResolver> urlResolvers = new List<IUrlResolver>() { new SafariUrlResolver(), new ChromeUrlResolver(), };

		public DesktopCaptureMacService(ISystemEventsService eventsService)
			: base (eventsService)
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
				return result;
			}
		}

		//this is very slow...
		protected override Bitmap GetScreenShot(Rectangle screenBounds)
		{
			using (var pool = new NSAutoreleasePool())
			{
				try
				{
					var frame = new RectangleF(screenBounds.X, screenBounds.Y, screenBounds.Width, screenBounds.Height);
					IntPtr imgRef = CGWindowListCreateImage(frame, CGWindowListOption.OnScreenOnly, 0, CGWindowImageOption.Default);
					using (var img = cgImgCtor.Invoke(new object[] {imgRef, true}) as CGImage)
					using (var nsbitmap = new NSBitmapImageRep(img))
					{
						var data = nsbitmap.TiffRepresentation.ToArray();
						var mem = new MemoryStream(data);
						return new Bitmap(mem);
					}
				}
				catch (Exception ex)
				{
					log.Error("Unexpected error in GetScreenShot", ex);
					Debug.Fail(ex.Message);
					return null;
				}
			}
		}

		//todo get key window (IsActive)
		//http://stackoverflow.com/questions/7422666/uniquely-identify-active-window-on-os-x
		//https://developer.apple.com/library/mac/#documentation/Cocoa/Reference/ApplicationKit/Classes/NSWorkspace_Class/DeprecationAppendix/AppendixADeprecatedAPI.html#//apple_ref/occ/instm/NSWorkspace/activeApplication
		protected override List<DesktopWindow> GetDesktopWindows()
		{
			using (var pool = new NSAutoreleasePool())
			{
				//var r = NSWorkspace.SharedWorkspace.ActiveApplication;
				//NSApplication.SharedApplication.KeyWindow.WindowNumber;
				//var w =NSApplication.SharedApplication.KeyWindow;
				bool topDockRemoved = false;
				IntPtr native = CGWindowListCopyWindowInfo(CGWindowListOption.OnScreenOnly, kCGNullWindowID);
				try
				{
					if (native == IntPtr.Zero)
						throw new Exception("Unable to get desktop windows");

					NSDictionary[] dicts = NSArray.ArrayFromHandle<NSDictionary>(native);
					var result = new List<DesktopWindow>(dicts.Length);

					for (int i = 0; i < dicts.Length; i++)
					{
						var dict = dicts[i];

						//I'm not sure what this means but Window Server - Shield is not shared but we can retrive the info we want
						//if ((WindowSharing)((NSNumber)dict[kCGWindowSharingState]).IntValue == WindowSharing.None)
						//{
						//	log.Info("Window is not shared " + dict.Handle);
						//	continue;
						//}

						RectangleF bounds;
						CGRectMakeWithDictionaryRepresentation(dict[kCGWindowBounds].Handle, out bounds);
						Rectangle boundsRect = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);

						var currWindow = new DesktopWindow()
						{
							CreateDate = DateTime.UtcNow,
							ProcessName = Convert.ToString(dict[kCGWindowOwnerName]),
							Title = Convert.ToString(dict[kCGWindowName]),
							ProcessId = ((NSNumber)dict[kCGWindowOwnerPID]).IntValue,
							WindowRect = boundsRect,
							ClientRect = boundsRect,
						};

						//this might not be enough in multi-monitor environment
						if (!topDockRemoved && currWindow.ProcessName == "Dock" && currWindow.X == 0 && currWindow.Y == 0)
						{
							topDockRemoved = true; //don't add transparent full window size dock which would hide everything
						}
						else
						{
							result.Add(currWindow);
						}

						dict.Dispose();
					}
					SetActiveWindowHax(result);
					return result;
				}
				finally
				{
					if (native != IntPtr.Zero)
						CFRelease(native);
				}
			}
		}

		private void SetActiveWindowHax(List<DesktopWindow> result)
		{
			DesktopWindow activeWindow = null;
			int activePid;
			if (TryGetActivePid(out activePid))
			{
				activeWindow = result.Where(n => n.ProcessId == activePid && n.ClientRect.Height * n.ClientRect.Width > 500 * 500).FirstOrDefault();
				if (activeWindow == null)
					activeWindow = result.Where(n => n.ProcessId == activePid).FirstOrDefault();
			}
			if (activeWindow == null)
			{
				activeWindow = new DesktopWindow()
				{
					WindowRect = new Rectangle(0, 0, 0, 0),
					ClientRect = new Rectangle(0, 0, 0, 0),
					Title = "",
					ProcessId = 0, //we might set activePid here but since we might get different processname for a pid which is not visible this is not ideal
					CreateDate = DateTime.UtcNow,
				};
				result.Add(activeWindow);
			}
			activeWindow.IsActive = true;
		}

		private bool TryGetActivePid(out int activePid)
		{
			try
			{
				activePid = ((NSNumber)NSWorkspace.SharedWorkspace.ActiveApplication[kNSApplicationProcessIdentifier]).IntValue;
				return true;
			}
			catch (Exception ex)
			{
				activePid = 0;
				log.Error("Unable to get active pid", ex);
				return false;
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

		private enum WindowSharing
		{
			None = 0,
			ReadOnly = 1,
			ReadWrite = 2,
		}

		[DllImport(LibraryMac.CoreGraphics.Path)]
		private static extern IntPtr CGWindowListCopyWindowInfo(CGWindowListOption opt, uint windowId);

		[DllImport(LibraryMac.CoreFundation.Path)]
		private static extern void CFRelease(IntPtr handle);

		[DllImport(LibraryMac.CoreGraphics.Path)]
		private static extern bool CGRectMakeWithDictionaryRepresentation(IntPtr dict, out RectangleF rect);

		[DllImport(LibraryMac.CoreGraphics.Path)]
		private static extern IntPtr CGWindowListCreateImage(RectangleF screenBounds, CGWindowListOption windowOption, uint windowID, CGWindowImageOption imageOption);
	}
}

