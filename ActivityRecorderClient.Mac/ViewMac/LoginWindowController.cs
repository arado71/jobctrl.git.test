using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public partial class LoginWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public LoginWindowController(IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public LoginWindowController(NSCoder coder) : base (coder)
		{
			Initialize();
		}
		
		// Call to load from the XIB/NIB file
		public LoginWindowController() : base ("LoginWindow")
		{
			Initialize();
		}
		
		// Shared initialization code
		void Initialize()
		{
		}
		
		#endregion
		
		//strongly typed window accessor
		public new LoginWindow Window
		{
			get
			{
				return (LoginWindow)base.Window;
			}
		}

		public static ConfigManager.LoginData DisplayLoginForm()
		{
			using (var loginCtrl = new LoginWindowController())
			{
				loginCtrl.Window.ShowDialog();
				if (!loginCtrl.Window.IsValid)
					return null;
				return new ConfigManager.LoginData()
				{
					UserId = loginCtrl.Window.UserId,
					UserPassword = loginCtrl.Window.Password,
					RememberMe = loginCtrl.Window.RememberMe,
					AuthData = loginCtrl.Window.AuthData,
				};
			}
		}

		public static ConfigManager.LoginData ShowChangePasswordDialog(ConfigManager.LoginData currentData)
		{
			using (var loginCtrl = new LoginWindowController())
			{
				loginCtrl.Window.RememberMe = currentData.RememberMe;
				loginCtrl.Window.UserId = currentData.UserId;
				loginCtrl.Window.ShowDialog();
				if (!loginCtrl.Window.IsValid)
					return null;
				return new ConfigManager.LoginData()
				{
					UserId = currentData.UserId,
					UserPassword = loginCtrl.Window.Password,
					RememberMe = loginCtrl.Window.RememberMe,
					AuthData = loginCtrl.Window.AuthData,
				};
			}
		}
	}
}

