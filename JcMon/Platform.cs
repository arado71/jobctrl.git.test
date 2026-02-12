using JCAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderClient.Hotkeys;

namespace Tct.JcMon
{
	internal static class Platform
	{
		internal static class Factory
		{
			public static IHotkeyService GetHotkeyService()
			{
				return Program.HotkeyService;
			}
		}
	}
}
