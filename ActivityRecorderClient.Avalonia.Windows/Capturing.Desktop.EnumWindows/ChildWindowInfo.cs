using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows
{
	public class ChildWindowInfo
	{
		public IntPtr Handle { get; set; }
		public string ClassName { get; set; }
		public string Caption { get; set; }
	}
}
