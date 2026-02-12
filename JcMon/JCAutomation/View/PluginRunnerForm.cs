using JCAutomation.View;

namespace JCAutomation
{
    using JCAutomation.Capturing;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Automation;
    using System.Windows.Forms;

    public class PluginRunnerForm : Form
    {
        private DataGridViewTextBoxColumn boundingRectangleDataGridViewTextBoxColumn;
        private BindingSource bsRunningPluginInfo;
        private IContainer components;
        private int count;
        private DataGridView dgvElements;
        private readonly Dictionary<IntPtr, RunningPluginInfo> elementDict;
        private readonly BindingList<RunningPluginInfo> elements;
        private readonly HighlightRectangle highlight;
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nativeHandleDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn processNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn scanTimeDataGridViewTextBoxColumn;
        private StatusStrip statusStrip1;
        private DataGridViewTextBoxColumn textDataGridViewTextBoxColumn;
        private Timer timerUpdate;
        private DataGridViewTextBoxColumn topLevelHandleDataGridViewTextBoxColumn;
        private ToolStripStatusLabel tsslSpring;
        private ToolStripStatusLabel tsslStatus;
        private DataGridViewTextBoxColumn valueDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn visibilityDataGridViewTextBoxColumn;

        public PluginRunnerForm() : this(null)
        {
        }

        public PluginRunnerForm(CustomPlugin plugin)
        {
            this.elements = new BindingList<RunningPluginInfo>();
            this.elementDict = new Dictionary<IntPtr, RunningPluginInfo>();
            HighlightRectangle rectangle = new HighlightRectangle {
                Color = Color.Red
            };
            this.highlight = rectangle;
            this.InitializeComponent();
            if (this.components == null)
            {
                this.components = new Container();
            }
            this.components.Add(new ComponentWrapper(this.highlight));
            this.Plugin = plugin;
            this.dgvElements.DataSource = this.elements;
            this.dgvElements.SelectionChanged += new EventHandler(this.dgvElements_SelectionChanged);
        }

        private void dgvElements_DoubleClick(object sender, EventArgs e)
        {
            int num;
            RunningPluginInfo selectedItem = this.GetSelectedItem(out num);
            if ((selectedItem != null) && this.dgvElements.GetRowDisplayRectangle(num, true).Contains(this.dgvElements.PointToClient(Control.MousePosition)))
            {
                SpyForm form2 = new SpyForm {
                    Plugin = this.Plugin,
                    TargetElement = selectedItem.Element
                };
                form2.Show();
            }
        }

