using System;
using MonoMac.AppKit;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public class TaggableNSMenuItem : NSMenuItem
	{
		public object TagObject { get; set; }

		public TaggableNSMenuItem(string title)
			: base(title)
		{
		}

		public TaggableNSMenuItem(string title, EventHandler handler)
			: base(title, handler)
		{
		}
	}

	public class TaggableNSMenuItem<T> : NSMenuItem
	{
		public T TagObject { get; set; }

		public TaggableNSMenuItem(IntPtr handle)
			: base(handle)
		{
		}

		public TaggableNSMenuItem(string title)
			: base(title)
		{
		}

		public TaggableNSMenuItem(string title, EventHandler handler)
			: base(title, handler)
		{
		}
	}
}

