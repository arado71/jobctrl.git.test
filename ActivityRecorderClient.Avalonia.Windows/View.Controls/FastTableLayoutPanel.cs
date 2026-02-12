using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public sealed class FastTableLayoutPanel : TableLayoutPanel
	{
		public FastTableLayoutPanel()
		{
			DoubleBuffered = true;
		}
	}
}