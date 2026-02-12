using System;
using MonoMac.ObjCRuntime;

namespace Tct.ActivityRecorderClient
{
	public static class LibraryMac
	{
		public static class CoreFundation
		{
			public const string Path = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
		}

		public static class AppKit
		{
			public const string Path = "/System/Library/Frameworks/AppKit.framework/AppKit";
		}

		public static class CoreGraphics
		{
			public const string Path = "/System/Library/Frameworks/ApplicationServices.framework/Versions/A/Frameworks/CoreGraphics.framework/CoreGraphics";
			public static IntPtr Handle = Dlfcn.dlopen(Path, 0);
		}

		public static class IOKit
		{
			public const string Path = "/System/Library/Frameworks/IOKit.framework/Versions/A/IOKit";
		}
	}
}

