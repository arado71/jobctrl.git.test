using JavaMon.Plugin;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Tct.JcMon.Common;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Hotkeys;
using Tct.Java.Accessibility;
using Configuration = Tct.JcMon.Common.Configuration;

namespace JavaMon
{
	class CapturePresenter
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private JavaAccessibilityForm form;
		private CaptureManager captureManager;

		public CapturePresenter(JavaAccessibilityForm form)
		{
			this.form = form;
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
				Configuration.CaptureFuncs = resFunc;

		}

		public static void UpdateQueryInterval(int queryInterval)
		{
			Configuration.CaptureInterval = queryInterval;
		}

		public void Initialize()
		{
			try
			{
				var test = JabApiController.Instance;
				captureManager = new CaptureManager();
				captureManager.CaptureOccured += CaptureOccured;
				captureManager.Start();
				RegisterHotkey();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Couldn't initialize Java Access Bridge.\n{ex}", "Error");
			}
		}

		private void RegisterHotkey()
		{
			HotkeyWinService.Instance.HotkeyPressed +=
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
					HotkeyWinService.Instance.Register(hotkey);
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
				log.Debug("Registered hotkey " + hotkey, null);
			}
			else
			{
				log.Debug("Unable to reister hotkey", null);
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
			form.TreeView.Invoke(new Action(() =>
			{
				RefreshTree();
			}));
			if (form.TreeView.Nodes.Count > 0)
			{
				foreach (TreeNode node in form.TreeView.Nodes)
				{
					if (node.Tag is AccessibleItem item)
					{
						var resNode = nodeFinder(Cursor.Position, node, item.Size);
						if (resNode != null && resNode != node)
						{
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
		}

		public void RefreshTree()
		{
			form.treeView.Nodes.Clear();
			EnumDelegate delEnumfunc = new EnumDelegate(EnumWindowsProc);
			bool bSuccessful = EnumDesktopWindows(IntPtr.Zero, delEnumfunc, IntPtr.Zero);
		}

		private bool EnumWindowsProc(IntPtr hwnd, int lparam)
		{
			try
			{
				int vmId;
				var javaTree = JabApiController.Instance.GetComponentTree(hwnd, out vmId);

				if (javaTree != null)
				{
					form.treeView.Nodes.Add(GetTree(javaTree));
				}
			}
			catch (Exception) { }

			return true;
		}

		private TreeNode bestNode;

		private TreeNode nodeFinder(Point point, TreeNode node, Size bestSize)
		{
			if (node.Nodes.Count > 0)
				for (int i = 0; i < node.Nodes.Count; i++)
				//foreach (TreeNode childNode in node.Nodes)
				{
					TreeNode childNode = node.Nodes[i];
					if (childNode.Tag is AccessibleItem item)
					{
						if (item.X == -1 || item.Y == -1) continue;
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

		public TreeNode GetTree(AccessibleItem treeItem)
		{
			if (treeItem == null) return new TreeNode("Empty --- Empty");
			TreeNode treeNode;
			if (treeItem.ChildrenCount == 0)
			{
				treeNode = new TreeNode(
					$"[{treeItem.Role}] {treeItem.Name} --- {treeItem.TextValue} [x: {treeItem.X}, y: {treeItem.Y}; w: {treeItem.Width}, h: {treeItem.Height}]");
				treeNode.Tag = treeItem;
				return treeNode;
			}

			var childList = new List<TreeNode>();
			foreach (var child in treeItem.Children)
			{
				childList.Add(GetTree(child));
			}

			treeNode = new TreeNode(
				$"[{treeItem.Role}] {treeItem.Name} --- {treeItem.TextValue} [x: {treeItem.X}, y: {treeItem.Y}; w: {treeItem.Width}, h: {treeItem.Height}]",
				childList.ToArray());
			treeNode.Tag = treeItem;
			return treeNode;
		}

		public void SelectNextMatch(string searchTerm)
		{
			if (form.treeView.Nodes.Count < 1) return;
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

		private delegate bool EnumDelegate(IntPtr hWnd, int lParam);
		[DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
			ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool EnumDesktopWindows(IntPtr hDesktop,
			EnumDelegate lpEnumCallbackFunction, IntPtr lParam);
	}
}
