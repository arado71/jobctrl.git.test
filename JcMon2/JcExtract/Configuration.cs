using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JcMon2.Extraction;

namespace JcExtract
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
	}
}
