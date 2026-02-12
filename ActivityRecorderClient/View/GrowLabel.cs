using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	//http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/97c18a1d-729e-4a68-8223-0fcc9ab9012b/
	public class GrowLabel : Label
	{
		private bool mGrowing;
		public GrowLabel()
		{
			this.AutoSize = false;
			this.UseMnemonic = false;
		}

		private void ResizeLabel()
		{
			if (mGrowing) return;
			try
			{
				mGrowing = true;
				Size sz = new Size(this.Width, Int32.MaxValue);
				sz = TextRenderer.MeasureText(this.Text, this.Font, sz, TextFormatFlags.WordBreak);
				this.Height = sz.Height;
			}
			finally
			{
				mGrowing = false;
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			ResizeLabel();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			ResizeLabel();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			ResizeLabel();
		}
	}
}
