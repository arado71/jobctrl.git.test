using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public partial class NotificationWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors

		// Called when created from unmanaged code
		public NotificationWindowController(IntPtr handle) : base (handle)
		{
			Initialize();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public NotificationWindowController(NSCoder coder) : base (coder)
		{
			Initialize();
		}

		// Call to load from the XIB/NIB file
		public NotificationWindowController() : base ("NotificationWindow")
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
			this.ShouldCascadeWindows = false;
		}
		
		#endregion

		//strongly typed window accessor
		public new NotificationWindow Window
		{
			get
			{
				return (NotificationWindow)base.Window;
			}
		}

		public string Title
		{
			get { return Window.Title; }
			set { Window.Title = value; }
		}

		public string Body
		{
			get { return txtBody.Value; }
			set { txtBody.Value = value; }
		}

		public NSColor Color
		{
			get { return vwScroll.BackgroundColor; }
			set { vwScroll.BackgroundColor = value; }
		}

		public string Key { get; set; }

		private NSTimer closeTimer;

		public void ShowWindow(TimeSpan duration)
		{
			if (duration > TimeSpan.Zero)
			{
				closeTimer = NSTimer.CreateScheduledTimer(duration, Close);
				NSRunLoop.Current.AddTimer(closeTimer, NSRunLoopMode.Common);
			}
			ShowWindow(this);
			Window.OrderFrontRegardless();
		}

		public override void Close()
		{
			if (closeTimer != null)
			{
				if (closeTimer.IsValid)
					closeTimer.Invalidate();
				closeTimer.Dispose();
				closeTimer = null;
			}
			base.Close();
		}
	}
}

