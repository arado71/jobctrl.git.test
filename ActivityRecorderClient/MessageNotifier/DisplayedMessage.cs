using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.MessageNotifier
{
	using Message = Tct.ActivityRecorderClient.ActivityRecorderServiceReference.Message;
	public class DisplayedMessage
	{
		public int Id { get; set; }
		public string Content { get; set; }
		public string ContentWithoutFormatting { get; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ReadDate { get; set; }
		public bool IsRead
		{
			get { return ReadDate.HasValue; }
		}

		public DisplayedMessage(Message message)
		{
			Id = message.Id;
			Content = message.Content;
			CreatedAt = message.CreatedAt.FromUtcToLocal();
			ReadDate = message.PCLastReadAt.HasValue ? (DateTime?)message.PCLastReadAt.Value.FromUtcToLocal() :
				(message.MobileLastReadAt.HasValue ? (DateTime?)message.MobileLastReadAt.Value.FromUtcToLocal() : null);
			ContentWithoutFormatting = message.ContentWithoutFormatting;
		}
	}
}
