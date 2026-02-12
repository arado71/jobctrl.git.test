using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public partial class SetIntWindow : MonoMac.AppKit.NSWindow
	{
		#region Constructors

		// Called when created from unmanaged code
		public SetIntWindow(IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public SetIntWindow(NSCoder coder) : base (coder)
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
		}
		
		#endregion

		public bool ShouldUseValue{ get; private set; }

		private int mValue;

		public int Value
		{
			get { return mValue; }
			set
			{
				mValue = value;
				txtValue.StringValue = value.ToString();
			}
		}

		public string Description { get { return lblDescription.StringValue; } set { lblDescription.StringValue = value; } }

		public string ValueTitle { get { return lblValue.StringValue; } set { lblValue.StringValue = value; } }

		public override void AwakeFromNib()
		{
			this.Center();
			txtValue.Changed += HandleTxtValueChanged;
			btnCancel.Activated += (sender, e) => this.PerformClose(this);
			btnOk.Activated += HandleBtnOkActivated;
			btnCancel.Title = Labels.Cancel;
			btnOk.Title = Labels.Ok;
		}

		private void HandleTxtValueChanged(object sender, EventArgs e)
		{
			txtValue.StringValue = Regex.Replace(txtValue.StringValue, "[^0-9]", "");
		}

		private void HandleBtnOkActivated(object sender, EventArgs e)
		{
			if (txtValue.StringValue == "")
				return;
			ShouldUseValue = true;
			Value = txtValue.IntValue;
			this.PerformClose(this);
		}

		public void Show()
		{
			this.MakeKeyAndOrderFront(this);
			NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
		}
	}
}

