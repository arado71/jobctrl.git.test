using JCAutomation.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using JcExtract;
using JCAutomation.Data;
using JCAutomation.Extraction;
using JCAutomation.Properties;
using JCAutomation.View;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Hotkeys;

namespace JCAutomation
{

	public partial class MainForm_old : Form
	{
		public CustomPlugin Plugin { get; set; }
		public AutomationElement TargetElement { get; set; }
		private readonly HighlightRectangle highlight;
		private IContainer components;
		private readonly SynchronizationContext context;
		private readonly ScriptCaptureManager scriptCaptureManager = new ScriptCaptureManager();

		public MainForm_old()
		{
			InitializeComponent();

			highlight = new HighlightRectangle();
			txtLog.GotFocus += (_, __) => HideCaret(this.txtLog.Handle);
			chbUpdate_CheckedChanged(this.chbUpdate, EventArgs.Empty);
			AutomationElementTree.LogFunc = (msg, ex) => this.Log(msg, ex);
			context = SynchronizationContext.Current;
			scriptCaptureManager.Captured += HandleCaptured;

			miSavePathToElement.Text = "Copy path to clipboard";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region events

		private void MainForm_Load(object sender, EventArgs e)
		{
			scriptCaptureManager.Start();

			Func<AutomationElement> getElementFuc = null;
			if (Plugin == null)
			{
				RegisterHotkey();
				GetDesktopElement();
			}
			else
			{
				this.btnPlugins.Visible = false;
				if (getElementFuc == null)
				{
					getElementFuc = delegate
					{
						if (this.TargetElement != null)
						{
							return this.TargetElement;
						}
						foreach (Process process in Process.GetProcesses())
						{
							try
							{
								int tickCount = Environment.TickCount;
								AutomationElement element = this.Plugin.Capture(process.MainWindowHandle, process.Id,
									process.ProcessName + ".exe");
								if (element != null)
								{
									this.chbUpdate.Checked = true;
									this.Log("Found element in " + (Environment.TickCount - tickCount) + "ms", null);
									return element;
								}
							}
							catch
							{
							}
						}
						return null;
					};
				}
				this.GetElementInfo(getElementFuc);
			}
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			this.highlight.Visible = false;
		}

		private void btnPlugins_Click(object sender, EventArgs e)
		{
			new CodeEditorForm().Show();
		}

		private void chbUpdate_CheckedChanged(object sender, EventArgs e)
		{
			this.timerUpdate.Enabled = this.chbUpdate.Checked;
		}

		#region context menu

		private void cmMenu_Opening(object sender, CancelEventArgs e)
		{
			this.miGenerateCodeStale.Available = (Control.ModifierKeys & Keys.Shift) != Keys.None;
		}

		private void miGenerateCode_Click(object sender, EventArgs e)
		{
			using (new WaitCursor())
			{
				TreeViewItem selectedTreeViewItem = this.GetSelectedTreeViewItem();
				if (selectedTreeViewItem != null)
				{
					bool flag;
					new CodeEditorForm(this.GetTreeForItem(selectedTreeViewItem, out flag)).Show();
				}
			}
		}

		private void miGenerateCodeStale_Click(object sender, EventArgs e)
		{
			using (new WaitCursor())
			{
				TreeViewItem selectedTreeViewItem = this.GetSelectedTreeViewItem();
				if (selectedTreeViewItem != null)
				{
					bool flag;
					new CodeEditorForm(this.GetTreeForItemStale(selectedTreeViewItem, out flag)).Show();
				}
			}
		}

		private void miRefresh_Click(object sender, EventArgs e)
		{
			TreeViewItem selectedTreeViewItem = this.GetSelectedTreeViewItem();
			if (selectedTreeViewItem != null)
			{
				WaitCursor cursor = new WaitCursor();
				try
				{
					selectedTreeViewItem.Refresh(true);
				}
				catch (Exception exception)
				{
					this.Log("Unable to refresh item", exception);
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
				TreeViewItem selectedTreeViewItem = this.GetSelectedTreeViewItem();
				if (selectedTreeViewItem != null)
				{
					this.SavePath(selectedTreeViewItem);
				}
			}
		}

		#endregion

		private void timerUpdate_Tick(object sender, EventArgs e)
		{
			this.UpdateSelectedElementInfo();
		}

		#region tree

		private void tvNodes_AfterSelect(object sender, TreeViewEventArgs e)
		{
			this.UpdateSelectedElementInfo();
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
					this.Log("Unable to expand item", exception);
				}
			}
		}

