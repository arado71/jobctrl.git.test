// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.ViewMac
{
	[Register ("WorkDetectorRulesWindowController")]
	partial class WorkDetectorRulesWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTableView tblRules { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton btnRemove { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton btnUp { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton btnDown { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton btnOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton btnCancel { get; set; }

		[Action ("AddClicked:")]
		partial void AddClicked (MonoMac.Foundation.NSObject sender);

		[Action ("RemoveClicked:")]
		partial void RemoveClicked (MonoMac.Foundation.NSObject sender);

		[Action ("UpClicked:")]
		partial void UpClicked (MonoMac.Foundation.NSObject sender);

		[Action ("DownClicked:")]
		partial void DownClicked (MonoMac.Foundation.NSObject sender);

		[Action ("CancelClicked:")]
		partial void CancelClicked (MonoMac.Foundation.NSObject sender);

		[Action ("OkClicked:")]
		partial void OkClicked (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (tblRules != null) {
				tblRules.Dispose ();
				tblRules = null;
			}

			if (btnRemove != null) {
				btnRemove.Dispose ();
				btnRemove = null;
			}

			if (btnUp != null) {
				btnUp.Dispose ();
				btnUp = null;
			}

			if (btnDown != null) {
				btnDown.Dispose ();
				btnDown = null;
			}

			if (btnOk != null) {
				btnOk.Dispose ();
				btnOk = null;
			}

			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}
		}
	}

	[Register ("WorkDetectorRulesWindow")]
	partial class WorkDetectorRulesWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
