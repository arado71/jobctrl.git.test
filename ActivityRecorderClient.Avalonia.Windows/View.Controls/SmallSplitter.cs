using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public partial class SmallSplitter : UserControl
	{
		private bool accent;

		public bool Accent
		{
			get { return accent; }

			set
			{
				accent = value;
				panel1.BackColor = accent ? StyleUtils.JcColor : StyleUtils.ForegroundLight;
				Invalidate();
			}
		}

		public SmallSplitter()
		{
			InitializeComponent();
			panel1.BackColor = StyleUtils.ForegroundLight;
		}
	}
}