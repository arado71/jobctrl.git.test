using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.TODOs
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class TodoListDTO
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }
		public List<TodoListItemDTO> TodoListItems { get; set; }
		public int UserId { get; set; }
		public DateTime? LockLastTakenAt { get; set; }
		public DateTime? CreatedAt { get; set; }

		public TodoListDTO() { }

		public TodoListDTO(TodoList tl)
		{
			Id = tl.Id;
			Date = tl.Day;
			UserId = tl.UserId;
			LockLastTakenAt = tl.LockLastTakenAt;
			CreatedAt = tl.CreatedAt;
			TodoListItems = tl.TodoListItems.OrderBy(x => x.Priority).Select(x => new TodoListItemDTO(x)).ToList();
		}
	}
}
