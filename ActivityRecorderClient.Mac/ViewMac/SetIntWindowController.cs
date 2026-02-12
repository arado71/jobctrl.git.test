using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public partial class SetIntWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public SetIntWindowController(IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public SetIntWindowController(NSCoder coder) : base (coder)
		{
			Initialize();
		}
		
		// Call to load from the XIB/NIB file
		public SetIntWindowController() : base ("SetIntWindow")
		{
			Initialize();
		}
		
		// Shared initialization code
		void Initialize()
		{
		}
		
		#endregion
		
		//strongly typed window accessor
		public new SetIntWindow Window
		{
			get
			{
				return (SetIntWindow)base.Window;
			}
		}
	}
}

