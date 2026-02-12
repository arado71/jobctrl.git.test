using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.TODOs
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class TodoListItemDTO
	{
		public int Id { get; set; }
		public int ListId { get; set; }
		public string Name { get; set; }
		public int Priority { get; set; }
		public TodoListItemStatusDTO Status { get; set; }
		public DateTime? CreatedAt { get; set; }
		
		public TodoListItemDTO() { }

		public TodoListItemDTO(TodoListItem tli)
		{
			Id = tli.Id;
			ListId = tli.ListId;
			Name = tli.Name;
			Priority = tli.Priority;
			CreatedAt = tli.CreatedAt;
			Status = new TodoListItemStatusDTO
			{
				Id = tli.TodoListItemStatus.Id,
				Name = tli.TodoListItemStatus.Name

			};
		}
	}
}
