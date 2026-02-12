using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderService.Stats
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum UserStatus
	{
		None,
		Online,
		OnlineIdle,
		OnlineNotWorking,
		Offline
	}
}
