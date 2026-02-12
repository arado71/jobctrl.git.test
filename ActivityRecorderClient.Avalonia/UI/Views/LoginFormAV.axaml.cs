using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using log4net;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Forms;
using ApplicationAV = Avalonia.Application;
using ButtonAV = Avalonia.Controls.Button;

namespace ActivityRecorderClientAV
{
	public partial class LoginFormAV : Window
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int WindowWidth = 420;
		private const int WindowHeight = 420;

		public int UserId { get; private set; }
		public bool RememberMe { get; private set; }
		public bool StartInGreen { get; private set; }
		public string UserPassword { get; private set; }
		public AuthData AuthData { get; private set; }
		public CultureInfo Culture { get => cbLanguage.SelectedValue == "Magyar" ? new CultureInfo("hu-HU") : new CultureInfo("en-US"); }

		private bool IsDisposed { get; set; }
		public DialogResult DialogResult { get; private set; } = DialogResult.None;

		private readonly TaskCompletionSource<bool> tcsForClose = new();

		public LoginFormAV()
		{
			InitializeComponent();
			InitTaskbarIcon();
			InitScaling();
			ApplyThemeBackground();
			//ApplyButtonBackgrounds();
			UpdateThemeIcons();
			//ApplyThemeColors();
			this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			ThemeHelperAV.RegisterThemeChangeHandler(this);

			ApplicationAV.Current!.PropertyChanged += (sender, e) =>
			{
				if (e.Property.Name == nameof(ApplicationAV.ActualThemeVariant)) UpdateThemeIcons();
			};

			this.Closed += (_, __) =>
			{
				IsDisposed = true;
				tcsForClose.SetResult(true);
			};

			tbUserID.KeyDown += (s, e) =>
			{
				if (e.Key == Key.Enter)
					tbPassword.Focus();
			};

			tbPassword.KeyDown += (s, e) =>
			{
				if (e.Key == Key.Enter)
					OnLoginClick(this, null);
			};
		}

		private void InitTaskbarIcon()
		{
			try
			{
				this.Icon = new WindowIcon("Assets/JobCtrl.ico");
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error initializing tray icon: " + ex.Message);
			}
		}

		private void InitScaling()
		{
			var scaleTransform = new ScaleTransform(ScaleHelperAV.GlobalWindowScale, ScaleHelperAV.GlobalWindowScale);
			LoginWindowLayoutTransformControl.LayoutTransform = scaleTransform;
			this.Width = WindowWidth * ScaleHelperAV.GlobalWindowScale;
			this.Height = WindowHeight * ScaleHelperAV.GlobalWindowScale;
		}

		private void ApplyThemeBackground()
		{
			var currentTheme = ApplicationAV.Current?.ActualThemeVariant;

			if (currentTheme == Avalonia.Styling.ThemeVariant.Light)
			{
				this.Classes.Add("light");
				this.Classes.Remove("dark");
			}
			else
			{
				this.Classes.Add("dark");
				this.Classes.Remove("light");
			}
		}

		private void ApplyButtonBackgrounds()
		{
			var buttonBackgroundColor = new SolidColorBrush(Colors.Transparent);

			foreach (var button in this.GetLogicalDescendants().OfType<ButtonAV>())
			{
				button.Background = buttonBackgroundColor;
			}
		}

		private void UpdateThemeIcons()
		{
			// Theme icon new
			bool lightmode = AppResourcesAV.IsLightTheme();
			var themeIcon = SvgIconRegistryAV.GetIcon("SvgThemeIcon");
			var systemThemeIcon = SvgIconRegistryAV.GetIcon("SvgSystemThemeIcon");
			if (themeIcon != null && systemThemeIcon != null)
			{
				var themeIconFileName = App.IsSystemTheme ? systemThemeIcon.GetFileName(lightmode) : themeIcon.GetFileName(lightmode);
				//SetIconSource("ThemeIcon", themeIconFileName);
				AppResourcesAV.SetIconSource(this, "ThemeIcon", themeIconFileName);
			}

			//SetIcon("SvgSettingsIcon", "SettingsIcon", lightmode);
			AppResourcesAV.SetIcon(this, "SvgSettingsIcon", "SettingsIcon", lightmode);
			//SetIcon("SvgCloseIcon", "CloseIcon", lightmode);
			AppResourcesAV.SetIcon(this, "SvgCloseIcon", "CloseIcon", lightmode);
		}

