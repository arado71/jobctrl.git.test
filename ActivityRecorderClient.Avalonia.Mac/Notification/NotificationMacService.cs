using Avalonia.Controls;
using CoreServices;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System.Diagnostics;
using System.Drawing;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Avalonia;
using Tct.ActivityRecorderClient.Avalonia.UI.ViewModels;
using Tct.ActivityRecorderClient.Avalonia.UI.Views;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Menu;
using Icon = MsBox.Avalonia.Enums.Icon;

namespace Tct.ActivityRecorderClient.Notification;

public class NotificationMacService : INotificationService
{
	private readonly Dictionary<string, NotificationWindow> notifications = new Dictionary<string, NotificationWindow>();
	private NotificationPosition position = ConfigManager.LocalSettingsForUser.NotificationPosition;

	public void CloseAll()
	{
		foreach (var form in notifications.Values)
		{
			if (!form.IsClosed)
			{
				form.CloseFast();
			}
		}
		notifications.Clear();
	}

	public void HideNotification(string key)
	{
		NotificationWindow form;
		if (notifications.TryGetValue(key, out form))
		{
			if (!form.IsClosed)
			{
				notifications[key].Close();
			}
			notifications.Remove(key);
		}
	}

	public bool IsActive(string key)
	{
		NotificationWindow form;
		if (notifications.TryGetValue(key, out form))
		{
			return !form.IsClosed;
		}
		return false;
	}

	public NotificationPosition Position
	{
		get { return position; }
		set
		{
			if (position == value) return;
			position = value;
			ConfigManager.LocalSettingsForUser.NotificationPosition = position;
			foreach (var notification in notifications)
				notification.Value.ViewModel!.Position = position;
		}
	}

	public DialogResult ShowMessageBox(string message)
	{
		var result = MessageBoxManager.GetMessageBoxCustom(
			new MessageBoxCustomParams
			{
				ContentTitle = message,
				ContentMessage = message,
				Topmost = true,
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				Icon = Icon.Error,
				ButtonDefinitions = new List<ButtonDefinition>
				{
					new ButtonDefinition { Name = "OK", },
				},
			})
			.ShowWindow();
		return MapResult(result);
	}

	public DialogResult ShowMessageBox(string message, string title)
	{
		var result = MessageBoxManager.GetMessageBoxCustom(
			new MessageBoxCustomParams
			{
				ContentTitle = title,
				ContentMessage = message,
				Topmost = true,
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				Icon = Icon.Error,
				ButtonDefinitions = new List<ButtonDefinition>
				{
					new ButtonDefinition { Name = "OK", },
				},
			})
			.ShowWindow();
		return MapResult(result);
	}

	public DialogResult ShowMessageBox(string message, string title, MessageBoxButtons buttons)
	{
		var result = MessageBoxManager.GetMessageBoxCustom(
			new MessageBoxCustomParams
			{
				ContentTitle = title,
				ContentMessage = message,
				Topmost = true,
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				Icon = Icon.Error,
				ButtonDefinitions = MapButtons(buttons),
			})
			.ShowWindow();
		return MapResult(result);
	}