		private void tvNodes_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				this.tvNodes.SelectedNode = e.Node;
				this.cmMenu.Show(Cursor.Position);
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
				new EventHandler<SingleValueEventArgs<Hotkey>>(this.HotkeyService_HotkeyPressed);
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
				this.Log("Registered hotkey " + hotkey, null);
			}
			else
			{
				this.Log("Unable to reister hotkey", null);
			}
		}

		private void SavePath(TreeViewItem treeViewItem)
		{
			bool flag;
			AutomationElementTree treeForItem = this.GetTreeForItem(treeViewItem, out flag);
			Clipboard.SetText((flag ? ("IMPORTANT *** Using stale data ***" + Environment.NewLine) : "") +
							  TreeDumper.Dump(treeForItem));
		}


		private void UpdateSelectedElementInfo()
		{
			TreeViewItem selectedTreeViewItem = this.GetSelectedTreeViewItem();
			if (selectedTreeViewItem != null)
			{
				AutomationElementInfo info = selectedTreeViewItem.Value.Value;
				WaitCursor cursor = new WaitCursor();
				try
				{
					selectedTreeViewItem.Refresh(false);
					this.highlight.Location = info.BoundingRectangle;
					this.highlight.Visible = true;
					this.txtName.Text = info.Name;
					this.txtValue.Text = info.Value;
					this.txtText.Text = info.Text;
					this.txtVisible.Text = info.Visibility.ToString();
				}
				catch (Exception exception)
				{
					this.Log("Unable to update element", exception);
					selectedTreeViewItem.Current.Text = "Error updating element";
					this.highlight.Visible = false;
					this.txtName.Text = "";
					this.txtValue.Text = "";
					this.txtText.Text = "";
					this.txtVisible.Text = "";
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
			this.tvNodes.Focus();
			TreeNode node2 = TreeViewItem.BuildTree(root, out node);
			this.tvNodes.Nodes.Clear();
			this.tvNodes.Nodes.AddRange(node2.Nodes.OfType<TreeNode>().ToArray<TreeNode>());
			this.tvNodes.SelectedNode = node;
		}

		private void GetDesktopElement()
		{
			this.GetElementInfo(delegate
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
				this.GetElementInfoImpl(getElementFuc);
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
					this.Log("Unable to get element", null);
					return;
				}
				info = new AutomationElementInfo(element);
				process = info.ProcessName;
			}
			catch (Exception exception)
			{
				this.Log("Unable to find element", exception);
				return;
			}
			this.UpdateSelectedElementInfo();
			this.txtLog.AppendText(
				string.Concat(new object[]
				{
					"Found element", Environment.NewLine, info, Environment.NewLine, "==============================================",
					Environment.NewLine
				}));
			if (this.chbPath.Checked)
			{
				try
				{
					AutomationElementTree root = AutomationElementTree.CreatePathToElement(info);
					this.UpdateTreeView(root);
					this.txtLog.AppendText(TreeDumper.Dump(root));
					this.txtLog.AppendText(Environment.NewLine + "==============================================" + Environment.NewLine);
				}
				catch (Exception exception2)
				{
					this.Log("Unable to get path to element", exception2);
				}
			}
			else
			{
				AutomationElementTree tree = AutomationElementTree.CreateElement(info);
				this.UpdateTreeView(tree);
			}
			// jcExtract
			scriptCaptureManager.Stop();
			txtScript.Text = string.Empty;
			dataGridView1.Rows.Clear();
		}

		private TreeViewItem GetSelectedTreeViewItem()
		{
			TreeNode selectedNode = this.tvNodes.SelectedNode;
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
				this.Log("Using stale data", exception);
				return this.GetTreeForItemStale(treeViewItem, out isStale);
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
			this.GetElementInfo(delegate
			{
				System.Drawing.Point position = Cursor.Position;
				return AutomationElement.FromPoint(new System.Windows.Point((double)position.X, (double)position.Y));
			});
		}


		private void Log(string msg, Exception ex = null)
		{
			if (!this.txtLog.IsDisposed)
			{
				this.txtLog.AppendText(msg);
				this.txtLog.AppendText(Environment.NewLine);
				if (ex != null)
				{
					this.txtLog.AppendText(ex.ToString());
					this.txtLog.AppendText(Environment.NewLine);
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
				default:
					if (txtValue.Text != string.Empty) type = "value";
					else if (txtText.Text != string.Empty) type = "text";
					else if (txtName.Text != string.Empty) type = "name";
					else return;
					break;
			}
			using (new WaitCursor())
			{
				TreeViewItem selectedTreeViewItem = this.GetSelectedTreeViewItem();
				var index = new List<int>();
				if (selectedTreeViewItem != null)
				{
					var node = selectedTreeViewItem.Current;
					TreeNode parent = null;
					while (true)
					{
						if (process != null && !node.Parent.Text.Contains(process))
							break;
						parent = node.Parent;
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
	}
}

