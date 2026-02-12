// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.ViewMac
{
	[Register ("WorkDetectorRuleEditSheetController")]
	partial class WorkDetectorRuleEditSheetController
	{
		[Outlet]
		MonoMac.AppKit.NSButton btnOk { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton btnCancel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSFormCell fcProcessRule { get; set; }

		[Outlet]
		MonoMac.AppKit.NSFormCell fcTitleRule { get; set; }

		[Outlet]
		MonoMac.AppKit.NSFormCell fcUrlRule { get; set; }

		[Outlet]
		MonoMac.AppKit.NSPopUpButton puRuleType { get; set; }

		[Outlet]
		MonoMac.AppKit.NSPopUpButton puWorks { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField lblRuleType { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField lblWork { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton cbIsRegex { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton cbIsPermanent { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton cbIgnoreCase { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton cbIsEnabled { get; set; }

		[Action ("OkClicked:")]
		partial void OkClicked (MonoMac.Foundation.NSObject sender);

		[Action ("CancelClicked:")]
		partial void CancelClicked (MonoMac.Foundation.NSObject sender);
		
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

			if (fcProcessRule != null) {
				fcProcessRule.Dispose ();
				fcProcessRule = null;
			}

			if (fcTitleRule != null) {
				fcTitleRule.Dispose ();
				fcTitleRule = null;
			}

			if (fcUrlRule != null) {
				fcUrlRule.Dispose ();
				fcUrlRule = null;
			}

			if (puRuleType != null) {
				puRuleType.Dispose ();
				puRuleType = null;
			}

			if (puWorks != null) {
				puWorks.Dispose ();
				puWorks = null;
			}

			if (lblRuleType != null) {
				lblRuleType.Dispose ();
				lblRuleType = null;
			}

			if (lblWork != null) {
				lblWork.Dispose ();
				lblWork = null;
			}

			if (cbIsRegex != null) {
				cbIsRegex.Dispose ();
				cbIsRegex = null;
			}

			if (cbIsPermanent != null) {
				cbIsPermanent.Dispose ();
				cbIsPermanent = null;
			}

			if (cbIgnoreCase != null) {
				cbIgnoreCase.Dispose ();
				cbIgnoreCase = null;
			}

			if (cbIsEnabled != null) {
				cbIsEnabled.Dispose ();
				cbIsEnabled = null;
			}
		}
	}

	[Register ("WorkDetectorRuleEditSheet")]
	partial class WorkDetectorRuleEditSheet
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
