using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using log4net;
using log4net.Appender;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.View
{
	public partial class LoginForm : FixedMetroForm
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public int UserId { get; private set; }
		public bool RememberMe { get; private set; }
		public bool StartInGreen { get; private set; }
		public string UserPassword { get; private set; }
		public AuthData AuthData { get; private set; }

		private new readonly SynchronizationContext context;

		public LoginForm()
			: this(null, ConfigManager.IsLoginRememberPasswordChecked)
		{
		}

		public LoginForm(int? userId, bool rememberMe)
		{
			context = AsyncOperationManager.SynchronizationContext;
			Debug.Assert(context is WindowsFormsSynchronizationContext);

			InitializeComponent();
			ApplyCultureSpecificLanguage();
			this.SetFormStartPositionCenterScreen();
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe

			txtUserId.Text = userId.ToString();
			cbRememberMe.Checked = rememberMe;
			cbStartInGreen.Visible = !userId.HasValue;
			lblStartInGreen.Visible = !userId.HasValue;
			cbStartInGreen.Enabled = !ConfigManager.StartWorkAfterLogin;
			cbStartInGreen.Checked = ConfigManager.StartWorkAfterLogin || IsFirstStart;
			btnOk.Enabled = false;
			if (userId.HasValue) //if userId is set then we only ask for a password
			{
				cbLanguage.Visible = false;
				lblLang.Visible = false;
				txtUserId.ReadOnly = true;
				ActiveControl = txtPassword;
				Height -= lblStartInGreen.Height;
			}
			else
			{
				ActiveControl = txtUserId;
				InitializeLanguageDropdown();
			}
			this.Shown += LoginForm_Shown;
		}

		private bool IsFirstStart
		{
			get
			{
				try
				{
					IsolatedStorageSerializationHelper.GetFileNames("WorkItems\\*");
				}
				catch (DirectoryNotFoundException)
				{
					return true;
				}
				return false;
			}
		}

		private void InitializeLanguageDropdown()
		{
			cbLanguage.DisplayMember = "NativeName";
			cbLanguage.Format += (s, e) =>
			{
				var parts = e.Value.ToString().Split('(');
				e.Value = parts[0].ToUpperInvariant()[0] + parts[0].Substring(1).TrimEnd();
			};
			cbLanguage.SelectedValueChanged += (s, e) =>
			{
				Labels.Culture = (CultureInfo)cbLanguage.SelectedItem;
				ApplyCultureSpecificLanguage();
			};
			cbLanguage.Items.Clear();
			foreach (var culture in LocalizationHelper.GetSupportedCultures())
			{
				cbLanguage.Items.Add(culture);
			}
			cbLanguage.SelectedItem = Labels.Culture ?? Thread.CurrentThread.CurrentUICulture;
			if (cbLanguage.SelectedItem == null) cbLanguage.SelectedIndex = 0; //make the first as default
		}

		private void ApplyCultureSpecificLanguage()
		{
			Text = string.Format(Labels.Login_Title, ConfigManager.AppNameOverride ?? ConfigManager.ApplicationName);
			lblUserId.Text = Labels.Login_UserId + ":";
			lblPassword.Text = Labels.Login_Password + ":";
			lblLang.Text = Labels.Login_Language + ":";
			cbRememberMe.Text = Labels.Login_RememeberMe;
			lblStartInGreen.Text = Labels.Login_StartInGreen;
			lnkForgotPassword.Text = Labels.Login_ForgottenPassword;
			btnOk.Text = Labels.Login_LoginButton;
			toolTip1.SetToolTip(btnSettings, Labels.Login_ConnectionSettings);
		}

		private System.Windows.Forms.Timer caretTimer; //to fix caret blinking / focus issue
		private void LoginForm_Shown(object sender, EventArgs e)
		{
			if (this.Handle == WinApi.GetForegroundWindow()) return;
			//we are not the foreground window but txtUserId caret is still blinking...
			WinApi.HideCaret(txtUserId.Handle);
			WindowState = FormWindowState.Minimized;
			if (this.components == null) this.components = new Container();
			caretTimer = new System.Windows.Forms.Timer(this.components);
			caretTimer.Interval = 100;
			caretTimer.Tick += CaretTimer_Tick;
			caretTimer.Enabled = true;
		}

		private void CaretTimer_Tick(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Normal;
			if (this.Handle != WinApi.GetForegroundWindow()) return;
			//we are foreground again but received no event (afaik) that is why we need this timer
			caretTimer.Enabled = false;
			caretTimer.Dispose();
			caretTimer = null;
			WinApi.ShowCaret(txtUserId.Handle);
		}

		private void cbRememberMe_KeyPress(object sender, KeyPressEventArgs e)
		{
			HandleEnter(sender, e);
		}

		private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
		{
			HandleEnter(sender, e);
			
		}

		private void txtPassword_TextChanged(object sender, EventArgs e)
		{
			btnOk.Enabled = (txtPassword.Text != "");
		}

		private void cbStartInGreen_KeyPress(object sender, KeyPressEventArgs e)
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
				MessageBox.Show(Labels.Login_NotificationEnterNumberBody, Labels.Login_NotificationEnterNumberTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			UserId = 0;
			RememberMe = cbRememberMe.Checked;
			StartInGreen = cbStartInGreen.Checked;
			UserPassword = AuthenticationHelper.GetHashedHexString(txtPassword.Text);
			btnSettings.Visible = false;
			SetEnable(false);
			var username = txtUserId.Text;
			//quick and dirty really ugly
			ThreadPool.QueueUserWorkItem(_ =>
				{
					//on background thread
					AuthData authData;
					string detailedErrorText;
					var authResult = AuthenticationHelper.TryAuthenticate(username, UserPassword, out authData, out detailedErrorText);
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
												var mbres = MessageBox.Show(this, Labels.Login_NotificationResponseUnknownBody + detailedErrorText, Labels.Login_NotificationResponseUnknownTitle, UserId != 0 ? MessageBoxButtons.AbortRetryIgnore : MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
												if (mbres == DialogResult.Retry)
												{
													btnOk_Click(null, EventArgs.Empty);
												}
												else if (mbres == DialogResult.Ignore)
												{
													DialogResult = DialogResult.Ignore;
													Close();
												}
												else if (mbres == DialogResult.Abort || mbres == DialogResult.Cancel)
												{
													//do nothing so the user can provide another userid/password
												}
												break;
											case AuthenticationHelper.AuthenticationResponse.Denied:
												MessageBox.Show(this, Labels.Login_NotificationResponseDeniedBody, Labels.Login_NotificationResponseDeniedTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
												break;
											case AuthenticationHelper.AuthenticationResponse.NotActive:
												MessageBox.Show(this, Labels.Login_NotificationResponseNotActiveBody, Labels.Login_NotificationResponseNotActiveTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
												Close(); //exit program
												break;
											case AuthenticationHelper.AuthenticationResponse.PasswordExpired:
												TopMost = false;
												PasswordExpiredMessageBox.Instance.ShowDialog();
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
			cbStartInGreen.Enabled = enabled;
			cbLanguage.Enabled = enabled;
		}

		public static ConfigManager.LoginData DisplayLoginForm()
		{
#if DEBUG
			// To login automatically when jobctrl.exe's parent folder is a number as userId
			// e.g. ..\13\jobctrl.exe
			if (int.TryParse(Path.GetFileName(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)), out var userId))
			{
				return new ConfigManager.LoginData() { UserId = userId, UserPassword = AuthenticationHelper.GetHashedHexString("1"), };
			}
#endif
			using (var loginForm = new LoginForm())
			{
				Application.ThreadException += ProgramWin.Application_ThreadException;
				Application.Run(loginForm);
				log.Info("Login result " + loginForm.DialogResult);
				if (loginForm.DialogResult != DialogResult.OK && loginForm.DialogResult != DialogResult.Ignore) return null;
				return new ConfigManager.LoginData()
					{
						UserId = loginForm.UserId,
						UserPassword = loginForm.UserPassword,
						RememberMe = loginForm.RememberMe,
						StartWorkAfterLogin = loginForm.StartInGreen,
						AuthData = loginForm.AuthData,
						Culture = (CultureInfo)loginForm.cbLanguage.SelectedItem,
					};
			}
		}

		public static ConfigManager.LoginData ShowChangePasswordDialog(ConfigManager.LoginData input)
		{
			using (var loginForm = new LoginForm(input.UserId, input.RememberMe))
			{
				loginForm.ShowDialog();
				log.Info("Login password change result " + loginForm.DialogResult);
				if (loginForm.DialogResult != DialogResult.OK && loginForm.DialogResult != DialogResult.Ignore) return null;
				return new ConfigManager.LoginData()
					{
						UserId = loginForm.UserId,
						UserPassword = loginForm.UserPassword,
						RememberMe = loginForm.RememberMe,
						AuthData = loginForm.AuthData,
						Culture = Labels.Culture,
					};
			}
		}

		private void lnkForgotPassword_Click(object sender, EventArgs e)
		{
			var forgotPassUrl = ConfigManager.WebsiteUrl + "Account/ForgotYourPassword.aspx";
			try
			{
				var sInfo = new ProcessStartInfo(forgotPassUrl);
				Process.Start(sInfo);
			}
			catch (Exception ex)
			{
				log.Error("Unable to open url: " + forgotPassUrl, ex);
			}
		}

		private void lblStartInGreen_Click(object sender, EventArgs e)
		{
			cbStartInGreen.Checked = !cbStartInGreen.Checked;
		}

		private void HandleSettingsClicked(object sender, EventArgs e)
		{
			var form = new LoginSettings();
			form.ShowDialog();
		}
	}
}
