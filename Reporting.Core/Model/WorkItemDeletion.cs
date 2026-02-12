using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;

namespace Reporter.Model
{
	public class WorkItemDeletion : IWorkItemDeletion
	{
		public int UserId { get; set; }
		public DeletionTypes Type { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public int ManualWorkItemTypeId
		{
			get
			{
				switch (Type)
				{
					case DeletionTypes.All:
						return 1;
					case DeletionTypes.Computer:
						return 3;
					case DeletionTypes.Ivr:
						return 2;
					case DeletionTypes.Mobile:
						return 6;
				}

				throw new NotImplementedException();
			}

			set
			{
				switch (value)
				{
					case 1:
						Type = DeletionTypes.All;
						break;
					case 2:
						Type = DeletionTypes.Ivr;
						break;
					case 3:
						Type = DeletionTypes.Computer;
						break;
					case 6:
						Type = DeletionTypes.Mobile;
						break;
					default:
						throw new NotImplementedException();
				}
			}
		}
	}
}
