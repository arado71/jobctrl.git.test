using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Notification
{
	public class MessageWithActions
	{
		private StringBuilder sb = new StringBuilder();
		private List<Link> links = new List<Link>();

		public IEnumerable<Link> Links { get { return links; } } 

		public MessageWithActions Append(string content)
		{
			sb.Append(content);
			return this;
		}

		public MessageWithActions Prepend(string content)
		{
			sb.Insert(0, content);
			return this;
		}

		public MessageWithActions Append(string content, Action onClick, string description = null)
		{
			var startPos = sb.Length;
			sb.Append(content);
			links.Add(new Link
			{
				StartPosition = startPos,
				EndPosition = sb.Length,
				Action = onClick,
				Description = description,
			});
			return this;
		}

		public MessageWithActions AppendLine(string content = null)
		{
			sb.AppendLine(content);
			return this;
		}

		public string GetText()
		{
			return sb.ToString();
		}

		public class Link
		{
			public int StartPosition { get; set; }
			public int EndPosition { get; set; }
			public Action Action { get; set; }
			public string Description { get; set; }
		}
	}
}
