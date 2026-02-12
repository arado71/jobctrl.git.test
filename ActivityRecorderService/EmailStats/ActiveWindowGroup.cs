using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	public class ActiveWindowGroup
	{
		public string ProcessName { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public int Count { get; set; }
	}
}
