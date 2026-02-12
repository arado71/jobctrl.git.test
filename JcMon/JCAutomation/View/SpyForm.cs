using JCAutomation.Data;
using JCAutomation.Extraction;
using JCAutomation.Managers;
using Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Hotkeys;

namespace JCAutomation.View
{
	public partial class SpyForm : Form
	{
		public CustomPlugin Plugin { get; set; }
		public AutomationElement TargetElement { get; set; }
		private readonly HighlightRectangle highlight;
		private readonly SynchronizationContext context;
		private readonly ScriptCaptureManager scriptCaptureManager = new ScriptCaptureManager();

		public SpyForm()
		{
			InitializeComponent();

			highlight = new HighlightRectangle();
			txtLog.GotFocus += (_, __) => HideCaret(txtLog.Handle);
			chbUpdate_CheckedChanged(chbUpdate, EventArgs.Empty);
			AutomationElementTree.LogFunc = (msg, ex) => Log(msg, ex);
			context = SynchronizationContext.Current;
			scriptCaptureManager.Captured += HandleCaptured;

			Automation.AddAutomationFocusChangedEventHandler(focusChangedHandler);

			miSavePathToElement.Text = "Copy path to clipboard";
		}

		private bool working = false;
		private void focusChangedHandler(object sender, AutomationFocusChangedEventArgs e)
		{

			if (sender is AutomationElement element)
			{
				AutomationElementInfo info = new AutomationElementInfo(element);
				if (info.ProcessName != "Teams.exe") return;
				if (working) return;
				Invoke((MethodInvoker) delegate
				{
					working = true;
					GetElementInfo(delegate
					{
					   return element;
					});
					working = false;
				});

			}
		}

		#region events

		private void jcmon_Load(object sender, EventArgs e)
		{
			Icon = new Icon(Icon, Icon.Size);   // ez qrva jó :)) 
			scriptCaptureManager.Start();
			numericUpDown1.Value = Configuration.CaptureInterval;

			if (Plugin == null)
			{
				RegisterHotkey();
				GetDesktopElement();
			}
			else
			{
				btnPlugins.Visible = false;
				Func<AutomationElement> getElementFuc = delegate
				{
					if (TargetElement != null)
					{
						return TargetElement;
					}
					foreach (Process proc in Process.GetProcesses())
					{
						try
						{
							int tickCount = Environment.TickCount;
							AutomationElement element = Plugin.Capture(proc.MainWindowHandle, proc.Id,
								proc.ProcessName + ".exe");
							if (element == null) continue;
							chbUpdate.Checked = true;
							Log("Found element in " + (Environment.TickCount - tickCount) + "ms", null);
							return element;
						}
						catch
						{
						}
					}
					return null;
				};
				GetElementInfo(getElementFuc);
			}
		}

		private void jcmon_FormClosed(object sender, FormClosedEventArgs e)
		{
			highlight.Visible = false;
		}

		private void btnPlugins_Click(object sender, EventArgs e)
		{
			new CodeEditorForm().Show();
		}

		private void chbUpdate_CheckedChanged(object sender, EventArgs e)
		{
			timerUpdate.Enabled = chbUpdate.Checked;
		}

		#region context menu

		private void cmMenu_Opening(object sender, CancelEventArgs e)
		{
			miGenerateCodeStale.Available = (Control.ModifierKeys & Keys.Shift) != Keys.None;
		}

		private void miGenerateCode_Click(object sender, EventArgs e)
		{
			using (new WaitCursor())
			{
				TreeViewItem selectedTreeViewItem = GetSelectedTreeViewItem();
				if (selectedTreeViewItem != null)
				{
					bool flag;
					new CodeEditorForm(GetTreeForItem(selectedTreeViewItem, out flag)).Show();
				}
			}
		}

		private void miGenerateCodeStale_Click(object sender, EventArgs e)
		{
			using (new WaitCursor())
			{
				TreeViewItem selectedTreeViewItem = GetSelectedTreeViewItem();
				if (selectedTreeViewItem != null)
				{
					bool flag;
					new CodeEditorForm(GetTreeForItemStale(selectedTreeViewItem, out flag)).Show();
				}
			}
		}

