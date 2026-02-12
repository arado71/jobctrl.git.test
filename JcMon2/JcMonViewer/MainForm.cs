using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JcMon2;
using log4net;

namespace JcMonViewer
{
	public partial class MainForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private CaptureInfo currentCapture = null;

		public MainForm()
		{
			InitializeComponent();
		}

		private void HandleLoaded(object sender, EventArgs eventArgs)
		{
			UpdateCaptureInfoList();
		}

		private IEnumerable<CaptureInfo> LoadNoteFiles()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Debug.Assert(path != null);
			var noteFiles = Directory.EnumerateFiles(path, "*.jcc");
			foreach (var noteFile in noteFiles)
			{
				using (var fs = new FileStream(noteFile, FileMode.Open))
				{
					CaptureInfo res = null;
					try
					{
						var bf = new BinaryFormatter();
						res = bf.Deserialize(fs) as CaptureInfo;
					}
					catch (Exception)
					{
					}

					if (res != null)
					{
						yield return res;
					}
					else
					{
						log.WarnFormat("Failed to load file {0}", noteFile);
					}
				}
			}
		}

		private IEnumerable<CaptureInfo> LoadHistories()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Debug.Assert(path != null);
			var historyFiles = Directory.EnumerateFiles(path, "*.jch");
			foreach (var historyFile in historyFiles)
			{
				using (var fs = new FileStream(historyFile, FileMode.Open))
				{
					var bf = new BinaryFormatter();
					List<CaptureInfo> list = null;
					try
					{
						list = bf.Deserialize(fs) as List<CaptureInfo>;
					}
					catch { }
					if (list != null)
					{
						foreach (var controlInfo in list)
						{
							yield return controlInfo;
						}
					}
					else
					{
						log.WarnFormat("Failed to load file {0}", historyFile);
					}
				}
			}
		}

		private void UpdateCaptureInfoList()
		{
			lbCaptures.Items.Clear();
			var captureInfos = LoadHistories().Union(LoadNoteFiles()).ToArray();
			foreach (var captureInfo in captureInfos)
			{
				lbCaptures.Items.Add(captureInfo);
			}
		}

		private void UpdateCaptureInfo(CaptureInfo selection)
		{
			currentCapture = selection;
			if (currentCapture != null)
			{
				UpdateCaptureInfo();
			}
			else
			{
				ClearCaptureInfo();
			}
		}

		private void UpdateCaptureInfo()
		{
			var ctrl = currentCapture.ActiveControl;
			if (ctrl != null)
			{
				ClearControlLabels();
				ClearWindowLabels();
				UpdateControlHierarchy(ctrl);
				lblNote.Text = currentCapture.Notes;
			}
			else
			{
				lblNote.Text = "-";
			}
		}

		private void ClearCaptureInfo()
		{
			ClearControlLabels();
			ClearWindowLabels();
			tvControl.Nodes.Clear();
		}

		private void ClearControlLabels()
		{
			lblName.Text = "-";
			lblClassName.Text = "-";
			lblControlType.Text = "-";
			lblHelpText.Text = "-";
			lblAutomationId.Text = "-";
			lblSelection.Text = 
			lblValue.Text =
			lblText.Text = "-";
		}

		private void UpdateWindow(WindowInfo window)
		{
			UpdateWindowLabels(window);
			pbScreenshot.Image = window.Image;
		}

		private void UpdateControl(ControlInfo ctrl)
		{
			UpdateControlLabels(ctrl);
			UpdateWindowHierarchy(ctrl);
		}

		private void UpdateControlLabels(ControlInfo ctrl)
		{
			Contract.Requires(ctrl != null);

			lblName.Text = ctrl.Name;
			lblClassName.Text = ctrl.ClassName;
			lblControlType.Text = ctrl.ControlType;
			lblHelpText.Text = ctrl.HelpText;
			lblAutomationId.Text = ctrl.AutomationId;
			lblSelection.Text = ctrl.Selection;
			lblText.Text = ctrl.Text;
			lblValue.Text = ctrl.Value;
		}

		private void ClearWindow()
		{
			ClearWindowLabels();
			pbScreenshot.Image = null;
		}

		private void ClearWindowLabels()
		{
			lblTitle.Text = "-";
			lblProcessName.Text = "-";
			lblWinClassName.Text = "-";
		}

		private void UpdateWindowLabels(WindowInfo window)
		{
			Contract.Requires(window != null);

			lblTitle.Text = window.Title;
			lblProcessName.Text = window.ProcessName;
			lblWinClassName.Text = window.ClassName;
		}

		private void UpdateControlHierarchy(ControlInfo ctrl)
		{
			Contract.Requires(ctrl != null);

			tvControl.Nodes.Clear();
			var rootControl = ctrl.GetRoot();
			var rootNode = BuildHierarchy(rootControl);
			tvControl.Nodes.Add(rootNode);
			tvControl.ExpandAll();
			tvControl.SelectedNode = FindNode(rootNode, x => ReferenceEquals(x.Tag, ctrl));
			UpdateControl(ctrl);
		}

		private void UpdateWindowHierarchy(ControlInfo ctrl)
		{
			Contract.Requires(ctrl != null);

			tvWindow.Nodes.Clear();
			if (ctrl.Window != null)
			{
				var rootWindow = ctrl.Window.GetRoot();
				var rootNode = BuildHierarchy(rootWindow);
				tvWindow.Nodes.Add(rootNode);
				tvWindow.ExpandAll();
				tvWindow.SelectedNode = FindNode(rootNode, x => ReferenceEquals(x.Tag, ctrl.Window));
				UpdateWindow(ctrl.Window);
			}
			else
			{
				ClearWindow();
			}
		}

		private TreeNode FindNode(TreeNode currentNode, Func<TreeNode, bool> selector)
		{
			if (selector(currentNode))
			{
				return currentNode;
			}

			foreach (object child in currentNode.Nodes)
			{
				TreeNode result = FindNode((TreeNode)child, selector);
				if (result != null) return result;
			}
			return null;
		}

		private TreeNode BuildHierarchy<T>(T rootNode) where T : IHierarchical<T>
		{
			return new TreeNode(rootNode.ToString(), rootNode.Children != null ? rootNode.Children.Select(BuildHierarchy).ToArray() : new TreeNode[] { }) { Tag = rootNode };
		}

		private void HandleCaptureSelectionChanged(object sender, EventArgs e)
		{
			UpdateCaptureInfo((CaptureInfo)lbCaptures.SelectedItem);
		}

		private void HandleControlSelectionChanged(object sender, TreeViewEventArgs e)
		{
			if (e.Action != TreeViewAction.ByKeyboard && e.Action != TreeViewAction.ByMouse) return;
			if (tvControl.SelectedNode == null) return;
			var ctrl = (ControlInfo)tvControl.SelectedNode.Tag;
			UpdateControl(ctrl);
		}

		private void HandleWindowSelectionChanged(object sender, TreeViewEventArgs e)
		{
			if (e.Action != TreeViewAction.ByKeyboard && e.Action != TreeViewAction.ByMouse) return;
			if (tvWindow.SelectedNode == null) return;
			var win = (WindowInfo)tvWindow.SelectedNode.Tag;
			UpdateWindow(win);
		}
	}
}
