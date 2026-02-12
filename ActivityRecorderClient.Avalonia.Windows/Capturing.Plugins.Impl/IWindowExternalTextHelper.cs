using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public interface IWindowExternalTextHelper
	{
		void AddTextToWindow(IntPtr hWnd, string text);

		void AddTextToCurrentWindow(string text);
	
		string GetTextByWindow(IntPtr hWnd);
	}
}
