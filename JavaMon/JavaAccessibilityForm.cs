using JavaMon.Plugin;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Tct.Java.Accessibility;
using Tct.JcMon.Common;

// ReSharper disable LocalizableElement

namespace JavaMon
{
	public partial class JavaAccessibilityForm : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly HighlightRectangle highlightRectangle;
		public DataGridView DataGridView => dataGridView;
		public TreeView TreeView => treeView;
		private CapturePresenter presenter;

		public JavaAccessibilityForm()
		{
			highlightRectangle = new HighlightRectangle();
			presenter = new CapturePresenter(this);
			InitializeComponent();
		}

		private void JavaAccessibilityForm_Load(object sender, System.EventArgs e)
		{
			presenter.Initialize();
		}

		private void refreshButton_Click(object sender, EventArgs e)
		{
			presenter.RefreshTree();
		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var treeNode = e.Node;
			if (treeNode?.Tag is AccessibleItem acItem)
			{
				nameTextBox.Text = acItem.Name;
				roleTextBox.Text = acItem.Role;
				descTextBox.Text = acItem.Description;
				textTextBox.Text = acItem.TextValue;
				highlightRectangle.Location = new Rectangle(acItem.X, acItem.Y, acItem.Width, acItem.Height);
				highlightRectangle.Visible = true;
				pluginResComboToolStripMenuItem.Visible = acItem.Role == "combo box";
				pluginResTableToolStripMenuItem.Visible = acItem.Role == "table";
			}
			else
			{
				nameTextBox.Text = "";
				roleTextBox.Text = "";
				descTextBox.Text = "";
				textTextBox.Text = "";
				highlightRectangle.Visible = false;
			}
		}

		private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				treeView.SelectedNode = e.Node;
				contextMenuStrip.Show(Cursor.Position);
			}
		}

		private void pluginResNameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.Name);
		}

		private void pluginResRoleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.Role);
		}

		private void pluginResDescToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.Description);
		}

		private void pluginResTextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.Text);
		}

		private void compileButton_Click(object sender, EventArgs e)
		{
			CapturePresenter.CompileQuery(paramTextBox.Text);
		}

		private void numericUpDown_ValueChanged(object sender, EventArgs e)
		{
			CapturePresenter.UpdateQueryInterval(decimal.ToInt32(numericUpDown.Value));
		}

		private void pluginResComboToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.ComboValue);
		}

		private void tableColsToolStripMenuItems_Click(object sender, EventArgs e)
		{

			if (sender is ToolStripMenuItem menuItem && treeView.SelectedNode?.Tag is AccessibleItem accItem)
				paramTextBox.Text = PluginController.Compile(accItem, PluginValueType.Table, menuItem.Text);
		}

		private void searchButton_Click(object sender, EventArgs e)
		{
			presenter.SelectNextMatch(searchTextBox.Text);
		}

		private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Enter)
			{
				searchButton.PerformClick();
				e.Handled = true;
			}
		}

		private void copyDetailsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!(treeView.SelectedNode?.Tag is AccessibleItem accItem)) return;
			var text = new List<string>
			{
				$"Name:                 {accItem.Name}",
				$"Description:          {accItem.Description}",
				$"RoleEnUs:             {accItem.RoleEnUs}",
				$"StatesEnUs:           {accItem.StatesEnUs}",
				$"TextValue:            {accItem.TextValue}",
				$"AccessibleComponent:  {accItem.AccessibleComponent}",
				$"AccessibleAction:     {accItem.AccessibleAction}{(accItem.Actions?.Count > 0 ? " (" + string.Join(", ", accItem.Actions) + ")" : "")}",
				$"AccessibleSelection:  {accItem.AccessibleSelection}",
				$"AccessibleText:       {accItem.AccessibleText}",
				$"AccessibleInterfaces: {accItem.AccessibleInterfaces}",
				$"Path:                 {PluginController.Compile(accItem, PluginValueType.Name)}",
				""
			};
			Clipboard.SetText(string.Join("\r\n", text));
		}
	}
}
