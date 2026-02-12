using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public class DropdownCheckElement : DropdownElementBase
	{
		private const int TextPadding = 3;
		private readonly Action onClick;
		private SmartLabel lblText;
		private Panel pChecked;

		public bool Checked
		{
			set { pChecked.Visible = value; }
		}

		public DropdownCheckElement(string value, Action onClick)
		{
			this.onClick = onClick;
			InitializeComponent();
			BackColor = StyleUtils.Background;
			lblText.BackColor = StyleUtils.Background;
			lblText.ForeColor = StyleUtils.Foreground;
			lblText.ForeColorAlternative = StyleUtils.ForegroundLight;
			Value = value;
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			return new Size(lblText.Location.X + lblText.PreferredSize.Width + TextPadding, Height);
		}

		protected override void RenderSelection()
		{
			BackColor = selected ? StyleUtils.BackgroundHighlight : StyleUtils.Background;
			lblText.BackColor = selected ? StyleUtils.BackgroundHighlight : StyleUtils.Background;
			lblText.RenderText();
			BackColor = selected ? StyleUtils.BackgroundHighlight : StyleUtils.Background;
			Invalidate();
		}

		protected override void RenderValue()
		{
			lblText.Clear().AddText(Value).RenderText();
		}

		private void HandleMouseClicked(object sender, MouseEventArgs e)
		{
			Selected = false;
			if (onClick != null) onClick();
			OnClick(EventArgs.Empty);
		}

		private void HandleMouseOver(object sender, EventArgs e)
		{
			Selected = true;
		}

		private void InitializeComponent()
		{
			this.lblText = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			this.pChecked = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// lblText
			// 
			this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblText.AutoWrap = false;
			this.lblText.FontSize = 8F;
			this.lblText.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(179)))), ((int)(((byte)(179)))));
			this.lblText.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Left;
			this.lblText.Location = new System.Drawing.Point(15, 1);
			this.lblText.Name = "lblText";
			this.lblText.Size = new System.Drawing.Size(160, 18);
			this.lblText.TabIndex = 0;
			this.lblText.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Top;
			this.lblText.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseClicked);
			this.lblText.MouseEnter += new System.EventHandler(this.HandleMouseOver);
			// 
			// pChecked
			// 
			this.pChecked.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.selection;
			this.pChecked.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pChecked.Location = new System.Drawing.Point(-1, 0);
			this.pChecked.Name = "pChecked";
			this.pChecked.Size = new System.Drawing.Size(16, 16);
			this.pChecked.TabIndex = 1;
			this.pChecked.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseClicked);
			this.pChecked.MouseEnter += new System.EventHandler(this.HandleMouseOver);
			// 
			// DropdownCheckElement
			// 
			this.Controls.Add(this.pChecked);
			this.Controls.Add(this.lblText);
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Name = "DropdownCheckElement";
			this.Size = new System.Drawing.Size(180, 17);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseClicked);
			this.MouseEnter += new System.EventHandler(this.HandleMouseOver);
			this.ResumeLayout(false);

		}
	}
}