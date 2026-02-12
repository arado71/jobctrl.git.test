using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderClient.Avalonia.UI.Views
{
	public class BaseWindow : Window
	{
		public bool IsClosed { get; private set; }

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			IsClosed = true;
		}
	}
}
