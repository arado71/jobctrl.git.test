// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.ViewMac
{
	[Register ("LoginWindow")]
	partial class LoginWindow
	{
		[Outlet]
		MonoMac.AppKit.NSButton btnOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton btnCancel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField txtUserId { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField txtPassword { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton cbRememberMe { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField lblUserId { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField lblPassword { get; set; }
		
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

			if (txtUserId != null) {
				txtUserId.Dispose ();
				txtUserId = null;
			}

			if (txtPassword != null) {
				txtPassword.Dispose ();
				txtPassword = null;
			}

			if (cbRememberMe != null) {
				cbRememberMe.Dispose ();
				cbRememberMe = null;
			}

			if (lblUserId != null) {
				lblUserId.Dispose ();
				lblUserId = null;
			}

			if (lblPassword != null) {
				lblPassword.Dispose ();
				lblPassword = null;
			}
		}
	}

	[Register ("LoginWindowController")]
	partial class LoginWindowController
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
