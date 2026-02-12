using Accessibility;
using Tct.JcMon.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows;
using Tct.ActivityRecorderClient.Hotkeys;
using Tct.JcMon.IAccessibility.Plugin;
using Configuration = Tct.JcMon.Common.Configuration;


namespace Tct.JcMon.IAccessibility
{
	class IAccessibilityCapturePresenter
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private JCIAccessibilityForm form;
		private CaptureManager captureManager;
		private Hotkey hotkey = null;

		public IAccessibilityCapturePresenter(JCIAccessibilityForm form)
		{
			this.form = form;
			captureManager = new CaptureManager();
			captureManager.CaptureOccured += CaptureOccured;
			captureManager.Start();
			form.FormClosed += Form_FormClosed;
			RegisterHotkey();
		}

		private void Form_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (hotkey != null)
			{
				var hotkeyService = Platform.Factory.GetHotkeyService();
				hotkeyService.Unregister(hotkey);
				hotkeyService.HotkeyPressed -= HotkeyService_HotkeyPressed;
			}
		}

		public static void CompileQuery(string query)
		{
			List<Func<IntPtr, CaptureResult>> resFunc;
			try
			{
				resFunc = PluginController.Decompile(query);
			}
			catch (Exception ex)
			{
				log.Error($"Couldn't compile query {query}.", ex);
				return;
			}

			lock (Configuration.CaptureFuncs)
			{
				Configuration.CaptureFuncs = resFunc;
			}

		}

		public static void UpdateQueryInterval(int queryInterval)
		{
			Configuration.CaptureInterval = queryInterval;
		}

		private void RegisterHotkey()
		{
			Platform.Factory.GetHotkeyService().HotkeyPressed += HotkeyService_HotkeyPressed;
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
					Platform.Factory.GetHotkeyService().Register(hotkey);
				}
				catch (Exception)
				{
					hotkey = null;
				}

				if (hotkey != null)
				{
					form.HotKey = hotkey.ToString();
					break;
				}
			}

			if (hotkey != null)
			{
				log.Debug("Registered hotkey " + hotkey, null);
			}
			else
			{
				log.Debug("Unable to register hotkey", null);
			}
		}

		private void CaptureOccured(IEnumerable<CaptureResult> captures)
		{
			if (form.DataGridView.InvokeRequired)
			{
				form.DataGridView.Invoke(new MethodInvoker(() => CaptureOccured(captures)));
				return;
			}

			form.DataGridView.Rows.Clear();
			foreach (var captureResult in captures)
			{

				form.DataGridView.Rows.Add(captureResult.Name, captureResult.Value, captureResult.ElapsedMilliseconds.ToString("0.000"));
			}
		}

		private void HotkeyService_HotkeyPressed(object sender, SingleValueEventArgs<Hotkey> e)
		{
			if (e.Value != hotkey) return;
			if (Configuration.Hwnd == null || Configuration.Hwnd == IntPtr.Zero)
			{
				form.Invoke(new Action(() => MessageBox.Show("Select a window first!")));
				return;
			}
			IAccessible accessible;
			AccessibilityHelper.AccessibleObjectFromPoint(new WinApi.POINT(Cursor.Position.X, Cursor.Position.Y), out accessible, out object childIDObject);
			form.TreeView.Invoke(new Action(() =>
			{
				form.refreshTree();
			}));
			if (form.TreeView.Nodes.Count > 0)
			{
				foreach (TreeNode node in form.TreeView.Nodes)
				{
					if (node.Tag is AccessibleItem item)
					{

						var resNode = nodeFinder(Cursor.Position, node, item.Size);

						if (resNode == null) resNode = node;
						form.TreeView.Invoke(new Action(() =>
						{
							form.TreeView.SelectedNode = resNode;
							form.TreeView.Focus();
						}));
						return;
					}
				}
			}
		}

		private TreeNode bestNode;

		private TreeNode nodeFinder(Point point, TreeNode node, Size bestSize)
		{
			if (node.Nodes.Count > 0)
				foreach (TreeNode childNode in node.Nodes)
				{
					if (childNode.Tag is AccessibleItem item)
					{
						//if (item.X == -1 || item.Y == -1) continue;
						if (item.Bounds.Contains(point))
						{
							if (item.Width != -1
								&& item.Height != -1
								&& item.Size.Height * item.Size.Width < bestSize.Height * bestSize.Width)
							{
								bestSize = item.Size;
								bestNode = childNode;
							}
						}
						nodeFinder(point, childNode, bestSize);
					}
				}
			return bestNode;
		}

		public TreeNode GetTree(IntPtr hwnd)
		{
			var accItem = AccessibilityHelper.GetIAccessibleFromWindow(hwnd, AccessibilityHelper.ObjId.WINDOW);
			depth = 0;
			return GetTree(new AccessibleItem(accItem, null, 0, 0));
		}

		private int depth = 0;

		public TreeNode GetTree(AccessibleItem treeItem)
		{
			depth++;
			if (treeItem == null || depth > 50) { depth--; return new TreeNode("Empty --- Empty"); }
			TreeNode treeNode;
			if (treeItem.ChildrenCount == 0)
			{
				treeNode = new TreeNode(
					$"[{treeItem.Role}] {treeItem.Name} --- {treeItem.TextValue} [x: {treeItem.X}, y: {treeItem.Y}; w: {treeItem.Width}, h: {treeItem.Height}]");
				treeNode.Tag = treeItem;
				depth--;
				return treeNode;
			}
			var childList = new List<TreeNode>();
			IEnumerable<AccessibleItem> children = treeItem.getChildren();
			if (children != null)
			{
				foreach (var child in children)
				{
					childList.Add(GetTree(child));
				}
			}

			treeNode = new TreeNode(
				$"[{treeItem.Role}] {treeItem.Name} --- {treeItem.TextValue} [x: {treeItem.X}, y: {treeItem.Y}; w: {treeItem.Width}, h: {treeItem.Height}]",
				childList.ToArray());
			treeNode.Tag = treeItem;
			depth--;
			return treeNode;
		}

		public void HandleSelectedHwndChanged(IntPtr hwnd)
		{
			Configuration.Hwnd = hwnd;
		}

		internal void SaveStructure(string fileName)
		{
			form.showProgressMarquee();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					var hwnd = Configuration.Hwnd;
					var accItem = AccessibilityHelper.GetIAccessibleFromWindow(hwnd, AccessibilityHelper.ObjId.WINDOW);
					int level = 0;
					var sb = new StringBuilder();
					saveTree(ref sb, new AccessibleItem(accItem, null, 0, 0), level, 100);
					File.WriteAllText(fileName, sb.ToString());
				}
				finally
				{
					form.Invoke(new Action(() => form.hideProgress()));
				}
			});
		}

		private void saveTree(ref StringBuilder sb, AccessibleItem accItem, int level, int maxLevel)
		{
			if (accItem == null || level > maxLevel)
			{
				sb = sb.Append(getSomeTabs(level)).Append("...").Append(Environment.NewLine);
				return;
			}
			sb = sb.Append(getSomeTabs(level)).Append(getSaveInfo(accItem)).Append(Environment.NewLine);
			if (accItem.ChildrenCount > 0)
			{
				foreach (var child in accItem.getChildren())
				{
					saveTree(ref sb, child, level + 1, maxLevel);
				}
			}
			return;
		}

		private string getSomeTabs(int tabs)
		{
			int n = 0;
			StringBuilder res = new StringBuilder();
			while (n++ < tabs) res.Append("\t");
			return res.ToString();
		}

		private string getSaveInfo(AccessibleItem item)
		{
			return $"[Role: {item.Role}] Name: '{item.Name}' --- TextValue: '{item.TextValue}' [x: {item.X}, y: {item.Y}; w: {item.Width}, h: {item.Height}], Path: {string.Join(",", item.Path)}";
		}

		public void SelectNextMatch(string searchTerm)
		{
			if (form.TreeView.Nodes.Count < 1) return;
			var currentNode = form.TreeView.SelectedNode ?? form.TreeView.Nodes[0];
			if (isMatch((AccessibleItem)currentNode.Tag, searchTerm))
				currentNode = getNextNode(currentNode);
			while (true)
			{
				if (currentNode == null)
				{
					MessageBox.Show("Can't find more matches.\nIf you want to search from the beginning, select the root node.", "Search finished");
					return;
				}
				var match = findMatch(currentNode, searchTerm);
				if (match != null)
				{
					form.TreeView.SelectedNode = match;
					form.TreeView.Focus();
					return;
				}
				currentNode = getNextNode(currentNode);
			}
		}

		private TreeNode getNextNode(TreeNode node)
		{
			if (node.Nodes.Count > 0) return node.Nodes[0];
			if (node.Parent == null) return node.NextNode;
			if (node.NextNode == null)
			{
				var parent = node.Parent;
				while (parent.NextNode == null)
				{
					if (parent.Parent == null) return null;
					parent = parent.Parent;
				}
				return parent.NextNode;
			}
			return node.NextNode;
		}

		private TreeNode findMatch(TreeNode node, string searchTerm)
		{
			var accItem = (AccessibleItem)node.Tag;
			if (isMatch(accItem, searchTerm))
			{
				return node;
			}
			foreach (TreeNode childNode in node.Nodes)
			{
				var match = findMatch(childNode, searchTerm);
				if (match != null) return match;
			}
			return null;
		}

		private bool isMatch(AccessibleItem accItem, string searchTerm)
		{
			var lowSearchTerm = searchTerm.ToLower();
			return accItem.Name != null && accItem.Name.ToLower().Contains(lowSearchTerm) ||
				accItem.Role != null && accItem.Role.ToLower().Contains(lowSearchTerm) ||
				accItem.Description != null && accItem.Description.ToLower().Contains(lowSearchTerm) ||
				accItem.TextValue != null && accItem.TextValue.ToLower().Contains(lowSearchTerm);
		}
	}
}
