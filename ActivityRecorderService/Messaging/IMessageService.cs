using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.WebsiteServiceReference;

namespace Tct.ActivityRecorderService.Messaging
{
	using Message = WebsiteServiceReference.Message;
	interface IMessageService
	{
		List<Message> GetMessages(int userId, DateTime? lastMessageLastChangeDate, int computerId);

		DateTime MarkMassageAsRead(int userId, int messageId, int computerId);
	}
}
