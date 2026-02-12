using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.MessageNotifier
{
	public interface IMessageService
	{
		DateTime? SetPCReadAt(int messageId);
		void ShowMessages();
	}
}
