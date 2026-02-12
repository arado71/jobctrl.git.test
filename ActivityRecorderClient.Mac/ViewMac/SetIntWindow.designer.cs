// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.ViewMac
{
	[Register ("SetIntWindow")]
	partial class SetIntWindow
	{
		[Outlet]
		MonoMac.AppKit.NSButton btnCancel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton btnOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField txtValue { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField lblValue { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField lblDescription { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}

			if (btnOk != null) {
				btnOk.Dispose ();
				btnOk = null;
			}

			if (txtValue != null) {
				txtValue.Dispose ();
				txtValue = null;
			}

			if (lblValue != null) {
				lblValue.Dispose ();
				lblValue = null;
			}

			if (lblDescription != null) {
				lblDescription.Dispose ();
				lblDescription = null;
			}
		}
	}

	[Register ("SetIntWindowController")]
	partial class SetIntWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSButton btnOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton btnCancel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField lblValue { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField lblDescription { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField txtValue { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnOk != null) {
				btnOk.Dispose ();
				btnOk = null;
			}

			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}

			if (lblValue != null) {
				lblValue.Dispose ();
				lblValue = null;
			}

			if (lblDescription != null) {
				lblDescription.Dispose ();
				lblDescription = null;
			}

			if (txtValue != null) {
				txtValue.Dispose ();
				txtValue = null;
			}
		}
	}
}
