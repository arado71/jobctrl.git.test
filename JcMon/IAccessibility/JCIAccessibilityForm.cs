using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.JcMon.Common;
using Tct.JcMon.IAccessibility.Plugin;

namespace Tct.JcMon.IAccessibility
{
	public partial class JCIAccessibilityForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public TreeView TreeView => treeView;
		private IAccessibilityCapturePresenter presenter;
		private readonly HighlightRectangle highlightRectangle;
		private List<WindowObject> accessibleWindowObjects;
		private const string UNESCAPE_URL_CHARS = "UnescapeURLChars";

		public string HotKey
		{
			get
			{
				return hotkeyLabel.Text;
			}
			set
			{
				Debug.Assert(!hotkeyLabel.InvokeRequired);
				hotkeyLabel.Text = value;
			}
		}

		public JCIAccessibilityForm()
		{
			highlightRectangle = new HighlightRectangle();
			accessibleWindowObjects = new List<WindowObject>();
			WinApi.EnumWindowsProc delEnumfunc = new WinApi.EnumWindowsProc(TopLevelWindows);
			bool bSuccessful = WinApi.EnumDesktopWindows(IntPtr.Zero, delEnumfunc, IntPtr.Zero);
#if !JcMon
			Icon = ActivityRecorderClient.Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
#endif
			InitializeComponent();
		}

		public void refreshTree()
		{
			treeView.Nodes.Clear();
			treeView.Nodes.Add(presenter.GetTree(((WindowObject)hwndComboBox.Items[hwndComboBox.SelectedIndex]).Hwnd));

		}

		private bool TopLevelWindows(IntPtr hwnd, int lParam)
		{
			// Is it visible? 
			uint style = WinApi.GetWindowLong(hwnd, WinApi.GWL_STYLE);

			if (IsBitSet(style, (int)WinApi.WS_EX.WS_VISIBLE))
			{
				string title = GetWindowText(hwnd);
				uint processId;
				WinApi.GetWindowThreadProcessId(hwnd, out processId);
				Process process = Process.GetProcessById((int)processId);
				var wo = new WindowObject { Hwnd = hwnd, Title = $"{hwnd} - [{process.ProcessName}] - {title}" };
				accessibleWindowObjects.Add(wo);
				if (lastSelectedHwnd != IntPtr.Zero && lastSelectedHwnd == hwnd) hwndComboBox.SelectedItem = wo;
			}
			return true;
		}

		internal static string GetWindowText(IntPtr hWnd)
		{
			System.Text.StringBuilder windowName = new System.Text.StringBuilder(WinApi.MAX_PATH + 1);
			WinApi.GetWindowText(hWnd, windowName, WinApi.MAX_PATH);
			return windowName.ToString();
		}

		internal static bool IsBitSet(uint flags, int bit)
		{
			return (flags & bit) == bit;
		}

		private void refreshButton_Click(object sender, EventArgs e)
		{
			refreshProcessList(true);
		}

		private IntPtr lastSelectedHwnd = IntPtr.Zero;

		private void refreshProcessList(bool trySelectLast)
		{
			if (hwndComboBox.Items.Count > 0 && trySelectLast) 
				lastSelectedHwnd = ((WindowObject)hwndComboBox.Items[hwndComboBox.SelectedIndex]).Hwnd;
			else
				lastSelectedHwnd = IntPtr.Zero;
			accessibleWindowObjects = new List<WindowObject>();
			WinApi.EnumWindowsProc delEnumfunc = new WinApi.EnumWindowsProc(TopLevelWindows);
			bool bSuccessful = WinApi.EnumDesktopWindows(IntPtr.Zero, delEnumfunc, IntPtr.Zero);
		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var treeNode = e.Node;
			if (treeNode?.Tag != null && treeNode?.Tag is AccessibleItem acItem)
			{
				nameTextBox.Text = acItem.Name;
				roleTextBox.Text = acItem.Role;
				descTextBox.Text = acItem.Description;
				textTextBox.Text = acItem.TextValue;
				highlightRectangle.Location = new Rectangle(acItem.X, acItem.Y, acItem.Width, acItem.Height);
				highlightRectangle.Visible = true;
				//paramTextBox.Text = string.Join(",", acItem.Path);
			}
			else
			{
				nameTextBox.Text = "";
				roleTextBox.Text = "";
				descTextBox.Text = "";
				textTextBox.Text = "";
				paramTextBox.Text = "";
				highlightRectangle.Visible = false;
			}
		}

		System.Windows.Forms.Timer scrTimer = new System.Windows.Forms.Timer();

		private WinApi.WinEventDelegate accessibilityDelegate;
		private IntPtr winEventPtr = IntPtr.Zero;

		private void JCIAccessibilityForm_Load(object sender, EventArgs e)
		{
			scrCheckBox.Checked = WinApi.IsScreenReaderRunning();
			accessibilityDelegate = winEventHook;
			winEventPtr = WinApi.SetWinEventHook(WinApi.SystemEventContants.EVENT_SYSTEM_ALERT, WinApi.SystemEventContants.EVENT_SYSTEM_ALERT, IntPtr.Zero,
				accessibilityDelegate, 0, 0, WinApi.WinEventFlags.WINEVENT_OUTOFCONTEXT);
			//Automation.AddAutomationFocusChangedEventHandler(new AutomationFocusChangedEventHandler((x, y) => { string s = ((AutomationElement)x).Current.Name; }));
			scrTimer.Tick += ScrTimer_Tick;
			scrTimer.Interval = 1000;
			scrTimer.Start();
			windowObjectBindingSource.DataSource = accessibleWindowObjects;
			presenter = new IAccessibilityCapturePresenter(this);
		}

