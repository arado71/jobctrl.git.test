using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Tct.ActivityRecorderClient.ViewMac;

namespace Tct.ActivityRecorderClient.Notification
{
	public class NotificationMacService : INotificationService
	{
		private readonly Dictionary<string, NotificationWindowController> notifications = new Dictionary<string, NotificationWindowController>();

		public bool ShowNotification(string key, TimeSpan showDuration, string title, string message)
		{
			return ShowNotification(key, showDuration, title, message, null);
		}

		public bool ShowNotification(string key, TimeSpan showDuration, string title, string message, System.Drawing.Color? backColor)
		{
			if (notifications.ContainsKey(key))
				return false;
			var n = new NotificationWindowController();
			n.Title = title;
			n.Body = message;
			var color = backColor == null ? NSColor.White : NSColor.FromDeviceRgba(backColor.Value.R / (float)255, backColor.Value.G / (float)255, backColor.Value.B / (float)255, backColor.Value.A / (float)255);
			n.Color = color;
			n.Key = key;
			n.Window.WillClose += HandleNWindowWillClose;
			notifications.Add(key, n);
			n.ShowWindow(showDuration);
			return true;
		}

		private void HandleNWindowWillClose(object sender, EventArgs e)
		{
			var key = ((NotificationWindowController)((NSWindow)((NSNotification)sender).Object).WindowController).Key;
			notifications.Remove(key);
		}

		public bool IsActive(string key)
		{
			return notifications.ContainsKey(key);
		}

		public void HideNotification(string key)
		{
			NotificationWindowController ctrl;
			if (notifications.TryGetValue(key, out ctrl))
			{
				ctrl.Close();
			}
		}

		public void CloseAll()
		{
			foreach (var key in notifications.Keys.ToArray())
			{
				HideNotification(key);
			}
		}

		public DialogResult ShowMessageBox(string message)
		{
			return ShowMessageBox("", message, MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		public DialogResult ShowMessageBox(string message, string title)
		{
			return ShowMessageBox(message, title, MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		public DialogResult ShowMessageBox(string message, string title, System.Windows.Forms.MessageBoxButtons buttons)
		{
			return ShowMessageBox(message, title, buttons, MessageBoxIcon.None);
		}

		//http://stackoverflow.com/questions/5149753/am-i-using-beginsheet-right-monomac
		public DialogResult ShowMessageBox(string message, string title, System.Windows.Forms.MessageBoxButtons buttons, System.Windows.Forms.MessageBoxIcon icon)
		{
			using (var loop = new NSAutoreleasePool())
			using (var alert = new NSAlert())
			{
				alert.MessageText = title;
				alert.InformativeText = message;

				switch (icon)
				{
					case MessageBoxIcon.Error:
						alert.AlertStyle = NSAlertStyle.Critical;
						break;
					case MessageBoxIcon.Warning:
						alert.AlertStyle = NSAlertStyle.Warning;
						break;
					default:
						alert.AlertStyle = NSAlertStyle.Informational;
						break;
				}

				switch (buttons)
				{
					case MessageBoxButtons.AbortRetryIgnore:
						alert.AddButton(Labels.Abort);
						alert.AddButton(Labels.Retry);
						alert.AddButton(Labels.Ignore);
						break;
					case MessageBoxButtons.OK:
						alert.AddButton(Labels.Ok);
						break;
					case MessageBoxButtons.OKCancel:
						alert.AddButton(Labels.Ok);
						alert.AddButton(Labels.Cancel);
						break;
					case MessageBoxButtons.RetryCancel:
						alert.AddButton(Labels.Retry);
						alert.AddButton(Labels.Cancel);
						break;
					case MessageBoxButtons.YesNo:
						alert.AddButton(Labels.Yes);
						alert.AddButton(Labels.No);
						break;
					case MessageBoxButtons.YesNoCancel:
						alert.AddButton(Labels.Yes);
						alert.AddButton(Labels.No);
						alert.AddButton(Labels.Cancel);
						break;
					default:
						throw new ArgumentException();
				}

				NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
				var result = alert.RunModal();

				switch (buttons)
				{
					case MessageBoxButtons.AbortRetryIgnore:
						return result == (int)NSAlertButtonReturn.First ? DialogResult.Abort : (result == (int)NSAlertButtonReturn.Second ? DialogResult.Retry : DialogResult.Ignore);
					case MessageBoxButtons.OK:
						return DialogResult.OK;
					case MessageBoxButtons.OKCancel:
						return result == (int)NSAlertButtonReturn.First ? DialogResult.OK : DialogResult.Cancel;
					case MessageBoxButtons.RetryCancel:
						return result == (int)NSAlertButtonReturn.First ? DialogResult.Retry : DialogResult.Cancel;
					case MessageBoxButtons.YesNo:
						return result == (int)NSAlertButtonReturn.First ? DialogResult.Yes : DialogResult.No;
					case MessageBoxButtons.YesNoCancel:
						return result == (int)NSAlertButtonReturn.First ? DialogResult.Yes : (result == (int)NSAlertButtonReturn.Second ? DialogResult.No : DialogResult.Cancel);
					default:
						throw new ArgumentException();
				}

			}
		}
	}
}

