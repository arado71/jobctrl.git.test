using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	internal class EmailToSendProject : EmailToSendBase
	{
		public ProjectCost Cost { get; set; }
		public string Name { get; set; }
		public int ProjectId { get; set; }
		public EmailBuilder EmailBuilder { get; set; }
	}
}
