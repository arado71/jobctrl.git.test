using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reporter.Interfaces;
using Reporter.Model.WorkItems;

namespace Reporter.Communication
{
	public class ManualOrMeetingWorkItem
	{
		public int UserId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set;}
		public int WorkId { get; set; }
		public string Comment { get; set; }
		public int? MeetingId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string ParticipantsEmails { get; set; }

		public IWorkItem GetWorkItem()
		{
			if (MeetingId != null)
			{
				return new AdhocMeetingWorkItem()
				{
					StartDate = StartDate,
					EndDate = EndDate,
					Description = Description,
					Participants = ParticipantsEmails,
					Title = Title,
					UserId = UserId,
					WorkId = WorkId,
				};
			}
			else
			{
				return new ManualWorkItem()
				{
					StartDate = StartDate,
					EndDate = EndDate,
					Description = Description,
					UserId = UserId,
					WorkId = WorkId,
				};
			}
		}
	}
}
