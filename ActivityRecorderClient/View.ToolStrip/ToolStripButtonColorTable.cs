using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.ToolStrip
{
	public class ToolStripButtonColorTable : ProfessionalColorTable
	{
		private bool isAlternativeColor;
		public void SetAlternativeColors(bool isAlternative)
		{
			isAlternativeColor = isAlternative;
		}

		public override Color MenuItemSelected
		{
			get
			{
				return isAlternativeColor
					? Color.FromArgb(128, Color.ForestGreen)
					: base.MenuItemSelected;
			}
		}

		public override Color MenuItemBorder
		{
			get
			{
				return isAlternativeColor
					? Color.ForestGreen
					: base.MenuItemBorder;
			}
		}

		public override Color ImageMarginGradientBegin
		{
			get
			{
				return isAlternativeColor
					? Color.FromArgb(32, Color.ForestGreen)
					: base.ImageMarginGradientBegin;
			}
		}

		public override Color ImageMarginGradientMiddle
		{
			get
			{
				return isAlternativeColor
					? Color.FromArgb(16, Color.ForestGreen)
					: base.ImageMarginGradientMiddle;
			}
		}

		public override Color ImageMarginGradientEnd
		{
			get
			{
				return isAlternativeColor
					? Color.FromArgb(64, Color.ForestGreen)
					: base.ImageMarginGradientEnd;
			}
		}
	}
}
