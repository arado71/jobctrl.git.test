using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailActivityTracker.Model
{
	[Flags]
	public enum MeetingPluginTaskIdSettings
	{
		None = 0,
		Description = 1 << 0,
		Subject = 1 << 1
	}
}
