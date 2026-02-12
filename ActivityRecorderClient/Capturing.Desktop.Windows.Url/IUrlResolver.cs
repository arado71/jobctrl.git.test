using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url
{
	public interface IUrlResolver
	{
		bool TryGetUrl(IntPtr hWnd, out string url);
	}
}