		private void miRefresh_Click(object sender, EventArgs e)
		{
			TreeViewItem selectedTreeViewItem = GetSelectedTreeViewItem();
			if (selectedTreeViewItem != null)
			{
				WaitCursor cursor = new WaitCursor();
				try
				{
					selectedTreeViewItem.Refresh(true);
				}
				catch (Exception exception)
				{
					Log("Unable to refresh item", exception);
				}
				finally
				{
					if (cursor != null)
					{
						cursor.Dispose();
					}
				}
			}
		}

		private void miSavePathToElement_Click(object sender, EventArgs e)
		{
			using (new WaitCursor())
			{
				TreeViewItem selectedTreeViewItem = GetSelectedTreeViewItem();
				if (selectedTreeViewItem != null)
				{
					SavePath(selectedTreeViewItem);
				}
			}
		}

		#endregion

		private void timerUpdate_Tick(object sender, EventArgs e)
		{
			UpdateSelectedElementInfo();
		}

		#region tree

		private void tvNodes_AfterSelect(object sender, TreeViewEventArgs e)
		{
			UpdateSelectedElementInfo();
		}

		private void tvNodes_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			using (new WaitCursor())
			{
				TreeViewItem tag = (TreeViewItem)e.Node.Tag;
				try
				{
					bool flag = tag.ExpandOnDemand();
					e.Cancel = !flag;
				}
				catch (Exception exception)
				{
					Log("Unable to expand item", exception);
				}
			}
		}

