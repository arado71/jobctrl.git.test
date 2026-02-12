using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.TODOs
{
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class TodoListItemStatusDTO
	{
		public byte Id { get; set; }
		public string Name { get; set; }
	}
}
