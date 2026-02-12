using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlaybackClient.ActivityRecorderServiceReference
{
	partial class Screen
	{
		public string ScreenShotPath { get; set; }
		public long? ScreenShotOffset { get; set; }
		public int? ScreenShotLength { get; set; }
	}
}
