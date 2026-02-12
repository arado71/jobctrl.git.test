using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class ToolStripLabelWithButton : ToolStripControlHost
	{
		public event EventHandler ButtonClick;
		public new event EventHandler<MouseEventArgs> MouseDown;
		public string ButtonText { get { return button.Text; } set { button.Text = value; } }
		public bool IsButtonVisible { get { return button.Visible; } set { button.Visible = value; } }

		private TableLayoutPanel tlPanel;
		private Label label;
		private Button button;
		private ToolTip toolTip;

		public ToolStripLabelWithButton()
			: base(new TableLayoutPanel() { RowCount = 1, ColumnCount = 2 })
		{
			tlPanel = Control as TableLayoutPanel;
			tlPanel.BackColor = Color.Transparent;
			tlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			tlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			label = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = true, Margin = new Padding(0), UseCompatibleTextRendering = true };
			label.MouseDown += new MouseEventHandler(label_MouseDown);
			tlPanel.Controls.Add(label, 0, 0);
			button = new Button { Visible = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, TextAlign = ContentAlignment.MiddleCenter };
			button.Click += new EventHandler(button_Click);
			tlPanel.Controls.Add(button, 1, 0);
			toolTip = new ToolTip();
		}

		void label_MouseDown(object sender, MouseEventArgs e)
		{
			var del = MouseDown;
			if (del != null) del(this, e);
		}

		void button_Click(object sender, EventArgs e)
		{
			var del = ButtonClick;
			if (del != null) del(this, e);
		}

		public ToolStripLabelWithButton(string text)
			: this()
		{
			label.Text = text;
		}

		public override string Text
		{
			get
			{
				return label.Text;
			}
			set
			{
				label.Text = value;
			}
		}

		/*
		public override bool Enabled
		{
			get
			{
				return label.Enabled;
			}
			set
			{
				label.Enabled = value;
			}
		}
		 */

		public new string ToolTipText { get { return toolTip.GetToolTip(label); } set { toolTip.SetToolTip(label, value); } }

		public string ButtonToolTipText { get { return toolTip.GetToolTip(button); } set { toolTip.SetToolTip(button, value); } }
	}
}