		private static void winEventHook(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, uint idObject, uint idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (idObject == 1)
			{
				Accessibility.IAccessible accessible;
				object child;
				var res = WinApi.AccessibleObjectFromEvent(hwnd, idObject, idChild, out accessible, out child);
				log.Debug($"Message sent to Chrome window {hwnd}. Result: {res}");
			}
		}

		private void ScrTimer_Tick(object sender, EventArgs e)
		{
			scrTimer.Stop();
			scrCheckBox.Checked = WinApi.IsScreenReaderRunning();
			scrTimer.Start();
		}

		private void nameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string parameters = null;
			if (unescapeUrlCheckBox.Checked) parameters = UNESCAPE_URL_CHARS;
			if (treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.Name, parameters);
		}

		private void roleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string parameters = null;
			if (unescapeUrlCheckBox.Checked) parameters = UNESCAPE_URL_CHARS;
			if (treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.Role, parameters);
		}

		private void descriptionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string parameters = null;
			if (unescapeUrlCheckBox.Checked) parameters = UNESCAPE_URL_CHARS;
			if (treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.Description, parameters);
		}

		private void textToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string parameters = null;
			if (unescapeUrlCheckBox.Checked) parameters = UNESCAPE_URL_CHARS;
			if (treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.Text, parameters);
		}

		private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				treeView.SelectedNode = e.Node;
				contextMenuStrip.Show(Cursor.Position);
			}
		}

		private void compileButton_Click(object sender, EventArgs e)
		{
			IAccessibilityCapturePresenter.CompileQuery(paramTextBox.Text);
		}

		private void numericUpDown_ValueChanged(object sender, EventArgs e)
		{
			IAccessibilityCapturePresenter.UpdateQueryInterval(decimal.ToInt32(numericUpDown.Value));
		}

		private void hwndComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (hwndComboBox.SelectedIndex == -1) return;
			var selected = ((WindowObject)hwndComboBox.Items[hwndComboBox.SelectedIndex]).Hwnd;
			presenter.HandleSelectedHwndChanged(selected);
			treeView.Nodes.Clear();
			treeView.Nodes.Add(presenter.GetTree(selected));
		}

		private void hwndComboBox_DropDown(object sender, EventArgs e)
		{
			accessibleWindowObjects.Clear();
			WinApi.EnumWindowsProc delEnumfunc = new WinApi.EnumWindowsProc(TopLevelWindows);
			bool bSuccessful = WinApi.EnumDesktopWindows(IntPtr.Zero, delEnumfunc, IntPtr.Zero);
			if (!bSuccessful) MessageBox.Show("Can't get window objects.");
			windowObjectBindingSource.DataSource = accessibleWindowObjects;
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			saveFileDialog.Filter = "Text Files | *.txt";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				presenter.SaveStructure(saveFileDialog.FileName);
			}
		}

		public void showProgressMarquee()
		{
			progressBar.Style = ProgressBarStyle.Marquee;
			progressBar.Visible = true;
		}

		public void hideProgress()
		{
			progressBar.Visible = false;
		}

		private void JCIAccessibilityForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (winEventPtr != IntPtr.Zero) WinApi.UnhookWinEvent(winEventPtr);
		}

		private void unescapeUrlCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			int unescapeIndex;
			if(!string.IsNullOrWhiteSpace(paramTextBox.Text))
			{
				if (unescapeUrlCheckBox.Checked && (unescapeIndex = paramTextBox.Text.IndexOf(UNESCAPE_URL_CHARS, StringComparison.OrdinalIgnoreCase)) < 0)
				{
					var parenthesisIndex = paramTextBox.Text.IndexOf(')');
					if (parenthesisIndex < 0) { paramTextBox.Text = paramTextBox.Text + "(UnescapeUrlChars)"; return; }
					paramTextBox.Text.Insert(parenthesisIndex, ",UnescapeUrlChars");
				}
				if (!unescapeUrlCheckBox.Checked && (unescapeIndex = paramTextBox.Text.IndexOf(UNESCAPE_URL_CHARS, StringComparison.OrdinalIgnoreCase)) >= 0)
				{
					int parUnescapeIndex;
					if ((parUnescapeIndex = paramTextBox.Text.IndexOf("(unescapeurlchars)", StringComparison.OrdinalIgnoreCase)) >= 0)
					{
						paramTextBox.Text = paramTextBox.Text.Substring(0, parUnescapeIndex);
					} else
					{
						paramTextBox.Text = paramTextBox.Text.Substring(0, unescapeIndex) + paramTextBox.Text.Substring(unescapeIndex + UNESCAPE_URL_CHARS.Length, paramTextBox.Text.Length - (unescapeIndex + UNESCAPE_URL_CHARS.Length));
					}
				}
			}
		}

		private void searchButton_Click(object sender, EventArgs e)
		{
			presenter.SelectNextMatch(searchTextBox.Text);
		}

		private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				searchButton.PerformClick();
				e.Handled = true;
			}
		}
	}
}
