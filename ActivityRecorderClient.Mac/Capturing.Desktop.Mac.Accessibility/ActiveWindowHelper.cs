using System;
using System.Collections.Generic;
using log4net;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Desktop.Mac.Accessibility
{
	public static class ActiveWindowHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly NSString sNSApplicationProcessIdentifier = new NSString("NSApplicationProcessIdentifier");

		public static bool SetActiveWindow(List<DesktopWindow> windowsInfo)
		{
			var activePid = ((NSNumber)NSWorkspace.SharedWorkspace.ActiveApplication[sNSApplicationProcessIdentifier]).IntValue;
			try
			{
				AXError error;
				using (var axApp = AXObject.CreateFromApplication(activePid))
				using (var axFocused = axApp.GetAttribute(AXAttribute.FocusedWindow, out error)) //cannot get this for MonoDevelop
				{
					if (error != AXError.Success)
					{
						log.Error("Unable to get focused window");
						return false;
					}
					log.Info("focused window title: " + axFocused.GetStringValueForAttribute(AXAttribute.Title, out error));
					using (var axWindows = axApp.GetAttribute(AXAttribute.Windows, out error))
					{
						if (error != AXError.Success)
						{
							log.Error("Unable to get windows");
							return false;
						}


					}

					//todo....
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to create AXObject for pid " + activePid, ex);
			}
			return false;
		}
	}
}

