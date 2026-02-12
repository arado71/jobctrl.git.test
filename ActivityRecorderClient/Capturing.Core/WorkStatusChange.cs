using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class WorkStatusChange
	{
		public int? WorkId { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
