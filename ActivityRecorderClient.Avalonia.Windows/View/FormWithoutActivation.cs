using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public partial class FormWithoutActivation : Form
	{
		private const int GWL_EXSTYLE = -20;
		private const int WS_EX_Transparent = 0x20;
		private const int WS_EX_TOOLWINDOW = 0x00000080;

		public FormWithoutActivation()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DoubleBuffered = true;
			WinApi.SetWindowLong(Handle, GWL_EXSTYLE, WinApi.GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_Transparent | WS_EX_TOOLWINDOW);
		}

	}
}
