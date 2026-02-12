using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class ToolStripButtonRenderer : ToolStripProfessionalRenderer
	{
		private bool isAlternativeColor;

		public ToolStripButtonRenderer()
			: base(new ToolStripButtonColorTable())
		{
		}

		public void SetAlternativeColors(bool isAlternative)
		{
			isAlternativeColor = isAlternative;
			((ToolStripButtonColorTable)ColorTable).SetAlternativeColors(isAlternative);
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			base.OnRenderMenuItemBackground(e);
			if (e.Item.Selected && e.Item.Enabled && e.Item is ToolStripMenuItemWithButton)
			{
				var item = e.Item as ToolStripMenuItemWithButton;
				if (!item.IsButtonVisible) return;
				ButtonRenderer.DrawButton(e.Graphics,
					item.ButtonRectangle,
					(isAlternativeColor ? item.AltButtonChar : item.ButtonChar).ToString(CultureInfo.InvariantCulture),
					e.Item.Font,
					TextFormatFlags.Internal,
					false,
					PushButtonState.Default);
			}
		}
	}
}
