namespace Reporter.Interfaces
{
	public interface IAdhocMeetingWorkItem : IWorkItem
	{
		string Title { get; }
		string Description { get; }
		string Participants { get; }
	}
}
