using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.Stats;

namespace Tct.ActivityRecorderService.EmailStats
{
	internal class EmailToSend : EmailToSendBase
	{
		//public string To { get; set; }
		//public string Subject { get; set; }
		//public string Body { get; set; }
		//public string BodyHtml { get; set; }
		public string SortKey { get; set; }
		public Dictionary<DateTime, FullWorkTimeStats> WorkTimes { get; set; }
		public FullWorkTimeStats FullWorkTime { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int UserId { get; set; }
		public EmailBuilder EmailBuilder { get; set; }
		public bool HasCredits { get; set; }
	}
}
