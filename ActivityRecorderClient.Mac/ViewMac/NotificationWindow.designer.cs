// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.ViewMac
{
	[Register ("NotificationWindowController")]
	partial class NotificationWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextView txtBody { get; set; }

		[Outlet]
		MonoMac.AppKit.NSScrollView vwScroll { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (txtBody != null) {
				txtBody.Dispose ();
				txtBody = null;
			}

			if (vwScroll != null) {
				vwScroll.Dispose ();
				vwScroll = null;
			}
		}
	}

	[Register ("NotificationWindow")]
	partial class NotificationWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
