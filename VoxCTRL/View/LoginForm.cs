using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using VoxCTRL.ActivityRecorderServiceReference;
using VoxCTRL.Communication;
using log4net;

namespace VoxCTRL.View
{
	public partial class LoginForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public int UserId { get; private set; }
		public bool RememberMe { get; private set; }
		public string UserPassword { get; private set; }
		public AuthData AuthData { get; private set; }

		private readonly SynchronizationContext context;

		public LoginForm()
			: this(null, false)
		{
			//lblPassword.Text = Labels.Login_Password;
			//cbRememberMe.Text = Labels.Login_RememeberMe;
			lblPassword.Text = "Jelszó";
			cbRememberMe.Text = "Emlékezzen rám";

			this.Text = "VoxCTRL - Login";
		}

		public LoginForm(int? userId, bool rememberMe)
		{
			context = AsyncOperationManager.SynchronizationContext;
			Debug.Assert(context is WindowsFormsSynchronizationContext);

			InitializeComponent();
			Icon = Properties.Resources.VoxCTRL; //don't set it in the designer as it would enlarge the exe

			txtUserId.Text = userId.ToString();
			cbRememberMe.Checked = rememberMe;
			if (userId.HasValue) //if userId is set then we only ask for a password
			{
				txtUserId.ReadOnly = true;
				ActiveControl = txtPassword;
			}
			else
			{
				ActiveControl = txtUserId;
			}
			this.Shown += LoginForm_Shown;
		}

		private System.Windows.Forms.Timer caretTimer; //to fix caret blinking / focus issue
		private void LoginForm_Shown(object sender, EventArgs e)
		{
			if (this.Handle == GetForegroundWindow()) return;
			//we are not the foreground window but txtUserId caret is still blinking...
			HideCaret(txtUserId.Handle);
			if (this.components == null) this.components = new Container();
			caretTimer = new System.Windows.Forms.Timer(this.components);
			caretTimer.Interval = 100;
			caretTimer.Tick += CaretTimer_Tick;
			caretTimer.Enabled = true;
		}

		private void CaretTimer_Tick(object sender, EventArgs e)
		{
			if (this.Handle != GetForegroundWindow()) return;
			//we are foreground again but received no event (afaik) that is why we need this timer
			caretTimer.Enabled = false;
			caretTimer.Dispose();
			caretTimer = null;
			ShowCaret(txtUserId.Handle);
		}

		private void cbRememberMe_KeyPress(object sender, KeyPressEventArgs e)
		{
			HandleEnter(sender, e);
		}

		private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
		{
			HandleEnter(sender, e);
		}

		private void txtUserId_KeyPress(object sender, KeyPressEventArgs e)
		{
			HandleEnter(sender, e);
		}

