using System;
using MonoMac.CoreFoundation;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility
{
	public class AXAttribute
	{
		private static readonly CFString axFocusedWindow = new CFString("AXFocusedWindow");
		private static readonly CFString axWindows = new CFString("AXWindows");
		private static readonly CFString axRole = new CFString("AXRole");
		private static readonly CFString axTitle = new CFString("AXTitle");
		private static readonly CFString axGroup = new CFString("AXGroup");
		private static readonly CFString axChildren = new CFString("AXChildren");
		private static readonly CFString axScroll = new CFString("AXScrollArea");
		private static readonly CFString axWeb = new CFString("AXWebArea");
		private static readonly CFString axUrl = new CFString("AXURL");
		private static readonly CFString axToolbar = new CFString("AXToolbar");
		private static readonly CFString axTextField = new CFString("AXTextField");
		private static readonly CFString axValue = new CFString("AXValue");
		//
		public static readonly AXAttribute FocusedWindow = new AXAttribute(axFocusedWindow);
		public static readonly AXAttribute Windows = new AXAttribute(axWindows);
		public static readonly AXAttribute Role = new AXAttribute(axRole);
		public static readonly AXAttribute Title = new AXAttribute(axTitle);
		public static readonly AXAttribute Group = new AXAttribute(axGroup);
		public static readonly AXAttribute Children = new AXAttribute(axChildren);
		public static readonly AXAttribute Scroll = new AXAttribute(axScroll);
		public static readonly AXAttribute Web = new AXAttribute(axWeb);
		public static readonly AXAttribute Url = new AXAttribute(axUrl);
		public static readonly AXAttribute Toolbar = new AXAttribute(axToolbar);
		public static readonly AXAttribute TextField = new AXAttribute(axTextField);
		public static readonly AXAttribute Value = new AXAttribute(axValue);

		private AXAttribute(CFString attr)
		{
			Handle = attr.Handle;
		}

		public IntPtr Handle { get; private set; }
	}
}

