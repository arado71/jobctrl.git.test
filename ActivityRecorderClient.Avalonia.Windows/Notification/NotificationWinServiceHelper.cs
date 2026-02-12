using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Notification
{
	public static class NotificationWinServiceHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly IEnumerable<JcButton> defaultButton = new List<JcButton> { new JcButton() { Id = "OK", Text = Labels.Ok } };

		// ReSharper disable AccessToDisposedClosure
		public static string ShowServerNotification(JcForm jcForm)
		{
			const int sidePadding = 12;
			const int topPadding = 25;
			const int bottomPadding = 16;
			const int contentWidth = 350;
			const int btnLeftMargin = 30;
			const int btnTopMargin = 16;
			var messageBox = jcForm.MessageBox;
			if (messageBox == null) throw new InvalidOperationException("messageBox is null");
			using (var form = new NotiForm())
			{
				form.Text = messageBox.Title;
				form.Size = new Size(contentWidth + 2 * sidePadding + 16, 200);
				form.SizeGripStyle = SizeGripStyle.Hide;
				form.StartPosition = FormStartPosition.CenterScreen;
				form.MaximizeBox = false;
				if (string.IsNullOrEmpty(jcForm.CloseButtonId))
				{
					form.CloseBox = false;
				}
				form.FormClosing += (sender, e) =>
				{
					if (e.CloseReason == CloseReason.UserClosing) //UserClosing is the reason if we call Close() and also if we click on the red X
					{
						if (form.Tag == null) //we didn't click on any button
						{
							if (string.IsNullOrEmpty(jcForm.CloseButtonId))
							{
								e.Cancel = true;
							}
							else
							{
								form.Tag = jcForm.CloseButtonId;
							}
						}
					}
					else
					{
						//force close e.g. app exit
						log.Info("Notification form closing " + e.CloseReason);
					}
				};

				//add label
				var label = new Label() { AutoSize = true, MaximumSize = new Size(contentWidth, 0), };
				label.Text = messageBox.Text;
				label.Location = new Point(sidePadding, topPadding);
				form.Controls.Add(label);

				//add buttons
				var btnPanel = new FlowLayoutPanel()
				{
					AutoSize = true,
					MaximumSize = new Size(contentWidth, 0),
					Width = contentWidth,
					FlowDirection = FlowDirection.LeftToRight,
					Location = new Point(sidePadding, label.Location.Y + label.Height),
					WrapContents = true,
				};
				var buttons = messageBox.Buttons == null || messageBox.Buttons.Count == 0
					? defaultButton
					: messageBox.Buttons;
				foreach (var btn in buttons)
				{
					var button = new Button()
					{
						AutoEllipsis = false,
						AutoSize = true,
						AutoSizeMode = AutoSizeMode.GrowOnly,
						Size = new Size(75, 23),
						Margin = new Padding(btnLeftMargin, btnTopMargin, 0, 0),
						Text = string.IsNullOrEmpty(btn.Text) ? btn.Id : btn.Text,
						Tag = string.IsNullOrEmpty(btn.Id) ? btn.Text : btn.Id,
					};
					button.Click += (sender, __) =>
					{
						form.Tag = ((Control)sender).Tag;
						form.Close();
					};
					btnPanel.Controls.Add(button);
				}

				//center btnPanel
				int minX = int.MaxValue, maxX = int.MinValue;
				foreach (var ctrl in btnPanel.Controls.OfType<Control>())
				{
					minX = ctrl.Location.X < minX ? ctrl.Location.X : minX;
					maxX = ctrl.Location.X + ctrl.Width > maxX ? ctrl.Location.X + ctrl.Width : maxX;
				}
				var toRight = (contentWidth - maxX - minX) / 2;
				if (toRight > 0)
				{
					btnPanel.Location = new Point(btnPanel.Location.X + toRight, btnPanel.Location.Y);
				}
				form.Controls.Add(btnPanel);

				//set final size
				form.ClientSize = new Size(contentWidth + 2 * sidePadding, bottomPadding + btnPanel.Location.Y + btnPanel.Height);
				form.MinimumSize = form.Size;
				form.MaximumSize = form.Size;
				form.TopMost = true;

				form.ShowDialog(((PlatformWin.PlatformFactory)PlatformWin.Factory).MainForm);
				return (string)form.Tag;
			}
		}
		// ReSharper restore AccessToDisposedClosure

		private class NotiForm : Form
		{
			public bool CloseBox { get; set; }

			public NotiForm()
			{
				CloseBox = true;
			}

			//http://www.codeproject.com/Articles/20379/Disabling-Close-Button-on-Forms
			private const int CP_NOCLOSE_BUTTON = 0x200;
			protected override CreateParams CreateParams
			{
				get
				{
					if (!CloseBox)
					{
						CreateParams myCp = base.CreateParams;
						myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
						return myCp;
					}
					return base.CreateParams;
				}
			}
		}
	}
}
