using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using JavaMon.Plugin;
using JCAutomation;
using log4net;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Hotkeys;
using TcT.Java.Accessibility;
using Configuration = Common.Configuration;

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
				form.refreshTree();
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
	}
}