		private void HandleEnter(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != (char)Keys.Return) return;
			if (txtUserId.Text == "")
			{
				txtUserId.Focus();
			}
			else if (txtPassword.Text == "")
			{
				txtPassword.Focus();
			}
			else
			{
				btnOk_Click(sender, e);
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			int userId;
			if (string.IsNullOrEmpty(txtUserId.Text) || ((!int.TryParse(txtUserId.Text, out userId) || userId <= 0) && !Regex.IsMatch(txtUserId.Text, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")))
			{
				//MessageBox.Show(Labels.Login_NotificationEnterNumberBody, Labels.Login_NotificationEnterNumberTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				MessageBox.Show("A Felhasználói azonosító/e-mail formátuma nem megfelelő. Kérjük, számot vagy érvényes e-mail címet adjon meg!", "Hiba!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			UserId = 0;
			RememberMe = cbRememberMe.Checked;
			UserPassword = AuthenticationHelper.GetHashedHexString(txtPassword.Text);
			SetEnable(false);
			var username = txtUserId.Text;

			//quick and dirty really ugly
			ThreadPool.QueueUserWorkItem(_ =>
				{
					//on background thread
					AuthData authData;
					var authResult = AuthenticationHelper.TryAuthenticate(username, UserPassword, out authData);
					context.Post(__ =>
									{
										//on GUI thread
										AuthData = authData;

										if (AuthData != null)
										{
											UserId = AuthData.Id;
											if (Regex.IsMatch(username, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
											{
												AuthenticationHelper.UpdateEmail(username, authData.Id);
											}
										}
										else
										{
											if (Regex.IsMatch(username, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
											{
												UserId = AuthenticationHelper.GetUserId(username) ?? 0;
											}
											else
											{

												int userIdRaw;
												if (int.TryParse(username, out userIdRaw) && userIdRaw > 0)
												{
													UserId = userIdRaw;
												}
											}
										}

										if (IsDisposed) return;
										SetEnable(true);
										switch (authResult)
										{
											case AuthenticationHelper.AuthenticationResponse.Successful:
												DialogResult = DialogResult.OK;
												Close();
												break;
											case AuthenticationHelper.AuthenticationResponse.Unknown:
												//var mbres = MessageBox.Show(this, Labels.Login_NotificationResponseUnknownBody, Labels.Login_NotificationResponseUnknownTitle, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning);
												var mbres = MessageBox.Show(this, "A jelszó ellenőrzése sikertelen", "Kommunikációs hiba!", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning);
												if (mbres == DialogResult.Retry)
												{
													btnOk_Click(null, EventArgs.Empty);
												}
												else if (mbres == DialogResult.Ignore)
												{
													DialogResult = DialogResult.Ignore;
													Close();
												}
												else if (mbres == DialogResult.Abort)
												{
													//do nothing so the user can provide an other userid/password
												}
												break;
											case AuthenticationHelper.AuthenticationResponse.Denied:
												//MessageBox.Show(this, Labels.Login_NotificationResponseDeniedBody, Labels.Login_NotificationResponseDeniedTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
												MessageBox.Show(this, "A felhasználó azonosító vagy a jelszó hibás!", "Hiba!", MessageBoxButtons.OK, MessageBoxIcon.Error);
												break;
											case AuthenticationHelper.AuthenticationResponse.NotActive:
												//MessageBox.Show(this, Labels.Login_NotificationResponseNotActiveBody, Labels.Login_NotificationResponseNotActiveTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
												MessageBox.Show(this, "Önt nem aktiválták", "Hiba!", MessageBoxButtons.OK, MessageBoxIcon.Error);
												Close(); //exit program
												break;
										}
									}
								 , null);
				});

		}

		private void SetEnable(bool enabled)
		{
			btnOk.Enabled = enabled;
			txtUserId.Enabled = enabled;
			txtPassword.Enabled = enabled;
			cbRememberMe.Enabled = enabled;
		}

		public static LoginData DisplayLoginForm()
		{
			using (var loginForm = new LoginForm())
			{
				Application.Run(loginForm);
				log.Info("Login result " + loginForm.DialogResult);
				if (loginForm.DialogResult != DialogResult.OK && loginForm.DialogResult != DialogResult.Ignore) return null;
				return new LoginData()
				{
					UserId = loginForm.UserId,
					UserPassword = loginForm.UserPassword,
					RememberMe = loginForm.RememberMe,
					AuthData = loginForm.AuthData,
				};
			}
		}

		//public static ConfigManager.LoginData ShowChangePasswordDialog(ConfigManager.LoginData input)
		//{
		//    using (var loginForm = new LoginForm(input.UserId, input.RememberMe))
		//    {
		//        loginForm.ShowDialog();
		//        log.Info("Login password change result " + loginForm.DialogResult);
		//        if (loginForm.DialogResult != DialogResult.OK && loginForm.DialogResult != DialogResult.Ignore) return null;
		//        return new ConfigManager.LoginData()
		//            {
		//                UserId = loginForm.UserId,
		//                UserPassword = loginForm.UserPassword,
		//                RememberMe = loginForm.RememberMe,
		//                AuthData = loginForm.AuthData,
		//            };
		//    }
		//}

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll")]
		private static extern bool HideCaret(IntPtr hWnd);
		[DllImport("user32.dll")]
		private static extern bool ShowCaret(IntPtr hWnd);
	}
}