		private void tvNodes_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				tvNodes.SelectedNode = e.Node;
				cmMenu.Show(Cursor.Position);
			}
		}

		#endregion

		#endregion

		#region jcExtract
		private void HandleCaptured(object sender, CaptureEventArgs eventArgs)
		{
			context.Post(_ => ShowCapture(eventArgs.ScriptCapture), null);
		}
		private void ShowCapture(ScriptCapture scriptCapture)
		{
			dataGridView1.Rows.Clear();
			lblHandle.Text = "Handle: " + scriptCapture.WindowHandle + " (" + scriptCapture.ProcessName + ")";
			foreach (var capturedValue in scriptCapture.Values)
			{
				dataGridView1.Rows.Add(capturedValue.Key, capturedValue.Value, capturedValue.Time);
			}
		}
		private void HandleCompileClicked(object sender, EventArgs e)
		{
			try
			{
				scriptCaptureManager.Stop();
				Configuration.ProcessFuncs = AutomationScriptHelper.Compile(txtScript.Text);
				scriptCaptureManager.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		#endregion
		private void RegisterHotkey()
		{
			Program.HotkeyService.HotkeyPressed +=
				new EventHandler<SingleValueEventArgs<Hotkey>>(HotkeyService_HotkeyPressed);
			Hotkey hotkey = null;
			for (char ch = 'C'; ch <= 'Z'; ch = (char)(ch + '\x0001'))
			{
				hotkey = new Hotkey
				{
					Control = true,
					Shift = true,
					KeyCode = (Keys)Enum.Parse(typeof(Keys), ch.ToString())
				};
				try
				{
					Program.HotkeyService.Register(hotkey);
				}
				catch (Exception)
				{
					hotkey = null;
				}
				if (hotkey != null)
				{
					break;
				}
			}
			if (hotkey != null)
			{
				Log("Registered hotkey " + hotkey, null);
			}
			else
			{
				Log("Unable to reister hotkey", null);
			}
		}

		private void SavePath(TreeViewItem treeViewItem)
		{
			bool flag;
			AutomationElementTree treeForItem = GetTreeForItem(treeViewItem, out flag);
			Clipboard.SetText((flag ? ("IMPORTANT *** Using stale data ***" + Environment.NewLine) : "") +
							  TreeDumper.Dump(treeForItem));
		}

		private void ocr(bool enabled)
		{
			pbtOcr.Enabled = enabled;
			pbtOcr.Image = enabled ? Resources.ocr : Resources.ocrd;
			pbtOcr.Cursor = enabled ? Cursors.Hand : Cursors.Default;
		}

		private void UpdateSelectedElementInfo()
		{
			TreeViewItem selectedTreeViewItem = GetSelectedTreeViewItem();
			if (selectedTreeViewItem != null)
			{
				AutomationElementInfo info = selectedTreeViewItem.Value.Value;
				WaitCursor cursor = new WaitCursor();
				try
				{
					selectedTreeViewItem.Refresh(false);
					highlight.Location = info.BoundingRectangle;
					highlight.Visible = true;
					txtName.Text = info.Name;
					txtValue.Text = info.Value;
					txtText.Text = info.Text;
					txtVisible.Text = info.Visibility.ToString();
					ocr(info.Name.IsEmpty() && info.Value.IsEmpty() && info.Text.IsEmpty());
				}
				catch (Exception exception)
				{
					Log("Unable to update element", exception);
					selectedTreeViewItem.Current.Text = "Error updating element";
					highlight.Visible = false;
					txtName.Text = "";
					txtValue.Text = "";
					txtText.Text = "";
					txtVisible.Text = "";
					ocr(true);
				}
				finally
				{
					if (cursor != null)
					{
						cursor.Dispose();
					}
				}
			}
		}

		private void UpdateTreeView(AutomationElementTree root)
		{
			TreeNode node;
			tvNodes.Focus();
			TreeNode node2 = TreeViewItem.BuildTree(root, out node);
			tvNodes.Nodes.Clear();
			tvNodes.Nodes.AddRange(node2.Nodes.OfType<TreeNode>().ToArray<TreeNode>());
			tvNodes.SelectedNode = node;
		}

		private void GetDesktopElement()
		{
			GetElementInfo(delegate
			{
				IntPtr hwnd = GetDesktopWindow();
				if (hwnd == IntPtr.Zero)
				{
					throw new Exception("Desktop window not found");
				}
				return AutomationElement.FromHandle(hwnd);
			});
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetDesktopWindow();

		private void GetElementInfo(Func<AutomationElement> getElementFuc)
		{
			using (new WaitCursor())
			{
				GetElementInfoImpl(getElementFuc);
			}
		}

		private string process;
		private void GetElementInfoImpl(Func<AutomationElement> getElementFuc)
		{
			AutomationElementInfo info;
			try
			{
				AutomationElement element = getElementFuc();
				if (element == null)
				{
					Log("Unable to get element", null);
					return;
				}
				info = new AutomationElementInfo(element);
				process = info.ProcessName;
			}
			catch (Exception exception)
			{
				Log("Unable to find element", exception);
				return;
			}
			UpdateSelectedElementInfo();
			txtLog.AppendText(
				string.Concat(new object[]
				{
					"Found element", Environment.NewLine, info, Environment.NewLine, "==============================================",
					Environment.NewLine
				}));
			if (chbPath.Checked)
			{
				try
				{
					AutomationElementTree root = AutomationElementTree.CreatePathToElement(info);
					UpdateTreeView(root);
					txtLog.AppendText(TreeDumper.Dump(root));
					txtLog.AppendText(Environment.NewLine + "==============================================" + Environment.NewLine);
				}
				catch (Exception exception2)
				{
					Log("Unable to get path to element", exception2);
				}
			}
			else
			{
				AutomationElementTree tree = AutomationElementTree.CreateElement(info);
				UpdateTreeView(tree);
			}
			// jcExtract
			scriptCaptureManager.Stop();
			txtScript.Text = string.Empty;
			dataGridView1.Rows.Clear();
		}

		private TreeViewItem GetSelectedTreeViewItem()
		{
			TreeNode selectedNode = tvNodes.SelectedNode;
			if (selectedNode == null)
			{
				return null;
			}
			return (selectedNode.Tag as TreeViewItem);
		}

		private AutomationElementTree GetTreeForItem(TreeViewItem treeViewItem, out bool isStale)
		{
			try
			{
				AutomationElementTree tree = AutomationElementTree.CreatePathToElement(treeViewItem.Value.Value);
				isStale = false;
				return tree;
			}
			catch (Exception exception)
			{
				Log("Using stale data", exception);
				return GetTreeForItemStale(treeViewItem, out isStale);
			}
		}

		private AutomationElementTree GetTreeForItemStale(TreeViewItem treeViewItem, out bool isStale)
		{
			treeViewItem.Value.IsSelected = true;
			TreeNode current = treeViewItem.Current;
			while ((current.Parent != null) && (current.Parent.Tag is TreeViewItem))
			{
				current = current.Parent;
			}
			AutomationElementTree item = ((TreeViewItem)current.Tag).Value;
			Queue<AutomationElementTree> queue = new Queue<AutomationElementTree>();
			queue.Enqueue(item);
			while (queue.Count > 0)
			{
				AutomationElementTree tree2 = queue.Dequeue();
				if (tree2.IsSelected && (tree2 != treeViewItem.Value))
				{
					tree2.IsSelected = false;
				}
				foreach (AutomationElementTree tree3 in tree2.Children)
				{
					queue.Enqueue(tree3);
				}
			}
			isStale = true;
			return item;
		}

		[DllImport("user32.dll")]
		private static extern bool HideCaret(IntPtr hWnd);

		private void HotkeyService_HotkeyPressed(object sender, SingleValueEventArgs<Hotkey> e)
		{
			GetElementInfo(delegate
			{
				WinApi.ScreenReaderOn();
				System.Drawing.Point position = Cursor.Position;
				return AutomationElement.FromPoint(new System.Windows.Point((double)position.X, (double)position.Y));
			});
		}


		private void Log(string msg, Exception ex = null)
		{
			if (!txtLog.IsDisposed)
			{
				txtLog.AppendText(msg);
				txtLog.AppendText(Environment.NewLine);
				if (ex != null)
				{
					txtLog.AppendText(ex.ToString());
					txtLog.AppendText(Environment.NewLine);
				}
			}
		}

		private void generateQueryStringUsingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!(sender is ToolStripMenuItem)) return;
			var mi = sender as ToolStripMenuItem;
			var type = ((ToolStripMenuItem)sender).Text;
			switch (type)
			{
				case null:
					return;
				case "value":
					if (txtValue.Text == string.Empty) return;
					break;
				case "text":
					if (txtText.Text == string.Empty) return;
					break;
				case "name":
					if (txtName.Text == string.Empty) return;
					break;
				case "radioname":
					break;
				case "radiovalue":
					break;
				default:
					if (txtValue.Text != string.Empty) type = "value";
					else if (txtText.Text != string.Empty) type = "text";
					else if (txtName.Text != string.Empty) type = "name";
					else return;
					break;
			}
			using (new WaitCursor())
			{
				TreeViewItem selectedTreeViewItem = GetSelectedTreeViewItem();
				var index = new List<int>();
				if (selectedTreeViewItem != null)
				{
					var node = selectedTreeViewItem.Current;
					while (true)
					{
						if (process != null && (node.Parent == null || !node.Parent.Text.Contains(process)))
							break;
						var parent = node.Parent;
						if (parent == null)
							break;
						var ix = parent.IndexOf(node);
						index.Add(ix);
						node = parent;
					}
				}
				StringBuilder sb = new StringBuilder("myKey:*//");
				foreach (var i in index.Reverse<int>())
					sb.Append(string.Format("[index={0}]/", i));
				sb.Append(string.Format("/[{0}];", type));
				Clipboard.SetText(sb.ToString());
				txtScript.Text = sb.ToString();
			}
		}
		private void HandleIntervalChanged(object sender, EventArgs e)
		{
			Configuration.CaptureInterval = (int)numericUpDown1.Value;
		}
		private void pbtOcr_Click(object sender, EventArgs e)
		{

		}
	}
}
