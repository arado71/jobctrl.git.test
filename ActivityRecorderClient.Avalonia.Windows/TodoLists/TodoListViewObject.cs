using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.TodoLists
{
	public class TodoListViewObject
	{
		public TodoListViewObject(int id, TodoListItemState state, string content, int priority, DateTime? createdAt)
		{
			Id = id;
			State = state;
			Content = content;
			Priority = priority;
			CreatedAt = createdAt;
		}

		public int Id { get; set; }
		public TodoListItemState State { get; set; }

		public string Content { get; set; }
		public int Priority { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
