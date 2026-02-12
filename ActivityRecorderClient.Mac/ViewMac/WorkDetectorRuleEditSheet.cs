using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public partial class WorkDetectorRuleEditSheet : MonoMac.AppKit.NSPanel
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public WorkDetectorRuleEditSheet(IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public WorkDetectorRuleEditSheet(NSCoder coder) : base (coder)
		{
			Initialize();
		}
		
		// Shared initialization code
		void Initialize()
		{
		}
		
		#endregion
	}
}

