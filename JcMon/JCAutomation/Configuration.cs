using System.Collections.Generic;
using JCAutomation.Extraction;

namespace JCAutomation
{
	internal static class Configuration
	{
		private static volatile List<AutomationCapture> processFuncs;
		private static volatile int captureInterval = 200;

		public static int CaptureInterval
		{
			get
			{
				return captureInterval;
			}

			set
			{
				captureInterval = value;
			}
		}

		public static List<AutomationCapture> ProcessFuncs
		{
			get
			{
				return processFuncs;
			}

			set
			{
				processFuncs = value;
			}
		}
		public static int RecordInterval { get; set; }

		public static bool CaptureCom { get; set; }

		public static bool IncludeScreenshots { get; set; }
		static Configuration()
		{
			CaptureInterval = 200;
			CaptureCom = false;
			IncludeScreenshots = true;
			RecordInterval = 30000;
		}
	}
}
