namespace Reporter.Interfaces
{
	public interface ICalendarMeetingWorkItem : IWorkItem
	{
		string Title { get; }
		string Description { get; }
		string Participants { get; }
        string Location { get; set; }
        string OrganizerEmail { get; set; }
    }
}
