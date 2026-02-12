using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public class DropdownElement : DropdownElementBase
	{
		private const int TextPadding = 3;
		private readonly Action onClick;
		private SmartLabel lblText;

		public DropdownElement(string value, Action onClick)
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
			this.SuspendLayout();
			// 
			// lblText
			// 
			this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblText.AutoWrap = false;
			this.lblText.FontSize = 8F;
			this.lblText.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(179)))), ((int)(((byte)(179)))));
			this.lblText.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Left;
			this.lblText.Location = new System.Drawing.Point(3, 2);
			this.lblText.Name = "lblText";
			this.lblText.Size = new System.Drawing.Size(174, 18);
			this.lblText.TabIndex = 0;
			this.lblText.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Top;
			this.lblText.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseClicked);
			this.lblText.MouseEnter += new System.EventHandler(this.HandleMouseOver);
			// 
			// DropdownElement
			// 
			this.Controls.Add(this.lblText);
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Name = "DropdownElement";
			this.Size = new System.Drawing.Size(180, 23);
			this.ResumeLayout(false);

		}
	}
}