		private void ApplyThemeColors()
		{
			var theme = ApplicationAV.Current?.ActualThemeVariant;
			var isLightTheme = theme == Avalonia.Styling.ThemeVariant.Light;

			if (isLightTheme)
				ApplicationAV.Current!.Resources["TextColor"] = ApplicationAV.Current.Resources["TextColorLight"];
			else
				ApplicationAV.Current!.Resources["TextColor"] = ApplicationAV.Current.Resources["TextColorDark"];
		}

		private void OnThemeClick(object sender, RoutedEventArgs e)
		{
			if (!App.IsSystemTheme)
			{
				if (AppResourcesAV.IsLightTheme())
				{
					// Switch to dark theme
					SwitchTheme(false);
				}
				else
				{
					// Enable system theme
					App.IsSystemTheme = true;

					if (AppResourcesAV.IsSystemInDarkMode())
					{
						// Change icon to system_dark
						//SetSystemThemeIcon(false);
						AppResourcesAV.SetSystemThemeIcon(this, false);
					}
					else
					{
						// Switch to light theme
						SwitchTheme(true);

						// Change icon to system_light
						//SetSystemThemeIcon(true);
						AppResourcesAV.SetSystemThemeIcon(this, true);
					}
				}
			}
			else
			{
				// Disable system theme
				App.IsSystemTheme = false;

				// Switch to light theme
				SwitchTheme(true);

				if (AppResourcesAV.IsLightTheme())
				{
					// Update theme icon
					//SetIcon("SvgThemeIcon","ThemeIcon", true);
					AppResourcesAV.SetIcon(this, "SvgThemeIcon", "ThemeIcon", true);
				}
			}
			/*
			var currentTheme = Application.Current?.ActualThemeVariant ?? Avalonia.Styling.ThemeVariant.Light;

			if (App.IsSystemTheme == false)
			{
				if (currentTheme == Avalonia.Styling.ThemeVariant.Light)
				{ Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark; }
				else
				{
					App.IsSystemTheme = true;

					if (App.IsSystemInDarkMode())
					{
						// Change icon to system_dark
						SetSystemThemeIcon(false);
					}
					else
					{
						// Change theme to light
						Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;

						// Change icon to system_light
						SetSystemThemeIcon(true);
					}
				}
			}
			else
			{
				App.IsSystemTheme = false;

				Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;

				if (currentTheme == Avalonia.Styling.ThemeVariant.Light)
				{
					// Change icon to sun
					//var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
					//string baseIconPath = $"avares://{assemblyName}/icons/";
					SetIconSource("ThemeIcon", "sun.svg");
				}
			}
			*/
		}

		public void SwitchTheme(bool lightmode)
		{
			ApplicationAV.Current!.RequestedThemeVariant = lightmode ? Avalonia.Styling.ThemeVariant.Light : Avalonia.Styling.ThemeVariant.Dark;
		}