        private void dgvElements_SelectionChanged(object sender, EventArgs e)
        {
            int num;
            RunningPluginInfo selectedItem = this.GetSelectedItem(out num);
            if (selectedItem == null)
            {
                this.highlight.Visible = false;
            }
            else
            {
                this.highlight.Location = selectedItem.BoundingRectangle;
                this.highlight.Visible = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private RunningPluginInfo GetSelectedItem(out int index)
        {
            index = -1;
            RunningPluginInfo item = null;
            if (this.dgvElements.SelectedRows.Count > 0)
            {
                item = this.dgvElements.SelectedRows[0].DataBoundItem as RunningPluginInfo;
            }
            else if (this.dgvElements.SelectedCells.Count > 0)
            {
                item = this.dgvElements.Rows[this.dgvElements.SelectedCells[0].RowIndex].DataBoundItem as RunningPluginInfo;
            }
            if (item == null)
            {
                return null;
            }
            index = this.elements.IndexOf(item);
            return item;
        }

        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.dgvElements = new System.Windows.Forms.DataGridView();
			this.topLevelHandleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.scanTimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.processNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.valueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.textDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.boundingRectangleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.nativeHandleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.visibilityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.bsRunningPluginInfo = new System.Windows.Forms.BindingSource(this.components);
			this.timerUpdate = new System.Windows.Forms.Timer(this.components);
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.tsslSpring = new System.Windows.Forms.ToolStripStatusLabel();
			this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
			((System.ComponentModel.ISupportInitialize)(this.dgvElements)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bsRunningPluginInfo)).BeginInit();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// dgvElements
			// 
			this.dgvElements.AllowUserToAddRows = false;
			this.dgvElements.AllowUserToDeleteRows = false;
			this.dgvElements.AutoGenerateColumns = false;
			this.dgvElements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvElements.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.topLevelHandleDataGridViewTextBoxColumn,
            this.scanTimeDataGridViewTextBoxColumn,
            this.processNameDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn,
            this.valueDataGridViewTextBoxColumn,
            this.textDataGridViewTextBoxColumn,
            this.boundingRectangleDataGridViewTextBoxColumn,
            this.nativeHandleDataGridViewTextBoxColumn,
            this.visibilityDataGridViewTextBoxColumn});
			this.dgvElements.DataSource = this.bsRunningPluginInfo;
			this.dgvElements.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvElements.Location = new System.Drawing.Point(0, 0);
			this.dgvElements.Name = "dgvElements";
			this.dgvElements.ReadOnly = true;
			this.dgvElements.Size = new System.Drawing.Size(997, 326);
			this.dgvElements.TabIndex = 0;
			this.dgvElements.DoubleClick += new System.EventHandler(this.dgvElements_DoubleClick);
			// 
			// topLevelHandleDataGridViewTextBoxColumn
			// 
			this.topLevelHandleDataGridViewTextBoxColumn.DataPropertyName = "TopLevelHandle";
			this.topLevelHandleDataGridViewTextBoxColumn.HeaderText = "TopLevelHandle";
			this.topLevelHandleDataGridViewTextBoxColumn.Name = "topLevelHandleDataGridViewTextBoxColumn";
			this.topLevelHandleDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// scanTimeDataGridViewTextBoxColumn
			// 
			this.scanTimeDataGridViewTextBoxColumn.DataPropertyName = "ScanTime";
			this.scanTimeDataGridViewTextBoxColumn.HeaderText = "ScanTime";
			this.scanTimeDataGridViewTextBoxColumn.Name = "scanTimeDataGridViewTextBoxColumn";
			this.scanTimeDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// processNameDataGridViewTextBoxColumn
			// 
			this.processNameDataGridViewTextBoxColumn.DataPropertyName = "ProcessName";
			this.processNameDataGridViewTextBoxColumn.HeaderText = "ProcessName";
			this.processNameDataGridViewTextBoxColumn.Name = "processNameDataGridViewTextBoxColumn";
			this.processNameDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// nameDataGridViewTextBoxColumn
			// 
			this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
			this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
			this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
			this.nameDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// valueDataGridViewTextBoxColumn
			// 
			this.valueDataGridViewTextBoxColumn.DataPropertyName = "Value";
			this.valueDataGridViewTextBoxColumn.HeaderText = "Value";
			this.valueDataGridViewTextBoxColumn.Name = "valueDataGridViewTextBoxColumn";
			this.valueDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// textDataGridViewTextBoxColumn
			// 
			this.textDataGridViewTextBoxColumn.DataPropertyName = "Text";
			this.textDataGridViewTextBoxColumn.HeaderText = "Text";
			this.textDataGridViewTextBoxColumn.Name = "textDataGridViewTextBoxColumn";
			this.textDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// boundingRectangleDataGridViewTextBoxColumn
			// 
			this.boundingRectangleDataGridViewTextBoxColumn.DataPropertyName = "BoundingRectangle";
			this.boundingRectangleDataGridViewTextBoxColumn.HeaderText = "BoundingRectangle";
			this.boundingRectangleDataGridViewTextBoxColumn.Name = "boundingRectangleDataGridViewTextBoxColumn";
			this.boundingRectangleDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// nativeHandleDataGridViewTextBoxColumn
			// 
			this.nativeHandleDataGridViewTextBoxColumn.DataPropertyName = "NativeHandle";
			this.nativeHandleDataGridViewTextBoxColumn.HeaderText = "NativeHandle";
			this.nativeHandleDataGridViewTextBoxColumn.Name = "nativeHandleDataGridViewTextBoxColumn";
			this.nativeHandleDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// visibilityDataGridViewTextBoxColumn
			// 
			this.visibilityDataGridViewTextBoxColumn.DataPropertyName = "Visibility";
			this.visibilityDataGridViewTextBoxColumn.HeaderText = "Visibility";
			this.visibilityDataGridViewTextBoxColumn.Name = "visibilityDataGridViewTextBoxColumn";
			this.visibilityDataGridViewTextBoxColumn.ReadOnly = true;
			// 
			// bsRunningPluginInfo
			// 
			this.bsRunningPluginInfo.DataSource = typeof(JCAutomation.RunningPluginInfo);
			// 
			// timerUpdate
			// 
			this.timerUpdate.Interval = 1000;
			this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslSpring,
            this.tsslStatus});
			this.statusStrip1.Location = new System.Drawing.Point(0, 304);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(997, 22);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// tsslSpring
			// 
			this.tsslSpring.Name = "tsslSpring";
			this.tsslSpring.Size = new System.Drawing.Size(982, 17);
			this.tsslSpring.Spring = true;
			// 
			// tsslStatus
			// 
			this.tsslStatus.Name = "tsslStatus";
			this.tsslStatus.Size = new System.Drawing.Size(0, 17);
			// 
			// PluginRunnerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(997, 326);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.dgvElements);
			this.Name = "PluginRunnerForm";
			this.Text = "JC Automation Code Runner";
			this.Load += new System.EventHandler(this.PluginRunnerForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.dgvElements)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bsRunningPluginInfo)).EndInit();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        private AutomationElement PluginCaptureNoThrow(DesktopWindow desktopWindow)
        {
            try
            {
                return this.Plugin.Capture(desktopWindow.Handle, desktopWindow.ProcessId, desktopWindow.ProcessName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void PluginRunnerForm_Load(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                this.RescanInfo();
            }
            this.timerUpdate.Enabled = true;
        }

        private void RefreshInfo()
        {
            List<RunningPluginInfo> list = new List<RunningPluginInfo>();
            foreach (RunningPluginInfo info in this.elements)
            {
                if (!this.TryRefreshInfo(info))
                {
                    list.Add(info);
                }
            }
            foreach (RunningPluginInfo info2 in list)
            {
                this.elements.Remove(info2);
                this.elementDict.Remove(info2.TopLevelHandle);
            }
        }

        private void RescanInfo()
        {
            List<DesktopWindow> windowsInfo = EnumWindowsHelper.GetWindowsInfo(false);
            List<RunningPluginInfo> list2 = new List<RunningPluginInfo>();
            foreach (DesktopWindow window in windowsInfo)
            {
                RunningPluginInfo info;
                int tickCount = Environment.TickCount;
                AutomationElement element = this.PluginCaptureNoThrow(window);
                string str = (Environment.TickCount - tickCount) + " ms";
                if (!this.elementDict.TryGetValue(window.Handle, out info))
                {
                    info = null;
                }
                if (element != null)
                {
                    if (info != null)
                    {
                        info.ScanTime = str;
                        if (!this.TryRefreshInfo(info))
                        {
                            list2.Add(info);
                        }
                    }
                    else
                    {
                        RunningPluginInfo item = new RunningPluginInfo(element) {
                            TopLevelHandle = window.Handle,
                            ScanTime = str
                        };
                        this.elements.Add(item);
                        this.elementDict.Add(window.Handle, item);
                    }
                }
                else if (info != null)
                {
                    this.elementDict.Remove(window.Handle);
                    this.elements.Remove(info);
                }
            }
            foreach (RunningPluginInfo info4 in list2)
            {
                this.elements.Remove(info4);
                this.elementDict.Remove(info4.TopLevelHandle);
            }
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                if ((++this.count % 10) == 0)
                {
                    this.tsslStatus.Text = "Rescanning...";
                    this.RescanInfo();
                }
                else
                {
                    this.tsslStatus.Text = "Refreshing...";
                    this.RefreshInfo();
                }
                this.dgvElements_SelectionChanged(null, EventArgs.Empty);
            }
            this.tsslStatus.Text = "Done.";
        }

        private bool TryRefreshInfo(AutomationElementInfo elementInfo)
        {
            try
            {
                elementInfo.RefreshInfo();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public CustomPlugin Plugin { get; set; }
    }
}

