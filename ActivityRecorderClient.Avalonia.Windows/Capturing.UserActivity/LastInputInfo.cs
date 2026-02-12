using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.UserActivity
{
	public class LastInputInfo
	{
		public int GetLastInputTicks()
		{
			var lastInputInfo = new WinApi.LASTINPUTINFO { cbSize = WinApi.LASTINPUTINFO.Size };
			if (WinApi.GetLastInputInfo(ref lastInputInfo))
			{
				return (int)lastInputInfo.dwTime;
			}
			var errCode = Marshal.GetLastWin32Error();
			throw new Win32Exception(errCode);
		}
	}
}