		private void OnForgotPasswordClick(object? sender, PointerReleasedEventArgs e)
		{
			var forgotPassUrl = ConfigManager.WebsiteUrl + "Account/ForgotYourPassword.aspx";
			try
			{
				if (OperatingSystem.IsWindows())
				{
					var sInfo = new ProcessStartInfo(forgotPassUrl);
					Process.Start(sInfo);
				}
				else
				{
					Process.Start("open", forgotPassUrl);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to open url: " + forgotPassUrl, ex);
			}
		}

		private void OnSettingsClick(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Settings clicked");
		}

		private void OnCloseClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void OnLoginClick(object sender, RoutedEventArgs e)
		{
			var context = SynchronizationContext.Current!;
			int userId;
			if (string.IsNullOrEmpty(tbUserID.Text) || ((!int.TryParse(tbUserID.Text, out userId) || userId <= 0) && !Regex.IsMatch(tbUserID.Text, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")))
			{
				//MessageBox.Show(Labels.Login_NotificationEnterNumberBody, Labels.Login_NotificationEnterNumberTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			UserId = 0;
			RememberMe = cbRememberMe.IsChecked ?? false;
			StartInGreen = cbStartAuto.IsChecked ?? false;
			UserPassword = AuthenticationHelper.GetHashedHexString(tbPassword.Text ?? "");
			SetEnable(false);
			var username = tbUserID.Text;

			//quick and dirty really ugly
			ThreadPool.QueueUserWorkItem(_ =>
			{
				//on background thread
				AuthData authData;
				string detailedErrorText;
				var authResult = AuthenticationHelper.TryAuthenticate(username, UserPassword, out authData, out detailedErrorText);
				context.Post(async (_) =>
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
					log.Info("Login result: " + authResult);
					// TODO: mac, handle cases properly
					switch (authResult)
					{
						case AuthenticationHelper.AuthenticationResponse.Successful:
							DialogResult = DialogResult.OK;
							Close();
							break;
						case AuthenticationHelper.AuthenticationResponse.Unknown:
							var mbres = await MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
							{
								ContentTitle = Labels.Login_NotificationResponseUnknownTitle,
								ContentMessage = Labels.Login_NotificationResponseUnknownBody + Environment.NewLine + detailedErrorText,
								MaxWidth = 1000,
								Topmost = true,
								WindowStartupLocation = WindowStartupLocation.CenterOwner,
								Icon = MsBox.Avalonia.Enums.Icon.Warning,
								ButtonDefinitions = UserId != 0 ? new List<ButtonDefinition>
								{
									new ButtonDefinition { Name = "Abort", },
									new ButtonDefinition { Name = "Retry", },
									new ButtonDefinition { Name = "Ignore", },
								} : new List<ButtonDefinition>
								{
									new ButtonDefinition { Name = "Retry", },
									new ButtonDefinition { Name = "Cancel", },
								},
							}).ShowWindowDialogAsync(this);

							if (mbres == "Retry")
							{
								OnLoginClick(this, new RoutedEventArgs());
							}
							else if (mbres == "Ignore")
							{
								DialogResult = DialogResult.Ignore;
								Close();
							}
							else if (mbres == "Abort" || mbres == "Cancel")
							{
								//do nothing so the user can provide another userid/password
							}
							break;
						case AuthenticationHelper.AuthenticationResponse.Denied:
							await MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
							{
								ContentTitle = Labels.Login_NotificationResponseDeniedTitle,
								ContentMessage = Labels.Login_NotificationResponseDeniedBody,
								Topmost = true,
								WindowStartupLocation = WindowStartupLocation.CenterOwner,
								Icon = MsBox.Avalonia.Enums.Icon.Error,
								ButtonDefinitions = new List<ButtonDefinition>
								{
									new ButtonDefinition { Name = "OK", },
								},
							}).ShowWindowDialogAsync(this);
							break;
						case AuthenticationHelper.AuthenticationResponse.NotActive:
							await MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
							{
								ContentTitle = Labels.Login_NotificationResponseNotActiveTitle,
								ContentMessage = Labels.Login_NotificationResponseNotActiveBody,
								Topmost = true,
								WindowStartupLocation = WindowStartupLocation.CenterOwner,
								Icon = MsBox.Avalonia.Enums.Icon.Error,
								ButtonDefinitions = new List<ButtonDefinition>
								{
									new ButtonDefinition { Name = "OK", },
								},
							}).ShowWindowDialogAsync(this);
							Close(); //exit program
							break;
						case AuthenticationHelper.AuthenticationResponse.PasswordExpired:
							await MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
							{
								ButtonDefinitions = new List<ButtonDefinition>
								{
									new ButtonDefinition { Name = "OK", },
								},
								ContentTitle = Labels.Login_PasswordExpiredTitle,
								//ContentMessage = "",
								Icon = MsBox.Avalonia.Enums.Icon.Warning,
								WindowStartupLocation = WindowStartupLocation.CenterOwner,
								ShowInCenter = true,
								Topmost = true,
								HyperLinkParams = new HyperLinkParams
								{
									Text = Labels.Login_PasswordExpiredClick,
									Action = new Action(() =>
									{
										var url = ConfigManager.WebsiteUrl + "Account/Login.aspx?url=" + Uri.EscapeDataString("/Account/ForgotYourPassword.aspx");

										if (OperatingSystem.IsWindows())
										{
											using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
											proc.Start();
										}
										else
										{
											Process.Start("open", url);
										}
									})
								}
							}).ShowWindowDialogAsync(this);
							break;
					}
				}
				, null);
			});

		}

		private void SetEnable(bool enabled)
		{
			cbRememberMe.IsEnabled = enabled;
			btnLogin.IsEnabled = enabled;
			tbPassword.IsEnabled = enabled;
			tbUserID.IsEnabled = enabled;
			cbStartAuto.IsEnabled = enabled;
			cbLanguage.IsEnabled = enabled;
		}

		public async Task<DialogResult> WaitForCloseAsync()
		{
			await tcsForClose.Task;
			return DialogResult;
		}
	}
}
