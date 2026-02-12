using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.Caching.Works
{
	public class WorkOrProject
	{
		public int Id { get; set; }
		public bool IsProject { get; set; }
		public string Name { get; set; }
		public int? ParentId { get; set; }

		public Work ToWork()
		{
			return IsProject ? null : new Work() { Id = Id, Name = Name, ProjectId = ParentId.GetValueOrDefault() };
		}

		public Project ToProject()
		{
			return IsProject ? new Project() { Id = Id, Name = Name, ParentId = ParentId } : null;
		}
	}
}
