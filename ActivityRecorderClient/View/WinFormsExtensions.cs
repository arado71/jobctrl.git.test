using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public static class WinFormsExtensions
	{
		public static void SetFormStartPositionCenterScreen(this Form form)
		{
			if (SystemInformation.MonitorCount == 1)
			{
				form.StartPosition = FormStartPosition.CenterScreen;
			}
			else
			{
				var priScreenArea = Screen.PrimaryScreen.WorkingArea;
				form.StartPosition = FormStartPosition.Manual;
				form.Location = new Point((priScreenArea.Width - form.Width) / 2, (priScreenArea.Height - form.Height) / 2);
			}
		}

		//don't leak memory due ToolStripItem registering to SystemEvents.UserPreferenceChanged
		public static void RemoveAtWithDispose(this ToolStripItemCollection coll, int idx)
		{
			var removed = coll[idx];
			coll.RemoveAt(idx);
			removed.Dispose(); //DropDownItems are also Disposed (recursively) if applicable
		}

		//don't leak memory due ToolStripItem registering to SystemEvents.UserPreferenceChanged
		public static void ClearWithDispose(this ToolStripItemCollection coll)
		{
			var items = coll.OfType<IDisposable>().ToList();
			coll.Clear();
			items.ForEach(n => n.Dispose()); //DropDownItems are also Disposed (recursively) if applicable
		}

		public static void RemoveRow(this TableLayoutPanel tableLayoutPanel, int rowNumber)
		{
			tableLayoutPanel.SuspendLayout();
			for (int i = 0; i < tableLayoutPanel.Controls.Count; i++)
			{
				var control = tableLayoutPanel.Controls[i];
				int row = tableLayoutPanel.GetRow(control);
				if (row == rowNumber)
				{
					tableLayoutPanel.Controls.Remove(control);
					i--;
				}
			}
			tableLayoutPanel.RowStyles.RemoveAt(rowNumber);

			foreach (Control control in tableLayoutPanel.Controls)
			{
				int row = tableLayoutPanel.GetRow(control);
				if (row > rowNumber)
				{
					tableLayoutPanel.SetRow(control, row - 1);
				}
			}
			tableLayoutPanel.ResumeLayout();
		}

		public static void InsertRow(this TableLayoutPanel tableLayoutPanel, int rowNumber, RowStyle rowStyle, Control[] rowControls)
		{
			if (rowStyle == null) throw new ArgumentNullException();
			if (rowControls == null) throw new ArgumentNullException();
			tableLayoutPanel.SuspendLayout();
			tableLayoutPanel.RowCount++;
			tableLayoutPanel.RowStyles.Insert(rowNumber, rowStyle);
			foreach (Control control in tableLayoutPanel.Controls)
			{
				int row = tableLayoutPanel.GetRow(control);
				if (row >= rowNumber)
				{
					tableLayoutPanel.SetRow(control, row + 1);
				}
			}
			int i = 0;
			foreach (var rowControl in rowControls)
			{
				tableLayoutPanel.Controls.Add(rowControl, i++, rowNumber);
			}
			tableLayoutPanel.ResumeLayout();
		}

		//http://rajeshkm.blogspot.hu/2006/11/adjust-combobox-drop-down-list-width-c.html
		public static void SetComboScrollWidth(this ComboBox cb)
		{
			SetComboScrollWidth(cb, null);
		}

		public static void SetComboScrollWidth(this ComboBox cb, Func<object, string> displayName)
		{
			if (cb == null) return;
			try
			{
				int width = cb.Width;
				using (var g = cb.CreateGraphics())
				{
					//checks if a scrollbar will be displayed.
					//If yes, then get its width to adjust the size of the drop down list.
					int vertScrollBarWidth = (cb.Items.Count > cb.MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;

					//Loop through list items and check size of each items.
					//set the width of the drop down list to the width of the largest item.
					foreach (var s in cb.Items)
					{
						if (s != null)
						{
							var text = displayName == null ? s.ToString() : displayName(s);
							var newWidth = (int)g.MeasureString(text.Trim(), cb.Font).Width + vertScrollBarWidth;
							if (width < newWidth)
							{
								width = newWidth;
							}
						}
					}
				}
				cb.DropDownWidth = width;
			}
			catch
			{
			}
		}

		public static void SetCueBanner(this TextBox textBox, string text, bool showFocused = false)
		{
			SendMessage(textBox.Handle, EM_SETCUEBANNER, showFocused ? 1 : 0, text);
		}

		public static void SetCueBanner(this ComboBox cb, string text)
		{
			SendMessage(cb.Handle, CB_SETCUEBANNER, 0, text);
		}

		public static void SetVisibleAndTag(this Control ctrl, bool value)
		{
			if (!(ctrl.Tag is bool) || (bool)ctrl.Tag != value)
			{
				ctrl.Visible = value;
				ctrl.Tag = value;
			}
		}

		//http://stackoverflow.com/questions/579665/how-can-i-show-a-systray-tooltip-longer-than-63-chars
		private static readonly BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
		private static readonly FieldInfo textNotifyIcon = typeof(NotifyIcon).GetField("text", hidden);
		private static readonly FieldInfo addedNotifyIcon = typeof(NotifyIcon).GetField("added", hidden);
		private static readonly FieldInfo windowNotifyIcon = typeof(NotifyIcon).GetField("window", hidden);
		private static readonly MethodInfo updateIconNotifyIcon = typeof(NotifyIcon).GetMethod("UpdateIcon", hidden);
		public static void SetText(this NotifyIcon ni, string text)
		{
			if (text == null || text.Length < 64) //use the normal method
			{
				ni.Text = text;
				return;
			}
			if (ni.Text == text) return; //no change
			if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");
			try //use reflection to bypass limitation
			{
				textNotifyIcon.SetValue(ni, text);
				if ((bool)addedNotifyIcon.GetValue(ni))
				{
					updateIconNotifyIcon.Invoke(ni, new object[] { true });
				}
			}
			catch (Exception ex)
			{
				Debug.Fail("Unexpected error setting the Text of the NotifyIcon: " + ex.Message);
				ni.Text = text.Substring(0, 60) + "..."; //fall back to the original method
			}
		}

		/// <summary>
		/// Shows ContextMenuStrip of the NotifyIcon and uses the last know position.
		/// </summary>
		/// <param name="ni">The NotifyIcon</param>
		public static void ShowContextMenuStrip(this NotifyIcon ni)
		{
			if (ni.ContextMenuStrip == null) return;
			var window = windowNotifyIcon.GetValue(ni) as NativeWindow;
			if (window == null) return;
			SetForegroundWindow(new HandleRef(window, window.Handle)); //I'm not sure why this is needed, but sure solves some problems
			var cmMenu = ni.ContextMenuStrip;
			cmMenu.Show();
		}

		private const int EM_SETCUEBANNER = 0x1501;
		private const int CB_SETCUEBANNER = 0x1703;

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
		private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, EntryPoint = "SetForeGroundWindow")]
		private static extern bool SetForegroundWindow(HandleRef hWnd);

	}
}
