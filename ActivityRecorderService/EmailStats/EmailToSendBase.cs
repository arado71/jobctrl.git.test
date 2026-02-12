using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	internal class EmailToSendBase
	{
		public string To { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string BodyHtml { get; set; }
		public List<EmailResource> HtmlResources { get; set; }
	}
}
