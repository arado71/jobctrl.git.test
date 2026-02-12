using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Notification
{
	public class NotificationWinService : INotificationService
	{
		private readonly Dictionary<string, NotificationForm> notifications = new Dictionary<string, NotificationForm>();
		private NotificationPosition position = ConfigManager.LocalSettingsForUser.NotificationPosition;

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
			NotificationForm form;
			if (notifications.TryGetValue(key, out form))
			{
				if (!form.IsDisposed) return false;
				notifications.Remove(key);
			}
			var pos = Position;
			if (ConfigManager.IsNotificationShown.HasValue && ConfigManager.IsNotificationShown.Value && pos == NotificationPosition.Hidden) pos = NotificationPosition.BottomRight;
			if ((pos == NotificationPosition.Hidden || ConfigManager.IsNotificationShown.HasValue && !ConfigManager.IsNotificationShown.Value) && NotificationKeys.IsImportant(key) && ConfigManager.IsTaskBarIconShowing) pos = NotificationPosition.BottomRight; //cannot suppress important keys if icon showing
			if (pos == NotificationPosition.Hidden || ConfigManager.IsNotificationShown.HasValue && !ConfigManager.IsNotificationShown.Value) return false;
			var newForm = new NotificationForm { ShowDuration = showDuration, Position = pos };
			if (backColor.HasValue)
			{
				newForm.Color = backColor.Value;
			}
			if (notificationClicked != null)
			{
				newForm.NotificationClicked += (_, __) => notificationClicked();
			}
			notifications.Add(key, newForm);
			//TelemetryHelper.RecordFeature("Notification", "Show");
			//newForm.TopMost = false;
			newForm.Show(title, message);
			//simply setting topmost to true will steal focus inside the program so use pinvoke with SWP_NOACTIVATE
			return true;
		}

		public bool IsActive(string key)
		{
			NotificationForm form;
			if (notifications.TryGetValue(key, out form))
			{
				return (!form.IsDisposed);
			}
			return false;
		}

		public void HideNotification(string key)
		{
			NotificationForm form;
			if (notifications.TryGetValue(key, out form))
			{
				if (!form.IsDisposed)
				{
					notifications[key].Close();
				}
				notifications.Remove(key);
			}
		}

		public void CloseAll()
		{
			foreach (var form in notifications.Values)
			{
				if (!form.IsDisposed)
				{
					form.CloseFast();
				}
			}
			notifications.Clear();
		}

		

		public static void SetInactiveTopMost(IWin32Window frm)
		{
			WinApi.SetWindowPos(frm.Handle, WinApi.HWND_TOPMOST, 0, 0, 0, 0, WinApi.SWP_NOSIZE | WinApi.SWP_NOMOVE | WinApi.SWP_NOACTIVATE);
		}

		public DialogResult ShowMessageBox(string message)
		{
			return TopMostMessageBox.Show(message);
		}

		public DialogResult ShowMessageBox(string message, string title)
		{
			return TopMostMessageBox.Show(message, title);
		}

		public DialogResult ShowMessageBox(string message, string title, MessageBoxButtons buttons)
		{
			return TopMostMessageBox.Show(message, title, buttons);
		}

		public DialogResult ShowMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return TopMostMessageBox.Show(message, title, buttons, icon);
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
					notification.Value.Position = position;
			}
		}

		public string ShowServerNotification(JcForm jcForm)
		{
			return NotificationWinServiceHelper.ShowServerNotification(jcForm);
		}

		public DialogResult ShowPasswordExpiredMessageBox()
		{
			return PasswordExpiredMessageBox.Instance.ShowDialog(true);
		}
	}
}