using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.EmailStats
{
	[Serializable]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class EmailMessage
	{
		public Guid Id { get; private set; }
		public string To { get; set; }
		public string Subject { get; set; }
		public string PlainBody { get; set; }
		public string HtmlBody { get; set; }
		public List<EmailResource> HtmlResources { get; set; }

		public EmailMessage()
		{
			Id = Guid.NewGuid();
		}
	}
}
