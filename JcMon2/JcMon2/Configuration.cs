using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JcMon2
{
	public static class Configuration
	{
		static Configuration()
		{
			CaptureInterval = 200;
			CaptureCom = false;
			IncludeScreenshots = true;
			RecordInterval = 30000;
		}

		public static int CaptureInterval { get; set; }

		public static int RecordInterval { get; set; }

		public static bool CaptureCom { get; set; }

		public static bool IncludeScreenshots { get; set; }
	}
}
