using System;
using System.Collections.Generic;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Url
{
	public interface IUrlResolver
	{
		void SetUrls(IEnumerable<DesktopWindow> windowsInfo);
	}
}

