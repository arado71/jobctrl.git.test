using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Forms;

namespace Tct.ActivityRecorderClient.Notification
{
	public interface INotificationService
	{
		bool ShowNotification(string key, TimeSpan showDuration, string title, string message);
		bool ShowNotification(string key, TimeSpan showDuration, string title, string message, Color? backColor);
		bool ShowNotification(string key, TimeSpan showDuration, string title, string message, Color? backColor, Action notificationClicked);
		bool ShowNotification(string key, TimeSpan showDuration, string title, MessageWithActions message, Color? backColor = null, Action notificationClicked = null);

		bool IsActive(string key);
		void HideNotification(string key);
		void CloseAll();
		NotificationPosition Position { get; set; }

		DialogResult ShowMessageBox(string message);
		DialogResult ShowMessageBox(string message, string title);
		DialogResult ShowMessageBox(string message, string title, MessageBoxButtons buttons);
		DialogResult ShowMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon);

		DialogResult ShowPasswordExpiredMessageBox();

		string ShowServerNotification(JcForm form);
	}
}
