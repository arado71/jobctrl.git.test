using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public partial class NotificationWindow : MonoMac.AppKit.NSWindow
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public NotificationWindow(IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public NotificationWindow(NSCoder coder) : base (coder)
		{
			Initialize();
		}
		
		// Shared initialization code
		void Initialize()
		{
		}
		
		#endregion

		public override bool CanBecomeKeyWindow
		{
			get
			{
				return false;
			}
		}
	}
}