	public DialogResult ShowMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		var result = MessageBoxManager.GetMessageBoxCustom(
			new MessageBoxCustomParams
			{
				ContentTitle = title,
				ContentMessage = message,
				Topmost = true,
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				Icon = MapIcon(icon),
				ButtonDefinitions = MapButtons(buttons),
			})
			.ShowWindow();
		return MapResult(result);
	}

	public bool ShowNotification(string key, TimeSpan showDuration, string title, string message)
	{
		return ShowNotification(key, showDuration, title, message, null);
	}

	public bool ShowNotification(string key, TimeSpan showDuration, string title, string message, Color? backColor)
	{
		return ShowNotification(key, showDuration, title, message, backColor, null);
	}

	public bool ShowNotification(string key, TimeSpan showDuration, string title, string message, Color? backColor, Action notificationClicked)
	{
		return ShowNotification(key, showDuration, title, new MessageWithActions().Append(message), backColor, notificationClicked);
	}

	public bool ShowNotification(string key, TimeSpan showDuration, string title, MessageWithActions message, Color? backColor = null, Action notificationClicked = null)
	{
		NotificationWindow form;
		if (notifications.TryGetValue(key, out form))
		{
			if (!form.IsClosed) return false;
			notifications.Remove(key);
		}
		var pos = Position;
		if (ConfigManager.IsNotificationShown.HasValue && ConfigManager.IsNotificationShown.Value && pos == NotificationPosition.Hidden) pos = NotificationPosition.BottomRight;
		if ((pos == NotificationPosition.Hidden || ConfigManager.IsNotificationShown.HasValue && !ConfigManager.IsNotificationShown.Value) && NotificationKeys.IsImportant(key) && ConfigManager.IsTaskBarIconShowing) pos = NotificationPosition.BottomRight; //cannot suppress important keys if icon showing
		if (pos == NotificationPosition.Hidden || ConfigManager.IsNotificationShown.HasValue && !ConfigManager.IsNotificationShown.Value) return false;
		var newForm = new NotificationWindow()
		{
			ViewModel = new NotificationViewModel()
			{
				Title = title,
				// TODO: mac, support MessageWithActions
				Message = message.GetText(),
				ShowDuration = showDuration,
				Position = pos,
				BarColor = backColor != null ? ToHexColor(backColor.Value) : null,
				// TODO: mac, support notificationClicked
				NotificationClicked = notificationClicked,
			},
			ShowActivated = false,
		};

		notifications.Add(key, newForm);
		newForm.Show();
		newForm.Topmost = true;

		return true;
	}

	static string? ToHexColor(Color c)
	{
		return c.A == 255
			? $"#{c.R:X2}{c.G:X2}{c.B:X2}"
			: $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
	}

	public DialogResult ShowPasswordExpiredMessageBox()
	{
		MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
		{
			ButtonDefinitions = new List<ButtonDefinition>
								{
									new ButtonDefinition { Name = "OK", },
								},
			ContentTitle = Labels.Login_PasswordExpiredTitle,
			//ContentMessage = "",
			Icon = Icon.Warning,
			WindowStartupLocation = WindowStartupLocation.CenterScreen,
			ShowInCenter = true,
			Topmost = true,
			HyperLinkParams = new HyperLinkParams
			{
				Text = Labels.Login_PasswordExpiredClick,
				Action = new Action(() =>
				{
					if (ConfigManager.IsAuthDataRequired)
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
					}
					else
					{
						RecentUrlQuery.Instance.OpenUrl("/Account/ForgotYourPassword.aspx");
					}
				})
			}
		}).ShowWindow();
		return DialogResult.OK;
	}

	public string ShowServerNotification(JcForm form)
	{
		// TODO: mac
		return "";
	}

	private static DialogResult MapResult(string buttonTitle)
	{
		return buttonTitle switch
		{
			"OK" => DialogResult.OK,
			"Cancel" => DialogResult.Cancel,
			"Abort" => DialogResult.Abort,
			"Retry" => DialogResult.Retry,
			"Ignore" => DialogResult.Ignore,
			"Yes" => DialogResult.Yes,
			"No" => DialogResult.No,
			"Try Again" => DialogResult.TryAgain,
			"Continue" => DialogResult.Continue,
			_ => DialogResult.None,
		};
	}

	private static List<ButtonDefinition> MapButtons(MessageBoxButtons buttons)
	{
		return buttons switch
		{
			MessageBoxButtons.OK => new()
			{
				new ButtonDefinition { Name = "OK" }
			},

			MessageBoxButtons.OKCancel => new()
			{
				new ButtonDefinition { Name = "OK" },
				new ButtonDefinition { Name = "Cancel" }
			},

			MessageBoxButtons.AbortRetryIgnore => new()
			{
				new ButtonDefinition { Name = "Abort" },
				new ButtonDefinition { Name = "Retry" },
				new ButtonDefinition { Name = "Ignore" }
			},

			MessageBoxButtons.YesNoCancel => new()
			{
				new ButtonDefinition { Name = "Yes" },
				new ButtonDefinition { Name = "No" },
				new ButtonDefinition { Name = "Cancel" }
			},

			MessageBoxButtons.YesNo => new()
			{
				new ButtonDefinition { Name = "Yes" },
				new ButtonDefinition { Name = "No" }
			},

			MessageBoxButtons.RetryCancel => new()
			{
				new ButtonDefinition { Name = "Retry" },
				new ButtonDefinition { Name = "Cancel" }
			},

			MessageBoxButtons.CancelTryContinue => new()
			{
				new ButtonDefinition { Name = "Cancel" },
				new ButtonDefinition { Name = "Try Again" },
				new ButtonDefinition { Name = "Continue" }
			},

			_ => throw new ArgumentOutOfRangeException(nameof(buttons), buttons, null)
		};
	}

	private static Icon MapIcon(MessageBoxIcon icon)
	{
		return icon switch
		{
			MessageBoxIcon.None => Icon.None,
			MessageBoxIcon.Hand => Icon.Error,
			MessageBoxIcon.Question => Icon.Question,
			MessageBoxIcon.Exclamation => Icon.Warning,
			MessageBoxIcon.Asterisk => Icon.Info,
			_ => Icon.None
		};
	}
}