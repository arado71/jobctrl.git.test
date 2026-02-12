using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.ViewMac
{
	public partial class LoginWindow : MonoMac.AppKit.NSWindow
	{
		public bool IsValid { get; set; }

		public int UserId { get; set; }

		public string Password { get; set; }

		public bool RememberMe { get; set; }

		public AuthData AuthData { get; set; }

		#region Constructors
		
		// Called when created from unmanaged code
		public LoginWindow(IntPtr handle) : base (handle)
		{
			Initialize();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public LoginWindow(NSCoder coder) : base (coder)
		{
			Initialize();
		}
		
		// Shared initialization code
		void Initialize()
		{
			//btnCancel, etc is null...
		}

		#endregion

		private bool isModal;

		public override void AwakeFromNib()
		{
			this.Center();
			this.WillClose += HandleHandleWillClose;
			btnCancel.Activated += (sender, e) => this.PerformClose(this);
			btnOk.Activated += HandleBtnOkActivated;
			txtUserId.Changed += HandleTxtUserIdChanged;
			//Labels.Culture = new System.Globalization.CultureInfo("hu-HU");
			btnCancel.Title = Labels.Cancel;
			btnOk.Title = Labels.Ok;
			lblPassword.StringValue = Labels.Login_Password + ":";
			cbRememberMe.Title = Labels.Login_RememeberMe;
		}

		private void HandleTxtUserIdChanged(object sender, EventArgs e)
		{
			txtUserId.StringValue = Regex.Replace(txtUserId.StringValue, "[^0-9]", "");
		}

		private void HandleHandleWillClose(object sender, EventArgs e)
		{
			if (isModal)
				NSApplication.SharedApplication.StopModal();
		}

		private void HandleBtnOkActivated(object sender, EventArgs e)
		{
			IsValid = false;
			if (txtUserId.IntValue <= 0 || string.IsNullOrEmpty(txtPassword.StringValue))
				return;
			UserId = txtUserId.IntValue;
			Password = AuthenticationHelper.GetHashedHexString(txtPassword.StringValue);
			RememberMe = cbRememberMe.IntValue == 1;

			SetEnable(false);

			//quick and dirty really ugly
			ThreadPool.QueueUserWorkItem(_ =>
			{
				//on background thread
				AuthData authData;
				var authResult = AuthenticationHelper.TryAuthenticate(
					UserId,
					Password,
					out authData
				);
				this.BeginInvokeOnMainThread(() =>
				{
					//on GUI thread
					AuthData = authData;
					//if (IsDisposed)
					//	return;
					SetEnable(true);
					var notificationSvc = Platform.Factory.GetNotificationService();
					switch (authResult)
					{
						case AuthenticationHelper.AuthenticationResponse.Successful:
							IsValid = true;
							this.PerformClose(this);
							break;
						case AuthenticationHelper.AuthenticationResponse.Unknown:
							var mbres = notificationSvc.ShowMessageBox(
								Labels.Login_NotificationResponseUnknownBody,
								Labels.Login_NotificationResponseUnknownTitle,
								MessageBoxButtons.AbortRetryIgnore,
								MessageBoxIcon.Warning
							);
							if (mbres == DialogResult.Retry)
							{
								HandleBtnOkActivated(null, EventArgs.Empty);
							}
							else if (mbres == DialogResult.Ignore)
							{
								IsValid = true;
								this.PerformClose(this);
							}
							else if (mbres == DialogResult.Abort)
							{
								//do nothing so the user can provide an other userid/password
							}
							break;
						case AuthenticationHelper.AuthenticationResponse.Denied:
							notificationSvc.ShowMessageBox(
								Labels.Login_NotificationResponseDeniedBody,
								Labels.Login_NotificationResponseDeniedTitle,
								MessageBoxButtons.OK,
								MessageBoxIcon.Error
							);
							break;
						case AuthenticationHelper.AuthenticationResponse.NotActive:
							notificationSvc.ShowMessageBox(
								Labels.Login_NotificationResponseNotActiveBody,
								Labels.Login_NotificationResponseNotActiveTitle,
								MessageBoxButtons.OK,
								MessageBoxIcon.Error
							);
							this.PerformClose(this); //exit program
							break;
					}
				}
				);
			}
			);


			//NSApplication.SharedApplication.StopModal();
			//this.PerformClose(this);
		}

		private void SetEnable(bool isEnabled)
		{
			txtUserId.Enabled = isEnabled;
			txtPassword.Enabled = isEnabled;
			cbRememberMe.Enabled = isEnabled;
			btnOk.Enabled = isEnabled;
			btnCancel.Enabled = isEnabled;
		}

		public int ShowDialog()
		{
			if (UserId != 0)
			{
				txtUserId.StringValue = UserId.ToString();
				txtUserId.Editable = false;
			}
			isModal = true;
			//MakeKeyAndOrderFront(this);
			return NSApplication.SharedApplication.RunModalForWindow(this);
		}


	}
}

